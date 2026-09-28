using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace ThoriumWishingGlassReplacer.Config
{
    /// <summary>
    /// Class that handles configs
    /// </summary>
    public class WishingGlassConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        [Header("$Mods.ThoriumWishingGlassReplacer.Configs.WishingGlassConfig.Headers.OriginalSettings")]
        [DefaultValue(false)]
        public bool RevertToOriginalWishingGlass { get; set; }

        [Header("$Mods.ThoriumWishingGlassReplacer.Configs.WishingGlassConfig.Headers.FeatureSettings")]
        [DefaultValue(true)]
        public bool ShowUpcomingDestinations { get; set; }

        [Header("$Mods.ThoriumWishingGlassReplacer.Configs.WishingGlassConfig.Headers.UnrestrictedSettings")]
        [DefaultValue(false)]
        public bool EnableInfiniteDeathLocationTeleportation { get; set; }

        [DefaultValue(false)]
        public bool DisableChaosStateRestriction { get; set; }

        // Reverts all Unrestricted settings to false if "RevertToOriginalWishingGlass" is on.
        public override void OnChanged()
        {
            if (RevertToOriginalWishingGlass)
            {
                EnableInfiniteDeathLocationTeleportation = false;
                DisableChaosStateRestriction = false;
                ShowUpcomingDestinations = false;
            }
        }
    }
}