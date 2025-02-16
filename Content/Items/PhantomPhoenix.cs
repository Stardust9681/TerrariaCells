using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaCells.Content.Items
{
    public class PhantomPhoenix : ModItem
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.DD2PhoenixBow;
        public override void SetDefaults()
        {
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAnimation = 18;
            Item.useTime = 18;
            Item.shootSpeed = 20f;
            Item.knockBack = 2f;
            Item.width = 20;
            Item.height = 12;
            Item.damage = 10;
            Item.UseSound = SoundID.Item5;
            Item.shoot = ProjectileID.FireArrow;
            Item.rare = ItemRarityID.Pink;
            Item.value = Terraria.Item.sellPrice(0, 1);
            Item.noMelee = true;
            Item.DamageType = DamageClass.Ranged;
            Item.autoReuse = true;
        }
        
    }
}
