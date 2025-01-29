using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    public class Bow : GlobalItem
    {
        //sniper is here because i was gonna give it the same animation anyways
        public static int[] Bows = { ItemID.WoodenBow, ItemID.AshWoodBow, ItemID.BorealWoodBow, ItemID.PalmWoodBow, ItemID.ShadewoodBow, ItemID.EbonwoodBow, ItemID.PearlwoodBow, ItemID.RichMahoganyBow,
            ItemID.CopperBow, ItemID.TinBow, ItemID.LeadBow, ItemID.IronBow, ItemID.SilverBow, ItemID.TungstenBow, ItemID.GoldBow, ItemID.PlatinumBow,
            ItemID.DemonBow, ItemID.TendonBow, ItemID.MoltenFury, ItemID.BeesKnees, ItemID.HellwingBow, ItemID.BloodRainBow,
            ItemID.DD2BetsyBow, ItemID.DaedalusStormbow, ItemID.IceBow, ItemID.Marrow, ItemID.Phantasm, ItemID.PulseBow, ItemID.ShadowFlameBow, ItemID.Tsunami};
        public int Charge = 0;
        public override bool InstancePerEntity => true;
        public override bool AppliesToEntity(Item entity, bool lateInstantiation)
        {
            return Bows.Contains(entity.type);
        }
        public override bool AltFunctionUse(Item item, Player player)
        {
            return true;
            return base.AltFunctionUse(item, player);
        }
        public override bool CanUseItem(Item item, Player player)
        {
            
            return base.CanUseItem(item, player);
        }
        public override void UpdateInventory(Item item, Player player)
        {
            
            if (Charge > 0 && (player.altFunctionUse != 2 || player.HeldItem != item))
            {
                
                Charge = 0;
            }
        }
        public override GlobalItem Clone(Item from, Item to)
        {
            to.GetGlobalItem(this).Charge = from.GetGlobalItem(this).Charge;
            return to.GetGlobalItem(this);
        }
        public override void UseStyle(Item item, Player player, Rectangle heldItemFrame)
        {
            int animationTime = player.itemAnimationMax - player.itemAnimation;
            player.itemLocation = player.Center + new Vector2(10 * player.direction, 0).RotatedBy(player.itemRotation);
            //Main.NewText(player.altFunctionUse);
            if (player.altFunctionUse == 2)
            {
                //Main.NewText(Charge);
                player.itemRotation = player.AngleTo(Main.MouseWorld);
                if (Main.MouseWorld.X > player.Center.X)
                {
                    player.direction = 1;
                }
                else
                {
                    player.direction = -1;
                    player.itemRotation += MathHelper.Pi;
                }
                if (Charge < item.useTime * 2)
                {
                    if (Charge == item.useTime * 2 - 1)
                    {
                        SoundEngine.PlaySound(SoundID.MaxMana, player.Center);
                        for (int i = 0; i < 10; i++)
                        {
                            Dust d = Dust.NewDustPerfect(player.itemLocation , DustID.FireworkFountain_Green);
                            d.noGravity = true;
                            Vector2 rotAdd = new Vector2(0, 1).RotatedBy(MathHelper.ToRadians(360f / 10 * i));
                            rotAdd.Y *= 2;
                            d.velocity = (Main.MouseWorld - player.itemLocation).SafeNormalize(Vector2.Zero) * 2 + rotAdd.RotatedBy(player.itemRotation);
                        }
                    }
                    Charge++;
                   
                }
                player.itemTime = player.itemTimeMax;
                player.itemAnimation = player.itemAnimationMax;
                if (!player.controlUseTile)
                {
                    player.altFunctionUse = 0;
                    int originalDamage = item.damage;
                    float originalShootSpeed = item.shootSpeed;
                    item.damage *= (int)(1+(Charge / (item.useTime * 2f)));
                    if (Charge >= item.useTime * 2)
                        item.shootSpeed *= 1.5f;
                    SoundEngine.PlaySound(item.UseSound, player.Center);
                    MethodInfo PlayerItemCheck_Shoot = typeof(Player).GetMethod("ItemCheck_Shoot", BindingFlags.NonPublic | BindingFlags.Instance);
                    PlayerItemCheck_Shoot.Invoke(player, [player.whoAmI, item, item.damage]);
                    item.damage = originalDamage;
                    item.shootSpeed = originalShootSpeed;
                }
            }
            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, player.itemRotation + (player.direction == -1 ? MathHelper.Pi/2 : -MathHelper.Pi/2));
            
        }
        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                return false;
            }
            
            
            return base.Shoot(item, player, source, position, velocity, type, damage, knockback);
        }
    }
}
