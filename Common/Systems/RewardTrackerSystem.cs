using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.DataStructures;

namespace TerrariaCells.Common.Systems
{
    public class RewardTrackerSystem : ModSystem
    {
        [FlagsAttribute]
        public enum TrackerAction : byte
        {
            None = 0,

            Start = 1 << 0,
            Pause = 1 << 1,
            Reset = 1 << 2,

            Stop = Pause | Reset,
            Restart = Start | Reset,
        }
        internal static void UpdateTracker_EnterNewWorld()
        {
            UpdateTracker(TrackerAction.Restart);
            targetTime = TimeSpan.FromMinutes(3);
            targetKillCount = 50;
        }
        public static void UpdateTracker(TrackerAction action)
        {
            for (byte i = 0; i < sizeof(TrackerAction) * 8; i++)
            {
                switch (action & (TrackerAction)(1 << i))
                {
                    case TrackerAction.Start:
                        trackerEnabled = true;
                        break;
                    case TrackerAction.Pause:
                        trackerEnabled = false;
                        break;
                    case TrackerAction.Reset:
                        levelTimer = 0;
                        killCount = 0;
                        break;
                }
            }
            trackerState = action;
        }
        public static TrackerAction trackerState = TrackerAction.Start;
        public static TimeSpan targetTime;
        public static byte targetKillCount;
        internal static void UpdateChests_OnTeleport()
        {
            bool shouldUnlock = false;
            List<Point> validChests = new List<Point>();

            for (int k = 0; k < Main.maxPlayers; k++)
            {
                Player player = Main.player[k];
                Point tilePos = player.Center.ToTileCoordinates();
                const int RANGE = 30;
                for (int j = -RANGE; j < RANGE; j++)
                {
                    for (int i = -RANGE; i < RANGE; i++)
                    {
                        Point checkPos = new Point(tilePos.X + i, tilePos.Y + j);
                        if (!WorldGen.InWorld(checkPos.X, checkPos.Y))
                            continue;
                        Tile tile = Framing.GetTileSafely(checkPos);
                        if (tile.TileType != TileID.Containers)
                            continue;
                        //Styles 3, 4
                        int style = tile.TileFrameX / 36;
                        if (style != 3 && style != 4)
                            continue;
                        if (tile.TileFrameX % 36 != 0 || tile.TileFrameY % 36 != 0)
                            continue;

                        validChests.Add(checkPos);
                    }
                }
            }

                int rewardsCount = (LevelTime.TotalSeconds / targetTime.TotalSeconds) switch
                {
                    < 0.4 => 1, //Platinum
                    < 0.7 => 1, //Gold
                    < 1 => 0, //Silver
                    _ => 0 //Copper
                };
                //If we intend to add bonuses for kill counter as well:
                /*float allKills = (float)killCount / (float)targetKillCount;
                rewardsCount += allKills switch
                {
                    < 0.3f => 0,
                    < 0.6f => 0,
                    < .9f => 1,
                    _ => 1
                };*/

            ref List<int> lootedChests = ref ModContent.GetInstance<Systems.ChestLootSpawner>().lootedChests;
            foreach (Point chest in validChests)
            {
                int chestIndex = Chest.FindChest(chest.X, chest.Y);
                lootedChests.Remove(chestIndex);

                Chest.Lock(chest.X, chest.Y);
                if (rewardsCount > 0)
                    Chest.Unlock(chest.X, chest.Y);
            }
        }
        internal static void UpdateChests_Open(int x, int y, Player? player = null)
        {
            if (!WorldGen.InWorld(x, y))
                return;
            Tile tile = Framing.GetTileSafely(x, y);
            if (tile.TileType != TileID.Containers)
                return;
            //Styles 3, 4
            int style = tile.TileFrameX / 36;
            if (style != 3 && style != 4)
                return;

            int rewardsCount = (LevelTime.TotalSeconds / targetTime.TotalSeconds) switch
            {
                < 0.4 => 1, //Platinum
                < 0.7 => 1, //Gold
                < 1 => 0, //Silver
                _ => 0 //Copper
            };
            //If we intend to add bonuses for kill counter as well:
            /*float allKills = (float)killCount / (float)targetKillCount;
            rewardsCount += allKills switch
            {
                < 0.3f => 0,
                < 0.6f => 0,
                < .9f => 1,
                _ => 1
            };*/

            int weaponType = Main.rand.Next(GlobalNPCs.VanillaNPCShop.Weapons);
            if (player is null)
            {
                float distance = -1;
                foreach (Player p in Main.ActivePlayers)
                {
                    if (p.Center.Distance(new Vector2(x, y).ToWorldCoordinates()) < distance || distance == -1)
                    {
                        player = p;
                    }
                }
            }
            Item droppedItem = Main.item[Item.NewItem(player.GetSource_TileInteraction(x, y), Utils.ToWorldCoordinates(new Vector2(x, y)), weaponType, 1, true, 0, true)];
            if (droppedItem.TryGetGlobalItem<GlobalItems.TierSystemGlobalItem>(out var tierItem))
            {
                tierItem.SetLevel(droppedItem, ModContent.GetInstance<Systems.TeleportTracker>().level + 1);
            }
        }

        private static bool trackerEnabled = false;
        internal static uint levelTimer = 0;
        internal static byte killCount = 0;

        public override void PostUpdatePlayers()
        {
            if (Main.gameMenu)
                return;
            if (Main.gamePaused)
                return;

            if (trackerEnabled)
            {
                levelTimer++;
            }
        }

        public override void SaveWorldData(TagCompound tag)
        {
            if (trackerEnabled)
                tag.Add(nameof(trackerEnabled), trackerEnabled);
            if (levelTimer > 0)
                tag.Add(nameof(levelTimer), levelTimer);
            if (killCount > 0)
                tag.Add(nameof(killCount), killCount);
            if (trackerState != TrackerAction.None)
                tag.Add(nameof(trackerState), (byte)trackerState);
        }
        public override void LoadWorldData(TagCompound tag)
        {
            trackerEnabled = tag.Get<bool>(nameof(trackerEnabled));
            levelTimer = tag.Get<uint>(nameof(levelTimer));
            killCount = tag.Get<byte>(nameof(killCount));
            trackerState = (TrackerAction)tag.Get<byte>(nameof(trackerState));
        }

        public static uint _LevelTime => levelTimer;
        public static TimeSpan LevelTime => TimeSpan.FromSeconds(levelTimer / 60.0);
    }
}
