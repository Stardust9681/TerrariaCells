using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.IO;
using Terraria.ModLoader.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections;
using System.Reflection;

namespace TerrariaCells.Common.ModPlayers
{
    public class MetaPlayer : ModPlayer
    {
        //Flags for progression. Literally just add whatever and it should work <3
        public bool CloudJump { get => metaProgression[0]; set => metaProgression[0] = value; }
        public bool Goblin { get => metaProgression[1]; set => metaProgression[1] = value; }

        //-1 because this is a property
        internal static int ProgressionCount => typeof(MetaPlayer).GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Length - 1;

        #region Backing Functionality
        private BitArray metaProgression = new BitArray(ProgressionCount);
        public override void SaveData(TagCompound tag)
        {
            if(metaProgression?.HasAnySet() == true)
                tag.Add("TerraCells:MetaProgress", metaProgression);
        }
        public override void LoadData(TagCompound tag)
        {
            int expectedLength = ProgressionCount;
            if (tag.TryGet<BitArray>("TerraCells:MetaProgress", out metaProgression))
            {
                if (metaProgression.Length < expectedLength)
                {
                    BitArray oldBits = new BitArray(metaProgression);
                    metaProgression = new BitArray(expectedLength);
                    for (int i = 0; i < oldBits.Length; i++)
                    {
                        metaProgression[i] = oldBits[i];
                    }
                }
            }
            else
            {
                metaProgression = new BitArray(expectedLength);
            }
        }
        #endregion
    }
    //TagCompound doesn't handle BitArray normally :/
    public class BitArraySerializer : TagSerializer<BitArray, TagCompound>
    {
        internal static FieldInfo BitArray_ArrayINT_m_array = typeof(BitArray).GetField("m_array", BindingFlags.NonPublic | BindingFlags.Instance);
        internal static FieldInfo BitArray_Int_m_length = typeof(BitArray).GetField("m_length", BindingFlags.NonPublic | BindingFlags.Instance);
        internal static FieldInfo BitArray_Int_version = typeof(BitArray).GetField("_version", BindingFlags.NonPublic | BindingFlags.Instance);

        public override BitArray Deserialize(TagCompound tag)
        {
            int[] arrs = tag.Get<int[]>("m_array");
            return new BitArray(arrs);
        }

        public override TagCompound Serialize(BitArray value)
        {
            return new TagCompound()
            {
                ["m_array"] = (int[])BitArray_ArrayINT_m_array.GetValue(value)
            };
        }
    }
}
