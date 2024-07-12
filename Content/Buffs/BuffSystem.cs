using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaCells.Common.GlobalNPCs;

namespace TerrariaCells.Content.Buffs
{
    //do special buff effects here
    //also holds variables for debuff strengthening/resistance
    public class PlayerBuffs : ModPlayer
    {
       
        public override void UpdateLifeRegen()
        {
            int dot = 0;
            //unfortunately terraria hardcodes player DoTs alongside beneficial life regen effects so we have to manually reverse all DoT Effects
            #region RemoveVanillaDoT
            if (Player.poisoned)
            {
                Player.lifeRegen += 4;
            }
            if (Player.venom)
            {
                Player.lifeRegen += 30;
            }
            if (Player.onFire)
            {
                Player.lifeRegen += 8;
            }
            if (Player.onFire3)
            {
                Player.lifeRegen += 8;
            }
            if (Player.onFrostBurn)
            {
                Player.lifeRegen += 16;
            }
            if (Player.onFrostBurn2)
            {
                Player.lifeRegen += 16;
            }
            if (Player.onFire2)
            {
                Player.lifeRegen += 24;
            }
            if (Player.burned)
            {
                Player.lifeRegen += 60;
                Player.moveSpeed /= 0.5f;
            }
            if (Player.suffocating)
            {
                Player.lifeRegen += 40;
            }
            if (Player.electrified)
            {
                Player.lifeRegen += 8;
                if (Player.controlLeft || Player.controlRight)
                {
                    Player.lifeRegen += 32;
                }
            }
            if (Player.tongued && Main.expertMode)
            {
                Player.lifeRegen += 100;
            }
            #endregion RemoveVanillaDoT
            for (int i = 0; i < BuffSystem.BuffInfo.Keys.Count; i++)
            {
                int id = BuffSystem.BuffInfo.Keys.ToList()[i];
                bool real = BuffSystem.BuffInfo.TryGetValue(id, out BuffData data);
                if (real && Player.HasBuff(id))
                {

                    if (data.PlayerDoT != null && (int)data.PlayerDoT != 0 && !data.CustomDoTLogic)
                    {
                        float multiplier = GlobalDebuffResist;
                        if (BuffData.HasTag(id, BuffData.Fire)) multiplier *= FireDebuffResist;
                        if (BuffData.HasTag(id, BuffData.Ice)) multiplier *= IceDebuffResist;
                        if (BuffData.HasTag(id, BuffData.Evil)) multiplier *= EvilDebuffResist;
                        if (BuffData.HasTag(id, BuffData.Holy)) multiplier *= HolyDebuffResist;
                        if (BuffData.HasTag(id, BuffData.Blood)) multiplier *= BloodDebuffResist;
                        if (BuffData.HasTag(id, BuffData.Poison)) multiplier *= PoisonDebuffResist;
                        if (BuffData.HasTag(id, BuffData.Natural)) multiplier *= NaturalDebuffResist;
                        if (BuffData.HasTag(id, BuffData.Electric)) multiplier *= ElectricDebuffResist;
                        dot -= (int)(data.PlayerDoT * multiplier * 2); //need to multiply intended dps by 2 because weird

                    }
                    //electrified is weird.
                    if (id == BuffID.Electrified)
                    {
                        dot -= (int)data.PlayerDoT * 2;
                        if (Player.controlLeft || Player.controlRight)
                        {
                            dot -= (int)data.PlayerDoT * 8;
                        }
                    }
                    //theres no other DoT debuffs applyable to players that do weird things thank god.
                    //but if you decide to make them do something, put the logic here.
                }
            }
            if (BuffData.HasAnyBuffWithTag(Player, BuffData.Fire) && Player.HasBuff(BuffID.Oiled))
            {
                dot -= 50; //oiled does +25 dps if you have any fire debuff
            }
            Player.lifeRegen += dot;
        }
        //debuffs of x type dealt by this player deal more/less damage
        public float GlobalDebuffMultiplier = 1;
        public float FireDebuffMultiplier = 1;
        public float IceDebuffMultiplier = 1;
        public float EvilDebuffMultiplier = 1;
        public float HolyDebuffMultiplier = 1;
        public float BloodDebuffMultiplier = 1;
        public float PoisonDebuffMultiplier = 1;
        public float NaturalDebuffMultiplier = 1;
        public float ElectricDebuffMultiplier = 1;

        //debuffs of x type deal more/less damage to the player
        public float GlobalDebuffResist = 1;
        public float FireDebuffResist = 1;
        public float IceDebuffResist = 1;
        public float EvilDebuffResist = 1;
        public float HolyDebuffResist = 1;
        public float BloodDebuffResist = 1;
        public float PoisonDebuffResist = 1;
        public float NaturalDebuffResist = 1;
        public float ElectricDebuffResist = 1;
    }
    public class BuffSystem : ModSystem
    {
        public static Dictionary<int, BuffData> BuffInfo;
        public override void Load()
        {
            //detour where debuff damage is applied in vanilla 
            On_NPC.UpdateNPC_BuffApplyDOTs += StopVanillaDoTMethod;

            //add buffdata for vanilla buffs. for modded buffs, set the dictionary slot for it in that buffs setstaticdefaults
            #region VanillaBuffData
            BuffInfo = new Dictionary<int, BuffData>();
            BuffInfo.Add(BuffID.OnFire, new BuffData(4, 4, [BuffData.Fire, BuffData.DamageOverTime]));
            BuffInfo.Add(BuffID.Bleeding, new BuffData(0, 0, [BuffData.Blood, BuffData.StatDown]));
            BuffInfo.Add(BuffID.Poisoned, new BuffData(6, 2, [BuffData.Poison, BuffData.Natural, BuffData.DamageOverTime]));
            BuffInfo.Add(BuffID.Venom, new BuffData(30, 15, [BuffData.Poison, BuffData.Natural, BuffData.DamageOverTime]));
            BuffInfo.Add(BuffID.CursedInferno, new BuffData(24, 12, [BuffData.Fire, BuffData.Evil, BuffData.DamageOverTime]));
            BuffInfo.Add(BuffID.Ichor, new BuffData(0, 0, [BuffData.Blood, BuffData.Evil, BuffData.StatDown]));
            BuffInfo.Add(BuffID.Frostburn, new BuffData(8, 8, [BuffData.Fire, BuffData.Ice, BuffData.DamageOverTime]));
            BuffInfo.Add(BuffID.Chilled, new BuffData(0, 0, [BuffData.Ice, BuffData.Slowness]));
            BuffInfo.Add(BuffID.Frozen, new BuffData(0, 0, [BuffData.Ice, BuffData.Freezing]));
            BuffInfo.Add(BuffID.Webbed, new BuffData(0, 0, [BuffData.Freezing]));
            BuffInfo.Add(BuffID.Stoned, new BuffData(0, 0, [BuffData.Freezing]));
            BuffInfo.Add(BuffID.Darkness, new BuffData(0, 0, [BuffData.Darkening]));
            BuffInfo.Add(BuffID.Blackout, new BuffData(0, 0, [BuffData.Darkening]));
            BuffInfo.Add(BuffID.Obstructed, new BuffData(0, 0, [BuffData.Darkening]));
            BuffInfo.Add(BuffID.Slow, new BuffData(0, 0, [BuffData.Slowness]));
            BuffInfo.Add(BuffID.TheTongue, new BuffData(0, 50, [BuffData.Freezing, BuffData.DamageOverTime]));
            BuffInfo.Add(BuffID.OgreSpit, new BuffData(0, 0, [BuffData.Slowness]));
            BuffInfo.Add(BuffID.Weak, new BuffData(0, 0, [BuffData.StatDown]));
            BuffInfo.Add(BuffID.BrokenArmor, new BuffData(0, 0, [BuffData.StatDown]));
            BuffInfo.Add(BuffID.WitheredArmor, new BuffData(0, 0, [BuffData.StatDown]));
            BuffInfo.Add(BuffID.WitheredWeapon, new BuffData(0, 0, [BuffData.StatDown]));
            BuffInfo.Add(BuffID.Electrified, new BuffData(8, 4, [BuffData.Electric, BuffData.DamageOverTime], true));
            //feral bite
            BuffInfo.Add(148, new BuffData(0, 0, [BuffData.StatDown]));
            BuffInfo.Add(BuffID.Suffocation, new BuffData(20, 20, [BuffData.DamageOverTime]));
            BuffInfo.Add(BuffID.Burning, new BuffData(50, 50, [BuffData.Fire, BuffData.DamageOverTime]));
            BuffInfo.Add(BuffID.Tipsy, new BuffData(0, 0, [BuffData.StatDown]));
            //Dazed (unused in vanilla)
            BuffInfo.Add(160, new BuffData(0, 0, [BuffData.Slowness]));
            BuffInfo.Add(BuffID.ShadowFlame, new BuffData(15, 7, [BuffData.Fire, BuffData.DamageOverTime]));
            BuffInfo.Add(BuffID.BetsysCurse, new BuffData(0, 0, [BuffData.StatDown]));
            //Penetrated (bone javeline debuff)
            BuffInfo.Add(169, new BuffData(3, 3, [BuffData.DamageOverTime], true));
            BuffInfo.Add(BuffID.Daybreak, new BuffData(100, 50, [BuffData.Fire, BuffData.DamageOverTime], true));
            //celled (stardust cell minion)
            BuffInfo.Add(183, new BuffData(20, 20, [BuffData.DamageOverTime], true));
            BuffInfo.Add(BuffID.DryadsWardDebuff, new BuffData(4, 0, [BuffData.DamageOverTime, BuffData.Natural], true));
            //hellfire
            BuffInfo.Add(BuffID.OnFire3, new BuffData(15, 7, [BuffData.Fire, BuffData.DamageOverTime]));
            //frostbite
            BuffInfo.Add(BuffID.Frostburn2, new BuffData(25, 13, [BuffData.Ice, BuffData.DamageOverTime]));
            BuffInfo.Add(BuffID.BloodButcherer, new BuffData(4, 4, [BuffData.Blood, BuffData.DamageOverTime], true));
            #endregion VanillaBuffData
        }
        

        //detour method. lots of redoing what vanilla does, unfortuneatly necessary
        private void StopVanillaDoTMethod(On_NPC.orig_UpdateNPC_BuffApplyDOTs orig, NPC self)
        {
            int dot = 0;
            for (int i = 0; i < BuffInfo.Keys.Count; i++)
            {
                int id = BuffInfo.Keys.ToList()[i];
                bool real = BuffInfo.TryGetValue(id, out BuffData data);
                BuffNPC buffNPC = self.GetGlobalNPC<BuffNPC>();
                //im so sorry
                float multiplier = buffNPC.GlobalDebuffResist;
                if (BuffData.HasTag(id, BuffData.Fire)) multiplier *= buffNPC.FireDebuffResist;
                if (BuffData.HasTag(id, BuffData.Ice)) multiplier *= buffNPC.IceDebuffResist;
                if (BuffData.HasTag(id, BuffData.Evil)) multiplier *= buffNPC.EvilDebuffResist;
                if (BuffData.HasTag(id, BuffData.Holy)) multiplier *= buffNPC.HolyDebuffResist;
                if (BuffData.HasTag(id, BuffData.Blood)) multiplier *= buffNPC.BloodDebuffResist;
                if (BuffData.HasTag(id, BuffData.Poison)) multiplier *= buffNPC.PoisonDebuffResist;
                if (BuffData.HasTag(id, BuffData.Natural)) multiplier *= buffNPC.NaturalDebuffResist;
                if (BuffData.HasTag(id, BuffData.Electric)) multiplier *= buffNPC.ElectricDebuffResist;
                for (int d = 0; d < buffNPC.PlayerTags.Length; i++)
                {
                    if (buffNPC.PlayerTags[i] > 0)
                    {
                        PlayerBuffs player = Main.player[i].GetModPlayer<PlayerBuffs>();
                        if (BuffData.HasTag(id, BuffData.Fire)) multiplier *= player.FireDebuffMultiplier;
                        if (BuffData.HasTag(id, BuffData.Ice)) multiplier *= player.IceDebuffMultiplier;
                        if (BuffData.HasTag(id, BuffData.Evil)) multiplier *= player.EvilDebuffMultiplier;
                        if (BuffData.HasTag(id, BuffData.Holy)) multiplier *= player.HolyDebuffMultiplier;
                        if (BuffData.HasTag(id, BuffData.Blood)) multiplier *= player.BloodDebuffMultiplier;
                        if (BuffData.HasTag(id, BuffData.Poison)) multiplier *= player.PoisonDebuffMultiplier;
                        if (BuffData.HasTag(id, BuffData.Natural)) multiplier *= player.NaturalDebuffMultiplier;
                        if (BuffData.HasTag(id, BuffData.Electric)) multiplier *= player.ElectricDebuffMultiplier;
                    }
                }

                if (real && self.HasBuff(id))
                {
                    if (data.DoT != 0 && dot == 0 && self.lifeRegen > 0)
                        self.lifeRegen = 0; // set life regen to 0 if above 0 only once
                    if (data.DoT != 0 && !data.CustomDoTLogic)
                    {

                        dot -= (int)(data.DoT * multiplier * 2); //need to multiply intended dps by 2 because weird
                    }
                    //custom debuff function
                    #region CustomDoTFunction
                    if (id == BuffID.Electrified)
                    {
                        dot -= (int)(data.DoT * multiplier * 2);
                        //idk random velocity number check to make electrified do more damage.
                        //electrified doesnt apply to selfs ever in vanilla so nothing to base off of
                        if (self.velocity.Length() >= 7) dot -= (int)(data.DoT * multiplier * 2);
                    }
                    //bone javeline debuff
                    if (id == 169 && self.javelined)
                    {
                        int numJavelines = 0;
                        for (int p = 0; p < Main.maxProjectiles; p++)
                        {
                            if (Main.projectile[p].active && Main.projectile[p].type == ProjectileID.BoneJavelin && Main.projectile[p].ai[0] == 1f && Main.projectile[p].ai[1] == self.whoAmI)
                            {
                                numJavelines++;
                            }
                        }
                        dot -= numJavelines * (int)(data.DoT * multiplier * 2);
                    }
                    if (id == BuffID.BloodButcherer)
                    {
                        int numBlood = 0;
                        for (int k = 0; k < Main.maxProjectiles; k++)
                        {
                            if (Main.projectile[k].active && Main.projectile[k].type == ProjectileID.BloodButcherer && Main.projectile[k].ai[0] == 1f && Main.projectile[k].ai[1] == self.whoAmI)
                            {
                                numBlood++;
                            }
                        }
                        dot -= numBlood * (int)(data.DoT * multiplier * 2);
                    }
                    if (id == BuffID.Daybreak)
                    {
                        int numDaybreak = 0;
                        for (int l = 0; l < Main.maxProjectiles; l++)
                        {
                            if (Main.projectile[l].active && Main.projectile[l].type == ProjectileID.Daybreak && Main.projectile[l].ai[0] == 1f && Main.projectile[l].ai[1] == self.whoAmI)
                            {
                                numDaybreak++;
                            }
                        }
                        if (numDaybreak == 0)
                        {
                            numDaybreak = 1;
                        }
                        dot -= numDaybreak * (int)(data.DoT * multiplier * 2);
                    }
                    //celled
                    if (id == 183)
                    {
                        int num10 = 0;
                        for (int m = 0; m < Main.maxProjectiles; m++)
                        {
                            if (Main.projectile[m].active && Main.projectile[m].type == ProjectileID.StardustCellMinionShot && Main.projectile[m].ai[0] == 1f && Main.projectile[m].ai[1] == self.whoAmI)
                            {
                                num10++;
                            }
                        }
                        dot -= num10 * (int)(data.DoT * multiplier * 2);
                    }
                    //copy pasted from vanilla (i hate it)
                    if (id == BuffID.DryadsWardDebuff)
                    {
                        int baseDoT = data.DoT;
                        float dryadmultiplier = 1f;
                        if (NPC.downedBoss1)
                        {
                            dryadmultiplier += 0.1f;
                        }
                        if (NPC.downedBoss2)
                        {
                            dryadmultiplier += 0.1f;
                        }
                        if (NPC.downedBoss3)
                        {
                            dryadmultiplier += 0.1f;
                        }
                        if (NPC.downedQueenBee)
                        {
                            dryadmultiplier += 0.1f;
                        }
                        if (Main.hardMode)
                        {
                            dryadmultiplier += 0.4f;
                        }
                        if (NPC.downedMechBoss1)
                        {
                            dryadmultiplier += 0.15f;
                        }
                        if (NPC.downedMechBoss2)
                        {
                            dryadmultiplier += 0.15f;
                        }
                        if (NPC.downedMechBoss3)
                        {
                            dryadmultiplier += 0.15f;
                        }
                        if (NPC.downedPlantBoss)
                        {
                            dryadmultiplier += 0.15f;
                        }
                        if (NPC.downedGolemBoss)
                        {
                            dryadmultiplier += 0.15f;
                        }
                        if (NPC.downedAncientCultist)
                        {
                            dryadmultiplier += 0.15f;
                        }
                        if (Main.expertMode)
                        {
                            dryadmultiplier *= Main.GameModeInfo.TownNPCDamageMultiplier;
                        }
                        baseDoT = (int)((float)baseDoT * dryadmultiplier * multiplier);
                        dot -= 2 * baseDoT;
                    }
                    #endregion CustomDoTFunction
                }

            }
            if (BuffData.HasAnyBuffWithTag(self, BuffData.Fire) && self.HasBuff(BuffID.Oiled))
            {
                dot -= 50; //oiled does +25 dps if you have any fire debuff
            }
            //some misc stuff that is also done in the method i turned off because lol
            #region MiscselfBuffRedo
            //this mechanic doesnt have an attatched debuff but its done in the method i turned off so i gotta redo it here
            if (self.tentacleSpiked)
            {
                if (dot == 0 && self.lifeRegen > 0) self.lifeRegen = 0;
                int numSpikes = 0;
                for (int j = 0; j < Main.maxProjectiles; j++)
                {
                    if (Main.projectile[j].active && Main.projectile[j].type == ProjectileID.TentacleSpike && Main.projectile[j].ai[0] == 1f && Main.projectile[j].ai[1] == self.whoAmI)
                    {
                        numSpikes++;
                    }
                }
                dot -= numSpikes * 2 * 3;
            }
            if (self.soulDrain && self.realLife == -1)
            {
                if (dot == 0 && self.lifeRegen > 0) self.lifeRegen = 0;
                dot -= 50;

            }
            self.lifeRegen += dot;
            self.lifeRegenCount += self.lifeRegen;
            while (self.lifeRegenCount >= 120)
            {
                self.lifeRegenCount -= 120;
                if (!self.immortal)
                {
                    if (self.life < self.lifeMax)
                    {
                        self.life++;
                    }
                    if (self.life > self.lifeMax)
                    {
                        self.life = self.lifeMax;
                    }
                }
            }
            if (dot < 0)
            {

                while (self.lifeRegenCount <= -120 * -dot / 2)
                {
                    self.lifeRegenCount += 120 * -dot / 2;

                    if (self.realLife >= 0)
                    {
                        self.whoAmI = self.realLife;
                    }
                    if (!Main.npc[self.whoAmI].immortal)
                    {
                        Main.npc[self.whoAmI].life -= -dot / 2;
                    }
                    CombatText.NewText(new Rectangle((int)self.position.X, (int)self.position.Y, self.width, self.height), CombatText.LifeRegenNegative, -dot / 2, dramatic: false, dot: true);
                    if (Main.npc[self.whoAmI].life > 0 || Main.npc[self.whoAmI].immortal)
                    {
                        continue;
                    }
                    Main.npc[self.whoAmI].life = 1;
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Main.npc[self.whoAmI].SimpleStrikeNPC(9999, 1, noPlayerInteraction: true);
                        if (Main.netMode == NetmodeID.Server)
                        {
                            NetMessage.SendData(MessageID.DamageNPC, -1, -1, null, self.whoAmI, 9999f);
                        }
                    }
                }
                return;
            }
            while (self.lifeRegenCount <= -120)
            {
                self.lifeRegenCount += 120;
                if (self.realLife >= 0)
                {
                    self.whoAmI = self.realLife;
                }
                if (!Main.npc[self.whoAmI].immortal)
                {
                    Main.npc[self.whoAmI].life--;
                }
                CombatText.NewText(new Rectangle((int)self.position.X, (int)self.position.Y, self.width, self.height), CombatText.LifeRegenNegative, 1, dramatic: false, dot: true);
                if (Main.npc[self.whoAmI].life > 0 || Main.npc[self.whoAmI].immortal)
                {
                    continue;
                }
                Main.npc[self.whoAmI].life = 1;
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Main.npc[self.whoAmI].SimpleStrikeNPC(9999, 1, noPlayerInteraction: true);
                    if (Main.netMode == NetmodeID.Server)
                    {
                        NetMessage.SendData(MessageID.DamageNPC, -1, -1, null, self.whoAmI, 9999f);
                    }
                }
            }
            #endregion MiscselfBuffRedo
            return;
        }

    }
    
    public class BuffData
    {
        //damage over time done to npcs
        public int DoT;
        //damage over time done to players. if null, do npc DoT / 2
        public int? PlayerDoT;
        //tags, example: add "fire" to this and anything that checks for fire debuff will see your buff.
        public string[]? Tags;
        //if the debuff does some wacky things in its DoT application and shouldnt be counted in a for loop thats gonna be done.
        //usually debuffs that do more damage when more projectiles are attatched to the enemy
        public bool CustomDoTLogic;
        public BuffData(int damageOverTime, int? PlayerDamageOverTime = null, string[]? tags = null, bool customDoT = false)
        {
            DoT = damageOverTime;
            PlayerDoT = PlayerDamageOverTime;
            if (tags != null)
            {
                for (int i = 0; i < tags.Length; i++)
                {
                    tags[i] = tags[i].ToLower();
                }
            }
            Tags = tags;
            CustomDoTLogic = customDoT;
        }
        //Constants for if you think youll make a typo or forget names when tagging debuffs.
        public const string DamageOverTime = "dot";
        public const string Fire = "fire";
        public const string Blood = "blood";
        public const string Poison = "poison";
        public const string Natural = "natural";
        public const string Ice = "ice";
        public const string Evil = "evil";
        public const string Holy = "holy";
        public const string StatDown = "stats";
        public const string Slowness = "slow";
        public const string Freezing = "stun";
        public const string Darkening = "dark";
        public const string Electric = "electric";
        /// <summary>
        /// gets the buffdata object of the given buffID
        /// returns null if there is no buffdata object for this buff
        /// </summary>
        /// <param name="buffID"></param>
        /// <returns></returns>
        public static BuffData? GetBuffData(int buffID)
        {
            bool real = BuffSystem.BuffInfo.TryGetValue(buffID, out BuffData buffData);
            if (real) return buffData;
            return null;
        }
        /// <summary>
        /// returns the amount of damage a buff does to npcs per second
        /// returns 0 if there is no buff data for the given buffID
        /// </summary>
        /// <param name="buffID"></param>
        /// <returns></returns>
        public static int GetBuffDoT(int buffID)
        {
            BuffData data = GetBuffData(buffID);
            if (data != null) return data.DoT;
            return 0;
        }
        /// <summary>
        /// returns the amount of damage a buff does to a player per second
        /// returns 0 if there is no buffdata or if the buffdata does not have player DoT set
        /// </summary>
        /// <param name="buffID"></param>
        /// <returns></returns>
        public static int GetBuffPlayerDoT(int buffID)
        {
            BuffData data = GetBuffData(buffID);
            if (data != null && data.PlayerDoT != null) return (int)data.PlayerDoT;
            return 0;
        }
        /// <summary>
        /// gets the list of tags the given buffID has
        /// returns an empty list if the buffdata is null or the tags list is null
        /// </summary>
        /// <param name="buffID"></param>
        /// <returns></returns>
        public static string[] GetBuffTags(int buffID)
        {
            BuffData data = GetBuffData(buffID);
            if (data != null && data.Tags != null) return data.Tags;
            return [];
        }
        /// <summary>
        /// returns true if the buffdata of the given buffID contains the given string in its tags
        /// returns false if it doesnt or if there is no buffdata or tags
        /// </summary>
        /// <param name="buffID"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static bool HasTag(int buffID, string tag)
        {
            string[] tags = GetBuffTags(buffID);
            if (tags != null && tags.Length > 0 && tags.Contains(tag))
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// returns true if the given npc has any buff with the given tag
        /// </summary>
        /// <param name="npc"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static bool HasAnyBuffWithTag(NPC npc, string tag)
        {
            for (int i = 0; i < npc.buffType.Length; i++)
            {
                if (npc.buffTime[i] > 0 && HasTag(npc.buffType[i], tag))
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// returns true if the given player has any buff with the given tag
        /// </summary>
        /// <param name="player"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static bool HasAnyBuffWithTag(Player player, string tag)
        {
            for (int i = 0; i < player.buffType.Length; i++)
            {
                if (player.buffTime[i] > 0 && HasTag(player.buffType[i], tag))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
