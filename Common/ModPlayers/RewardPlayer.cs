using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader;
using Terraria;
using Terraria.UI;
using Terraria.ModLoader.UI;
using Terraria.ModLoader.UI.Elements;
using Terraria.ID;
using System.Timers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader.IO;

namespace TerrariaCells.Common.ModPlayers
{
    public class RewardPlayer : ModPlayer, IComparable<int>, IComparable<uint>, IComparable<TimeSpan>
    {
        [FlagsAttribute]
        public enum TrackerAction : byte
        {
            None = 0,

            Start = 1 << 0,
            Pause = 1 << 1,
            Reset = 1 << 2,
            
            Stop = Pause | Reset,
            Restart = Start | Reset,
        }
        public void UpdateTracker_EnterNewWorld()
        {
            UpdateTracker(TrackerAction.Restart);
            targetTime = TimeSpan.FromMinutes(3);
            targetKillCount = 50;
        }
        public void UpdateTracker(TrackerAction action)
        {
            for (byte i = 0; i < 8; i++)
            {
                switch (action & (TrackerAction)(1 << i))
                {
                    case TrackerAction.Start:
                        trackerEnabled = true;
                        break;
                    case TrackerAction.Pause:
                        trackerEnabled = false;
                        break;
                    case TrackerAction.Reset:
                        levelTimer = 0;
                        killCount = 0;
                        break;
                }
            }
        }
        public TimeSpan targetTime;
        public byte targetKillCount;

        private bool trackerEnabled = false;
        private uint levelTimer = 0;
        private byte killCount = 0;
        public override void PostUpdate()
        {
            if (Main.gameMenu)
                return;
            if (Main.gamePaused)
                return;
            if (Player.DeadOrGhost)
                return;

            if (trackerEnabled)
            {
                levelTimer++;
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (target.life < 1 && target.lifeMax > 5 && !target.friendly && !NPCID.Sets.ProjectileNPC[target.type])
            {
                killCount++;
            }
        }
        public override void OnEnterWorld()
        {
            Systems.DeadCellsUISystem.ToggleActive<Content.UI.RewardTracker>(true);
        }

        public override void SaveData(TagCompound tag)
        {
            if(trackerEnabled)
                tag.Add(nameof(trackerEnabled), trackerEnabled);
            if(levelTimer > 0)
                tag.Add(nameof(levelTimer), levelTimer);
            if (killCount > 0)
                tag.Add(nameof(killCount), killCount);
        }
        public override void LoadData(TagCompound tag)
        {
            trackerEnabled = tag.Get<bool>(nameof(trackerEnabled));
            levelTimer = tag.Get<uint>(nameof(levelTimer));
            killCount = tag.Get<byte>(nameof(killCount));
        }

        int IComparable<int>.CompareTo(int frames) => levelTimer.CompareTo(frames);
        int IComparable<uint>.CompareTo(uint frames) => levelTimer.CompareTo(frames);
        int IComparable<TimeSpan>.CompareTo(TimeSpan time) => LevelTime.CompareTo(time);

        public uint _LevelTime => levelTimer;
        public TimeSpan LevelTime => TimeSpan.FromSeconds(levelTimer / 60.0);

        public byte KillCount => killCount;
    }
}
