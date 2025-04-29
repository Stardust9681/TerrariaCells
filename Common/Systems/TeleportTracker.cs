using System;
using System.Linq;
using System.Security.Cryptography.Pkcs;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaCells.Common.Systems;

public class TeleportTracker : ModSystem
{
    // determines the tier of items dropped from chests
    public int level = 1;
    private string nextLevel = "forest";

    public string NextLevel
    {
        get => nextLevel;
        set => nextLevel = value;
    }

    public override void OnModLoad()
    {
        base.OnModLoad();
    }

    public override void OnWorldLoad()
    {
        Main.dayTime = true;
        Main.time = 4f * 3600f;
        Main.StopRain();
        nextLevel = "forest";
        level = 1;
    }

    public void Teleport(string destination)
    {
        Mod.Logger.Info($"Teleporting to {destination}:");
        BasicWorldGenData worldLevelData = Mod.GetContent<BasicWorldGeneration>()
            .First()
            .BasicWorldGenData;
        int variation = worldLevelData.LevelVariations[destination];
        Mod.Logger.Info($"	Variation: {variation}");
        Vector2 position;
        LevelStructure levelStructure;
        Point16 roomPos;
        if (!destination.Equals("inn", StringComparison.CurrentCultureIgnoreCase))
        {
            Mod.Logger.Info($"	Detouring to inn.");

            nextLevel = destination;
            roomPos = worldLevelData.LevelPositions[destination];

            levelStructure = BasicWorldGeneration
                .StaticLevelData.Find(x =>
                    x.Name.Equals("inn", StringComparison.CurrentCultureIgnoreCase)
                )
                .Structures[0];
            roomPos = worldLevelData.LevelPositions["Inn"];

            position = (
                roomPos + new Point16(levelStructure.SpawnX, levelStructure.SpawnY)
            ).ToWorldCoordinates();
            Main.LocalPlayer.Teleport(position, TeleportationStyleID.TeleportationPylon);
            NetMessage.SendData(
                MessageID.TeleportPlayerThroughPortal,
                -1,
                -1,
                null,
                Main.LocalPlayer.whoAmI,
                (int)position.X,
                (int)position.Y
            );

            return;
        }

        roomPos = worldLevelData.LevelPositions[nextLevel];

        levelStructure = BasicWorldGeneration
            .StaticLevelData.Find(x =>
                x.Name.Equals(nextLevel, StringComparison.CurrentCultureIgnoreCase)
            )
            .Structures[variation];

        position = (
            roomPos + new Point16(levelStructure.SpawnX, levelStructure.SpawnY)
        ).ToWorldCoordinates();
        Mod.Logger.Info($"	Found structure. Teleporting to {position}.");
        float hour = 7.5f;
        bool day = true;
        float rain = 0f;
        switch (nextLevel.ToLower())
        {
            case "forest": //Forest
                hour = 4f;
                break;
            case "crimson": //Crimson
                hour = 2.5f;
                level = 2;
                break;
            case "desert": //Desert
                level = 3;
                break;
            case "hive": //Hive
                hour = 4;
                day = false;
                level = 4;
                break;
            case "frozencity": //Frozen City
                hour = 4.5f;
                rain = 1f;
                level = 5;
                var bottle = new Item(ItemID.CloudinaBottle);
                Utils.Swap(ref Main.LocalPlayer.armor[5], ref bottle);
                break;
            //case 10: //Caverns
            //  position = new Vector2(28818.312f, 17606);
            //  return;
        }
        Main.StartRain();
        Main.raining = rain != 0;
        Main.rainTime = rain != 0 ? 100000f : 0f;
        Main.dayTime = day;
        Main.time = hour * 3600;
        if (Main.netMode == NetmodeID.SinglePlayer)
        {
            Main.LocalPlayer.Teleport(position, TeleportationStyleID.TeleportationPylon);
            return;
        }
        // Teleports player on the server, if we're not on singleplayer
        NetMessage.SendData(
            MessageID.TeleportPlayerThroughPortal,
            -1,
            -1,
            null,
            Main.LocalPlayer.whoAmI,
            (int)position.X,
            (int)position.Y
        );
        nextLevel = "Inn";
    }
}
