using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Reflection;
using Terraria.Audio;
using TerrariaCells.Common.GlobalItems;
using TerrariaCells.Common.Utilities;

namespace TerrariaCells.Content.Projectiles.HeldProjectiles
{
    public class Sword : ModProjectile
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.IronBroadsword;

        public int SwingCounter = 0;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
        }
        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.timeLeft = 10000;
            Projectile.penetrate = -2;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.aiStyle = -1;
            Projectile.extraUpdates = 5;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player owner = Main.player[Projectile.owner];
            if (!owner.active || owner.dead || owner.noItems || owner.CCed)
            {
                Projectile.Kill();
                return;
            }
            MethodInfo PlayerItemCheck_Shoot = typeof(Player).GetMethod("ApplyNPCOnHitEffects", BindingFlags.NonPublic | BindingFlags.Instance);
            PlayerItemCheck_Shoot.Invoke(owner, [owner.HeldItem, target.Hitbox, hit.SourceDamage, hit.Knockback, target.whoAmI, 1, damageDone]);
            
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
            
            if (Projectile.ai[2] != 1)
            {
                Main.EntitySpriteDraw(t.Value, armPosition + new Vector2(TCellsUtils.LerpFloat(-15, -25, Timer, owner.HeldItem.useAnimation * Projectile.extraUpdates+1, TCellsUtils.LerpEasing.Bell), -5 * Projectile.spriteDirection).RotatedBy(Projectile.rotation) - Main.screenPosition,
                    null, lightColor,
                    Projectile.rotation + MathHelper.ToRadians(Projectile.spriteDirection == 1 ? -135 : 135),
                    new Vector2(2, Projectile.spriteDirection == 1 ? t.Height() - 2 : 2),
                    Projectile.scale,// * MathHelper.Lerp(0.8f, 1.1f, scaleLerper),
                    Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically);
            }
            if (Projectile.ai[2] == 1)
            {
               
                Main.EntitySpriteDraw(t.Value, armPosition + new Vector2(TCellsUtils.LerpFloat(-15, -25, Timer, owner.HeldItem.useAnimation * Projectile.extraUpdates+1, TCellsUtils.LerpEasing.Bell), -5 * Projectile.spriteDirection).RotatedBy(Projectile.rotation) - Main.screenPosition,
                    null, lightColor,
                    Projectile.rotation + MathHelper.ToRadians(Projectile.spriteDirection == 1 ? -40 : 40),
                    new Vector2(t.Width() - 2, Projectile.spriteDirection == 1 ? t.Height() - 2 : 2),
                    Projectile.scale, //* MathHelper.Lerp(0.8f, 1.1f, scaleLerper),
                    Projectile.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically);
            }


            return false;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Player owner = Main.player[Projectile.owner];
            if (!owner.active || owner.dead || owner.noItems || owner.CCed)
            {
                Projectile.Kill();
                return false;
            }
            Asset<Texture2D> t = TextureAssets.Item[(int)Projectile.ai[0]];
            Vector2 armPosition = owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.Pi / 2);
            
            float scale = Projectile.scale * TCellsUtils.LerpFloat(0.8f, 1.1f, Timer, (owner.HeldItem.useAnimation * Projectile.extraUpdates + 1), TCellsUtils.LerpEasing.Bell) + 0.2f;
            return Collision.CheckAABBvLineCollision(targetHitbox.Location.ToVector2(), targetHitbox.Size(), armPosition, armPosition + new Vector2(t.Width(), t.Height()).RotatedBy(Projectile.rotation + MathHelper.ToRadians(130 + (Projectile.spriteDirection == -1 ? 5 : 15)))* scale );
        }
        public float Timer = 0;
        //ai[0] = item to clone
        //ai[1] = target rotation
        //ai[2] = ai style
        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            owner.itemAnimation = 2;
            owner.itemTime = 2;
            if (!owner.active || owner.dead || owner.noItems || owner.CCed)
            {
                Projectile.Kill();
                return;
            }
            Projectile.spriteDirection = Main.MouseWorld.X > owner.MountedCenter.X ? 1 : -1;
            owner.direction = Projectile.spriteDirection;
            if (Projectile.ai[2] != 2)
                Projectile.ai[1] = Projectile.AngleTo(Main.MouseWorld);
            Projectile.Center = owner.Center;
            owner.heldProj = Projectile.whoAmI;
            Projectile.netUpdate = true;
            Timer += 1f;
            


            if (Projectile.ai[2] == 0)
            {
                Projectile.friendly = true;
                Projectile.rotation = TCellsUtils.LerpFloat(Projectile.ai[1] + MathHelper.ToRadians(180 - 120 * Projectile.spriteDirection),
                     Projectile.ai[1] + MathHelper.ToRadians(180 + 60 * Projectile.spriteDirection),
                     Timer, (owner.HeldItem.useAnimation * Projectile.extraUpdates + 1), TCellsUtils.LerpEasing.InOutQuint);
            }
            else if (Projectile.ai[2] == 1)
            {
                Projectile.friendly = true;
                Projectile.rotation = TCellsUtils.LerpFloat(Projectile.ai[1] + MathHelper.ToRadians(180 + 60 * Projectile.spriteDirection),
                    Projectile.ai[1] + MathHelper.ToRadians(180 - 120 * Projectile.spriteDirection),
                     Timer, (owner.HeldItem.useAnimation * Projectile.extraUpdates + 1), TCellsUtils.LerpEasing.InOutQuint);
            }
            else if (Projectile.ai[2] == 2)
            {
                if (!owner.HeldItem.GetGlobalItem<WeaponHoldoutify>().Heavyweight)
                {
                    Projectile.friendly = false;
                    Projectile.rotation = owner.AngleFrom(Main.MouseWorld);
                }
                else
                {
                    Projectile.rotation = TCellsUtils.LerpFloat(Projectile.ai[1] + MathHelper.ToRadians(180 - 120 * Projectile.spriteDirection),
                     Projectile.ai[1] + MathHelper.ToRadians(180 + 240 * Projectile.spriteDirection),
                     Timer, (owner.HeldItem.useAnimation * Projectile.extraUpdates + 1), TCellsUtils.LerpEasing.InOutQuint);
                }
               
            }
            
            
            owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.ToRadians(-90) );
            owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, 0);
            Vector2 armpos = owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, Projectile.rotation);


            if (Projectile.ai[2] == 2 && Main.myPlayer == owner.whoAmI && Timer == 1 && !owner.HeldItem.GetGlobalItem<WeaponHoldoutify>().Heavyweight)
            {
                Projectile.NewProjectileDirect(Projectile.GetSource_FromAI(), owner.Center, Vector2.Zero, ModContent.ProjectileType<SwordStabWave>(), Projectile.damage, Projectile.knockBack, owner.whoAmI, Projectile.whoAmI, owner.HeldItem.useAnimation - 5);

            }
            
            if (Timer == (int)((owner.HeldItem.useAnimation * Projectile.extraUpdates + 1) * 0.4f))
            {
                SoundEngine.PlaySound(owner.HeldItem.GetGlobalItem<WeaponHoldoutify>().StoreSound, Projectile.Center);
                if (owner.HeldItem.shoot != Projectile.type)
                {
                    owner.HeldItem.GetGlobalItem<WeaponHoldoutify>().vanillaShoot = true;
                    MethodInfo PlayerItemCheck_Shoot = typeof(Player).GetMethod("ItemCheck_Shoot", BindingFlags.NonPublic | BindingFlags.Instance);
                    PlayerItemCheck_Shoot.Invoke(owner, [owner.whoAmI, owner.HeldItem, owner.GetWeaponDamage(owner.HeldItem)]);
                    owner.HeldItem.GetGlobalItem<WeaponHoldoutify>().vanillaShoot = false;
                }
            }
            if (Timer >= (owner.HeldItem.useAnimation * Projectile.extraUpdates + 1))
            {
                
                if (owner.controlUseItem)
                {
                    SwingCounter++;
                    Projectile.CritChance = 0;
                    Timer = 0;
                    
                    Projectile.ai[2] = Projectile.ai[2] == 0 ? 1 : 0;
                    if (SwingCounter >= 2)
                    {
                        Projectile.ai[2] = 2;
                        SwingCounter = -1;
                        Projectile.CritChance = 100;
                    }

                    Projectile.ResetLocalNPCHitImmunity();
                    FieldInfo VolcanoExplosions = typeof(Player).GetField("_spawnVolcanoExplosion", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (VolcanoExplosions != null)
                        VolcanoExplosions.SetValue(owner, true);
                }
                else
                {
                    Projectile.Kill();
                }
            }
            //Main.NewText(Projectile.rotation);
        }
    }
}
