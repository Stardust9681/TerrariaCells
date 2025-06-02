using System;
using System.Linq;
using System.Text.RegularExpressions;
using Terraria;
using Terraria.GameContent.Personalities;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaCells.Common.SceneEffects;

public class DesertMusicEffect : ModSceneEffect
{
    public override int Music => MusicLoader.GetMusicSlot("TerrariaCells/Music/Pyramid ambience(1)");
    public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;

    public override bool IsSceneEffectActive(Player player)
    {
        return player.ZoneDesert;
    }
}

// public class UndergroundDesertMusicEffect : ModSceneEffect
// {
//     public override int Music => MusicLoader.GetMusicSlot("TerrariaCells/Pyramid 1.2(2)");
//     public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;

//     public override bool IsSceneEffectActive(Player player)
//     {
//         return player.ZoneUndergroundDesert;
//     }
// }

