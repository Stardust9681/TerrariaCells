using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework.Graphics;
using TerrariaCells.Common.Utilities;

using static TerrariaCells.Common.Utilities.NPCHelpers;

namespace TerrariaCells.Common.GlobalNPCs
{
	public class CombatNPC : GlobalNPC
	{
		public override bool InstancePerEntity => true;
		public bool allowContactDamage = false;

		public override bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot)
		{
			if (!allowContactDamage) return false;
			return base.CanHitPlayer(npc, target, ref cooldownSlot);
		}

		public override Color? GetAlpha(NPC npc, Color drawColor)
		{
			Color? returnVal = base.GetAlpha(npc, drawColor);
			if (npc.dontTakeDamage) returnVal = Color.Lerp(drawColor, Color.DarkSlateGray * 0.67f, 0.5f);
			if (npc.GetGlobalNPC<CombatNPC>().allowContactDamage) returnVal = Color.Lerp(drawColor, Color.IndianRed * (drawColor.A / 255f), 0.3f);
			return returnVal;
		}

		public static void ToggleContactDamage(NPC npc, bool value) => npc.GetGlobalNPC<CombatNPC>().allowContactDamage = value;

		public override void SetStaticDefaults()
		{
			NPCID.Sets.ProjectileNPC[NPCID.Creeper] = true;
		}
		public override void SetDefaults(NPC npc)
		{
			switch (npc.type)
			{
				#region Forest
				case NPCID.Wolf:
					npc.lifeMax = 120;
					npc.damage = 30;
					break;
                case NPCID.Raven:
                    npc.lifeMax = 20;
                    npc.damage = 20;
                    break;
                case NPCID.GoblinArcher:
                    npc.lifeMax = 30;
                    npc.damage = 25;
                    break;
                case NPCID.GoblinThief:
                    npc.lifeMax = 40;
                    npc.damage = 30;
                    break;
                case NPCID.GoblinSorcerer:
                    npc.lifeMax = 10;
                    npc.damage = 20;
                    break;
				#endregion

				#region Desert
				case NPCID.Mummy:
					npc.lifeMax = 200;
					npc.damage = 60;
					break;
                case NPCID.DesertGhoul:
                    npc.lifeMax = 50;
                    npc.damage = 50;
					break;
				// Sand Poachers have two NPC IDs???
				//Yeah they do lmao. Relogic's fuckin hilarious amirite?
                case NPCID.DesertScorpionWalk:
                case NPCID.DesertScorpionWall:
                    npc.lifeMax = 150;
                    npc.damage = 40;
                    break;
                case NPCID.DesertDjinn:
                    npc.lifeMax = 55;
                    npc.damage = 40;
                    break;
                case NPCID.Vulture:
                    npc.lifeMax = 50;
                    npc.damage = 40;
                    break;
				#endregion

				#region Frozen City
				case NPCID.CultistDevote:
					npc.lifeMax = 100;
					npc.damage = 40;
					npc.chaseable = true;
                    break;
				case NPCID.CultistArcherBlue:
					npc.lifeMax = 150;
					npc.damage = 40;
					npc.chaseable = true;
					break;
				case NPCID.IceGolem:
					npc.lifeMax = 300;
					npc.damage = 40;
                    break;
				case NPCID.IceElemental:
					npc.lifeMax = 100;
					npc.damage = 30;
                    break;
				#endregion

				#region Crimson
				case NPCID.Crimera:
                    npc.lifeMax = 40;
                    break;
                case NPCID.BloodJelly:
                    npc.lifeMax = 80;
                    npc.damage = 30;
                    break;
                case NPCID.BloodCrawler:
					npc.buffImmune[BuffID.Poisoned] = false;
                    break;
                case NPCID.Crimslime:
					npc.buffImmune[BuffID.Poisoned] = false;
                    npc.lifeMax = 130;
					npc.damage = 40;
                    break;
				case NPCID.BrainofCthulhu:
					npc.lifeMax = (int)(npc.lifeMax * 1.3f); //This thing squishy as HEYLLLL
					npc.knockBackResist = 0f; //Takes 0 knockback
					break;
				case NPCID.Creeper:
					npc.lifeMax = 5;
					npc.knockBackResist = 0f;
					npc.defense = 4; //Make low damage projectile spam less effective at clearing low-health targets
					npc.scale = 1.4f;
					npc.damage = 45;
					return;
				#endregion

				#region Jungle
				case NPCID.Hornet:
                    break;
				#endregion

				//Do early return if you don't want enemy to have 0 defence
				//No point repeating the same line a bajillion times :)
				default:
					return;
			}
			npc.defense = 0;
		}
	}
}
