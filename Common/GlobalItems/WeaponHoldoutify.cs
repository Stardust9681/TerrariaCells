using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
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
        public float SkillTimer = 0;
        public float ReloadTime;
        public float ReloadSuccessRange = 0;
        public float ReloadSuccessLocation = 0;
        public bool RightClickedBefore = false;
        public int EmpoweredAmmo;
        public int MaxAmmo;
        public int Ammo;

        //fixing a sound anamoly
        public SoundStyle? StoreSound;
 
        public static int[] Shotguns = { ItemID.Boomstick, ItemID.OnyxBlaster, ItemID.Shotgun, ItemID.TacticalShotgun, ItemID.QuadBarrelShotgun, ItemID.Xenopopper};
        public static int[] Autorifles = { ItemID.Megashark, ItemID.Minishark, ItemID.Uzi, ItemID.ChainGun, ItemID.ClockworkAssaultRifle, ItemID.VortexBeater, ItemID.CandyCornRifle, ItemID.SDMG, ItemID.Gatligator, ItemID.CoinGun};
        //eventide and phantom phoenix not included because they're weird
        public static int[] Bows = { ItemID.WoodenBow, ItemID.AshWoodBow, ItemID.BorealWoodBow, ItemID.PalmWoodBow, ItemID.ShadewoodBow, ItemID.EbonwoodBow, ItemID.PearlwoodBow, ItemID.RichMahoganyBow,
            ItemID.CopperBow, ItemID.TinBow, ItemID.LeadBow, ItemID.IronBow, ItemID.SilverBow, ItemID.TungstenBow, ItemID.GoldBow, ItemID.PlatinumBow,
            ItemID.DemonBow, ItemID.TendonBow, ItemID.MoltenFury, ItemID.BeesKnees, ItemID.HellwingBow, ItemID.BloodRainBow,
            ItemID.DD2BetsyBow, ItemID.DaedalusStormbow, ItemID.IceBow, ItemID.Marrow, ItemID.Phantasm, ItemID.PulseBow, ItemID.ShadowFlameBow, ItemID.Tsunami};
        //phasesabers and shiny swings included by default because the way their set defaults is set ruins everything
        public static int[] Broadswords = { ItemID.BluePhasesaber, ItemID.GreenPhasesaber, ItemID.PurplePhasesaber, ItemID.YellowPhasesaber, ItemID.OrangePhasesaber, ItemID.RedPhasesaber, ItemID.WhitePhasesaber,
            ItemID.NightsEdge, ItemID.Excalibur, ItemID.TrueExcalibur, ItemID.TrueNightsEdge, ItemID.TheHorsemansBlade, ItemID.TerraBlade};
        public static int[] Handguns = { ItemID.FlintlockPistol, ItemID.PewMaticHorn, ItemID.PhoenixBlaster, ItemID.Revolver, ItemID.TheUndertaker, ItemID.VenusMagnum, ItemID.Handgun, ItemID.FlareGun, ItemID.PainterPaintballGun };
        public static int[] Snipers = { ItemID.SniperRifle };
        public static int[] Muskets = { ItemID.Musket, ItemID.RedRyder, ItemID.Blowpipe, ItemID.Blowgun, ItemID.Sandgun };
        public static int[] Guns = Shotguns.Concat(Autorifles).Concat(Handguns).Concat(Muskets).Concat(Snipers).ToArray();
        public bool IsBroadsword(Item item)
        {
            return ((item.DamageType == DamageClass.Melee || item.DamageType == DamageClass.MeleeNoSpeed) && item.useStyle == ItemUseStyleID.Swing && item.pick == 0 && item.axe == 0 && !item.noMelee && !item.noUseGraphic) || Broadswords.Contains(item.type);
            
        }
        
        public override void SetDefaults(Item entity)
        {
            if (entity.type == ItemID.MusketBall)
            {
                entity.damage = 0;
            }
            if (Guns.Contains(entity.type))
            {
                StoreSound = entity.UseSound;
                EmpoweredAmmo = 0;
                entity.noUseGraphic = true;
                entity.UseSound = null;
                entity.useAmmo = AmmoID.None;
                entity.shoot = ProjectileID.Bullet;
                //default for all guns
                ReloadSuccessRange = 0.3f;
                ReloadSuccessLocation = 0.5f;
                if (entity.type == ItemID.CandyCornRifle) entity.shoot = ProjectileID.CandyCorn;
            }
            if (Shotguns.Contains(entity.type))
            {
                MaxAmmo = 2;
                if (entity.type == ItemID.Shotgun)
                {
                    entity.damage = 8;
                }
                if (entity.type == ItemID.OnyxBlaster)
                {
                    entity.damage = 4;
                    MaxAmmo = 4;
                    ReloadSuccessRange = 0.15f;
                    ReloadSuccessLocation = 0.7f;
                }
                ReloadTime = (int)(entity.useTime * MaxAmmo * 0.7f);
                
            }
            if (Handguns.Contains(entity.type))
            {
                MaxAmmo = 15;
                ReloadTime = entity.useAnimation * 3;
                if (entity.type == ItemID.PainterPaintballGun)
                {
                    entity.shoot = ProjectileID.PainterPaintball;
                }
                if (entity.type == ItemID.FlareGun)
                {
                    entity.shoot = Main.rand.NextBool() ? ProjectileID.Flare : ProjectileID.BlueFlare;
                    entity.holdStyle = ItemHoldStyleID.None;
                }
                if (entity.type == ItemID.PewMaticHorn)
                {
                    entity.shoot = ProjectileID.PewMaticHornShot;
                }
            }
            if (Autorifles.Contains(entity.type))
            {
                ReloadTime = entity.useAnimation * 7;
                if (entity.type == ItemID.CoinGun)
                {
                    MaxAmmo = 30;
                    entity.damage = 20;
                    entity.shoot = ProjectileID.SilverCoin;
                }
                if (entity.type == ItemID.Minishark)
                {
                    MaxAmmo = 60;
                    entity.useTime = entity.useAnimation = 6;
                }
                if (entity.type == ItemID.VortexBeater)
                {
                    MaxAmmo = 100;
                    ReloadTime = 40;
                }
            }
            if (Muskets.Contains(entity.type))
            {
                MaxAmmo = 1;
                
                if (entity.type == ItemID.Blowgun)
                {
                    entity.shoot = ProjectileID.PoisonDartBlowgun;
                }
                if (entity.type == ItemID.Blowpipe)
                {
                    entity.shoot = ProjectileID.Seed;
                }
                if (entity.type == ItemID.Sandgun)
                {
                    entity.shoot = ProjectileID.SandBallGun;
                    entity.useTime = entity.useAnimation = 40;
                }
                ReloadTime = entity.useAnimation * 1.2f;
            }
            if (Snipers.Contains(entity.type))
            {
                MaxAmmo = 1;
                ReloadTime = entity.useAnimation * 0.8f;
                
            }
            if (Bows.Contains(entity.type) )
            {
                entity.noUseGraphic = true;
                entity.useAmmo = AmmoID.None;
                entity.shoot = ProjectileID.WoodenArrowFriendly;
                if (entity.type == ItemID.DemonBow || entity.type == ItemID.TendonBow)
                    entity.shoot = ProjectileID.UnholyArrow;
                if (entity.type == ItemID.TendonBow)
                    entity.shoot = ProjectileID.BloodArrow;
                if (entity.type == ItemID.BeesKnees)
                    entity.shoot = ProjectileID.BeeArrow;
                if (entity.type == ItemID.MoltenFury)
                    entity.shoot = ProjectileID.FireArrow;
                if (entity.type == ItemID.IceBow)
                    entity.shoot = ProjectileID.FrostArrow;
                if (entity.type == ItemID.PulseBow)
                    entity.shoot = ProjectileID.PulseBolt;
                if (entity.type == ItemID.HellwingBow)
                    entity.shoot = ProjectileID.Hellwing;
                if (entity.type == ItemID.BloodRainBow)
                    entity.shoot = ProjectileID.BloodArrow;
                if (entity.type == ItemID.Marrow)
                    entity.shoot = ProjectileID.BoneArrow;
                if (entity.type == ItemID.DD2BetsyBow)
                    entity.shoot = ProjectileID.DD2BetsyArrow;
                if (entity.type == ItemID.ShadowFlameBow)
                    entity.shoot = ProjectileID.ShadowFlameArrow;
                if (entity.type == ItemID.RocketLauncher)
                    entity.shoot = ProjectileID.RocketI;
            }
            if (entity.type == ItemID.FairyQueenRangedItem)
            {
                entity.useAmmo = AmmoID.None;
                entity.shoot = ProjectileID.FairyQueenRangedItemShot;
            }
            if (entity.type == ItemID.DD2PhoenixBow)
            {
                entity.useAmmo = AmmoID.None;
            }
            if (IsBroadsword(entity))
            {
                StoreSound = entity.UseSound;
                entity.UseSound = null;
                entity.noMelee = true;
                entity.noUseGraphic = true;
                if (entity.shoot == ProjectileID.None) entity.shoot = ModContent.ProjectileType<Sword>();
                if (!Broadswords.Contains(entity.type))
                    Broadswords = Broadswords.Append(entity.type).ToArray();
            }
            if (MaxAmmo > 0) Ammo = MaxAmmo;
            if (ReloadTime > 0) SkillTimer = ReloadTime;
        }
        public override void UpdateInventory(Item item, Player player)
        {
            if (SkillTimer < ReloadTime + 20)
            {
                
                float skillProgress = Math.Clamp(SkillTimer, 0, ReloadTime) / ReloadTime;
                SkillTimer++;
                if (skillProgress < ReloadSuccessLocation + ReloadSuccessRange/2 && skillProgress > ReloadSuccessLocation - ReloadSuccessRange/2 && player.controlUseTile && !RightClickedBefore)
                {
                    SkillTimer = ReloadTime;
                    Ammo = MaxAmmo;
                    if (Shotguns.Contains(item.type))
                    {
                        EmpoweredAmmo = 1;
                    }
                    if (Autorifles.Contains(item.type))
                    {
                        EmpoweredAmmo = (int)(MaxAmmo * 0.3f);
                    }
                    if (Snipers.Contains(item.type))
                    {
                        EmpoweredAmmo = 1;
                    }
                    if (Handguns.Contains(item.type))
                    {
                        EmpoweredAmmo = 3;
                    }
                    if (Muskets.Contains(item.type))
                    {
                        EmpoweredAmmo = 1;
                    }
                    SoundEngine.PlaySound(SoundID.Unlock);
                }
                if (player.controlUseTile)
                {
                    RightClickedBefore = true;
                }
                
            }
            else
            {
                RightClickedBefore = false;
            }
            if (player.controlUseTile && item.type == ItemID.FairyQueenRangedItem)
                item.noUseGraphic = true;
            else if (item.type == ItemID.FairyQueenRangedItem)
                item.noUseGraphic = false;
            //code for making autorifles shoot faster after a perfect reload. uncomment if you want it
            //if (EmpoweredAmmo > 0 && Autorifles.Contains(item.type))
            //{
            //    item.useTime = (int)(ContentSamples.ItemsByType[item.type].useTime * 0.7f);
            //    item.useAnimation = (int)(ContentSamples.ItemsByType[item.type].useAnimation * 0.7f);
            //    item.reuseDelay = (int)(ContentSamples.ItemsByType[item.type].reuseDelay * 0.7f);
            //}
            //if (EmpoweredAmmo == 1 && Autorifles.Contains(item.type))
            //{
            //    item.useTime = ContentSamples.ItemsByType[item.type].useTime;
            //    item.useAnimation = ContentSamples.ItemsByType[item.type].useAnimation;
            //    item.reuseDelay = ContentSamples.ItemsByType[item.type].reuseDelay;
            //}
            base.UpdateInventory(item, player);
        }
        public void ManualShoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Shoot(item, player, source, position, velocity, type, damage, knockback);
        }
        public override bool AltFunctionUse(Item item, Player player)
        {

            if ((Bows.Contains(item.type) || item.type == ItemID.DD2PhoenixBow || item.type == ItemID.FairyQueenRangedItem && !player.ItemAnimationActive) && player.ownedProjectileCounts[ModContent.ProjectileType<Bow>()] < 1)
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
                if (Guns.Contains(item.type) && item.type != ItemID.VortexBeater)
                {
                    Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<Guns>(), damage, knockback, Main.myPlayer, item.type, 0, Ammo == 0 ? 1 : 0);
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
