using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;
using TerrariaCells.Common.Utilities;
using TerrariaCells.Content.WeaponAnimations;

namespace TerrariaCells.Common.ModPlayers
{
    public class WeaponPlayer : ModPlayer
    {
        //sword
        public bool shouldShoot;
        public int useDirection = -1;
        public float useRotation = 0;
        public int swingType = 0;
        public float itemScale = 1;
        public bool reloading = false;
        //multiple
        public float OriginalRotation = 0;
        public override void HideDrawLayers(PlayerDrawSet drawInfo)
        {
            base.HideDrawLayers(drawInfo);
            if ((Sword.IsBroadsword(drawInfo.heldItem) || Bow.Bows.Contains(drawInfo.heldItem.type)) && drawInfo.drawPlayer.ItemAnimationActive)
            {
                PlayerDrawLayers.HeldItem.Hide();
            }
        }
        public override void PostUpdate()
        {
            if (!Player.controlUseItem && !Player.controlUseTile && Player.itemAnimation == 0)
            {
                reloading = false;
            }
            base.PostUpdate();
        }

    }
    public class ReloadDrawLayer : PlayerDrawLayer
    {
        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        {
            Player player = drawInfo.drawPlayer;
            WeaponPlayer mplayer = player.GetModPlayer<WeaponPlayer>();

            bool animationActive = drawInfo.drawPlayer.ItemAnimationActive;
            bool holdingGun = Shotgun.Shotguns.Contains(drawInfo.heldItem.type) || Handgun.Handguns.Contains(drawInfo.heldItem.type);
            bool firstReloadStep = Handgun.Handguns.Contains(drawInfo.heldItem.type) && ((drawInfo.heldItem.TryGetGlobalItem<Handgun>(out Handgun handgun) && handgun.ReloadStep == 0) || !Handgun.Handguns.Contains(drawInfo.heldItem.type));

            return animationActive && holdingGun && firstReloadStep;
        }
        public override Position GetDefaultPosition() => new BeforeParent(PlayerDrawLayers.ArmOverItem);
        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            
            Player player = drawInfo.drawPlayer;
            WeaponPlayer mplayer = player.GetModPlayer<WeaponPlayer>();
            if (player.itemAnimation < player.itemAnimationMax / 2 && mplayer.reloading)
            {

                int animationTime = player.itemAnimationMax - player.itemAnimation;
                Asset<Texture2D> t = ModContent.Request<Texture2D>("TerrariaCells/Content/Projectiles/ShotgunShell");
                float rotation = TCellsUtils.LerpFloat(0, -50 * mplayer.useDirection, animationTime - (player.itemAnimationMax / 2), player.itemAnimationMax / 2, TCellsUtils.LerpEasing.InSine);
                Vector2 origin = new Vector2(t.Width() / 2, t.Height());
                if (Handgun.Handguns.Contains(drawInfo.heldItem.type))
                {
                    t = ModContent.Request<Texture2D>("TerrariaCells/Content/Projectiles/Mag");
                    rotation -= 40 * mplayer.useDirection;
                    origin = t.Size() / 2;
                }
                
                Vector2 position = drawInfo.drawPlayer.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, MathHelper.ToRadians(rotation)) - Main.screenPosition;
                SpriteEffects effects = mplayer.useDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
                drawInfo.DrawDataCache.Add(new DrawData(t.Value, position, null, Lighting.GetColor(drawInfo.drawPlayer.itemLocation.ToTileCoordinates()), MathHelper.ToRadians(rotation + 120 * mplayer.useDirection), origin, 0.8f, effects));
            }
        }
    }
    public class ItemReplaceDrawLayer : PlayerDrawLayer
    {
        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        {
            return (Sword.IsBroadsword(drawInfo.drawPlayer.HeldItem) || Bow.Bows.Contains(drawInfo.heldItem.type)) && drawInfo.drawPlayer.ItemAnimationActive;
        }
        public override Position GetDefaultPosition() => new Between(PlayerDrawLayers.SolarShield, PlayerDrawLayers.ArmOverItem);

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            Asset<Texture2D> t = TextureAssets.Item[drawInfo.drawPlayer.HeldItem.type];

            WeaponPlayer player = drawInfo.drawPlayer.GetModPlayer<WeaponPlayer>();
            Vector2 position = drawInfo.drawPlayer.itemLocation - Main.screenPosition;

            if (Sword.IsBroadsword(drawInfo.heldItem))
            {
                int dir = drawInfo.drawPlayer.direction;
                int swingDir = 1;
                if (drawInfo.drawPlayer.GetModPlayer<WeaponPlayer>().swingType == 1) swingDir = -1;

                SpriteEffects effects = SpriteEffects.None;
                float rotationAdd = 0;
                Vector2 origin = new Vector2(2, t.Height() - 2);
                if ((dir == -1 && swingDir == 1) || (swingDir == -1 && dir == 1))
                {

                    effects = SpriteEffects.FlipHorizontally;
                    origin = new Vector2(t.Width() - 2, t.Height() - 2);
                }
                if (swingDir == -1)
                {
                    rotationAdd += MathHelper.ToRadians(90 * dir);

                }


                drawInfo.DrawDataCache.Add(new DrawData(
                    t.Value,
                    position,
                    null,
                    Lighting.GetColor(drawInfo.drawPlayer.itemLocation.ToTileCoordinates()),
                    drawInfo.drawPlayer.itemRotation + rotationAdd,
                    origin,
                    drawInfo.drawPlayer.HeldItem.scale + player.itemScale - 1,
                    effects,
                    0
                    )
                );
            }
            else if (Bow.Bows.Contains(drawInfo.heldItem.type))
            {
                SpriteEffects effects = drawInfo.drawPlayer.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
                Vector2 scale = new Vector2(1, 1);
                if (drawInfo.drawPlayer.altFunctionUse != 2)
                {
                    scale = TCellsUtils.LerpVector2(new Vector2(1, 0.8f), new Vector2(1, 1), drawInfo.drawPlayer.itemAnimationMax - drawInfo.drawPlayer.itemAnimation, drawInfo.drawPlayer.itemAnimationMax, TCellsUtils.LerpEasing.OutQuint);
                }
                else
                {
                    scale = TCellsUtils.LerpVector2(new Vector2(1, 1), new Vector2(1, 0.9f), drawInfo.heldItem.GetGlobalItem<Bow>().Charge, drawInfo.heldItem.useTime*2, TCellsUtils.LerpEasing.OutQuint);
                }

                drawInfo.DrawDataCache.Add(new DrawData(
                    t.Value,
                    drawInfo.drawPlayer.itemLocation - Main.screenPosition,
                    null,
                    Lighting.GetColor(drawInfo.drawPlayer.itemLocation.ToTileCoordinates()),
                    drawInfo.drawPlayer.itemRotation,
                    new Vector2(t.Width()/2, t.Height()/2),
                    scale * drawInfo.heldItem.scale,
                    effects,
                    0)
                );
            }
        }
    }
}
