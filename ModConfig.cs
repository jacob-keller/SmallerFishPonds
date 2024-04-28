using Microsoft.Xna.Framework;
using StardewModdingAPI;

namespace SmallerFishPondsSpace
{
    public sealed class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool InstantConstruction { get; set; } = false;
        public bool KeepSmallSizeOnSave { get; set; } = true;
    }
}
