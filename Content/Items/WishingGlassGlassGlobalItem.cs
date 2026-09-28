using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using ThoriumWishingGlassReplacer.Config;
using ThoriumWishingGlassReplacer.Content.Players;

namespace ThoriumWishingGlassReplacer.Content.Items
{
    /// <summary>
    /// Class that handles modifying the Wishing Glass item from Thorium mod
    /// </summary>
    public class WishingGlassGlobalItem : GlobalItem
    {
        private const string TargetModName = "ThoriumMod";
        private const string TargetItemName = "WishingGlass";

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            // Returns true if the "RevertToOriginalWishingGlass" Config or the wishing glass could not be found.
            if (ModContent.GetInstance<WishingGlassConfig>().RevertToOriginalWishingGlass || !IsTargetItem(item))
                return;

            // Grab local instance cleanly to read proper multiplayer selection values
            var modPlayer = Main.LocalPlayer.GetModPlayer<WishingGlassPlayer>();
            int currentIndex = modPlayer.ForcedSelectionIndex;
            string currentTarget = WishingGlassPlayer.DestinationNames[currentIndex];
            string combinedDestinationText;

            // Setup the selected destination tooltip depending on if the "ShowUpcomingDestination" config is on/off
            if (ModContent.GetInstance<WishingGlassConfig>().ShowUpcomingDestinations)
            {
                int nextIndex1 = (currentIndex + 1) % WishingGlassPlayer.DestinationNames.Length;
                int nextIndex2 = (currentIndex + 2) % WishingGlassPlayer.DestinationNames.Length;

                string nextTarget1 = WishingGlassPlayer.DestinationNames[nextIndex1];
                string nextTarget2 = WishingGlassPlayer.DestinationNames[nextIndex2];

                combinedDestinationText = $"Selected Destination: [c/46c1db:{currentTarget}] <- [c/ffffff:{nextTarget1}] <- [c/ffffff:{nextTarget2}]";
            }
            else
            {
                combinedDestinationText = $"Selected Destination: [c/46c1db:{currentTarget}]";
            }

            bool replacedSelectionLine = false;
            
            // Replaces lines inside the tooltip  
            for (int i = 0; i < tooltips.Count; i++)
            {
                TooltipLine line = tooltips[i];

                if (line.Text.Contains("Right click while holding to bring up a selection of destinations") ||
                    line.Text.Contains("Right click to toggle destinations"))
                {
                    line.Text = "Right click to toggle destination";
                }

                if (line.Text.Contains("Selected Destination:") || line.Text.Contains("Teleport") || line.Text.Contains("Destination"))
                {
                    line.Text = combinedDestinationText;
                    replacedSelectionLine = true;
                }
            }

            if (!replacedSelectionLine)
            {
                tooltips.Add(new TooltipLine(Mod, "WishingGlassQoLTarget", combinedDestinationText));
            }
        }

        // Helper method that returns if the thorium mod, wishing glass is found
        private static bool IsTargetItem(Item item)
        {
            return item.ModItem != null && 
                   item.ModItem.Mod.Name == TargetModName && 
                   item.ModItem.Name == TargetItemName;
        } 
    }
}
