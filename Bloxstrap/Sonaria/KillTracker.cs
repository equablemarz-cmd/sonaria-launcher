using System.Runtime.InteropServices;

namespace Bloxstrap.Sonaria
{
    /// <summary>
    /// Counts your kills in Creatures of Sonaria for the Kosers of Sonaria kill leaderboard.
    ///
    /// While you're in CoS and Roblox is the window in front, it copies the top-middle strip of the
    /// game off the screen four times a second (a plain copy of the pixels, the same way the Damage
    /// Counter looks) and checks it for the "KILL CONFIRMED" banner (see <see cref="KillDetector"/>).
    /// Each time the banner appears counts as one kill, which is sent to the Sonaria Software kill
    /// service with your Roblox user ID. Nothing else about you or your PC is sent, the pictures never
    /// leave your PC, and nothing here reads or changes Roblox's memory or sends keys or clicks.
    /// </summary>
    public class KillTracker : IDisposable
    {
        /// <summary>
        /// The Sonaria Software kill service (a Cloudflare Worker)
        /// </summary>
        public const string ServiceUrl = "https://sonaria-kills.equablemarz.workers.dev";

        private const int LooksPerSecond = 4;

        // a banner needs this score in two looks in a row to count
        private const double Threshold = 0.45;

        private const int SightingsNeeded = 2;

        // the banner stays up for about 3 seconds; it has to be gone this long before another can count
        private static readonly TimeSpan GoneAfter = TimeSpan.FromSeconds(1);

        // and two kills can't be counted closer together than this
        private static readonly TimeSpan MinimumGap = TimeSpan.FromSeconds(2);

        private const int MaxPending = 50;

        private readonly long _userId;

        private readonly long _placeId;

        private readonly CancellationTokenSource _cts = new();

        private readonly object _sendLock = new();

        private readonly Queue<long> _pending = new();

        private bool _sending = false;

        private static string PendingLocation => Path.Combine(Paths.Base, "PendingKills.json");

        public KillTracker(long userId, long placeId)
        {
            _userId = userId;
            _placeId = placeId;

            LoadPending();

            var thread = new Thread(Run)
            {
                IsBackground = true,
                Name = "KillTracker",
                Priority = ThreadPriority.BelowNormal
            };

            thread.Start();

            SendPending();
        }

        private void Run()
        {
            const string LOG_IDENT = "KillTracker::Run";

            App.Logger.WriteLine(LOG_IDENT, $"Watching for kills (user {_userId})");

            // work in real screen pixels, so the game area lines up with the screen picture on any display scaling
            try
            {
                SetThreadDpiAwarenessContext(new IntPtr(-4)); // per-monitor aware v2
            }
            catch
            {
                // older Windows, the process's own DPI setting is used
            }

            var grabber = new ScreenGrabber();
            var interval = TimeSpan.FromMilliseconds(1000 / LooksPerSecond);

            int streak = 0;
            bool showing = false;
            DateTime lastSeen = DateTime.MinValue;
            DateTime lastKill = DateTime.MinValue;
            int errors = 0;

            try
            {
                while (!_cts.IsCancellationRequested)
                {
                    var started = DateTime.UtcNow;

                    try
                    {
                        double score = Look(grabber);
                        var now = DateTime.UtcNow;

                        if (score >= Threshold)
                        {
                            streak++;
                            lastSeen = now;

                            if (!showing && streak >= SightingsNeeded && now - lastKill >= MinimumGap)
                            {
                                showing = true;
                                lastKill = now;

                                App.Logger.WriteLine(LOG_IDENT, $"Kill confirmed (score {score:0.00})");
                                AddKill(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                            }
                        }
                        else
                        {
                            streak = 0;

                            if (showing && now - lastSeen >= GoneAfter)
                                showing = false;
                        }

                        errors = 0;
                    }
                    catch (Exception ex)
                    {
                        // log the first few, then keep quiet so a broken display can't fill the log
                        if (++errors <= 3)
                            App.Logger.WriteException(LOG_IDENT, ex);
                    }

                    var wait = interval - (DateTime.UtcNow - started);

                    if (wait > TimeSpan.Zero)
                        _cts.Token.WaitHandle.WaitOne(wait);
                }
            }
            finally
            {
                grabber.Dispose();
            }
        }

        /// <summary>
        /// Takes one look at the game. 0 when Roblox isn't the window in front (there's nothing of it to see)
        /// </summary>
        private static double Look(ScreenGrabber grabber)
        {
            IntPtr hwnd = GetForegroundWindow();

            if (hwnd == IntPtr.Zero || IsIconic(hwnd) || !IsRobloxWindow(hwnd))
                return 0;

            if (!GetClientRect(hwnd, out RECT rect))
                return 0;

            int width = rect.Right - rect.Left, height = rect.Bottom - rect.Top;

            if (width < 200 || height < 200 || width > 16000 || height > 16000)
                return 0;

            var origin = new POINT();

            if (!ClientToScreen(hwnd, ref origin))
                return 0;

            // the banner sits in the top middle: look at the top 35% of the middle half only
            int stripLeft = origin.X + width / 4;
            int stripWidth = width - 2 * (width / 4);
            int stripHeight = height * 35 / 100;

            byte[]? pixels = grabber.Grab(stripLeft, origin.Y, stripWidth, stripHeight);

            if (pixels is null)
                return 0;

            return KillDetector.Score(pixels, stripWidth, stripHeight, stripWidth * 4);
        }

        private static bool IsRobloxWindow(IntPtr hwnd)
        {
            var title = new StringBuilder(64);
            GetWindowText(hwnd, title, title.Capacity);

            return title.ToString() == "Roblox";
        }

        private void AddKill(long unixMs)
        {
            lock (_sendLock)
            {
                _pending.Enqueue(unixMs);

                while (_pending.Count > MaxPending)
                    _pending.Dequeue();

                SavePending();
            }

            SendPending();
        }

        /// <summary>
        /// Sends any kills that haven't been sent yet, one at a time, oldest first. Kills that can't be sent
        /// (no internet, the service is down) stay saved on this PC and are sent next time.
        /// </summary>
        private void SendPending()
        {
            lock (_sendLock)
            {
                if (_sending || _pending.Count == 0)
                    return;

                _sending = true;
            }

            Task.Run(async () =>
            {
                const string LOG_IDENT = "KillTracker::SendPending";

                try
                {
                    int failures = 0;

                    while (!_cts.IsCancellationRequested || failures == 0)
                    {
                        long kill;

                        lock (_sendLock)
                        {
                            if (_pending.Count == 0)
                                break;

                            kill = _pending.Peek();
                        }

                        if (await Send(kill))
                        {
                            lock (_sendLock)
                            {
                                if (_pending.Count > 0 && _pending.Peek() == kill)
                                    _pending.Dequeue();

                                SavePending();
                            }

                            failures = 0;
                            continue;
                        }

                        if (++failures >= 3 || _cts.IsCancellationRequested)
                        {
                            App.Logger.WriteLine(LOG_IDENT, "Could not reach the kill service, will try again later");
                            break;
                        }

                        await Task.Delay(TimeSpan.FromSeconds(5 * failures));
                    }
                }
                catch (Exception ex)
                {
                    App.Logger.WriteException(LOG_IDENT, ex);
                }
                finally
                {
                    lock (_sendLock)
                        _sending = false;
                }
            });
        }

        private async Task<bool> Send(long unixMs)
        {
            const string LOG_IDENT = "KillTracker::Send";

            try
            {
                string body = JsonSerializer.Serialize(new
                {
                    userId = _userId,
                    placeId = _placeId,
                    at = unixMs,
                    version = App.Version
                });

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                using var content = new StringContent(body, Encoding.UTF8, "application/json");
                using var response = await App.HttpClient.PostAsync($"{ServiceUrl}/kill", content, cts.Token);

                // a kill the service turns down (too old, a duplicate) is dropped rather than retried forever
                if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                {
                    App.Logger.WriteLine(LOG_IDENT, $"The kill service turned a kill down ({(int)response.StatusCode})");
                    return true;
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine(LOG_IDENT, $"Failed to send a kill ({ex.GetType().Name})");
                return false;
            }
        }

        private class PendingFile
        {
            [JsonPropertyName("userId")]
            public long UserId { get; set; }

            [JsonPropertyName("placeId")]
            public long PlaceId { get; set; }

            [JsonPropertyName("kills")]
            public List<long> Kills { get; set; } = new();
        }

        private void LoadPending()
        {
            try
            {
                if (String.IsNullOrEmpty(Paths.Base) || !File.Exists(PendingLocation))
                    return;

                var file = JsonSerializer.Deserialize<PendingFile>(File.ReadAllText(PendingLocation));

                // kills saved by a different Roblox account stay theirs: they're only sent while that account plays
                if (file is null || file.UserId != _userId)
                    return;

                lock (_sendLock)
                {
                    foreach (long kill in file.Kills.TakeLast(MaxPending))
                        _pending.Enqueue(kill);
                }
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("KillTracker::LoadPending", ex);
            }
        }

        // called with _sendLock held
        private void SavePending()
        {
            try
            {
                if (String.IsNullOrEmpty(Paths.Base) || !Directory.Exists(Paths.Base))
                    return;

                if (_pending.Count == 0)
                {
                    if (File.Exists(PendingLocation))
                        File.Delete(PendingLocation);

                    return;
                }

                string json = JsonSerializer.Serialize(new PendingFile { UserId = _userId, PlaceId = _placeId, Kills = _pending.ToList() });
                string temp = PendingLocation + ".tmp";

                File.WriteAllText(temp, json);
                File.Move(temp, PendingLocation, true);
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("KillTracker::SavePending", ex);
            }
        }

        public void Dispose()
        {
            _cts.Cancel();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Copies part of the screen into memory: one screen handle and one picture buffer, reused between
        /// looks and only remade when the size changes. A plain copy (SRCCOPY, no CAPTUREBLT), so Windows
        /// never has to redraw any other window for it.
        /// </summary>
        private sealed class ScreenGrabber : IDisposable
        {
            private IntPtr _screen, _mem, _bitmap, _old, _bits;

            private int _width, _height;

            private byte[]? _buffer;

            public byte[]? Grab(int left, int top, int width, int height)
            {
                if (!Ready(width, height))
                    return null;

                if (!BitBlt(_mem, 0, 0, width, height, _screen, left, top, SRCCOPY))
                {
                    // start afresh next look (after a display change, for example)
                    Close();
                    return null;
                }

                GdiFlush();

                Marshal.Copy(_bits, _buffer!, 0, width * height * 4);

                return _buffer;
            }

            private bool Ready(int width, int height)
            {
                if (_screen == IntPtr.Zero)
                {
                    _screen = GetDC(IntPtr.Zero);

                    if (_screen == IntPtr.Zero)
                        return false;

                    _mem = CreateCompatibleDC(_screen);

                    if (_mem == IntPtr.Zero)
                    {
                        Close();
                        return false;
                    }
                }

                if (_bitmap != IntPtr.Zero && _width == width && _height == height)
                    return true;

                DropBitmap();

                var info = new BITMAPINFOHEADER
                {
                    biSize = (uint)Marshal.SizeOf<BITMAPINFOHEADER>(),
                    biWidth = width,
                    biHeight = -height, // top row first
                    biPlanes = 1,
                    biBitCount = 32
                };

                _bitmap = CreateDIBSection(_screen, ref info, 0, out _bits, IntPtr.Zero, 0);

                if (_bitmap == IntPtr.Zero || _bits == IntPtr.Zero)
                {
                    DropBitmap();
                    return false;
                }

                _old = SelectObject(_mem, _bitmap);
                _width = width;
                _height = height;
                _buffer = new byte[width * height * 4];

                return true;
            }

            private void DropBitmap()
            {
                if (_bitmap != IntPtr.Zero)
                {
                    if (_mem != IntPtr.Zero && _old != IntPtr.Zero)
                        SelectObject(_mem, _old);

                    DeleteObject(_bitmap);
                }

                _bitmap = _old = _bits = IntPtr.Zero;
                _width = _height = 0;
                _buffer = null;
            }

            private void Close()
            {
                DropBitmap();

                if (_mem != IntPtr.Zero)
                    DeleteDC(_mem);

                if (_screen != IntPtr.Zero)
                    ReleaseDC(IntPtr.Zero, _screen);

                _mem = _screen = IntPtr.Zero;
            }

            public void Dispose() => Close();
        }

        private const uint SRCCOPY = 0x00CC0020;

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left, Top, Right, Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X, Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct BITMAPINFOHEADER
        {
            public uint biSize;
            public int biWidth;
            public int biHeight;
            public ushort biPlanes;
            public ushort biBitCount;
            public uint biCompression;
            public uint biSizeImage;
            public int biXPelsPerMeter;
            public int biYPelsPerMeter;
            public uint biClrUsed;
            public uint biClrImportant;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ClientToScreen(IntPtr hWnd, ref POINT lpPoint);

        [DllImport("user32.dll")]
        private static extern IntPtr SetThreadDpiAwarenessContext(IntPtr dpiContext);

        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateDIBSection(IntPtr hdc, ref BITMAPINFOHEADER pbmi, uint usage, out IntPtr ppvBits, IntPtr hSection, uint offset);

        [DllImport("gdi32.dll")]
        private static extern IntPtr SelectObject(IntPtr hdc, IntPtr h);

        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool BitBlt(IntPtr hdc, int x, int y, int cx, int cy, IntPtr hdcSrc, int x1, int y1, uint rop);

        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GdiFlush();

        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeleteObject(IntPtr ho);

        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeleteDC(IntPtr hdc);
    }
}
