using System.Collections.Generic;

namespace BalartroLike.Run
{
    // TODO: 道行解锁商店、天劫 4+ 词条尚未接入；当前已接入天劫难度与图鉴熟练度。
    public sealed class RunMetaProgressState
    {
        public int DaoHeart { get; set; }
        public int HighestHeavenTribulation { get; set; }
        public int SelectedHeavenTribulation { get; set; }
        public int CompletedRunCount { get; set; }
        public Dictionary<string, int> HexagramUses { get; } = new Dictionary<string, int>();
    }
}