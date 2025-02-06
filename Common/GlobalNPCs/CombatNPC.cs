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

		public override void SetDefaults(NPC npc)
		{
			switch (npc.type)
			{
				case NPCID.BrainofCthulhu:
					npc.knockBackResist = 0f; //0 effect from knockback
					break;
			}

			SetEnemyStats(npc);
		}

		public override bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot)
		{
			if (!allowContactDamage) return false;
			return base.CanHitPlayer(npc, target, ref cooldownSlot);
		}

		public static void ToggleContactDamage(NPC npc, bool value) => npc.GetGlobalNPC<CombatNPC>().allowContactDamage = value;

		//Just so it's not taking space at the top of the file tbh
		private void SetEnemyStats(NPC npc)
		{
			switch (npc.type)
			{
				// Forest
				case NPCID.Wolf:
					npc.lifeMax = 120;
					npc.damage = 30;
					npc.defense = 0;
					break;
                case NPCID.Raven:
                    npc.lifeMax = 20;
                    npc.damage = 20;
                    npc.defense = 0;
                    break;
                case NPCID.GoblinArcher:
                    npc.lifeMax = 30;
                    npc.damage = 25;
                    npc.defense = 0;
                    break;
                case NPCID.GoblinThief:
                    npc.lifeMax = 40;
                    npc.damage = 30;
                    npc.defense = 0;
                    break;
                case NPCID.GoblinSorcerer:
                    npc.lifeMax = 10;
                    npc.damage = 20;
                    npc.defense = 0;
                    break;

                // Desert
                case NPCID.Mummy:
					npc.lifeMax = 200;
					npc.damage = 60;
					npc.defense = 0;
					break;
                case NPCID.DesertGhoul:
                    npc.lifeMax = 50;
                    npc.damage = 50;
                    npc.defense = 0;
					break;
				// Sand Poachers have two NPC IDs???
                case NPCID.DesertScorpionWalk:
                case NPCID.DesertScorpionWall:
                    npc.lifeMax = 150;
                    npc.damage = 40;
                    npc.defense = 0;
                    break;
                case NPCID.DesertDjinn:
                    npc.lifeMax = 55;
                    npc.damage = 40;
                    npc.defense = 0;
                    break;
                case NPCID.Vulture:
                    npc.lifeMax = 50;
                    npc.damage = 40;
                    npc.defense = 0;
                    break;

				// Frozen City
                case NPCID.CultistDevote:
					npc.lifeMax = 100;
					npc.damage = 40;
                    npc.defense = 0;
                    break;
				case NPCID.CultistArcherBlue:
					npc.lifeMax = 150;
					npc.damage = 40;
                    npc.defense = 0;
                    break;
				case NPCID.IceGolem:
					npc.lifeMax = 300;
					npc.damage = 40;
                    npc.defense = 0;
                    break;
				case NPCID.IceElemental:
					npc.lifeMax = 100;
					npc.damage = 30;
                    npc.defense = 0;
                    break;

                // Crimson
                case NPCID.Crimera:
                    npc.lifeMax = 80;
                    npc.defense = 0;
                    break;
                case NPCID.BloodJelly:
                    npc.lifeMax = 100;
                    npc.damage = 30;
                    npc.defense = 0;
                    break;
                case NPCID.BloodCrawler:
                    npc.defense = 0;
                    break;
                case NPCID.Crimslime:
                    npc.lifeMax = 150;
                    npc.defense = 0;
                    break;
				case NPCID.BrainofCthulhu:
					npc.lifeMax = (int)(npc.lifeMax * 1.3f); //This thing squishy as HEYLLLL
					npc.defense = 0;
					break;

                // Jungle
                case NPCID.Hornet:
                    npc.defense = 0;
                    break;
            }
		}
	}
}
