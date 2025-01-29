using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaCells.Common.ModPlayers;
using TerrariaCells.Common.Utilities;
using TerrariaCells.Content.Projectiles;

namespace TerrariaCells.Content.WeaponAnimations
{
    public class Sword : GlobalItem
    {
        public bool VanillaShoot = false;
        public static int[] Swords = { ItemID.BluePhasesaber, ItemID.GreenPhasesaber, ItemID.PurplePhasesaber, ItemID.YellowPhasesaber, ItemID.OrangePhasesaber, ItemID.RedPhasesaber, ItemID.WhitePhasesaber,
            ItemID.NightsEdge, ItemID.Excalibur, ItemID.TrueExcalibur, ItemID.TrueNightsEdge, ItemID.TheHorsemansBlade, ItemID.TerraBlade};
        public static int[] HeavySwords = { ItemID.DeathSickle, ItemID.IceSickle, ItemID.BladeofGrass, ItemID.CandyCaneSword, ItemID.ChlorophyteClaymore, ItemID.ChristmasTreeSword, ItemID.TheHorsemansBlade, ItemID.DD2SquireBetsySword };
        public static bool IsBroadsword(Item item)
        {
            return ((item.DamageType == DamageClass.Melee || item.DamageType == DamageClass.MeleeNoSpeed) && item.useStyle == ItemUseStyleID.Swing && item.pick == 0 && item.axe == 0 && item.hammer == 0 && !item.noMelee && !item.noUseGraphic) || Swords.Contains(item.type);

        }
        public override bool InstancePerEntity => true;
        public override bool AppliesToEntity(Item entity, bool lateInstantiation)
        {
            if (IsBroadsword(entity))
            {
                if (!Swords.Contains(entity.type))
                {
                    Swords = Swords.Append(entity.type).ToArray();
                }
                if (entity.useTime > 30 && !HeavySwords.Contains(entity.type))
                {
                    HeavySwords = HeavySwords.Append(entity.type).ToArray();
                }
                return true;
            };

            return false;
        }
        public override void UseStyle(Item item, Player player, Rectangle heldItemFrame)
        {
            if (IsBroadsword(item))
            {
                WeaponPlayer mplayer = player.GetModPlayer<WeaponPlayer>();


                mplayer.useDirection = -1;
                if (Main.MouseWorld.X >= player.Center.X)
                {
                    mplayer.useDirection = 1;

                }
                //make player face direction of swing
                player.direction = mplayer.useDirection;
                //sword point at cursor
                if (mplayer.swingType != 2)
                {
                    mplayer.useRotation = player.Center.AngleTo(Main.MouseWorld);
                }
                //get an offset for swinging motion
                float angleOffset = TCellsUtils.LerpFloat(-140, 110, player.itemAnimation, player.itemAnimationMax, TCellsUtils.LerpEasing.InOutCubic);
                if (mplayer.swingType == 0)
                {
                    angleOffset = TCellsUtils.LerpFloat(110, -140, player.itemAnimation, player.itemAnimationMax, TCellsUtils.LerpEasing.InOutCubic);
                }
                if (mplayer.swingType == 2)
                {
                    if (HeavySwords.Contains(item.type))
                    {
                        angleOffset = TCellsUtils.LerpFloat(220, -140, player.itemAnimation, player.itemAnimationMax, TCellsUtils.LerpEasing.InOutCubic);
                        item.noMelee = false;
                    }
                    else
                    {
                        angleOffset = 0;
                        item.noMelee = true;
                        if (player.itemAnimation == player.itemAnimationMax)
                            Projectile.NewProjectileDirect(player.GetSource_ItemUse(item), player.Center, player.itemRotation.ToRotationVector2(), ModContent.ProjectileType<SwordStabWave>(), item.damage, item.knockBack, player.whoAmI, ai1: item.useTime);
                    }
                }
                else
                {
                    item.noMelee = false;
                }
                //set that offset
                player.itemRotation = mplayer.useRotation + MathHelper.ToRadians(mplayer.useDirection == 1 ? 45 : 135) + MathHelper.ToRadians(angleOffset * mplayer.useDirection);
                //work with grav potion
                if (player.gravDir == -1f)
                {
                    player.itemRotation = -player.itemRotation;
                }

                //shoot projectiles the weapon should shoot at about 60% through animation
                if (player.itemAnimation == (int)(player.itemAnimationMax * 0.6f) && mplayer.shouldShoot)
                {
                    mplayer.shouldShoot = false;
                    VanillaShoot = true;
                    MethodInfo PlayerItemCheck_Shoot = typeof(Player).GetMethod("ItemCheck_Shoot", BindingFlags.NonPublic | BindingFlags.Instance);
                    PlayerItemCheck_Shoot.Invoke(player, [player.whoAmI, item, item.damage]);
                    VanillaShoot = false;

                }

                //increment swing type counter at end of swing
                if (player.itemAnimation == 1)
                {
                    mplayer.swingType += 1;
                    if (mplayer.swingType > 2)
                    {
                        mplayer.swingType = 0;
                    }

                }
                //arm
                player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, player.itemRotation + MathHelper.ToRadians(-135 * mplayer.useDirection));
                //correct item location
                player.itemLocation = player.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, player.itemRotation + MathHelper.ToRadians(-135 * mplayer.useDirection));
                //stab attack distance
                if (mplayer.swingType == 2 && !HeavySwords.Contains(item.type))
                {
                    int distance = 8;
                    float pinnacleTime = 0.6f;

                    //Main.NewText(player.itemAnimationMax * 0.6f);
                    if (player.itemAnimation > player.itemAnimationMax * pinnacleTime)
                    {
                        player.itemLocation += TCellsUtils.LerpVector2(Vector2.Zero, (player.itemRotation + MathHelper.ToRadians(135 + (mplayer.useDirection == -1 ? -90 : 0))).ToRotationVector2() * -distance, (float)player.itemAnimation, (float)player.itemAnimationMax * pinnacleTime, TCellsUtils.LerpEasing.OutSine);
                    }
                    else
                    {
                        player.itemLocation += TCellsUtils.LerpVector2(Vector2.Zero, (player.itemRotation + MathHelper.ToRadians(135 + (mplayer.useDirection == -1 ? -90 : 0))).ToRotationVector2() * -distance,  (float)player.itemAnimation, (float)player.itemAnimationMax * pinnacleTime, TCellsUtils.LerpEasing.OutSine);
                    }
                }
            }
            base.UseStyle(item, player, heldItemFrame);
        }
        public override bool CanUseItem(Item item, Player player)
        {
            if (player.GetModPlayer<WeaponPlayer>().swingType == 2 && !HeavySwords.Contains(item.type))
            {
                item.noMelee = true;
            }
            else
            {
                item.noMelee = false;
            }
            return base.CanUseItem(item, player);
        }
        public override void UseItemHitbox(Item item, Player player, ref Rectangle hitbox, ref bool noHitbox)
        {
            if (IsBroadsword(item))
            {
                //Main.NewText("j");
                WeaponPlayer mplayer = player.GetModPlayer<WeaponPlayer>();
                //width of the sword sprite with correct scale
                int itemWidth = (int)(TextureAssets.Item[item.type].Width() * (item.scale + mplayer.itemScale - 1));
                //make hitbox a square with the size of the weapon
                hitbox = new Rectangle(0, 0, itemWidth, itemWidth);
                //center it on the sword
                hitbox.Location = (player.Center + new Vector2(itemWidth, 0).RotatedBy(player.itemRotation - MathHelper.ToRadians(mplayer.useDirection == 1 ? 40 : 140)) - hitbox.Size() / 2).ToPoint();

            }
            //base.UseItemHitbox(item, player, ref hitbox, ref noHitbox);
        }
        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (IsBroadsword(item) && !VanillaShoot)
            {
                return false;
            }
            return base.Shoot(item, player, source, position, velocity, type, damage, knockback);
        }
    }
}
