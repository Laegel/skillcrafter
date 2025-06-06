using System;
using System.Collections.Generic;
using System.Linq;

using Godot;

public class Zone
{
    public static Zone Instance;


    public static void Instantiate()
    {
        if (Instance != null)
        {
            return;
        }
        Instance = new Zone();
        // var gameObject = new GameObject
        // {
        //     name = "ZonesManager"
        // };
        // Instance = gameObject.AddComponent<Zone>();
        // DontDestroyOnLoad(gameObject);
    }

    private void Awake()
    {
        if (Instance != null)
        {
            // this.CallDeferred("queue_free");
            // Destroy(this);
            return;
        }
        Instance = this;
        // DontDestroyOnLoad(gameObject);
    }

    public Dictionary<string, ZoneObject> Zones
    {
        get
        {
            var gameData = Storage.Read();
            return ZonesReader.Get();
            // return new OutZone
            // {
            //     maps = ZonesReader.Get("forest")
            //         .Where(item => gameData.progress["forest"].ContainsKey(item.Key))
            //         .ToDictionary(item => item.Key, item => ObjectMerger.MergeObjects(item.Value, new MapItem
            //         {
            //             status = gameData.progress["forest"][item.Key].status
            //         }))
            // };
        }
        set
        {
            zones = value;
            OnZonesChanged?.Invoke(zones);
        }
    }

    public event Action<Dictionary<string, ZoneObject>> OnZonesChanged;
    Dictionary<string, ZoneObject> zones;

    void Start()
    {
        var gameData = Storage.Read();
        zones = ZonesReader.Get();
        // zones = new OutZone
        // {
        //     maps = ZonesReader.Get("forest")
        //             .Where(item => gameData.progress["forest"].ContainsKey(item.Key))
        //             .ToDictionary(item => item.Key, item => ObjectMerger.MergeObjects(item.Value, new MapItem
        //             {
        //                 status = gameData.progress["forest"][item.Key].status
        //             }))
        // };
    }

    public (string, int) CurrentBattle
    {
        get { return currentBattle; }
        set
        {
            currentBattle = value;
            GD.Print("Changing currentBattle: " + currentBattle);
            OnCurrentBattleChanged?.Invoke(currentBattle);
        }
    }

    public event Action<(string, int)> OnCurrentBattleChanged;
    // [SerializeField]
    (string, int) currentBattle = ("forest", 0);

    public static void SetCurrentBattle(string zone, int index)
    {
        Instance.currentBattle = (zone, index);
    }
}

public class ZonesReader
{
    public static Dictionary<string, MapItem> GetByKey(string key)
    {
        string filePath = "res://resources/zones/" + key + ".json";

        var allZones = JSONFile.Read<ZoneObject>(filePath).maps;

        var maps = new Dictionary<string, MapItem>();
        foreach (var zone in allZones)
        {
            maps.Add(zone.name, zone);
        }
        return maps;
    }

    public static Dictionary<string, ZoneObject> Get()
    {
        var files = DirAccess.Open("res://resources/zones").GetFiles();

        return files.ToDictionary(file => file.Replace(".json", ""), file => JSONFile.Read<ZoneObject>(file));
    }
}

[Serializable]
public enum ZoneStatus
{
    Locked,
    Pending,
    Unlocked,
}

[Serializable]
public class Specifications
{
    public bool isIsland;
}

[Serializable]
public class MapItemBase
{
    public string name;
    public ZoneStatus status;
}


[Serializable]
public class MapItem : MapItemBase
{
    public string layout;
    public List<string> enemies;
    public List<ZoneContraption> contraptions;
    public Specifications? specs;
    public Dictionary<string, string> unlocks;
    public bool isHidden;
    public List<BarterOffer> barterOffers;
    public float difficulty;
    public Location location;
    public Dictionary<string, TriggerMap>? triggers;
    public Dictionary<string, DialogueMap>? dialogues;
}



[Serializable]
public class ZoneContraption
{
    public string name;
    public string kind;
    public List<string> triggers;
}

[Serializable]
public class Location
{
    public int x;
    public int y;
}

[Serializable]
public class ZoneObject
{
    public Location location;
    public MapItem[] maps;
    public string color;
}

public class OutZone
{
    public Dictionary<string, MapItem> maps;
}

public class TriggerMap
{
    public TriggerKind kind;
    public Dictionary<string, object> data;
}


public class Choice
{
    public string text;
    public TriggerMap trigger;
}

// [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
public class DialogueMap
{
    public string text;
    public List<Choice>? choices; 
    public Speaker? leftSpeaker;
  public Speaker? rightSpeaker;
//   variables = {},
}

public class Speaker {
    public string name;
    public bool isHighlighted;
}