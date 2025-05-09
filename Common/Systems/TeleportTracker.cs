using System;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaCells.Common.Systems;

public class TeleportTracker : ModSystem
{
    // determines the tier of items dropped from chests
    public int level = 1;
    private string nextLevel = "Forest";
    private int nextLevelVariation = 0;

    /// <summary>
    /// setting Main.time during world loading is borked for some reason, so its delayed until its more convenient.
    /// </summary>
    private bool deferredTimeSet = true;

    public string NextLevel
    {
        get => nextLevel;
        set => nextLevel = value;
    }

    public override void ClearWorld()
    {
        deferredTimeSet = true;
        Mod.Logger.Info($"b4: {Main.time};{Main.dayTime}");
        Main.dayTime = false;
        Main.time = 1f * 3600f;
        Main.StopRain();
        Mod.Logger.Info($"af: {Main.time};{Main.dayTime}");
        // Mod.Logger.Info($"after time: ");
        level = 1;
        nextLevel = "Forest";
        nextLevelVariation = 0;
    }

    public override void PreUpdateWorld()
    {
        if (deferredTimeSet)
        {
            ClearWorld();
            deferredTimeSet = false;
        }
    }

    public void Teleport(string destination)
    {
        if (destination.Equals("inn", StringComparison.CurrentCultureIgnoreCase))
        {
            Mod.Logger.Info($"Teleporting to next level: {nextLevel}:");
            GoToNextLevel();
            return;
        }

        Mod.Logger.Info($"Detouring to inn.");
        DetourToInn(destination);
    }

    private void DetourToInn(string destination)
    {
        BasicWorldGenData worldLevelData = Mod.GetContent<BasicWorldGeneration>()
            .First()
            .BasicWorldGenData;

        nextLevel = destination;
        nextLevelVariation = worldLevelData.LevelVariations[destination];

        Point16 roomPos = worldLevelData.LevelPositions[destination];

        LevelStructure levelStructure = BasicWorldGeneration
            .StaticLevelData.Find(x =>
                x.Name.Equals("inn", StringComparison.CurrentCultureIgnoreCase)
            )
            .Structures[0];

        roomPos = worldLevelData.LevelPositions["Inn"];

        Vector2 position = (
            roomPos + new Point16(levelStructure.SpawnX, levelStructure.SpawnY)
        ).ToWorldCoordinates();

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
    }

    private void GoToNextLevel()
    {
        BasicWorldGenData worldLevelData = Mod.GetContent<BasicWorldGeneration>()
            .First()
            .BasicWorldGenData;

        Mod.Logger.Info($"	Variation: {nextLevelVariation}");

        Point16 roomPos = worldLevelData.LevelPositions[nextLevel];

        LevelStructure levelStructure = BasicWorldGeneration
            .StaticLevelData.Find(x =>
                x.Name.Equals(nextLevel, StringComparison.CurrentCultureIgnoreCase)
            )
            .Structures[nextLevelVariation];

        Vector2 position = (
            roomPos + new Point16(levelStructure.SpawnX, levelStructure.SpawnY)
        ).ToWorldCoordinates();

        Mod.Logger.Info($"	Found structure. Teleporting to position {position}.");

        float hour = 8.5f;
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
        nextLevelVariation = 0;
        return;
    }
}
