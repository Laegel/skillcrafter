using System;
using System.Collections.Generic;
using System.ComponentModel;



public class GearReader
{

    public static Dictionary<string, Gear> Get()
    {

        string filePath = "res://resources/gear/gear.json";
        var allItems = JSON.Read<GearObject>(filePath).gear;

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
    Head,
    Torso,
    Arms,
    Legs,
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
    Worn,
    Common,
    Perfected,
    Masterpiece,
    Mythical,
    Unique,
}


[Serializable]
public class Gear
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
