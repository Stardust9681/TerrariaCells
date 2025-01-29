using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaCells.Common.ModPlayers;
using TerrariaCells.Common.Utilities;
using TerrariaCells.Content.Projectiles.HeldProjectiles;

namespace TerrariaCells.Content.WeaponAnimations
{
    public class Autorifle : Gun
    {
        //sniper is here because i was gonna give it the same animation anyways
        public static int[] Autorifles = { ItemID.Megashark, ItemID.Minishark, ItemID.Uzi, ItemID.ChainGun, ItemID.ClockworkAssaultRifle, ItemID.CandyCornRifle, ItemID.SDMG, ItemID.Gatligator, ItemID.CoinGun, ItemID.SniperRifle };
        public override bool InstancePerEntity => true;
        public override bool AppliesToEntity(Item entity, bool lateInstantiation)
        {
            return Autorifles.Contains(entity.type);
        }
        public override bool AltFunctionUse(Item item, Player player)
        {
            if (Ammo < MaxAmmo && player.itemAnimation == 0)
            {
                player.GetModPlayer<WeaponPlayer>().reloading = true;
                return true;
            }
            return base.AltFunctionUse(item, player);
        }
        public override bool CanUseItem(Item item, Player player)
        {
            WeaponPlayer mplayer = player.GetModPlayer<WeaponPlayer>();
            if (Ammo > 0 && !mplayer.reloading && player.altFunctionUse != 2)
            {

                item.useTime = OriginalUseTime;
                item.useAnimation = OriginalUseAnimation;
                item.reuseDelay = OriginalReuseDelay;
            }
            else
            {
                item.useTime = (int)(OriginalUseTime * ReloadTimeMult);
                item.useAnimation = (int)(OriginalUseAnimation * ReloadTimeMult);
                item.reuseDelay = (int)(OriginalReuseDelay * ReloadTimeMult);
            }
            return base.CanUseItem(item, player);
        }
        public override void UseStyle(Item item, Player player, Rectangle heldItemFrame)
        {
            WeaponPlayer mplayer = player.GetModPlayer<WeaponPlayer>();
            //player.itemAnimation but starting at 0
            int animationTime = player.itemAnimationMax - player.itemAnimation;
            //time recoil takes
            int maxRecoilTime = player.itemAnimationMax / 2;
            ReloadTimeMult = 3;

            //only do shoot animation if you shouldnt be reloading
            if (Ammo > 0 && !mplayer.reloading && player.altFunctionUse != 2)
            {
                mplayer.useDirection = -1;
                if (Main.MouseWorld.X >= player.Center.X)
                {
                    mplayer.useDirection = 1;
                }
                //at start of animation
                if (player.itemAnimation == player.itemAnimationMax - 1 && player.reuseDelay == item.reuseDelay)
                {
                    //set values that stay constant throughout
                    mplayer.OriginalRotation = player.itemRotation;
                    
                    //play shoot sound
                    SoundEngine.PlaySound(StoredSound, player.Center);
                }
                //decrement ammo at end of animation because reasons
                if (player.itemAnimation == 1 && player.reuseDelay == item.reuseDelay)
                {
                    Ammo--;  
                }

                player.itemRotation = player.AngleTo(Main.MouseWorld);
                if (player.direction == -1)
                {
                    player.itemRotation += MathHelper.Pi;
                }
                if (player.itemTime < 5 && player.reuseDelay == item.reuseDelay)
                {
                    player.itemLocation += TCellsUtils.LerpVector2(Vector2.Zero, (player.AngleTo(Main.MouseWorld) + MathHelper.Pi).ToRotationVector2() * 5, player.itemTime, 5, TCellsUtils.LerpEasing.DownParabola);
                }
                //arm position
                player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, player.itemRotation - (MathHelper.PiOver2 - MathHelper.ToRadians(20)) * player.direction);
                player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, player.itemRotation - (MathHelper.PiOver2 - MathHelper.ToRadians(10)) * player.direction);
            }
            else
            {
                
                //set constants for start of animation
                if (player.itemAnimation == player.itemAnimationMax - 1 && player.reuseDelay == item.reuseDelay)
                {
                    mplayer.useDirection = -1;
                    if (Main.MouseWorld.X >= player.Center.X)
                    {
                        mplayer.useDirection = 1;

                    }
                    //Main.NewText(player.altFunctionUse);
                    mplayer.reloading = true;
                }
                
                //fixed item rotation and back hand rotation
                player.itemRotation = MathHelper.ToRadians(0 * mplayer.useDirection);
                player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, MathHelper.ToRadians(-80 * mplayer.useDirection));
                //front hand movement (go to pocket then back up to gun)
                if (ReloadStep == 0)
                {
                    Player.CompositeArmStretchAmount stretch = Player.CompositeArmStretchAmount.Full;
                    if (animationTime > player.itemAnimationMax / 2)
                    {
                        stretch = Player.CompositeArmStretchAmount.Quarter;
                    }
                    if (animationTime == player.itemAnimationMax-1)
                    {
                        //Projectile.NewProjectileDirect(player.GetSource_ItemUse(item), player.itemLocation + (player.itemRotation + MathHelper.ToRadians(mplayer.useDirection == -1 ? 165 : 30)).ToRotationVector2() *20, player.velocity / 2, ModContent.ProjectileType<AmmoResidue>(), 0, 0, player.whoAmI, ai1:1);
                    }
                    player.SetCompositeArmFront(true, stretch, player.itemRotation - MathHelper.PiOver2 * mplayer.useDirection);
                }
                else if (ReloadStep == 1)
                {
                    float handRot = TCellsUtils.LerpFloat(-50 * mplayer.useDirection, 0, animationTime, player.itemAnimationMax / 2, TCellsUtils.LerpEasing.InSine);
                    if (animationTime > player.itemAnimationMax / 2)
                    {
                        handRot = TCellsUtils.LerpFloat(0, -50 * mplayer.useDirection, animationTime - ((player.itemAnimationMax - 4) / 2), player.itemAnimationMax / 2, TCellsUtils.LerpEasing.InSine);

                    }
                    handRot = TCellsUtils.LerpFloat(-50 * mplayer.useDirection, 0, animationTime , player.itemAnimationMax, TCellsUtils.LerpEasing.DownParabola);
                    player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, MathHelper.ToRadians(handRot));
                }
                else
                {
                    Player.CompositeArmStretchAmount stretch = Player.CompositeArmStretchAmount.Full;
                    
                    if (animationTime > player.itemAnimationMax / 3)
                    {
                        stretch = Player.CompositeArmStretchAmount.ThreeQuarters;
                    }
                    if (animationTime > player.itemAnimationMax / 2)
                    {
                        stretch = Player.CompositeArmStretchAmount.Quarter;
                    }
                    if (animationTime > player.itemAnimationMax * (2 / 3f))
                    {
                        stretch = Player.CompositeArmStretchAmount.None;
                    }
                    player.SetCompositeArmFront(true, stretch, player.itemRotation - MathHelper.PiOver2 * player.direction);
                }
                //play reload sound and increase ammo at end of animation
                if (player.itemAnimation == 1)
                {
                    SoundEngine.PlaySound(SoundID.Unlock, player.Center);
                    if (ReloadStep == 2)
                    {
                        Ammo = MaxAmmo;
                        ReloadStep = 0;
                    }
                    else
                    {

                        ReloadStep++;
                    }
                    //force stop reloading if at max ammo
                    if (Ammo >= MaxAmmo)
                    {
                        mplayer.reloading = false;
                    }
                }
            }
        }
        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            //only shoot if not reloading
            if (Ammo > 0 && !player.GetModPlayer<WeaponPlayer>().reloading)
            {       
                return base.Shoot(item, player, source, position, velocity, type, damage, knockback);
            }
            return false;
        }
    }
}
