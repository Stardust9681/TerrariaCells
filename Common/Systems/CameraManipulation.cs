using System;
using System.Reflection;
using Terraria;
using Terraria.Graphics;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TerrariaCells.Common.Configs;
using Terraria.Graphics.CameraModifiers;

namespace TerrariaCells.Common.Systems
{
	/// <summary>
	/// Use <see cref="SetZoom(int, Vector2?, float?)"/> and <see cref="SetCamera(int, Vector2)"/> to change camera position and zoom.
	/// </summary>
	/// <remarks><see cref="SpriteViewMatrix"/> rotation translation wasn't working for the world, only for the background, so no rotation.</remarks>
	public class CameraManipulation : ModSystem
	{
		//For Zoom:
			//Lower = Zoom Out (See more all at once, in less detail)
			//Higher = Zoom In (See less all at once, in more detail)
		private const float _DefaultZoomCap = 1.75f; //Lower bound, Zoom > 1.75 unless overridden

		private static ZoomOverride _ZoomOverride = new ZoomOverride();
		private static CameraModifier _CameraModifier = new CameraModifier();

		public static void SetZoom(int lerpTime, Vector2? screenSize = null, float? zoomLevel = null)
		{
			if (screenSize == null && zoomLevel == null) return;
			if (_ZoomOverride.ScreenSize == screenSize && _ZoomOverride.ZoomLevel == zoomLevel)
			{
				_ZoomOverride.LerpTime = lerpTime;
				if (_ZoomOverride.Time == 0) _ZoomOverride.Time = 2 * lerpTime;
				else if (_ZoomOverride.Time < lerpTime) _ZoomOverride.Time = lerpTime;
			}
			else if (_ZoomOverride.Time == 0)
			{
				_ZoomOverride.Time = 2 * lerpTime;
				_ZoomOverride.LerpTime = lerpTime;
				_ZoomOverride.ScreenSize = screenSize;
				_ZoomOverride.ZoomLevel = zoomLevel;
			}
		}
		public static void SetCamera(int lerpTime, Vector2 position)
		{
			if (_CameraModifier.Position == position)
			{
				_CameraModifier.LerpTime = lerpTime;
				if (_CameraModifier.Time == 0) _CameraModifier.Time = 2 * lerpTime;
				else if (_CameraModifier.Time < lerpTime) _CameraModifier.Time = lerpTime;
			}
			else if (_CameraModifier.Time == 0)
			{
				_CameraModifier.Time = 2 * lerpTime;
				_CameraModifier.LerpTime = lerpTime;
				_CameraModifier.Position = position;
			}
		}

		public override void ModifyTransformMatrix(ref SpriteViewMatrix Transform)
		{
			if (Main.gameMenu || TerrariaCellsConfig.Instance.DisableZoom)
				return;

			// Caps zoom at 175%-200%
			float zoomClamp = Math.Max(Transform.Zoom.X, _DefaultZoomCap);
			Vector2 zoom = Vector2.One * zoomClamp;

			if (!Main.LocalPlayer.dead)
			{
				_ZoomOverride.TryApply(ref zoom);
			}

			Transform.Zoom = zoom;
		}
		public override void ModifyScreenPosition()
		{
			if (!Main.LocalPlayer.dead)
			{
				_CameraModifier.TryApply(ref Main.screenPosition);
				Main.instance.CameraModifiers.ApplyTo(ref Main.screenPosition);
			}
			else
			{
				_CameraModifier.Position = Main.screenPosition;
				_CameraModifier.LerpTime = 0;
				_CameraModifier.Time = 0;
			}
		}
		public override void PreUpdateEntities()
		{
			if (_ZoomOverride.Time > 0)
				_ZoomOverride.Time--;
			if (_CameraModifier.Time > 0)
				_CameraModifier.Time--;
		}
	}
	public class ZoomOverride
	{
		public Vector2? ScreenSize;
		public float? ZoomLevel;

		public int LerpTime;
		public int Time;

		public bool TryApply(ref Vector2 zoom)
		{
			if (Time < 1) return false;

			Vector2 newZoom = Vector2.One;
			if (ScreenSize != null)
			{
				Vector2 newSize = ScreenSize.Value;
				float testWidth = newSize.Y * (float)Main.screenWidth / (float)Main.screenHeight;
				if (testWidth > newSize.X)
					newSize.X = testWidth;
				float testHeight = newSize.X * (float)Main.screenHeight / (float)Main.screenWidth;
				if (testHeight > newSize.Y)
					newSize.Y = testHeight;
				newSize = new Vector2(Main.screenWidth / newSize.X, Main.screenHeight / newSize.Y);
				newZoom *= newSize;
			}
			if (ZoomLevel != null)
			{
				newZoom *= ZoomLevel.Value;
			}

			float lerpValue = 1f - (MathF.Abs(Time - LerpTime) / LerpTime);
			if (lerpValue < 1)
				zoom = Vector2.Lerp(zoom, newZoom, lerpValue);
			else
				zoom = newZoom;

			return true;
		}
	}
	public class CameraModifier
	{
		public Vector2 Position;

		public int LerpTime;
		public int Time;

		public bool TryApply(ref Vector2 screenPos)
		{
			if (Time < 1) return false;

			float lerpValue = 1f - (MathF.Abs(Time - LerpTime) / LerpTime);
			if (lerpValue < 1)
				screenPos = Vector2.Lerp(screenPos, Position, lerpValue);
			else
				screenPos = Position;

			return true;
		}
	}
}