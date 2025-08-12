using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class MenuInventory : BuilderComponent
{
    public Node Build(Storage store, TreeService treeService, ParchmentState parchmentState, MenuState menuState)
    {
        var size = DisplayServer.WindowGetSize();
        var items = store.GameData;

        var allGear = GearReader.Get();
        var allResources = ResourcesReader.Get();

        ReactiveState<StorageItem> selectedItem = new();
        
        Func<List<Node>, Node> makeGrid = (children) => NodeBuilder.CreateNode(new ScrollContainer()
        {
            ScrollVertical = 1,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill,
            CustomMinimumSize = new Vector2(size.X, 400),
        }, NodeBuilder.Map(new GridContainer
        {
            Columns = 10,
            CustomMinimumSize = new Vector2(size.X, 400),
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill,
        }, children));


        var contextMenu = NodeBuilder.Watch(new CanvasLayer()
        {
            Transform = new Transform2D(1, 0, 0, 1, 0, 0),
            Layer = 2,
        }, () =>
        {
            if (selectedItem == null)
            {
                return new Node();
            }

            var res = selectedItem.Value switch
            {
                Parchment parchment => NodeBuilder.CreateNode(new VBoxContainer() { },
                    new DynamicLabel() { Text = $"[theme classes=text-rarity-{parchment.Rarity}]Parchment[/theme]" },
                    new HSeparator(),
                    new Label() { Text = "Rarity: " + parchment.Rarity.GetEnumDescription() },
                    new Label() { Text = "Effect: " + parchment.SkillConfiguration.SkillEffect.Value.GetEnumDescription() },
                    new CallbackButton()
                    {
                        Text = "Go to Craft",
                        Callback = () =>
                        {
                            parchmentState.Parchment = (Parchment)selectedItem;
                            menuState.currentMenu.Value = Menus.Parchment;
                        }
                    }
                ),
                DisplayableResource resource => NodeBuilder.CreateNode(new VBoxContainer() { },
                    new DynamicLabel() { Text = $"[theme classes=text-rarity-{resource.rarity}]{resource.name}[/theme]" },
                    new HSeparator(),
                    new Label() { Text = "Rarity: " + resource.rarity },
                    new Label() { Text = resource.description }
                ),
                Gear gear => NodeBuilder.CreateNode(new VBoxContainer() {},
                    new DynamicLabel() { Text = $"[theme classes=text-rarity-{gear.quality}]{gear.name}[/theme]" },
                    new HSeparator(),
                    new Label() { Text = "Set: " + gear.set },
                    new Label() { Text = "Slot: " + gear.slot.GetEnumDescription() },
                    new Label() { Text = "Type: " + gear.type.GetEnumDescription() },
                    new Label() { Text = "Quality: " + gear.quality.GetEnumDescription() }
                ),
                SkillBlueprint skill => new SkillTooltip() { SkillConfiguration = new()
                {
                    Name = skill.Name,
                    Range = new(skill.Range.Min, skill.Range.Max),
                    TargetRadius = new(skill.TargetRadius.Min, skill.TargetRadius.Max),
                    AP = skill.AP,
                    MP = skill.MP,
                    HP = skill.HP,
                    Stability = skill.Stability,
                    SkillEffect = skill.SkillEffect,
                    SkillEffectConfiguration = skill.SkillEffectConfiguration,
                    Visibility = skill.Visibility,
                    Cooldown = skill.Cooldown,
                    
                } },
                _ => new Node()
            };
            var node = treeService.GetNodeRecursively<Button>("ItemComponent" + selectedItem);

            return NodeBuilder.CreateNode(new BoxContainer()
            {
                AnchorRight = 0.0f,
                AnchorLeft = 1.0f,
                AnchorTop = 0.0f,
                AnchorBottom = 1.0f,

                GrowHorizontal = Control.GrowDirection.Begin,
                GrowVertical = Control.GrowDirection.Both,
                CustomMinimumSize = new(200, 100),
                Position = new(0, 0),
                SizeFlagsVertical = Control.SizeFlags.ExpandFill
            }, res);
        }, selectedItem);

        var onClickItem = (StorageItem item) => {
            selectedItem.Value = item;
        };

        var watchSelectedItem = (Func<Node> lazy) => NodeBuilder.Watch(new Node2D(), lazy, selectedItem);

        return NodeBuilder.CreateNode(new Tabs(new() {
            { "gear", watchSelectedItem(() => makeGrid(items.gear.Select((gear, i) => {
                return (Node)new ItemComponent {
                    Key = i,
                    onLongPress = () => { },
                    OnClick = () => onClickItem(allGear[gear.name]),
                    borderColor = SCTheme.GetColorByQuality(allGear[gear.name].quality),
                    BackgroundImage = ResourceLoader.Load<Texture2D>($"res://images/gear/{BattleScene.GetFullGearPath(allGear[gear.name])}.png"),
                    BorderWidth = selectedItem == allGear[gear.name] ? 4 : 2
                };
            }).ToList())) },
            { "skill", watchSelectedItem(() => makeGrid(items.skills.Select((skill, i) => {
                return (Node)new ItemComponent {
                    Key = i,
                    OnClick = () => onClickItem(skill),
                    borderColor = SCTheme.Content,
                    onLongPress = () => { },
                    BorderWidth = selectedItem == skill ? 4 : 2
                };
            }).ToList()))  },
            { "resource", watchSelectedItem(() => makeGrid(items.resources.Select((resource, i) => {
                return (Node)new ItemComponent {
                    Key = i,
                    onLongPress = () => { },
                    OnClick = () => onClickItem(allResources[resource.name]),
                    BackgroundImage = ResourceLoader.Load<Texture2D>($"res://images/resources/{resource.name}.png"),
                    borderColor = SCTheme.GetColorByRarity(allResources[resource.name].rarity),
                    BorderWidth = selectedItem == allResources[resource.name] ? 4 : 2
                };
            }).ToList())) },
            { "parchment", watchSelectedItem(() => makeGrid(items.Parchments.Select((parchment, i) => {
                return (Node)new ItemComponent {
                    Key = i,
                    onLongPress = () => {},
                    OnClick = () => onClickItem(parchment),
                    BackgroundImage = ResourceLoader.Load<Texture2D>($"res://images/parchment/parchment.png"),
                    borderColor = SCTheme.GetColorByRarity(parchment.Rarity),
                    BorderWidth = selectedItem == parchment ? 4 : 2
                };
            }).ToList())) },

        })
        {
            OnChange = () =>
        {
            selectedItem.Value = null;
        }
        }, contextMenu);
    }
}
