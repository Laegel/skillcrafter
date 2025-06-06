using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class MenuDesigner : BuilderComponent
{
    public override Node Build()
    {
        var size = DisplayServer.WindowGetSize();
        var items = Storage.Read();
        var allResources = ResourcesReader.Get();


        Func<List<Node>, Node> makeTwoColumnGrid = (children) => NodeBuilder.CreateNode(
            new GridContainer
            {
                Columns = 2,
                CustomMinimumSize = new Vector2(size.X, 400),
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                SizeFlagsVertical = Control.SizeFlags.ExpandFill,
            }, children.ToArray());

        var columnContent = new List<Node>
    {
        new ConfigurationPanel().Build(),
        new CostAndResultPanel().Build(),
    };

        return NodeBuilder.CreateNode(new VBoxContainer
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill,
        }, makeTwoColumnGrid(columnContent));
    }
}

public partial class ConfigurationPanel : BuilderComponent
{
    public override Node Build()
    {
        /*
        Skill name: text
        Skill effect: dropdown ("Damage", "Heal", "Buff", etc.)
        Cost
        - AP: number 0-10
        - MP: number 0-10
        - HP: number 0-90
        Range: numeric range input (1-10)
        Target radius: numeric range input (1-5)
        */
        return NodeBuilder.CreateNode(new VBoxContainer
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill,
        },
        new LineEdit
        {
            PlaceholderText = "Skill Name",
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
        },
        new Dropdown
        {

            Items = new[] { "Damage", "Heal", "Buff", "Other" },
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
        }, NodeBuilder.CreateNode(new HBoxContainer(),
        
            new Label { Text = "AP:", SizeFlagsHorizontal = Control.SizeFlags.ExpandFill },
            new SpinBox { MinValue = 0, MaxValue = 10, SizeFlagsHorizontal = Control.SizeFlags.ExpandFill }
        )
        // ,
        // new HBoxContainer
        // {
        //     Children = new Node[]
        //     {
        //     new Label { Text = "MP:", SizeFlagsHorizontal = Control.SizeFlags.ExpandFill },
        //     new SpinBox { MinValue = 0, MaxValue = 10, SizeFlagsHorizontal = Control.SizeFlags.ExpandFill },
        //     }
        // },
        // new HBoxContainer
        // {
        //     Children = new Node[]
        //     {
        //     new Label { Text = "HP:", SizeFlagsHorizontal = Control.SizeFlags.ExpandFill },
        //     new SpinBox { MinValue = 0, MaxValue = 90, SizeFlagsHorizontal = Control.SizeFlags.ExpandFill },
        //     }
        // },
        // new HBoxContainer
        // {
        //     Children = new Node[]
        //     {
        //     new Label { Text = "Range:", SizeFlagsHorizontal = Control.SizeFlags.ExpandFill },
        //     new SpinBox { MinValue = 1, MaxValue = 10, SizeFlagsHorizontal = Control.SizeFlags.ExpandFill },
        //     }
        // },
        // new HBoxContainer
        // {
        //     Children = new Node[]
        //     {
        //     new Label { Text = "Target Radius:", SizeFlagsHorizontal = Control.SizeFlags.ExpandFill },
        //     new SpinBox { MinValue = 1, MaxValue = 5, SizeFlagsHorizontal = Control.SizeFlags.ExpandFill },
        //     }
        // }
        );
    }
}

public partial class CostAndResultPanel : BuilderComponent
{
    public override Node Build()
    {
        /*
        Grid with radii
        Skill icon
        Skill tooltip
        - Skill name
        - Damages
        - Cost
        - Stability
        - Range
        - Target radius
        */
        return NodeBuilder.CreateNode(new VBoxContainer
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill,
        }, new Label
        {
            Text = "Cost and Result Panel",
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill
        });
    }
}

public partial class Dropdown : OptionButton
{
    private string[] _items;

    public string[] Items
    {
        get => _items;
        set
        {
            _items = value;
            Clear();
            foreach (var item in _items)
            {
                AddItem(item);
            }
        }
    }
}