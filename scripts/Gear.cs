using System;
using System.Collections.Generic;
using System.ComponentModel;



public class GearReader
{

    public static Dictionary<string, Gear> Get()
    {

        string filePath = "res://resources/gear/gear.json";
        var allItems = JSONFile.Read<GearObject>(filePath).gear;

        var items = new Dictionary<string, Gear>();
        foreach (var item in allItems)
        {
            items.Add(item.name, item);
        }
        return items;
    }
}

[Serializable]
public enum GearSlot
{
    [Description("head")]
    Head,
    [Description("torso")]
    Torso,
    [Description("arms")]
    Arms,
    [Description("legs")]
    Legs,
    [Description("heavy")]
    Weapon,
}

[Serializable]
public enum GearType
{
    [Description("heavy")]
    Heavy,
    [Description("light")]
    Light,
    [Description("clothes")]
    Clothes,
    [Description("weapon")]
    Weapon,
}


public enum Quality
{
    [Description("worn")]
    Worn,
    [Description("common")]
    Common,
    [Description("perfected")]
    Perfected,
    [Description("masterpiece")]
    Masterpiece,
    [Description("mythical")]
    Mythical,
    [Description("unique")]
    Unique,
}


[Serializable]
public class Gear : StorageItem
{
    public string name;
    public string set;
    public GearType type;
    public GearSlot slot;
    public string description;
    public Quality quality;
    public Ingredient[] ingredients;
    public string[] skills;

    public string GetGearString()
    {
        return $"{type.GetEnumDescription()}/{set}";
    }

    public int GetPower()
    {
        var rate = type switch
        {
            GearType.Heavy => 0.5,
            GearType.Light => 1,
            GearType.Clothes => 2,
        };
        return (int)(rate * 4);
    }

    public int GetArmor()
    {
        var rate = type switch
        {
            GearType.Heavy => 2,
            GearType.Light => 1,
            GearType.Clothes => 0.5,
        };
        return (int)(rate * 2);
    }
}


[Serializable]
public class GearObject
{
    public Gear[] gear;
}
