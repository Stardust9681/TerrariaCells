using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using TerrariaCells.Common.Systems;

namespace TerrariaCells.Common.GlobalNPCs.NPCTypes
{
	//Using ILoadable instead of IModType, because this isn't content, but this is still something I want loaded
	//Not sure if it would make sense to separate these into static classes w/ static methods instead?
		//But then we lose the super nice inheritence structure, the blueprint methods, etc
	//Anyway if you want to redo this fine, I just want a better system, honest.

	//~ Star

	public abstract class AIType : ILoadable
	{
		public void Load(Mod mod)
		{
			Load();
		}
		public virtual void Load() { }
		public virtual void Unload() { }

		public abstract bool AppliesToNPC(int npcType);
		public abstract void Behaviour(NPC npc);
		public virtual void FindFrame(NPC npc) { }
		public virtual bool PreDraw(NPC npc, SpriteBatch spritebatch, Vector2 screenPos, Color lightColor) { return true; }
	}

	internal class AITypeHandler : GlobalNPC
	{
		public override bool PreAI(NPC npc)
		{
			if (!AIOverwriteSystem.TryGetAIType(npc.type, out AIType ai))
				return base.PreAI(npc);
			ai.Behaviour(npc);
			return false;
		}

		public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			if (!AIOverwriteSystem.TryGetAIType(npc.type, out AIType ai))
				return base.PreDraw(npc, spriteBatch, screenPos, drawColor);
			return ai.PreDraw(npc, spriteBatch, screenPos, drawColor);
		}

		public override void FindFrame(NPC npc, int frameHeight)
		{
			if (!AIOverwriteSystem.TryGetAIType(npc.target, out AIType ai))
			{
				base.FindFrame(npc, frameHeight);
				return;
			}
			ai.FindFrame(npc);
		}
	}
}
