using System;
using System.Collections.Generic;


public class ResourcesReader
{

    public static Dictionary<string, DisplayableItem> Get()
    {
        string filePath = "res://resources/resources/resources.json";
        var allItems = JSON.Read<ItemObject>(filePath);

        var items = new Dictionary<string, DisplayableItem>();
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
    Banal,
    Common,
    Uncommon,
    Rare,
    Mythical,
    Unique,
}


[Serializable]
public class DisplayableItem
{
    public string name;
    public string description;
    public Rarity rarity;
}


[Serializable]
public class ItemObject
{
    public DisplayableItem[] items;
}
