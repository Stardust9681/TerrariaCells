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
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace TerrariaCells.Common.GlobalTiles
{
    public class TileInteractionHandler : GlobalTile
    {
        public override void Load()
        {
            On_TileSmartInteractCandidateProvider.FillPotentialTargetTiles += On_TileSmartInteractCandidateProvider_FillPotentialTargetTiles;
            IL_TileSmartInteractCandidateProvider.FillPotentialTargetTiles += IL_TileSmartInteractCandidateProvider_FillPotentialTargetTiles;
            On_Player.TileInteractionsUse += On_Player_TileInteractionsUse;
            On_Player.InInteractionRange += On_Player_InInteractionRange;
            On_Player.TileInteractionsMouseOver += On_Player_TileInteractionsMouseOver;
        }

        public override void Unload()
        {
            On_TileSmartInteractCandidateProvider.FillPotentialTargetTiles -= On_TileSmartInteractCandidateProvider_FillPotentialTargetTiles;
            IL_TileSmartInteractCandidateProvider.FillPotentialTargetTiles -= IL_TileSmartInteractCandidateProvider_FillPotentialTargetTiles;
            On_Player.TileInteractionsUse -= On_Player_TileInteractionsUse;
            On_Player.InInteractionRange -= On_Player_InInteractionRange;
            On_Player.TileInteractionsMouseOver -= On_Player_TileInteractionsMouseOver;
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
        public static bool CanSmartInteract(int tileID)
        {
            if (!CanInteract(tileID)) return false;
            return !InvalidSmartCursorTargets.Contains(tileID);
        }
        public static bool IsSpent(int i, int j)
        {
            return IsSpent(i, j, Framing.GetTileSafely(i, j));
        }
        public static bool IsSpent(int i, int j, Tile tile)
        {
            switch (tile.TileType)
            {
                case TileID.Containers:
                case TileID.Containers2:
                case TileID.FakeContainers:
                case TileID.FakeContainers2:
                    Systems.ChestLootSpawner instance = ModContent.GetInstance<Systems.ChestLootSpawner>();
                    if (tile.TileFrameX % 36 != 0)
                        i--;
                    if (tile.TileFrameY % 38 != 0)
                        j--;
                    return instance.lootedChests.Any(c => (Main.chest[c].x == i && Main.chest[c].y == j) && Main.chest[c].frame > 0);
                case TileID.Heart:
                case TileID.ManaCrystal:
                    return tile.IsActuated;
                case TileID.CatBast:
                    return tile.TileFrameX >= 72;
                default:
                    return false;
            }
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
                    if (!CanSmartInteract(tile.TileType) || IsSpent(tuple_XY.Item1, tuple_XY.Item2, tile))
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
        private void IL_TileSmartInteractCandidateProvider_FillPotentialTargetTiles(ILContext context)
        {
            try
            {
                ILCursor cursor = new ILCursor(context);

                if (!cursor.TryGotoNext(
                    i => i.Match(OpCodes.Ldloca_S),
                    i => i.MatchCall<Tile>("get_type")))
                    return;

                cursor.EmitLdloc(2); //Tile
                //cursor.EmitCall(typeof(Tile).GetProperty("TileType", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).GetMethod); //Tile type
                cursor.Emit(OpCodes.Ldarg_0); //TileSmartInteractCandidateProvider
                cursor.EmitLdfld(typeof(TileSmartInteractCandidateProvider).GetField("targets", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)); //List<Tuple<int, int>>
                cursor.Emit(OpCodes.Ldloc_0); //int
                cursor.Emit(OpCodes.Ldloc_1); //int
                cursor.EmitDelegate((Tile tile, List<Tuple<int, int>> targets, int i, int j) => {
                    int type = tile.TileType;
                    Tuple<int, int> tilePos = new Tuple<int, int>(i, j);
                    if (CanSmartInteract(type) && !targets.Contains(tilePos))
                        targets.Add(tilePos);
                });

            }
            catch (Exception x)
            {
            }
        }
        private void On_Player_TileInteractionsMouseOver(On_Player.orig_TileInteractionsMouseOver orig, Player self, int myX, int myY)
        {
            orig.Invoke(self, myX, myY);
            if (self.cursorItemIconID == self.HeldItem.type)
            {
                self.cursorItemIconEnabled = false;
                return;
            }
            Tile tile = Framing.GetTileSafely(myX, myY);
            if (CanInteract(tile.TileType) && !IsSpent(myX, myY, tile))
            {
                self.cursorItemIconID = tile.TileType switch
                {
                    TileID.Heart => ItemID.LifeCrystal,
                    TileID.CatBast => ItemID.CatBast,
                    TileID.ManaCrystal => ItemID.ManaCrystal,
                    _ => self.cursorItemIconID,
                };
                self.cursorItemIconEnabled = self.cursorItemIconID != 0;
            }
            else
            {
                self.cursorItemIconEnabled = false;
                return;
            }
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
