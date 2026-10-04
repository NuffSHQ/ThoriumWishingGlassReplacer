using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Microsoft.Xna.Framework;
using System;
using ThoriumWishingGlassReplacer.Config;
using ThoriumWishingGlassReplacer.Content.Keybind;

namespace ThoriumWishingGlassReplacer.Content.Players
{
    /// <summary>
    /// Class that handles the overhaul and replacer function of the Wishing Glass from Thorium Mod
    /// </summary>
    public class WishingGlassPlayer : ModPlayer
    {
        public int ForcedSelectionIndex = 0;
        private bool hasDiedYet = false;
        private bool alternateOceanDirection = false; 
        private bool hasUsedDeathTeleportForCurrentDeath = false;
        private int cachedWishingGlassType = -1;
        private int lastCombatTextIndex = -1;
        public static readonly string[] DestinationNames = 
            { "Spawn", "Home", "Ocean", "Underworld", "Dungeon", "Temple", "Last Death", "Random" };

        // Sets variables on player selection screen
        public override void Initialize()
        {
            ForcedSelectionIndex = 0;
            hasDiedYet = false;
            alternateOceanDirection = false;
            hasUsedDeathTeleportForCurrentDeath = false;
        }

        // Try and cache the Wishing Glass type storing it in both the "type" and "cachedWishingGlassType" variable
        private bool TryGetWishingGlassType(out int type)
        {

            if (cachedWishingGlassType != -1)
            {
                type = cachedWishingGlassType;
                return true;
            }

            if (ModLoader.TryGetMod("ThoriumMod", out Mod thoriumMod) && 
                thoriumMod.TryFind("WishingGlass", out ModItem wishingGlass))
            {
                cachedWishingGlassType = wishingGlass.Type;
                type = cachedWishingGlassType;
                return true;
            }

            type = -1;
            return false;
        }
 
        public override void Kill(double damage, int hitDirection, bool pvp, Terraria.DataStructures.PlayerDeathReason damageSource)
        {
            hasDiedYet = true;
            hasUsedDeathTeleportForCurrentDeath = false;
        }

        // Disables the right-click function of the wishing glass if overhaul functionality is disabled,
        // due to usage conflicts with the original mod right-click functionality when held
        public override void PreUpdate()
        {
            if (ModContent.GetInstance<WishingGlassConfig>().RevertToOriginalWishingGlass)
                return;

            if (TryGetWishingGlassType(out int glassType) && Player.HeldItem?.type == glassType)
            {
                if (Main.mouseRight && !Main.playerInventory)
                {
                    Player.controlUseTile = false;
                }
            }
        }

        // Helper method that when called plays "unlock" sound, display the selected destination, and emits
        // particles around the player
        private void DestinationCycleHelper()
        {
            SoundEngine.PlaySound(SoundID.Unlock); 

            string msg = $"Destination: {DestinationNames[ForcedSelectionIndex]}";

            // Check if the previous text index is active and if so turn it off
            if (lastCombatTextIndex >= 0 && lastCombatTextIndex < Main.combatText.Length && Main.combatText[lastCombatTextIndex].active)
            {
                Main.combatText[lastCombatTextIndex].active = false;
            }

            // Spawn a new text and replace it as the current 
            lastCombatTextIndex = CombatText.NewText(Player.getRect(), new Color(70, 193, 219), msg, dramatic: false);

            int activeDustType = GetDustIDForDestination(ForcedSelectionIndex); 
            
            // Optimization: Cache mathematical allocations outside the loop
            float angleStep = MathHelper.TwoPi / 12f;
            for (int i = 0; i < 12; i++)
            {
                float angle = i * angleStep;
                Vector2 velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 3f; 
                int d = Dust.NewDust(Player.Center, 0, 0, activeDustType, velocity.X, velocity.Y, 100, default, 1.5f);
                Main.dust[d].noGravity = true;
            }
        }

        // Retrieves key presses and runs code if player has the Wishing Glass and if any of the mod's hotkeys are pressed.
        public override void ProcessTriggers(Terraria.GameInput.TriggersSet triggersSet)
        {   
            // If "RevertToOriginalWishingGlass" is true, early exits the method
            if (ModContent.GetInstance<WishingGlassConfig>().RevertToOriginalWishingGlass)
                return;

            // Execute code if player has Wishing Glass and depending on which hotkey is pressed 
            if (TryGetWishingGlassType(out int glassType) && Player.HasItem(glassType))
            {
                if (WishingGlassKeybinds.CycleLeftKey.JustPressed)
                {
                    ForcedSelectionIndex = (ForcedSelectionIndex - 1 + DestinationNames.Length) % DestinationNames.Length;
                    DestinationCycleHelper();
                }

                if (WishingGlassKeybinds.CycleRightKey.JustPressed)
                {
                    ForcedSelectionIndex = (ForcedSelectionIndex + 1) % DestinationNames.Length;
                    DestinationCycleHelper();
                }

                if (WishingGlassKeybinds.QuickUseKey.JustPressed)
                {
                    HandleWarpExecution();
                }
            }
        }

        // Method that if the item hovered over and right-clicked is the Wishing Glass allows the player to,
        // increments destination selection while refreshing and playing the "unlock" sound
        public override bool HoverSlot(Item[] inventory, int context, int slot)
        {
            Item item = inventory[slot];

            if (!ModContent.GetInstance<WishingGlassConfig>().RevertToOriginalWishingGlass &&
                item != null && !item.IsAir && TryGetWishingGlassType(out int glassType) && item.type == glassType)
            {
                if (Main.mouseRight && Main.mouseRightRelease)
                {
                    Main.mouseRightRelease = false;
                    ForcedSelectionIndex = (ForcedSelectionIndex + 1) % DestinationNames.Length;
                    SoundEngine.PlaySound(SoundID.Unlock);
                    item.Refresh();
                    return true;
                }
            }
            return base.HoverSlot(inventory, context, slot);
        }

        // Disables right-clicking mod on hold if the "RevertToOriginalWishingGlass" config is disabled,
        // due to usage conflicts with the original mod right-click functionality when held
        public override bool CanUseItem(Item item)
        {
            if (TryGetWishingGlassType(out int glassType) && item.type == glassType)
            {
                if (!ModContent.GetInstance<WishingGlassConfig>().RevertToOriginalWishingGlass)
                {
                    if (Main.mouseRight && !Main.playerInventory)
                        return false;

                    HandleWarpExecution();
                    
                    return false; 
                }

                return true; 
            }

            return base.CanUseItem(item);
        }

        // Handles flag checks, chaos debuff, sets and play effects, and play sounds before teleporting the player.
        public void HandleWarpExecution()
        {
            // Prevents teleporting to death location if checks failed 
            if (Player.dead ||ForcedSelectionIndex == 6 && ((!ModContent.GetInstance<WishingGlassConfig>().EnableInfiniteDeathLocationTeleportation && hasUsedDeathTeleportForCurrentDeath) 
                || !hasDiedYet))
            {
                return;
            }

            bool appliedChaosFilter = ForcedSelectionIndex == 7 && !ModContent.GetInstance<WishingGlassConfig>().DisableChaosStateRestriction; // Checks if chaos state should apply
            int chaosBuffType = 0;

            // Prevents teleporting if "Chaos Debuff" is active
            if (appliedChaosFilter && ModLoader.GetMod("ThoriumMod").TryFind("ChaosState", out ModBuff chaosBuff))
            {
                chaosBuffType = chaosBuff.Type;
                if (Player.HasBuff(chaosBuffType)) 
                    return;
            }

            int activeDustID = GetDustIDForDestination(ForcedSelectionIndex);
            SoundEngine.PlaySound(SoundID.Item6, Player.position);

            SpawnRingDust(Player.Center, activeDustID);
            ExecuteCustomWarp(ForcedSelectionIndex);
            
            // Applies chaos debuff
            if (appliedChaosFilter && chaosBuffType != 0)
            {
                Player.AddBuff(chaosBuffType, 600);
            }

            SpawnRingDust(Player.Center, activeDustID);
        }

        // Method to retrieve the dust effect ID used for teleporting
        private int GetDustIDForDestination(int index)
        {
            return index switch
            {
                0 => DustID.GreenTorch,
                1 => DustID.BlueTorch,
                2 => DustID.YellowTorch,
                3 => DustID.Torch,
                4 => DustID.WaterCandle,
                5 => DustID.OrangeTorch,
                6 => DustID.RedTorch,
                7 => DustID.PurpleTorch,
                _ => DustID.MagicMirror
            };
        }

        // Helper method that spawns a ring of dust particles around the player on teleport
        private void SpawnRingDust(Vector2 centerPosition, int dustType)
        {
            const int totalParticles = 40;
            const float expansionSpeed = 4.5f;

            for (int i = 0; i < totalParticles; i++)
            {
                // Calculates a uniform angle around the circle
                float angle = i / (float)totalParticles * MathHelper.TwoPi;
                Vector2 directionalVelocity = new((float)Math.Cos(angle), (float)Math.Sin(angle));

                // Spawn the dust directly on a offset, adjusting width/height to 0,0 ensures they 
                // all start at the exact point
                int d = Dust.NewDust(centerPosition, 0, 0, dustType, 0f, 0f, 100, default, 1.4f);

                Main.dust[d].noGravity = true; 

                // Appies the velocity so it can be moved outward frame-by-frame
                Main.dust[d].velocity = directionalVelocity * expansionSpeed;
            }
        }

        // Method that handles the locational teleport of the Wishing Glass
        private void ExecuteCustomWarp(int index)
        {
            switch (index)
            {
                case 0: // Spawn Location
                    Player.Teleport(new Vector2(Main.spawnTileX * 16, (Main.spawnTileY * 16) - 40), 1);
                    break;

                case 1: // Home / Bed
                    Player.Spawn(PlayerSpawnContext.RecallFromItem);
                    break;

                case 2: // Ocean
                    alternateOceanDirection = !alternateOceanDirection;
                    int boundaryX = alternateOceanDirection ? (Main.maxTilesX - 300) : 300;
                    int oceanSurfaceY = FindSurfaceTileY(boundaryX, 50);
                    Player.Teleport(new Vector2(boundaryX * 16, (oceanSurfaceY - 3) * 16), 1);
                    break;

                case 3: // Underworld
                    Player.Teleport(new Vector2(Player.position.X, (Main.maxTilesY - 150) * 16), 1);
                    break;

                case 4: // Dungeon Entry 
                    int dungeonSurfaceY = FindSurfaceTileY(Main.dungeonX, (int)Main.worldSurface - 100);
                    Player.Teleport(new Vector2((Main.dungeonX - 5) * 16, (dungeonSurfaceY - 3) * 16), 1);
                    break;

                case 5: // Lihzahrd Temple Entrance
                    Player.Teleport(FindTempleEntranceLocation(), 1);
                    break;

                case 6: // Last Death Location
                    if (Player.lastDeathPostion != Vector2.Zero)
                    {
                        Player.Teleport(Player.lastDeathPostion, 1);

                        if (!ModContent.GetInstance<WishingGlassConfig>().EnableInfiniteDeathLocationTeleportation)
                        {
                            hasUsedDeathTeleportForCurrentDeath = true;
                        }
                    }
                    break;

                case 7: // Random Teleportation
                    Player.TeleportationPotion();
                    break;
            }
        }

        // Helper method to help locating the surface tile of certain teleportation location
        private int FindSurfaceTileY(int tileX, int startY)
        {
            for (int y = startY; y < Main.maxTilesY; y++)
            {
                if (Main.tile[tileX, y].HasTile && Main.tileSolid[Main.tile[tileX, y].TileType])
                {
                    return y;
                }
            }

            return (int)Main.worldSurface;
        }

        // Helper method to help with locating the Jungle Temple and teleport to the entrance/Altar
        // depending on the Golem defeat state
        private Vector2 FindTempleEntranceLocation()
        {
            int startX = 150;
            int endX = Main.maxTilesX - 150;

            // On Golem defeat
            if (NPC.downedGolemBoss)
            {
                for (int x = startX; x < endX; x += 2)
                {
                    for (int y = (int)Main.worldSurface; y < Main.maxTilesY - 150; y += 2)
                    {
                        if (Main.tile[x, y].HasTile && Main.tile[x, y].TileType == TileID.LihzahrdAltar)
                        {
                            return new Vector2(x * 16, (y - 3) * 16);
                        }
                    }
                }
            }

            // Golem hasn't been defeated
            for (int x = startX; x < endX; x++)
            {
                for (int y = (int)Main.worldSurface; y < Main.maxTilesY - 200; y++)
                {
                    Tile tile = Main.tile[x, y];

                    if (tile.HasTile && (tile.TileType == TileID.ClosedDoor || tile.TileType == TileID.OpenDoor))
                    {
                        int doorStyle = tile.TileFrameY / 54;

                        if (doorStyle == 11 || doorStyle == 12)
                        {
                            int standX = (!Main.tile[x - 1, y].HasTile) ? x - 2 : x + 2;
                            return new Vector2(standX * 16, y * 16);
                        }
                    }
                }
            }
        
        // Fallback if Jungle Temple door couldn't be located
        return new Vector2(Main.spawnTileX * 16, Main.spawnTileY * 16);

        }
    }
}
