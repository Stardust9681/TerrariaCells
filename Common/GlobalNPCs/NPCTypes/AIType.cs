using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using TerrariaCells.Common.Systems;
using Terraria.DataStructures;

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
		public virtual bool FindFrame(NPC npc) => true;
		public virtual bool PreDraw(NPC npc, SpriteBatch spritebatch, Vector2 screenPos, Color lightColor) { return true; }
	}

	internal class AITypeHandler : GlobalNPC
	{
		public override void Load()
		{
			Terraria.On_NPC.VanillaFindFrame += On_FindFrame;
		}
		public override void Unload()
		{
			Terraria.On_NPC.VanillaFindFrame -= On_FindFrame;
		}

		//Okay, so here's the deal:
		//GlobalNPC.FindFrame(..) caused conflicts with NPC.VanillafindFrame(..) where the latter sort of overrode the former
		//And, look, they don't let you disable that either. So here's what I'm doing to fix that
		//Also: frameHeight param is literally useless in all situations. Not sure why it's ever included as a parameter for this function
		public void On_FindFrame(On_NPC.orig_VanillaFindFrame orig, NPC npc, int frameHeight, bool isLikeATownNPC, int typeToAnimateAs)
		{
			if (AIOverwriteSystem.TryGetAIType(npc.type, out AIType ai))
			{
				if (!ai.FindFrame(npc))
					return;
			}
			orig.Invoke(npc, frameHeight, isLikeATownNPC, typeToAnimateAs);
		}

		public override void OnSpawn(NPC npc, IEntitySource source)
		{
			npc.GetGlobalNPC<CombatNPC>().allowContactDamage = !AIOverwriteSystem.AITypeExists(npc.type);
		}

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

		public override Color? GetAlpha(NPC npc, Color drawColor)
		{
			Color? returnVal = base.GetAlpha(npc, drawColor);
			if (npc.dontTakeDamage) returnVal = Color.Lerp(drawColor, Color.DarkSlateGray * 0.67f, 0.5f);
			if (npc.GetGlobalNPC<CombatNPC>().allowContactDamage) returnVal = Color.Lerp(drawColor, Color.IndianRed * (drawColor.A/255f), 0.3f);
			return returnVal;
		}
	}
}
