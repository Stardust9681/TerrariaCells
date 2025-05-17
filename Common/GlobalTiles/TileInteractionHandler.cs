using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaCells.Common.Configs;

namespace TerrariaCells.Common.GlobalTiles
{
    public class TileInteractionHandler : GlobalTile
    {
        public override void Load()
        {
            On_TileSmartInteractCandidateProvider.FillPotentialTargetTiles += On_TileSmartInteractCandidateProvider_FillPotentialTargetTiles;
            On_Player.TileInteractionsUse += On_Player_TileInteractionsUse;
            On_Player.InInteractionRange += On_Player_InInteractionRange;
        }

        public override void SetStaticDefaults()
        {
            InvalidSmartCursorTargets = new HashSet<int>() {
                //Wiring
                TileID.GemLocks,
                TileID.Switches,
                TileID.Lever,
            };
            InteractionTargets = new HashSet<int>() {
                //Wiring
                TileID.GemLocks,
                TileID.Switches,
                TileID.Lever,
                //Game objects
                TileID.Containers,
                TileID.Containers2,
                TileID.Heart,
                TileID.ManaCrystal,
                TileID.CatBast,
                //Teleports
                TileID.TeleportationPylon,
                ModContent.TileType<Content.Tiles.LevelExitPylon.ForestExitPylon>(),
                ModContent.TileType<Content.Tiles.HivePylon>(),
                ModContent.TileType<Content.Tiles.CrimsonPylon>(),
            };
        }

        //Tiles that cannot be auto-selected by Smart Cursor
        private static HashSet<int> InvalidSmartCursorTargets;
        //Tiles that can be interacted with
        private static HashSet<int> InteractionTargets;

        public static bool CanInteract(int tileID)
        {
            return InteractionTargets.Contains(tileID);
        }
        public static bool CanSmartCursor(int tileID)
        {
            if (!CanInteract(tileID)) return false;
            return !InvalidSmartCursorTargets.Contains(tileID);
        }

        private static System.Reflection.FieldInfo TileSmartInteract_Targets = typeof(TileSmartInteractCandidateProvider).GetField("targets", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        private void On_TileSmartInteractCandidateProvider_FillPotentialTargetTiles(On_TileSmartInteractCandidateProvider.orig_FillPotentialTargetTiles orig, TileSmartInteractCandidateProvider self, SmartInteractScanSettings settings)
        {
            orig.Invoke(self, settings);
            if (DevConfig.Instance.BuilderMode)
                return;
            List<Tuple<int, int>> targets = (List<Tuple<int, int>>)TileSmartInteract_Targets.GetValue(self);
            if (targets.Count > 0)
            {
                List<int> toRemove = new List<int>();
                for (int i = 0; i < targets.Count; i++)
                {
                    Tuple<int, int> tuple_XY = targets[i];
                    if (!WorldGen.InWorld(tuple_XY.Item1, tuple_XY.Item2))
                        goto RemoveAndContinue;
                    Tile tile = Framing.GetTileSafely(tuple_XY.Item1, tuple_XY.Item2);
                    if (!CanSmartCursor(tile.TileType))
                        goto RemoveAndContinue;

                    continue;

                RemoveAndContinue:
                    toRemove.Add(i);
                }
                for (int i = toRemove.Count - 1; i >= 0; i--)
                {
                    targets.RemoveAt(toRemove[i]);
                }
            }
            TileSmartInteract_Targets.SetValue(self, targets);
        }
        private bool On_Player_InInteractionRange(On_Player.orig_InInteractionRange orig, Player self, int interactX, int interactY, Terraria.DataStructures.TileReachCheckSettings settings)
        {
            if (DevConfig.Instance.BuilderMode)
            {
                return orig.Invoke(self, interactX, interactY, settings);
            }

            Tile tile = Framing.GetTileSafely(interactX, interactY);
            if (CanInteract(tile.TileType))
            {
                if (tile.TileType == TileID.TeleportationPylon)
                {
                    settings.OverrideXReach = Systems.WorldPylonSystem.MAX_PYLON_RANGE;
                    settings.OverrideYReach = Systems.WorldPylonSystem.MAX_PYLON_RANGE;
                }
                return orig.Invoke(self, interactX, interactY, settings);
            }
            return false;
        }
        private void On_Player_TileInteractionsUse(On_Player.orig_TileInteractionsUse orig, Player self, int myX, int myY)
        {
            if (DevConfig.Instance.BuilderMode)
            {
                orig.Invoke(self, myX, myY);
                return;
            }

            Tile tile = Framing.GetTileSafely(myX, myY);
            if (CanInteract(tile.TileType))
            {
                orig.Invoke(self, myX, myY);
            }
        }
    }
}
