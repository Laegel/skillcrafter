using System;
using System.Collections.Generic;
using System.ComponentModel;


public class ResourcesReader
{

    public static Dictionary<string, DisplayableResource> Get()
    {
        string filePath = "res://resources/resources/resources.json";
        var allItems = JSONFile.Read<ResourceObject>(filePath);

        var items = new Dictionary<string, DisplayableResource>();
        foreach (var item in allItems.items)
        {
            items.Add(item.name, item);
        }
        return items;
    }
}

[Serializable]
public enum Rarity
{
    [Description("banal")]
    Banal,
    [Description("common")]
    Common,
    [Description("uncommon")]
    Uncommon,
    [Description("rare")]
    Rare,
    [Description("mythical")]
    Mythical,
    [Description("unique")]
    Unique,
}


[Serializable]
public class DisplayableResource : StorageItem
{
    public string name;
    public string description;
    public Rarity rarity;
}


[Serializable]
public class ResourceObject
{
    public DisplayableResource[] items;
}
