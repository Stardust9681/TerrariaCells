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
using Terraria.ModLoader.Core;
using Terraria.ModLoader.IO;
using System.IO;

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
        public virtual void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter) { }
        public virtual void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader) { }
		public virtual bool FindFrame(NPC npc, int frameHeight) => true;
		public virtual bool PreDraw(NPC npc, SpriteBatch spritebatch, Vector2 screenPos, Color lightColor) { return true; }
	}

	internal class AITypeHandler : GlobalNPC
	{
        internal static readonly FieldInfo NPCLoader_HookFindFrame = typeof(NPCLoader).GetField("HookFindFrame", BindingFlags.NonPublic | BindingFlags.Static);
        public override void Load()
		{
			IL_NPC.StrikeNPC_HitInfo_bool_bool += IL_StrikeNPC;
            IL_NPC.FindFrame += IL_NPC_FindFrame;
		}
        public override void Unload()
		{
			IL_NPC.StrikeNPC_HitInfo_bool_bool -= IL_StrikeNPC;
            IL_NPC.FindFrame -= IL_NPC_FindFrame;
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

				cursor.GotoLabel(IL_0161, MoveType.Before);
				cursor.Emit(OpCodes.Br, IL_04A1);

                ILLabel? exitCritKbBonus = default;
                if (!cursor.TryGotoNext(
                    MoveType.Before,
                    i => i.Match(OpCodes.Ldloc_2),
                    i => i.MatchBrfalse(out exitCritKbBonus),
                    i => i.Match(OpCodes.Ldloc_S, 8),
                    i => i.Match(OpCodes.Ldc_R4)))
                {
                    GetInstanceLogger().Error("Failed to patch crit knockback bonus");
                    return;
                }

                cursor.EmitBr(exitCritKbBonus);
			}
			catch (Exception x)
			{
				//Something went wrong! :O
				GetInstanceLogger().Error($"Something went wrong with IL Patch: {context.Method.Name}");
				MonoModHooks.DumpIL(ModContent.GetInstance<TerrariaCells>(), context);
			}
		}
        private void IL_NPC_FindFrame(ILContext context)
        {
            try
            {
                ILCursor cursor = new ILCursor(context);

                if (!cursor.TryGotoNext(
                    i => i.Match(OpCodes.Ldarg_0),
                    i => i.Match(OpCodes.Ldloc_0),
                    i => i.MatchCall(typeof(NPCLoader).FullName, nameof(NPCLoader.FindFrame))))
                {
                    return;
                }
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldloc_0);

                //NPCLoader.FindFrame(NPC, int)
                cursor.EmitDelegate((NPC npc, int frameHeight) => {
                    bool isLikeTownNPC = npc.isLikeATownNPC;
                    int? num = npc.ModNPC?.AnimationType;
                    int animationType = (num.HasValue && num.GetValueOrDefault() > 0) ? num.Value : npc.type;
                    bool shouldRunVanillAFrame = true;
                    if (AIOverwriteSystem.TryGetAIType(animationType, out AIType ai))
                    {
                        shouldRunVanillAFrame = ai.FindFrame(npc, frameHeight);
                    }
                    if (shouldRunVanillAFrame)
                    {
                        npc.VanillaFindFrame(frameHeight, isLikeTownNPC, animationType);
                    }

                    npc.ModNPC?.FindFrame(frameHeight);
                    EntityGlobalsEnumerator<GlobalNPC> enumerator = ((GlobalHookList<GlobalNPC>)NPCLoader_HookFindFrame.GetValue(null)).Enumerate(npc).GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.FindFrame(npc, frameHeight);
                    }
                });
                cursor.Emit(OpCodes.Ret);
            }
            catch (Exception x)
            {

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
        public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
        {
            if (!AIOverwriteSystem.TryGetAIType(npc.type, out AIType ai))
                return;
            ai.SendExtraAI(npc, bitWriter, binaryWriter);
        }
        public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
        {
            if (!AIOverwriteSystem.TryGetAIType(npc.type, out AIType ai))
                return;
            ai.ReceiveExtraAI(npc, bitReader, binaryReader);
        }
        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			if (!AIOverwriteSystem.TryGetAIType(npc.type, out AIType ai))
				return base.PreDraw(npc, spriteBatch, screenPos, drawColor);
			return ai.PreDraw(npc, spriteBatch, screenPos, drawColor);
		}

        public override bool? CanFallThroughPlatforms(NPC npc)
        {
            if (!AIOverwriteSystem.AITypeExists(npc.type))
                return base.CanFallThroughPlatforms(npc);
            return npc.stairFall;
        }
    }
}
