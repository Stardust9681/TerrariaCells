﻿using System;
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

namespace TerrariaCells.Content.WeaponAnimations
{
    public class Shotgun : Gun
    {
        public static int[] Shotguns = { ItemID.Boomstick, ItemID.OnyxBlaster, ItemID.Shotgun, ItemID.TacticalShotgun, ItemID.QuadBarrelShotgun, ItemID.Xenopopper };
        public override bool InstancePerEntity => true;
        public override void SetStaticDefaults()
        {
            for (int i = 0; i < Shotguns.Length; i++)
            {
                ItemID.Sets.ItemsThatAllowRepeatedRightClick[Shotguns[i]] = true;
            }

            base.SetStaticDefaults();
        }
        public override bool AppliesToEntity(Item entity, bool lateInstantiation)
        {
            return Shotguns.Contains(entity.type);
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
        public override void UseStyle(Item item, Player player, Rectangle heldItemFrame)
        {
            WeaponPlayer mplayer = player.GetModPlayer<WeaponPlayer>();
            //player.itemAnimation but starting at 0
            int animationTime = player.itemAnimationMax - player.itemAnimation;
            //time recoil takes
            int maxRecoilTime = player.itemAnimationMax / 2;

            //only do shoot animation if you shouldnt be reloading
            if (Ammo > 0 && !mplayer.reloading && player.altFunctionUse != 2)
            {
                //at start of animation
                if (player.itemAnimation == player.itemAnimationMax - 1)
                {
                    //set values that stay constant throughout
                    mplayer.OriginalRotation = player.itemRotation;
                    mplayer.useDirection = -1;
                    if (Main.MouseWorld.X >= player.Center.X)
                    {
                        mplayer.useDirection = 1;
                    }
                    //play shoot sound
                    SoundEngine.PlaySound(StoredSound, player.Center);
                }
                //decrement ammo at end of animation because reasons
                if (player.itemAnimation == 1)
                {
                    Ammo--;  
                }
                //how far in the recoil time the recoil reaches its peak
                float midpoint = 0.7f;
                //lower than midpoint; going up
                if (animationTime < (int)(maxRecoilTime * midpoint))
                {
                    //epic math
                    player.itemRotation = TCellsUtils.LerpFloat(mplayer.OriginalRotation, mplayer.OriginalRotation - MathHelper.ToRadians(20 * mplayer.useDirection), animationTime, maxRecoilTime - (int)(maxRecoilTime * midpoint), TCellsUtils.LerpEasing.OutCubic);
                }
                //high than midpoint; going down
                else if (animationTime < maxRecoilTime)
                {
                    //epic math part 2
                    player.itemRotation = TCellsUtils.LerpFloat(mplayer.OriginalRotation - MathHelper.ToRadians(20 * mplayer.useDirection), mplayer.OriginalRotation, animationTime - (int)(maxRecoilTime * midpoint), (int)(maxRecoilTime - maxRecoilTime * midpoint), TCellsUtils.LerpEasing.InOutSine);
                }
                //arm position
                player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, player.itemRotation - (MathHelper.PiOver2 - MathHelper.ToRadians(20)) * mplayer.useDirection);
                player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, player.itemRotation - (MathHelper.PiOver2 - MathHelper.ToRadians(10)) * mplayer.useDirection);
            }
            else
            {
                //set constants for start of animation
                if (player.itemAnimation == player.itemAnimationMax - 1)
                {
                    mplayer.useDirection = -1;
                    if (Main.MouseWorld.X >= player.Center.X)
                    {
                        mplayer.useDirection = 1;

                    }
                    
                    mplayer.reloading = true;
                }
                //play reload sound and increase ammo at end of animation
                if (player.itemAnimation == 1)
                {
                    SoundEngine.PlaySound(SoundID.Unlock, player.Center);
                    Ammo++;
                    //force stop reloading if at max ammo
                    if (Ammo >= MaxAmmo)
                    {
                        mplayer.reloading = false;
                    }
                }
                //fixed item rotation and back hand rotation
                player.itemRotation = MathHelper.ToRadians(20 * mplayer.useDirection);
                player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, MathHelper.ToRadians(-30 * mplayer.useDirection));
                //front hand movement (go to pocket then back up to gun)
                float handRot = TCellsUtils.LerpFloat(-50 * mplayer.useDirection, 0, animationTime, player.itemAnimationMax / 2, TCellsUtils.LerpEasing.InSine);
                if (animationTime > player.itemAnimationMax / 2)
                {
                    handRot = TCellsUtils.LerpFloat(0, -50 * mplayer.useDirection, animationTime - (player.itemAnimationMax/2), player.itemAnimationMax / 2, TCellsUtils.LerpEasing.InSine);
                }
                player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, MathHelper.ToRadians(handRot));
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
