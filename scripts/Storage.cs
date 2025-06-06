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
    public SkillBlueprint[] skills;
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
            var data = JSONFile.Read<GameData>(path);

            // GameData data = Json.Parse(json);
            data.skills ??= Array.Empty<SkillBlueprint>();
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
            skills = new SkillBlueprint[] {
                new() {
                    name = "test",
                    cost = new Cost() { ap = 1, mp = 0 },
                    restrictions = new SkillRestrictions() { cooldown = 0, perTurn = 1, perTarget = 1 },
                    damages = new Damage[] {
                        new() { min = 1, max = 2, damageType = DamageType.Direct, element = Element.Neutral },
                    },
                    range = new Range() { min = 1, max = 1 },
                    vision = 0,
                    targetRadius = new Range() { min = 0, max = 0 },
                },
                // new() { name = "skill1", }
            },
            gear = new GameDataGear[] {
                new() { name = "vineCowl" },
                new() { name = "vineJacket" },
                new() { name = "vineHandcovers" },
                new() { name = "vineJambs" },
                new() { name = "dagger" },
                new() { name = "ironHelm" },
                new() { name = "ironArmor" },
                new() { name = "ironGauntlets" },
                new() { name = "ironCuisses" },
            },
            resources = new InventoryItem[] {
                new() { name = "myceliumThreads", quantity = 10 },
                new() { name = "sporeSacs", quantity = 5 }
            },
            skillSlots = new() {
                {0, new GameDataSkill(){ name = "bareHandedStrike", label = "bareHandedStrike" } },
                {1, new GameDataSkill(){ name = "interact", label = "interact" } },
                {2, null },
                {3, null },
                {4, null },
                {5, null },
                {6, null },
                {7, null },
                {8, null },
                {9, null }
            },
            gearSlots = new() {
                { GearSlot.Head,new GameDataGear(){name="vineCowl"}},
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

        JSONFile.Write(path, data);
    }

    private static string PathForDocumentsFile()
    {
        var filename = "strg";
        return Path.Combine("res://", filename);
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
