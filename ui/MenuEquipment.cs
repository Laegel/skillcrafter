using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using System.Linq;
using System;

public enum ItemType
{
    [Description("gear")]
    Gear,
    [Description("resource")]
    Resource,
    [Description("skill")]
    Skill,
}

public class ItemFilterState
{
    private static string _state;

    public static ItemType Type
    {
        get => Enum.Parse<ItemType>(_state.Split(':')[0]);
        set
        {
            var category = Category;
            var newState = $"{value}:{category}";
            if (_state != newState)
            {
                _state = newState;
                OnValueChanged?.Invoke();
            }
        }
    }

    public static int Category
    {
        get => int.Parse(_state.Split(':')[1]);
        set
        {
            var type = Type;
            var newState = $"{type}:{value}";
            if (_state != newState)
            {
                _state = newState;
                OnValueChanged?.Invoke();
            }
        }
    }

    public static void Set(ItemType type, int category)
    {
        _state = $"{type}:{category}";
        OnValueChanged?.Invoke();
    }

    public static event Action OnValueChanged;
}

public partial class MenuEquipment : BuilderComponent
{
    private Node node;
    private GameData gameData = Storage.Read();
    private Dictionary<string, SkillBlueprint> allSkills = SkillsReader.Get();
    private Dictionary<string, Gear> allGear = GearReader.Get();
    private Dictionary<string, DisplayableItem> allResources = ResourcesReader.Get();
    private int gridItemSize = 100;
    private ReactiveState<List<Node>> itemsToDisplay = new(new());
    public override Node Build()
    {
        var allGear = GearReader.Get();
        var allResources = ResourcesReader.Get();
        var border = 4;


        var getGearBySlot = (GearSlot slot) =>
        {
            return GearSlotState.equipedGear.Value[slot];
        };

        ItemFilterState.OnValueChanged += () =>
        {
            itemsToDisplay.Value = GetItemsSection();
        };

        var onClick = (GearSlot gearSlot) =>
        {
            ItemFilterState.Set(ItemType.Gear, (int)gearSlot);
        };
        Func<GearSlot, Node> getGearItemComponent = (gearSlot) =>
        {
            var slotItem = getGearBySlot(gearSlot);

            var itemComponent = new ItemComponent
            {
                gridItemSize = gridItemSize,
                onClick = () => onClick(gearSlot),
            };

            if (slotItem != null)
            {
                var gear = allGear[slotItem.name];
                itemComponent.backgroundImage = GD.Load<Texture2D>($"res://images/gear/{BattleScene.GetFullGearPath(gear)}.png");
                itemComponent.borderColor = SCTheme.GetColorByQuality(gear.quality);
            }
            else
            {
                itemComponent.backgroundImage = GD.Load<Texture2D>("res://images/skills/bareHandedStrike.png");
                itemComponent.borderColor = SCTheme.GetColorByQuality(Quality.Worn);
            }

            return itemComponent.Build();
        };

        var panelsContainer = new GridContainer()
        {
            CustomMinimumSize = new Vector2(gridItemSize * 3, gridItemSize * 3),
            Columns = 3,
        };
        panelsContainer.AddThemeConstantOverride("h_separation", border);
        panelsContainer.AddThemeConstantOverride("v_separation", border);

        var blankSpot = () => new Control()
        {
            CustomMinimumSize = new Vector2(gridItemSize, gridItemSize),
        };
        
        var equipmentContainer = NodeBuilder.Map(panelsContainer,
            GearSlotState.equipedGear.Map((items) =>
            {
                return new List<Node>
                {
                    blankSpot(), getGearItemComponent(GearSlot.Head), blankSpot(),
                    getGearItemComponent(GearSlot.Arms), getGearItemComponent(GearSlot.Torso), getGearItemComponent(GearSlot.Weapon),
                    blankSpot(), getGearItemComponent(GearSlot.Legs), blankSpot()
                };
            }
        ), (item, i) => item);

        var itemsContainer = new GridContainer()
        {
            Columns = 10,
        };
        itemsContainer.AddThemeConstantOverride("h_separation", border);
        itemsContainer.AddThemeConstantOverride("v_separation", border);

        node = NodeBuilder.CreateNode(new VBoxContainer(), equipmentContainer, NodeBuilder.Map(itemsContainer, itemsToDisplay, (item, i) => item));
        return node;
    }

    private List<Node> GetItemsSection()
    {
        var items = new List<Node>();

        if ((ItemFilterState.Type == ItemType.Gear && ItemFilterState.Category == (int)GearSlot.Weapon) || ItemFilterState.Type == ItemType.Skill)
        {
            var crossIcon = new ItemComponent
            {
                gridItemSize = gridItemSize,
                onClick = OnItemClick,
                borderColor = SCTheme.GetColorByQuality(Quality.Worn),
                backgroundImage = GD.Load<Texture2D>("res://images/skills/bareHandedStrike.png"),
            };
            items.Add(crossIcon.Build());
        }

        switch (ItemFilterState.Type)
        {
            case ItemType.Gear:
                foreach (var item in gameData.gear.Where(item =>
                     (int)allGear[item.name].slot == ItemFilterState.Category))
                {
                    var gear = (Gear)GetItemByName(item.name);
                    var gearNode = NodeBuilder.CreateNode(new ItemComponent
                    {
                        gridItemSize = gridItemSize,
                        borderColor = SCTheme.GetColorByQuality(allGear[gear.name].quality),
                        backgroundImage = GD.Load<Texture2D>($"res://images/gear/{BattleScene.GetFullGearPath(gear)}.png"),
                        onClick = () => OnSelectGear(gear),
                    }.Build());

                    items.Add(gearNode);
                }
                break;
            case ItemType.Skill:
                foreach (var skill in gameData.skills)
                {
                    var skillNode = NodeBuilder.CreateNode(new ItemComponent()
                    {
                        gridItemSize = gridItemSize,
                        onClick = () => OnSelectSkill(skill),
                        // backgroundImage = GD.Load<Texture2D>($"res://images/skills/{skill.name}.png"),
                    }.Build());

                    items.Add(skillNode);

                }
                break;

        }
        // foreach (var item in gameData.Get(ItemFilterState.type.GetEnumDescription()).Where(item =>
        //     ItemFilterState.type == ItemType.Skill || battleScene.AllGear[item.Name].Slot == ItemFilterState.category))
        // {
        //     var skillOrGear = GetItemByName(item.Name);

        //     if (skillOrGear is SkillBlueprint skill)
        //     {
        //         var skillNode = NodeBuilder.CreateNode(new Button
        //         {
        //             Text = skill.name,
        //             CustomMinimumSize = new Vector2(100, 100),
        //             // MarginTop = 10,
        //             // MarginLeft = 10
        //         });

        //         // skillNode.Connect("pressed", this, nameof(OnSkillClick), new Godot.Collections.Array { skill });
        //         itemContainer.AddChild(skillNode);
        //     }
        //     else if (skillOrGear is Gear gear)
        //     {
        //         var gearNode = NodeBuilder.CreateNode(new Button
        //         {
        //             Text = "",
        //             CustomMinimumSize = new Vector2(100, 100),
        //             // MarginTop = 10,
        //             // MarginLeft = 10,
        //             // Modulate = GetBorderColor(gear)
        //         });

        //         // gearNode.Connect("pressed", this, nameof(OnGearClick), new Godot.Collections.Array { gear });
        //         itemContainer.AddChild(gearNode);

        //         var gearIcon = new TextureRect
        //         {
        //             Texture = GD.Load<Texture2D>($"res://images/gear/{BattleScene.GetFullGearPath(gear)}.png"),
        //             StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered
        //         };
        //         gearNode.AddChild(gearIcon);
        //     }
        // }

        // Add the item container to the parent node
        return items;
    }

    // Event handlers
    private void OnItemClick()
    {
        if (ItemFilterState.Type == ItemType.Gear)
        {
            gameData.gearSlots[(GearSlot)ItemFilterState.Category] = null;
            // BattleScene.SetEquipment("equipment", ItemFilterState.category, null);
            // BattleScene.SetEquipment("Player", ItemFilterState.category, null);
        }
        else
        {
            gameData.skillSlots[(int)ItemFilterState.Category] = null;
        }

        UpdateGameData();
    }

    private void OnSelectSkill(SkillBlueprint skill)
    {
        SkillSlotsState.Set(ItemFilterState.Category, (new TurnSkillRestrictions(), skill));
        UpdateGameData();
    }

    private void OnSelectGear(Gear gear)
    {
        var item = GetItemByName(gear.name);
        GearSlotState.Set((GearSlot)ItemFilterState.Category, gear.name);
        // gameData.gearSlots[ItemFilterState.category] = gear;
        // var gear = gear.type === GearType.Weapon
        //                             ? gear.set + "/" + gear.name
        //                             : battleScene.GearTypeToFolder(gear.type) +
        //                                 "/" +
        //                                 gear.set;
        // BattleScene.SetEquipment(node.GetTree().Root.GetNode<Entity>("Player"), ItemFilterState.category, gear);
        // BattleScene.SetEquipment(node.GetTree().Root.GetNode<Entity>("Equipment"), ItemFilterState.category, gear);
        UpdateGameData();
    }

    private void UpdateGameData()
    {
        // var skills = BattleScene.SetSkillsFromGear();
        // var playerEntity = GameObject.Find("Player").GetComponent<Entity>();
        // playerEntity.Skills = skills;
        // ItemFilterState.type = null;
    }

    private object GetItemByName(string name)
    {
        if (allResources.ContainsKey(name))
        {
            return allResources[name];
        }
        else if (allSkills.ContainsKey(name))
        {
            return allSkills[name];
        }
        else if (allGear.ContainsKey(name))
        {
            return allGear[name];
        }

        return null;
    }
}
