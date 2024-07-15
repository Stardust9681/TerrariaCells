using Microsoft.CodeAnalysis.Operations;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaCells.Common.GlobalItems;

namespace TerrariaCells.Content.Projectiles.HeldProjectiles
{
    public class Shotgun : ModProjectile
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.Shotgun;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
        }
        public int SkillTimer = 0;
        public int SkillTimerMax = 0;
        public bool OwnerRightClickLastFrame = false;
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
        public override bool PreDraw(ref Color lightColor)
        {
            //ai[0] is the itemID of the sprite to clone
            Player owner = Main.player[Projectile.owner];
            if (!owner.active || owner.dead || owner.noItems || owner.CCed)
            {
                Projectile.Kill();
                return false;
            }
            Asset<Texture2D> t = TextureAssets.Item[(int)Projectile.ai[0]];
            Vector2 armPosition = owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.Pi / 2); // get position of hand
            armPosition.Y += owner.gfxOffY;
            Vector2 offset = new Vector2(0, -2 * Projectile.spriteDirection);
            if (Projectile.ai[2] == 1) offset = new Vector2(-3, -8 * Projectile.spriteDirection);
            Main.EntitySpriteDraw(t.Value, armPosition + offset.RotatedBy(Projectile.rotation) - Main.screenPosition, null, lightColor, Projectile.rotation, new Vector2(10, t.Height()/2), Projectile.scale, Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically);
            if (Projectile.ai[2] == 1 && Projectile.ai[1] > 18)
            {
                int reloadTime = (int)(owner.HeldItem.useTime * 0.8f);
                float x = Projectile.ai[1] / reloadTime;
                float lerper = (float)Math.Pow(-(2 * x - 1), 2);
                float rot = MathHelper.Lerp(0, MathHelper.Pi / 2 * -Projectile.spriteDirection, lerper);
                armPosition = owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, rot);
                armPosition.Y += owner.gfxOffY;
                Asset<Texture2D> shell = ModContent.Request<Texture2D>("TerrariaCells/Content/Projectiles/HeldProjectiles/ShotgunShell");
                Main.EntitySpriteDraw(shell.Value, armPosition + new Vector2(-2 * Projectile.spriteDirection, 5 ).RotatedBy(rot) - Main.screenPosition, null, lightColor, rot - MathHelper.ToRadians(-130*Projectile.spriteDirection), new Vector2(shell.Width()/2, shell.Height()/2), 0.8f, SpriteEffects.None);
            }
            if (Projectile.ai[2] == 1 && SkillTimer > 0)
            {
                Asset<Texture2D> circle = TextureAssets.Extra[174];
                Main.EntitySpriteDraw(circle.Value, owner.Center - Main.screenPosition, null, Color.LightSeaGreen * 0.6f, 0, circle.Size() / 2, MathHelper.Lerp(0, 1, ((float)SkillTimer) / SkillTimerMax), SpriteEffects.None);
            }
            return false;
        }

        public override void OnSpawn(IEntitySource source)
        {
            base.OnSpawn(source);
        }
        //ai[0] = ItemID of mimiced weapon
        //ai[1] = timer
        //ai[2] = ai mode
        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            owner.itemAnimation = 2;
            owner.itemTime = 2;
            if (SkillTimer > 0)
                SkillTimer--;
            if (!owner.active || owner.dead || owner.noItems || owner.CCed)
            {
                Projectile.Kill();
                return;
            }
            Projectile.spriteDirection = Main.MouseWorld.X > owner.MountedCenter.X ? 1 : -1;
            Projectile.rotation = (Main.MouseWorld - owner.MountedCenter).ToRotation();
            owner.direction = Projectile.spriteDirection;
            owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.ToRadians(90));

            Projectile.Center = owner.Center;
            owner.heldProj = Projectile.whoAmI;
            //fire the weapon if in fire mode
            if (Projectile.ai[2] == 0)
            {
                Projectile.ai[1]++;
                if (Projectile.ai[1] == 1)
                {
                    SoundEngine.PlaySound(SoundID.Item36, Projectile.Center);
                    owner.HeldItem.GetGlobalItem<WeaponHoldoutify>().vanillaShoot = true;
                    float originalSpeed = owner.HeldItem.shootSpeed;
                    int damage = owner.HeldItem.damage;
                    if (owner.HeldItem.GetGlobalItem<WeaponHoldoutify>().EmpoweredAmmo > 0)
                    {
                        owner.HeldItem.GetGlobalItem<WeaponHoldoutify>().EmpoweredAmmo--;
                        owner.HeldItem.shootSpeed *= 2;
                        damage = (int)(owner.HeldItem.damage * 1.4f);
                        Vector2 armPosition = owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.Pi / 2);
                        Projectile.NewProjectile(Projectile.GetSource_FromAI(), armPosition + new Vector2(20, -2 * Projectile.spriteDirection).RotatedBy(Projectile.rotation), Vector2.Zero, ModContent.ProjectileType<GunExplosion>(), 30, 0, owner.whoAmI);
                        SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
                        owner.velocity += new Vector2(-10, 0).RotatedBy(Projectile.rotation);
                    }
                    MethodInfo PlayerItemCheck_Shoot = typeof(Player).GetMethod("ItemCheck_Shoot", BindingFlags.NonPublic | BindingFlags.Instance);
                    PlayerItemCheck_Shoot.Invoke(owner, [owner.whoAmI, owner.HeldItem, damage]);
                    owner.HeldItem.shootSpeed = originalSpeed;
                    owner.HeldItem.GetGlobalItem<WeaponHoldoutify>().vanillaShoot = false;
                   
                }
                if (Projectile.ai[1] == 10)
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromAI(), owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.ToRadians(90)), new Vector2(Main.rand.Next(-2, 3), -5), ModContent.ProjectileType<AmmoResidue>(), 0, 0, owner.whoAmI, 0);
                }
                int recoilTime = owner.HeldItem.useTime / 3;
                int recoverTime = owner.HeldItem.useTime / 3 * 2;
                if (Projectile.ai[1] < recoilTime)
                {
                    float x = Projectile.ai[1] / recoilTime;
                    float lerper = x == 1 ? 1 : (1 - (float)Math.Pow(2, -10 * x));
                    Projectile.rotation += MathHelper.Lerp(0, MathHelper.ToRadians(-60 * Projectile.spriteDirection), lerper);
                }
                if (Projectile.ai[1] >= recoilTime && Projectile.ai[1] < recoilTime + recoverTime)
                {
                    float x = (Projectile.ai[1] - recoilTime) / recoverTime;
                    float lerper = x < 0.5 ? 8 * x * x * x * x : 1 - (float)Math.Pow(-2 * x + 2, 4) / 2;
                    Projectile.rotation += MathHelper.Lerp(MathHelper.ToRadians(-60 * Projectile.spriteDirection), 0, lerper);
                }
                float backArmAngle = Projectile.rotation - MathHelper.Pi/2;
                owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, backArmAngle);
                owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.ToRadians(90 - 20 * Projectile.spriteDirection));
                if (Projectile.ai[1] >= recoilTime + recoverTime + 1) Projectile.Kill();
            }
            //reload
            else
            {
                int reloadTime = (int)(owner.HeldItem.useTime * 0.8f);
                if (Projectile.ai[1] == 0)
                {
                    SkillTimer = (int)(reloadTime * owner.HeldItem.GetGlobalItem<WeaponHoldoutify>().MaxAmmo * 0.8f);
                    SkillTimerMax = SkillTimer;
                }
                if (owner.controlUseTile && !OwnerRightClickLastFrame && SkillTimer < 15 && SkillTimer > 0)
                {
                    owner.HeldItem.GetGlobalItem<WeaponHoldoutify>().Ammo = owner.HeldItem.GetGlobalItem<WeaponHoldoutify>().MaxAmmo;
                    owner.HeldItem.GetGlobalItem<WeaponHoldoutify>().EmpoweredAmmo = 1;
                    Projectile.ai[1] = 0;
                    for (int i = 0; i < 10; i++)
                    {
                        Dust d = Dust.NewDustPerfect(owner.Center + new Vector2(5 * owner.direction, -10), DustID.Terra);
                        d.noGravity = true;
                        d.velocity *= 0.1f;
                    }
                    SoundEngine.PlaySound(SoundID.Item30, owner.Center);
                }else if (owner.controlUseTile)
                {
                    SkillTimer = 0;
                }
                OwnerRightClickLastFrame = owner.controlUseTile;
                Projectile.ai[1]++;
                Projectile.Center = owner.Center;
                Projectile.rotation = MathHelper.ToRadians(140);
                if (Projectile.spriteDirection == 1) Projectile.rotation -= MathHelper.Pi/2;
                
                owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, MathHelper.ToRadians(40 * -Projectile.spriteDirection));
                WeaponHoldoutify item = owner.HeldItem.GetGlobalItem<WeaponHoldoutify>();
              
                
                
                if (Projectile.ai[1] <= reloadTime && item.Ammo < item.MaxAmmo)
                {
                    float x = Projectile.ai[1] / reloadTime;
                    float lerper = (float)Math.Pow(-(2 * x - 1), 2);
                    owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, MathHelper.Lerp(0 , MathHelper.Pi/2 * -Projectile.spriteDirection, lerper));
                    if (Projectile.ai[1] == reloadTime)
                    {
                        item.Ammo++;
                        Projectile.ai[1] = 1;
                        SoundEngine.PlaySound(SoundID.Unlock, Projectile.Center);
                        if (!owner.controlUseItem) Projectile.Kill();
                    }

                }
                if (item.Ammo == item.MaxAmmo && Projectile.ai[1] >= reloadTime/2)
                {
                    Projectile.Kill();
                }
            }
        }
    }
}
