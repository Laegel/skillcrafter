using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;
using Microsoft.Win32.SafeHandles;

[Serializable]
public class GameDataSkill
{
    public string name;
    public string label;
    // Modifiers
}

public class GameDataGear
{
    public string name;
    // Tint?
}

public class GameDataBarterOffer : BarterOffer
{
    public bool isAvailable;
}

[Serializable]
public class InventoryItem
{
    public string name;
    public int quantity;
}


[Serializable]
public partial class GameData : GodotObject
{
    public Dictionary<GearSlot, GameDataGear> gearSlots;
    public Dictionary<int, GameDataSkill> skillSlots;
    public GameDataSkill[] skills;
    public GameDataGear[] gear;
    public Dictionary<string, Dictionary<string, MapItemBase>> progress;
    public InventoryItem[] resources;
    public GameDataBarterOffer[] barterOffers;
}

public class Storage
{
    public static GameData Read()
    {
        string path = PathForDocumentsFile();
        var env = OS.GetModelName();

        if (File.Exists(path) && env != "GenericDevice")
        {
            var data = JSON.Read<GameData>(path);

            // GameData data = Json.Parse(json);
            data.skills ??= Array.Empty<GameDataSkill>();
            data.gear ??= Array.Empty<GameDataGear>();
            data.resources ??= Array.Empty<InventoryItem>();
            data.skillSlots ??= new();
            data.gearSlots ??= new();
            data.barterOffers ??= Array.Empty<GameDataBarterOffer>();
            data.progress ??= new Dictionary<string, Dictionary<string, MapItemBase>>
                {
                    { "forest", new() {
                        {"forest1", new MapItemBase() { name = "forest1", status = ZoneStatus.Unlocked }}
                    } }
                };
            return data;

        }
        return new GameData()
        {
            skills = Array.Empty<GameDataSkill>(),
            gear = Array.Empty<GameDataGear>(),
            resources = Array.Empty<InventoryItem>(),
            skillSlots = new() {
                {0, new GameDataSkill(){ name = "bareHandedStrike", label = "bareHandedStrike" } },
                {1, new GameDataSkill(){ name = "interact", label = "interact" } },
            },
            gearSlots = new() {
                {GearSlot.Head,new GameDataGear(){name="vineCowl"}},
                { GearSlot.Torso, new GameDataGear(){name="vineJacket" }},
                { GearSlot.Arms, new GameDataGear(){ name="vineHandcovers"} },
                { GearSlot.Legs, new GameDataGear(){name="vineJambs"} },
                { GearSlot.Weapon, null}
            },
            barterOffers = Array.Empty<GameDataBarterOffer>(),
            progress = new Dictionary<string, Dictionary<string, MapItemBase>>
            {
                { "forest", new() {
                    {"forest1", new MapItemBase() { name = "forest1", status = ZoneStatus.Unlocked }}
                } }
            }
        };
    }

    public static void Write(GameData data)
    {
        string path = PathForDocumentsFile();

        JSON.Write(path, data);
    }

    private static string PathForDocumentsFile()
    {
        var filename = "strg";
        return Path.Combine("user://", filename);
        // var env = OS.GetModelName();
        // if (env == "IOS")
        // {
        //     string path = Application.dataPath[..^5];
        //     path = path[..path.LastIndexOf('/')];
        //     return Path.Combine(Path.Combine(path, "Documents"), filename);
        // }
        // else if (env == "Android")
        // {
        //     string path = Application.persistentDataPath;
        //     path = path[..path.LastIndexOf('/')];
        //     return Path.Combine(path, filename);
        // }
        // else
        // {
        //     string path = Application.dataPath;
        //     path = path[..path.LastIndexOf('/')];
        //     return Path.Combine(path, filename);
        // }
    }

}
