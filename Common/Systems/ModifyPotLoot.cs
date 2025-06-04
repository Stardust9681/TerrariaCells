using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using TerrariaCells.Content.Tiles.LevelExitPylon;
using TerrariaCells.Common.Utilities;
using static TerrariaCells.Common.Utilities.JsonUtil;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace TerrariaCells.Common.Systems;

public class ModifyPotLoot : GlobalTile, IEntitySource
{
	const string ChestLootPoolsFileName = "pot loot tables.json";
	public string Context => "TerrariaCells.Common.Systems.ModifyPotLoot";
	//Dictionary of Dictionaries is pretty undesireable, but I really didn't feel like naming json fields :/
	public Dictionary<int, Dictionary<int, float>> potLootPools = new Dictionary<int, Dictionary<int, float>>();

	public override void Load()
	{
		Terraria.On_WorldGen.SpawnThingsFromPot += KillPotLoot;
	}
	public override void Unload()
	{
		Terraria.On_WorldGen.SpawnThingsFromPot -= KillPotLoot;
	}
	private void KillPotLoot(On_WorldGen.orig_SpawnThingsFromPot orig, int i, int j, int x2, int y2, int style) { }

	public override void SetStaticDefaults()
	{
		using (System.IO.StreamReader reader = new System.IO.StreamReader(Mod.GetFileStream(ChestLootPoolsFileName)))
		{
			string json = reader.ReadToEnd();
			JArray rootArr = (JArray)JsonConvert.DeserializeObject(json);
			JObject obj = (JObject)rootArr.First;

			Dictionary<int, float> pool = obj.GetItem<Dictionary<int, float>>("newLootPool");
			int[] styles = obj.GetItem<int[]>("styles");
			foreach (int s in styles)
				potLootPools[s] = pool;
		}
	}

	public override void KillTile(
		int i,
		int j,
		int type,
		ref bool fail,
		ref bool effectOnly,
		ref bool noItem
	)
	{
		if (type != TileID.Pots && type != TileID.PotsEcho)
		{
			return;
		}
		//tMod doesn't actually route this through vanilla pot loot
		//But I kept this here for Rubblemaker pots, so they won't drop their original materials (tragic)
		noItem = true;

		if (Main.tile[i, j].TileFrameX % 36 != 0)
		{
			return;
		}
		if (Main.tile[i, j].TileFrameY % 36 != 0)
		{
			return;
		}
		PotLoot(i, j, type);
	}

	public void PotLoot(int i, int j, int type)
	{
		int style = 0,
			alt = 0;

		TileObjectData.GetTileInfo(Main.tile[i, j], ref style, ref alt);

		Dictionary<int, float> pool = potLootPools[style];
		Terraria.Utilities.WeightedRandom<int> wRand = new Terraria.Utilities.WeightedRandom<int>(Main.rand);
		foreach (KeyValuePair<int, float> KVP in pool)
		{
			wRand.Add(KVP.Key, KVP.Value);
		}

		int itemType = wRand.Get();
		Item item = new Item(itemType);

		switch (itemType)
		{
			case ItemID.CopperCoin:
				item.stack = Main.rand.Next(40, 100);
				break;
			case ItemID.SilverCoin:
				item.stack = Main.rand.Next(51);
				break;

			case ItemID.Apple: //4009
				itemType = GlobalNPCs.DropFoodHeals.PickFoodItem();
				item = new Item(itemType);
				break;
		}

		Item.NewItem(this, Rectangle.Empty with { X = i * 16, Y = j * 16 }, item);
	}
}