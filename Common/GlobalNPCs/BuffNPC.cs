using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static System.Net.Mime.MediaTypeNames;

namespace TerrariaCells.Common.GlobalNPCs
{
	public class BuffNPC : GlobalNPC
	{
		public override bool InstancePerEntity => true;
		public override void Load()
		{
			On_NPC.AddBuff += On_NPC_AddBuff;
			On_NPC.UpdateNPC_BuffSetFlags += On_NPC_UpdateNPC_BuffSetFlags;
			On_NPC.DelBuff += On_NPC_DelBuff;
		}
		public override void Unload()
		{
			On_NPC.AddBuff -= On_NPC_AddBuff;
			On_NPC.UpdateNPC_BuffSetFlags -= On_NPC_UpdateNPC_BuffSetFlags;
			On_NPC.DelBuff -= On_NPC_DelBuff;
		}

		private static readonly HashSet<int> BuffsToClear = new HashSet<int>()
		{
			BuffID.OnFire,
			BuffID.Frostburn,
			BuffID.CursedInferno,
			BuffID.Poisoned,
			BuffID.Venom,
			BuffID.Bleeding
		};
		public readonly int[] buffStacks = new int[NPC.maxBuffs];
		public readonly int[] buffOrigTimes = new int[NPC.maxBuffs];
		public bool bleeding = false;

		private void On_NPC_AddBuff(On_NPC.orig_AddBuff orig, NPC self, int type, int time, bool quiet)
		{
			orig.Invoke(self, type, time, quiet);

			int buffIndex = self.FindBuffIndex(type);
			if (buffIndex != -1)
			{
				BuffNPC buffNPC = self.GetGlobalNPC<BuffNPC>();
				if (buffNPC.buffOrigTimes[buffIndex] < time)
					buffNPC.buffOrigTimes[buffIndex] = time;
				buffNPC.buffStacks[buffIndex]++;
			}
		}

		///Brought over from <see cref="Systems.VanillaClearingSystem"/>...
		///I actually want to retain visuals, and to adjust things *past* "removing" the buffs
		private void On_NPC_UpdateNPC_BuffSetFlags(On_NPC.orig_UpdateNPC_BuffSetFlags orig, NPC self, bool lowerBuffTime)
		{
			BuffNPC buffNPC = self.GetGlobalNPC<BuffNPC>();

			for (int i = 0; i < NPC.maxBuffs; i++)
			{
				if (self.buffType[i] > 0 && self.buffTime[i] > 0 && BuffsToClear.Contains(self.buffType[i]))
				{
					// negative buffs are ignored by the original method
					self.buffType[i] = -self.buffType[i];
					if (lowerBuffTime)
					{
						// this would get skipped inside the method otherwise
						//Just some function I picked to have more stacks last less time
						int framesToRemove = 1 + (int)(60 - (2400f / ((2400f / 60) + buffNPC.buffStacks[i] - 1)));
						self.buffTime[i] -= framesToRemove;
					}
				}
			}
			orig.Invoke(self, lowerBuffTime);
			// restore the original buffs after we've run orig
			for (int i = 0; i < NPC.maxBuffs; i++)
			{
				if (BuffsToClear.Contains(-self.buffType[i]))
				{
					self.buffType[i] = -self.buffType[i];
				}
			}
		}
		private void On_NPC_DelBuff(On_NPC.orig_DelBuff orig, NPC self, int buffIndex)
		{
			if (BuffsToClear.Contains(self.buffType[buffIndex]))
			{
				BuffNPC buffNPC = self.GetGlobalNPC<BuffNPC>();
				if (buffNPC.buffStacks[buffIndex] > 1)
				{
					buffNPC.buffStacks[buffIndex]--;
					self.buffTime[buffIndex] = buffNPC.buffOrigTimes[buffIndex];
					return;
				}
				orig.Invoke(self, buffIndex);
				for ( ; buffIndex < NPC.maxBuffs - 1; )
				{
					buffNPC.buffStacks[buffIndex] = buffNPC.buffStacks[++buffIndex];
					buffNPC.buffOrigTimes[buffIndex - 1] = buffNPC.buffOrigTimes[buffIndex];
				}
				buffNPC.buffStacks[buffIndex] = 0;
				buffNPC.buffOrigTimes[buffIndex] = 0;
				//Might need to sync this
			}
			else
			{
				orig.Invoke(self, buffIndex);
			}
		}

		//Literally just helper methods for DPS scaling
		private static int LinearScale(float x, int num, int den)
		{
			return (int)(x * num / den);
		}
		private static int GeometricScale(float x, float mult, float input)
		{
			return (int)(mult * input * (MathF.Pow(input, x) - 1) / (input - 1));
		}
		private static int ExponentialScale(float x, float mult)
		{
			return (int)(mult * MathF.Pow(x, 2));
		}

		///Re-adding buff flags here for VFX (NPC.UpdateNPC_BuffApplyVFX runs after NPC.UpdateNPC_BuffApplyDOTs)
		///In tML NPC.UpdateNPC_BuffApplyDOTs invokes UpdateLifeRegen here
		public override void UpdateLifeRegen(NPC npc, ref int damage)
		{
			BuffNPC buffNPC = npc.GetGlobalNPC<BuffNPC>();
			for(int buffIndex = 0; buffIndex < NPC.maxBuffs; buffIndex++)
			{
				if (npc.buffTime[buffIndex] < 1 || npc.buffType[buffIndex] < 1)
					continue;

				int buffType = npc.buffType[buffIndex];
				int buffStacks = buffNPC.buffStacks[buffIndex];
				if (buffStacks == 0)
					continue;

				//Multiplying DPS results by 2 for damage in 1 second instead of in 2
				//And adding 1 so the debuff actually still deals damage when scaling would otherwise reduce it below 1
				switch (buffType)
				{
					//Linear-Scaling DPS
					case BuffID.OnFire:
						if (!npc.HasBuff(BuffID.Oiled))
						{
							npc.onFire = true;
							npc.lifeRegen -= 2 * LinearScale(buffStacks, 1, 3) + 1;
						}
						else //Double efficacy with Oiled debuff
						{
							npc.onFire3 = true;
							npc.lifeRegen -= 2 * LinearScale(buffStacks, 2, 3) + 1;
							damage++;
						}
						break;
					case BuffID.CursedInferno:
						npc.onFire2 = true;
						npc.lifeRegen -= 2 * LinearScale(buffStacks, 3, 5) + 1;
						damage += 4;
						break;

					//Front-Loaded DPS
					case BuffID.Poisoned:
						npc.poisoned = true;
						npc.lifeRegen -= 2 * GeometricScale(buffStacks * 0.33f, 2.5f, 0.85f) + 1;
						break;
					case BuffID.Venom:
						npc.venom = true;
						npc.lifeRegen -= 2 * GeometricScale(buffStacks * 0.5f, 4f, 0.85f) + 1;
						if (damage > 4)
							damage = damage * 3 / 4; //Tick slightly more frequently
						break;

					//Exponential-Scaling DPS
					case BuffID.Bleeding:
						npc.lifeRegen -= 2 * ExponentialScale(buffStacks, 0.02f) + 1;
						if (damage > 2)
							damage /= 2; //Tick more frequently
						break;

					default:
						continue;
				}

				if (damage < 1)
					damage = 1;
				while (MathF.Abs(npc.lifeRegen/damage) > 6)
				{
					damage++;
				}
			}
		}
		public override void DrawEffects(NPC npc, ref Color drawColor)
		{
			if (npc.GetGlobalNPC<BuffNPC>().bleeding)
			{
				if (Main.rand.NextBool(12))
				{
					Dust blood = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.Blood);
					blood.velocity = Vector2.Zero;
					blood.noGravity = false;
					blood.scale = Main.rand.NextFloat(0.9f, 1.1f);
				}
			}
		}

		public static void AddBuff(NPC npc, int buffType, int time, int damage, int addStacks = 1)
		{
			BuffNPC buffNPC = npc.GetGlobalNPC<BuffNPC>();
			int stacksToAdd = (int)MathF.Sqrt(damage/6) + addStacks;
			int buffIndex = npc.FindBuffIndex(buffType);
			if (buffIndex != -1)
			{
				if (buffNPC.buffOrigTimes[buffIndex] < time)
					buffNPC.buffOrigTimes[buffIndex] = time;
				npc.buffTime[buffIndex] = buffNPC.buffOrigTimes[buffIndex];
				buffNPC.buffStacks[buffIndex] += stacksToAdd;
			}
			else
			{
				npc.AddBuff(buffType, time, false);
				buffIndex = npc.FindBuffIndex(buffType);
				buffNPC.buffStacks[buffIndex] += stacksToAdd - 1;
				buffNPC.buffOrigTimes[buffIndex] = time;
			}
		}
	}
}
