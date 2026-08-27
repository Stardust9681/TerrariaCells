using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using TerrariaCells.Common.Utilities;
using TerrariaCells.Common.GlobalNPCs;

namespace TerrariaCells.Content.NPCs;

public class MageHornet : ModNPC, OnAnyPlayerHit.INPC
{
    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[NPC.type] = 3;
    }
    public override void SetDefaults()
    {
        NPC.lifeMax = 200;
        NPC.damage = 50;
        NPC.knockBackResist = 0.8f;
        NPC.noGravity = true;
        NPC.width = 50;
        NPC.height = 36;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.noTileCollide = true;
    }

    private const int Idle = 0;
    private const int Move = 1;
    private const int Charge_Release = 2;
    public override void AI()
    {
        int oldAI1 = (int)NPC.ai[1];
        switch(oldAI1)
        {
            case Idle:
                IdleAI();
                break;
            case Move:
                MoveAI();
                break;
            case Charge_Release:
                AttackAI();
                break;
        }
        if((int)NPC.ai[1] != oldAI1)
            NPC.netUpdate = true;
    }
    
    private void IdleAI()
    {
        CombatNPC.ToggleContactDamage(NPC, false);
        int[] followNPCTypes = [ NPCID.Hornet, ModContent.NPCType<NPCs.SWATHornet>() ];
        
        if(NPC.ai[0] == 0 || !Main.npc[(int)NPC.ai[0]].active)
        {
            foreach(NPC npc in Main.ActiveNPCs)
            {
                if(followNPCTypes.Contains(npc.type) && Collision.CanHitLine(NPC.Center, 8, 8, npc.Center, 8, 8))
                {
                    NPC.ai[0] = npc.whoAmI + 1; //+1 since ai[0] starts at 0, which is a valid NPC index
                    NPC.ai[2] = 0;
                    NPC.ai[3] = 0;
                    break;
                }    
            }
        }
        
        Vector2 movePos;
        if(NPC.ai[0] == 0)
        {
            movePos = new Vector2(NPC.ai[2], NPC.ai[3]);

            if (movePos.X == 0 || movePos.Y == 0)
            {
                if (Main.netMode == NetmodeID.MultiplayerClient)
                    return;

                for (int i = 0; i < 15; i++)
                {
                    movePos = NPC.position;
                    Vector2 direction = Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi);

                    int iterations = 0;
                    while (Collision.CanHitLine(NPC.position, NPC.width, NPC.height, movePos, NPC.width, NPC.height) && iterations < 8)
                    {
                        movePos += direction * 16;
                        iterations++;
                    }
                    movePos -= direction * 16;
                    if (iterations > 3)
                        break;
                }
                NPC.ai[2] = movePos.X;
                NPC.ai[3] = movePos.Y;
                NPC.netUpdate = true;
                return;
            }
        }
        else
        {
            movePos = Main.npc[(int)NPC.ai[0]].Center;
            movePos += (NPC.Center - movePos).SafeNormalize(-Vector2.UnitY) * 24f;
        }

        if (movePos.X - NPC.position.X != 0)
            NPC.velocity.X += MathF.Sign(movePos.X - NPC.position.X) * MathF.Sqrt(MathF.Abs(movePos.X - NPC.position.X)) * 0.005f;

        NPC.velocity.Y += (movePos.Y < NPC.position.Y ? -1 : 1) * 0.024f;

        Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);

        NPC.rotation = MathHelper.ToRadians(NPC.velocity.X) * 4.5f;
        NPC.direction = MathF.Sign(NPC.velocity.X);
        NPC.spriteDirection = NPC.direction;
        
        NPC.TargetClosest(false);
        if(NPC.TargetInAggroRange())
        {
            NPC.ai[0] = Main.rand.NextFloat(MathHelper.TwoPi);
            NPC.ai[1] = Move;
            NPC.ai[2] = 0;
            NPC.ai[3] = 0;
            return;
        }
    }
    
    private void MoveAI()
    {
        CombatNPC.ToggleContactDamage(NPC, false);
        if(!NPC.TryGetTarget(out Entity target))
        {
            NPC.ai[0] = 0;
            NPC.ai[1] = Idle;
            NPC.ai[2] = 0;
            NPC.ai[3] = 0;
            return;
        }
        
        Vector2 movePos;
        do
        {
            movePos = target.position + Vector2.UnitX.RotatedBy(NPC.ai[0]) * (12 * 16);
            NPC.ai[0]++;
        }
        while(!Collision.CanHitLine(movePos, NPC.width, NPC.height, target.position, target.width, target.height));
        NPC.ai[0]--;

        if (movePos.X - NPC.position.X != 0 && MathF.Abs(NPC.velocity.X) < 4f)
            NPC.velocity.X += MathF.Sign(movePos.X - NPC.position.X) * MathF.Sqrt(MathF.Abs(movePos.X - NPC.position.X)) * 0.045f;

        NPC.velocity.Y += (movePos.Y < NPC.position.Y ? -1 : 1) * 0.06f;

        if (MathF.Sign(movePos.X - NPC.position.X) != MathF.Sign(NPC.velocity.X))
        {
            NPC.velocity.X *= 0.95f;
        }
        if (MathF.Sign(movePos.Y - NPC.position.Y) != MathF.Sign(NPC.velocity.Y))
        {
            NPC.velocity.Y *= 0.95f;
        }

        Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);

        NPC.rotation = MathHelper.ToRadians(NPC.velocity.X) * 4.5f;
        NPC.direction = MathF.Sign(target.position.X - NPC.position.X);
        NPC.spriteDirection = NPC.direction;
        
        if(MathF.Abs(NPC.position.X - movePos.X) < 64 && MathF.Abs(NPC.position.Y - movePos.Y) < 64)
        {
            NPC.ai[2]++;
            if(NPC.ai[2] > 40)
            {
                NPC.ai[0] = 0;
                NPC.ai[1] = Charge_Release;
                NPC.ai[2] = 0;
                NPC.ai[3] = 0;
                return;
            }
        }
        else if (NPC.ai[2] > 10)
            NPC.ai[2]--;
    }


    private const int ChargeUpTime = 90;
    private void AttackAI()
    {
        CombatNPC.ToggleContactDamage(NPC, false);
        if (!NPC.TryGetTarget(out Entity target))
            target = NPC;
        
        NPC.direction = MathF.Sign(target.position.X - NPC.position.X);
        NPC.spriteDirection = NPC.direction;
        NPC.rotation = MathHelper.ToRadians(NPC.velocity.X) * 6f;

        NPC.ai[0]++;
        
        if(NPC.ai[0] > ChargeUpTime * 1.5f)
        {
            NPC.ai[0] = Main.rand.NextFloat(MathHelper.TwoPi);
            NPC.ai[1] = Move;
            NPC.ai[2] = 0;
            NPC.ai[3] = 0;
            return;
        }
        else if(NPC.ai[0] == ChargeUpTime)
        {
            NPC.velocity *= 0.8f;
            if(Main.netMode != NetmodeID.MultiplayerClient)
                Projectile.NewProjectile(NPC.GetSource_FromAI(), target.Center, Vector2.Zero, ModContent.ProjectileType<Projectiles.MageHornetLightning>(), NPC.damage*2/3, 1, Main.myPlayer);
            NPC.DoAttackWarning();
        }
        else
        {
            NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, MathF.Sin(MathHelper.ToRadians(NPC.ai[0] * 9)) * 2f, 0.25f);
        }
        NPC.velocity.Y *= 0.8f;
    }

    public override void FindFrame(int frameHeight)
    {
        this.NPC.frame.Width = 64;
        
        this.NPC.frameCounter++;
        if(this.NPC.frameCounter > 4)
        {
            int frameNumY = this.NPC.frame.Y / frameHeight;
            frameNumY++;
            if(frameNumY > 2)
                frameNumY = 0;
            this.NPC.frame.Y = frameNumY * frameHeight;
            
            if(NPC.ai[1] == Move)
            {
                int frameNumX = this.NPC.frame.X / this.NPC.frame.Width;
                if(Main.rand.NextBool())
                {
                    frameNumX++;
                }
                if(frameNumX > 1)
                    frameNumX = 0;
                this.NPC.frame.X = frameNumX * this.NPC.frame.Width;
            }
            
            NPC.frameCounter = 0;
        }
        
        //0-6
        //6-8
        //8-13
        if(NPC.ai[1] == Charge_Release)
        {
            if(NPC.ai[0] < ChargeUpTime - 15)
            {
                float progress = NPC.ai[0]/(ChargeUpTime - 15f);
                progress *= 6;
                NPC.frame.X = (int)progress * NPC.frame.Width;
            }
            else if(NPC.ai[0] < ChargeUpTime)
            {
                int frameNumX = 6 + (int)MathF.Round(MathHelper.Lerp(0, 2, 1-Utils.PingPongFrom01To010((NPC.ai[0]-ChargeUpTime+15)/15f)));
                frameNumX = (int)MathHelper.Clamp(frameNumX, 6, 8);
                NPC.frame.X = frameNumX * NPC.frame.Width;
            }
            else
            {
                int frameNumX = 8 + (int)(5 * (NPC.ai[0]-ChargeUpTime)/(ChargeUpTime*0.5f));
                NPC.frame.X = frameNumX * NPC.frame.Width;
            }
        }
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        //Sprites in sheet are backwards :x
        Texture2D tex = TextureAssets.Npc[NPC.type].Value;
        spriteBatch.Draw(tex, NPC.Center + new Vector2(0, NPC.gfxOffY) - screenPos, NPC.frame, drawColor, NPC.rotation, NPC.frame.Size()*0.5f, NPC.scale, NPC.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
        return false;
    }

    public void OnAnyPlayerHit(Player attacker, NPC.HitInfo hit, int damage)
    {
        if(hit.DamageType.CountsAsClass(DamageClass.Melee))
        {
            if(NPC.ai[1] == Charge_Release)
            {
                if(NPC.ai[0] > ChargeUpTime)
                    NPC.ai[0] = ChargeUpTime;
                else
                    NPC.ai[0] = MathF.Max(NPC.ai[0] - 10, ChargeUpTime*0.5f);

                NPC.ai[3]++;
                if(NPC.ai[3] > 2)
                {
                    NPC.ai[0] = 0;
                    NPC.ai[1] = Move;
                    NPC.ai[2] = 0;
                    NPC.ai[3] = 0;
                    NPC.netUpdate = true;
                    return;
                }
            }
        }
    }
}