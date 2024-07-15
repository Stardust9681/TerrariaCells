using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.LootSimulation;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaCells.Content.Projectiles.HeldProjectiles;

namespace TerrariaCells.Common.GlobalItems
{
    public class WeaponHoldoutify : GlobalItem
    {
        public override bool InstancePerEntity => true;
        //used by all
        public bool vanillaShoot = false;
        //used by guns
        public int EmpoweredAmmo;
        public int MaxAmmo;
        public int Ammo;

        //fixing a sound anamoly
        public SoundStyle? StoreSound;
 
        public static int[] Shotguns = { ItemID.Boomstick, ItemID.OnyxBlaster, ItemID.Shotgun, ItemID.TacticalShotgun, ItemID.QuadBarrelShotgun, ItemID.Xenopopper};
        public static int[] Bows = { ItemID.WoodenBow, ItemID.AshWoodBow, ItemID.BorealWoodBow, ItemID.PalmWoodBow, ItemID.ShadewoodBow, ItemID.EbonwoodBow, ItemID.PearlwoodBow, ItemID.RichMahoganyBow,
            ItemID.CopperBow, ItemID.TinBow, ItemID.LeadBow, ItemID.IronBow, ItemID.SilverBow, ItemID.TungstenBow, ItemID.GoldBow, ItemID.PlatinumBow,
            ItemID.DemonBow, ItemID.TendonBow, ItemID.MoltenFury, ItemID.BeesKnees, ItemID.HellwingBow, ItemID.BloodRainBow,
            ItemID.DD2BetsyBow, ItemID.DaedalusStormbow, ItemID.FairyQueenRangedItem, ItemID.IceBow, ItemID.Marrow, ItemID.Phantasm, ItemID.DD2PhoenixBow, ItemID.PulseBow, ItemID.ShadowFlameBow, ItemID.Tsunami};
        public static int[] Broadswords;
        public bool IsBroadsword(Item item)
        {
            return (item.DamageType == DamageClass.Melee || item.DamageType == DamageClass.MeleeNoSpeed)  && item.useStyle == ItemUseStyleID.Swing;
            
        }
        public override void SetDefaults(Item entity)
        {
            
            if (Shotguns.Contains(entity.type))
            {
                EmpoweredAmmo = 0;
                entity.noUseGraphic = true;
                MaxAmmo = 2;
                Ammo = 2;
                entity.UseSound = null;
            }
            if (Bows.Contains(entity.type))
            {
                entity.noUseGraphic = true;
            }
            if (IsBroadsword(entity))
            {
                StoreSound = entity.UseSound;
                entity.UseSound = null;
                entity.noMelee = true;
                entity.noUseGraphic = true;
                if (entity.shoot == ProjectileID.None) entity.shoot = ModContent.ProjectileType<Sword>();
            }
        }
        public void ManualShoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Shoot(item, player, source, position, velocity, type, damage, knockback);
        }
        public override bool AltFunctionUse(Item item, Player player)
        {
            if (Bows.Contains(item.type) && player.ownedProjectileCounts[ModContent.ProjectileType<Bow>()] < 1)
            {
                Projectile.NewProjectile(new EntitySource_ItemUse_WithAmmo(player, item, player.ChooseAmmo(item).type), player.Center, Vector2.Zero, ModContent.ProjectileType<Bow>(), item.damage, item.knockBack, Main.myPlayer, item.type, 0, 1);
                return false;
            }
            return base.AltFunctionUse(item, player);
        }
        
        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (!vanillaShoot)
            {
                if (Shotguns.Contains(item.type) && !vanillaShoot)
                {
                    Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<Shotgun>(), damage, knockback, Main.myPlayer, item.type, 0, Ammo == 0 ? 1 : 0);
                    if (Ammo > 0) Ammo--;
                    return false;
                }
                if (Bows.Contains(item.type))
                {
                    if (player.ownedProjectileCounts[ModContent.ProjectileType<Bow>()] < 1)
                    Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<Bow>(), damage, knockback, Main.myPlayer, item.type, 0, 0);
                    return false;
                }
                if (IsBroadsword(item))
                {
                    
                    if (player.ownedProjectileCounts[ModContent.ProjectileType<Sword>()] < 1)
                    {
                        Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<Sword>(), damage, knockback, Main.myPlayer, item.type, velocity.ToRotation(), 0);
                    }
                    return false;
                }
            }
            return base.Shoot(item, player, source, position, velocity, type, damage, knockback);
        }
    }
}
