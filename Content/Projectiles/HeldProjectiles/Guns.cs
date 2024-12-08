using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaCells.Common.GlobalItems;
using TerrariaCells.Common.Utilities;

namespace TerrariaCells.Content.Projectiles.HeldProjectiles
{
    public class Guns : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.timeLeft = 10000;
            Projectile.penetrate = -2;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.aiStyle = -1;


        }
        public override string Texture => "Terraria/Images/Item_" + ItemID.Handgun;
        public float? ForcedRotation = null;
        //lots of properties teehee
        public ref float gunID => ref Projectile.ai[0];
        public ref float timer => ref Projectile.ai[1];
        public ref float mode => ref Projectile.ai[2];
        public Asset<Texture2D> texture => TextureAssets.Item[(int)gunID];
        public Player owner => Main.player[Projectile.owner];
        public Item item => owner.HeldItem;
        public WeaponHoldoutify weapon => item.GetGlobalItem<WeaponHoldoutify>();
        public bool shotgun => WeaponHoldoutify.Shotguns.Contains((int)gunID);
        public bool autorifle => WeaponHoldoutify.Autorifles.Contains((int)gunID);
        public bool sniper => WeaponHoldoutify.Snipers.Contains((int)gunID);
        public bool handgun => WeaponHoldoutify.Handguns.Contains((int)gunID);
        public bool musket => WeaponHoldoutify.Muskets.Contains((int)gunID);
        public bool launcher => WeaponHoldoutify.Launchers.Contains((int)gunID);
        public Vector2 shotgunOffset { get
            {
                if (mode == 1) return new Vector2(-3, -8 * Projectile.spriteDirection);
                return new Vector2(-10, -2 * Projectile.spriteDirection);
            }
        }
        public Vector2 autorifleOffset
        {
            get
            {
                if (mode == 1) return new Vector2(-8, 0);
                return new Vector2(-10, -5 * Projectile.spriteDirection);
            }
        }
        public Vector2 sniperOffset
        {
            get
            {
                if (mode == 1) return new Vector2(-15, -2 * Projectile.spriteDirection);
                return new Vector2(-15, -2 * Projectile.spriteDirection);
            }
        }
        public Vector2 handgunOffset
        {
            get
            {
                if (mode == 1) return new Vector2(5, -5 * Projectile.spriteDirection);
                return new Vector2(-10, -4 * Projectile.spriteDirection);
            }
        }
        public Vector2 musketOffset
        {
            get
            {
                if (mode == 1) return new Vector2(-35, 10 * Projectile.spriteDirection);
                return new Vector2(-10, -5 * Projectile.spriteDirection);
            }
        }
        public Vector2 LauncherOffset
        {
            get
            {
                if (mode == 1) new Vector2(-20, -5 * Projectile.spriteDirection);
                return new Vector2(-20, -5 * Projectile.spriteDirection);
            }
        }
        
        public Vector2 armPosition
        {
            get
            {
                Vector2 pos = owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.Pi / 2);
                pos.Y += owner.gfxOffY;
                return pos;
            }
        }
         public int trueReloadTime { get
            {
                if (shotgun || musket || sniper || launcher) return (int)weapon.ReloadTime / weapon.MaxAmmo;
                return (int)weapon.ReloadTime;
            }
        }   
        public override bool PreDraw(ref Color lightColor)
        {
            if (!RealCheck())
            {
                return false;
            }
            //gun just moves backward slightly instead of rotating upward
            if (sniper || autorifle || gunID == ItemID.Toxikarp)
                DrawBackwardRecoil(ref lightColor, sniper ? sniperOffset : autorifleOffset);
            else if (shotgun || handgun || musket || launcher)
            {
                DrawNothingSpecial(ref lightColor, shotgun ? shotgunOffset : (handgun ? handgunOffset : (musket ? musketOffset : LauncherOffset)));
            }
            int[] magHandguns = { ItemID.Handgun, ItemID.PhoenixBlaster };
            if (shotgun || autorifle || (handgun && magHandguns.Contains((int)gunID)) || launcher)
            {
                Asset<Texture2D> heldAmmo = ModContent.Request<Texture2D>("TerrariaCells/Content/Projectiles/HeldProjectiles/Mag");
                if (shotgun) heldAmmo = ModContent.Request<Texture2D>("TerrariaCells/Content/Projectiles/HeldProjectiles/ShotgunShell");
                if (launcher) heldAmmo = TextureAssets.Projectile[ProjectileID.RocketI];
                if (gunID == ItemID.GrenadeLauncher) heldAmmo = TextureAssets.Projectile[ProjectileID.GrenadeI];
                if (gunID == ItemID.StarCannon) heldAmmo = TextureAssets.Projectile[ProjectileID.StarCannonStar];
                DrawAmmoInHand(ref lightColor, (int)trueReloadTime / 2,heldAmmo,0.8f);
            }
            if (launcher)
            {

            }
            return false;
        }
        public override void AI()
        {
            if (!RealCheck())
            {
                return;
            }
            Projectile.spriteDirection = Main.MouseWorld.X > owner.MountedCenter.X ? 1 : -1;
            Projectile.rotation = (Main.MouseWorld - owner.MountedCenter).ToRotation();
            if (gunID == ItemID.Toxikarp)
            {
                Projectile.rotation += MathHelper.ToRadians(45 * Projectile.spriteDirection);
            }
            if (ForcedRotation != null) Projectile.rotation = ForcedRotation.Value;
            owner.direction = Projectile.spriteDirection;
            Projectile.Center = owner.Center;
            owner.heldProj = Projectile.whoAmI;
            owner.itemAnimation = 2;
            owner.itemTime = 2;
            owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.PiOver2);
            Projectile.netUpdate = true;
            Projectile.scale = owner.HeldItem.scale;
            if (mode == 0)
            {
                int[] bigSpreadWeapons = { ItemID.ChainGun, ItemID.Gatligator };
                //uses the vanilla shooting method to shoot the gun. When adding our own weapons (if we do) this will need to be updated to account for modded weapons.
                //weird math because of clockwork assault rifle
                if (timer % item.useTime == 0 && timer < item.useAnimation)
                {
                    if (bigSpreadWeapons.Contains((int)gunID))
                    {
                        ForcedRotation = owner.AngleTo(Main.MouseWorld) + Main.rand.NextFloat(MathHelper.ToRadians(-20), MathHelper.ToRadians(20));
                    }
                    else
                    {
                        Shoot(2f, 2);
                    }
                    if (weapon.EmpoweredAmmo > 0) weapon.EmpoweredAmmo--;
                    weapon.Ammo--;
                    SoundEngine.PlaySound(weapon.StoreSound, Projectile.Center);
                }
                timer++;
                if (handgun || shotgun || musket || launcher)
                {
                    UpwardRecoil(handgun ? 4 : 3, handgun ? 30 : 60);
                }
                //shotgun shell falling out of gun
                if (Projectile.ai[1] == 10 && shotgun)
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromAI(), owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.ToRadians(90)), new Vector2(Main.rand.Next(-2, 3), -5), ModContent.ProjectileType<AmmoResidue>(), 0, 0, owner.whoAmI, 0);
                }
                if (timer >= item.useAnimation + item.reuseDelay)
                {
                    timer = 0;
                    if (weapon.Ammo <= 0) mode = 1;

                    if (!owner.controlUseItem)
                    {
                        //i already account for reusedelay time so dont need vanilla to do it again
                        owner.reuseDelay = 0;
                        Projectile.Kill();
                        return;
                    }
                }
            }
            if (mode == 1)
            {
                //enable active reload minigame
                if (timer == 0)
                {
                    weapon.SkillTimer = 0;
                }
                timer++;
                //guns at different specific rotations during reload
                if (shotgun)
                {
                    Projectile.rotation = MathHelper.ToRadians(140);
                    if (Projectile.spriteDirection == 1) Projectile.rotation -= MathHelper.Pi / 2;
                    owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, MathHelper.ToRadians(40 * -Projectile.spriteDirection));
                }
                if (handgun || launcher)
                {
                    Projectile.rotation = MathHelper.ToRadians(30);
                    if (Projectile.spriteDirection == -1) Projectile.rotation = MathHelper.ToRadians(150);
                    owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, MathHelper.ToRadians(40 * -Projectile.spriteDirection));
                }
                if (autorifle )
                {
                    Projectile.rotation = MathHelper.ToRadians(-30);
                    if (Projectile.spriteDirection == -1) Projectile.rotation = MathHelper.ToRadians(210);
                    owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, MathHelper.ToRadians(40 * -Projectile.spriteDirection));
                }
                if (gunID == ItemID.Toxikarp)
                {
                    Projectile.rotation = MathHelper.ToRadians(45);
                    if (Projectile.spriteDirection == -1) Projectile.rotation = MathHelper.ToRadians(135);
                }
                if (shotgun || autorifle || handgun || launcher)
                {
                    //arm goes down to the players side and then back up to the weapon
                    PocketReload();
                }
                else if (sniper || gunID == ItemID.Toxikarp)
                {
                    //arm cocks back
                    SniperReload();
                }
                else if (musket)
                {
                    //weapon turns upward and player "drops" something into the top
                    MusketReload();
                }
                //add ammo to the gun, some guns instantly gain full ammo, some add one at a time
                
                if (timer == trueReloadTime)
                {
                    if (autorifle || handgun || gunID == ItemID.Toxikarp) weapon.Ammo = weapon.MaxAmmo;
                    else weapon.Ammo++;
                    timer = 1;
                    SoundEngine.PlaySound(SoundID.Unlock, Projectile.Center);
                }
                if (weapon.Ammo >= weapon.MaxAmmo)
                {
                    timer = 0;
                    mode = 0;
                    //vortex beater is weird (vanilla has its own heldproj for it)
                    if (!owner.controlUseItem || gunID == ItemID.VortexBeater)
                    {
                        Projectile.Kill();
                    }
                    return;
                }

            }
            //owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.ToRadians(90));
        }
        public bool RealCheck()
        {
            if (Projectile.ai[0] < 0 || !owner.active || owner.dead || owner.noItems || owner.CCed)
            {
                Projectile.Kill();
                return false;
            }
            return true;
        }
        public void Shoot(float EmpoweredSpeedMult = 0, float EmpoweredDamageMult = 0, int EmpoweredCritChance = -1)
        {
            Player player = Main.player[Projectile.owner];
            weapon.vanillaShoot = true;
            float originalSpeed = item.shootSpeed;
            int originalCritChance = item.crit;
            int damage = player.GetWeaponDamage(item);
            if (weapon.EmpoweredAmmo > 0)
            {
                item.shootSpeed *= EmpoweredSpeedMult;
                damage = (int)(damage * EmpoweredDamageMult);
                if (EmpoweredCritChance >= 0) item.crit = EmpoweredCritChance;
                
            }
            MethodInfo PlayerItemCheck_Shoot = typeof(Player).GetMethod("ItemCheck_Shoot", BindingFlags.NonPublic | BindingFlags.Instance);
            PlayerItemCheck_Shoot.Invoke(owner, [owner.whoAmI, item, damage]);
            item.shootSpeed = originalSpeed;
            item.crit = originalCritChance;
            weapon.vanillaShoot = false;
            
        }
        public void UpwardRecoil(int divisor = 3, float angleDiff = 60)
        {
            int recoilTime = owner.HeldItem.useTime / divisor;
            int recoverTime = owner.HeldItem.useTime / divisor * 2;
            if (timer < recoilTime)
            {
                
                Projectile.rotation += TCellsUtils.LerpFloat(0, MathHelper.ToRadians(-angleDiff * Projectile.spriteDirection), timer, recoilTime, TCellsUtils.LerpEasing.OutQuint);
            }
            if (timer >= recoilTime && timer < recoilTime + recoverTime)
            {
                
                Projectile.rotation += TCellsUtils.LerpFloat(MathHelper.ToRadians(-60 * Projectile.spriteDirection), 0, timer, recoverTime, TCellsUtils.LerpEasing.InOutQuint, recoilTime);
            }
            float backArmAngle = Projectile.rotation - MathHelper.Pi / 2;
            if (!handgun)
            owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, backArmAngle);
            owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.ToRadians(90 - 20 * Projectile.spriteDirection));
        }
        public void PocketReload()
        {
            if (timer <= trueReloadTime && weapon.Ammo < weapon.MaxAmmo)
            {
                
                owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, TCellsUtils.LerpFloat( MathHelper.Pi / 2 * -Projectile.spriteDirection, 0, timer, trueReloadTime, TCellsUtils.LerpEasing.DownParabola));
                
            }
        }
        public void SniperReload()
        {
            owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.ToRadians(90));

            if (timer <= trueReloadTime * 0.2f)
            {
                owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.ToRadians(90));
            }
            else if (timer <= trueReloadTime * 0.4f)
            {
                owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.ThreeQuarters, Projectile.rotation - MathHelper.ToRadians(90));
            }
            else if (timer <= trueReloadTime * 0.6f)
            {
                owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Quarter, Projectile.rotation - MathHelper.ToRadians(90));
            }
            else if (timer <= trueReloadTime * 0.8f)
            {
                owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.ThreeQuarters, Projectile.rotation - MathHelper.ToRadians(90));
            }
            else if (timer <= trueReloadTime * 0.9f)
            {
                owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.ToRadians(90));
            }
            if (timer == (int)(trueReloadTime / 2))
            {
                SoundEngine.PlaySound(SoundID.Unlock, Projectile.Center);
            }
        }
        public void MusketReload()
        {
            Projectile.rotation = MathHelper.ToRadians(-90);
            owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, MathHelper.ToRadians(40 * -Projectile.spriteDirection));
            int stretch = 0;
            if (timer < trueReloadTime * 0.3f)
            {
                stretch = 2;
            }
            else if (timer < trueReloadTime * 0.5f)
            {
                stretch = 3;
            }
            float angle = 200;
            if (timer > trueReloadTime * 0.7f) angle += MathHelper.Lerp(0, 30, (timer - trueReloadTime * 0.7f) / (trueReloadTime * 0.7f));
            owner.SetCompositeArmFront(true, (Player.CompositeArmStretchAmount)stretch, MathHelper.ToRadians(angle * Projectile.spriteDirection));
        }
        public void DrawNothingSpecial(ref Color lightColor, Vector2 offset, Vector2? reloadOffset = null)
        {
            if (mode == 1 && reloadOffset != null) offset = reloadOffset.Value;
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (gunID == ItemID.Blowgun) spriteEffects = SpriteEffects.FlipHorizontally; 
            Main.EntitySpriteDraw(texture.Value, armPosition + offset.RotatedBy(Projectile.rotation) - Main.screenPosition, null, lightColor, Projectile.rotation, new Vector2(1, texture.Height()/2), Projectile.scale, Projectile.spriteDirection == 1 ? spriteEffects : spriteEffects | SpriteEffects.FlipVertically);
            
        }
        public void DrawBackwardRecoil(ref Color lightColor, Vector2 offset, Vector2? reloadOffset = null)
        {
            if (mode == 1 && reloadOffset != null) offset = reloadOffset.Value;
            
            
            Main.EntitySpriteDraw(texture.Value, armPosition + offset.RotatedBy(Projectile.rotation) +
                new Vector2(Projectile.ai[2] == 0 ? TCellsUtils.LerpFloat(5f, 0f, timer, item.useTime, TCellsUtils.LerpEasing.DownParabola, ((int)(timer / item.useTime) * item.useTime)) : 0, 0f).RotatedBy(Projectile.rotation - (gunID == ItemID.Toxikarp ? MathHelper.PiOver4 * Projectile.spriteDirection : 0))
                - Main.screenPosition,
                null, lightColor, Projectile.rotation, new Vector2(5, texture.Height() / 2), Projectile.scale, Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically);
        }
        public void DrawAmmoInHand(ref Color lightColor, int timeStart, Asset<Texture2D> ammoTexture, float scale = 0.8f)
        {
            if (mode == 1 && timer > timeStart)
            {
                
                
                float rot = TCellsUtils.LerpFloat(MathHelper.Pi / 2 * -Projectile.spriteDirection, 0, timer, trueReloadTime, TCellsUtils.LerpEasing.DownParabola);
                Vector2 armPos = owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, rot);
                armPos.Y += owner.gfxOffY;
                Main.EntitySpriteDraw(ammoTexture.Value, armPos + new Vector2(-2 * Projectile.spriteDirection, 5).RotatedBy(rot) - Main.screenPosition, null, lightColor, rot - MathHelper.ToRadians(-130 * Projectile.spriteDirection), ammoTexture.Size()/2, scale, SpriteEffects.None);
            }
        }
        
    }
}
