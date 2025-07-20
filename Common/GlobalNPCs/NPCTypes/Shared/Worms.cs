using Microsoft.Build.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaCells.Common.GlobalNPCs.NPCTypes.Shared
{
    internal partial class Worms : GlobalNPC
    {
        //A worm goes in circles until a player is near

        //then it idles in a figure 8 pattern under the player until it finds a spot near the player to attack
        //the attack will be randomly vertical or horizontal if both are possible
        //once it found a spot, the worm will go behind the attack spot in the span of 1 second (the spot will now have a particle effect warning for a worm attack)
        //and then towards the attack spot again in the span of 1 second
        //after reaching the attack spot it will go horizontally or vertically
        //after the attack started it will slowly fall down but also slightly stear towards the player
        //once the worm is beneath the player and some distance away the attack cooldown start during which it idles under the player again
        //this loop repeats until defeated

        public override bool InstancePerEntity => true;

        public float AttackSpeed = 100f; //Speed of the worm at the start of its attack in tiles per second
        public int AttackCooldownTicks = 180;
        public int MaxHorizontalAttackDistance = 30; //Max distance in tiles how far the worm can attack from horizontally
        public int MaxVerticalAttackDistance = 20; //Max distance in tiles how far the worm can attack from vertically
        public int AttackEndDistance = 30; //Distance after which, if beneath the player, the attack stops in tiles
        public int AttackWarningDust = DustID.Sand;
        public SoundStyle AttackWarningSound = SoundID.WormDig;

        //THESE TWO NEED TO BE UPDATED FOR EVERY NEW TYPE OF WORM
        public static Dictionary<int, int> WormHeadToBody = new Dictionary<int, int>()
        {
            { NPCID.GiantWormHead, NPCID.GiantWormBody },
        };
        public static Dictionary<int, int> WormBodyToTail = new Dictionary<int, int>()
        {
            { NPCID.GiantWormBody, NPCID.GiantWormTail },
        };

        enum WormState
        {
            Idle,
            PreparingAttack,
            GoingToAttackSpotHorizontalRight,
            GoingToAttackSpotHorizontalLeft,
            GoingToAttackSpotVertical,
            Attacking,
        }

        static NPC targetWormEntity;

        static int behindSegmentIndex
        {
            get
            {
                int IntAI = BitConverter.ToInt32(BitConverter.GetBytes(targetWormEntity.ai[0]), 0);
                int bitMask = 0b0000_0000_0000_0000_1111_1111_1111_1111;
                IntAI &= bitMask;

                return IntAI;
            }
            set
            {
                int top16Bits = BitConverter.ToInt32(BitConverter.GetBytes(targetWormEntity.ai[0]), 0);
                int bitMask = 0b0000_0000_0000_0000_1111_1111_1111_1111 ^ -1;
                top16Bits &= bitMask;

                int bottom16Bits = value;
                bottom16Bits &= bitMask ^ -1;

                float floatAI0 = BitConverter.ToSingle(BitConverter.GetBytes(bottom16Bits | top16Bits), 0);

                targetWormEntity.ai[0] = floatAI0;
            }
        }

        static int aheadSegmentIndex
        {
            get
            {
                int IntAI = BitConverter.ToInt32(BitConverter.GetBytes(targetWormEntity.ai[0]), 0);

                return IntAI >> 16;
            }
            set
            {
                int bottom16Bits = BitConverter.ToInt32(BitConverter.GetBytes(targetWormEntity.ai[0]), 0);
                int bitMask = 0b0000_0000_0000_0000_1111_1111_1111_1111;
                bottom16Bits &= bitMask;

                int top16Bits = value << 16;
                top16Bits &= bitMask ^ -1;

                float floatAI0 = BitConverter.ToSingle(BitConverter.GetBytes(bottom16Bits | top16Bits), 0);

                targetWormEntity.ai[0] = floatAI0;
            }
        }

        static Vector2 targetPosition
        {
            get
            {
                int intAI = BitConverter.ToInt32(BitConverter.GetBytes(targetWormEntity.ai[1]), 0);

                int bitMask = 0b0000_0000_0000_0000_1111_1111_1111_1111;
                float X = (float)BitConverter.ToHalf(BitConverter.GetBytes(intAI & bitMask), 0);
                float Y = (float)BitConverter.ToHalf(BitConverter.GetBytes((intAI & (bitMask ^ -1)) >> 16), 0);

                return new Vector2(X, Y);
            }
            set
            {
                int bottom16Bits = BitConverter.ToInt16(BitConverter.GetBytes((Half)value.X), 0);
                int top16Bits = BitConverter.ToInt16(BitConverter.GetBytes((Half)value.Y), 0) << 16;

                targetWormEntity.ai[1] = BitConverter.ToSingle(BitConverter.GetBytes(bottom16Bits | top16Bits), 0);
            }
        }

        static int segmentCountBehind
        {
            get
            {
                int IntAI = BitConverter.ToInt32(BitConverter.GetBytes(targetWormEntity.ai[2]), 0);
                int bitMask = 0b0000_0000_0000_0000_1111_1111_1111_1111;
                IntAI &= bitMask;

                return IntAI;
            }
            set
            {
                int top16Bits = BitConverter.ToInt32(BitConverter.GetBytes(targetWormEntity.ai[2]), 0);
                int bitMask = 0b0000_0000_0000_0000_1111_1111_1111_1111 ^ -1; //xor because if the leading bit is 1, it's seen as uint and the compiler complains
                top16Bits &= bitMask;

                int bottom16Bits = value;
                bitMask ^= -1; //reverse the bits
                bottom16Bits &= bitMask;

                targetWormEntity.ai[2] = BitConverter.ToSingle(BitConverter.GetBytes(bottom16Bits | top16Bits), 0);
            }
        }

        static int ticksUntilAttack
        {
            get
            {
                int IntAI = BitConverter.ToInt32(BitConverter.GetBytes(targetWormEntity.ai[2]), 0);
                int bitMask = 0b0000_0000_0000_0000_1111_1111_1111_1111 ^ -1;
                IntAI &= bitMask;

                return IntAI >> 16;
            }
            set
            {
                int bottom16Bits = (short)BitConverter.ToInt32(BitConverter.GetBytes(targetWormEntity.ai[2]), 0);
                int bitMask = 0b0000_0000_0000_0000_1111_1111_1111_1111;
                bottom16Bits &= bitMask;

                int top16Bits = ((short)value) << 16;

                targetWormEntity.ai[2] = BitConverter.ToSingle(BitConverter.GetBytes(bottom16Bits | top16Bits), 0);
            }
        }

        static WormState wormState
        {
            get
            {
                int IntAI = BitConverter.ToInt32(BitConverter.GetBytes(targetWormEntity.ai[3]), 0);
                int bitMask = 0b0000_1111_1111_1111_1111_1111_1111_1111 ^ -1; //xor because if the leading bit is 1, it's seen as uint and the compiler complains
                IntAI &= bitMask;

                return (WormState)(IntAI >> 28);
            }
            set
            {
                int top4Bits = (int)value;
                top4Bits <<= 28;
                int bitMask = 0b0000_1111_1111_1111_1111_1111_1111_1111 ^ -1; //xor because if the leading bit is 1, it's seen as uint and the compiler complains
                top4Bits &= bitMask;

                int bottom28Bits = BitConverter.ToInt32(BitConverter.GetBytes(targetWormEntity.ai[3]), 0);
                bitMask ^= -1; //reverse the bits
                bottom28Bits &= bitMask;

                targetWormEntity.ai[3] = BitConverter.ToSingle(BitConverter.GetBytes(bottom28Bits | top4Bits), 0);
            }
        }

        static float speed
        {
            get
            {
                return velocity.Length();
            }
            set
            {
                if (velocity == Vector2.Zero)
                {
                    return;
                }

                velocity *= value / velocity.Length();
            }
        }

        static Vector2 velocity
        {
            get
            {
                int intAI = BitConverter.ToInt32(BitConverter.GetBytes(targetWormEntity.ai[3]), 0);

                int bottom14Bits = intAI;
                int bitMask = 0b0000_0000_0000_0000_0011_1111_1111_1111;
                bottom14Bits &= bitMask;

                int middle14Bits = intAI;
                bitMask = 0b0000_1111_1111_1111_1100_0000_0000_0000;
                middle14Bits &= bitMask;

                //shift 2 to the left to make it have 16 bits with the two least significant bits of the mantissa being 0
                float X = (float)BitConverter.ToHalf(BitConverter.GetBytes((short)(bottom14Bits << 2)), 0);
                //shift 14 to the right to align the bottom part of the middle bits with the bottom part of the short and then shift 2 to the left for the same reason as above
                float Y = (float)BitConverter.ToHalf(BitConverter.GetBytes((short)(middle14Bits >> 12)), 0);

                return new Vector2(X, Y);
            }
            set
            {
                Half X = (Half)value.X;
                Half Y = (Half)value.Y;

                int bottom14Bits = BitConverter.ToInt16(BitConverter.GetBytes(X), 0) >> 2;
                int bitMask = 0b0000_0000_0000_0000_0011_1111_1111_1111;
                bottom14Bits &= bitMask;

                int middle14Bits = BitConverter.ToInt16(BitConverter.GetBytes(Y), 0) << 12;
                bitMask = 0b0000_1111_1111_1111_1100_0000_0000_0000;
                middle14Bits &= bitMask;

                int top4Bits = BitConverter.ToInt32(BitConverter.GetBytes(targetWormEntity.ai[3]), 0);
                bitMask = 0b0000_1111_1111_1111_1111_1111_1111_1111 ^ -1; //xor because if the leading bit is 1, it's seen as uint and the compiler complains
                top4Bits &= bitMask;

                targetWormEntity.ai[3] = BitConverter.ToSingle(BitConverter.GetBytes(bottom14Bits | middle14Bits | top4Bits), 0);
            }
        }

        static Vector2 direction
        {
            get
            {
                float rotation = targetWormEntity.rotation - MathF.PI / 2;
                return new Vector2(MathF.Cos(rotation), MathF.Sin(rotation));
            }
            set
            {
                targetWormEntity.rotation = MathF.Atan2(value.Y, value.X) + MathF.PI / 2;
            }
        }

        void SetDirection(NPC wormSegment, Vector2 direction)
        {
            wormSegment.rotation = MathF.Atan2(direction.Y, direction.X) + MathF.PI / 2;
        }

        Vector2 GetDirection(NPC wormSegment)
        {
            float rotation = wormSegment.rotation - MathF.PI / 2;
            return new Vector2(MathF.Cos(rotation), MathF.Sin(rotation));
        }

        public override void SetDefaults(NPC npc)
        {
            base.SetDefaults(npc);

            AttackWarningSound.MaxInstances = 1;
            AttackWarningSound.SoundLimitBehavior = SoundLimitBehavior.IgnoreNew;

            if (WormHeadToBody.Keys.Contains(npc.type) || WormHeadToBody.Values.Contains(npc.type) || WormBodyToTail.Values.Contains(npc.type))
            {
                npc.aiStyle = -1;
            }

            if (npc.type is NPCID.GiantWormHead)
            {
                targetWormEntity = npc;
                segmentCountBehind = 30;
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

            Player player = Main.player[wormEntity.target];
            Vector2 playerCenter = player.Center;
            Vector2 dirToPlayer = player.position - wormEntity.position;
            float playerDistance = MathF.Sqrt(dirToPlayer.X * dirToPlayer.X + dirToPlayer.Y * dirToPlayer.Y);
            dirToPlayer /= playerDistance;

            targetWormEntity = wormEntity;

            switch (wormState)
            {
                case WormState.Idle:
                    //goes in circles
                    wormEntity.rotation -= MathF.PI / 60f;
                    velocity = direction * 6f;

                    //Transition
                    if (playerDistance < 30f * 16f)
                    {
                        wormState = WormState.PreparingAttack;
                    }
                    break;
                case WormState.PreparingAttack:
                    //idles under the player in a figure 8 pattern until it can attack
                    speed = float.Lerp(speed, 10f, 10f / 60f);
                    targetPosition = playerCenter + new Vector2(0, 16f * 40f);
                    Vector2 toTargetPosition = targetPosition - wormEntity.Center;
                    velocity = new Vector2(float.Lerp(velocity.X, toTargetPosition.X, 0.05f / 60f), float.Lerp(velocity.Y, toTargetPosition.Y, 0.5f / 60f));

                    #region worm state transition

                    if (ticksUntilAttack <= 0 && TryGetAttackPosition(out bool isHorizontal, out bool isRight, out Vector2 attackPosition))
                    {
                        wormState = isHorizontal ?
                            (isRight ? WormState.GoingToAttackSpotHorizontalRight : WormState.GoingToAttackSpotHorizontalLeft) :
                            WormState.GoingToAttackSpotVertical;
                        targetPosition = attackPosition;
                        ticksUntilAttack = 120;
                    }

                    #endregion

                    ticksUntilAttack = Math.Max(0, ticksUntilAttack - 1);

                    break;
                case WormState.GoingToAttackSpotHorizontalRight:
                case WormState.GoingToAttackSpotHorizontalLeft:
                case WormState.GoingToAttackSpotVertical:
                    //Goes behind attack spot and then towards it
                    Vector2 attackSpot = targetPosition;
                    Vector2 dirToAttackSpot = attackSpot - wormEntity.Center;
                    dirToAttackSpot.Normalize();

                    #region update velocity

                    int isFirstHalfOfAttackJourneyFactor = ticksUntilAttack > 60 ? 1 : 0;
                    Vector2 targetDir;
                    if (wormState == WormState.GoingToAttackSpotHorizontalRight)
                    {
                        Vector2 dirToPreAttackSpot = attackSpot + new Vector2(40f * 16f, 0) - wormEntity.Center;
                        dirToPreAttackSpot.Normalize();
                        targetDir = ticksUntilAttack < 60 ?
                            dirToAttackSpot :
                            dirToPreAttackSpot;
                    }
                    else if (wormState == WormState.GoingToAttackSpotHorizontalLeft)
                    {
                        Vector2 dirToPreAttackSpot = attackSpot + new Vector2(-40f * 16f, 0) - wormEntity.Center;
                        dirToPreAttackSpot.Normalize();
                        targetDir = ticksUntilAttack < 60 ?
                            dirToAttackSpot :
                            dirToPreAttackSpot;
                    }
                    else
                    {
                        Vector2 dirToPreAttackSpot = attackSpot + new Vector2(0, 40f * 16f) - wormEntity.Center;
                        dirToPreAttackSpot.Normalize();
                        targetDir = ticksUntilAttack < 60 ?
                            dirToAttackSpot :
                            dirToPreAttackSpot;
                    }
                    targetDir.Normalize();
                    speed = 20f;
                    velocity = Vector2.Lerp(velocity, speed * targetDir, 10f / 60f);

                    if (Vector2.Dot(dirToPlayer, GetDirection(wormEntity)) > 0 && ticksUntilAttack < 60) //Only set speed when travelling towards player so that you don't speed up exponentially away from the player
                    {
                        //speed adjusts so that the worm arrives at the attack spot exactly when the attack should start
                        speed = 0.5f * (attackSpot - wormEntity.Center).Length() / MathF.Max(1f, ticksUntilAttack);
                    }

                    #endregion

                    #region worm attack warning

                    if (wormState is WormState.GoingToAttackSpotHorizontalLeft or WormState.GoingToAttackSpotHorizontalRight) //horizontal attack
                    {
                        Dust.NewDustDirect(attackSpot, 0, 0, DustID.Sand, 0, 0).noGravity = true;
                        for (int i = 1; i < 5; i++)
                        {
                            Dust.NewDustDirect(attackSpot + new Vector2(0, 8f) * i, 0, 0, DustID.Sand, 0, 0).noGravity = true;
                            Dust.NewDustDirect(attackSpot + new Vector2(0, -8f) * i, 0, 0, DustID.Sand, 0, 0).noGravity = true;
                        }
                    }
                    else //vertical attacks
                    {
                        Dust.NewDustDirect(attackSpot, 0, 0, DustID.Sand, 0, 0).noGravity = true;
                        for (int i = 1; i < 5; i++)
                        {
                            Dust.NewDustDirect(attackSpot + new Vector2(8f, 0) * i, 0, 0, DustID.Sand, 0, 0).noGravity = true;
                            Dust.NewDustDirect(attackSpot + new Vector2(-8f, 0) * i, 0, 0, DustID.Sand, 0, 0).noGravity = true;
                        }
                    }

                    AttackWarningSound.MaxInstances = 1;
                    AttackWarningSound.SoundLimitBehavior = SoundLimitBehavior.IgnoreNew;
                    SoundEngine.PlaySound(in AttackWarningSound, attackSpot);

                    #endregion

                    #region worm state transition

                    if (ticksUntilAttack <= 0)
                    {
                        switch (wormState)
                        {
                            case WormState.GoingToAttackSpotHorizontalRight:
                                velocity = new Vector2(-1, 0);
                                break;
                            case WormState.GoingToAttackSpotHorizontalLeft:
                                velocity = new Vector2(1, 0);
                                break;
                            case WormState.GoingToAttackSpotVertical:
                                velocity = new Vector2(0, -1);
                                break;
                        }
                        wormState = WormState.Attacking;
                        speed = AttackSpeed * 16f / 60f;
                    }

                    #endregion

                    ticksUntilAttack = Math.Max(0, ticksUntilAttack - 1);
                    break;
                case WormState.Attacking:
                    //Falls down and slightly stears towards player until beneath and far enough
                    velocity = Vector2.Lerp(velocity, 15f * (dirToPlayer - new Vector2(0, dirToPlayer.Y)) / dirToPlayer.Length(), 0.5f / 60f);
                    velocity = new Vector2(velocity.X, velocity.Y + 10f / 60f);

                    #region worm state transition

                    if (Vector2.Dot(direction, dirToPlayer) < 0 &&
                        (player.position - wormEntity.position).Length() > 16f * AttackEndDistance &&
                        player.position.Y < wormEntity.position.Y)
                    {
                        wormState = WormState.PreparingAttack;
                        ticksUntilAttack = AttackCooldownTicks;
                    }

                    #endregion

                    break;
            }

            direction = velocity;
            wormEntity.Center += velocity;
        }

        bool TryGetAttackPosition(out bool isHorizontal, out bool isRight, out Vector2 attackPosition)
        {
            Vector2 startPosition = Main.player[targetWormEntity.target].Center;

            if (Main.rand.NextBool())
            {
                #region horizontal attacks

                isRight = Main.rand.NextBool();

                for (int i = 1; i <= MaxHorizontalAttackDistance; i++)
                {
                    Vector2 leftTargetPosition = startPosition + new Vector2(-i * 16f, 0);
                    Vector2 rightTargetPosition = startPosition + new Vector2(i * 16f, 0);

                    if (!isRight)
                    {

                        if (Collision.IsWorldPointSolid(leftTargetPosition) && !Collision.IsWorldPointSolid(leftTargetPosition + new Vector2(16f, 0f)) &&
                            Collision.IsWorldPointSolid(leftTargetPosition + new Vector2(0, 16f)) && !Collision.IsWorldPointSolid(leftTargetPosition + new Vector2(16f, 16f)) &&
                            Collision.IsWorldPointSolid(leftTargetPosition + new Vector2(0, -16f)) && !Collision.IsWorldPointSolid(leftTargetPosition + new Vector2(16f, -16f)))
                        {
                            isHorizontal = true;
                            attackPosition = leftTargetPosition;
                            isRight = false;
                            return true;
                        }
                    }
                    else
                    {
                        if (Collision.IsWorldPointSolid(rightTargetPosition) && !Collision.IsWorldPointSolid(rightTargetPosition + new Vector2(-16f, 0f)) &&
                            Collision.IsWorldPointSolid(rightTargetPosition + new Vector2(0, 16f)) && !Collision.IsWorldPointSolid(rightTargetPosition + new Vector2(-16f, 16f)) &&
                            Collision.IsWorldPointSolid(rightTargetPosition + new Vector2(0, -16f)) && !Collision.IsWorldPointSolid(rightTargetPosition + new Vector2(-16f, -16f)))
                        {
                            isHorizontal = true;
                            attackPosition = rightTargetPosition;
                            isRight = true;
                            return true;
                        }
                    }
                }

                #endregion
            }
            else
            {
                #region vertical attacks

                for (int i = 0; i <= 5; i++)
                {
                    Vector2 leftStartPosition = startPosition + new Vector2(-i * 16f, 0);
                    Vector2 rightStartPosition = startPosition + new Vector2(i * 16f, 0);

                    for (int j = 1; j <= MaxVerticalAttackDistance; j++)
                    {
                        Vector2 leftTargetPosition = leftStartPosition + new Vector2(0, j * 16f);
                        Vector2 rightTargetPosition = rightStartPosition + new Vector2(0, j * 16f);

                        if (Collision.IsWorldPointSolid(leftTargetPosition) && !Collision.IsWorldPointSolid(leftTargetPosition + new Vector2(0, -16f)) &&
                            Collision.IsWorldPointSolid(leftTargetPosition + new Vector2(16f, 0)) && !Collision.IsWorldPointSolid(leftTargetPosition + new Vector2(16f, -16f)) &&
                            Collision.IsWorldPointSolid(leftTargetPosition + new Vector2(-16f, 0)) && !Collision.IsWorldPointSolid(leftTargetPosition + new Vector2(-16f, -16f)))
                        {
                            isHorizontal = false;
                            attackPosition = leftTargetPosition;
                            isRight = false;
                            return true;
                        }

                        if (Collision.IsWorldPointSolid(rightTargetPosition) && !Collision.IsWorldPointSolid(rightTargetPosition + new Vector2(0, -16f)) &&
                            Collision.IsWorldPointSolid(rightTargetPosition + new Vector2(16f, 0)) && !Collision.IsWorldPointSolid(rightTargetPosition + new Vector2(16f, -16f)) &&
                            Collision.IsWorldPointSolid(rightTargetPosition + new Vector2(-16f, 0)) && !Collision.IsWorldPointSolid(rightTargetPosition + new Vector2(-16f, -16f)))
                        {
                            isHorizontal = false;
                            attackPosition = rightTargetPosition;
                            isRight = false;
                            return true;
                        }
                    }
                }

                #endregion
            }

            isHorizontal = false;
            attackPosition = Vector2.Zero;
            isRight = false;
            return false;
        }

        public void WormSegmentAndTailAI(NPC wormEntity)
        {
            NPC targetWormEntity = Worms.targetWormEntity;
            Worms.targetWormEntity = wormEntity;
            NPC segmentAhead = Main.npc[aheadSegmentIndex];
            Worms.targetWormEntity = targetWormEntity;

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
            NPC ogTargetWormEntity = Worms.targetWormEntity;
            Worms.targetWormEntity = wormEntity;
            if (behindSegmentIndex > 0 || segmentCountBehind <= -1)
            {
                return;
            }

            if (wormEntity.type is NPCID.GiantWormHead)
            {
                direction = new Vector2(0, 1f);
                segmentCountBehind = 30;
            }

            int spawnedSegmentType;

            if (segmentCountBehind == 0)
            {
                spawnedSegmentType = NPCID.GiantWormTail;
            }
            else
            {
                spawnedSegmentType = NPCID.GiantWormBody;
            }

            int spawnedSegmentIndex = NPC.NewNPC(new EntitySource_SpawnNPC(), (int)wormEntity.Center.X, (int)wormEntity.Center.Y + wormEntity.height / 2, spawnedSegmentType);
            behindSegmentIndex = spawnedSegmentIndex;
            int thisSegmentCountBehind = segmentCountBehind;

            NPC spawnedNPC = Main.npc[spawnedSegmentIndex];
            Worms.targetWormEntity = spawnedNPC;

            spawnedNPC.realLife = wormEntity.realLife;
            aheadSegmentIndex = wormEntity.whoAmI;
            segmentCountBehind = thisSegmentCountBehind - 1;
            spawnedNPC.GetGlobalNPC<CombatNPC>().allowContactDamage = false;

            Worms.targetWormEntity = ogTargetWormEntity;
        }
    }
}
