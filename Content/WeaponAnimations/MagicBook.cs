using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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
    public class MagicBook : GlobalItem
    {
        public static int[] MagicBooks = {
            ItemID.WaterBolt,
            ItemID.BookofSkulls,
            ItemID.CrystalStorm,
            ItemID.CursedFlames,
            ItemID.GoldenShower,
            ItemID.DemonScythe,
            ItemID.MagnetSphere,
            ItemID.RazorbladeTyphoon,
            ItemID.LunarFlareBook
        };
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Item entity, bool lateInstantiation)
        {
            //TODO: add not a magic book
            return MagicBooks.Contains(entity.type);
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
            player.itemRotation = 0;
            if (player.direction == -1)
            {
                player.itemRotation += MathHelper.Pi;
            }
            Vector2 offset1 = new Vector2(2, -20);
            player.itemLocation = player.Center + offset1;
            //arm position
            player.SetCompositeArmFront(
                true,
                Player.CompositeArmStretchAmount.Full,
                player.Center.AngleTo(Main.MouseWorld)
                    - (MathHelper.PiOver2) 
            );
            player.SetCompositeArmBack(true,
                Player.CompositeArmStretchAmount.Full,
                player.itemRotation
                    - (MathHelper.PiOver2) + MathHelper.ToRadians(20) * player.direction);
        }
    }
}
