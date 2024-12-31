using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaCells.Common.Globals
{
	public class HealthItem : GlobalItem
	{
		private const bool _DO_FOOD_HIGHLIGHTS = true;
		public override bool PreDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
		{
			if (_DO_FOOD_HIGHLIGHTS)
			{
				int strength = 0;
				switch (item.buffType)
				{
					case BuffID.WellFed:
						strength = 1;
						break;
					case BuffID.WellFed2:
						strength = 2;
						break;
					case BuffID.WellFed3:
						strength = 3;
						break;
					default:
						return true;
				}
				Texture2D glow = Terraria.GameContent.TextureAssets.Extra[98].Value;
				Terraria.DataStructures.DrawData data = new Terraria.DataStructures.DrawData(glow, item.Center - Main.screenPosition, Color.Wheat);
				data.origin = glow.Size() * 0.5f;
				data.scale = new Vector2(0.75f, 1.25f);
				for (int i = 0; i < strength; i++)
				{
					data.scale *= new Vector2(0.8f, 1.25f);
					data.rotation = item.timeSinceItemSpawned * ((int)(-0.75f * (i * i) + 2f)) * 0.02f;
					data.Draw(Main.spriteBatch);
				}
				//Main.spriteBatch.Draw(glow, item.Center - Main.screenPosition, null, Color.Wheat, item.timeSinceItemSpawned * 0.15707f, glow.Size() * 0.5f, new Vector2(((item.position.X % 4 + 4) * 0.0867f), ((item.position.Y % 8 + 8) * 0.0867f)), SpriteEffects.None, 0);
				return true;
			}
			else
			{
				return base.PreDrawInWorld(item, spriteBatch, lightColor, alphaColor, ref rotation, ref scale, whoAmI);
			}
		}

		public override Color? GetAlpha(Item item, Color lightColor)
		{
			if (_DO_FOOD_HIGHLIGHTS)
			{
				switch (item.buffType)
				{
					case BuffID.WellFed:
					case BuffID.WellFed2:
					case BuffID.WellFed3:
						return Color.White;
					default:
						return base.GetAlpha(item, lightColor);
				}
			}
			else
			{
				return base.GetAlpha(item, lightColor);
			}
		}

		public override void SetDefaults(Item item)
		{
			switch (item.type)
			{
				case ItemID.LesserHealingPotion:
					item.healLife = 75;
					break;
				case ItemID.HealingPotion:
					item.healLife = 150;
					break;
				case ItemID.GreaterHealingPotion:
					item.healLife = 250;
					break;
				default:
					break;
			}
			if (item.potion && item.healLife > 0)
			{
				//Sorbet said 3, I'm gonna set this to 4 for now though because 3 seems too limiting
				item.maxStack = 4;
			}
		}

		public override bool? UseItem(Item item, Player player)
		{
			if (item.potion && item.healLife > 0)
			{
				player.GetModPlayer<ModPlayers.Regenerator>().SetStaggerDamage(0);
				return true;
			}
			return base.UseItem(item, player);
		}
	}
}
