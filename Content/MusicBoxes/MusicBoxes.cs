using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace TerrariaCells.Content.MusicBoxes {
    internal class MusicBoxes : ModTile {
        public override void SetStaticDefaults() {
            Main.tileFrameImportant[Type] = true;
            Main.tileObsidianKill[Type] = true;
            TileID.Sets.HasOutlines[Type] = true;
            TileID.Sets.DisableSmartCursor[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
            TileObjectData.newTile.Origin = new Point16(0, 1);
            TileObjectData.newTile.LavaDeath = false;
            TileObjectData.newTile.DrawYOffset = 2;
            TileObjectData.newTile.StyleLineSkip = 2;
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(191, 142, 111), Language.GetText("ItemName.MusicBox"));
        }
        public override void MouseOver(int i, int j) {
            var tile = Main.tile[i, j];

            var player = Main.LocalPlayer;
            player.noThrow = 2;
            player.cursorItemIconEnabled = true;
            player.cursorItemIconID = MusicBox.items[tile.TileFrameY / 36];
        }

        public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) {
            return true;
        }

        public override void Load() {
            // we need to call it from here to prevent infinite recursive calls
            // it took me way too long to realise that's why I was getting
            // 'Access Violation' crashes-on-launch :(
            ModContent.GetInstance<MusicBox>().registerAll();
        }

        // not implementing particles because ExampleMusicBox uses an
        // unreleased hook and they're going to be echo coated anyway
    }
    internal class MusicBox : ModItem {
        internal static int[] items = [];
        int track;
        MusicBox() {
            // ModItems need to be default-constructable. We'll default to Boss1 and keep track of it later
            track = 0;
        }
        MusicBox(int track) {
            this.track = track;
        }
        static readonly string[] tracks = [
            "Boss1",
            "Caverns",
            "Corruption",
            "Credits",
            "Desert",
            "DesertAmbience",
            "Dungeon",
            "Factory",
            "Forest",
        ];
        public override string Name => base.Name + tracks[track];
        public override void SetStaticDefaults() {
            // These are probably irrelevant but I might as well set them
            ItemID.Sets.CanGetPrefixes[Type] = false;
            ItemID.Sets.ShimmerTransformToItem[Type] = ItemID.MusicBox;
            MusicLoader.AddMusicBox(
                Mod,
                MusicLoader.GetMusicSlot(Mod, $"Common/Assets/Music/{tracks[track]}"),
                Type,
                ModContent.TileType<MusicBoxes>(),
                track * 36
            );
        }

        protected override bool CloneNewInstances => true;

        public override void SetDefaults() {
            Item.DefaultToMusicBox(ModContent.TileType<MusicBoxes>(), track);
        }

        public void registerAll() {
            var mod = ModContent.GetInstance<TerrariaCells>();
            int add(int track) {
                MusicBox box = new(track);
                mod.AddContent(box);
                return box.Type;
            }
            items = [
                this.Type, // Boss1
                add(1), // Caverns (Elyse - Tectonic Etude)
                add(2), // Corruption (Elyse - Thistle Thorns)
                add(3), // Credits (Aeolian - Beautiful Isolation)
                add(4), // Desert
                add(5), // Desert Ambience
                add(6), // Dungeon (Aeolian - Necrosis)
                add(7), // Factory (Aeolian - Apathy)
                add(8), // Forest (NACHOZ)
            ];
        }
    }
}
