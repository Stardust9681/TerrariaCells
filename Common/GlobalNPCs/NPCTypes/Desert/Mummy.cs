using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaCells.Common.Utilities;
using TerrariaCells.Content.Projectiles;

namespace TerrariaCells.Common.GlobalNPCs.NPCTypes.Shared
{
    public partial class Fighters
    {
        int[] Mummies = { NPCID.Mummy, NPCID.DarkMummy, NPCID.BloodMummy, NPCID.LightMummy };
        public bool DrawMummy(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Asset<Texture2D> t = TextureAssets.Npc[npc.type];
            Asset<Texture2D> slam = ModContent.Request<Texture2D>("TerrariaCells/Common/Assets/MummySlam");

            if (npc.ai[3] == 0)
            {
                spriteBatch.Draw(t.Value, npc.Center - screenPos + new Vector2(0, npc.height / 2 + 5), new Rectangle(npc.frame.X, npc.frame.Y, npc.frame.Width, npc.frame.Height), drawColor, npc.rotation, new Vector2(t.Width() / 2, t.Height() / Main.npcFrameCount[npc.type]), new Vector2(npc.scale * 1.1f, npc.scale), npc.direction == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0);
            }
            else if (npc.ai[3] == 1)
            {
                spriteBatch.Draw(slam.Value, npc.Center - screenPos + new Vector2(-2, npc.height + 14), new Rectangle(0, (int)CustomFrameY*54, 44, 52), drawColor, npc.rotation, new Vector2(t.Width() / 2, t.Height() / 12), new Vector2(npc.scale * 1.1f, npc.scale), npc.direction == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0);
            }

            return false;
        }
        public void MummyFrame(NPC npc)
        {
            int slamTime = 100;
            if (npc.ai[3] == 1)
            {
                CustomFrameCounter++;
                if (CustomFrameCounter > slamTime/11)
                {
                    CustomFrameCounter = 0;
                    CustomFrameY++;
                    if (CustomFrameY >= 11)
                    {
                        CustomFrameY = 0;
                    }
                }
            }
            else
            {
                npc.localAI[0] = 0;
                npc.localAI[1] = 0;
            }
        }
        public void MummyAI(NPC npc, Player? target)
        {
            const int SlamCooldown = 50;
            const int SlamTime = 100;

			npc.ai[2]++;
			CombatNPC.ToggleContactDamage(npc, false);

			bool validTarget;
			if (target != null)
			{
				validTarget = npc.TargetInAggroRange(target, 240);
				if (MathF.Abs(target.position.X - npc.position.X) < 80)
					ShouldWalk = false;
                if (MathF.Abs(target.position.X - npc.position.X) > 240)
                    ShouldWalk = true;
			}
			else
			{
				validTarget = npc.TargetInAggroRange(240);
				ShouldWalk = false;
			}

			if (validTarget && npc.ai[2] >= SlamCooldown && npc.ai[3] == 0 && npc.direction == MathF.Sign(target.position.X - npc.position.X))
			{
                npc.ai[2] = 0;
                npc.ai[3] = 1;
                CustomFrameCounter = 0;
                CustomFrameY = 0;
            }
            if (npc.ai[2] >= SlamTime && npc.ai[3] == 1)
            {
                npc.ai[2] = 0;
                npc.ai[3] = 0;
            }
            if (npc.ai[3] == 1)
            {
				npc.direction = npc.oldDirection;
                ShouldWalk = false;
                npc.velocity.X *= 0.9f;
                if (npc.ai[2] == 70)
                {
                    Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center + new Vector2(40 * npc.direction, 0), Vector2.Zero, ModContent.ProjectileType<MummyShockwave>(), TCellsUtils.ScaledHostileDamage(npc.damage), 1, -1, npc.direction);
                    SoundEngine.PlaySound(SoundID.Item14, npc.Center);
                }
            }
        }
    }
}
