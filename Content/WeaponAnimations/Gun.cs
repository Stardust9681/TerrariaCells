using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaCells.Content.WeaponAnimations
{
    public abstract class Gun : GlobalItem
    {
        public override bool InstancePerEntity => true;
        public int MaxAmmo = 4;
        public int Ammo = 0;
        public float ReloadTimeMult = 1;
        public int ReloadStep = 0;
        public bool FullyReloads = true;
        public SoundStyle? StoredSound = null;
        public int SkipStep = -1;

        public int OriginalUseTime;
        public int OriginalUseAnimation;
        public int OriginalReuseDelay;
        

        public override void SetDefaults(Item item)
        {
            
            StoredSound = item.UseSound;
            item.UseSound = null;

            OriginalUseAnimation = item.useAnimation;
            OriginalUseTime = item.useTime;
            OriginalReuseDelay = item.reuseDelay;

            switch (item.type)
            {

                case ItemID.SniperRifle:
                case ItemID.StarCannon:
                case ItemID.RocketLauncher:
                case ItemID.FlareGun:
                case ItemID.GrenadeLauncher:
                    MaxAmmo = 1;
                    break;
                case ItemID.QuadBarrelShotgun:
                case ItemID.Boomstick:
                case ItemID.Shotgun:
                    MaxAmmo = 2;
                    break;
                case ItemID.OnyxBlaster:
                case ItemID.TacticalShotgun:
                case ItemID.Xenopopper:
                    MaxAmmo = 4;
                    break;
                case ItemID.Revolver:
                case ItemID.TheUndertaker:
                case ItemID.ClockworkAssaultRifle:
                    MaxAmmo = 6;
                    break;
                case ItemID.FlintlockPistol:
                case ItemID.PewMaticHorn:
                case ItemID.PainterPaintballGun:
                    MaxAmmo = 8;
                    break;
                case ItemID.PhoenixBlaster:
                case ItemID.Handgun:
                case ItemID.CoinGun:
                    MaxAmmo = 15;
                    break;
                case ItemID.VenusMagnum:
                case ItemID.CandyCornRifle:
                    MaxAmmo = 30;
                    break;
                case ItemID.Minishark:
                case ItemID.Megashark:
                case ItemID.Uzi:
                case ItemID.Gatligator:
                    MaxAmmo = 40;
                    break;
                case ItemID.ChainGun:
                case ItemID.SDMG:
                    MaxAmmo = 80;
                    break;

            }
            switch (item.type)
            {

                 case ItemID.FlareGun: 
                case ItemID.Revolver:
                case ItemID.TheUndertaker:
                case ItemID.FlintlockPistol:
                case ItemID.PewMaticHorn:
                case ItemID.PainterPaintballGun:
                case ItemID.PhoenixBlaster:
                case ItemID.Handgun:
                case ItemID.VenusMagnum:
                    ReloadTimeMult = 2;
                    break;
                case ItemID.QuadBarrelShotgun:
                case ItemID.Boomstick:
                case ItemID.Shotgun:
                case ItemID.OnyxBlaster:
                case ItemID.TacticalShotgun:
                case ItemID.Xenopopper:
                    ReloadTimeMult = 1;
                    break;
                
                case ItemID.Minishark:
                    ReloadTimeMult = 4;
                    break;
                case ItemID.ClockworkAssaultRifle:
                case ItemID.CandyCornRifle:
                case ItemID.Megashark:
                case ItemID.Uzi:
                case ItemID.Gatligator:
                case ItemID.CoinGun:
                case ItemID.ChainGun:
                case ItemID.SDMG:
                    ReloadTimeMult = 3;
                    break;
                 
                case ItemID.GrenadeLauncher:
                    ReloadTimeMult = 0.6f;
                    break;
                case ItemID.StarCannon:
                case ItemID.RocketLauncher:
                    ReloadTimeMult = 2f;
                    break;
                case ItemID.SniperRifle:
                    ReloadTimeMult = 1.2f;
                    SkipStep = 1;
                    break;
                 
                    
                
            }

            Ammo = MaxAmmo;
            base.SetDefaults(item);
        }
        public override GlobalItem Clone(Item from, Item to)
        {
            to.GetGlobalItem(this).Ammo = from.GetGlobalItem(this).Ammo;
            to.GetGlobalItem(this).MaxAmmo = from.GetGlobalItem(this).MaxAmmo;
            to.GetGlobalItem(this).ReloadTimeMult = from.GetGlobalItem(this).ReloadTimeMult;
            to.GetGlobalItem(this).FullyReloads = from.GetGlobalItem(this).FullyReloads;
            to.GetGlobalItem(this).ReloadStep = from.GetGlobalItem(this).ReloadStep;
            to.GetGlobalItem(this).StoredSound = from.GetGlobalItem(this).StoredSound;
            to.GetGlobalItem(this).OriginalUseTime = from.GetGlobalItem(this).OriginalUseTime;
            to.GetGlobalItem(this).OriginalUseAnimation = from.GetGlobalItem(this).OriginalUseAnimation;
            to.GetGlobalItem(this).OriginalReuseDelay = from.GetGlobalItem(this).OriginalReuseDelay;
            return to.GetGlobalItem(this);
        }
    }
}
