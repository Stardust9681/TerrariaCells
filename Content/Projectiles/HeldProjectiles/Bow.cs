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
            Projectile.friendly = true;
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
            Main.EntitySpriteDraw(t.Value, armPosition + new Vector2(8, -2 * Projectile.spriteDirection).RotatedBy(Projectile.rotation) - Main.screenPosition, null, lightColor, Projectile.rotation, new Vector2(10, t.Height() / 2), Projectile.scale, Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically);
            if (Projectile.ai[2] == 1)
            {
                int arrow = GetArrowType();
                Asset<Texture2D> ar = TextureAssets.Projectile[arrow];
                Main.EntitySpriteDraw(ar.Value, armPosition + new Vector2(MathHelper.Lerp(8, 5, Projectile.ai[1] / owner.HeldItem.useAnimation), -6 ).RotatedBy(Projectile.rotation) - Main.screenPosition, null, lightColor, Projectile.rotation + MathHelper.Pi/2, new Vector2(1, ar.Height() / 2), Projectile.scale, Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally);
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
                    Projectile.Kill();
                    return;
                }
            }
            //hold right click to charge for more powerful arrows
            if (Projectile.ai[2] == 1)
            {
                if (owner.controlUseTile)
                {
                    if (Projectile.ai[1] < owner.HeldItem.useAnimation*2)
                    Projectile.ai[1]++;
                }
                else
                {
                    Projectile.ai[2] = 0;
                    
                    SoundEngine.PlaySound(owner.HeldItem.UseSound, Projectile.Center);
                    WeaponHoldoutify weapon = owner.HeldItem.GetGlobalItem<WeaponHoldoutify>();
                    weapon.vanillaShoot = true;
                    float arrowCharge = MathHelper.Lerp(1, 1.5f, Projectile.ai[1] / (owner.HeldItem.useAnimation * 2));
                    arrowCharge = MathHelper.Clamp(arrowCharge, 1, 1.5f);
                    //int originalDamage = owner.HeldItem.damage;
                    float originalShootSpeed = owner.HeldItem.shootSpeed;
                    //owner.HeldItem.damage = (int)(owner.HeldItem.damage * arrowCharge);
                    //owner.HeldItem.shootSpeed = (int)(owner.HeldItem.shootSpeed*arrowCharge*2);
                    Item item = new Item(owner.HeldItem.type);
                    item.shootSpeed *= arrowCharge * 2;
                    item.damage = (int)(item.damage * arrowCharge);
                    MethodInfo PlayerItemCheck_Shoot = typeof(Player).GetMethod("ItemCheck_Shoot", BindingFlags.NonPublic | BindingFlags.Instance);
                    PlayerItemCheck_Shoot.Invoke(owner, [owner.whoAmI, item, (int)(owner.HeldItem.damage * arrowCharge)]);
                    //owner.HeldItem.damage = originalDamage;
                    owner.HeldItem.shootSpeed *= originalShootSpeed;
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
            if (type == ItemID.WoodenArrow)
            {
                if (bow == ItemID.MoltenFury) type = ProjectileID.FlamingArrow;
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
