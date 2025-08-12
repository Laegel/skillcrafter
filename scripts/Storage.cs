using System;
using System.Collections.Generic;
using System.IO;
using Godot;

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

public class StorageItem {}


[Serializable]
public partial class GameData : GodotObject
{
    public Dictionary<GearSlot, GameDataGear> gearSlots;
    public Dictionary<int, GameDataSkill> skillSlots;
    public SkillBlueprint[] skills;
    public GameDataGear[] gear;
    public Dictionary<string, Dictionary<string, MapItemBase>> progress;
    public InventoryItem[] resources;
    public Parchment[] Parchments;
    public GameDataBarterOffer[] barterOffers;
}

[AutoRegister]
public class Storage
{
    private GameData _gameData;
    public GameData GameData {
        get => _gameData;
        set
        {
            Write(value);
            _gameData = value;
        }
    }

    public Storage()
    {
        _gameData = Read();
    }

    public GameData Read()
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
                    Name = "bareHandedStrike",
                    AP = 3,
                    Cooldown = 1,
                    Range = new Range() { Min = 1, Max = 1 },
                    Visibility = true,
                    TargetRadius = new Range() { Min = 1, Max = 1 },
                },
                new() {
                    Name = "tearDrop",
                    AP = 3,
                    Cooldown = 1,
                    Range = new Range() { Min = 1, Max = 1 },
                    Visibility = true,
                    TargetRadius = new Range() { Min = 1, Max = 1 },
                    SkillEffect = SkillEffects.CreateSurface,
                    SkillEffectConfiguration = new() {
                        {"element", new ReactiveState<Element>(Element.Water)}
                    }
                },
                new() {
                    Name = "earth",
                    AP = 3,
                    Cooldown = 1,
                    Range = new Range() { Min = 1, Max = 1 },
                    Visibility = true,
                    TargetRadius = new Range() { Min = 1, Max = 1 },
                    SkillEffect = SkillEffects.CreateSurface,
                    SkillEffectConfiguration = new() {
                        {"element", new ReactiveState<Element>(Element.Earth)}
                    }
                },
                new() {
                    Name = "air",
                    AP = 3,
                    Cooldown = 1,
                    Range = new Range() { Min = 1, Max = 1 },
                    Visibility = true,
                    TargetRadius = new Range() { Min = 1, Max = 1 },
                    SkillEffect = SkillEffects.CreateSurface,
                    SkillEffectConfiguration = new() {
                        {"element", new ReactiveState<Element>(Element.Air)}
                    }
                },
                new() {
                    Name = "fire",
                    AP = 3,
                    Cooldown = 1,
                    Range = new Range() { Min = 1, Max = 1 },
                    Visibility = true,
                    TargetRadius = new Range() { Min = 1, Max = 1 },
                    SkillEffect = SkillEffects.CreateSurface,
                    SkillEffectConfiguration = new() {
                        {"element", new ReactiveState<Element>(Element.Fire)}
                    }
                },
                new() {
                    Name = "test",
                    AP = 1, MP = 0, HP = 0,
                    Cooldown = 0,
                    Range = new Range() { Min = 1, Max = 20 },
                    Visibility = false,
                    TargetRadius = new Range() { Min = 1, Max = 1 },
                    SkillEffect = SkillEffects.DealDamage1,
                    SkillEffectConfiguration = new() {
                        {"damage", (new ReactiveState<int>(1), new ReactiveState<int>(5))},
                        {"element", new ReactiveState<Element>(Element.Fire)}
                    }
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
                // {1, new GameDataSkill(){ name = "interact", label = "interact" } },
                {1, new GameDataSkill(){ name = "tearDrop", label = "tearDrop" } },
                {2, new GameDataSkill(){ name = "test", label = "test" } },
                {3, new GameDataSkill(){ name = "earth", label = "earth" } },
                {4, new GameDataSkill(){ name = "air", label = "air" } },
                {5, new GameDataSkill(){ name = "fire", label = "fire" } },
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
            Parchments = new Parchment[]{
                Parchment.Randomize(),
                Parchment.Randomize(),
                Parchment.Randomize(),
                Parchment.Randomize(),
                Parchment.Randomize(),
            },
            progress = new Dictionary<string, Dictionary<string, MapItemBase>>
            {
                { "forest", new() {
                    {"forest1", new MapItemBase() { name = "forest1", status = ZoneStatus.Unlocked }}
                } }
            }
        };
    }

    public void Write(GameData data)
    {
        string path = PathForDocumentsFile();

        JSONFile.Write(path, data);
    }

    private string PathForDocumentsFile()
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
