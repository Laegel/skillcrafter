using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class Tabs : BuilderComponent
{
    private ReactiveState<int> currentTab;
    private Dictionary<string, Node> tabs;

    public Action OnChange;

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
            return NodeBuilder.Watch(new Control() { CustomMinimumSize = new Vector2(100, 50), }, () =>
            {
                var button = new Button()
                {
                    CustomMinimumSize = new Vector2(100, 50),
                };
                button.AddChild(new Label { Text = tab.Key });
                button.Pressed += () =>
                {
                    currentTab.Value = index;
                    OnChange();
                };

                var style = new StyleBoxFlat
                {
                    BgColor = SCTheme.Base100,
                };
                style.SetBorderWidthAll(2);

                    style.BorderColor = currentTab == index ? SCTheme.Content : SCTheme.Base300;
                style.SetCornerRadiusAll(8);
                button.AddThemeStyleboxOverride("normal", style);
                return button;
            }, currentTab);
        }).ToList()), NodeBuilder.CreateNode(marginContainer, NodeBuilder.Match(
            currentTab,
            tabs.Select((tab, index) => new KeyValuePair<int, Func<Node>>(index, () => tab.Value)).ToDictionary(kv => kv.Key, kv => kv.Value)
        )));
    }
}
