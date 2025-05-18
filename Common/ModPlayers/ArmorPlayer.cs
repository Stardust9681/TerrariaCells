using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Chat;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using TerrariaCells.Common.GlobalItems;
using TerrariaCells.Content.Buffs;
using TerrariaCells.Content.Projectiles;
using TerrariaCells.Content.WeaponAnimations;

namespace TerrariaCells.Common.ModPlayers
{
    public class ArmorPlayer : ModPlayer
    {
        public bool ninjaArmorSet => ninjaHood && ninjaShirt && ninjaPants; //shadow dodge <- WORKS
        int ticksUntilShadowDodgeAvailable = 0;
        public bool jungleArmorSet => jungleHat && jungleShirt && junglePants; //killing an enemy reduces mana cost by 100% for 3 seconds <- WORKS
        int ticksUntilManaCostNormal = 0;
        public bool necroArmorSet => necroHelmet && necroBreastplate && necroGreaves; //bows charge twice as fast, guns reload in half the time <- WORKS
        public bool moltenArmorSet => moltenHelmet && moltenBreastplate && moltenGreaves; //all fire debuffs are upgraded to Hellfire, with increased damage and duration <- WORKS

        public bool ninjaHood;
        public bool ninjaShirt; //grants a short dodge <- WORKS
        int ticksUntilDashAvailable = 0;
        int ticksSinceLastLeftPress = 100000;
        int ticksSinceLastRightPress = 100000;
        int leftMultiPressCount;
        int rightMultiPressCount;
        int ticksUntilLeftDashEnd = 0;
        int ticksUntilRightDashEnd = 0;
        public bool ninjaPants;

        public bool jungleHat;
        public bool jungleShirt; //Picking up mana stars reduces skill cooldowns by 1/2 second <- WORKS
        public bool junglePants;

        public bool necroHelmet;
        public bool necroBreastplate; //Killing an enemy spawns baby spiders, which attack nearby enemies <- WORKS
        public bool necroGreaves; //The last bullet in a magazine deals 50% more damage <- WORKS

        public bool moltenHelmet;
        public bool moltenBreastplate; //-20% damage taken. Upon taking damage, all nearby enemies are lit on fire <- WORKS
        public bool moltenGreaves; //Leave a trail of flames that ignites enemies (hellfire treads, but functional) <- WORKS
        float distanceUntilFlameSpawn = 0;

        public override void ResetEffects()
        {
            ninjaHood = false;
            ninjaShirt = false;
            ninjaPants = false;
            jungleHat = false;
            jungleShirt = false;
            junglePants = false;
            necroHelmet = false;
            necroBreastplate = false;
            necroGreaves = false;
            moltenHelmet = false;
            moltenBreastplate = false;
            moltenGreaves = false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (target.life < 1 && target.lifeMax > 5 && !target.friendly) OnKill(target, hit, hit.Damage);

            if (ninjaArmorSet && ticksUntilShadowDodgeAvailable <= 0 && !Player.HasBuff(ModContent.BuffType<ShadowDodgeBuff>()))
            {
                Player.AddBuff(ModContent.BuffType<ShadowDodgeBuff>(), 1200);
                //isShadowDodgeActive = true;
                ticksUntilShadowDodgeAvailable = 1800;
            }
        }

        private void OnKill(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (necroBreastplate && hit.DamageType != DamageClass.Summon)
            {
                Projectile.NewProjectile(Player.GetSource_Accessory(Player.armor[1]), target.Center, Vector2.Zero, ProjectileID.BabySpider, 5, 0);
            }

            if (jungleArmorSet)
            {
                ticksUntilManaCostNormal = 180;
            }
        }

        public override void ModifyManaCost(Item item, ref float reduce, ref float mult)
        {
            if (ticksUntilManaCostNormal > 0)
            {
                mult *= 0;
            }
        }

		public override bool OnPickup(Item item)
		{
			if (item.type is ItemID.Star or ItemID.SoulCake or ItemID.SugarPlum)
			{
				Systems.AbilityHandler modPlayer = Player.GetModPlayer<Systems.AbilityHandler>();
				foreach (Systems.AbilitySlot ability in modPlayer.Abilities)
				{
					if (ability.IsOnCooldown)
						ability.cooldownTimer -= 30;
				}
			}
			return base.OnPickup(item);
		}

		public override void ModifyShootStats(Item item, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            if (necroGreaves && Gun.TryGetGlobalItem(item, out Gun gun) && gun.Ammo <= 1)
            {
                damage = (int)(damage * 1.25f);
            }
        }

        public override void ModifyHurt(ref Player.HurtModifiers modifiers)
        {
            if (moltenBreastplate)
            {
                modifiers.FinalDamage *= 0.8f;
                foreach (NPC anyNpc in Main.ActiveNPCs)
                {
                    if (!anyNpc.friendly && anyNpc.lifeMax > 5 && anyNpc.Distance(Player.Center) < 160) //10 blocks
                    {
                        anyNpc.AddBuff(BuffID.OnFire, 600);
                    }
                }
            }
        }

        public override void PreUpdateMovement()
        {
            if (ticksUntilRightDashEnd > 0)
            {
                Player.velocity.X = Math.Max(Player.velocity.X, 10);
            }
            if (ticksUntilLeftDashEnd > 0)
            {
                Player.velocity.X = Math.Min(Player.velocity.X, -10);
            }
        }

        public override void PreUpdate()
        {
            ticksUntilShadowDodgeAvailable = Math.Max(0, ticksUntilShadowDodgeAvailable - 1);
            ticksUntilManaCostNormal = Math.Max(0, ticksUntilManaCostNormal - 1);
            ticksUntilDashAvailable = Math.Max(0, ticksUntilDashAvailable - 1);
            ticksSinceLastRightPress++;
            ticksSinceLastLeftPress++;
            ticksUntilRightDashEnd = Math.Max(0, ticksUntilRightDashEnd - 1);
            ticksUntilLeftDashEnd = Math.Max(0, ticksUntilLeftDashEnd - 1);

            if (ninjaShirt && ticksUntilDashAvailable <= 0)
            {
                if (ticksSinceLastRightPress >= 15)
                {
                    rightMultiPressCount = 0;
                }

                if (ticksSinceLastLeftPress >= 15)
                {
                    leftMultiPressCount = 0;
                }

                if (Player.holdDownCardinalTimer[2] != 1 || Player.holdDownCardinalTimer[3] != 1 || true)
                {
                    if (Player.holdDownCardinalTimer[2] == 1)
                    {
                        rightMultiPressCount++;
                        ticksSinceLastRightPress = 0;
                        leftMultiPressCount = 0;
                    }

                    if (Player.holdDownCardinalTimer[3] == 1)
                    {
                        leftMultiPressCount++;
                        ticksSinceLastLeftPress = 0;
                        rightMultiPressCount = 0;
                    }
                }

                if (rightMultiPressCount >= 2 && ticksUntilDashAvailable <= 0)
                {
                    ticksUntilRightDashEnd = 3;
                    ticksUntilDashAvailable = 45;
                    rightMultiPressCount = 0;
                }
                if (leftMultiPressCount >= 2 && ticksUntilDashAvailable <= 0)
                {
                    ticksUntilLeftDashEnd = 3;
                    ticksUntilDashAvailable = 45;
                    leftMultiPressCount = 0;
                }
            }

            if (moltenArmorSet)
            {
                foreach (NPC npc in Main.ActiveNPCs)
                {
                    if (npc.HasBuff(BuffID.OnFire))
                    {
                        int onFireBuffIfx = npc.FindBuffIndex(BuffID.OnFire);
                        npc.AddBuff(BuffID.OnFire3, (int)(npc.buffTime[onFireBuffIfx] * 1.5f));
                        npc.buffTime[onFireBuffIfx] = 0;
                    }
                }
            }
        }

        public override void PostUpdate()
        {
            if (moltenGreaves && Player.velocity.Y == 0)
            {
                float horizontalSpeed = Math.Abs(Player.velocity.X);
                distanceUntilFlameSpawn -= horizontalSpeed;
                if (Player.velocity.X == 0)
                {
                    distanceUntilFlameSpawn = 0f;
                }
                if (distanceUntilFlameSpawn < 0 && Player.velocity.X != 0)
                {
                    distanceUntilFlameSpawn += Math.Min(8f, horizontalSpeed * 4f);
                    Projectile projectile = Projectile.NewProjectileDirect(Player.GetSource_FromThis(), Player.Center + new Vector2(0f, 16f), Vector2.Zero, ModContent.ProjectileType<TrailOfFlames>(), 1, 0);
                    ((TrailOfFlames)projectile.ModProjectile).scaleFactor = Math.Min(0.8f, (0.75f + Main.rand.NextFloat() * 0.5f) * 0.65f * Math.Abs(horizontalSpeed / Player.maxRunSpeed));
                }
            }
        }
    }
}
