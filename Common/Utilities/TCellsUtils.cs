using Humanizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.Utilities.Terraria.Utilities;

namespace TerrariaCells.Common.Utilities
{
    public class TCellsUtils
    {
        public enum LerpEasing
        {
            Linear,
            InSine,
            OutSine,
            InOutSine,
            InCubic,
            OutCubic,
            InOutCubic,
            InQuint,
            OutQuint,
            InOutQuint,
            InBack,
            OutBack,
            InOutBack,
            DownParabola,
            Bell
        }
        
        public static Vector2 FindGround(Rectangle rectangle, int attempts = 200)
        {
            
            Vector2 tilecords = new Vector2(rectangle.X, rectangle.Y).ToTileCoordinates().ToVector2();
            //Dust.NewDustDirect(rectangle.Center.ToVector2(), 1, 1, DustID.TerraBlade);
            while (!Collision.SolidCollision(rectangle.BottomLeft(), rectangle.Width, 2, true) && attempts > 0)
            {
                attempts--;
                rectangle.Y += 16;
               
            }
            
            while (Collision.SolidCollision(rectangle.BottomLeft(), rectangle.Width, -2, true) && attempts > 0)
            {
                attempts--;
                rectangle.Y -= 16;
            }

            Vector2 pos = new Vector2(rectangle.Center.X, rectangle.Center.Y);
            return pos;
        }
        /// <summary>
        /// scales damage so you can ignore vanilla's wacky scaling messing up your values
        /// Only use on things that damage the player
        /// </summary>
        /// <param name="damage"></param>
        /// how much damage it does in multiplayer
        /// <param name="expertScale"></param>
        /// multiplier for expert mode
        /// <param name="masterScale"></param>
        /// multiplier for master mode. does not stack with expertScale.
        /// <returns></returns>
        public static int ScaledHostileDamage(int damage, float expertScale = 2, float masterScale = 3)
        {
            //make the number actually what you typed in and increase it by a custom amount per difficulty
            damage /= 2;
            if (Main.expertMode && !Main.masterMode)
            {
                damage /= 2;
                damage = (int)(damage * expertScale);
            }
            if (Main.masterMode)
            {
                damage /= 3;
                damage = (int)(damage * masterScale);
            }

            return damage;
        }
        /// <summary>
        /// gets a value used in lerping. used for the lerp helper functions, you probably shouldnt need to use this, use the helpers instead.
        /// </summary>
        /// <param name="timer"></param>
        /// <param name="length"></param>
        /// <param name="easing"></param>
        /// <param name="start"></param>
        /// <param name="clamp"></param>
        /// <returns></returns>
        public static float GetLerpValue(float timer, float length, LerpEasing easing, float start, bool clamp)
        {
            float x = (timer - start) / length;
            //Main.NewText(x);
            float lerp = 0;
            //i do not know the significance of these numbers. they are used in back easing calculations.
            float overshoot = 1.70158f;
            float overshoot2 = overshoot + 1;
            float overshoot3 = overshoot * 1.525f;
            lerp = easing switch
            {
                LerpEasing.InSine => 1 - (float)Math.Cos((x * Math.PI) / 2),
                LerpEasing.OutSine => (float)Math.Sin((x * Math.PI) / 2),
                LerpEasing.InOutSine => -(float)(Math.Cos(Math.PI * x) - 1) / 2,

                LerpEasing.InCubic => x * x * x,
                LerpEasing.OutCubic => 1 - (float)Math.Pow(1 - x, 3),
                LerpEasing.InOutCubic => x < 0.5f ? 4 * x * x * x : 1 - (float)Math.Pow(-2 * x + 2, 3) / 2,

                LerpEasing.InQuint => x * x * x * x * x,
                LerpEasing.OutQuint => 1 - (float)Math.Pow(1 - x, 5),
                LerpEasing.InOutQuint => x < 0.5f ? 16 * x * x * x * x * x : 1 - (float)Math.Pow(-2 * x + 2, 5) / 2,

                LerpEasing.InBack => overshoot2 * x * x * x - overshoot * x * x,
                LerpEasing.OutBack => 1 + overshoot2 * (float)Math.Pow(x - 1, 3) + overshoot * (float)Math.Pow(x - 1, 2),
                LerpEasing.InOutBack => x < 0.5f
                    ? ((float)Math.Pow(2 * x, 2) * ((overshoot3 + 1) * 2 * x - overshoot3)) / 2
                    : ((float)Math.Pow(2 * x - 2, 2) * ((overshoot3 + 1) * (x * 2 - 2) + overshoot3) + 2) / 2,
                LerpEasing.DownParabola => -(float)Math.Pow(2 * x - 1, 2) + 1,
                LerpEasing.Bell => (float)Math.Sin(2*Math.PI* x - 0.5f*Math.PI) / 2 + 0.5f,
                _ => x
            };
            if (clamp)
            {
                lerp = Math.Clamp(lerp, 0, 1);
            }
            return lerp;
        }
        /// <summary>
        /// Lerps a float from one value to another so you dont have to do a bunch of math
        /// or go to easings.net
        /// </summary>
        /// <param name="startValue">
        /// the value the float begins at
        /// </param>
        /// <param name="endValue">
        /// the value the float ends at
        /// </param>
        /// <param name="timer">
        /// the timer being used to determine lerp progress (your npc's ai timer for example)
        /// this value should be incrementing every frame, the function does not do it for you
        /// </param>
        /// <param name="length">
        /// how long the lerp takes
        /// </param>
        /// <param name="value">
        /// the varibale being lerped. the function will change this variable.
        /// </param>
        /// <param name="easingStyle">
        /// easing style. choose from the LerpEasing enum.
        /// </param>
        /// <param name="startOffset">
        /// when the lerp starts in the timeline of the timer.
        /// so if your lerp starts 30 ticks in for example, it doesnt break (as long as this is set to 30)
        /// </param>
        /// <param name="clamp">
        /// if true, if the lerp value goes out of bounds 0 - 1, it will clamp to 0 or 1.
        /// </param>
        public static float LerpFloat( float startValue, float endValue, float timer, float length, LerpEasing easingStyle, float startOffset = 0, bool clamp = true)
        {
            float lerp = GetLerpValue(timer, length, easingStyle, startOffset, clamp);
            return MathHelper.Lerp(startValue, endValue, lerp);
        }
        /// <summary>
        /// Lerps a vector from one value to another so you dont have to do a bunch of math
        /// or go to easings.net
        /// </summary>
        /// <param name="startValue">
        /// the value the vector begins at
        /// </param>
        /// <param name="endValue">
        /// the value the vector ends at
        /// </param>
        /// <param name="timer">
        /// the timer being used to determine lerp progress (your npc's ai timer for example)
        /// this value should be incrementing every frame, the function does not do it for you
        /// </param>
        /// <param name="length">
        /// how long the lerp takes
        /// </param>
        /// <param name="value">
        /// the varibale being lerped. the function will change this variable.
        /// </param>
        /// <param name="easingStyle">
        /// easing style. choose from the LerpEasing enum.
        /// </param>
        /// <param name="startOffset">
        /// when the lerp starts in the timeline of the timer.
        /// so if your lerp starts 30 ticks in for example, it doesnt break (as long as this is set to 30)
        /// </param>
        /// <param name="clamp">
        /// if true, if the lerp value goes out of bounds 0 - 1, it will clamp to 0 or 1.
        /// </param>
        public static Vector2 LerpVector2(Vector2 startValue, Vector2 endValue, float timer, float length, LerpEasing easingStyle, float startOffset = 0, bool clamp = true)
        {
            float lerp = GetLerpValue(timer, length, easingStyle, startOffset, clamp);
            return Vector2.Lerp(startValue, endValue, lerp);
        }
        /// <summary>
        /// Lerps a color from one value to another so you dont have to do a bunch of math
        /// or go to easings.net
        /// </summary>
        /// <param name="startValue">
        /// the value the color begins at
        /// </param>
        /// <param name="endValue">
        /// the value the color ends at
        /// </param>
        /// <param name="timer">
        /// the timer being used to determine lerp progress (your npc's ai timer for example)
        /// this value should be incrementing every frame, the function does not do it for you
        /// </param>
        /// <param name="length">
        /// how long the lerp takes
        /// </param>
        /// <param name="value">
        /// the varibale being lerped. the function will change this variable.
        /// </param>
        /// <param name="easingStyle">
        /// easing style. choose from the LerpEasing enum.
        /// </param>
        /// <param name="startOffset">
        /// when the lerp starts in the timeline of the timer.
        /// so if your lerp starts 30 ticks in for example, it doesnt break (as long as this is set to 30)
        /// </param>
        /// <param name="clamp">
        /// if true, if the lerp value goes out of bounds 0 - 1, it will clamp to 0 or 1.
        /// </param>
        public static Color LerpColor(Color startValue, Color endValue, float timer, float length, LerpEasing easingStyle, float startOffset = 0, bool clamp = true)
        {
            float lerp = GetLerpValue(timer, length, easingStyle, startOffset, clamp);
             return Color.Lerp(startValue, endValue, lerp);
        }
    }
}
