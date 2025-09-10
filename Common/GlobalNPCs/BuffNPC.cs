using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

using TerrariaCells.Common.ModPlayers;
using TerrariaCells.Common.Utilities;
using TerrariaCells.Content.Packets;

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
			On_NPC.UpdateNPC_BuffApplyVFX += On_NPC_UpdateNPC_BuffApplyVFX;
			On_NPC.Transform += On_NPC_Transform;
		}

		public override void Unload()
		{
			On_NPC.AddBuff -= On_NPC_AddBuff;
			On_NPC.UpdateNPC_BuffSetFlags -= On_NPC_UpdateNPC_BuffSetFlags;
			On_NPC.DelBuff -= On_NPC_DelBuff;
			On_NPC.UpdateNPC_BuffApplyVFX -= On_NPC_UpdateNPC_BuffApplyVFX;
			On_NPC.Transform -= On_NPC_Transform;
		}

		private static readonly HashSet<int> BuffsToClear = new HashSet<int>()
		{
			BuffID.OnFire,
			BuffID.CursedInferno,
			BuffID.Poisoned,
			BuffID.Venom,
			BuffID.Bleeding,
            BuffID.OnFire3,
		};
		public readonly int[] buffStacks = new int[NPC.maxBuffs];
		public readonly int[] buffOrigTimes = new int[NPC.maxBuffs];
		public bool bleeding = false;

		private static void On_NPC_AddBuff(On_NPC.orig_AddBuff orig, NPC self, int type, int time, bool quiet)
		{
            int stacksToAdd = 1;

            //Interactions aren't tracked on MP client. So you would hit an enemy, it wouldn't see you as having interacted with it, and then it wouldn't replace the buff
            //Stupid ass game
            if (self.AnyInteractions())
            {
                //int oldType = type;
                type = Main.player[self.lastInteraction].GetModPlayer<BuffPlayer>().GetBuffToApply(type, ref time, ref stacksToAdd);
                //ModContent.GetInstance<TerrariaCells>().Logger.Info($"(BUFF) Replaced {oldType} with {type}");
            }
            else if(Main.netMode != 2)
            {
                //int oldType = type;
                type = Main.LocalPlayer.GetModPlayer<BuffPlayer>().GetBuffToApply(type, ref time, ref stacksToAdd);
                //ModContent.GetInstance<TerrariaCells>().Logger.Info($"(BUFF) Replaced {oldType} with {type}");
            }
			orig.Invoke(self, type, time, quiet);

			int buffIndex = self.FindBuffIndex(type);
            if (buffIndex != -1 && buffIndex < NPC.maxBuffs)
			{
                if (!self.TryGetGlobalNPC<BuffNPC>(out BuffNPC buffNPC))
                    return;
                if (buffNPC.buffOrigTimes[buffIndex] < time)
					buffNPC.buffOrigTimes[buffIndex] = time;
				if (buffNPC.buffStacks[buffIndex] < 1)
					buffNPC.buffStacks[buffIndex] = stacksToAdd;
				else
					buffNPC.buffStacks[buffIndex] += stacksToAdd;

                if(!quiet)
                {
                    //ModContent.GetInstance<TerrariaCells>().Logger.Info($"(BUFF) Applied {type} to {self.TypeName} for {time} ({buffNPC.buffOrigTimes[buffIndex]}) with {stacksToAdd}");
                    if (Main.netMode == NetmodeID.MultiplayerClient)
                    {
                        buffNPC.NetSend_NewBuff(self, buffIndex);
                    }
                    else if(Main.netMode == NetmodeID.Server)
                    {
                        buffNPC.NetSend_BuffVars(self);
                    }
                }
            }
		}
		///Brought over from <see cref="Systems.VanillaClearingSystem"/>...
		//Adjusted to allow different behaviour with a stack counter
		private void On_NPC_UpdateNPC_BuffSetFlags(On_NPC.orig_UpdateNPC_BuffSetFlags orig, NPC self, bool lowerBuffTime)
		{
            if (!self.TryGetGlobalNPC<BuffNPC>(out BuffNPC buffNPC))
                return;

			for (int i = 0; i < NPC.maxBuffs; i++)
			{
				if (BuffsToClear.Contains(self.buffType[i]))
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
            if (!self.TryGetGlobalNPC<BuffNPC>(out BuffNPC buffNPC))
                return;

            if (BuffsToClear.Contains(self.buffType[buffIndex]))
			{
				//Check time bc NPC could gain sudden immunity (eg, to On Fire, while in Water)
				if (buffNPC.buffStacks[buffIndex] > 1 && self.buffTime[buffIndex] < 1)
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

            if (Main.netMode == NetmodeID.Server)
            {
                buffNPC.NetSend_BuffVars(self);
            }
        }

		//Buff stacks were being reduced to 0 by Blood Crawlers' `NPC.Transform(..)` call
		private static void On_NPC_Transform(On_NPC.orig_Transform orig, NPC self, int newType)
		{
            if (!self.TryGetGlobalNPC<BuffNPC>(out BuffNPC buffNPC))
                return;

            int[] oldBuffStacks = buffNPC.buffStacks;
			int[] oldBuffTimes = buffNPC.buffOrigTimes;
			orig.Invoke(self, newType);
			buffNPC = self.GetGlobalNPC<BuffNPC>();
			for (int i = 0; i < NPC.maxBuffs; i++)
			{
				buffNPC.buffStacks[i] = oldBuffStacks[i];
				buffNPC.buffOrigTimes[i] = oldBuffTimes[i];
			}

            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                buffNPC.NetSend_BuffVars(self);
            }
        }

		//Handles debuff VFX
		///Didn't use <see cref="GlobalNPC.DrawEffects(NPC, ref Color)"/> because I wanted to now *disable* vanilla VFX
		private void On_NPC_UpdateNPC_BuffApplyVFX(On_NPC.orig_UpdateNPC_BuffApplyVFX orig, NPC npc)
		{
			//Ensure debuff particles are enabled
			Configs.TerrariaCellsConfig.DebuffIndicators indicator = Configs.TerrariaCellsConfig.Instance.IndicatorType;
			if ((indicator & Configs.TerrariaCellsConfig.DebuffIndicators.Particles) == Configs.TerrariaCellsConfig.DebuffIndicators.None)
				return;

            if (!npc.TryGetGlobalNPC<BuffNPC>(out BuffNPC globalNPC))
                return;

            //Vanilla code
            npc.position += npc.netOffset;
            //

            for (int buffIndex = 0; buffIndex < NPC.maxBuffs; buffIndex++)
			{
				int buffType = npc.buffType[buffIndex];
				int buffTime = npc.buffTime[buffIndex];
				int buffStacks = globalNPC.buffStacks[buffIndex];

				if (buffTime < 1 || buffType == 0)
					continue;

				Dust dust;
				switch (buffType)
				{
					case BuffID.OnFire:
						int maxStrength_OF = Math.Min(buffStacks, 5);
						if (Main.rand.NextBool(8 - maxStrength_OF))
						{
							int height = (int)(npc.height * (maxStrength_OF * 0.2f));
							dust = Dust.NewDustDirect(npc.position + new Vector2(-2), npc.width + 4, height + 4, DustID.Torch, Alpha: 100);
							dust.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
							dust.scale = (MathF.Sin(dust.rotation) * 0.25f) + (1.5f + maxStrength_OF * 0.2f);
							dust.noGravity = true;
							dust.velocity = npc.velocity * 0.3f + Vector2.One.RotatedByRandom(MathHelper.TwoPi);
						}
						break;
					case BuffID.CursedInferno:
						int maxStrength_CI = Math.Min(buffStacks, 5);
						if (Main.rand.NextBool(8 - maxStrength_CI))
						{
							int height = (int)(npc.height * (maxStrength_CI * 0.2f));
							dust = Dust.NewDustDirect(npc.position + new Vector2(-2), npc.width + 4, npc.height + 4, DustID.CursedTorch, Alpha: 100);
							dust.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
							dust.scale = (MathF.Sin(dust.rotation) * 0.25f) + (2 + maxStrength_CI * 0.1f);
							dust.noGravity = true;
							dust.velocity = npc.velocity * 0.3f + Vector2.One.RotatedByRandom(MathHelper.TwoPi);
						}
						break;
                    case BuffID.OnFire3:
                        int maxStrength_OF3 = Math.Min(buffStacks, 6);
                        if (Main.rand.NextBool(10 - maxStrength_OF3))
                        {
                            dust = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.InfernoFork, Alpha: 100);
                            dust.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                            dust.scale = (MathF.Sin(dust.rotation) * 0.25f) + (2 + maxStrength_OF3 * 0.1f);
                            dust.noGravity = true;
                            dust.velocity = npc.velocity * 0.3f + Vector2.One.RotatedBy(MathHelper.TwoPi);
                        }
                        break;

					case BuffID.Poisoned:
						int maxStrength_P = Math.Min(buffStacks, 8);
						if (Main.rand.NextBool(14 - maxStrength_P))
						{
							dust = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.Poisoned, Alpha: 120, Scale: 0.2f);
							dust.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
							dust.velocity = Vector2.Zero;
							dust.fadeIn = 1.9f;
						}
						break;
					case BuffID.Venom:
						int maxStrength_V = Math.Min(buffStacks, 8);
						if (Main.rand.NextBool(12 - maxStrength_V))
						{
							dust = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.Venom, Alpha: 100, Scale: 0.5f);
							dust.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
							dust.velocity = Vector2.Zero;
							dust.fadeIn = 1.5f;
						}
						break;

					case BuffID.Bleeding:
						int maxStrength_B = Math.Min(buffStacks, 14);
						if (Main.rand.NextBool(14))
						{
							Vector2 pos = Vector2.Zero;
							Vector2 direction = pos;
							do //RARE USE OF DO-WHILE LOOP OMG!!!
							{
								pos = new Vector2(Main.rand.NextFloat(npc.width), Main.rand.NextFloat(npc.height)) + npc.position;
								direction = npc.Center.DirectionTo(pos);
							}
							while (direction.HasNaNs());

							for (int i = 0; i < maxStrength_B; i++)
							{
								dust = Dust.NewDustDirect(pos, 1, 1, DustID.Blood);
								dust.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
								dust.scale = Main.rand.NextFloat(1f, 1.4f);
								dust.velocity = direction.RotatedByRandom(MathHelper.ToRadians(10)) * (((float)i / (float)maxStrength_B) + 1f) * 2.5f;
								dust.noGravity = false;
							}
						}
						break;
				}
			}

			//Vanilla code
			npc.position -= npc.netOffset;
			//

			//Ensure flags are set appropriately
			bool was_onFire = npc.onFire;
			npc.onFire = false;
			bool was_onFire2 = npc.onFire2;
			npc.onFire2 = false;
            bool was_onFire3 = npc.onFire3;
            npc.onFire3 = false;
			bool was_poisoned = npc.poisoned;
			npc.poisoned = false;
			bool was_venom = npc.venom;
			npc.venom = false;

			orig.Invoke(npc);

			npc.onFire = was_onFire;
			npc.onFire2 = was_onFire2;
            npc.onFire3 = was_onFire3;
			npc.poisoned = was_poisoned;
			npc.venom = was_venom;
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
		//Set lower bound, increase to be DPS instead of DP2S, apply constant scale multiplier
		private static int Adjust(int orig)
		{
			return Math.Max((int)(1.5f * 2 * orig + 1), 1);
		}

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
                        npc.onFire = true;
                        if (!npc.HasBuff(BuffID.Oiled))
						{
							npc.lifeRegen -= Adjust(LinearScale(buffStacks, 1, 3));
						}
						else //Double efficacy with Oiled debuff
						{
							npc.lifeRegen -= 2 * Adjust(LinearScale(buffStacks, 1, 3));
							damage++;
						}
						break;
					case BuffID.CursedInferno:
						npc.onFire2 = true;
						npc.lifeRegen -= Adjust(LinearScale(buffStacks, 3, 5));
						damage += 4;
						break;
                    case BuffID.OnFire3:
                        npc.onFire3 = true;
                        npc.lifeRegen -= Adjust(LinearScale(buffStacks, 2, 3));
                        damage += 2;
                        break;

					//Front-Loaded DPS
					case BuffID.Poisoned:
						npc.poisoned = true;
						npc.lifeRegen -= Adjust(GeometricScale(buffStacks * 0.25f, 3f, 0.8125f));
						break;
					case BuffID.Venom:
						npc.venom = true;
						npc.lifeRegen -= Adjust(GeometricScale(buffStacks * 0.167f, 5.5f, 0.8125f));
						if (damage > 4)
							damage = damage * 3 / 4; //Tick slightly more frequently
						break;

					//Exponential-Scaling DPS
					case BuffID.Bleeding:
						npc.lifeRegen -= Adjust(ExponentialScale(buffStacks, 0.025f));
						if (damage > 2)
							damage /= 2; //Tick more frequently
                        if (damage < (buffStacks / 2))
                            damage = buffStacks / 2;
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

		//Handles debuff icons for enemies
		public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
            //Hide buffs when NPC can't display them (as with colour tinting)
            if (!npc.canDisplayBuffs)
                return;
			//Don't do this draw step if you have debuff icons disabled
			Configs.TerrariaCellsConfig.DebuffIndicators indicator = Configs.TerrariaCellsConfig.Instance.IndicatorType;
			if ((indicator & Configs.TerrariaCellsConfig.DebuffIndicators.Icon) == Configs.TerrariaCellsConfig.DebuffIndicators.None)
				return;

			List<(int BuffType, int BuffStacks)> BuffInfo = new List<(int BuffType, int BuffStacks)>();

			BuffNPC globalNPC = npc.GetGlobalNPC<BuffNPC>();
			for (int i = 0; i < NPC.maxBuffs; i++)
			{
				if (BuffsToClear.Contains(npc.buffType[i]))
				{
					BuffInfo.Add((npc.buffType[i], globalNPC.buffStacks[i]));
				}
			}

			Vector2 drawPos = npc.Center - screenPos;
			const int IconSize = 20;

			//24 px for each debuff, 4 px between each
			int xOffset = IconSize + 4;
			int yOffset = -Configs.TerrariaCellsConfig.Instance.EnemyDebuffOffset;
			if (yOffset > 0)
				drawPos.Y += npc.height / 2;
			else if(yOffset < 0) //else if, so yOffset = 0 will leave them in the centre
				drawPos.Y -= (npc.height / 2) + IconSize;

			for (int i = 0; i < BuffInfo.Count; i++)
			{
				if (!TextureAssets.Buff[BuffInfo[i].BuffType].IsLoaded)
					continue;

				Texture2D buffIcon = TextureAssets.Buff[BuffInfo[i].BuffType].Value;

				Vector2 drawOffset = new Vector2((i - (BuffInfo.Count * 0.5f)) * xOffset, yOffset);
				Vector2 pos = drawPos + drawOffset;
				Rectangle destination = new Rectangle((int)pos.X, (int)pos.Y, IconSize, IconSize);

				spriteBatch.Draw(buffIcon, destination, Color.White * Configs.TerrariaCellsConfig.Instance.EnemyDebuffOpacity);
				spriteBatch.DrawString(FontAssets.ItemStack.Value, $"{BuffInfo[i].BuffStacks}", pos + new Vector2(IconSize * .5f), Color.White * Configs.TerrariaCellsConfig.Instance.EnemyDebuffOpacity);
			}
		}

		//Helper method for applying debuffs consistently
		public static void AddBuff(NPC npc, int buffType, int time, int damage, int addStacks = 0)
		{
            if (!npc.TryGetGlobalNPC<BuffNPC>(out BuffNPC buffNPC))
                return;

            int stacksToAdd = (damage/9) + addStacks;
			stacksToAdd = Math.Max(stacksToAdd, 1);

            if (npc.AnyInteractions())
            {
                buffType = Main.player[npc.lastInteraction].GetModPlayer<BuffPlayer>().GetBuffToApply(buffType, ref time, ref stacksToAdd);
            }

			int buffIndex = npc.FindBuffIndex(buffType);
			if (buffIndex != -1 && buffIndex < NPC.maxBuffs)
			{
				if (buffNPC.buffOrigTimes[buffIndex] < time)
					buffNPC.buffOrigTimes[buffIndex] = time;
				npc.buffTime[buffIndex] = buffNPC.buffOrigTimes[buffIndex];
				buffNPC.buffStacks[buffIndex] += stacksToAdd;

                if (Main.netMode == NetmodeID.MultiplayerClient)
                {
                    buffNPC.NetSend_NewBuff(npc, buffIndex);
                }
            }
			else
			{
				npc.AddBuff(buffType, time, false);
				buffIndex = npc.FindBuffIndex(buffType);
				if (buffIndex != -1 && buffIndex < NPC.maxBuffs)
				{
					buffNPC.buffStacks[buffIndex] += stacksToAdd - 1;
					buffNPC.buffOrigTimes[buffIndex] = time;
				}

                if (Main.netMode == NetmodeID.MultiplayerClient)
                {
                    buffNPC.NetSend_BuffVars(npc);
                }
            }
		}

        private void NetSend_NewBuff(NPC npc, int buffIndex, int toClient = -1, int ignoreClient = -1)
        {
            ModPacket packet = ModNetHandler.GetPacket(ModContent.GetInstance<TerrariaCells>(), TCPacketType.BuffPacket);
            packet.Write((byte)BuffPacketHandler.BuffPacketType.AddBuff);
            packet.Write((byte)npc.whoAmI);
            packet.Write((byte)buffIndex);
            packet.Write7BitEncodedInt(this.buffOrigTimes[buffIndex]);
            packet.Write7BitEncodedInt(this.buffStacks[buffIndex]);
            packet.Send(toClient, ignoreClient);
        }
        private void NetSend_BuffVars(NPC npc, int toClient = -1, int ignoreClient = -1)
        {
            ModPacket packet = ModNetHandler.GetPacket(ModContent.GetInstance<TerrariaCells>(), TCPacketType.BuffPacket);
            packet.Write((byte)BuffPacketHandler.BuffPacketType.Buffs);
            packet.Write((byte)npc.whoAmI);
            for (int i = 0; i < NPC.maxBuffs; i++)
            {
                packet.Write7BitEncodedInt(this.buffOrigTimes[i]);
                packet.Write7BitEncodedInt(this.buffStacks[i]);
            }
            packet.Send(toClient, ignoreClient);
        }

        public void NetReceieve(NPC npc, BuffPacketHandler.BuffPacketType packetType, BinaryReader reader, int fromWho)
        {
            switch (packetType)
            {
                case BuffPacketHandler.BuffPacketType.AddBuff:
                    byte buffIndex = reader.ReadByte();
                    int origTime = reader.Read7BitEncodedInt();
                    int buffStacks = reader.Read7BitEncodedInt();
                    this.buffOrigTimes[buffIndex] = origTime;
                    this.buffStacks[buffIndex] = buffStacks;

                    if (Main.netMode == NetmodeID.Server)
                    {
                        NetSend_NewBuff(npc, buffIndex, -1, fromWho);
                    }
                    break;

                case BuffPacketHandler.BuffPacketType.Buffs:
                    for (int i = 0; i < NPC.maxBuffs; i++)
                    {
                        this.buffOrigTimes[i] = reader.Read7BitEncodedInt();
                        this.buffStacks[i] = reader.Read7BitEncodedInt();
                    }
                    break;
            }
        }
	}
}