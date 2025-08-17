using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using MonoMod.Cil;
using Mono.Cecil.Cil;

using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;

namespace TerrariaCells.Common.GlobalNPCs.NPCTypes.Shared
{
    //Took a little bit to wrap my head around how adding custom hooks works
    //Basically, if you want an NPC to have 'PreFindFrame,' have the respective class inherit 'PreFindFrame.INPC' or 'PreFindFrame.IGlobal'
    public class PreFindFrame : ILoadable
    {
        public interface INPC //Used for ModNPC
        {
            bool PreFindFrame(NPC npc, int frameHeight);// => true;
        }
        public interface IGlobal //Used for GlobalNPC
        {
            bool PreFindFrame(NPC npc, int frameHeight);// => true;
        }
        private static GlobalHookList<GlobalNPC> _hook;
        internal static bool Invoke(NPC npc, int frameHeight)
        {
            if (npc.ModNPC is INPC n)
            {
                if(!n.PreFindFrame(npc, frameHeight)) return false;
            }

            foreach (GlobalNPC g in _hook.Enumerate(npc))
            {
                if(g is not IGlobal ig) continue;

                if(!ig.PreFindFrame(npc, frameHeight)) return false;
            }
            return true;
        }
        public void Load(Mod mod)
        {
            _hook = NPCLoader.AddModHook(GlobalHookList<GlobalNPC>.Create(e => ((IGlobal)e).PreFindFrame));
        }
        public void Unload()
        {
            _hook = null;
        }
    }

    public class NPCTypeEdits : GlobalNPC
    {
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

        private void IL_StrikeNPC(ILContext context)
        {
            log4net.ILog log = ModContent.GetInstance<TerrariaCells>().Logger;
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
                    log.Error($"Couldn't match IL Patch: {context.Method.Name} @ {cursor.Index}");
                    return;
                }
                ILLabel? IL_0161 = cursor.MarkLabel(); //IL Instruction 0161 (by ilSpy)

                if (!cursor.TryGotoNext(MoveType.Before,
                        i => i.MatchLdarg0(), //NPC self
                        i => i.MatchLdfld<NPC>("immortal"), //bool self.immortal
                        i => i.Match(OpCodes.Brtrue_S))) //if(!self.immortal)
                {
                    //Couldn't match given instructions, perform no further edits
                    log.Error($"Couldn't match IL Patch: {context.Method.Name} @ {cursor.Index}");
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
                    log.Error("Failed to patch crit knockback bonus");
                    return;
                }

                cursor.EmitBr(exitCritKbBonus);
            }
            catch (Exception x)
            {
                //Something went wrong! :O
                log.Error($"Something went wrong with IL Patch: {context.Method.Name}");
                //MonoModHooks.DumpIL(ModContent.GetInstance<TerrariaCells>(), context);
            }
        }

        internal static readonly FieldInfo NPCLoader_HookFindFrame = typeof(NPCLoader).GetField("HookFindFrame", BindingFlags.NonPublic | BindingFlags.Static);
        private void IL_NPC_FindFrame(ILContext context)
        {
            log4net.ILog log = ModContent.GetInstance<TerrariaCells>().Logger;
            try
            {
                ILCursor cursor = new ILCursor(context);

                if (!cursor.TryGotoNext(
                    i => i.Match(OpCodes.Ldarg_0),
                    i => i.Match(OpCodes.Ldloc_0),
                    i => i.MatchCall(typeof(NPCLoader).FullName, nameof(NPCLoader.FindFrame))))
                {
                    log.Error($"Couldn't match IL Patch: {context.Method.Name} @ {cursor.Index}");
                    return;
                }
                cursor.Emit(OpCodes.Ldarg_0); //NPC npc
                cursor.Emit(OpCodes.Ldloc_0); //int num

                //NPCLoader.FindFrame(NPC, int)
                cursor.EmitDelegate(NPCLoader_FindFrame);
                cursor.Emit(OpCodes.Ret);
            }
            catch (Exception x)
            {
                log.Error($"Something went wrong with IL Patch: {context.Method.Name}");
            }
        }
        private static void NPCLoader_FindFrame(NPC npc, int frameHeight)
        {
            bool isLikeTownNPC = npc.isLikeATownNPC;
            int? num = npc.ModNPC?.AnimationType;
            int animationType = (num.HasValue && num.GetValueOrDefault() > 0) ? num.Value : npc.type;
            bool shouldRunVanillAFrame = PreFindFrame.Invoke(npc, frameHeight);
            if (shouldRunVanillAFrame)
            {
                npc.VanillaFindFrame(frameHeight, isLikeTownNPC, animationType);

                npc.ModNPC?.FindFrame(frameHeight);
                EntityGlobalsEnumerator<GlobalNPC> enumerator = ((GlobalHookList<GlobalNPC>)NPCLoader_HookFindFrame.GetValue(null)).Enumerate(npc);
                foreach (var g in enumerator)
                {
                    g.FindFrame(npc, frameHeight);
                }
            }
            else
            {
                npc.position -= npc.netOffset;
            }
        }
    }
}