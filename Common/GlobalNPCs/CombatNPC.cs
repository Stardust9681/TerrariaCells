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

		public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			switch (npc.type)
			{
				case NPCID.BrainofCthulhu:
					int timer = npc.Timer();

					if (timer > NPCTypes.BrainOfCthulhu.XTimeStart - 60 && timer < NPCTypes.BrainOfCthulhu.XTimeStart)
					{
						Vector2 centre = NPCTypes.BrainOfCthulhu.GetArenaCentre(npc);
						Vector2 offset = NPCTypes.BrainOfCthulhu.ArenaSize * 0.5f;
						Vector2[] positions = new Vector2[] {
						centre + (offset * -1),
						centre + offset,
						centre + (offset * new Vector2(1, -1)),
						centre + (offset * new Vector2(-1, 1)),
						};
						for (int i = 0; i < positions.Length; i++)
						{
							//int opposingIndex = (2 * (i / 2)) - (i % 2) + 1;
							//Vector2 opposingPosition = positions[opposingIndex];
							Vector2 pos = positions[i];
							Color c = Color.Red * 0.75f;
							float res = NPCHelpers.Pack(pos.ToTileCoordinates16());
							if (res.Equals(npc.ai[2]))
							{
								c = Color.Yellow * 0.75f;
							}
							Terraria.Utils.DrawLine(spriteBatch, pos, centre, c, Color.Transparent, ((NPCTypes.BrainOfCthulhu.XTimeStart - timer) / 60f) * 8);
						}
					}
					if (NPCTypes.BrainOfCthulhu.XTimeStart < timer && timer < NPCTypes.BrainOfCthulhu.XTimeEnd)
					{
						float percent = TCellsUtils.GetLerpValue(timer, (NPCTypes.BrainOfCthulhu.XTimeEnd - NPCTypes.BrainOfCthulhu.XTimeStart), TCellsUtils.LerpEasing.InOutBack, NPCTypes.BrainOfCthulhu.XTimeStart, false);
						Vector2 centre = NPCTypes.BrainOfCthulhu.GetArenaCentre(npc);
						Vector2 size = NPCTypes.BrainOfCthulhu.ArenaSize;
						Vector2 topLeft = centre - (size * 0.5f);
						Vector2 botLeft = topLeft + new Vector2(0, size.Y);
						Vector2 topRight = topLeft + new Vector2(size.X, 0);
						Vector2 botRight = topLeft + size;
						ReLogic.Content.Asset<Texture2D> brain = Terraria.GameContent.TextureAssets.Npc[NPCID.BrainofCthulhu];
						spriteBatch.Draw(brain.Value, Vector2.Lerp(botLeft, topRight, percent) - (npc.Size * 0.5f) - screenPos, npc.frame, drawColor * npc.Opacity);
						spriteBatch.Draw(brain.Value, Vector2.Lerp(botRight, topLeft, percent) - (npc.Size * 0.5f) - screenPos, npc.frame, drawColor * npc.Opacity);
						spriteBatch.Draw(brain.Value, Vector2.Lerp(topRight, botLeft, percent) - (npc.Size * 0.5f) - screenPos, npc.frame, drawColor * npc.Opacity);
					}
					return true;
				default:
					return base.PreDraw(npc, spriteBatch, screenPos, drawColor);
			}
		}



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
                    npc.lifeMax = 20;
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
                    npc.lifeMax = 30;
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

                // Jungle
                case NPCID.Hornet:
                    npc.defense = 0;
                    break;
            }
		}
	}
}
