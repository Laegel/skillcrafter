using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Godot;


public class MonstersReader
{

    public static Dictionary<string, Monster> Get()
    {
        string filePath = "res://resources/monsters/monsters.json";
        var allMonsters = JSONFile.Read<MonsterObject>(filePath);
        
        var monsters = new Dictionary<string, Monster>();
        foreach (var monster in allMonsters.monsters)
        {
            monsters.Add(monster.name, monster);
        }
        return monsters;
    }
}

[Serializable]
public class Loot
{
    public string name;
    public Range quantity;
}

[Serializable]
public class Monster
{
    public string name;
    public int health;
    public int ability;
    public int movement;
    public List<string> skills;
    public Dictionary<Element, int> resistance;
    public int armor;
    public int power;
    public Loot[] loot;
}


[Serializable]
public class MonsterObject
{
    public Monster[] monsters;
}
