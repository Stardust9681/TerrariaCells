using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaCells.Common.GlobalNPCs.NPCTypes.Shared
{
    internal partial class Worms : GlobalNPC
    {
        enum WormState
        {
            Idle,
            PreparingAttackHorizontal,
            ShowingAttackSpotHorizontal,
            AttackingHorizontal,
        }

        void SetDirection(NPC wormSegment, Vector2 direction)
        {
            wormSegment.rotation = MathF.Atan2(direction.Y, direction.X) + MathF.PI / 2;
        }

        void LerpTowardsDirection(NPC wormSegment, Vector2 direction)
        {
            Vector2 currentDirection = GetDirection(wormSegment);
            SetDirection(wormSegment, Vector2.Lerp(currentDirection, direction, 0.05f / 60f));
        }

        Vector2 GetDirection(NPC wormSegment)
        {
            float rotation = wormSegment.rotation - MathF.PI / 2;
            return new Vector2(MathF.Cos(rotation), MathF.Sin(rotation));
        }

        public override void SetDefaults(NPC npc)
        {
            base.SetDefaults(npc);

            //NPC.NewNPC();
            if (npc.type is NPCID.GiantWormBody or NPCID.GiantWormHead or NPCID.GiantWormTail)
            {
                npc.aiStyle = -1;
            }
            if (npc.type is NPCID.GiantWormHead)
            {
                npc.ai[2] = 5;
            }
        }

        public override bool PreAI(NPC npc)
        {
            if (npc.type is NPCID.GiantWormHead or NPCID.GiantWormBody)
            {
                TrySpawnSegmentBehind(npc);
            }

            if (npc.type is NPCID.GiantWormHead)
            {
                WormHeadAI(npc);
            }

            if (npc.type is NPCID.GiantWormBody or NPCID.GiantWormTail)
            {
                WormSegmentAndTailAI(npc);
            }

            return base.PreAI(npc);
        }

        public void WormHeadAI(NPC wormEntity)
        {
            wormEntity.TargetClosest();

            Player targetPlayer = Main.player[wormEntity.target];
            Vector2 targetPlayerCenter = targetPlayer.Center;
            Vector2 toPlayer = targetPlayer.position - wormEntity.position;
            float playerDistance = MathF.Sqrt(toPlayer.X * toPlayer.X + toPlayer.Y * toPlayer.Y);
            toPlayer.Normalize();
            Vector2 targetCenter = wormEntity.Center + GetDirection(wormEntity);
            float speed = 6f;
            Vector2 direction = GetDirection(wormEntity);

            switch ((WormState)(int)wormEntity.ai[3])
            {
                case WormState.Idle: //go in lying 8 figure
                    wormEntity.rotation -= MathF.PI / 60f;
                    if (playerDistance < 30f * 16f)
                    {
                        wormEntity.ai[3] = (float)WormState.PreparingAttackHorizontal;
                    }
                    break;
                case WormState.PreparingAttackHorizontal:
                    Main.NewText("preparing attack horizontal", Color.Green);
                    speed = 10f;
                    targetCenter = targetPlayerCenter + new Vector2(-MathF.Sign(toPlayer.X) * 16f * 50f, 0);
                    if (MathF.Sign(direction.X) == MathF.Sign(toPlayer.X) && Vector2.Distance(direction, toPlayer) < 0.1f)
                    {
                        wormEntity.ai[3] = (float)WormState.ShowingAttackSpotHorizontal;
                    }
                    break;
                case WormState.ShowingAttackSpotHorizontal:
                    Main.NewText("showing attack horizontal", Color.Yellow);
                    targetCenter = targetPlayerCenter + new Vector2(-MathF.Sign(toPlayer.X) * 16f * 30f, 0);
                    if (MathF.Abs(wormEntity.Center.X - targetPlayerCenter.X) < 16f * 30f)
                    {
                        SetDirection(wormEntity, targetPlayerCenter - wormEntity.position - new Vector2(0, 16f * 10f));
                        targetCenter = wormEntity.Center + GetDirection(wormEntity);
                        wormEntity.ai[3] = (float)WormState.AttackingHorizontal;
                    }
                    break;
                case WormState.AttackingHorizontal:
                    Main.NewText("attacking horizontal", Color.Red);
                    speed = 10f;
                    targetCenter.Y += 10f;
                    if (MathF.Sign(GetDirection(wormEntity).X) != MathF.Sign(toPlayer.X))
                    {
                        wormEntity.ai[3] = (float)WormState.PreparingAttackHorizontal;
                    }
                    break;
            }

            LerpTowardsDirection(wormEntity, targetCenter - wormEntity.Center);
            direction = GetDirection(wormEntity);
            wormEntity.Center += direction * speed;
        }

        public void WormSegmentAndTailAI(NPC wormEntity)
        {
            NPC segmentAhead = Main.npc[(int)wormEntity.ai[1]];

            Vector2 toSegmentAhead = segmentAhead.Center - wormEntity.Center;
            float distanceToSegmentAhead = MathF.Sqrt(toSegmentAhead.X * toSegmentAhead.X + toSegmentAhead.Y * toSegmentAhead.Y);
            toSegmentAhead.Normalize();

            if (distanceToSegmentAhead > 16f)
            {
                wormEntity.Center = segmentAhead.Center - toSegmentAhead * 16f;
            }
            SetDirection(wormEntity, toSegmentAhead);
        }

        void TrySpawnSegmentBehind(NPC wormEntity)
        {
            if (wormEntity.ai[0] != 0 || wormEntity.ai[2] == -1)
            {
                return;
            }

            if (wormEntity.type is NPCID.GiantWormHead)
            {
                wormEntity.ai[2] = 30;
            }

            int spawnedSegmentType;

            if (wormEntity.ai[2] == 0)
            {
                spawnedSegmentType = NPCID.GiantWormTail;
            }
            else
            {
                spawnedSegmentType = NPCID.GiantWormBody;
            }

            int spawnedSegmentIndex = NPC.NewNPC(new EntitySource_SpawnNPC(), (int)wormEntity.Center.X, (int)wormEntity.Center.Y + wormEntity.height / 2, spawnedSegmentType);
            wormEntity.ai[0] = spawnedSegmentIndex; //segment one behind

            NPC spawnedNPC = Main.npc[spawnedSegmentIndex];
            spawnedNPC.realLife = wormEntity.realLife;
            spawnedNPC.ai[1] = wormEntity.whoAmI; //segment one ahead
            spawnedNPC.ai[2] = wormEntity.ai[2] - 1; //segment count that still need to be spawned
            spawnedNPC.GetGlobalNPC<CombatNPC>().allowContactDamage = false;
            Main.dayTime = true;
        }
    }
}
