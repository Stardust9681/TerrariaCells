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
using TerrariaCells.Common.GlobalItems;
using TerrariaCells.Common.GlobalProjectiles;
using TerrariaCells.Common.ModPlayers;
using TerrariaCells.Common.Utilities;
using TerrariaCells.Content.Items;
using TerrariaCells.Content.Projectiles.HeldProjectiles;

namespace TerrariaCells.Content.WeaponAnimations
{
    public class Bow : GlobalItem
    {
        //sniper is here because i was gonna give it the same animation anyways
        public static int[] Bows = { ItemID.WoodenBow, ItemID.AshWoodBow, ItemID.BorealWoodBow, ItemID.PalmWoodBow, ItemID.ShadewoodBow, ItemID.EbonwoodBow, ItemID.PearlwoodBow, ItemID.RichMahoganyBow,
            ItemID.CopperBow, ItemID.TinBow, ItemID.LeadBow, ItemID.IronBow, ItemID.SilverBow, ItemID.TungstenBow, ItemID.GoldBow, ItemID.PlatinumBow,
            ItemID.DemonBow, ItemID.TendonBow, ItemID.MoltenFury, ItemID.BeesKnees, ItemID.HellwingBow, ItemID.BloodRainBow, 
            ItemID.DD2BetsyBow, ItemID.DaedalusStormbow, ItemID.IceBow, ItemID.Marrow, ItemID.Phantasm, ItemID.PulseBow, ItemID.ShadowFlameBow, ItemID.Tsunami, ModContent.ItemType<PhantomPhoenix>()};
        public int Charge = 0;
        public SoundStyle? StoredSound;
        public int ForcedProjectile = ProjectileID.WoodenArrowFriendly;
        public int ChargedProjectile = ProjectileID.WoodenArrowFriendly;
        public int TimeToCharge = 60;
        public override bool InstancePerEntity => true;
        public override void SetStaticDefaults()
        {
            for (int i = 0; i < Bows.Length; i++) {
                ItemID.Sets.ItemsThatAllowRepeatedRightClick[Bows[i]] = true;
            }
            
            base.SetStaticDefaults();
        }
        public override void SetDefaults(Item item)
        {
            item.autoReuse = true;
            StoredSound = item.UseSound;
            item.UseSound = null;
            if (item.type == ItemID.PulseBow)
            {
                ChargedProjectile = ProjectileID.PulseBolt;
            }
            if (item.type == ItemID.IceBow)
            {
                ChargedProjectile = ProjectileID.FrostArrow;
            }
            if (item.type == ItemID.MoltenFury)
            {
                ForcedProjectile = ProjectileID.FireArrow;
                ChargedProjectile = ProjectileID.HellfireArrow;
            }
            if (item.type == ItemID.Marrow)
            {
                ForcedProjectile = ProjectileID.BoneArrow;
            }
            if (item.type == ModContent.ItemType<PhantomPhoenix>())
            {
                ForcedProjectile = ProjectileID.FireArrow;
                ChargedProjectile = ProjectileID.DD2PhoenixBowShot;
            }
            if (item.type == ItemID.BeesKnees)
            {
                ChargedProjectile = ProjectileID.BeeArrow;
            }
            if (item.type == ItemID.HellwingBow)
            {
                ForcedProjectile = ProjectileID.Hellwing;
                ChargedProjectile = ProjectileID.Hellwing;
            }
            base.SetDefaults(item);
        }
        public override bool AppliesToEntity(Item entity, bool lateInstantiation)
        {
            return Bows.Contains(entity.type);
        }
        public override bool AltFunctionUse(Item item, Player player)
        {
            return true;
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
            player.itemLocation = player.Center + new Vector2(10 * player.direction, player.gfxOffY).RotatedBy(player.itemRotation);
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
                if (Charge < TimeToCharge)
                {
                    if (Charge == TimeToCharge - 1)
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
                    SoundEngine.PlaySound(item.UseSound, player.Center);
                    MethodInfo PlayerItemCheck_Shoot = typeof(Player).GetMethod("ItemCheck_Shoot", BindingFlags.NonPublic | BindingFlags.Instance);
                    PlayerItemCheck_Shoot.Invoke(player, [player.whoAmI, item, item.damage]);
                }
            }else if (player.itemTime == player.itemTimeMax - 1 && StoredSound != null)
            {
                SoundEngine.PlaySound(StoredSound, player.Center);
            }
            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, player.itemRotation + (player.direction == -1 ? MathHelper.Pi/2 : -MathHelper.Pi/2));
            
        }
        public override void ModifyShootStats(Item item, Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            
            type = ForcedProjectile;
            damage = (int)TCellsUtils.LerpFloat(damage * 1, damage * 1.4f, Charge, (float)TimeToCharge, TCellsUtils.LerpEasing.InCubic);
            if (Charge >= TimeToCharge)
            {
                type = ChargedProjectile;
                velocity *= 1.5f;
            }
            base.ModifyShootStats(item, player, ref position, ref velocity, ref type, ref damage, ref knockback);
        }
        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                return false;
            }
            Projectile proj = Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, player.whoAmI);
            if (Charge >= TimeToCharge)
            {
                proj.GetGlobalProjectile<VanillaReworksGlobalProjectile>().ForceCrit = true;
            }
            return false;
        }
    }
}
