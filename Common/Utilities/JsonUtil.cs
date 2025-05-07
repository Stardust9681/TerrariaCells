using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Terraria.ModLoader;

namespace TerrariaCells.Common.Utilities
{
	public static class JsonUtil
	{
		/// <summary>
		/// Safe getter for JSON values from <paramref name="token"/>
		/// </summary>
		/// <typeparam name="T">Type to deserialise JSON into</typeparam>
		/// <param name="obj"></param>
		/// <param name="key">Name of item</param>
		/// <param name="def">Default value if not found</param>
		/// <returns></returns>
		public static T? GetItem<T>(this JToken token, string key, T defaultValue = default(T))
		{
			if (token is not JObject obj) return defaultValue;

			if (obj.TryGetValue(key, out JToken? result)) return result.ToObject<T>();

			ModContent.GetInstance<TerrariaCells>().Logger.Warn($"JSON: Token does not contain a value for '{key}' at '{obj.Path}'");
			return defaultValue;
		}
	}
}
