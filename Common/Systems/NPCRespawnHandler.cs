using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Terraria.GameContent;

using MonoMod.Cil;
using Mono.Cecil.Cil;
using Terraria.ModLoader.IO;

namespace TerrariaCells.Common.Systems
{
	//TODO
	//Multiplayer compat
    public class NPCRespawnHandler : ModSystem
    {
        public struct NPCRespawnMarker
        {
            internal NPCRespawnMarker(int type, Vector2 worldPos, int health)
            {
                NPCType = type;
                RespawnTile = worldPos.ToTileCoordinates16();
                HealthLeft = health;
            }
            public int NPCType;
            public Point16 RespawnTile;
            public int HealthLeft;
        }
        internal static List<NPCRespawnMarker> RespawnMarkers;

        public override void Load()
        {
            On_NPC.CheckActive += On_NPC_CheckActive;
            On_CoinLossRevengeSystem.CacheEnemy += On_CacheEnemyForRespawn;
        }

        public override void Unload()
        {
            On_NPC.CheckActive -= On_NPC_CheckActive;
            On_CoinLossRevengeSystem.CacheEnemy -= On_CacheEnemyForRespawn;
        }

        private static void On_NPC_CheckActive(On_NPC.orig_CheckActive orig, NPC self)
        {
            bool wasActive = self.active;
            orig.Invoke(self);
            if (self.life > 1 && !NPCID.Sets.ProjectileNPC[self.type] && wasActive && !self.active)
            {
                Vector2 savePos = self.position;
                if (NPCID.Sets.SpecialSpawningRules.TryGetValue(self.type, out int value) && value == 0)
                    savePos = new Vector2(self.ai[0], self.ai[1]).ToWorldCoordinates();
                RespawnMarkers.Add(
                    new NPCRespawnMarker(
                        self.type,
                         savePos, //Utilities.TCellsUtils.FindGround(self.getRect(), 20) - new Vector2(self.width * 0.5f, self.height * 0.5f),
                        self.life));
            }
        }

        //Prepare datastructures
        public override void ClearWorld()
        {
            RespawnMarkers = new List<NPCRespawnMarker>();
        }

        //More or less modified source for respawn system
        public override void PostUpdateNPCs()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            if (RespawnMarkers.Count == 0)
                return;

			//Disable spawns if disabled
			if (Configs.DevConfig.Instance.DisableSpawns)
				return;

            List<Rectangle> respawnRects = new List<Rectangle>();
            Vector2 rectSize = new Vector2(2608f*0.67f, 1840f*0.67f);
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player player = Main.player[i];
                if (!player.active || player.dead)
                    continue;
                respawnRects.Add(Utils.CenteredRectangle(player.Center, rectSize));
            }
            if (respawnRects.Count == 0)
                return;

            bool RectContainsPt16(Rectangle rectangle, Point16 pt16)
            {
                int x = pt16.X << 4;
                int y = pt16.Y << 4;
                return rectangle.Left < x
                    && rectangle.Right > x
                    && rectangle.Top < y
                    && rectangle.Bottom > y;
            }

            for (int i = RespawnMarkers.Count - 1; i > -1; i--)
            {
                NPCRespawnMarker marker = RespawnMarkers[i];
                bool shouldRespawn = false;
                foreach (Rectangle rect in respawnRects)
                {
                    if (RectContainsPt16(rect, marker.RespawnTile))
                    {
                        shouldRespawn = true;
                        break;
                    }
                }
                if (!shouldRespawn)
                    continue;
                RespawnMarkers.RemoveAt(i);

                NPC newNPC = NPC.NewNPCDirect(
                    new EntitySource_RevengeSystem(),
                    marker.RespawnTile.ToWorldCoordinates(),
                    marker.NPCType);
                if (marker.HealthLeft != newNPC.lifeMax)
                {
                    newNPC.life = marker.HealthLeft;
                    newNPC.netUpdate = true;
                }
                if (NPCID.Sets.SpecialSpawningRules.TryGetValue(newNPC.netID, out int value) && value == 0)
                {
                    newNPC.ai[0] = marker.RespawnTile.X;
                    newNPC.ai[1] = marker.RespawnTile.Y;
                    newNPC.netUpdate = true;
                }
                newNPC.timeLeft = 3600; //1 min
            }
        }

        private void On_CacheEnemyForRespawn(On_CoinLossRevengeSystem.orig_CacheEnemy orig, CoinLossRevengeSystem self, NPC npc)
        {
            return;
        }





        /// <summary>
        /// UNUSED : Implemented with intent to use Vanilla RevengeMarker system, replaced with a new one
        /// </summary>
        /// <param name="context"></param>
        private static void IL_CheckActiveToCacheNPCs(ILContext context)
        {
            log4net.ILog GetInstanceLogger() => ModContent.GetInstance<TerrariaCells>().Logger;
            try
            {
                ILCursor cursor = new ILCursor(context);

                if (!cursor.TryGotoNext(MoveType.Before,
                        i => i.MatchLdarg0(),
                        i => i.MatchLdfld<NPC>(nameof(NPC.extraValue)),
                        i => i.MatchLdcI4(0),
                        i => i.Match(OpCodes.Ble_S)))
                {
                    GetInstanceLogger().Error($"Couldn't match IL Edit: {context.Method.Name} @ {cursor.Index}");
                    return;
                }

                cursor.Index += 0;
                ILLabel jumpBack = cursor.MarkLabel();

                if (!cursor.TryGotoNext(MoveType.After,
                        i => i.MatchLdarg0(),
                        i => i.MatchLdfld<NPC>(nameof(NPC.extraValue)),
                        i => i.MatchLdcI4(0),
                        i => i.Match(OpCodes.Ble_S)))
                {
                    GetInstanceLogger().Error($"Couldn't match IL Edit: {context.Method.Name} @ {cursor.Index}");
                    return;
                }

                ILLabel IL_038D = cursor.MarkLabel();
                cursor.GotoLabel(jumpBack);
                cursor.EmitBr(IL_038D);
            }
            catch (Exception x)
            {
                MonoModHooks.DumpIL(ModContent.GetInstance<TerrariaCells>(), context);
            }
        }

        /// <summary>
        /// UNUSED : Implemented with intent to use Vanilla RevengeMarker system, replaced with a new one
        /// </summary>
        /// <param name="context"></param>
        private static void Dep_IL_CacheEnemyForRespawn(ILContext context)
        {
            log4net.ILog GetInstanceLogger() => ModContent.GetInstance<TerrariaCells>().Logger;
            try
            {
                ILCursor cursor = new ILCursor(context);

                ILLabel jumpBack = cursor.MarkLabel();

                if (!cursor.TryGotoNext(MoveType.Before,
                        i => i.MatchLdarg1(),
                        i => i.MatchLdfld<NPC>(nameof(NPC.netID)),
                        i => i.MatchStloc0()))
                {
                    GetInstanceLogger().Error($"Couldn't match IL Edit: {context.Method.Name} @ {cursor.Index}");
                    return;
                }
                ILLabel IL_00BC = cursor.MarkLabel();

                cursor.GotoLabel(jumpBack);
                cursor.EmitBr(IL_00BC);
            }
            catch (Exception x)
            {
                MonoModHooks.DumpIL(ModContent.GetInstance<TerrariaCells>(), context);
            }
        }
    }
}
