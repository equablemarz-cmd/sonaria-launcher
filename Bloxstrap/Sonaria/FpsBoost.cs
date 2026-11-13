namespace Bloxstrap.Sonaria
{
    /// <summary>
    /// One switch for the speed-ups Roblox still allows launchers to set: anti-aliasing off and
    /// textures at their lowest quality. It only sets the launcher's existing engine presets,
    /// so both stay visible and adjustable on the engine settings page.
    /// </summary>
    public static class FpsBoost
    {
        private const string MsaaOff = "1";
        private const string LowestTextures = "0";

        public static bool IsEnabled =>
            App.FastFlags.GetPreset("Rendering.MSAA") == MsaaOff
            && App.FastFlags.GetPreset("Rendering.TextureQuality.OverrideEnabled") == "True"
            && App.FastFlags.GetPreset("Rendering.TextureQuality.Level") == LowestTextures;

        public static void Set(bool enabled)
        {
            if (enabled)
            {
                App.FastFlags.SetPreset("Rendering.MSAA", MsaaOff);
                App.FastFlags.SetPreset("Rendering.TextureQuality.OverrideEnabled", "True");
                App.FastFlags.SetPreset("Rendering.TextureQuality.Level", LowestTextures);
            }
            else
            {
                App.FastFlags.SetPreset("Rendering.MSAA", null);
                App.FastFlags.SetPreset("Rendering.TextureQuality", null);
            }
        }
    }
}
