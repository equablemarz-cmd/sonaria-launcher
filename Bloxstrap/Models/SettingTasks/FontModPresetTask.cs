using Bloxstrap.Models.SettingTasks.Base;

namespace Bloxstrap.Models.SettingTasks
{
    public class FontModPresetTask : StringBaseTask
    {
        public string? GetFileHash()
        {
            if (!File.Exists(Paths.CustomFont))
                return null;

            using var fileStream = File.OpenRead(Paths.CustomFont);
            return MD5Hash.Stringify(App.MD5Provider.ComputeHash(fileStream));
        }

        public FontModPresetTask() : base("ModPreset", "TextFont")
        {
            if (File.Exists(Paths.CustomFont))
                OriginalState = Paths.CustomFont;
        }

        private static void DeleteBoldFont()
        {
            if (!File.Exists(Paths.CustomFontBold))
                return;

            Filesystem.AssertReadOnly(Paths.CustomFontBold);
            File.Delete(Paths.CustomFontBold);
        }

        public override void Execute()
        {
            if (NewState == Sonaria.CosFont.Marker)
            {
                // the Creatures of Sonaria font comes as two files, one for regular text and one for bold
                Sonaria.CosFont.Install();
            }
            else if (!String.IsNullOrEmpty(NewState))
            {
                if (String.Compare(NewState, Paths.CustomFont, StringComparison.InvariantCultureIgnoreCase) != 0 && File.Exists(NewState))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(Paths.CustomFont)!);

                    Filesystem.AssertReadOnly(Paths.CustomFont);
                    File.Copy(NewState, Paths.CustomFont, true);

                    // a font picked by hand is one file, used for every weight
                    DeleteBoldFont();
                }
            }
            else
            {
                if (File.Exists(Paths.CustomFont))
                {
                    Filesystem.AssertReadOnly(Paths.CustomFont);
                    File.Delete(Paths.CustomFont);
                }

                DeleteBoldFont();
            }

            OriginalState = NewState;
        }
    }
}
