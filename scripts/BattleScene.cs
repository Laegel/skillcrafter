using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Godot;
using YATI;
using Map = System.Collections.Generic.Dictionary<string, MapCell>;
using TurnOrder = System.Collections.Generic.List<TurnState>;

public enum EquipmentSlot
{
    Head,
    Torso,
    Arms,
    Legs,
    Weapon,
}


public enum TurnStatus
{
    Pending,
    InProgress,
    Ended,
}

[Serializable]
public class TurnSkillRestrictions
{
    public int maxCooldown;
    public int cooldown;
    public int lastCastTime;
}
[Serializable]
public class TurnSideEffect
{
    public SideEffect effect;
    public int remainingTurns;
}
[Serializable]
public class TurnOverTimeEffect
{
    public Entity caster;
    public Damage damage;
    public int remainingTurns;
    public string name;
}

public class TurnStateOut
{
    public TurnStatus turnStatus;
    public int maxHealthPoints;
    public int healthPoints;
    public string name;
    public List<TurnSideEffect> sideEffects;
}

public enum Characteristic
{
    Evasion,
    Stability,
    MoveResistance,
}


public class TurnState
{
    public Observer<TurnStatus> turnStatus;
    public int maxMovementPoints;
    public int movementPoints;
    public int maxAbilityPoints;
    public int abilityPoints;
    public Entity entity;
    public Observer<Vector2I> cell;
    public List<TurnSideEffect> sideEffects;
    public List<TurnOverTimeEffect> overTimeEffects;
    public List<(TurnSkillRestrictions, SkillBlueprint)> skills;

    public TurnState(int maxMovementPoints, int maxAbilityPoints, Entity entity, Vector2I cell, List<TurnSideEffect> sideEffects, List<TurnOverTimeEffect> overTimeEffects, List<(TurnSkillRestrictions, SkillBlueprint)> skills)
    {
        this.maxMovementPoints = maxMovementPoints;
        movementPoints = maxMovementPoints;
        this.maxAbilityPoints = maxAbilityPoints;
        abilityPoints = maxAbilityPoints;
        this.entity = entity;
        turnStatus = new(TurnStatus.Pending);
        this.cell = new(cell);
        this.sideEffects = sideEffects;
        this.overTimeEffects = overTimeEffects;
        this.skills = skills;
    }

    public bool CanCast(SkillBlueprint skill)
    {
        return skills.Find(item => item.Item2.name == skill.name).Item1.cooldown == 0 &&
             abilityPoints >= skill.cost.ap &&
             movementPoints >= skill.cost.mp;
    }
}

public enum SkillAction
{
    Inactive,
    Skill0,
    Skill1,
    Skill2,
    Skill3,
    Skill4,

    SkillShove,
    SkillJump,
}

public enum Direction
{
    Left,
    Top,
    Right,
    Bottom
}


public partial class Interactive : Node2D
{
    public bool isHidden = false;
    public int detectionLevel = 1;
}

public abstract partial class Contraption : Interactive
{
    public bool isActive = false;
    abstract public List<(TriggerKind, Dictionary<string, object> data)> Trigger();
}


public class Observer<T>
{
    private T value;
    public Observer(T value)
    {
        this.value = value;
    }

    public T Value
    {
        get { return value; }
        set
        {
            var oldValue = this.value;
            this.value = value;
            Notify(value, oldValue);
        }
    }
    private List<Action<T, T>> subscribers = new();


    public void Subscribe(Action<T, T> subscriber)
    {
        subscriber(value, value);
        subscribers.Add(subscriber);
    }


    public void Unsubscribe(Action<T, T> subscriber)
    {
        subscribers.Remove(subscriber);
    }

    public void Notify(T newValue, T oldValue = default)
    {
        foreach (var subscriber in subscribers)
        {
            subscriber.Invoke(newValue, oldValue);
        }
    }
}

public class SurfaceStatus
{
    public Surface surface;
    public int turns = -1;
}

public enum EntitySlot
{
    PlayerSpawn, Player, Enemy, Entity,
}


public class MapCell
{
    public CellKind kind;
    public bool isOccupied = false;
    public SurfaceStatus? surfaceStatus;
    public EntitySlot? entitySlot;
    public Dictionary<string, string> metadata;
}

public enum CellKind
{
    [Description("-")]
    Out,
    [Description(" ")]
    Empty,
    [Description("X")]
    Obstacle,
    [Description("G")]
    Gap,
    [Description("W")]
    Wall,
    [Description("C")]
    Contraption,

}

public enum Surface
{
    [Description("Water")]
    Water,
    [Description("Poison")]
    Poison,
    [Description("Fire")]
    Fire,
    [Description("Radiating")]
    Radiating,
    [Description("Ice")]
    Ice,
    [Description("Fog")]
    Fog,
    [Description("Smoke")]
    Smoke,
}



public partial class BattleScene : Node2D
{
    [Export] public string command;
    public Entity playerEntity;
    private bool isMoving = false;
    public SkillAction isCastingSkill = SkillAction.Inactive;
    public event Action<SkillAction> OnIsCastingSkillChanged;
    public SkillAction IsCastingSkill
    {
        get { return isCastingSkill; }
        set
        {
            isCastingSkill = value;
            OnIsCastingSkillChanged?.Invoke(isCastingSkill);
        }
    }

    public string? focusedInteractive;
    public event Action<string?> OnFocusedInteractiveChanged;
    public string? FocusedInteractive
    {
        get { return focusedInteractive; }
        set
        {
            focusedInteractive = value;
            OnFocusedInteractiveChanged?.Invoke(focusedInteractive);
        }
    }

    private int turnCounter = 0;
    public List<Entity> turnEntities;
    private int turnEntityIndex = 0;
    public event Action<List<TurnStateOut>> OnTurnOrderChanged;
    private List<TurnStateOut> turnOrderOut;

    public List<TurnStateOut> TurnOrder
    {
        get { return turnOrderOut; }
        set
        {
            turnOrderOut = value;
            OnTurnOrderChanged?.Invoke(turnOrderOut);
        }
    }

    private int refresh;
    public event Action<int> OnRefreshChanged;
    public int Refresh
    {
        get { return refresh; }
        set
        {
            refresh = value;
            OnRefreshChanged?.Invoke(refresh);
        }
    }

    private List<(string, Dictionary<string, string>)> logs = new();
    public event Action<string> OnLogsChanged;

    public List<(string, Dictionary<string, string>)> Logs
    {
        get { return logs; }
        set
        {
            logs = value;
            var output = "[\"" + String.Join("\", \"", logs) + "\"]";
            OnLogsChanged?.Invoke(output);
        }
    }

    private Entity currentPlayableEntity;
    public event Action<Entity> OnCurrentPlayableEntityChanged;

    public Entity CurrentPlayableEntity
    {
        get { return currentPlayableEntity; }
        set
        {
            currentPlayableEntity = value;
            OnCurrentPlayableEntityChanged?.Invoke(currentPlayableEntity);
        }
    }

    private List<Entity> playableEntities;
    public event Action<List<Entity>> OnPlayableEntitiesChanged;

    public List<Entity> PlayableEntities
    {
        get { return playableEntities; }
        set
        {
            playableEntities = value;
            OnPlayableEntitiesChanged?.Invoke(playableEntities);
        }
    }

    private (int, int) skillCooldown;
    public event Action<(int, int)> OnSkillCooldownChanged;

    public (int, int) SkillCooldown
    {
        get { return skillCooldown; }
        set
        {
            skillCooldown = value;
            OnSkillCooldownChanged?.Invoke(skillCooldown);
        }
    }

    private void AppendLogs(string line, Dictionary<string, string> values)
    {
        logs.Add((line, values));
        Logs = logs;
    }

    // Map
    private TileMapLayer tilemap;
    private TileMapTerrain scope;
    private TileMapTerrain validationScope;
    public event Action<BattleStatus> OnStatusChanged;
    public BattleStatus Status
    {
        get { return status; }
        set
        {
            status = value;
            OnStatusChanged?.Invoke(status);
        }
    }
    public BattleStatus status = BattleStatus.Exploring;

    public string assetBundlePath = "Assets/StreamingAssets"; // Path to the folder containing AssetBundles
    GameData gameData;

    public string map = "";
    private List<Entity> enemyPool;
    private List<Entity> entities;
    private List<List<Entity>> sectionEntities;
    private List<string> enemyNames;
    public Dictionary<string, SkillBlueprint> allSkills;
    public Dictionary<string, Gear> allGear;

    private Map currentMap = new();
    private MapItem currentMapItem;

    private Dictionary<string, Monster> monsters;
    private Dictionary<string, Gear> gear;
    private TurnOrder enemies = new();
    private float difficulty;

    public readonly MapCell[] skillCommonCells = new[] {  new MapCell(){
        kind = CellKind.Empty,
        isOccupied = true,
    }, new MapCell(){
        kind = CellKind.Empty,
        isOccupied = false,
    }, new MapCell(){
        kind = CellKind.Gap,
        isOccupied = false,
    }, new MapCell(){
        kind = CellKind.Contraption,
        isOccupied = true,
    } };

    private Vector2I? validateCastingSkillTargetCell;
    private Dictionary<string, MapItem> maps;
    public event Action<GameData> OnGameDataStorageChanged;
    public GameData GameDataStorage
    {
        get
        {
            return gameData;
        }
        set
        {
            GD.Print("Writing to storage");
            gameData = value;
            Storage.Write(gameData);
            OnGameDataStorageChanged?.Invoke(gameData);
        }
    }




    public override void _Ready()
    {
        DisplayServer.ScreenSetOrientation(DisplayServer.ScreenOrientation.Landscape);
        // Screen.orientation = ScreenOrientation.LandscapeLeft;
        GD.Print("Loading document manager...");
        // DocumentManager.Create("Battle", "Default");
        // Instantiate(Resources.Load<GameObject>("Common/Prefabs/ScriptEngine"));
        GD.Print("Done loading document manager");

        GD.Print("Loading data...");
        Zone.Instantiate();
        maps = ZonesReader.GetByKey(Zone.Instance.CurrentBattle.Item1);
        monsters = MonstersReader.Get();
        items = ResourcesReader.Get();
        gear = GearReader.Get();
        allSkills = SkillsReader.Get();
        allGear = GearReader.Get();
        gameData = Storage.Read();
        GD.Print("Done loading data");
        SkillSlotsState.equipedSkills.Value = gameData.skillSlots.Select(x => x.Value == null ? (null, null) : (new TurnSkillRestrictions() { cooldown = 0 }, allSkills[x.Value.name])).ToList();
        GearSlotState.equipedGear.Value = gameData.gearSlots;

        GD.Print("Creating player character portrait...");
        CreatePlayerCharacterForPortrait();
        GD.Print("Done creating player portrait");

        GD.Print("Finishing awakening...");
        scope = new TileMapTerrain(GetNode<TileMapLayer>("Grid/Scope"));
        validationScope = new TileMapTerrain(GetNode<TileMapLayer>("Grid/ValidationScope"));
        var currentZone = Zone.Instance.CurrentBattle;
        currentMapItem = maps[Zone.Instance.CurrentBattle.Item1 + (currentZone.Item2 + 1)];

        tilemap = GetNode<TileMapLayer>("Grid/Ground");
        BattleMap.tilemap = tilemap;

        // var audioSource = this.AddComponent<AudioSource>();
        // audioSource.clip = Resources.Load<AudioClip>("Music/" + Zone.Instance.CurrentBattle.Item1);
        // audioSource.Play();
        // audioSource.loop = true;

        enemyNames = currentMapItem.enemies.ToList();
        // enemyPool = currentMapItem.enemies.Select(x => Resources.Load<GameObject>("Battle/Prefabs/Entities/skeleton-" + x)).ToList();

        for (var sectionIndex = 1; sectionIndex < 5; sectionIndex++)
        {
            var prefix = Zone.Instance.CurrentBattle.Item1 + (Zone.Instance.CurrentBattle.Item2 + 1);
            LoadSection(prefix, sectionIndex);
            if (sectionIndex == 1)
            {
                GetNode<TileMapLayer>("Grid/Ground").TileSet = GetNode<TileMapLayer>(prefix + "-" + sectionIndex + "/Ground").TileSet;
            }
            var newTiles = TilemapExtractor.ExtractTiles(GetNode<Node2D>(prefix + "-" + sectionIndex), GetNode<Node2D>("Grid"), sectionIndex);

            // var linkedSectionCells = GetMapCellsBy(x => x.metadata.ContainsKey("section") && x.metadata["section"] == sectionIndex - 1 + "-" + sectionIndex + ":a");
            // foreach (var kvp in linkedSectionCells)
            // {
            //     var arrow = GetNode("arrow:" + kvp.Key);
            //     arrow.SetProcess(false);
            // }
            SpawnEntities(newTiles);
            if (sectionIndex == 1)
            {
                PreparePlayableEntities(newTiles);
            }

            foreach (var kvp in newTiles)
            {
                if (!currentMap.ContainsKey(kvp.Key) || currentMap[kvp.Key].kind == CellKind.Gap)
                    currentMap[kvp.Key] = kvp.Value;
            }
        }


        foreach (var gearSlot in gameData.gearSlots)
        {
            if (gearSlot.Value != null)
            {
                var gearItem = allGear[gearSlot.Value.name];
                (playerEntity as Humanoid).SetGear(gearSlot.Key, gearItem.GetGearString());
            }
        }
        GearSlotState.equipedGear.OnValueChanged += (gearSlots) =>
        {
            foreach (var gearSlot in gearSlots)
            {
                if (gearSlot.Value != null)
                {
                    var gearItem = allGear[gearSlot.Value.name];
                    (playerEntity as Humanoid).SetGear(gearSlot.Key, gearItem.GetGearString());
                }
                else
                {
                    (playerEntity as Humanoid).SetGear(gearSlot.Key, null);
                }
            }
        };

        LoadUI();
        GD.Print("Awakening done");
    }



    private void LoadUI()
    {
        var battlePanel = new BattlePanel((index) =>
        {
            if (MenuState.currentMenu.Value == Menus.Equipment)
            {
                ItemFilterState.Set(ItemType.Skill, index);
            }
            else
            {
                GD.Print("Skill pressed: " + index);
                if (playerEntity.skills.Count - 1 < index)
                {
                    return;
                }
                PressSkillButton((SkillAction)index + 1);
            }
        });
        AddChild(battlePanel.Build());
        var menuContainer = new MenuContainer();
        AddChild(menuContainer.Build());
        var menuButton = new MenuButton().Build();
        AddChild(menuButton);
    }

    private void Start()
    {
        // nextTickTime = Time.time + tickRate;

    }


    public string GetGearPath(Gear item)
    {
        string setName = item.set;

        if (item.type != GearType.Weapon)
        {
            string folder = GearTypeToFolder(item.type);
            return $"{folder}/{setName}";
        }
        else
        {
            return $"{setName}/{item.name}";
        }
    }
    public static string GetFullGearPath(Gear item)
    {
        string setName = item.set;

        if (item.type != GearType.Weapon)
        {
            string folder = GearTypeToFolder(item.type);
            string slotFolder = GearSlotToFolder(item.slot);
            return $"{folder}/{setName}/{slotFolder}";
        }
        else
        {
            return $"{setName}/{item.name}";
        }
    }

    private OverTimeEffect? GetGearSetEffect(Dictionary<GearSlot, GameDataGear> gearSlots)
    {
        Dictionary<GearType, int> sameSetCount = new()
        {
            { GearType.Heavy, 0 },
            { GearType.Light, 0 },
            { GearType.Clothes, 0 }
        };
        foreach (GearSlot slot in Enum.GetValues(typeof(EquipmentSlot)))
        {
            if (slot == GearSlot.Weapon)
            {
                continue;
            }
            if (!gearSlots.TryGetValue(slot, out var value))
            {
                continue;
            }
            var type = gear.First((x) => x.Value.name == value.name).Value.type;
            sameSetCount[type]++;

        }
        if (sameSetCount[GearType.Heavy] >= 4)
        {
            return new OverTimeEffect() { name = "Heavy", duration = -1 };
        }
        else if (sameSetCount[GearType.Light] >= 4)
        {
            return new OverTimeEffect() { name = "Evasion", duration = -1 };
        }
        else if (sameSetCount[GearType.Clothes] >= 4)
        {
            return new OverTimeEffect() { name = "Focus", duration = -1 };
        }

        return null;
    }

    private void PreparePlayableEntities(Map currentSection)
    {
        foreach (KeyValuePair<string, MapCell> cell in currentSection)
        {
            if (!cell.Value.isOccupied || cell.Value.metadata.ContainsKey("contraption") || cell.Value.metadata.ContainsKey("entity") && cell.Value.metadata["entity"] != "Player")
            {
                continue;
            }
            var position = GridHelper.StringToVector2I(cell.Key);

            playerEntity = LoadEntity(cell.Value.metadata["entity"], position);
            // var humanoidPrefab = GD.Load<PackedScene>("res://scenes/humanoid.tscn");
            // playerEntity = humanoidPrefab.Instantiate<Humanoid>();
            playerEntity.Name = "Player";
            // player.tag = "PlayableEntity";


            var totalArmor = Enumerable.Range(0, 4).Aggregate(0, (acc, x) =>
            {
                var gearSlot = gameData.gearSlots[(GearSlot)x];
                return acc + (gearSlot != null ? allGear[gearSlot.name].GetArmor() : 0);
            });

            var totalPower = Enumerable.Range(0, 4).Aggregate(0, (acc, x) =>
            {
                var gearSlot = gameData.gearSlots[(GearSlot)x];
                return acc + (gearSlot != null ? allGear[gearSlot.name].GetPower() : 0);
            });

            playerEntity.armor += totalArmor;
            playerEntity.power += totalPower;

            var gearSetEffect = GetGearSetEffect(gameData.gearSlots);
            if (gearSetEffect != null)
            {
                playerEntity.overTimeEffects.Add(new TurnOverTimeEffect()
                {
                    name = gearSetEffect.name,
                    remainingTurns = gearSetEffect.duration
                });
            }

            playerEntity.skills = SetSkillsFromGear();

            AddChild(playerEntity);
        }
    }

    public List<(TurnSkillRestrictions, SkillBlueprint)?> SetSkillsFromGear()
    {
        var skills1 = gameData.skillSlots.ToList();
        var skills = skills1.Select<KeyValuePair<int, GameDataSkill>, (TurnSkillRestrictions, SkillBlueprint)?>(x => x.Value == null ? null : (new TurnSkillRestrictions() { cooldown = 0 }, allSkills[x.Value.name])).ToList();
        if (gameData.gearSlots.TryGetValue(GearSlot.Weapon, out var weaponName) && weaponName != null)
        {
            var weapon = allGear[weaponName.name];
            skills[skills.FindIndex((x) => x != null && x.Value.Item2.name == "bareHandedStrike")] = (new TurnSkillRestrictions() { cooldown = 0 }, allSkills[weapon.skills[0]]);
        }
        return skills;
    }

    private void SpawnNonPlayableEntities(List<Entity> enemyPool, Map currentSection)
    {
        var entityCounter = 0;
        foreach (KeyValuePair<string, MapCell> cell in currentSection)
        {
            if (cell.Value.isOccupied == false || cell.Value.entitySlot != EntitySlot.Enemy)
            {
                continue;
            }
            // var position = GridHelper.StringToVector2I(cell.Key);
            // GameObject enemy = Instantiate(enemyPool[entityCounter]);
            // var monster = monsters[enemyNames[entityCounter]];
            // enemy.name = enemyNames[entityCounter] + ":" + entityCounter;

            // MoveRendererToCell(enemy.transform, Caster.Into(position));
        }
    }


    public static string GearTypeToFolder(GearType gearType)
    {
        return gearType switch
        {
            GearType.Heavy => "heavy",
            GearType.Light => "light",
            GearType.Clothes => "clothes",
            _ => throw new ArgumentOutOfRangeException(nameof(gearType), gearType, null),
        };
    }

    public static string GearSlotToFolder(GearSlot gearSlot)
    {
        return gearSlot switch
        {
            GearSlot.Head => "head",
            GearSlot.Torso => "torso",
            GearSlot.Arms => "arms",
            GearSlot.Legs => "legs",
            GearSlot.Weapon => "weapon",
            _ => throw new ArgumentOutOfRangeException(nameof(gearSlot), gearSlot, null),
        };
    }

    // bool RandomPoint(Vector3 center, float range, out Vector3 result)
    // {
    //     for (int i = 0; i < 30; i++)
    //     {
    //         Vector3 randomPoint = center + Random.insideUnitSphere * range;
    //         NavMeshHit hit;
    //         if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas))
    //         {
    //             result = hit.position;
    //             return true;
    //         }
    //     }
    //     result = Vector3.zero;
    //     return false;
    // }

    public bool CreatePlayerCharacterForPortrait()
    {
        // var playerCharacter = Instantiate(
        //     Resources.Load<GameObject>("Battle/Prefabs/Entities/skeleton-Player")
        // );
        // playerCharacter.name = "PlayerPortrait";
        // playerCharacter.transform.position = new Vector3(1000, 1000, 0);
        // var skeleton = ((SpineSprite)playerCharacter.GetNode("skeleton")).GetAnimationState();
        // skeleton.SetTimeScale(0);
        // SetCompleteEquipment(playerCharacter);
        // GD.Print("PlayerPortrait created");
        return true;
    }

    public void PressMovementButton()
    {
        if (IsCastingSkill != SkillAction.Inactive)
        {
            IsCastingSkill = SkillAction.Inactive;
            DestroyTargetCells();
            DestroyValidationCells();
        }
        if (!isMoving)
        {
            var entity = playerEntity;

            DrawTargetCells(BattleMap.EntityPositionToCellCoordinates(entity.Position), 1, entity.movementPoints, new[] { new MapCell() {
                kind = CellKind.Empty,
                isOccupied = false
            }, new MapCell() {
                kind = CellKind.Contraption,
                isOccupied = false
            } }, SCTheme.Movement);
            // StartCoroutine(SetIsMoving(0.2f));
            isMoving = true;

        }
        else
        {
            DestroyTargetCells();
            isMoving = false;
        }

    }


    // IEnumerator SetIsMoving(float delay)
    // {
    //     yield return new WaitForSeconds(delay);
    //     isMoving = true;
    // }

    private Vector3 EntityPositionToCellPosition(Vector3 position)
    {
        return position;
    }

    // private Vector3I EntityPositionToCellCoordinates(Vector3 position)
    // {
    //     return tilemap.LocalToMap(EntityPositionToCellPosition(position));
    // }

    private Vector3 CellPositionToEntityPosition(Vector3 position)
    {
        // return new Vector3(position.X, position.Y, Math.Abs(position.Y) / 100);
        return position;
    }

    // private Vector3 CellCoordinatesToEntityPosition(Vector3I position)
    // {
    //     return CellPositionToEntityPosition(tilemap.LocalToMap(position));
    // }


    public void PressSkillButton(SkillAction pickedSkill)
    {
        HandleSkillPressed(pickedSkill);
    }

    public void PressEndTurnButton()
    {
        OnTurnEntityEnd();
        IsCastingSkill = SkillAction.Inactive;
        DestroyTargetCells();
        DestroyValidationCells();
        RefreshTurnOrder();
    }

    private Vector2I[] GetNeighborPositions(Vector2I cellPosition)
    {
        Vector2I[] neighborPositions = new Vector2I[]
        {
            cellPosition + new Vector2I(1, 0),        // Right
            cellPosition + new Vector2I(-1, 0),       // Left
            cellPosition + new Vector2I(0, 1),        // Up
            cellPosition + new Vector2I(0, -1),       // Down
            cellPosition + new Vector2I(1, -1),     // Top Right
            cellPosition + new Vector2I(-1, -1),    // Top Left
            cellPosition + new Vector2I(1, 1),      // Bottom Right
            cellPosition + new Vector2I(-1, 1)      // Bottom Left
        };

        return neighborPositions;
    }

    private Vector2I[] GetCellsInSquare(Vector2 centerCellPosition, float radius)
    {
        // Calculate the number of cells needed in each direction based on the radius and cell sizes
        int cellsInRadiusX = Mathf.CeilToInt(radius / 1);
        int cellsInRadiusY = Mathf.CeilToInt(radius / 1);

        // Calculate the starting position for the loop
        int startX = Mathf.FloorToInt(centerCellPosition.X - cellsInRadiusX);
        int startY = Mathf.FloorToInt(centerCellPosition.Y - cellsInRadiusY);

        // Create a list to store the resulting cell positions
        var cellPositions = new List<Vector2I>();

        // Loop through the cells within the radius and add their positions to the list
        for (int x = startX; x <= startX + 2 * cellsInRadiusX; x++)
        {
            for (int y = startY; y <= startY + 2 * cellsInRadiusY; y++)
            {
                var cellPosition = new Vector2I(x, y);
                cellPositions.Add(cellPosition);
            }
        }

        return cellPositions.ToArray();
    }

    public Vector2I[] GetCellsInRadius(Vector2I centerCellPosition, int minRadius, int maxRadius)
    {
        var cellPositions = new List<Vector2I>();

        if (minRadius == 0)
        {
            cellPositions.Add(centerCellPosition);
        }

        for (int x = -maxRadius; x <= maxRadius; x++)
        {
            for (int y = -maxRadius; y <= maxRadius; y++)
            {
                int distance = Mathf.Abs(x) + Mathf.Abs(y);
                if (distance >= minRadius && distance <= maxRadius)
                {
                    var cellPosition = centerCellPosition + new Vector2I(x, y);
                    cellPositions.Add(cellPosition);
                }
            }
        }

        return cellPositions.ToArray();
    }

    private Vector2 WorldToTilePosition(Vector2 position)
    {
        var scope = GetNode<TileMapLayer>("Grid/Scope");
        return scope.MapToLocal((Vector2I)position);
    }

    // private Vector3 NormalizeWorldPosition(Vector3 position)
    // {
    //     var scope = GetNode<TileMapLayer>("Scope");
    //     return scope.LocalToMap(scope.LocalToMap(position));
    // }

    // public static Vector3 GetTilemapCenter()
    // {
    //     var tilemap = GetNode<TileMapLayer>("Grid");
    //     var cellPosition = new Vector3I(tilemap.cellBounds.size.X / 2, tilemap.cellBounds.size.Y / 2, 0);
    //     return tilemap.LocalToMap(cellPosition);
    // }

    private Vector2 TileToWorldPosition(Vector2I tilePosition)
    {
        var scope = GetNode<TileMapLayer>("Tilemap");
        return scope.LocalToMap(tilePosition);
    }

    private void UpdateMapCell(int x, int y, MapCell value)
    {
        currentMap[CoordsToString(x, y)] = value;
    }

    private void UpdateMapCell(Vector2I cell, MapCell value)
    {
        currentMap[CoordsToString(cell.X, cell.Y)] = value;
    }

    public MapCell ReadMapCell(int x, int y)
    {
        return currentMap[CoordsToString(x, y)];
    }

    public MapCell ReadMapCell(Vector2I cell)
    {
        return currentMap[Vector2ToString(cell)];
    }

    private void SetMapCellSurface(int x, int y, SurfaceStatus surfaceStatus)
    {
        currentMap[new Vector3I(x, y, 0).ToString()].surfaceStatus = surfaceStatus;
    }


    private void DrawTargetCells(Vector2I position, int fromRadius, int radius, MapCell[] includableCells, Color? color = null, bool requiresVision = false)
    {
        // var ruleTile = Resources.Load<RuleTile>("Highlight");
        scope.tileMapLayer.Modulate = color ?? new Color(1, 1, 1);

        var cells = FindReachableCells(position, fromRadius, radius, includableCells, requiresVision);
        foreach (var cell in cells)
        {
            scope.PlaceTile(cells, cell);
        }
    }

    private void DestroyTargetCells()
    {
        var scope = GetNode<TileMapLayer>("Grid/Scope");
        scope.Clear();
    }

    private void DrawValidationCells(Vector2I position, int fromRadius, int radius, MapCell[] includableCells, Color? color = null)
    {
        // var ruleTile = Resources.Load<RuleTile>("Highlight");
        // validationScope.Modulate = color ?? new Color(1, 1, 1);

        var cells = FindReachableCells(position, fromRadius, radius, includableCells);
        // foreach (var cell in cells)
        // {
        //     validationScope.SetCell(validationScope.MapToLocal(tilemap.LocalToMap(cell)), ruleTile);
        // }
    }

    private void DestroyValidationCells()
    {
        var scope = GetNode<TileMapLayer>("Grid/ValidationScope");
        scope.Clear();
    }


    private void HandleSkillPressed(SkillAction pickedSkill)
    {
        validateCastingSkillTargetCell = null;
        DestroyValidationCells();
        HideMovementRange();
        if (isMoving)
        {
            isMoving = false;
            DestroyTargetCells();
        }
        if (pickedSkill == IsCastingSkill)
        {
            DestroyTargetCells();
            IsCastingSkill = SkillAction.Inactive;
            if (status != BattleStatus.Exploring)
            {
                DisplayMovementRange(playerEntity);
            }
            return;
        }
        else if (IsCastingSkill != SkillAction.Inactive)
        {
            DestroyTargetCells();
        }
        IsCastingSkill = pickedSkill;

        var skillDefinition = playerEntity.skills[(int)pickedSkill - 1].Value.Item2;
        DrawTargetCells(BattleMap.EntityPositionToCellCoordinates(playerEntity.Position), skillDefinition.range.min, skillDefinition.range.max, GetTargetableCells(skillDefinition), SCTheme.Ability, skillDefinition.vision == 1);
    }

    public void PerformMovement(Entity entity, Vector2I[] path, Action onFinish)
    {
        DoMovement(path, entity, onFinish);
    }

    private Map GetMapCellsBy(Predicate<MapCell> predicate)
    {
        var map = new Map();
        foreach (var cell in currentMap)
        {
            if (predicate(cell.Value))
            {
                map[cell.Key] = cell.Value;
            }
        }
        return map;
    }

public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed)
        {
            if (keyEvent.Keycode == Key.C)
            {
                var point = GetGlobalMousePosition();
                var cellPos = tilemap.LocalToMap(point);
                GD.Print(cellPos);
            }
            else if (keyEvent.Keycode == Key.D)
            {
                DisplayInput();
            }
        }
    }

    private void DisplayInput()
    {
        var text = new TextEdit() {
            Name = "InputDisplay",
            CustomMinimumSize = new Vector2(100, 50),
        };
        var button = new Button();
        button.Pressed += () =>
        {
            SendInput();  
        };
        AddChild(text);
        AddChild(button);
    }

    private void SendInput()
    {
        var node = GetNode<TextEdit>("InputDisplay");
        var value = node.Text;
        GD.Print(value);
    }

    public void DoMovement(Vector2I[] path, Entity entity, Action onFinish)
    {
        var previousCell = BattleMap.EntityPositionToCellCoordinates(entity.Position);
        entity.Move();

        foreach (var cell in path)
        {
            entity.LookAt(cell);
            MoveRendererToCell(entity, cell);
            var mapCell = ReadMapCell(cell);
            var surface = mapCell.surfaceStatus?.surface;

            if (status != BattleStatus.Exploring)
            {
                entity.movementPoints -= 1;
            }
            OnChangeCell(previousCell, cell);
            previousCell = cell;
            RefreshTurnOrder();
            if (surface != null && TriggerSurfaceEnter(entity, surface))
            {
                break;
            }


            if (mapCell.metadata.ContainsKey("section"))
            {
                var chunks = mapCell.metadata["section"].Split(':');
                var nextSection = chunks[0].Split('-');
                if (!IsSectionLoaded(Zone.Instance.CurrentBattle.Item1 + (Zone.Instance.CurrentBattle.Item2 + 1), int.Parse(nextSection[1])))
                {
                    var newSection = LoadSection(Zone.Instance.CurrentBattle.Item1 + (Zone.Instance.CurrentBattle.Item2 + 1), int.Parse(nextSection[1]));
                    var newTiles = TilemapExtractor.ExtractTiles(newSection.GetChild<TileMapLayer>(0), GetNode<TileMapLayer>("Grid"), int.Parse(nextSection[1]));
                    var linkedSectionCells = GetMapCellsBy(x => x.metadata.ContainsKey("section") && x.metadata["section"] == mapCell.metadata["section"]);
                    foreach (var kvp in linkedSectionCells)
                    {
                        var arrow = GetNode("arrow:" + kvp.Key);
                        arrow.SetProcess(false);
                    }

                    SpawnEntities(newTiles);
                    foreach (var kvp in newTiles)
                    {
                        if (!currentMap.ContainsKey(kvp.Key) || currentMap[kvp.Key].kind == CellKind.Gap)
                            currentMap[kvp.Key] = kvp.Value;
                    }

                    if (mapCell.metadata.ContainsKey("triggers"))
                    {
                        // var trigger = currentMapItem.triggers[mapCell.metadata["triggers"]].Copy();
                        // Trigger(trigger.kind, trigger.data);
                    }
                }
            }

            // yield return new WaitForSeconds(0.2f);
        }
        entity.Stop();
        onFinish();
    }

    public Dictionary<string, object> ObjectToDictionary(object obj)
    {
        return obj.GetType().GetFields()
            .Where(prop => prop.GetValue(obj) != null)
            .Select(prop => new { Key = prop.Name, Value = prop.GetValue(obj) })
            .ToDictionary(x => x.Key, x => x.Value);
    }

    // public RenderTexture RenderTexture(GameObject target)
    // {
    //     var cameraGO = new GameObject();
    //     var camera = cameraGO.AddComponent<Camera>();
    //     camera.orthographic = true;
    //     camera.orthographicSize = .2f;
    //     var renderTexture = new RenderTexture(512, 512, 24);

    //     camera.targetTexture = renderTexture;
    //     camera.clearFlags = CameraClearFlags.Depth;
    //     camera.renderingPath = RenderingPath.Forward;
    //     camera.cullingMask = 1 << target.layer;
    //     camera.depth = -1;
    //     camera.backgroundColor = new Color(0f, 0f, 0f, 0f);
    //     camera.stereoSeparation = 0f;
    //     camera.nearClipPlane = 0.3f;
    //     camera.farClipPlane = 1000f;
    //     camera.fieldOfView = 4f;
    //     camera.aspect = 1f;
    //     camera.transform.position = new Vector3(target.transform.position.X, target.transform.position.Y + 1.6f, -10f);
    //     camera.Render();
    //     camera.targetTexture = null;
    //     Destroy(cameraGO);
    //     return renderTexture;
    // }

    // public RenderTexture RenderGrid(Range rangeRadius, Range targetRadius)
    // {
    //     var target = GameObject.Find("DesignerGrid");
    //     var ruleTile = Resources.Load<RuleTile>("Highlight");
    //     var scope = GameObject.Find("DesignerGrid/Scope1").GetComponent<TileMapLayer>();
    //     scope.color = new Color(1f, 0.843f, 0f);
    //     var validationScope = GameObject.Find("DesignerGrid/ValidationScope1").GetComponent<TileMapLayer>();
    //     validationScope.color = Color.green;

    //     scope.Clear();
    //     validationScope.Clear();
    //     var initialCell = new Vector2I(0, 0);

    //     var rangeCells = GetCellsInRadius(initialCell, rangeRadius.min - 1, rangeRadius.max - 1).ToList();

    //     foreach (var cell in rangeCells)
    //     {
    //         scope.SetCell(new Vector3I(cell.X, cell.Y, 0), ruleTile);
    //     }


    //     var targetCells = GetCellsInRadius(initialCell, targetRadius.min - 1, targetRadius.max - 1).ToList();

    //     foreach (var cell in targetCells)
    //     {
    //         validationScope.SetCell(new Vector3I(cell.X, cell.Y, 0), ruleTile);
    //     }

    //     var camera = GameObject.Find("Main Camera1").GetComponent<Camera>();

    //     var renderTexture = new RenderTexture(1024, 512, 24);

    //     camera.targetTexture = renderTexture;

    //     camera.transform.position = new Vector3(target.transform.position.X, target.transform.position.Y, -13f);
    //     camera.Render();
    //     return renderTexture;
    // }


    public void Trigger(TriggerKind triggerKind, Dictionary<string, object> data)
    {
        // switch (triggerKind)
        // {
        //     case TriggerKind.Dialogue:
        //         Dictionary<string, object> dict;
        //         if (data.ContainsKey("ref"))
        //         {
        //             var zonePrefix = Zone.Instance.CurrentBattle.Item1 + (Zone.Instance.CurrentBattle.Item2 + 1) + ".";
        //             var dialogue = currentMapItem.dialogues[data["ref"].ToString()];
        //             dialogue.text = zonePrefix + dialogue.text;
        //             dialogue.choices?.ForEach(choice =>
        //                 {
        //                     choice.text = zonePrefix + choice.text;
        //                 });
        //             dict = ObjectToDictionary(dialogue);
        //         }
        //         else
        //         {
        //             dict = data;
        //         }

        //         DocumentManager.GetInstance().Dialogue = dict;
        //         break;
        //     case TriggerKind.Battle:
        //         List<string> enemies = Json.Parse(data["enemies"].ToString());
        //         var involvedEntities = enemies.Select(x =>
        //         {
        //             var entity = GetEntityByName(x).GetComponent<Entity>();
        //             entity.disposition = Disposition.Enemy;
        //             return entity;
        //         }).ToList();
        //         if (data.ContainsKey("allies"))
        //         {
        //             List<string>allies = JsonConvert.Parse(data["allies"].ToString());
        //             allies.ForEach(x =>
        //             {
        //                 var entity = GetEntityByName(x).GetComponent<Entity>();
        //                 entity.disposition = Disposition.Ally;
        //                 involvedEntities.Add(entity);
        //             });
        //         }
        //         involvedEntities.Add(player.GetComponent<Entity>());
        //         PrepareBattle(involvedEntities);
        //         StartBattle();
        //         break;
        //     case TriggerKind.OpenBarterOffers:
        //         DocumentManager.GetInstance().BarterOffers = currentMapItem.barterOffers.ToArray();
        //         break;
        //     case TriggerKind.SpawnEntities:
        //         break;
        //     case TriggerKind.UnlockZone:
        //         break;
        //     case TriggerKind.UnlockMap:
        //         break;
        //     case TriggerKind.AddToInventory:
        //         var item = data["item"].ToString().Split('.');
        //         switch (item[0])
        //         {
        //             case "gear":
        //                 var gameData = GameDataStorage;
        //                 var tempGear = gameData.gear.ToList();
        //                 tempGear.Add(new() { name = item[1] });
        //                 gameData.gear = tempGear.ToArray();
        //                 GameDataStorage = gameData;
        //                 break;
        //             default:
        //                 break;
        //         }
        //         break;
        //     case TriggerKind.DiscardGameObject:
        //         var go = GameObject.Find(data["name"].ToString());
        //         FadeOut(go.transform.GetChild(0).GetChild(0).GetChild(0).transform, () => { });
        //         var coordinates = (Vector2I)WorldToTilePosition(go.transform.GetChild(0).GetChild(0).transform.position);
        //         var previousCell = ReadMapCell(coordinates);
        //         previousCell.isOccupied = false;
        //         break;
        //     default:
        //         break;
        // }
    }

    private void OnChangeCell(Vector2I oldCell, Vector2I newCell)
    {
        var value = ReadMapCell(oldCell);
        UpdateMapCell(oldCell.X, oldCell.Y, new MapCell()
        {
            isOccupied = false,
            kind = value.kind,
            surfaceStatus = value.surfaceStatus,
            metadata = value.metadata,
        });
        var value1 = ReadMapCell(newCell);
        value1.metadata["entity"] = value.metadata["entity"];
        value.metadata.Remove("entity");
        UpdateMapCell(newCell.X, newCell.Y, new MapCell()
        {
            isOccupied = true,
            kind = value1.kind,
            surfaceStatus = value1.surfaceStatus,
            metadata = value1.metadata,
        });
    }

    private void OnTurnEntityEnd()
    {
        if (turnEntityIndex >= turnEntities.Count() - 1)
        {
            turnEntityIndex = 0;
            ++turnCounter;

            turnEntities.ForEach(entity =>
            {
                entity.abilityPoints = entity.maxAbilityPoints;
                entity.movementPoints = entity.maxMovementPoints;

                var decreasedEffects = entity.sideEffects;
                for (int i = 0; i < decreasedEffects.Count(); i++)
                {
                    var sideEffect = decreasedEffects[i];
                    switch (sideEffect.effect)
                    {
                        case SideEffect.Burning:
                            var damageBurn = Random.Range(1, 4);
                            entity.TakeDamage(damageBurn, (int)Element.Fire, false);
                            AppendLogs("takeDamage", new Dictionary<string, string>() {
                                {"entity", entity.Name},
                                {"damage", damageBurn.ToString()},
                                {"element", "Fire"},
                            });
                            break;
                        case SideEffect.Poisoned:
                            var damagePoison = Random.Range(1, 4);
                            entity.TakeDamage(damagePoison, (int)Element.Poison, false);
                            AppendLogs("takeDamage", new Dictionary<string, string>() {
                                {"entity", entity.Name},
                                {"damage", damagePoison.ToString()},
                                {"element", "Poison"},
                            });
                            break;
                        default:
                            break;
                    }
                    sideEffect.remainingTurns -= 1;
                    if (sideEffect.remainingTurns == 0)
                    {
                        decreasedEffects.RemoveAt(i);
                    }
                }
                entity.sideEffects = decreasedEffects;

                var decreasedOverTimeEffects = entity.overTimeEffects;
                for (int i = 0; i < decreasedOverTimeEffects.Count(); i++)
                {
                    var overTimeEffect = decreasedOverTimeEffects[i];
                    if (overTimeEffect.damage != null)
                    {
                        var damageDealt = Random.Range(overTimeEffect.damage.min, overTimeEffect.damage.max);
                        entity.TakeDamage(damageDealt, (int)overTimeEffect.damage.element, false);
                        AppendLogs("takeDamage", new Dictionary<string, string>() {
                        {"entity", entity.Name},
                        {"damage", damageDealt.ToString()},
                        {"element", overTimeEffect.damage.element.GetEnumDescription()},
                    });
                    }
                    if (overTimeEffect.remainingTurns != -1)
                    {
                        overTimeEffect.remainingTurns -= 1;
                        if (overTimeEffect.remainingTurns == 0)
                        {
                            decreasedOverTimeEffects.RemoveAt(i);
                        }
                    }
                }
                entity.overTimeEffects = decreasedOverTimeEffects;

                entity.skills.FindAll(s => s != null).ForEach(skill =>
                {
                    if (skill.Value.Item1.cooldown > 0)
                    {
                        skill.Value.Item1.cooldown -= 1;
                    }
                });
            });
        }
        else
        {
            ++turnEntityIndex;
            if (turnEntities[turnEntityIndex].IsInGroup("PlayableEntity"))
            {
                DisplayMovementRange(turnEntities[turnEntityIndex]);
            }
        }
    }

    private void MoveRendererToCell(Node2D movingTarget, Vector2I cell)
    {
        // var mesh = GetMeshRenderer(movingTarget);
        var worldPosition = BattleMap.CellCoordinatesToEntityPosition(cell);
        // mesh.sortingOrder = GetSortingOrder(cell);
        GD.Print($"Moving {movingTarget.Name} to cell {cell} at position {worldPosition}");
        movingTarget.Position = worldPosition;
    }

    public static int GetSortingOrder(Vector2I cell)
    {
        return (int)1e6 - cell.X - cell.Y;
    }

    private bool TriggerSurfaceEnter(Entity entity, Surface? surface)
    {
        switch (surface)
        {
            case Surface.Fire:
            case Surface.Poison:
                TriggerSurfaceEffectOverTime(entity, surface);
                break;
            case Surface.Ice:
                var slipped = Random.Range(1, 10) == 1;
                if (slipped)
                {
                    AppendLogs("slipped", new Dictionary<string, string> {
                        {"entity", entity.Name}
                    });
                    OnTurnEntityEnd();
                    // turn.turnStatus.Value = TurnStatus.Ended;
                    return true;
                }
                break;
        }
        return false;
    }

    private void TriggerSurfaceEffectOverTime(Entity entity, Surface? surface)
    {
        switch (surface)
        {
            case Surface.Fire:
                var damageBurn = Random.Range(1, 4);
                entity.TakeDamage(damageBurn, (int)Element.Fire, false);
                AppendLogs("takeDamage", new Dictionary<string, string>() {
                    {"entity", entity.Name},
                    {"damage", damageBurn.ToString()},
                    {"element", Element.Fire.GetEnumDescription()},
                });
                break;
            case Surface.Poison:
                var damagePoison = Random.Range(1, 4);
                entity.TakeDamage(damagePoison, (int)Element.Poison, false);
                AppendLogs("takeDamage", new Dictionary<string, string>() {
                    {"entity", entity.Name},
                    {"damage", damagePoison.ToString()},
                    {"element", Element.Poison.GetEnumDescription()},
                });
                break;
        }
    }

    // private MeshRenderer GetMeshRenderer(Node target)
    // {
    //     return target.Find("skeleton").gameObject.GetComponent<MeshRenderer>();
    // }

    public float tickRate = 6.0f;
    private float nextTickTime;


    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (command != null && Commander.ExecuteCommand(command))
        {
            command = null;
        }

        // if (Input.GetKeyDown(KeyCode.R) && Input.GetKey(KeyCode.LeftControl))
        // {
        //     GD.Print("Reloading scene...");
        //     string currentSceneName = SceneManager.GetActiveScene().name;
        //     SceneManager.LoadScene(currentSceneName);
        // }


        // if (Input.GetKeyDown(KeyCode.C) && Input.GetKey(KeyCode.LeftControl))
        // {
        //     GD.Print("Clearing memory...");
        //     System.GC.Collect();
        // }

        // if (Camera.main.GetComponent<SwipeCamera>().IsBlockedByUI)
        // {
        //     return;
        // }



        var pointerPosition = GetPointerDownPosition();

        //         if (turnEntities.Count() > 0 &&
        // turnEntities.FindAll(turnEntity => turnEntity.disposition == Disposition.Enemy).All(turnEntity => turnEntity.healthPoints == 0))
        //         {
        //             EndBattle();
        //             RefreshTurnOrder();
        //         }

        if (pointerPosition != null)
        {
            DoStuff(pointerPosition);
        }

        if (status == BattleStatus.Exploring)
        {

            // if (Time.time >= nextTickTime)
            // {
            // Tick();
            //     TickSurfaces();
            //     RefreshTurnOrder();
            //     nextTickTime += tickRate;
            // }
        }

        if (status != BattleStatus.Ongoing)
        {
            return;
        }

        // if (turnOrder[0].turnStatus.Value == TurnStatus.Ended)
        // if (turnEntityIndex > turnEntities.Count())
        // {
        //     turnCounter++;
        //     turnEntityIndex = 0;
        //     // turnOrder.FindAll(turnState => turnState.entity.healthPoints != 0).ForEach(turnState => turnState.turnStatus.Value = TurnStatus.Pending);
        //     // turnOrder[0].turnStatus.Value = TurnStatus.InProgress;
        //     RefreshTurnOrder();

        //     turnEntities.ForEach(entity =>
        //     {
        //         entity.abilityPoints = entity.maxAbilityPoints;
        //         entity.movementPoints = entity.maxMovementPoints;

        //         var decreasedEffects = entity.sideEffects;
        //         for (int i = 0; i < decreasedEffects.Count(); i++)
        //         {
        //             var sideEffect = decreasedEffects[i];
        //             switch (sideEffect.effect)
        //             {
        //                 case SideEffect.Burning:
        //                     var damageBurn = Random.Range(1, 4);
        //                     entity.TakeDamage(damageBurn, (int)Element.Fire, false);
        //                     AppendLogs("takeDamage", new Dictionary<string, string>() {
        //                         {"entity", entity.Name},
        //                         {"damage", damageBurn.ToString()},
        //                         {"element", "Fire"},
        //                     });
        //                     break;
        //                 case SideEffect.Poisoned:
        //                     var damagePoison = Random.Range(1, 4);
        //                     entity.TakeDamage(damagePoison, (int)Element.Poison, false);
        //                     AppendLogs("takeDamage", new Dictionary<string, string>() {
        //                         {"entity", entity.Name},
        //                         {"damage", damagePoison.ToString()},
        //                         {"element", "Poison"},
        //                     });
        //                     break;
        //                 default:
        //                     break;
        //             }
        //             sideEffect.remainingTurns -= 1;
        //             if (sideEffect.remainingTurns == 0)
        //             {
        //                 decreasedEffects.RemoveAt(i);
        //             }
        //         }
        //         entity.sideEffects = decreasedEffects;

        //         var decreasedOverTimeEffects = entity.overTimeEffects;
        //         for (int i = 0; i < decreasedOverTimeEffects.Count(); i++)
        //         {
        //             var overTimeEffect = decreasedOverTimeEffects[i];
        //             var damageDealt = Random.Range(overTimeEffect.damage.min, overTimeEffect.damage.max);
        //             entity.TakeDamage(damageDealt, (int)overTimeEffect.damage.element, false);
        //             AppendLogs("takeDamage", new Dictionary<string, string>() {
        //                 {"entity", entity.Name},
        //                 {"damage", damageDealt.ToString()},
        //                 {"element", overTimeEffect.damage.element.GetEnumDescription()},
        //             });

        //             overTimeEffect.remainingTurns -= 1;
        //             if (overTimeEffect.remainingTurns == 0)
        //             {
        //                 decreasedOverTimeEffects.RemoveAt(i);
        //             }
        //         }
        //         entity.overTimeEffects = decreasedOverTimeEffects;

        //         entity.skills.ForEach(skill =>
        //         {
        //             if (skill.Item1.cooldown > 0)
        //             {
        //                 skill.Item1.cooldown -= 1;
        //             }
        //         });
        //     });
        //     GD.Print("Going to next turn: " + turnCounter);
        //     RefreshTurnOrder();
        //     TickSurfaces();
        // }
        // else if (turnOrder.All(turnState => turnState.turnStatus.Value != TurnStatus.InProgress))
        // {
        //     var nextTurn = turnOrder.Find(turnState => turnState.turnStatus.Value == TurnStatus.Pending);
        //     if (nextTurn != null)
        //     {
        //         nextTurn.turnStatus.Value = TurnStatus.InProgress;
        //         var surface = ReadMapCell(nextTurn.cell.Value).surfaceStatus?.surface;
        //         if (surface != null)
        //         {
        //             TriggerSurfaceEffectOverTime(nextTurn, surface);
        //         }
        //         RefreshTurnOrder();

        //         if (nextTurn.entity.Name != "player")
        //         {
        //             // PerformAIActions(nextTurn);
        //         }
        //     }
        //     RefreshTurnOrder();
        // }
        else
        {
            var currentAgentTurn = turnEntities[turnEntityIndex];
            if (currentAgentTurn != null && !currentAgentTurn.IsInGroup("PlayableEntity"))
            {
                var result = PerformAIActions(currentAgentTurn);
                if (result)
                {
                    OnTurnEntityEnd();
                    RefreshTurnOrder();
                    // currentAgentTurn.turnStatus.Value = TurnStatus.Ended;
                }
            }
        }

        // if (AreUnlockingConditionsMet(out string next))
        // {
        //     Status = BattleStatus.Exploring;
        //     var nextBattle = gameData.progress["forest"].ToList().Find(item => item.Key == next).Value;
        //     if (nextBattle == null)
        //     {
        //         nextBattle = new MapItemBase()
        //         {
        //             name = next,
        //             status = ZoneStatus.Pending
        //         };
        //         gameData.progress["forest"][next] = nextBattle;
        //     }
        //     else
        //     {
        //         nextBattle.status = nextBattle.status == ZoneStatus.Locked ? ZoneStatus.Pending : ZoneStatus.Unlocked;
        //     }
        //     var inventory = gameData.resources.ToList();
        //     foreach (var item in lootBag)
        //     {
        //         var existingItem = inventory.FirstOrDefault(i => i.name == item.name);
        //         if (existingItem != null)
        //         {
        //             existingItem.quantity += item.quantity;
        //         }
        //         else
        //         {
        //             inventory.Add(new() { name = item.name, quantity = item.quantity });
        //         }
        //     }
        //     gameData.resources = inventory.ToArray();
        //     gameData.difficulty = difficulty;

        //     var currentBarterOffers = gameData.barterOffers.ToList().Select(x => x.item);
        //     gameData.barterOffers = currentMapItem.barterOffers.ToList().Where(x => !currentBarterOffers.Contains(x.item)).Select(x => new GameDataBarterOffer()
        //     {
        //         item = x.item,
        //         quantity = x.quantity,
        //         specificRequirements = x.specificRequirements,
        //         randomRequirements = x.randomRequirements,
        //         isAvailable = true
        //     }).ToArray();
        //     Storage.Write(gameData);
        // }
        // else if (player.GetComponent<Entity>().healthPoints == 0)
        // {
        //     GD.Print("player died!");
        //     Status = BattleStatus.Lost;
        // }



        if (pointerPosition != null)
        {
            FocusedInteractive = null;
            if (IsCastingSkill != SkillAction.Inactive)
            {

                // var point = Camera.main.ScreenToWorldPoint((Vector3)pointerPosition);
                var point = GetGlobalMousePosition();
                // point.z = 0f;

                var cellPos = (Vector2I)tilemap.LocalToMap(point);

                // var transformedDestination = TilemapToArrayPosition((Vector2I)cellPos);

                // var tile = GameObject.Find("scope:" + transformedDestination.X + "," + transformedDestination.Y);
                var tile = GetScopeTileFromWorld(point);
                GD.Print(tile);
                if (IsInBoundaries(scope.tileMapLayer, scope.tileMapLayer.LocalToMap(point)))
                {
                    var entity = playerEntity;

                    var tilemapTargetPosition = cellPos;

                    var pickedSkill = entity.skills[(int)IsCastingSkill - 1].Value.Item2;



                    if (validateCastingSkillTargetCell == tilemapTargetPosition)
                    {
                        validateCastingSkillTargetCell = null;
                        DestroyTargetCells();
                        DestroyValidationCells();
                        CastSkill(entity, pickedSkill, tilemapTargetPosition, () =>
                        {

                            if (status != BattleStatus.Exploring)
                                DisplayMovementRange(entity);
                        });
                        entity.LookAt(tilemapTargetPosition);

                        IsCastingSkill = SkillAction.Inactive;
                        RefreshTurnOrder();


                    }
                    else
                    {
                        DestroyValidationCells();
                        // if (validateCastingSkillTargetCell != null) {
                        DestroyTargetCells();
                        DrawTargetCells(BattleMap.EntityPositionToCellCoordinates(entity.Position), pickedSkill.range.min, pickedSkill.range.max, GetTargetableCells(pickedSkill), new Color(1f, 0.843f, 0f), pickedSkill.vision == 1);
                        // }
                        validateCastingSkillTargetCell = tilemapTargetPosition;
                        DrawValidationCells(tilemapTargetPosition, pickedSkill.targetRadius.min - 1, pickedSkill.targetRadius.max - 1, new[] {  new MapCell(){
                            kind = CellKind.Empty,
                            isOccupied = true,
                        }, new MapCell(){
                            kind = CellKind.Empty,
                            isOccupied = false,
                        }, new MapCell(){
                            kind = CellKind.Gap,
                            isOccupied = false,
                        }, new MapCell(){
                            kind = CellKind.Gap,
                            isOccupied = true,
                        }, new MapCell(){
                            kind = CellKind.Obstacle,
                            isOccupied = false,
                        } }, new Color(0, 1, 0));
                    }
                }
                Refresh = (int)delta;
            }
            else if (isMoving)
            {
                // var point = Camera.main.ScreenToWorldPoint((Vector3)pointerPosition);
                var point = GetGlobalMousePosition();
                // point.z = 0f;

                var tile = GetScopeTileFromWorld(point);

                if (tile != null)
                {
                    var entity = playerEntity;

                    DestroyTargetCells();
                    isMoving = false;

                    var tpath = FindShortestPath(
                        BattleMap.EntityPositionToCellCoordinates(entity.Position),
                        scope.tileMapLayer.LocalToMap(point),
                        new[] { new MapCell(){
                            kind = CellKind.Empty,
                            isOccupied = false,
                        },new MapCell(){
                            kind = CellKind.Contraption,
                            isOccupied = false,
                        } }
                    );

                    DoMovement(tpath, entity, () =>
                    {
                        RefreshTurnOrder();
                        Refresh = (int)delta;
                        if (status != BattleStatus.Exploring)
                            DisplayMovementRange(entity);
                    });
                }


            }
            else
            {
                validateCastingSkillTargetCell = null;
                pointerDown = true;

                var entity = playerEntity;
                DisplayMovementRange(entity);
                isMoving = true;
            }
        }

        if (pointerDown)
        {
            pointerDownTimer += (float)delta;
        }
        if (GetPointerUpPosition() != null)
        {
            ResetLongPress();
        }
        if (pointerDownTimer >= requiredHoldTime)
        {
            OnLongPress();
            ResetLongPress();
        }

        // if (Input.GetKeyDown(KeyCode.C))
        // {
        //     Vector3 mousePos = Input.mousePosition;

        //     var point = Camera.main.ScreenToWorldPoint(mousePos);
        //     point.z = 0f;

        //     Vector3I cellPos = tilemap.LocalToMap(point);
        //     var tilePosition = new Vector3I(cellPos.X, cellPos.Y, 0);
        // var tile = tilemap.GetTile<Tile>(tilePosition);
        // GD.Print("Hovered world to cell position: " + cellPos.X + ";" + cellPos.Y);
        // if (tile != null)
        // {
        //     GD.Print("Hovered tile position: " + tilePosition.X + ";" + tilePosition.Y);
        // }

        // turnOrder.Find(turn => turn.entity.Name != "player").entity.GetComponent<BaseAgent>().TakeDecision();
        // }
    }
    private void DisplayMovementRange(Entity entity)
    {
        DrawTargetCells(BattleMap.EntityPositionToCellCoordinates(entity.Position), 1, entity.movementPoints, new[] { new MapCell() {
            kind = CellKind.Empty,
            isOccupied = false
        }, new MapCell() {
            kind = CellKind.Contraption,
            isOccupied = false
        } });
    }


    private void HideMovementRange()
    {
        DestroyTargetCells();
    }

    private void DoStuff(Vector3? pointerPosition)
    {

        if (status == BattleStatus.Exploring)
        {

            FocusedInteractive = null;
            if (IsCastingSkill != SkillAction.Inactive)
            {

                // var point = Camera.main.ScreenToWorldPoint((Vector3)pointerPosition);
                var point = GetGlobalMousePosition();
                // point.z = 0f;

                var cellPos = tilemap.MapToLocal((Vector2I)point);

                var tile = GetScopeTileFromWorld(point);
                if (tile != null)
                {
                    var entity = playerEntity;

                    var tilemapTargetPosition = cellPos;

                    var pickedSkill = entity.skills[(int)IsCastingSkill - 1].Value.Item2;


                    if (validateCastingSkillTargetCell == tilemapTargetPosition)
                    {
                        validateCastingSkillTargetCell = null;
                        DestroyTargetCells();
                        DestroyValidationCells();

                        SkillCooldown = ((int)IsCastingSkill - 1, pickedSkill.restrictions.cooldown);
                        var targets = CastSkill(entity, pickedSkill, (Vector2I)tilemapTargetPosition, () => { });
                        entity.LookAt(tilemapTargetPosition);

                        if (targets.Count > 0)
                        {
                            var involvedEntities = targets.Select(x =>
                            {
                                var entity = (Entity)x;
                                entity.disposition = Disposition.Enemy;
                                return entity;
                            }).ToList();
                            involvedEntities.Add(playerEntity);
                            PrepareBattle(involvedEntities);
                            StartBattle();
                        }
                        IsCastingSkill = SkillAction.Inactive;
                        RefreshTurnOrder();
                    }
                    else
                    {
                        DestroyValidationCells();
                        // if (validateCastingSkillTargetCell != null) {
                        DestroyTargetCells();
                        DrawTargetCells(BattleMap.EntityPositionToCellCoordinates(entity.Position), pickedSkill.range.min, pickedSkill.range.max, GetTargetableCells(pickedSkill), new Color(1f, 0.843f, 0f), pickedSkill.vision == 1);
                        // }
                        validateCastingSkillTargetCell = (Vector2I)tilemapTargetPosition;
                        DrawValidationCells(validationScope.tileMapLayer.LocalToMap(point), pickedSkill.targetRadius.min - 1, pickedSkill.targetRadius.max - 1, new[] {  new MapCell(){
                            kind = CellKind.Empty,
                            isOccupied = true,
                        }, new MapCell(){
                            kind = CellKind.Empty,
                            isOccupied = false,
                        }, new MapCell(){
                            kind = CellKind.Gap,
                            isOccupied = false,
                        }, new MapCell(){
                            kind = CellKind.Gap,
                            isOccupied = true,
                        }, new MapCell(){
                            kind = CellKind.Obstacle,
                            isOccupied = false,
                        }, new MapCell(){
                            kind = CellKind.Contraption,
                            isOccupied = false,
                        }, new MapCell(){
                            kind = CellKind.Contraption,
                            isOccupied = true,
                        } }, new Color(0, 1, 0));
                    }
                }
            }
            else if (!isMoving)
            {
                // var point = Camera.main.ScreenToWorldPoint((Vector3)pointerPosition);
                var point = GetGlobalMousePosition();
                // point.z = 0f;

                var tile = GetTileFromWorld(point);

                if (tile != null)
                {
                    var tilePosition = tilemap.LocalToMap(point);
                    DestroyTargetCells();
                    var entity = playerEntity;
                    DrawTargetCells((Vector2I)tilePosition, 0, 0, new[] { new MapCell() {
                        kind = CellKind.Empty,
                        isOccupied = false
                    }, new MapCell() {
                        kind = CellKind.Contraption,
                        isOccupied = false
                    } });
                    isMoving = true;
                }
            }
            else
            {
                validateCastingSkillTargetCell = null;
                pointerDown = true;

                // var point = Camera.main.ScreenToWorldPoint((Vector3)pointerPosition);
                var point = GetGlobalMousePosition();
                // point.z = 0f;

                var tile = GetScopeTileFromWorld(point);

                if (tile != null)
                {
                    DestroyTargetCells();
                    var entity = playerEntity;

                    isMoving = false;

                    var tpath = FindShortestPath(
                        BattleMap.EntityPositionToCellCoordinates(entity.Position),
                        scope.tileMapLayer.LocalToMap(point),
                        new[] { new MapCell(){
                            kind = CellKind.Empty,
                            isOccupied = false,
                        },new MapCell(){
                            kind = CellKind.Contraption,
                            isOccupied = false,
                        } }
                    );

                    DoMovement(tpath, entity, RefreshTurnOrder);
                }
                isMoving = false;
            }


            if (pointerDown)
            {
                // pointerDownTimer += Time.deltaTime;
            }
            if (GetPointerUpPosition() != null)
            {
                ResetLongPress();
            }
            if (pointerDownTimer >= requiredHoldTime)
            {
                OnLongPress();
                ResetLongPress();
            }



        }
        return;
    }

    private TileData GetScopeTileFromWorld(Vector2 position)
    {
        var tilePosition = scope.tileMapLayer.MapToLocal((Vector2I)position);
        return scope.tileMapLayer.GetCellTileData((Vector2I)tilePosition);
    }
    private TileData GetTileFromWorld(Vector2 position)
    {
        var tilePosition = tilemap.MapToLocal((Vector2I)position);
        return tilemap.GetCellTileData((Vector2I)tilePosition);
    }


    public bool AreUnlockingConditionsMet(out string next)
    {
        next = "";
        var tempNext = "";
        var items = currentMapItem.unlocks.Select(kv => new Tuple<string, string>(kv.Key, kv.Value));
        var result = items.Any(row =>
        {
            if (row.Item2 == "victory" && enemies.All(turnState => turnState.entity.healthPoints == 0))
            {
                GD.Print("enemies died!");
                tempNext = row.Item1;
                return true;
            }
            else if (row.Item2.StartsWith("#") && HasBeenActivated(row.Item2.Replace("#", "")))
            {
                GD.Print("enabled contraption");
                tempNext = row.Item1;
                return true;
            }

            return false;
        });
        next = tempNext;
        return result;
    }

    public bool HasBeenActivated(string name)
    {

        // Find game objects whose names start with a specific value
        var go = GetTree().GetNodesInGroup("contraption").Where(obj => obj.Name.ToString().StartsWith(name)).ToArray()[0];
        // var go = FindObjectsOfType<GameObject>().Where(obj => obj.name.StartsWith(name)).ToArray()[0];
        // var go = GameObject.Find(name);
        if (go == null)
        {
            GD.Print("Missing object: " + name);
            return false;
        }
        return ((Contraption)go).isActive;
    }

    public MapCell[] GetTargetableCells(SkillBlueprint skill)
    {
        if (skill.movement?.target == "caster")
        {
            return new[] { new MapCell(){
                kind = CellKind.Empty,
                isOccupied = false,
            }, new MapCell(){
                kind = CellKind.Gap,
                isOccupied = false,
            } };
        }
        return skillCommonCells;
    }

    // public IEnumerator ExecuteSkill(SkillBlueprint skill, Vector2I targetCell, Action onFinish)
    // {
    //     DrawTargetCells(targetCell, skill.targetRadius.min - 1, skill.targetRadius.max - 1, skillCommonCells, new Color(1f, 0.843f, 0f));
    //     yield return new WaitForSeconds(1f);
    //     DestroyTargetCells();
    //     DestroyValidationCells();
    //     onFinish();
    // }

    public List<Interactive> CastSkill(Entity entity, SkillBlueprint skill, Vector2I targetCell, Action onFinish)
    {
        if (status != BattleStatus.Exploring)
        {
            entity.abilityPoints -= skill.cost.ap;
            entity.movementPoints -= skill.cost.mp;
        }
        var modifier = 1f;
        var isCritical = false;
        if (skill.stability != null)
        {
            var casterStability = entity.GetRateFromEffect(Characteristic.Stability);
            int getStability(int stability) => (int)MathF.Ceiling(stability - stability * casterStability / 100);
            var roll = Random.Range(1, 100);
            if (roll <= getStability(skill.stability[0]))
            {
                AppendLogs("criticalFailure", new Dictionary<string, string>() {
                            {"entity", entity.Name},
            });
                onFinish();
                return new();
            }
            else if (roll <= getStability(skill.stability[1]))
            {
                AppendLogs("failure", new Dictionary<string, string>() {
                            {"entity", entity.Name},
            });
                onFinish();
                return new();
            }
            else if (roll <= getStability(skill.stability[2]))
            {
                // success
            }
            else
            {
                // critical success
                modifier *= 1.5f;
                isCritical = true;
                // check skills that increase this modifier
            }
        }
        AppendLogs("cast", new Dictionary<string, string>() {
            {"entity", entity.Name},
            {"skill", "skill." + skill.name},
        });

        var turnSkill = entity.skills.Find(item => item.Value.Item2.name == skill.name);
        turnSkill.Value.Item1.maxCooldown = skill.restrictions.cooldown;
        turnSkill.Value.Item1.cooldown = skill.restrictions.cooldown;
        turnSkill.Value.Item1.lastCastTime = (int)Time.GetUnixTimeFromSystem();

        // StartCoroutine(ExecuteSkill(skill, targetCell, onFinish));
        var targetCells = FindReachableCells(targetCell, skill.targetRadius.min - 1, skill.targetRadius.max - 1, new[] {  new MapCell(){
            kind = CellKind.Empty,
            isOccupied = true,
        }, new MapCell(){
            kind = CellKind.Empty,
            isOccupied = false,
        }, new MapCell(){
            kind = CellKind.Gap,
            isOccupied = false,
        }, new MapCell(){
            kind = CellKind.Gap,
            isOccupied = true,
        }, new MapCell(){
            kind = CellKind.Obstacle,
            isOccupied = false,
        }, new MapCell(){
            kind = CellKind.Contraption,
            isOccupied = true,
        } }, false);
        var targets = targetCells
            .Select(cell => GetInteractiveByPosition(cell))
            .Where(x => x != null)
            .ToList();

        if (IsSpecialSkill(skill.name))
        {
            switch (skill.name.Split('-')[0])
            {
                case "interact":
                    CastInteract(targetCell);
                    break;
                case "detect":
                    CastDetect(targets, int.Parse(skill.name.Split('-')[1]));
                    break;
                default:
                    break;
            }
            return new();
        }

        var effects = entity.Cast(skill, targets.Select(target => (Entity)target).ToList());

        effects.damages.Select((damage, index) => (damage, index)).ToList().ForEach((row) =>
        {
            var targetEntity = (Entity)targets[row.index];
            var rate = targetEntity.GetRateFromEffect(Characteristic.Evasion);
            var hit = Random.Range(1, 100) > rate;
            if (hit || isCritical)
            {
                targetEntity.TakeDamage(row.damage.amount, (int)row.damage.element, isCritical);
                AppendLogs(isCritical ? "takeCriticalDamage" : "takeDamage", new Dictionary<string, string>() {
                            {"entity", targetEntity.Name},
                            {"element", row.damage.element.GetEnumDescription()},
                            {"damage", row.damage.amount.ToString()},
            });
            }
            else
            {
                AppendLogs("evaded", new Dictionary<string, string>() {
                            {"entity", targetEntity.Name},
            });
            }
        });


        var tilemapTargetPosition = targetCell;
        var targetGameObject = GetInteractiveByPosition(tilemapTargetPosition);

        var tilemapPlayerPosition = BattleMap.EntityPositionToCellCoordinates(entity.Position);
        effects.movementEffects.ForEach((effect) =>
        {
            if (effect.reference == "caster")
            {
                var entityReference = playerEntity;
                var referencePosition = BattleMap.EntityPositionToCellCoordinates(entityReference.Position);
                var entityTarget = effect.target;
                var targetMoveResistance = entityTarget.GetRateFromEffect(Characteristic.MoveResistance);
                var roll = Random.Range(1, 100);
                if (roll <= targetMoveResistance)
                {
                    return;
                }
                var casterEntityPosition = BattleMap.EntityPositionToCellCoordinates(entityTarget.Position);
                var position = GridHelper.GetRelativePosition(referencePosition, casterEntityPosition);

                var newCoordinates = position switch
                {
                    Direction.Left => new Vector2I(casterEntityPosition.X - effect.cells, casterEntityPosition.Y),
                    Direction.Top => new Vector2I(casterEntityPosition.X, casterEntityPosition.Y + effect.cells),
                    Direction.Right => new Vector2I(casterEntityPosition.X + effect.cells, casterEntityPosition.Y),
                    Direction.Bottom => new Vector2I(casterEntityPosition.X, casterEntityPosition.Y - effect.cells),
                };

                var targetCell = ReadMapCell(newCoordinates);
                if (targetCell.kind == CellKind.Obstacle || targetCell.isOccupied || targetCell.kind == CellKind.Wall)
                {
                    return;
                }
                OnChangeCell(casterEntityPosition, newCoordinates);
                // turnOrderTarget.cell.Value = newCoordinates;

                MoveRendererToCell(effect.target, newCoordinates);

                if (targetCell.kind == CellKind.Gap)
                {
                    AppendLogs("feltHole", new Dictionary<string, string>() {
                        {"entity", entityTarget.Name},
                    });
                    entityTarget.Kill();
                }

                var surface = targetCell.surfaceStatus?.surface;
                if (surface != null)
                {
                    TriggerSurfaceEnter(entityTarget, surface);
                }
            }
            else
            {
                var entity = effect.target;
                var entityPosition = BattleMap.EntityPositionToCellCoordinates(entity.Position);
                var position = GridHelper.GetRelativePosition(tilemapTargetPosition, entityPosition);

                var newCoordinates = position switch
                {
                    Direction.Left => new Vector2I(tilemapTargetPosition.X - effect.cells, tilemapTargetPosition.Y),
                    Direction.Top => new Vector2I(tilemapTargetPosition.X, tilemapTargetPosition.Y + effect.cells),
                    Direction.Right => new Vector2I(tilemapTargetPosition.X + effect.cells, tilemapTargetPosition.Y),
                    Direction.Bottom => new Vector2I(tilemapTargetPosition.X, tilemapTargetPosition.Y - effect.cells),
                };

                var targetCell = ReadMapCell(newCoordinates.X, newCoordinates.Y);
                if (targetCell.kind == CellKind.Obstacle || targetCell.isOccupied || targetCell.kind == CellKind.Wall)
                {
                    return;
                }
                OnChangeCell(entityPosition, newCoordinates);
                // turnOrder.cell.Value = newCoordinates;
                MoveRendererToCell(effect.target, newCoordinates);
                if (targetCell.kind == CellKind.Gap)
                {
                    AppendLogs("feltHole", new Dictionary<string, string>() {
                        {"entity", entity.Name},
                    });
                    entity.Kill();
                }

                var surface = targetCell.surfaceStatus?.surface;
                if (surface != null)
                {
                    TriggerSurfaceEnter(entity, surface);
                }
            }
        });

        effects.overTimeEffects.ForEach((effect) =>
        {
            foreach (var target in targets)
            {
                var targetEntity = (Entity)target;
                if (targetEntity.overTimeEffects.Select(x => x.name).Contains(effect.name))
                {
                    var altered = targetEntity.overTimeEffects;
                    var item = altered.Find(x => x.name == effect.name);
                    item.remainingTurns = effect.duration;
                    targetEntity.overTimeEffects = altered;
                    return;
                }

                targetEntity.overTimeEffects = targetEntity.overTimeEffects.Concat(new[] { new TurnOverTimeEffect(){
                    damage = effect.damage,
                    caster = effect.caster,
                    remainingTurns = effect.duration,
                    name = effect.name
                } }).ToList();
            }
        });

        effects.sideEffects.ForEach((effect) =>
        {
            foreach (var target in targets)
            {
                var targetEntity = (Entity)target;
                var value = Random.Range(0, 100);
                if (effect.rate > value)
                {
                    if (targetEntity.sideEffects.Select(x => x.effect).Contains(effect.name))
                    {
                        var altered = targetEntity.sideEffects;
                        var item = altered.Find(x => x.effect == effect.name);
                        item.remainingTurns = 3;
                        targetEntity.sideEffects = altered;
                        return;
                    }

                    targetEntity.sideEffects = targetEntity.sideEffects.Concat(new[] { new TurnSideEffect(){
                        effect = effect.name,
                        remainingTurns = 3
                    } }).ToList();
                }
            }
        });

        if (effects.mainSurfaceEffect != null)
        {
            ApplySurfaceToCells(targetCells, effects.mainSurfaceEffect.surface);
        }
        else
        {
            effects.surfaceEffects.ForEach((effect) =>
            {
                targetCells.ForEach((cell) =>
                {
                    var region = GetSurfaceRegionCells(cell, effect.surface);
                    var currentSurface = ReadMapCell(cell.X, cell.Y).surfaceStatus?.surface;
                    if (currentSurface == null)
                    {
                        return;
                    }
                    var surface = GetSurfaceResult(currentSurface, effect.surface);


                    region.ForEach(c =>
                    {
                        SetMapCellSurface(c.X, c.Y, new SurfaceStatus() { surface = surface, turns = GetSurfaceTurns(surface) });
                        DeleteSurfaceTile(c);
                        CreateSurface(c, surface);
                    });
                });
            });
        }

        return targets;
    }

    public void CastInteract(Vector2I targetCell)
    {
        // var mapCell = ReadMapCell(targetCell);
        // if (mapCell.metadata.ContainsKey("contraption"))
        // {
        //     var contraption = GameObject.Find(mapCell.metadata["contraption"]).GetComponent<Contraption>();
        //     var commands = contraption.Trigger();
        //     foreach (var command in commands)
        //     {
        //         Trigger(command.Item1, command.data);
        //     }
        // }
        // else if (mapCell.metadata.ContainsKey("entity"))
        // {
        //     var data = currentMapItem.dialogues.ContainsKey(mapCell.metadata["entity"]) ? ObjectToDictionary(currentMapItem.dialogues[mapCell.metadata["entity"]]) : new Dictionary<string, object>() {
        //         {"text", Zone.Instance.CurrentBattle.Item1 + (Zone.Instance.CurrentBattle.Item2 + 1) + "." + mapCell.metadata["entity"]},
        //         {"rightSpeaker", new Dictionary<string, object>() {
        //             {"name", mapCell.metadata["entity"]},
        //             {"isHighlighted", true},
        //         }},
        //     };
        //     Trigger(TriggerKind.Dialogue, data);
        // }
    }


    public void CastDetect(List<Interactive> targets, int skillRank)
    {
        var detectionValue = Random.Range(1, 20) + skillRank;
        foreach (var target in targets)
        {
            // var interactive = target.GetComponent<Interactive>();
            if (target.isHidden && detectionValue >= target.detectionLevel)
            {
                target.isHidden = false;
            }
        }
    }

    private bool IsSpecialSkill(string skillName)
    {
        return new[] { "interact", "detect" }.Contains(skillName.Split("-")[0]);
    }

    private int GetSurfaceTurns(Surface surface)
    {
        if (surface == Surface.Fire || surface == Surface.Ice || surface == Surface.Smoke || surface == Surface.Fog)
        {
            return 3;
        }

        return -1;
    }

    private bool RehydrateSkills(int currentTime = 0)
    {
        var refreshed = false;

        // GameObject.FindGameObjectsWithTag("PlayableEntity").ToList().ForEach(gameObject =>
        // {
        //     var entity = gameObject.GetComponent<Entity>();
        //     entity.skills.ForEach(skill =>
        //     {
        //         if (skill == null)
        //         {
        //             return;
        //         }
        //         if (skill.Value.Item1.cooldown > 0)
        //         {
        //             if (currentTime == 0)
        //             {
        //                 skill.Value.Item1.cooldown -= 1;
        //             }
        //             else
        //             {
        //                 skill.Value.Item1.cooldown = Math.Max(skill.Value.Item1.maxCooldown - (currentTime - skill.Value.Item1.lastCastTime), 0);
        //             }
        //             refreshed = true;
        //         }
        //     });
        // });

        return refreshed;
    }

    private void Tick()
    {
        turnEntities.ForEach(entity =>
        {
            entity.abilityPoints = entity.maxAbilityPoints;
            entity.movementPoints = entity.maxMovementPoints;

            var decreasedEffects = entity.sideEffects;
            for (int i = 0; i < decreasedEffects.Count(); i++)
            {
                var sideEffect = decreasedEffects[i];
                switch (sideEffect.effect)
                {
                    case SideEffect.Burning:
                        var damageBurn = Random.Range(1, 4);
                        entity.TakeDamage(damageBurn, (int)Element.Fire, false);
                        AppendLogs("takeDamage", new Dictionary<string, string>() {
                            {"entity", entity.Name},
                            {"damage", damageBurn.ToString()},
                            {"element", "Fire"},
                        });
                        break;
                    case SideEffect.Poisoned:
                        var damagePoison = Random.Range(1, 4);
                        entity.TakeDamage(damagePoison, (int)Element.Poison, false);
                        AppendLogs("takeDamage", new Dictionary<string, string>() {
                            {"entity", entity.Name},
                            {"damage", damagePoison.ToString()},
                            {"element", "Poison"},
                        });
                        break;
                    default:
                        break;
                }
                sideEffect.remainingTurns -= 1;
                if (sideEffect.remainingTurns == 0)
                {
                    decreasedEffects.RemoveAt(i);
                }
            }
            entity.sideEffects = decreasedEffects;

            var decreasedOverTimeEffects = entity.overTimeEffects;
            for (int i = 0; i < decreasedOverTimeEffects.Count(); i++)
            {
                var overTimeEffect = decreasedOverTimeEffects[i];
                var damageDealt = Random.Range(overTimeEffect.damage.min, overTimeEffect.damage.max);
                entity.TakeDamage(damageDealt, (int)overTimeEffect.damage.element, false);
                AppendLogs("takeDamage", new Dictionary<string, string>() {
                    {"entity", entity.Name},
                    {"damage", damageDealt.ToString()},
                    {"element", overTimeEffect.damage.element.GetEnumDescription()},
                });

                overTimeEffect.remainingTurns -= 1;
                if (overTimeEffect.remainingTurns == 0)
                {
                    decreasedOverTimeEffects.RemoveAt(i);
                }
            }
            entity.overTimeEffects = decreasedOverTimeEffects;

            entity.skills.ForEach(skill =>
            {
                if (skill.Value.Item1.cooldown > 0)
                {
                    skill.Value.Item1.cooldown -= 1;
                }
            });
        });
        GD.Print("Going to next turn: " + turnCounter);
        RefreshTurnOrder();
    }

    private void TickSurfaces()
    {
        try
        {
            // GameObject.FindGameObjectsWithTag("Surface").ToList().ForEach(go =>
            // {
            //     var coordinates = go.name.Split("-").Last().Split(",");
            //     var tilemapPosition = new Vector2I(int.Parse(coordinates[0]), int.Parse(coordinates[1]));
            //     var cellPosition = tilemapPosition;
            //     var cellData = ReadMapCell(cellPosition);
            //     if (cellData.surfaceStatus.turns != -1)
            //     {
            //         cellData.surfaceStatus.turns -= 1;
            //         if (cellData.surfaceStatus.turns == 0)
            //         {
            //             UpdateMapCell(cellPosition.X, cellPosition.Y, new MapCell()
            //             {
            //                 surfaceStatus = null,
            //                 kind = cellData.kind,
            //                 isOccupied = cellData.isOccupied,
            //                 metadata = cellData.metadata
            //             });
            //             Destroy(go);
            //         }
            //         if (cellData.surfaceStatus.turns == 0 && cellData.surfaceStatus.surface == Surface.Ice)
            //         {
            //             UpdateMapCell(cellPosition.X, cellPosition.Y, new MapCell()
            //             {
            //                 surfaceStatus = new SurfaceStatus() { surface = Surface.Water, turns = GetSurfaceTurns(Surface.Water) },
            //                 kind = cellData.kind,
            //                 isOccupied = cellData.isOccupied,
            //                 metadata = cellData.metadata
            //             });
            //             CreateSurface(tilemapPosition, Surface.Water);
            //         }
            //     }
            // });
        }
        catch (Exception e)
        {

        }
    }

    private void ApplySurfaceToCells(List<Vector2I> cells, Surface surface)
    {
        cells.ForEach(cell =>
        {
            var cellData = ReadMapCell(cell.X, cell.Y);

            if (cellData.kind != CellKind.Empty)
            {
                return;
            }

            var newSurface = GetSurfaceResult(cellData.surfaceStatus?.surface, surface);
            DeleteSurfaceTile(cell);
            CreateSurface(cell, newSurface);
            SetMapCellSurface(cell.X, cell.Y, new SurfaceStatus() { surface = newSurface, turns = GetSurfaceTurns(newSurface) });
        });
    }

    private void CreateSurface(Vector2I position, Surface surface)
    {
        // var prefab = Resources.Load<GameObject>("Battle/Prefabs/Surfaces/" + surface.GetEnumDescription());
        // GameObject go;
        // var scale = 2f;
        // if (prefab == null)
        // {
        //     go = new GameObject("surface-" + position.X + "," + position.Y);
        //     go.transform.SetParent(GameObject.Find("Surfaces").transform);
        //     var sprite = Resources.Load<Sprite>("Battle/Images/Environment/Textures/Surfaces/" + surface.GetEnumDescription());
        //     var renderer = go.AddComponent<SpriteRenderer>();

        //     if (sprite == null)
        //     {
        //         sprite = Resources.Load<Sprite>("Battle/Images/Environment/Textures/tile-iso");
        //         scale = 1.2f;
        //     }
        //     renderer.sprite = sprite;

        //     go.transform.localScale = new Vector3(scale, scale);
        //     var spritePosition = tilemap.LocalToMap(new Vector3I(position.X, position.Y, 0));
        //     spritePosition.X += 0.05f;
        //     spritePosition.Y += (int)sprite.bounds.size.Y * scale / 2 - 0.5f - 0.025f;
        //     go.transform.position = spritePosition;

        //     renderer.sortingOrder = (int)1e6 - position.X - position.Y - 2;
        // }
        // else
        // {
        //     go = Instantiate(prefab);
        //     go.name = "surface-" + position.X + "," + position.Y;
        //     var renderer = go.GetComponent<SpriteRenderer>();
        //     var spritePosition = tilemap.LocalToMap(new Vector3I(position.X, position.Y, 0));
        //     spritePosition.X += 0.05f;
        //     spritePosition.Y += (int)renderer.sprite.bounds.size.Y * scale / 2 - 0.5f - 0.025f;
        //     go.transform.position = spritePosition;
        //     renderer.sortingOrder = (int)1e6 - position.X - position.Y - 2;
        // }

        // go.transform.SetParent(GameObject.Find("Surfaces").transform);


    }

    private void DeleteSurfaceTile(Vector2I position)
    {
        // Destroy(GameObject.Find("surface-" + position.X + "," + position.Y));
    }

    // private void SetTileColor(Tile tile, Surface surface)
    // {
    //     switch (surface)
    //     {
    //         case Surface.Water:
    //             tile.color = Color.blue;
    //             break;
    //         case Surface.Ice:
    //             tile.color = Color.cyan;
    //             break;
    //         case Surface.Fire:
    //             tile.color = Color.red;
    //             break;
    //         case Surface.Smoke:
    //             tile.color = Color.grey;
    //             break;
    //     }
    // }

    private bool pointerDown = false;
    private float pointerDownTimer;
    private readonly float requiredHoldTime = 0.5f;
    public Dictionary<string, DisplayableItem> items;
    private List<LootResult> lootBag = new();

    public List<LootResult> GetLoot()
    {
        return lootBag;
    }

    void ResetLongPress()
    {
        pointerDown = false;
        pointerDownTimer = 0;
    }
    void OnLongPress()
    {
        // Vector3 mousePos = Input.mousePosition;
        var point = GetGlobalMousePosition();
        // var point = Camera.main.ScreenToWorldPoint(mousePos);
        // point.z = 0f;

        var cellPos = tilemap.LocalToMap(point);
        GD.Print("Clicked: " + cellPos);
        var isInBoundaries = IsInBoundaries(tilemap, cellPos);

        if (isInBoundaries)
        {
            GD.Print("Long-pressed tile: " + cellPos.X + ";" + cellPos.Y);
            try
            {
                // GD.Print("Cell data: " + JsonUtility.ToJson(ReadMapCell((Vector2I)cellPos)));
                var targetGameObject = GetInteractiveByPosition(cellPos);
                if (targetGameObject != null)
                {
                    GD.Print(targetGameObject.Name);
                    FocusedInteractive = targetGameObject.Name;
                }
            }
            catch (KeyNotFoundException)
            {
                GD.Print("Cell not defined!");
            }

        }
    }

    private bool IsInBoundaries(TileMapLayer tilemap, Vector2I target)
    {
        foreach (var position in tilemap.GetUsedCells())
        {
            if (position.X == target.X && position.Y == target.Y)
            {
                return true;
            }
        }
        return false;
    }


    public void RefreshTurnOrder()
    {
        turnEntities.ForEach(entity =>
        {

            if (entity.healthPoints == 0)
            {
                var location = BattleMap.EntityPositionToCellCoordinates(entity.Position);
                if (ReadMapCell(location).kind == CellKind.Gap)
                {
                    entity.SetProcess(false);
                }
                else
                {
                    FadeOut(entity, () =>
                    {
                        // turnState.turnStatus.Value = TurnStatus.Ended;
                        entity.SetProcess(false);
                    });
                    // var monster = monsters.ToList().Find(item => item.Key == entity.Name.Split("-")[0]).Value;
                    // monster.loot.ForEach((loot) =>
                    // {
                    //     var quantity = Random.Range(loot.quantity.min, loot.quantity.max + 1);


                    //     var rarity = items[loot.name].rarity;
                    //     var lootThreshold = rarity switch
                    //     {
                    //         Rarity.Banal => 30,
                    //         Rarity.Common => 40,
                    //         Rarity.Uncommon => 50,
                    //         Rarity.Rare => 80,
                    //         Rarity.Mythical => 90,
                    //         Rarity.Unique => 99,
                    //     };
                    //     var lootRoll = Random.Range(0, 100);
                    //     if (quantity > 0 && lootRoll > lootThreshold)
                    //     {
                    //         lootBag.Add(new()
                    //         {
                    //             name = loot.name,
                    //             quantity = quantity,
                    //         });
                    //     }
                    // });
                }
                var cellData = ReadMapCell(location);
                UpdateMapCell(location.X, location.Y, new MapCell()
                {
                    isOccupied = false,
                    kind = cellData.kind,
                    surfaceStatus = cellData.surfaceStatus,
                    metadata = cellData.metadata,
                });
            }
        });

        TurnOrder = turnEntities.Select(entity => new TurnStateOut()
        {
            turnStatus = turnEntities[turnEntityIndex] == entity ? TurnStatus.InProgress : (turnEntities.FindIndex(turn => entity == turn) > turnEntityIndex ? TurnStatus.Pending : TurnStatus.Ended),
            maxHealthPoints = entity.maxHealthPoints,
            healthPoints = entity.healthPoints,
            name = entity.Name,
            sideEffects = entity.sideEffects,
        }).ToList();
    }

    public void FadeOut(Node2D transform, Action onFinish)
    {
        StartCoroutine(DoFadeOut(transform, onFinish));
    }

    private IEnumerator DoFadeOut(Node2D transform, Action onFinish)
    {
        var fadeDuration = 1f;
        double startTime = Time.GetUnixTimeFromSystem();
        var color = transform.Modulate;

        // if (transform != null)
        // {
        // var skeleton = ((SpineSprite)transform.GetNode("skeleton")).GetSkeleton();
        // var skeleton = transform.GetNode("skeleton").GetComponent<SkeletonAnimation>().Skeleton;

        while (Time.GetUnixTimeFromSystem() - startTime < fadeDuration)
        {
            float alpha = Mathf.Lerp(1f, 0f, (float)(Time.GetUnixTimeFromSystem() - startTime) / fadeDuration);

            color.A = alpha;
            transform.Modulate = color;

            yield return null;
            // elapsedTime += Time.Get();
        }

        // skeleton.A = 0f;
        color.A = 0;
        transform.Modulate = color;
        // }
        // else
        // {
        //     var target = transform.GetComponent<SpriteRenderer>();
        //     while (Time.GetUnixTimeFromSystem() - startTime < fadeDuration)
        //     {
        //         float alpha = Mathf.Lerp(1f, 0f, (float)(Time.GetUnixTimeFromSystem() - startTime) / fadeDuration);
        //         target.color = new Color(target.color.r, target.color.g, target.color.b, alpha);

        //         yield return null;
        //     }
        //     target.color = new Color(target.color.r, target.color.g, target.color.b, 0);
        // }

        onFinish();
    }

    private bool PerformAIActions(Entity entityGameObject)
    {
        // var agent = (BaseAgent)entityGameObject;
        // return agent.TakeDecision();
        return true;
    }

    // private Vector3 MapPositionToWorldPosition(Vector3I mapPosition)
    // {
    //     var worldPosition = tilemap.LocalToMap(mapPosition);
    //     return new Vector3(worldPosition.X, worldPosition.Y, 0);
    // }



    public List<Vector2I> FindReachableCells(Vector2I start, int minimum, int range, MapCell[] includableCells, bool requiresVision = true)
    {
        var grid = currentMap;
        int rows = grid.Keys.Select(k => GridHelper.StringToVector2I(k).X).Max() + 1;
        int cols = grid.Keys.Select(k => GridHelper.StringToVector2I(k).Y).Max() + 1;

        int minRow = start.Y - range;
        int maxRow = start.Y + range;
        int minCol = start.X - range;
        int maxCol = start.X + range;

        // Array to track visited cells
        bool[,] visited = new bool[maxRow - minRow + 1, maxCol - minCol + 1];
        visited[start.Y - minRow, start.X - minCol] = true;

        // Queue for BFS
        Queue<Vector2I> queue = new();
        queue.Enqueue(start);

        // Dictionary to store parent cells
        Dictionary<Vector2I, Vector2I> parent = new();

        // Directions for moving in the grid (up, down, left, right)
        int[] dx = { -1, 1, 0, 0 };
        int[] dy = { 0, 0, -1, 1 };

        int depth = 0;
        int nodesInCurrentLayer = 1;
        int nodesInNextLayer = 0;

        List<Vector2I> reachableCells = new();

        while (queue.Count > 0 && depth <= range)
        {
            Vector2I current = queue.Dequeue();
            nodesInCurrentLayer--;

            var isMatchingCurrent = Array.Find(includableCells, (cell) => cell.kind == grid.GetValueOrDefault(Vector2ToString(current))?.kind && cell.isOccupied == grid.GetValueOrDefault(Vector2ToString(current))?.isOccupied) != null;
            if (depth >= minimum && isMatchingCurrent)
            {
                reachableCells.Add(current);
            }

            // Check all adjacent cells
            for (int i = 0; i < 4; i++)
            {
                int newX = current.X + dx[i];
                int newY = current.Y + dy[i];

                // Check if the new cell is within bounds and not an obstacle
                if (newX >= minCol && newX <= maxCol && newY >= minRow && newY <= maxRow && !visited[newY - minRow, newX - minCol] && Array.Find(includableCells, (cell) => cell.kind == grid.GetValueOrDefault(CoordsToString(newX, newY))?.kind && cell.isOccupied == grid.GetValueOrDefault(CoordsToString(newX, newY))?.isOccupied) != null)
                {
                    Vector2I next = new(newX, newY);

                    visited[newY - minRow, newX - minCol] = true;
                    queue.Enqueue(next);
                    parent[next] = current;
                    nodesInNextLayer++;
                }
            }

            if (nodesInCurrentLayer == 0)
            {
                nodesInCurrentLayer = nodesInNextLayer;
                nodesInNextLayer = 0;
                depth++;
            }
        }

        if (requiresVision)
        {

            if (grid.GetValueOrDefault(Vector2ToString(start))?.surfaceStatus?.surface == Surface.Smoke)
            {
                foreach (var cell in reachableCells.ToList())
                {
                    if (!IsAdjacentCell(start, cell))
                    {
                        reachableCells.Remove(cell);
                    }
                }
            }
            else
            {
                foreach (var cell in reachableCells.ToList())
                {
                    if (!IsCellInLineOfSight(start, cell))
                    {
                        reachableCells.Remove(cell);
                    }
                }
            }
        }

        return reachableCells;
    }

    private string Vector2ToString(Vector2I vector)
    {
        return vector.ToString();
    }

    private string CoordsToString(int x, int y)
    {
        return new Vector2I(x, y).ToString();
    }

    private bool IsCellInLineOfSight(Vector2I origin, Vector2I target)
    {
        int x0 = origin.X;
        int y0 = origin.Y;
        int x1 = target.X;
        int y1 = target.Y;

        int dx = Math.Abs(x1 - x0);
        int dy = Math.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            if (x0 == x1 && y0 == y1)
            {
                break;
            }

            if (currentMap.GetValueOrDefault(CoordsToString(x0, y0)) == null || currentMap[CoordsToString(x0, y0)].kind == CellKind.Obstacle || currentMap[CoordsToString(x0, y0)].kind == CellKind.Wall || ((currentMap[CoordsToString(x0, y0)].isOccupied || currentMap[CoordsToString(x0, y0)].surfaceStatus?.surface == Surface.Smoke) && (x0 != origin.X || y0 != origin.Y)))
            {
                return false;
            }

            int e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }

        return true; // Line of sight is clear
    }

    public Vector2I[] FindShortestPath(Vector2I start, Vector2I target, MapCell[] includableCells, bool stopBeforeTarget = false)
    {
        var grid = currentMap;

        // Array to track visited cells
        var visited = new HashSet<Vector2I>
        {
            start
        };

        // Queue for BFS
        var queue = new Queue<Vector2I>();
        queue.Enqueue(start);

        // Dictionary to store parent cells
        var parent = new Dictionary<Vector2I, Vector2I>();

        // Directions for moving in the grid (up, down, left, right)
        var dx = new[] { -1, 1, 0, 0 };
        var dy = new[] { 0, 0, -1, 1 };

        Func<Vector2I, Vector2I[]> rebuildPath = (current) =>
        {
            var path = new List<Vector2I>();
            while (current != start)
            {
                path.Add(current);
                current = parent[current];
            }
            path.Reverse();
            return path.ToArray();
        };

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            if (current == target)
            {
                // Found the target, reconstruct the path
                return rebuildPath(current);
            }

            // Check all adjacent cells
            for (var i = 0; i < 4; i++)
            {
                var newX = current.X + dx[i];
                var newY = current.Y + dy[i];

                if (newX == target.X && newY == target.Y && stopBeforeTarget)
                {
                    return rebuildPath(current);
                }

                var next = new Vector2I(newX, newY);

                if (grid.TryGetValue(Vector2ToString(next), out var cell) && Array.Find(includableCells, c => c.kind == cell.kind && c.isOccupied == cell.isOccupied) != null && !visited.Contains(next))
                {
                    visited.Add(next);
                    queue.Enqueue(next);
                    parent[next] = current;
                }
            }
        }

        return null;
    }

    public Vector2I? FindFreeCellNCellsAway(Dictionary<Vector2I, MapCell> grid, Vector2I target, int N)
    {
        // Array to track visited cells
        var visited = new HashSet<Vector2I> { target };

        // Queue for BFS
        var queue = new Queue<(Vector2I, int)>();
        queue.Enqueue((target, 0));

        // Directions for moving in the grid (up, down, left, right)
        var dx = new[] { -1, 1, 0, 0 };
        var dy = new[] { 0, 0, -1, 1 };

        while (queue.Count > 0)
        {
            (var current, var distance) = queue.Dequeue();

            for (var i = 0; i < 4; i++)
            {
                var nextX = current.X + dx[i];
                var nextY = current.Y + dy[i];
                var next = new Vector2I(nextX, nextY);

                if (grid.TryGetValue(next, out var nextCell) && !visited.Contains(next) && nextCell.kind == CellKind.Empty && nextCell.isOccupied == false)
                {
                    if (distance + 1 == N)
                    {
                        return next;
                    }
                    visited.Add(next);
                    queue.Enqueue((next, distance + 1));
                }
            }
        }

        // No free cell N cells away found
        return null;
    }

    private Interactive GetInteractiveByPosition(Vector2I position)
    {
        var mapCell = ReadMapCell(position);

        // var contraptions = GameObject.FindGameObjectsWithTag("Contraption").Where(obj => obj.name.EndsWith("-" + position.X + ":" + position.Y)).ToArray();

        // if (mapCell.metadata.ContainsKey("entity"))
        // {
        //     return GameObject.Find(mapCell.metadata["entity"]);
        // }
        // else if (mapCell.metadata.ContainsKey("contraption"))
        // {
        //     return GameObject.Find(mapCell.metadata["contraption"]);
        // }
        return null;
    }

    private int CalculateCellDistance(Vector3I a, Vector3I b)
    {
        return Mathf.Abs(a.X - b.X) + Mathf.Abs(a.Y - b.Y);
    }

    private int CalculateCellDistance(Vector2I a, Vector2I b)
    {
        return Mathf.Abs(a.X - b.X) + Mathf.Abs(a.Y - b.Y);
    }



    private Vector3? GetPointerHoldPosition()
    {
        // if (Input.GetMouseButton(0))
        // {
        //     return Input.mousePosition;
        // }
        // else if ((Input.touchCount > 0) && (Input.GetTouch(0).phase == TouchPhase.Began))
        // {
        //     return Input.GetTouch(0).position;
        // }
        return null;
    }


    private Vector3? GetPointerDownPosition()
    {
        // if (Input.GetMouseButtonDown(0))
        // {
        //     return Input.mousePosition;
        // }
        // else if ((Input.touchCount > 0) && (Input.GetTouch(0).phase == TouchPhase.Began))
        // {
        //     return Input.GetTouch(0).position;
        // }
        return null;
    }

    private Vector3? GetPointerUpPosition()
    {
        // if (Input.GetMouseButtonUp(0))
        // {
        //     return Input.mousePosition;
        // }

        // if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
        // {
        //     return Input.GetTouch(0).position;
        // }

        return null;
    }

    private bool IsAdjacentCell(Vector2I origin, Vector2I target)
    {
        var adjacentCells = GetAdjacentCells(origin);
        return adjacentCells.Contains(target);
    }


    List<Vector2I> GetAdjacentCells(Vector2I cell)
    {
        return new List<Vector2I>
        {
            new(cell.X - 1, cell.Y),
            new(cell.X + 1, cell.Y),
            new(cell.X, cell.Y - 1),
            new(cell.X, cell.Y + 1)
        };
    }

    List<Vector2I> GetAdjacentCellsSurface(Vector2I cell)
    {
        var surface = ReadMapCell(cell.X, cell.Y).surfaceStatus?.surface;
        if (surface == null)
        {
            return new List<Vector2I>();
        }
        List<Vector2I> adjacentCells = GetAdjacentCells(cell);
        adjacentCells = adjacentCells.Where(c => ReadMapCell(c.X, c.Y).surfaceStatus?.surface == surface).ToList();
        return adjacentCells;
    }

    List<Vector2I> GetSurfaceRegionCells(Vector2I cell, Surface surface)
    {
        List<Vector2I> regionCells = new();
        HashSet<Vector2I> visited = new();
        Queue<Vector2I> queue = new();

        queue.Enqueue(cell);
        visited.Add(cell);

        while (queue.Count > 0)
        {
            Vector2I currentCell = queue.Dequeue();
            regionCells.Add(currentCell);

            foreach (Vector2I adjacentCell in GetAdjacentCellsSurface(currentCell))
            {
                if (!visited.Contains(adjacentCell))
                {
                    queue.Enqueue(adjacentCell);
                    visited.Add(adjacentCell);
                }
            }
        }

        return regionCells;
    }

    Surface GetSurfaceResult(Surface? currentSurface, Surface newSurface)
    {
        if (currentSurface == Surface.Water && newSurface == Surface.Ice)
        {
            return Surface.Ice;
        }
        else if (currentSurface == Surface.Ice && newSurface == Surface.Fire)
        {
            return Surface.Water;
        }
        else if (currentSurface == Surface.Water && newSurface == Surface.Fire)
        {
            return Surface.Fog;
        }
        else if (currentSurface == Surface.Water && newSurface == Surface.Poison)
        {
            return Surface.Poison;
        }
        else if (currentSurface == Surface.Poison && newSurface == Surface.Fire)
        {
            return Surface.Smoke;
        }
        return newSurface;
    }

    private bool IsSectionLoaded(string name, int index)
    {
        return GetNode(name + "-" + index) != null;
    }

    private Node LoadSection(string name, int index)
    {
        var map = Importer.Import("res://resources/zones/maps/" + name + "-" + index + ".tmx");

        map.Name = name + "-" + index;
        GD.Print(map.Name);
        map.GlobalPosition = new Vector2(-1220, -1f);
        AddChild(map);
        var transparent = new Color(255, 255, 255, 0);
        var opaque = new Color(255, 255, 255, 1);
        // map.GetComponentsInChildren<SpriteRenderer>().ToList().ForEach(x => x.color = transparent);
        // Set map renderers to alpha 0 for animation
        // var childTransform = map.transform.Find("Grid/SectionCells").transform.GetChild(0).GetChild(0).GetChild(0);
        // var childName = childTransform.name;
        // var items = childName.Split(":");
        // var chunks = items[0].Split("-");
        // var identifier = items[1];

        Func<string, (int, int, string)> extractValues = input =>
        {
            // var pattern = @"(\d+)-(\d+):(\w+)";
            var pattern = @"(\d+)-(\d+)_(\w+)";
            var match = System.Text.RegularExpressions.Regex.Match(input, pattern);

            if (match.Success)
            {
                return (
                    int.Parse(match.Groups[1].Value),
                    int.Parse(match.Groups[2].Value),
                    match.Groups[3].Value
                );
            }
            else
            {
                throw new FormatException("Input string does not match the expected pattern");
            }
        };

        if (index != 1)
        {
            foreach (Node2D child in map.GetNode("SectionCells").GetChildren())
            {
                try
                {
                    // var childName = child.Name.ToString().Replace(" (TRS)", "");
                    var childName = child.Name.ToString();
                    var (chunk0, chunk1, identifier) = extractValues(childName);

                    if (chunk0 == index || identifier != "a")
                    {
                        continue;
                    }
                    var otherSection = GetNodeOrNull<Node2D>(name + "-" + chunk0);
                    if (otherSection == null)
                    {
                        continue;
                    }
                    var matchingCell = otherSection.GetNodeOrNull<Node2D>("SectionCells/" + childName);
                    if (matchingCell == null)
                    {
                        continue;
                    }
                    var targetPosition = matchingCell.GlobalPosition - child.Position;
                    var prevParent = child.GetParent();

                    map.GlobalPosition = targetPosition;
                    break;
                }
                catch (FormatException)
                {
                    continue;
                }
            }
        }
        // foreach (Transform sectionCell in map.transform.Find("Grid/SectionCells").transform)
        // {
        //     var child = sectionCell.GetChild(0).GetChild(0).GetChild(0);
        //     var arrowName = "arrow:" + ((Vector3I)BattleMap.EntityPositionToCellCoordinates(child.position)).ToString();
        //     if (GetNode(arrowName) == null)
        //     {
        //         var arrow = new GameObject
        //         {
        //             name = arrowName
        //         };
        //         var spriteRenderer = arrow.AddComponent<SpriteRenderer>();
        //         spriteRenderer.sortingOrder = 7;
        //         spriteRenderer.sprite = Resources.Load<Sprite>("arrow-top");
        //         arrow.transform.position = new Vector3(sectionCell.position.X - 1f, sectionCell.position.Y, 0);
        //     }
        // }
        // float time = 0;
        // var duration = 10f;
        // while (time < duration)
        // {
        //     map.GetComponentsInChildren<SpriteRenderer>().ToList().ForEach((x) => x.color = Color.Lerp(transparent, opaque, time / duration));
        //     time += Time.deltaTime;
        // }

        void disableTilemap(string input)
        {
            var item = map.GetNodeOrNull<Node2D>(input);
            if (item != null) item.Visible = false;
        }

        disableTilemap("Ground");
        disableTilemap("SectionCells");
        disableTilemap("ForegroundStoppers");
        disableTilemap("BackgroundStoppers");
        disableTilemap("EntitySlots");
        disableTilemap("Gap");

        void applyTranslation(string input)
        {
            var item = map.GetNodeOrNull<Node2D>(input);
            if (item != null) item.Position += new Vector2(-1f, -0.5f);
        }

        // // applyTranslation("Grid/SectionCells");
        // applyTranslation("Grid/Obstacles");
        // applyTranslation("Grid/PlayerSpawnSlot");
        // applyTranslation("Grid/Foreground");
        // applyTranslation("Grid/EntitySlots");
        // applyTranslation("Grid/Background");
        // applyTranslation("Grid/Contraptions");
        // // applyTranslation("Grid/ForegroundStoppers");
        // // applyTranslation("Grid/BackgroundStoppers");
        // applyTranslation("Grid/Paths");

        void setSortingOrder(string input, int sortingOrder)
        {
            var item = map.GetNodeOrNull<CanvasItem>(input);
            if (item != null) item.ZIndex = sortingOrder;
        }
        setSortingOrder("Grid/Paths", 2);

        var foreground = map.GetNodeOrNull<Node2D>("Grid/Foreground");
        if (foreground != null)
        {
            // var tint = foreground.GetComponent<SuperObjectLayer>().m_TintColor;
            foreach (var child in foreground.GetChildren())
            {
                var obstacle = (Node2D)child;
                // var obstacle = child.GetChild(0).GetChild(0).GetChild(0).GetComponent<SpriteRenderer>();
                // obstacle.color = tint;
                obstacle.ZIndex = GetSortingOrder(tilemap.LocalToMap(obstacle.Position));
            }
        }
        // var obstacles = map.transform.Find("Grid/Obstacles");
        // if (obstacles != null)
        // {
        //     var tint = obstacles.GetComponent<SuperObjectLayer>().m_TintColor;
        //     foreach (Transform child in obstacles.transform)
        //     {
        //         var obstacle = child.GetChild(0).GetChild(0).GetChild(0).GetComponent<SpriteRenderer>();
        //         obstacle.color = tint;
        //         obstacle.sortingOrder = GetSortingOrder((Vector2I)tilemap.LocalToMap(obstacle.transform.position));
        //     }
        // }
        var background = map.GetNodeOrNull<Node2D>("Grid/Background");
        if (background != null)
        {
            foreach (var layer in background.GetChildren())
            {
                // var tint = layer.GetComponent<SuperObjectLayer>().m_TintColor;
                foreach (var child in layer.GetChildren())
                {
                    // var obstacle = child.GetChild(0).GetChild(0).GetChild(0).GetComponent<SpriteRenderer>();
                    var obstacle = (Node2D)child;
                    // obstacle.color = tint;
                    obstacle.ZIndex = GetSortingOrder((Vector2I)tilemap.LocalToMap(obstacle.Position));
                }
            }
        }
        return map;
    }


    private void SpawnEntities(Map currentSection)
    {
        foreach (KeyValuePair<string, MapCell> cell in currentSection)
        {
            if (cell.Value.isOccupied == false || cell.Value.entitySlot != EntitySlot.Entity || cell.Value.metadata["entity"] == "Player")
            {
                continue;
            }
            var position = GridHelper.StringToVector2I(cell.Key);
            AddChild(LoadEntity(cell.Value.metadata["entity"], position));
        }
    }

    private Entity LoadEntity(string entity, Vector2I position)
    {
        var entityKind = entity.Split(":");
        var prefab = GD.Load<PackedScene>($"res://scenes/entities/{entityKind[0]}.tscn");
        var entityObject = prefab.Instantiate<Entity>();

        entityObject.Name = entity;
        MoveRendererToCell(entityObject, position);
        return entityObject;
    }

    public Entity GetEntityByName(string name)
    {
        var entity = GetNode<Entity>(name);
        if (entity == null)
        {
            return (Entity)GetTree().GetNodesInGroup("entity").FirstOrDefault((x) =>
            {
                var entity = (Entity)x;
                var entityKind = entity.Name.ToString().Split(":");
                return entityKind.Length > 1 && entityKind[1] == name;
            });
        }
        return entity;
    }

    private void PrepareBattle(List<Entity> involvedEntities)
    {
        turnEntities = involvedEntities.OrderBy(x => Guid.NewGuid()).ToList();
        involvedEntities.ForEach(entity =>
        {
            if (entity.IsInGroup("Entity"))
            {
                // var agent = (BasseAgent)entity;
                // var monster = monsters[entity.Name.Split(":")[0]];
                // entity.skills = entity.skillNames.Select(skillName => ((TurnSkillRestrictions, SkillBlueprint)?)(new TurnSkillRestrictions() { cooldown = 0 }, allSkills[skillName])).ToList();
                // agent.Setup(this, entity);
            }
        });
        Status = BattleStatus.Preparing;
    }

    private void StartBattle()
    {
        // if player starts, the turn order is not displayed?!
        turnEntityIndex = 0;
        turnCounter = 0;
        Status = BattleStatus.Ongoing;
    }

    private void EndBattle()
    {
        Status = BattleStatus.Exploring;
        turnEntities.Clear();
        // GameObject.FindGameObjectsWithTag("PlayableEntity").ToList().ForEach(gameObject =>
        // {
        //     var entity = gameObject.GetComponent<Entity>();
        //     entity.abilityPoints = entity.maxAbilityPoints;
        //     entity.movementPoints = entity.maxMovementPoints;
        // });
        HideMovementRange();
    }

    public void CraftSkill(string label, SkillBlueprint skill)
    {
        var skills = gameData.skills.ToList();
        var inventory = gameData.resources.ToList();

        foreach (var item in skill.ingredients)
        {
            var existingItem = inventory.FirstOrDefault(i => i.name == item.name);
            if (existingItem != null)
            {
                existingItem.quantity -= item.quantity;
                if (existingItem.quantity == 0)
                {
                    inventory.Remove(existingItem);
                }
            }
        }
        // skills.Add(new()
        // {
        //     label = label,
        //     name = skill.name,
        // });
        // gameData.skills = skills.ToArray();
        // gameData.resources = inventory.ToArray();
        // GameDataStorage = gameData;
    }

    // Used in inventory, need to move that
    public void SpawnPlayer()
    {
        // var prefab = Resources.Load<GameObject>("Battle/Prefabs/Entities/skeleton-Player");

        // var player = GameObject.Find("equipment") ?? Instantiate(prefab);
        // player.name = "equipment";

        // player.AddComponent<Interactive>();

        // player.transform.position = new Vector3(2.4f, -0.6f, 0);
        // player.transform.localScale = new Vector3(1f, 1f);
        // SetCompleteEquipment(player);
    }

    public void DestroyPlayer()
    {
        GetNode("equipment").SetProcess(false);
    }

    private async void StartCoroutine(IEnumerator routine)
    {
        await WaitAndPrint();
    }

    private async Task WaitAndPrint()
    {
        await ToSignal(GetTree().CreateTimer(2.0f), "timeout");
        GD.Print("2 seconds passed");
    }
}

class BattleMap
{
    public static TileMapLayer tilemap;

    public static Vector3 EntityPositionToCellPosition(Vector3 position)
    {
        return new Vector3(position.X, position.Y + 64, 0);
    }

    public static Vector2I EntityPositionToCellCoordinates(Vector2 position)
    {
        return tilemap.LocalToMap(Caster.Into(EntityPositionToCellPosition(Caster.Into(position))));
    }

    public static Vector2 CellPositionToEntityPosition(Vector2 position)
    {
        // return new Vector3(position.X, position.Y, Math.Abs(position.Y) / 100);
        return new Vector2(position.X + 56, position.Y - 92);
    }

    public static Vector2 CellCoordinatesToEntityPosition(Vector2I position)
    {
        return CellPositionToEntityPosition(tilemap.MapToLocal(position));
    }
}

public enum BattleStatus
{
    Exploring,
    Positioning,
    Ongoing,
    Preparing,
    Lost,
    Wan,
}

public class LootResult
{
    public string name;
    public int quantity;
}

public class GridHelper
{
    public static Direction GetRelativePosition(Vector2I positionA, Vector2I positionB)
    {
        if (positionB.X < positionA.X)
        {
            return Direction.Left;
        }
        else if (positionB.X > positionA.X)
        {
            return Direction.Right;
        }
        else if (positionB.Y > positionA.Y)
        {
            return Direction.Top;
        }
        else if (positionB.Y < positionA.Y)
        {
            return Direction.Bottom;
        }
        else
        {
            // The positions are the same
            return Direction.Left;
        }
    }

    public static Vector3I StringToVector3I(string input)
    {
        if (input.StartsWith("(") && input.EndsWith(")"))
        {
            input = input[1..^1];
        }

        string[] sArray = input.Split(',');

        var result = new Vector3I(
            int.Parse(sArray[0]),
            int.Parse(sArray[1]),
            int.Parse(sArray[2]));

        return result;
    }

    public static Vector2I StringToVector2I(string input)
    {
        if (input.StartsWith("(") && input.EndsWith(")"))
        {
            input = input[1..^1];
        }

        string[] sArray = input.Split(',');

        var result = new Vector2I(
            int.Parse(sArray[0]),
            int.Parse(sArray[1])
        );

        return result;
    }
}
class Random
{
    public static int Range(int min, int max)
    {
        var rand = new System.Random();
        return rand.Next(min, max + 1);
    }
}