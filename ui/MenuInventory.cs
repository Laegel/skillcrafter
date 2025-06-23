using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class MenuInventory : BuilderComponent
{
    public Node Build(Storage store)
    {
        var size = DisplayServer.WindowGetSize();
        var items = store.Read();

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
                return (Node)new ItemComponent {
                    Key = i,
                    onLongPress = () => { },
                    borderColor = SCTheme.GetColorByQuality(allGear[gear.name].quality),
                    backgroundImage = ResourceLoader.Load<Texture2D>($"res://images/gear/{BattleScene.GetFullGearPath(allGear[gear.name])}.png"),
                };
            }).ToList()) },
            { "skill", makeGrid(items.skills.Select((skill, i) => {
                return (Node)new ItemComponent {
                    Key = i,
                    onLongPress = () => { },
                };
            }).ToList())  },
            { "resource", makeGrid(items.resources.Select((resource, i) => {
                return (Node)new ItemComponent {
                    Key = i,
                    onLongPress = () => { },
                    backgroundImage = ResourceLoader.Load<Texture2D>($"res://images/resources/{resource.name}.png"),
                    borderColor = SCTheme.GetColorByRarity(allResources[resource.name].rarity),
                };
            }).ToList()) },
        }));
    }
}
