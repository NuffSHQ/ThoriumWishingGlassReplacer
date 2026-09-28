using Microsoft.Xna.Framework.Input;
using Terraria.ModLoader;

namespace ThoriumWishingGlassReplacer.Content.Keybind
{
    /// <summary>
    /// Class that handles keybinds
    /// </summary>
    public class WishingGlassKeybinds : ModSystem
    {
        public static ModKeybind CycleLeftKey { get; private set; }
        public static ModKeybind CycleRightKey { get; private set; }
        public static ModKeybind QuickUseKey { get; private set; }

        // On Load display the default keybinds 
        public override void Load()
        {
            CycleLeftKey = KeybindLoader.RegisterKeybind(Mod, "CycleDestinationLeft", Keys.Left);
            CycleRightKey = KeybindLoader.RegisterKeybind(Mod, "CycleDestinationRight", Keys.Right);
            QuickUseKey = KeybindLoader.RegisterKeybind(Mod, "QuickUseWishingGlass", "H");
        }

        // Sets the keybinds to null once mod is unloaded
        public override void Unload()
        {
            CycleLeftKey = null;
            CycleRightKey = null;
            QuickUseKey = null;
        }
    }
}
