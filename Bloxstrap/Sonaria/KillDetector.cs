namespace Bloxstrap.Sonaria
{
    /// <summary>
    /// Spots Creatures of Sonaria's "KILL CONFIRMED" banner in a picture of the top of the game.
    ///
    /// The banner's words are one flat gold colour (#FFBF56), so the picture is reduced to "gold or not",
    /// the gold letters are joined up into lines of text, and any line that is roughly centred and shaped
    /// like the banner is compared with a picture of the real banner's words. The result is a score from
    /// 0 (nothing like it) to 1 (exactly it). Tested on a 9 minute fight video: real banners scored
    /// 0.51 to 0.89 and nothing else went above 0.13.
    ///
    /// It only looks at pictures. Nothing here touches Roblox.
    /// </summary>
    public static class KillDetector
    {
        // the gold of the banner's words
        private const int GoldR = 255, GoldG = 191, GoldB = 86;

        // how far each colour may be from that gold (allows for video compression and blending)
        private const int Tolerance = 40;

        private const int TemplateWidth = 200, TemplateHeight = 20;

        /// <summary>
        /// The banner's words as gold/not gold, 200 x 20, taken from a real screenshot (0-255 per cell)
        /// </summary>
        private static readonly double[] Template = LoadTemplate();

        private const string TemplateData = "/////4AAAAAAAAD/////AACA////gAAAAID///+AAAAAAAAAAAAAgP///4AAAAAAAAAAAAAAAAAAAAAAAAAAAACA/////wAAAAAAAAAAAAAA/////wAAAAAAAAAAAP//////AAAAAACA////AAAAAP////////////+AAAD/////AAAAAP///////////wAAAAAAAAAAgP//////AAAAAACA//////8AAAAAgP////////////+AAACA//////8AAAAAAAAAAAD/////gAAAAAAAv////4AAAID///+AAAAAgP///4AAAAAAAAAAAACA////gAAAAAAAAAAAAAAAAAAAAAAAAACA/////////79AAAAAAABAv////////79AAAAAAAAA//////+AAAAAAID///8AAAAA/////////////4AAAP////8AAAAA//////////////+AAAAAAACA//////8AAAAAAID//////wAAAACA/////////////4AAAID//////////7+AAAAAAP////+AAAAAAED///+/AAAAgP///4AAAACA////gAAAAAAAAAAAAID///+AAAAAAAAAAAAAAAAAAAAAAABAv/////////////8AAAAAv/////////////9AAAAAAAD//////78AAAAAgP///wAAAAD/////////////gAAA/////wAAAAD///////////////+/AAAAAID//////wAAAAAA////////AAAAAID/////////////QAAAgP////////////+/QAAA/////4AAAAAA/////0AAAACA////gAAAAID///+AAAAAAAAAAAAAgP///4AAAAAAAAAAAAAAAAAAAAAAAL//////gICAgP//QAAAAID/////gICAv/////9AAAAAAP///////0AAAACA////AAAAAP////+AgICAgIBAAAD/////AAAAAP////+/gICAv/////9AAAAAgP//////gAAAAAD///////8AAAAAgP///7+AgICAgIAAAACA////v4CAgP//////QAD/////gAAAAL////+AAAAAAID///+AAAAAgP///4AAAAAAAAAAAACA////gAAAAAAAAAAAAAAAAAAAAACA////vwAAAAAAAIAAAABA/////0AAAAAAgP////8AAAAA////////gAAAAID///8AAAAA/////wAAAAAAAAAAAP////8AAAAA/////4AAAAAAv////78AAAD///+///+AAAAAQP//v////wAAAACA////gAAAAAAAAAAAAID///+AAAAAAL//////AP////+AAABA////vwAAAAAAgP///4AAAACA////gAAAAAAAAAAAAID///+AAAAAAAAAAAAAAAAAAAAAAP////9AAAAAAAAAAAAAAID///+AAAAAAAAAv////4AAAAD///+A////AAAAgP///wAAAAD/////AAAAAAAAAAAA/////wAAAAD/////gAAAAACA/////wAAAP///4D//78AAACA//8A////AAAAAID///+AAAAAAAAAAAAAgP///4AAAAAAAP////+A/////4AAAP////8AAAAAAACA////gAAAAID///+AAAAAAAAAAAAAgP///4AAAAAAAAAAAAAAAAAAAACA/////wAAAAAAAAAAAAAA/////0AAAAAAAACA////gAAAAP///wD///9AAACA////AAAAAP////8AAAAAAAAAAAD/////AAAAAP////+AAAAAAID/////AAAA////gP///wAAAID//wD///8AAAAAgP///4AAAAAAAAAAAACA////gAAAAAAAgP///4D/////gAC/////QAAAAAAAAID///+AAAAAgP///4AAAAAAAAAAAACA////gAAAAAAAAAAAAAAAAAAAAID///+AAAAAAAAAAAAAAAD/////AAAAAAAAAID/////AAAA////QID//78AAID///8AAAAA/////wAAAAAAAAAAAP////8AAAAA/////4AAAAAAgP///78AAAD///+AgP//QAAA//+/AP///4AAAACA////gAAAAAAAAAAAAID///+AAAAAAACA//////////+AQP///78AAAAAAAAAgP///4AAAACA////gAAAAAAAAAAAAID///+AAAAAAAAAAAAAAAAAAAAA/////4AAAAAAAAAAAAAAgP////8AAAAAAAAAAP////8AAAD///+AQP///wAAgP///wAAAAD/////AAAAAAAAAAAA/////wAAAAD/////gAAAAID/////QAAAAP///4CA//+AAAD//4AA////gAAAAID///+/gICAgIBAAAAAgP///4AAAAAAAAD//////////4C/////gAAAAAAAAACA////gAAAAID///+AAAAAAAAAAAAAgP///4AAAAAAAAAAAAAAAAAAAAD/////gAAAAAAAAAAAAACA/////wAAAAAAAAAA/////wAAAP///4AA////gACA////AAAAAP///////////4AAAAD/////AAAAAP///////////////78AAAAA////gAD//4AAQP//gAD///+AAAAAgP///////////4AAAACA////gAAAAAAAAP//////////gED/////QAAAAAAAAID///+AAAAAgP///4AAAAAAAAAAAACA////gAAAAAAAAAAAAAAAAAAAAP////+AAAAAAAAAAAAAAID/////AAAAAAAAAAD/////AAAA////gACA//+/AID///8AAAAA////////////gAAAAP////8AAAAA//////////////9AAAAAAED///+AAP///wCA//8AAP///4AAAACA////////////gAAAAID///+AAAAAAAAA//////////+AAL////+/AAAAAAAAgP///4AAAACA////gAAAAAAAAAAAAID///+AAAAAAAAAAAAAAAAAAAAA/////4AAAAAAAAAAAAAAgP////8AAAAAAAAAAP////8AAAD///+AAED///9AgP///wAAAAD///////////+AAAAA/////wAAAAD/////////////gAAAAAAAgP///4AAgP//AID//wAA////gAAAAID///+/gICAgIBAAAAAgP///4AAAAAAAED//////////4AAAP////9AAAAAAACA////gAAAAID///+AAAAAAAAAAAAAgP///4AAAAAAAAAAAAAAAAAAAACA////gAAAAAAAAAAAAABA/////wAAAAAAAABA/////wAAAP///4AAAL///4CA////AAAAAP////8AAAAAAAAAAAD/////AAAAAP////+AAAD/////AAAAAACA////gACA//+A//+AAAD///+AAAAAgP///4AAAAAAAAAAAACA////gAAAAAAAgP//////////gAAAgP////8AAAAAAID///+AAAAAgP///4AAAAAAAAAAAACA////gAAAAAAAAAAAAAAAAAAAAID///+/AAAAAAAAAAAAAAD/////AAAAAAAAAID///+/AAAA////gAAAgP///wD///8AAAAA/////wAAAAAAAAAAAP////8AAAAA/////4AAAID///+/AAAAAID///+AAAD//7///4AAAP///78AAACA////gAAAAAAAAAAAAID///+AAAAAAACA////gP////+AAAAAv////78AAAAAgP///4AAAACA////gAAAAAAAAAAAAID///+AAAAAAAAAAAAAAAAAAAAAQP////8AAAAAAAAAAAAAAL////+AAAAAAAAAv////4AAAAD///+AAAAA////QP///wAAAAD/////AAAAAAAAAAAA/////wAAAAD/////gAAAAP////9AAAAAgP///0AAAP//////QAAA/////wAAAID///+AAAAAAAAAAAAAgP///4AAAAAAAP////9A/////4AAAABA/////0AAAACA////gAAAAID///+AAAAAAAAAAAAAgP///4AAAAAAAAAAAAAAAAAAAAAAv////78AAAAAAABAAAAAQP////8AAAAAAED/////AAAAAP///4AAAAC/////////AAAAAP////8AAAAAAAAAAAD/////AAAAAP////+AAAAAgP///78AAACA////AAAAv/////8AAAD/////AAAAgP///4AAAAAAAAAAAACA////gAAAAAC/////vwD/////gAAAAAC/////vwAAAID///+AAAAAgP////////////+/AACA/////////////78AAAAAAAAAAABA/////7+AAABAgP+/AAAAv////79AAABA/////4AAAAAA////gAAAAED///////8AAAAA/////wAAAAAAAAAAAP////8AAAAA/////4AAAAAA/////4AAAID///8AAACA////vwAAAP////8AAACA////v4CAgICAgEAAAID///+/gICA//////9AAP////+AAAAAAED/////gAAAgP///4AAAACA/////////////4AAAID/////////////gAAAAAAAAAAAAABA//////////////9AAAAAv/////////////+/AAAAAAD///+AAAAAAP///////wAAAAD/////AAAAAAAAAAAA/////wAAAAD/////gAAAAACA/////wAAv////wAAAECAgIBAAAAA/////wAAAID/////////////gAAAgP////////////+/QAAA/////4AAAAAAAID/////QACA////gAAAAID/////////////gAAAgP////////////+AAAAAAAAAAAAAAABA////////////gAAAAAAAv///////////gAAAAAAAAP///4AAAAAAgP//////AAAAAP////8AAAAAAAAAAAD/////AAAAAP////+AAAAAAAC/////vwD/////AAAAAAAAAAAAAAD/////AAAAgP////////////+AAACA//////////+/gAAAAACAgICAQAAAAAAAAICAgIBAAECAgIBAAAAAQICAgICAgICAgIBAAABAgICAgICAgICAgEAAAAAAAAAAAAAAAAAAQICA//+AgAAAAAAAAAAAQICA/7+AgAAAAAAAAAAAgICAQAAAAABAgICAgIAAAAAAgICAgAAAAAAAAAAAAICAgIAAAAAAAAAAAAAAAAAAAAAAAAAAAICAgIAAAAAAAAAAAAAAAICAgIAAAABAgICAgICAgICAgEAAAECAgICAgIAAAAAAAAAAAA==";

        private static double[] LoadTemplate()
        {
            byte[] raw = Convert.FromBase64String(TemplateData);

            var values = new double[raw.Length];

            for (int i = 0; i < raw.Length; i++)
                values[i] = raw[i] / 255.0;

            return values;
        }

        private readonly struct Box
        {
            public readonly int X, Y, W, H;

            public Box(int x, int y, int w, int h)
            {
                X = x; Y = y; W = w; H = h;
            }
        }

        /// <summary>
        /// Scores a picture of the top-middle strip of the game area.
        /// </summary>
        /// <param name="bgra">32-bit pixels, top row first, in blue-green-red-alpha order</param>
        /// <param name="stride">bytes from one row to the next</param>
        /// <returns>0 to 1, how much the best line of gold text looks like the banner</returns>
        public static double Score(byte[] bgra, int width, int height, int stride)
        {
            if (width < 50 || height < 20)
                return 0;

            // 1. gold or not
            var gold = new bool[width * height];
            int goldCount = 0;

            for (int y = 0; y < height; y++)
            {
                int row = y * stride;

                for (int x = 0; x < width; x++)
                {
                    int p = row + x * 4;

                    if (Math.Abs(bgra[p + 2] - GoldR) < Tolerance
                        && Math.Abs(bgra[p + 1] - GoldG) < Tolerance
                        && Math.Abs(bgra[p] - GoldB) < Tolerance)
                    {
                        gold[y * width + x] = true;
                        goldCount++;
                    }
                }
            }

            if (goldCount < 30)
                return 0;

            // 2. separate gold shapes (letters), touching corners counting as joined
            var letters = new List<Box>();
            var seen = new bool[width * height];
            var stack = new Stack<int>();

            for (int start = 0; start < gold.Length; start++)
            {
                if (!gold[start] || seen[start])
                    continue;

                int minX = int.MaxValue, minY = int.MaxValue, maxX = -1, maxY = -1, area = 0;

                seen[start] = true;
                stack.Push(start);

                while (stack.Count > 0)
                {
                    int i = stack.Pop();
                    int px = i % width, py = i / width;

                    area++;
                    if (px < minX) minX = px;
                    if (px > maxX) maxX = px;
                    if (py < minY) minY = py;
                    if (py > maxY) maxY = py;

                    for (int dy = -1; dy <= 1; dy++)
                    {
                        int ny = py + dy;

                        if (ny < 0 || ny >= height)
                            continue;

                        for (int dx = -1; dx <= 1; dx++)
                        {
                            int nx = px + dx;

                            if ((dx == 0 && dy == 0) || nx < 0 || nx >= width)
                                continue;

                            int j = ny * width + nx;

                            if (gold[j] && !seen[j])
                            {
                                seen[j] = true;
                                stack.Push(j);
                            }
                        }
                    }
                }

                int w = maxX - minX + 1, h = maxY - minY + 1;

                // letter-sized only: big gold areas (a sunset sky) and specks are left out
                if (h >= 6 && area >= 15 && h <= height / 4)
                    letters.Add(new Box(minX, minY, w, h));
            }

            // 3. join letters that sit on one line with small gaps into lines of text
            letters.Sort((a, b) => a.X != b.X ? a.X.CompareTo(b.X) : a.Y != b.Y ? a.Y.CompareTo(b.Y) : a.W != b.W ? a.W.CompareTo(b.W) : a.H.CompareTo(b.H));

            var lines = new List<(Box Box, int Count)>();

            foreach (var l in letters)
            {
                bool joined = false;

                for (int g = 0; g < lines.Count; g++)
                {
                    var b = lines[g].Box;

                    if (Math.Abs((2 * l.Y + l.H) - (2 * b.Y + b.H)) < 0.8 * b.H
                        && l.X - (b.X + b.W) < 1.2 * b.H
                        && Math.Abs(l.H - b.H) < 0.5 * b.H)
                    {
                        int nx = Math.Min(b.X, l.X), ny = Math.Min(b.Y, l.Y);
                        int nw = Math.Max(b.X + b.W, l.X + l.W) - nx, nh = Math.Max(b.Y + b.H, l.Y + l.H) - ny;

                        lines[g] = (new Box(nx, ny, nw, nh), lines[g].Count + 1);
                        joined = true;
                        break;
                    }
                }

                if (!joined)
                    lines.Add((l, 1));
            }

            // 4. compare each banner-shaped, roughly centred line with the real banner
            double best = 0;

            foreach (var (b, count) in lines)
            {
                if (count < 6 || b.H < 6)
                    continue;

                if (!(7 * b.H < b.W && b.W < 14 * b.H))
                    continue;

                if (Math.Abs((2 * b.X + b.W) - width) > 0.32 * width)
                    continue;

                double score = Compare(gold, width, b);

                if (score > best)
                    best = score;
            }

            return best;
        }

        /// <summary>
        /// Shrinks the line's gold/not gold picture to the template's size (averaging) and
        /// returns how closely the two match (Pearson correlation, -1 to 1)
        /// </summary>
        private static double Compare(bool[] gold, int width, Box b)
        {
            var cells = new double[TemplateWidth * TemplateHeight];

            for (int ty = 0; ty < TemplateHeight; ty++)
            {
                int y0 = ty * b.H / TemplateHeight;
                int y1 = Math.Max(y0 + 1, (ty + 1) * b.H / TemplateHeight);

                for (int tx = 0; tx < TemplateWidth; tx++)
                {
                    int x0 = tx * b.W / TemplateWidth;
                    int x1 = Math.Max(x0 + 1, (tx + 1) * b.W / TemplateWidth);

                    int hits = 0;

                    for (int y = y0; y < y1; y++)
                    {
                        int row = (b.Y + y) * width + b.X;

                        for (int x = x0; x < x1; x++)
                        {
                            if (gold[row + x])
                                hits++;
                        }
                    }

                    cells[ty * TemplateWidth + tx] = (double)hits / ((y1 - y0) * (x1 - x0));
                }
            }

            double meanA = cells.Average(), meanB = Template.Average();
            double sumAB = 0, sumAA = 0, sumBB = 0;

            for (int i = 0; i < cells.Length; i++)
            {
                double a = cells[i] - meanA, c = Template[i] - meanB;

                sumAB += a * c;
                sumAA += a * a;
                sumBB += c * c;
            }

            double d = Math.Sqrt(sumAA * sumBB);

            return d == 0 ? 0 : sumAB / d;
        }
    }
}
