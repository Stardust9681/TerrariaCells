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
using rail;

namespace TerrariaCells.Content.Projectiles.HeldProjectiles
{
    public class Bow : ModProjectile
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.WoodenBow;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
        }
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
            float x = Projectile.ai[1] / (owner.HeldItem.useAnimation * 2);
            float lerper = x == 1 ? 1 : 1 - (float)Math.Pow(2, -10 * x);
            float min = Projectile.scale - 0.1f;
            if (Projectile.ai[2] == 1)
            {
                min = Projectile.scale - 0.1f;
                lerper = 1 - lerper;
            }
            Main.EntitySpriteDraw(t.Value, armPosition + new Vector2(8, -2 * Projectile.spriteDirection).RotatedBy(Projectile.rotation) - Main.screenPosition, null, lightColor, Projectile.rotation, new Vector2(10, t.Height() / 2), new Vector2(MathHelper.Lerp(Projectile.scale + 0.1f, Projectile.scale, lerper), MathHelper.Lerp(min, Projectile.scale , lerper)), Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically);
            if (Projectile.ai[2] == 1)
            {
                int arrow = GetArrowType();
                Asset<Texture2D> ar = TextureAssets.Projectile[arrow];
                Main.instance.LoadProjectile(arrow);
                Main.EntitySpriteDraw(ar.Value, armPosition + new Vector2(MathHelper.Lerp(8, 5, Projectile.ai[1] / owner.HeldItem.useAnimation), -6 ).RotatedBy(Projectile.rotation) - Main.screenPosition, new Rectangle(0, 0, ar.Width(), ar.Height()/ Main.projFrames[arrow]), lightColor, Projectile.rotation + MathHelper.Pi/2, new Vector2(1, ar.Height() / Main.projFrames[arrow] / 2), Projectile.scale, Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally);
            }
            return false;
        }
        //ai[0] = ItemID of mimiced weapon
        //ai[1] = timer
        //ai[2] = ai mode
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
            Projectile.rotation = (Main.MouseWorld - owner.MountedCenter).ToRotation();
            owner.direction = Projectile.spriteDirection;
            owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.ToRadians(90));
            owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.ToRadians(90));
            Projectile.Center = owner.Center;
            owner.heldProj = Projectile.whoAmI;

            //autofire arrows with no charge
            if (Projectile.ai[2] == 0)
            {
                if (Projectile.ai[1] % owner.HeldItem.useTime == 0)
                {
                    //fire using what vanilla does to shoot weapons
                    SoundEngine.PlaySound(owner.HeldItem.UseSound, Projectile.Center);
                    owner.HeldItem.GetGlobalItem<WeaponHoldoutify>().vanillaShoot = true;
                    MethodInfo PlayerItemCheck_Shoot = typeof(Player).GetMethod("ItemCheck_Shoot", BindingFlags.NonPublic | BindingFlags.Instance);
                    PlayerItemCheck_Shoot.Invoke(owner, [owner.whoAmI, owner.HeldItem, owner.HeldItem.damage]);
                    owner.HeldItem.GetGlobalItem<WeaponHoldoutify>().vanillaShoot = false;
                }
                Projectile.ai[1]++;
                if (Projectile.ai[1] >= owner.HeldItem.useAnimation)
                {
                    if (owner.controlUseTile)
                    {
                        Projectile.ai[1] = 0;
                        Projectile.ai[2] = 1;
                    }
                    else
                    {
                        Projectile.Kill();
                        return;
                    }
                }
            }
            //hold right click to charge for more powerful arrows
            if (Projectile.ai[2] == 1)
            {
                int chargeTime = (int)(owner.HeldItem.useAnimation * 1.5f);
                if (owner.controlUseTile)
                {
                    if (Projectile.ai[1] < chargeTime)
                    {
                        Projectile.ai[1]++;
                        if (Projectile.ai[1] == chargeTime)
                        {
                            SoundEngine.PlaySound(SoundID.MaxMana, Projectile.Center);
                            Vector2 armPosition = owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.Pi / 2);
                            armPosition.Y += 0;
                            for (int i = 0; i < 10; i++)
                            {
                                Dust d = Dust.NewDustPerfect(armPosition, DustID.FireworkFountain_Green);
                                d.noGravity = true;
                                Vector2 rotAdd = new Vector2(0, 1).RotatedBy(MathHelper.ToRadians(360f / 10 * i));
                                rotAdd.Y *= 2;
                                d.velocity = (Main.MouseWorld - armPosition).SafeNormalize(Vector2.Zero) * 2 + rotAdd.RotatedBy(Projectile.rotation);
                            }
                        }
                    }
                    if (Projectile.ai[1] > chargeTime * 0.3f)
                    {
                        owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.ThreeQuarters, Projectile.rotation - MathHelper.ToRadians(90));
                    }
                    if (Projectile.ai[1] > chargeTime * 0.6f)
                    {
                        owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Quarter, Projectile.rotation - MathHelper.ToRadians(90));
                    }
                    
                }
                else
                {
                    Projectile.ai[2] = 0;
                    
                    SoundEngine.PlaySound(owner.HeldItem.UseSound, Projectile.Center);
                    WeaponHoldoutify weapon = owner.HeldItem.GetGlobalItem<WeaponHoldoutify>();
                    weapon.vanillaShoot = true;
                    float arrowCharge = MathHelper.Lerp(0f, 1f, Projectile.ai[1] / chargeTime);
                    int originalCrit = owner.HeldItem.crit;
                    float originalShootSpeed = owner.HeldItem.shootSpeed;
                    if (arrowCharge == 1)
                    owner.HeldItem.crit = 100;

                    owner.HeldItem.shootSpeed += arrowCharge*9;
                    //Main.NewText(owner.HeldItem.shootSpeed);
                    MethodInfo PlayerItemCheck_Shoot = typeof(Player).GetMethod("ItemCheck_Shoot", BindingFlags.NonPublic | BindingFlags.Instance);
                    PlayerItemCheck_Shoot.Invoke(owner, [owner.whoAmI, owner.HeldItem, (int)(owner.HeldItem.damage * (arrowCharge + 2f))]);
                    //owner.HeldItem.damage = originalDamage;
                    owner.HeldItem.shootSpeed = originalShootSpeed;
                    owner.HeldItem.crit = originalCrit;
                    weapon.vanillaShoot = false;
                    Projectile.ai[2] = 0; //autofire part of code will handle killing the heldproj once usetime is fulfilled
                    Projectile.ai[1] = 1;
                    return;
                }
            }
        }
        //get which arrow to draw when charging
        //b/c ammo types, or bows using custom ammos
        public int GetArrowType()
        {
            Player owner = Main.player[Projectile.owner];
            int bow = owner.HeldItem.type;
            Item ammo = owner.ChooseAmmo(owner.HeldItem);
            int type = ammo.shoot;
            if (type == ProjectileID.WoodenArrowFriendly)
            {
                if (bow == ItemID.MoltenFury) type = ProjectileID.FireArrow;
                if (bow == ItemID.FairyQueenRangedItem) type = ProjectileID.FairyQueenRangedItemShot;
                if (bow == ItemID.HellwingBow) type = ProjectileID.Hellwing;
                if (bow == ItemID.BeesKnees) type = ProjectileID.BeeArrow;
            }
            if (bow == ItemID.BloodRainBow) type = ProjectileID.BloodRain;
            if (bow == ItemID.PulseBow) type = ProjectileID.PulseBolt;
            if (bow == ItemID.IceBow) type = ProjectileID.FrostArrow;
            if (bow == ItemID.Marrow) type = ProjectileID.BoneArrow;
            if (bow == ItemID.DD2BetsyBow) type = ProjectileID.DD2BetsyArrow;
            if (bow == ItemID.ShadowFlameBow) type = ProjectileID.ShadowFlameArrow;

            return type;
        }
    }
}
