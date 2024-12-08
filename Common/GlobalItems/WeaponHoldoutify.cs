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

        //swords
        public bool Heavyweight = false;
        public static int[] HeavyweightAnyways = { ItemID.DeathSickle, ItemID.IceSickle, ItemID.BladeofGrass, ItemID.CandyCaneSword, ItemID.ChlorophyteClaymore, ItemID.ChristmasTreeSword, ItemID.TheHorsemansBlade, ItemID.DD2SquireBetsySword };


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
        public static int[] Launchers = { ItemID.RocketLauncher, ItemID.StarCannon, ItemID.GrenadeLauncher };
        public static int[] Guns = Shotguns.Concat(Autorifles).Concat(Handguns).Concat(Muskets).Concat(Snipers).Concat(Launchers).Append(ItemID.Toxikarp).ToArray();
        public bool IsBroadsword(Item item)
        {
            return ((item.DamageType == DamageClass.Melee || item.DamageType == DamageClass.MeleeNoSpeed) && item.useStyle == ItemUseStyleID.Swing && item.pick == 0 && item.axe == 0 && item.hammer == 0 && !item.noMelee && !item.noUseGraphic) || Broadswords.Contains(item.type);
            
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
                if (entity.type == ItemID.Toxikarp)
                {
                    entity.shoot = ProjectileID.ToxicBubble;
                    MaxAmmo = 20;
                    entity.useTime = entity.useAnimation = 12;
                    entity.damage = 2;
                    ReloadTime = 60;
                };
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
                    entity.damage = 5;
                    MaxAmmo = 2;
                    entity.useTime = entity.useAnimation = 48;

                    //ReloadSuccessRange = 0.15f;
                    //ReloadSuccessLocation = 0.7f;
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
                if (entity.type == ItemID.PhoenixBlaster)
                {
                    MaxAmmo = 15;
                    entity.damage = 8;
                    entity.useTime = entity.useAnimation = 14;

                }
            }
            if (Autorifles.Contains(entity.type))
            {
                MaxAmmo = 30;
                ReloadTime = entity.useAnimation * 7;
                if (entity.type == ItemID.CoinGun)
                {
                    entity.damage = 20;
                    entity.shoot = ProjectileID.SilverCoin;
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
                
                if (entity.type == ItemID.SniperRifle)
                {
                    entity.useTime = entity.useAnimation = 40;
                    entity.damage = 25;

                }
                ReloadTime = entity.useAnimation * 0.8f;
            }
            if (Launchers.Contains(entity.type))
            {
                MaxAmmo = 1;
                entity.shoot = ProjectileID.RocketI;
                if (entity.type == ItemID.RocketLauncher)
                {
                    MaxAmmo = 4;
                    entity.useTime = entity.useAnimation = 30;
                    entity.damage = 25;
                }
                if (entity.type == ItemID.GrenadeLauncher)
                {
                    MaxAmmo = 5;
                    entity.damage = 18;
                    entity.useTime = entity.useAnimation = 20;
                    entity.shoot = ProjectileID.GrenadeI;
                }
                if (entity.type == ItemID.StarCannon)
                {
                    entity.useTime = entity.useAnimation = 40;
                    entity.damage = 15;
                    MaxAmmo = 3;
                    entity.shoot = ProjectileID.StarCannonStar;
                }

                ReloadTime = (int)(entity.useAnimation * MaxAmmo);
                if (entity.type == ItemID.StarCannon)
                {
                    ReloadTime *= 2;
                }
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

                if (entity.type == ItemID.IceBow)
                {
                    entity.damage = 4;
                }
                if (entity.type == ItemID.PulseBow)
                {
                    entity.damage = 8;
                    entity.useTime = entity.useAnimation = 23;
                }

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

                if (entity.useAnimation >= 30 || HeavyweightAnyways.Contains(entity.type))
                {
                    Heavyweight = true;
                }
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
                    if (Launchers.Contains(item.type))
                    {
                        EmpoweredAmmo = 1;
                    }
                    if (item.type == ItemID.Toxikarp)
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
            if (item.type == ItemID.OnyxBlaster)
            {
                for (int i = 0; i < 4; i++)
                {
                    Projectile.NewProjectile(source, position, velocity.RotatedByRandom(MathHelper.ToRadians(20)) * Main.rand.NextFloat(0.8f, 1.2f), type, damage, knockback);
                }
                if (EmpoweredAmmo > 0)
                {
                    Projectile proj = Projectile.NewProjectileDirect(source, position, velocity, 661, damage, knockback);
                    proj.CritChance = 100;
                }
                return false;
            }
            if (item.type == ItemID.StarCannon)
            {
                Projectile.NewProjectileDirect(source, position, velocity * 1.1f, type, damage, knockback);
                Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback);
                Projectile.NewProjectileDirect(source, position, velocity * 0.9f, type, damage, knockback);
            }
            return base.Shoot(item, player, source, position, velocity, type, damage, knockback);
        }
    }
}
