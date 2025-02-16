using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaCells.Common.ModPlayers;
using TerrariaCells.Common.Utilities;
using TerrariaCells.Content.Projectiles.HeldProjectiles;

namespace TerrariaCells.Content.WeaponAnimations
{
    public class MagicWand : GlobalItem
    {
        
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Item entity, bool lateInstantiation)
        {
            //TODO: add not a magic book
            return entity.DamageType == DamageClass.Magic && entity.useStyle == ItemUseStyleID.Shoot && !MagicBook.MagicBooks.Contains(entity.type);
        }


        public override void UseStyle(Item item, Player player, Rectangle heldItemFrame)
        {
            WeaponPlayer mplayer = player.GetModPlayer<WeaponPlayer>();
            //player.itemAnimation but starting at 0
            int animationTime = player.itemAnimationMax - player.itemAnimation;

            player.direction = -1;
            if (Main.MouseWorld.X >= player.Center.X)
            {
                player.direction = 1;
            }
            player.itemRotation = player.AngleTo(Main.MouseWorld);
            if (player.direction == -1)
            {
                player.itemRotation += MathHelper.Pi;
            }
            if (player.itemTime > player.itemTimeMax - 10 && player.reuseDelay == item.reuseDelay)
            {
                player.itemLocation += TCellsUtils.LerpVector2(
                    Vector2.Zero,
                    (player.AngleTo(Main.MouseWorld) + MathHelper.Pi).ToRotationVector2() * 5,
                    player.itemTimeMax - player.itemTime,
                    10,
                    TCellsUtils.LerpEasing.DownParabola
                );
            }
            //arm position
            player.SetCompositeArmFront(
                true,
                Player.CompositeArmStretchAmount.Full,
                player.itemRotation
                    - (MathHelper.PiOver2 ) * player.direction
            );
        }
    }
}
