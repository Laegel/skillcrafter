using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class Tabs : BuilderComponent
{
    private ReactiveState<int> currentTab;
    private Dictionary<string, Node> tabs;

    public Tabs(Dictionary<string, Node> tabs)
    {
        this.tabs = tabs;
        currentTab = new(0);
    }

    public Node Build()
    {
        var marginContainer = new MarginContainer();
        marginContainer.AddThemeConstantOverride("margin_top", 50);
        return NodeBuilder.CreateNode(new VBoxContainer { }, NodeBuilder.Map(new HBoxContainer()
        {
        }, tabs.Select((tab, index) =>
        {
            var button = new TextureButton()
            {
                CustomMinimumSize = new Vector2(100, 50),
            };
            button.AddChild(new Label { Text = tab.Key });
            button.Pressed += () =>
            {
                currentTab.Value = index;
            };

            return NodeBuilder.CreateNode(button, NodeBuilder.Show(currentTab.Map(x => x == index), new Label
            {
                Text = "          *",
            }));
        }).ToList()), NodeBuilder.CreateNode(marginContainer, NodeBuilder.Match(
            currentTab,
            tabs.Select((tab, index) => new KeyValuePair<int, Func<Node>>(index, () => tab.Value)).ToDictionary(kv => kv.Key, kv => kv.Value)
        )));
    }
}
