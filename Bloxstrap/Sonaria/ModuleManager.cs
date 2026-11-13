using System.IO.Compression;
using System.Security.Cryptography;

namespace Bloxstrap.Sonaria
{
    /// <summary>
    /// Downloads, verifies and unpacks Sonaria Software modules into the launcher's own folder.
    /// Nothing here touches Roblox.
    /// </summary>
    public static class ModuleManager
    {
        // modules are only ever fetched from this repository's own releases
        private const string AllowedUrlPrefix = $"https://github.com/{App.ProjectRepository}/releases/download/";

        private const string InstalledMarker = ".installed";

        private static readonly SemaphoreSlim _installLock = new(1, 1);

        public static string ModulesDirectory => Path.Combine(Paths.Base, "Modules");

        public static string GetModuleDirectory(ModuleInfo module) => Path.Combine(ModulesDirectory, module.Id);

        public static string GetInstallDirectory(ModuleInfo module) => Path.Combine(GetModuleDirectory(module), module.Version);

        public static string GetEntryPath(ModuleInfo module) => Path.Combine(GetInstallDirectory(module), module.EntryPoint);

        public static bool IsInstalled(ModuleInfo module)
            => File.Exists(Path.Combine(GetInstallDirectory(module), InstalledMarker)) && File.Exists(GetEntryPath(module));

        /// <summary>
        /// Makes sure the given module is on disk, downloading and verifying it first if needed.
        /// </summary>
        /// <returns>Whether the module is installed and ready to run</returns>
        public static async Task<bool> EnsureInstalled(ModuleInfo module)
        {
            const string LOG_IDENT = "ModuleManager::EnsureInstalled";

            if (!Paths.Initialized)
            {
                App.Logger.WriteLine(LOG_IDENT, "Launcher is not installed, cannot install modules");
                return false;
            }

            await _installLock.WaitAsync();

            try
            {
                if (IsInstalled(module))
                    return true;

                if (!module.DownloadUrl.StartsWith(AllowedUrlPrefix, StringComparison.Ordinal))
                {
                    App.Logger.WriteLine(LOG_IDENT, $"Refusing to download {module.Id} from an unexpected location");
                    return false;
                }

                string moduleDir = GetModuleDirectory(module);
                string installDir = GetInstallDirectory(module);
                string zipPath = Path.Combine(moduleDir, $"{module.Version}.zip.part");
                string stagingDir = Path.Combine(moduleDir, $"{module.Version}.staging");

                Directory.CreateDirectory(moduleDir);

                App.Logger.WriteLine(LOG_IDENT, $"Downloading {module.Id} {module.Version}...");

                using (var response = await App.HttpClient.GetAsync(module.DownloadUrl, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode();

                    await using var fileStream = new FileStream(zipPath, FileMode.Create, FileAccess.Write, FileShare.None);
                    await response.Content.CopyToAsync(fileStream);
                }

                string hash = await ComputeSha256(zipPath);

                if (!String.Equals(hash, module.Sha256, StringComparison.OrdinalIgnoreCase))
                {
                    App.Logger.WriteLine(LOG_IDENT, $"{module.Id} download failed verification (expected {module.Sha256}, got {hash}), discarding it");
                    File.Delete(zipPath);
                    return false;
                }

                App.Logger.WriteLine(LOG_IDENT, $"{module.Id} download verified, extracting...");

                if (Directory.Exists(stagingDir))
                    Directory.Delete(stagingDir, true);

                // ExtractToDirectory refuses entries that would land outside the target folder
                ZipFile.ExtractToDirectory(zipPath, stagingDir);
                File.Delete(zipPath);

                if (Directory.Exists(installDir))
                    Directory.Delete(installDir, true);

                Directory.Move(stagingDir, installDir);

                if (!File.Exists(GetEntryPath(module)))
                {
                    App.Logger.WriteLine(LOG_IDENT, $"{module.Id} is missing its program file after extraction");
                    return false;
                }

                File.WriteAllText(Path.Combine(installDir, InstalledMarker), module.Sha256);

                RemoveOtherVersions(module);

                App.Logger.WriteLine(LOG_IDENT, $"{module.Id} {module.Version} installed");

                return true;
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine(LOG_IDENT, $"Failed to install {module.Id}");
                App.Logger.WriteException(LOG_IDENT, ex);
                return false;
            }
            finally
            {
                _installLock.Release();
            }
        }

        private static async Task<string> ComputeSha256(string path)
        {
            using var sha256 = SHA256.Create();
            await using var stream = File.OpenRead(path);

            byte[] hash = await sha256.ComputeHashAsync(stream);

            return Convert.ToHexString(hash).ToLowerInvariant();
        }

        private static void RemoveOtherVersions(ModuleInfo module)
        {
            const string LOG_IDENT = "ModuleManager::RemoveOtherVersions";

            string current = GetInstallDirectory(module);

            foreach (string dir in Directory.GetDirectories(GetModuleDirectory(module)))
            {
                if (String.Equals(dir, current, StringComparison.OrdinalIgnoreCase))
                    continue;

                try
                {
                    Directory.Delete(dir, true);
                }
                catch (Exception ex)
                {
                    // most likely still running from there, it'll be cleaned up next time
                    App.Logger.WriteLine(LOG_IDENT, $"Could not remove {dir}");
                    App.Logger.WriteException(LOG_IDENT, ex);
                }
            }
        }
    }
}
