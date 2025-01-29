using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
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

        public int OriginalUseTime;
        public int OriginalUseAnimation;
        public int OriginalReuseDelay;

        //dont know what this method is really for but it is necessary
        public override void SetDefaults(Item entity)
        {
            OriginalUseTime = entity.useTime;
            OriginalUseAnimation = entity.useAnimation;
            OriginalReuseDelay = entity.reuseDelay;
            Ammo = MaxAmmo;
            StoredSound = entity.UseSound;
            entity.UseSound = null;
            base.SetDefaults(entity);
        }
        public override GlobalItem Clone(Item from, Item to)
        {
            to.GetGlobalItem(this).Ammo = from.GetGlobalItem(this).Ammo;
            to.GetGlobalItem(this).MaxAmmo = from.GetGlobalItem(this).MaxAmmo;
            to.GetGlobalItem(this).ReloadTimeMult = from.GetGlobalItem(this).ReloadTimeMult;
            to.GetGlobalItem(this).FullyReloads = from.GetGlobalItem(this).FullyReloads;
            to.GetGlobalItem(this).ReloadStep = from.GetGlobalItem(this).ReloadStep;
            to.GetGlobalItem(this).StoredSound = from.GetGlobalItem(this).StoredSound;
            return to.GetGlobalItem(this);
        }
    }
}
