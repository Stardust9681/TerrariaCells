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
using MonoMod.RuntimeDetour;
using System.Collections.Concurrent;
using Terraria.ID;

namespace TerrariaCells.Common.GlobalNPCs.NPCTypes.Shared
{
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
		public virtual bool FindFrame(NPC npc, int frameHeight) => true;
		public virtual bool PreDraw(NPC npc, SpriteBatch spritebatch, Vector2 screenPos, Color lightColor) { return true; }

        public virtual void ModifyNPCLoot(NPC npc, NPCLoot npcLoot) { }
	}

	internal class AITypeHandler : GlobalNPC
	{
		public override void Load()
		{
			IL_NPC.StrikeNPC_HitInfo_bool_bool += IL_StrikeNPC;
		}

		public override void Unload()
		{
			IL_NPC.StrikeNPC_HitInfo_bool_bool -= IL_StrikeNPC;
		}

		//Disable vanilla type-specific "on hit" modifications (eg, changing AI values when hit)
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

		//Find frame is finicky. Not sure what the problem is.
		private static void IL_FindFrame(ILContext context)
		{
			log4net.ILog GetInstanceLogger() => ModContent.GetInstance<TerrariaCells>().Logger;
			try
			{
				ILCursor cursor = new ILCursor(context);

				cursor.EmitLdarg0();
				cursor.EmitLdarg1();
				cursor.EmitDelegate((NPC npc, int frameHeight) => {
					if (AIOverwriteSystem.TryGetAIType(npc.type, out AIType ai))
					{
						if(!ai.FindFrame(npc, frameHeight))
						{
							npc.position -= npc.netOffset;
							return false;
						}
					}
					return true;
				});

				ILLabel label = cursor.DefineLabel();
				cursor.EmitBrtrue(label);
				cursor.EmitRet();
				cursor.MarkLabel(label);

				return;
				//One of several attempts to get this working
				//It's suddenly working again for me and I can't bloody tell why
				//And seemingly this isn't needed
				int offset = 0;
				for (int i = 0; i < cursor.Index; i++)
				{
					int instrSize = cursor.Instrs[i].GetSize();
					cursor.Instrs[i].Offset += offset;
					offset += instrSize;
				}
				for (int i = cursor.Index; i < context.Instrs.Count; i++)
				{
					cursor.Instrs[i].Offset += offset;
				}
			}
			catch (Exception x)
			{
				GetInstanceLogger().Error(x.Message);
			}
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

		public override void FindFrame(NPC npc, int frameHeight)
		{
			if (!AIOverwriteSystem.TryGetAIType(npc.type, out AIType ai))
				return;
			ai.FindFrame(npc, frameHeight);
		}

        public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
        {
            if (!AIOverwriteSystem.TryGetAIType(npc.type, out AIType ai))
            {
                return;
            }
            ai.ModifyNPCLoot(npc, npcLoot);
        }
	}
}
