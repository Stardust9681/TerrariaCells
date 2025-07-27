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

			NPCID.Sets.TrailCacheLength[NPCID.Crimslime] = 10;
			NPCID.Sets.TrailingMode[NPCID.Crimslime] = 1;

            NPCID.Sets.SpecialSpawningRules[NPCID.DesertDjinn] = 0;

            NPCID.Sets.SpecificDebuffImmunity[NPCID.BloodCrawler][BuffID.Poisoned] = false;
            NPCID.Sets.SpecificDebuffImmunity[NPCID.BloodCrawlerWall][BuffID.Poisoned] = false;
            NPCID.Sets.SpecificDebuffImmunity[NPCID.Crimslime][BuffID.Poisoned] = false;
        }
		public override void SetDefaults(NPC npc)
		{
            //Some levels will lead to more than one *other* level. Using a/b/.. to direct path in comments
			switch (npc.type)
			{
                //Level 1
				#region Forest
				case NPCID.Wolf:
					npc.lifeMax = 70;
					npc.damage = 20;
					break;
                case NPCID.Raven:
                    npc.lifeMax = 20;
                    npc.damage = 15;
                    break;
                case NPCID.GoblinArcher:
                    npc.lifeMax = 35;
                    npc.damage = 15;
                    break;
                case NPCID.GoblinThief:
                    npc.lifeMax = 50;
                    npc.damage = 20;
                    break;
                case NPCID.GoblinSorcerer:
                    npc.lifeMax = 20;
                    npc.damage = 15;
                    break;
                #endregion

                //Level 2
                #region Crimson
                case NPCID.Crimera:
                    npc.lifeMax = 30;
                    break;
                case NPCID.Drippler:
                    npc.lifeMax = 60;
                    break;
                //No Blood Jellies present
                //case NPCID.BloodJelly:
                    //npc.lifeMax = 80;
                    //npc.damage = 30;
                    //break;
                case NPCID.BloodCrawler:
                    npc.lifeMax = 60;
                    break;
                case NPCID.BloodCrawlerWall:
                    npc.lifeMax = 60;
                    break;
                case NPCID.Crimslime:
                    npc.lifeMax = 140;
                    npc.damage = 30;
                    npc.knockBackResist = 0f;
                    break;
                case NPCID.BrainofCthulhu:
                    npc.lifeMax = 2000;
                    npc.knockBackResist = 0f; //Takes 0 knockback
                    break;
                case NPCID.Creeper:
                    npc.lifeMax = 5;
                    npc.knockBackResist = 0f;
                    npc.defense = 4; //Make ultra low damage projectile spam less effective at clearing low-health targets
                    npc.scale = 1.4f;
                    npc.damage = 45;
                    return;
                #endregion

                //Lebel 2
                #region Corruption
                case NPCID.EaterofSouls:
                    npc.lifeMax = 30;
                    break;
                #endregion

                //Level 3.a
                #region Desert
                case NPCID.Mummy:
					npc.lifeMax = 400;
					npc.damage = 80;
					break;
                case NPCID.DesertGhoul:
                    npc.lifeMax = 80;
                    npc.damage = 40;
					break;
				// Sand Poachers have two NPC IDs???
				//Yeah they do lmao. Relogic's fuckin hilarious amirite?
                case NPCID.DesertScorpionWalk:
                case NPCID.DesertScorpionWall:
                    npc.lifeMax = 175;
                    npc.damage = 35;
                    break;
                case NPCID.DesertDjinn:
                    npc.lifeMax = 80;
                    npc.damage = 40;
                    break;
                case NPCID.Vulture:
                    npc.lifeMax = 60;
                    npc.damage = 20;
                    break;
				#endregion

                //Level 3.b
                #region Frozen City
                case NPCID.CultistDevote:
                    npc.lifeMax = 100;
                    npc.damage = 50;
                    npc.chaseable = true;
                    break;
                case NPCID.CultistArcherBlue:
                    npc.lifeMax = 160;
                    npc.damage = 35;
                    npc.chaseable = true;
                    break;
                case NPCID.IceGolem:
                    npc.lifeMax = 600;
                    npc.damage = 50;
                    break;
                case NPCID.IceElemental:
                    npc.lifeMax = 120;
                    npc.damage = 35;
                    break;
                #endregion

                //Level 4.a.a
                #region Hive
                case NPCID.Hornet:
                    npc.lifeMax = 175;
                    break;
                #endregion

                //Level 4.b.a
                #region Dungeon
                case NPCID.DiabolistRed:
                case NPCID.DiabolistWhite:
                    npc.lifeMax = 125;
                    npc.damage = 75;
                    break;
                case NPCID.RaggedCaster:
                case NPCID.RaggedCasterOpenCoat:
                    npc.lifeMax = 125;
                    npc.damage = 60;
                    break;
                case NPCID.RustyArmoredBonesAxe:
                case NPCID.RustyArmoredBonesFlail:
                case NPCID.RustyArmoredBonesSword:
                case NPCID.RustyArmoredBonesSwordNoArmor:
                    npc.lifeMax = 400;
                    npc.damage = 60;
                    break;
                #endregion

                //Level 5
                #region Caverns
                case NPCID.GraniteFlyer: //Granite Elemental
                    npc.lifeMax = 200;
                    npc.damage = 80;
                    break;
                case NPCID.Skeleton:
                    npc.lifeMax = 300;
                    npc.damage = 60;
                    break;
                case NPCID.Tim:
                    npc.lifeMax = 250;
                    npc.damage = 60;
                    break;
                case NPCID.RockGolem:
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
