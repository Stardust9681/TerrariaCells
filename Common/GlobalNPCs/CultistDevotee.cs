using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaCells.Common.Utilities;
using TerrariaCells.Content.Projectiles;

namespace TerrariaCells.Common.GlobalNPCs
{
    public partial class Casters : GlobalNPC
    {
        public bool DrawCultistDevotee(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Asset<Texture2D> t = TextureAssets.Npc[npc.type];
            spriteBatch.Draw(t.Value, npc.Center - screenPos + new Vector2(0, -3), new Rectangle(npc.frame.X, CustomFrameY, npc.frame.Width, npc.frame.Height), drawColor, npc.rotation, new Vector2(t.Width() / 2, t.Height() / Main.npcFrameCount[npc.type] / 2), new Vector2(npc.scale, npc.scale), npc.direction == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0);
            return false;
        }

        public void CultistDevoteeFrame(NPC npc)
        {
            if (npc.ai[0] <= 20)
            {
                CustomFrameY = (int)TCellsUtils.LerpFloat(0, 3, npc.ai[0], 20, TCellsUtils.LerpEasing.Linear) * 54;
            }
            if (npc.ai[0] >= 75)
            {
                CustomFrameY = (int)TCellsUtils.LerpFloat(3, 0, npc.ai[0], 10, TCellsUtils.LerpEasing.Linear, 75) * 54;
            }
            if (npc.ai[0] <= 0)
            {
                CustomFrameY = 54;
            }
        }
        
        public bool CultistDevoteeAI(NPC npc, Player target)
        {
			bool validTarget = npc.TargetInAggroRange(target, 400, false);

            if (target != null)
            {
                npc.direction = npc.Center.X > target.Center.X ? -1 : 1;
                npc.spriteDirection = npc.direction;
            }

            if (npc.ai[0] > 0 || validTarget)
            {
                npc.ai[0]++;
                if (npc.ai[0] == 20 && npc.HasValidTarget)
                {
                    Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center - new Vector2(0, 60), Vector2.Zero, ModContent.ProjectileType<IceBall>(), TCellsUtils.ScaledHostileDamage(50), 1, Main.myPlayer, 0, target.whoAmI, 70);
                }
                if (npc.ai[0] >= 90)
                {
                    npc.ai[0] = -50;
                }
            }

            return false;
        }
    }
}