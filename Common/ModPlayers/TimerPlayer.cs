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
    public class TimerPlayer : ModPlayer, IComparable<int>, IComparable<uint>, IComparable<TimeSpan>
    {
        [FlagsAttribute]
        public enum TimerAction : byte
        {
            None = 0,

            Start = 1 << 0,
            Pause = 1 << 1,
            Reset = 1 << 2,
            
            Stop = Pause | Reset,
            Restart = Start | Reset,
        }
        public void UpdateTimer_EnterNewWorld()
        {
            UpdateTimer(TimerAction.Restart);
        }
        public void UpdateTimer(TimerAction action)
        {
            for (byte i = 0; i < 8; i++)
            {
                switch (action & (TimerAction)(1 << i))
                {
                    case TimerAction.Start:
                        timerEnabled = true;
                        break;
                    case TimerAction.Pause:
                        timerEnabled = false;
                        break;
                    case TimerAction.Reset:
                        levelTimer = 0;
                        break;
                }
            }
        }
        private bool timerEnabled = false;
        private uint levelTimer = 0;
        public override void PostUpdate()
        {
            if (Main.gameMenu)
                return;
            if (Main.gamePaused)
                return;
            if (Player.DeadOrGhost)
                return;

            if (timerEnabled)
            {
                levelTimer++;
            }
        }
        public override void OnEnterWorld()
        {
            UI.DeadCellsUISystem.ToggleActive<Content.UI.LevelTimer>(true);
        }

        public override void SaveData(TagCompound tag)
        {
            if(timerEnabled)
                tag.Add(nameof(timerEnabled), timerEnabled);
            if(levelTimer > 0)
                tag.Add(nameof(levelTimer), levelTimer);
        }
        public override void LoadData(TagCompound tag)
        {
            timerEnabled = tag.Get<bool>(nameof(timerEnabled));
            levelTimer = tag.Get<uint>(nameof(levelTimer));
        }

        int IComparable<int>.CompareTo(int frames) => levelTimer.CompareTo(frames);
        int IComparable<uint>.CompareTo(uint frames) => levelTimer.CompareTo(frames);
        int IComparable<TimeSpan>.CompareTo(TimeSpan time) => LevelTime.CompareTo(time);

        public uint _LevelTime => levelTimer;
        public TimeSpan LevelTime => TimeSpan.FromSeconds(levelTimer / 60.0);
    }
}
