using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Godot;

public enum SkillKinds
{
    [Description("Deal Damage ")]
    DealDamage,
    [Description("Attract")]
    Attract,
    [Description("Repel")]
    Repel,
    [Description("Activate")]
    Activate,
    [Description("Move")]
    Move,
    [Description("Apply Debuff")]
    ApplyDebuff,
    [Description("Apply Buff")]
    ApplyBuff,
    [Description("Create Surface")]
    CreateSurface,
    [Description("Consume Surface")]
    ConsumeSurface,
}


public partial class SkillConfiguration
{
    public ReactiveState<string> SkillName = new("New Skill");
    public ReactiveState<SkillKinds> SkillKind = new(SkillKinds.DealDamage);
    public ReactiveState<int> Cooldown { get; set; } = new(0);
    public ReactiveState<int> AP = new(6);
    public ReactiveState<int> MP = new(0);
    public ReactiveState<int> HP = new(0);
    public (ReactiveState<int> Min, ReactiveState<int> Max) Range = (new ReactiveState<int>(1), new ReactiveState<int>(10));
    public (ReactiveState<int> Min, ReactiveState<int> Max) TargetRadius = new(new ReactiveState<int>(1), new ReactiveState<int>(5));
    public ReactiveState<(int, int, int)> Stability = new((5, 10, 90));
}

public enum SkillConfigurationTabs
{
    Spec,
    Range,
    Constraints,
    Style, // Not sure to keep it
}

public partial class MenuDesigner : BuilderComponent
{
    public Node Build()
    {
        var size = DisplayServer.WindowGetSize();
        var items = Storage.Read();
        var allResources = ResourcesReader.Get();
        var skillConfiguration = new SkillConfiguration();


        skillConfiguration.AP.OnValueChanged += (v) =>
        {
            skillConfiguration.Stability.Value = CalculateStability(skillConfiguration);
        };

        skillConfiguration.MP.OnValueChanged += (v) =>
        {
            skillConfiguration.Stability.Value = CalculateStability(skillConfiguration);
        };

        skillConfiguration.HP.OnValueChanged += (v) =>
        {
            skillConfiguration.Stability.Value = CalculateStability(skillConfiguration);
        };

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
        new ConfigurationPanel(skillConfiguration),
        new CostAndResultPanel(skillConfiguration),
    };

        return NodeBuilder.CreateNode(new VBoxContainer
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill,
        }, makeTwoColumnGrid(columnContent));
    }

    private (int, int, int) CalculateStability(SkillConfiguration skillConfig)
    {
        // Define weights for each attribute
        const float apWeight = 1.5f;
        const float mpWeight = 1.2f;
        const float hpWeight = 1.0f;
        const float rangeWeight = 0.8f;
        const float targetRadiusWeight = 0.7f;

        // Calculate weighted values
        float apImpact = skillConfig.AP.Value * apWeight;
        float mpImpact = skillConfig.MP.Value * mpWeight;
        float hpImpact = skillConfig.HP.Value * hpWeight;
        float rangeImpact = (skillConfig.Range.Max.Value - skillConfig.Range.Min.Value) * rangeWeight;
        float targetRadiusImpact = (skillConfig.TargetRadius.Max.Value - skillConfig.TargetRadius.Min.Value) * targetRadiusWeight;

        // Aggregate the impacts
        float totalImpact = apImpact + mpImpact + hpImpact + rangeImpact + targetRadiusImpact;

        // Normalize the Stability levels (e.g., split into critical failure, failure, success, critical success)
        int criticalFailure = Math.Min(10 + (int)(totalImpact * 0.1f), 100);
        int failure = Math.Min(criticalFailure + (int)(totalImpact * 0.2f), 100);
        int success = Math.Min(failure + (int)(totalImpact * 0.4f), 100);

        return (criticalFailure, failure, success);
    }
}

public partial class ConfigurationPanel : BuilderComponent
{
    public SkillConfiguration skillConfiguration;
    private ReactiveState<SkillConfigurationTabs> currentTab = new(SkillConfigurationTabs.Spec);

    public ConfigurationPanel(SkillConfiguration skillConfiguration)
    {
        this.skillConfiguration = skillConfiguration;
    }
    public Node Build()
    {
        return NodeBuilder.CreateNode(new VBoxContainer
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill,
        },
        new TextInput
        {
            PlaceholderText = "Skill Name",
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            Value = skillConfiguration.SkillName,
        },
        new Dropdown
        {
            SelectedItem = skillConfiguration.SkillKind.Bind(x => (int)x, x => (SkillKinds)x),
            Items = Enum.GetValues(typeof(SkillKinds))
                .Cast<SkillKinds>()
                .Select(x => x.GetEnumDescription())
                .ToArray(),
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
        },

        NodeBuilder.CreateNode(new HBoxContainer()
        {
            SizeFlagsVertical = Control.SizeFlags.ExpandFill
        },

            NodeBuilder.Map(new VBoxContainer()
            {
                CustomMinimumSize = new Vector2(50, 50),
            }, Enum.GetValues(typeof(SkillConfigurationTabs))
                .Cast<SkillConfigurationTabs>()
                .Select((x) =>
                {
                    var button = new Button()
                    {
                        CustomMinimumSize = new Vector2(50, 50)
                    };
                    var border = 2;
                    var style = new StyleBoxFlat
                    {
                        BgColor = new Color(x switch
                        {
                            SkillConfigurationTabs.Spec => "#ffffff",
                            SkillConfigurationTabs.Range => "#ff0000",
                            SkillConfigurationTabs.Constraints => "#00ff00",
                            SkillConfigurationTabs.Style => "#0000ff"
                        }),
                    };
                    button.Pressed += () =>
                    {
                        currentTab.Value = x;
                    };
                    style.SetBorderWidthAll(border);
                    button.AddThemeStyleboxOverride("normal", style);
                    button.AddThemeStyleboxOverride("hover", style);
                    button.AddThemeStyleboxOverride("focus", style);
                    button.AddThemeStyleboxOverride("pressed", style);
                    return (Node)button;
                })
                .ToList()),

            NodeBuilder.Match(currentTab, new ScrollContainer()
            {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            }, new()
            {
                {SkillConfigurationTabs.Spec, () => NodeBuilder.Match(skillConfiguration.SkillKind, new VBoxContainer(), new() {
                    {SkillKinds.DealDamage, () => {
                        return new Label(){ Text = "Damage" };
                    }},
                    {SkillKinds.Attract, () => {
                        return new Label(){ Text = "Attract" };
                    }},
                })},
                {SkillConfigurationTabs.Range, () => NodeBuilder.CreateNode(new VBoxContainer(),
                    new Label()
                    {
                        Text = "Range"
                    },
                    new NumericRangeInput(skillConfiguration.Range, 1, 10, (value) =>
                    {
                        GD.Print($"Range changed to: {value.Item1} - {value.Item2}");
                    }),
                    new Label()
                    {
                        Text = "Target Radius"
                    },
                    new NumericRangeInput(skillConfiguration.TargetRadius, 1, 5, (value) =>
                    {
                        GD.Print($"Target radius changed to: {value.Item1} - {value.Item2}");
                    })
                )},
                {SkillConfigurationTabs.Constraints, () => NodeBuilder.CreateNode(new VBoxContainer(),
                    new Label { Text = "AP:", SizeFlagsHorizontal = Control.SizeFlags.ExpandFill },
                    new NumericInput(skillConfiguration.AP, new(0), new(10))
                    , NodeBuilder.CreateNode(new HBoxContainer(),
                        new Label { Text = "MP:", SizeFlagsHorizontal = Control.SizeFlags.ExpandFill },
                        new NumericInput(skillConfiguration.MP, new(0), new(10))
                    ), NodeBuilder.CreateNode(new HBoxContainer(),
                        new Label { Text = "HP:", SizeFlagsHorizontal = Control.SizeFlags.ExpandFill },
                        new NumericInput(skillConfiguration.HP, new(0), new(10))
                    ),
                    new Label()
                    {
                        Text = "Cooldown"
                    },
                    new NumericInput(skillConfiguration.Cooldown, new ReactiveState<int>(0), new ReactiveState<int>(100))
                    {
                        CustomMinimumSize = new Vector2(100, 50),
                    }
                )},
                {SkillConfigurationTabs.Style, () => new Label()
                    {
                        Text = "TBD"
                    }
                },
            })

        )

        );
    }
}

public partial class CostAndResultPanel : BuilderComponent
{
    public SkillConfiguration skillConfiguration;
    public CostAndResultPanel(SkillConfiguration skillConfiguration)
    {
        this.skillConfiguration = skillConfiguration;
    }
    public Node Build()
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
        },
            new DynamicLabel
            {
                Content = skillConfiguration.SkillName,
            },
            new DynamicLabel
            {
                Content = skillConfiguration.SkillKind.Map(x => x.GetEnumDescription()),
            },
            new StabilityBar(skillConfiguration.Stability)
            {
                CustomMinimumSize = new Vector2(500, 20)
            },
            new CallbackButton()
            {
                Callback = () =>
                {

                },
                Text = "Craft"
            }
        );
    }
}

