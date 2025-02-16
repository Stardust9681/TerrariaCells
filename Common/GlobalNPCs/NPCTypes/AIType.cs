using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;

using MonoMod.Cil;
using Mono.Cecil.Cil;
using System.Reflection;

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
		public virtual bool FindFrame(NPC npc) => true;
		public virtual bool PreDraw(NPC npc, SpriteBatch spritebatch, Vector2 screenPos, Color lightColor) { return true; }
	}

	internal class AITypeHandler : GlobalNPC
	{
		public override void Load()
		{
			On_NPC.VanillaFindFrame += On_FindFrame;
			IL_NPC.StrikeNPC_HitInfo_bool_bool += IL_StrikeNPC;
		}

		public override void Unload()
		{
			On_NPC.VanillaFindFrame -= On_FindFrame;
			IL_NPC.StrikeNPC_HitInfo_bool_bool -= IL_StrikeNPC;
		}

		//Go decompile NPC.StrikeNPC(NPC.HitInfo, bool, bool) using ilSpy (or dnSpy hell if I care)
		//And you tell me why I have to make this IL edit
		private void IL_StrikeNPC(ILContext context)
		{
			log4net.ILog GetInstanceLogger() => ModContent.GetInstance<TerrariaCells>().Logger;
			try
			{
				ILCursor cursor = new ILCursor(context);

				if (!cursor.TryGotoNext(MoveType.Before,
					i => i.MatchLdarg0(), //NPC self
					i => i.MatchLdfld<NPC>("type"), //Int32 self.type
					i => i.MatchLdcI4(438), //Push 438 onto stack
					i => i.Match(OpCodes.Beq_S))) //if(self.type == 438 ...
				{
					//Couldn't match given instructions, perform no further edits
					GetInstanceLogger().Error($"Couldn't match IL Patch: {context.Method.Name} @ {cursor.Index}");
					return;
				}
				ILLabel? IL_0161 = cursor.MarkLabel(); //IL Instruction 0161 (by ilSpy)
				/*if (IL_0161 == null)
				{
					//Matched correctly but didn't get Label ???
					GetInstanceLogger().Error($"IL Label {nameof(IL_0161)} not found in IL Patch {context.Method.Name} @ {cursor.Index}");
					return;
				}*/

				/*ILLabel? IL_01C9 = null; //IL Instruction 01C9 (by ilSpy)
				if (!cursor.TryGotoNext(MoveType.Before,
						i => i.MatchLdarg0(), //NPC self
						i => i.MatchLdfld<NPC>("townNPC"), //bool self.townNPC
						i => i.MatchBrfalse(out _))) //if(self.townNPC)
				{
					//Couldn't match given instructions, perform no further edits
					GetInstanceLogger().Error($"Couldn't match IL Patch: {context.Method.Name} @ {cursor.Index}");
					return;
				}
				if (IL_01C9 == null)
				{
					//Matched correctly but didn't get Label ???
					GetInstanceLogger().Error($"IL Label {nameof(IL_01C9)} not found in IL Patch {context.Method.Name} @ {cursor.Index}");
					return;
				}*/

				
				if (!cursor.TryGotoNext(MoveType.Before,
						i => i.MatchLdarg0(), //NPC self
						i => i.MatchLdfld<NPC>("immortal"), //bool self.immortal
						i => i.Match(OpCodes.Brtrue_S))) //if(!self.immortal)
				{
					//Couldn't match given instructions, perform no further edits
					GetInstanceLogger().Error($"Couldn't match IL Patch: {context.Method.Name} @ {cursor.Index}");
					return;
				}
				ILLabel? IL_04A1 = cursor.MarkLabel(); //IL Instruction 04A1 (by ilSpy)
				/*if (IL_04A1 == null)
				{
					//Matched correctly but didn't get Label ???
					GetInstanceLogger().Error($"IL Label {nameof(IL_04A1)} not found in IL Patch {context.Method.Name} @ {cursor.Index}");
					return;
				}*/

				cursor.GotoLabel(IL_0161, MoveType.Before);
				cursor.Emit(OpCodes.Br, IL_04A1);
			}
			catch (Exception x)
			{
				//Something went wrong! :O
				GetInstanceLogger().Error($"Something went wrong with IL Patch: {context.Method.Name}");
				MonoModHooks.DumpIL(ModContent.GetInstance<TerrariaCells>(), context);
			}
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
	}
}
