using System;
using System.Collections.Generic;


public class ItemsReader
{

    public static Dictionary<string, Item> Get()
    {
        string filePath = "res://resources/items/items.json";
        var allItems = JSON.Read<ItemObject>(filePath);

        var items = new Dictionary<string, Item>();
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
public class Item
{
    public string name;
    public string description;
    public Rarity rarity;
}


[Serializable]
public class ItemObject
{
    public Item[] items;
}
