using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class MenuInventory : BuilderComponent
{
    public override Node Build()
    {
        var size = DisplayServer.WindowGetSize();
        var items = Storage.Read();
        var gridItemSize = 100;
        var allGear = GearReader.Get();
        var allResources = ResourcesReader.Get();

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
        return NodeBuilder.CreateNode(new Tabs(new() {
            { "gear", makeGrid(items.gear.Select((gear, i) => {
                return new ItemComponent {
                    key = i,
                    gridItemSize = gridItemSize,
                    onLongPress = () => { },
                    borderColor = SCTheme.GetColorByQuality(allGear[gear.name].quality),
                    backgroundImage = ResourceLoader.Load<Texture2D>($"res://images/gear/{BattleScene.GetFullGearPath(allGear[gear.name])}.png"),
                }.Build();
            }).ToList()) },
            { "skill", makeGrid(items.skills.Select((skill, i) => {
                return new ItemComponent {
                    key = i,
                    gridItemSize = gridItemSize,
                    onLongPress = () => { },
                }.Build();
            }).ToList())  },
            { "resource", makeGrid(items.resources.Select((resource, i) => {
                return new ItemComponent {
                    key = i,
                    gridItemSize = gridItemSize,
                    onLongPress = () => { },
                    backgroundImage = ResourceLoader.Load<Texture2D>($"res://images/resources/{resource.name}.png"),
                    borderColor = SCTheme.GetColorByRarity(allResources[resource.name].rarity),
                }.Build();
            }).ToList()) },
        }).Build());
    }
}
