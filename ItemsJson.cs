using System;
using System.Collections.Generic;
using Terraria.ModLoader;
using System.IO;
using System.Linq;
using MonoMod.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Terraria.ID;
using TerrariaCells.Common.Utilities;

namespace TerrariaCells
{
    public class ItemsJson : ILoadable
    {
        public enum ItemCategory
        {
            //Used for any item that doesn't have an explicit categorisation restriction -- these will go into coin and storage slots
            Undefined,
            
            Weapons,
            Abilities,
            Accessories,
            Armor,
            Potions,
        }
        
        public static ItemsJson Instance { get; private set; }
        
        public IReadOnlyDictionary<ItemCategory, IReadOnlyList<int>> Loot;
        public IReadOnlyDictionary<int, ItemCategory> Category;
        public IReadOnlyDictionary<string, Func<IReadOnlyList<int>>> ChestOverrides;
        
        public void Load(Mod mod)
        {
            Instance = this;
            
            using(StreamReader stream = new StreamReader(mod.GetFileStream("Items.json")))
            {
                string json = stream.ReadToEnd();
                JObject root = (JObject)JsonConvert.DeserializeObject(json); //Get json contents in whole
                JToken loot = root.GetValue("Loot");
                Loot = new Dictionary<ItemCategory, IReadOnlyList<int>>()
                {
                    [ItemCategory.Weapons] = loot.GetItem<string[]>("Weapons").Select(x => int.TryParse(x, out int id) ? id : ItemID.Search.GetId(x)).ToArray(),
                    [ItemCategory.Abilities] = loot.GetItem<string[]>("Skills").Select(x => int.TryParse(x, out int id) ? id : ItemID.Search.GetId(x)).ToArray(),
                    [ItemCategory.Accessories] = loot.GetItem<string[]>("Accessories").Select(x => int.TryParse(x, out int id) ? id : ItemID.Search.GetId(x)).ToArray(),
                    [ItemCategory.Armor] = loot.GetItem<string[]>("Armor").Select(x => int.TryParse(x, out int id) ? id : ItemID.Search.GetId(x)).ToArray(),
                    [ItemCategory.Potions] = loot.GetItem<string[]>("Potions").Select(x => int.TryParse(x, out int id) ? id : ItemID.Search.GetId(x)).ToArray(),
                };

                
                
                Dictionary<int, ItemCategory> categories = new Dictionary<int, ItemCategory>();
                foreach(var kvp in Loot)
                {
                    foreach(int id in kvp.Value)
                    {
                        categories[id] = kvp.Key;
                    }
                }
                JToken category = root.GetValue("Category");
                Dictionary<ItemCategory, IReadOnlyList<int>> jsonCats = new Dictionary<ItemCategory, IReadOnlyList<int>>()
                {
                    [ItemCategory.Weapons] = category.GetItem<string[]>("Weapons").Select(x => int.TryParse(x, out int id) ? id : ItemID.Search.GetId(x)).ToArray(),
                    [ItemCategory.Abilities] = category.GetItem<string[]>("Skills").Select(x => int.TryParse(x, out int id) ? id : ItemID.Search.GetId(x)).ToArray(),
                    [ItemCategory.Accessories] = category.GetItem<string[]>("Accessories").Select(x => int.TryParse(x, out int id) ? id : ItemID.Search.GetId(x)).ToArray(),
                    [ItemCategory.Armor] = category.GetItem<string[]>("Armor").Select(x => int.TryParse(x, out int id) ? id : ItemID.Search.GetId(x)).ToArray(),
                    [ItemCategory.Potions] = category.GetItem<string[]>("Potions").Select(x => int.TryParse(x, out int id) ? id : ItemID.Search.GetId(x)).ToArray(),
                };
                foreach(var kvp in jsonCats)
                {
                    foreach(int id in kvp.Value)
                    {
                        categories[id] = kvp.Key;
                    }
                }
                Category = categories;

                
                
                Dictionary<string, Func<IReadOnlyList<int>>> jsonChestOverrides = new Dictionary<string, Func<IReadOnlyList<int>>>();
                Dictionary<string, object> overrides = root.GetItem<Dictionary<string, object>>("ChestOverrides");
                foreach(var kvp in overrides)
                {
                    if(kvp.Value is string s)
                    {
                        jsonChestOverrides.Add(kvp.Key, () => Loot[Enum.TryParse<ItemCategory>(s, out ItemCategory cat) ? cat : ItemCategory.Weapons]);
                    }
                    else if(kvp.Value is IEnumerable<string> e)
                    {
                        jsonChestOverrides.Add(kvp.Key, () => e.Select(x => int.TryParse(x, out int id) ? id : ItemID.Search.GetId(x)).ToList());
                    }
                }
                ChestOverrides = jsonChestOverrides;
            }
        }
        public void Unload()
        {
            Instance = null;
        }
    }
}