using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Godot;

using SkillKindConfiguration = ReactiveState<System.Collections.Generic.Dictionary<string, object>>;

public enum SkillKinds
{
    [Description("Deal Damage I")]
    DealDamage1,
    [Description("Deal Damage II")]
    DealDamage2,
    [Description("Deal Damage III")]
    DealDamage3,
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
    public ReactiveState<string> Name = "New Skill";
    public ReactiveState<SkillKinds> SkillKind = SkillKinds.DealDamage1;
    public SkillKindConfiguration SkillKindConfiguration = DealDamage.Init();
    public ReactiveState<int> Cooldown = 2;
    public ReactiveState<int> AP = 4;
    public ReactiveState<int> MP = 0;
    public ReactiveState<int> HP = 0;
    public (ReactiveState<int> Min, ReactiveState<int> Max) Range = (1, 5);
    public (ReactiveState<int> Min, ReactiveState<int> Max) TargetRadius = (1, 1);
    public ReactiveState<bool> Visibility = true;
    public ReactiveState<(int, int, int)> Stability = (5, 10, 90);
}

public enum SkillConfigurationTabs
{
    Spec,
    Scope,
    Constraints,
    Style, // Not sure to keep it
}

public partial class MenuDesigner : BuilderComponent
{
    public Node Build(Storage store)
    {
        var size = DisplayServer.WindowGetSize();
        var items = store.GameData;
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
        float apImpact = skillConfig.AP * apWeight;
        float mpImpact = skillConfig.MP * mpWeight;
        float hpImpact = skillConfig.HP * hpWeight;
        float rangeImpact = (skillConfig.Range.Max - skillConfig.Range.Min) * rangeWeight;
        float targetRadiusImpact = (skillConfig.TargetRadius.Max - skillConfig.TargetRadius.Min) * targetRadiusWeight;

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
            Value = skillConfiguration.Name,
        },
        new Dropdown
        {
            SelectedItem = skillConfiguration.SkillKind.Map(x => (int)x),
            OnChanged = (index) =>
            {
                var newSkillKind = (SkillKinds)index;
                skillConfiguration.SkillKindConfiguration = newSkillKind switch
                {
                    SkillKinds.DealDamage1 => DealDamage.Init(),
                    SkillKinds.DealDamage2 => DealDamage.Init(),
                    SkillKinds.DealDamage3 => DealDamage.Init(),
                    // SkillKinds.Attract => throw new NotImplementedException(),
                    // SkillKinds.Repel => throw new NotImplementedException(),
                    // SkillKinds.Activate => throw new NotImplementedException(),
                    // SkillKinds.Move => throw new NotImplementedException(),
                    // SkillKinds.ApplyDebuff => throw new NotImplementedException(),
                    // SkillKinds.ApplyBuff => throw new NotImplementedException(),
                    // SkillKinds.CreateSurface => throw new NotImplementedException(),
                    // SkillKinds.ConsumeSurface => throw new NotImplementedException(),
                    _ => DealDamage.Init(),
                };
                skillConfiguration.SkillKind.Value = newSkillKind;

            },
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
                            SkillConfigurationTabs.Scope => "#ff0000",
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
                    {SkillKinds.DealDamage1, () => new DealDamage() {
                        SkillKindConfiguration = skillConfiguration.SkillKindConfiguration,

                    }},
                    {SkillKinds.DealDamage2, () => new DealDamage() {
                        SkillKindConfiguration = skillConfiguration.SkillKindConfiguration,
                        Strength = 2,
                    }},
                    {SkillKinds.DealDamage3, () => new DealDamage() {
                        SkillKindConfiguration = skillConfiguration.SkillKindConfiguration,
                        Strength = 3,
                    }},
                    { SkillKinds.Attract, () => {
                        return new Label(){ Text = "Attract" };
                    }},
                })},
                {SkillConfigurationTabs.Scope, () => NodeBuilder.CreateNode(new VBoxContainer(),
                    new Label()
                    {
                        Text = "Range"
                    },
                    new NumericRangeInput(skillConfiguration.Range, 1, 10),
                    new Label()
                    {
                        Text = "Target Radius"
                    },
                    new NumericRangeInput(skillConfiguration.TargetRadius, 1, 5),
                    new Label()
                    {
                        Text = "Visibility Required"
                    },
                    new CallbackCheckBox() {
                        ButtonPressed = skillConfiguration.Visibility,
                        Callback = (value) =>
                        {
                            skillConfiguration.Visibility.Value = value;
                        }
                    }
                )},
                {SkillConfigurationTabs.Constraints, () => NodeBuilder.CreateNode(new VBoxContainer(),
                    new Label { Text = "AP:", SizeFlagsHorizontal = Control.SizeFlags.ExpandFill },
                    new NumericInput(skillConfiguration.AP, new(0), new(10))
                    , NodeBuilder.CreateNode(new HBoxContainer(),
                        new Label { Text = "MP:", SizeFlagsHorizontal = Control.SizeFlags.ExpandFill },
                        new NumericInput(skillConfiguration.MP, new(0), new(10))
                    ), NodeBuilder.CreateNode(new HBoxContainer(),
                        new Label { Text = "HP:", SizeFlagsHorizontal = Control.SizeFlags.ExpandFill },
                        new NumericInput(skillConfiguration.HP, new(0), new(9))
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

public partial class DealDamage : BuilderComponent
{
    public SkillKindConfiguration SkillKindConfiguration;
    public int Strength = 1;

    public static SkillKindConfiguration Init()
    {
        return new(new()
        {
            { "damage", (new ReactiveState<int>(1), new ReactiveState<int>(2)) },
            { "element", new ReactiveState<Element>(Element.Fire)}
        });
    }
    public Node Build()
    {
        var damageRange = ((ReactiveState<int>, ReactiveState<int>))SkillKindConfiguration.Value["damage"];
        var element = (ReactiveState<Element>)SkillKindConfiguration.Value["element"];
        return NodeBuilder.CreateNode(new VBoxContainer(),
            new Label() { Text = "Damage Range" },
            new NumericRangeInput(damageRange, 1, 5 * Strength),
            new Label() { Text = "Element" },
            new Dropdown()
            {
                SelectedItem = element.Map(x => (int)x),
                OnChanged = (index) =>
                {
                    element.Value = (Element)index;
                },
                Items = Enum.GetValues(typeof(Element))
                .Cast<Element>()
                .Select(x => x.GetEnumDescription())
                .ToArray(),
            }
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
    public Node Build(Storage store)
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
                Content = skillConfiguration.Name,
                FontSize = 20,
                AutowrapMode = TextServer.AutowrapMode.Off,
            },

            new HSeparator(),
            new StabilityBar(skillConfiguration.Stability)
            {
                CustomMinimumSize = new Vector2(500, 20)
            },
            new DynamicLabel
            {
                Content = skillConfiguration.SkillKind.Map(x => x.GetEnumDescription()),
            },
            NodeBuilder.Match(skillConfiguration.SkillKind, new VBoxContainer(), new()
            {
                { SkillKinds.DealDamage1, () => {
                    var damage = ((ReactiveState<int>, ReactiveState<int>))skillConfiguration.SkillKindConfiguration.Value["damage"];
                    var element = (ReactiveState<Element>)skillConfiguration.SkillKindConfiguration.Value["element"];
                    return NodeBuilder.CreateNode(new VBoxContainer(),

                        NodeBuilder.Watch(new HBoxContainer(), () => {
                            return new RichTextLabel() {
                                BbcodeEnabled = true, FitContent = true, AutowrapMode = TextServer.AutowrapMode.Off, Text = Translation.T("damage", new() {
                                { "min", damage.Item1 },
                                { "max", damage.Item2 },
                                { "element", element.Value.GetEnumDescription().ToLower() },
                                { "elementColor", SCTheme.GetElementColor(element).ToHtml() }
                            }) };
                        }, damage.Item1, damage.Item2, element.Map(x => (int)x))
                    );
                } }
            }),

            NodeBuilder.Watch(new HBoxContainer(), () =>
            {
                return new Label()
                {
                    Text = Translation.T("range", new() {
                        { "min", skillConfiguration.Range.Min },
                        { "max", skillConfiguration.Range.Max }
                    })
                };
            }, skillConfiguration.Range.Min, skillConfiguration.Range.Max),
            NodeBuilder.Watch(new HBoxContainer(), () =>
            {
                return new Label()
                {
                    Text = Translation.T("targetRadius", new() {
                        { "min", skillConfiguration.TargetRadius.Min },
                        { "max", skillConfiguration.TargetRadius.Max }
                    })
                };
            }, skillConfiguration.TargetRadius.Min, skillConfiguration.TargetRadius.Max),
            new DynamicLabel
            {
                Content = skillConfiguration.Visibility.Map(x => x ? "Requires visibility" : "Doesn't require visibility"),
            },
            NodeBuilder.CreateNode(new HBoxContainer(),
                NodeBuilder.Show(skillConfiguration.AP.Map(x => x > 0), new DynamicLabel()
                {
                    AutowrapMode = TextServer.AutowrapMode.Off,
                    Content = skillConfiguration.AP.Map(x => $"[color={SCTheme.Ability.ToHtml()}][img=40x40]res://images/skin/icon-star.svg[/img][b]" + x + "[/b][/color]"),
                }),
                NodeBuilder.Show(skillConfiguration.MP.Map(x => x > 0), new DynamicLabel()
                {
                    AutowrapMode = TextServer.AutowrapMode.Off,
                    Content = skillConfiguration.MP.Map(x => $"[color={SCTheme.Movement.ToHtml()}][img=40x40]res://images/skin/icon-boot.svg[/img][b]" + x + "[/b][/color]"),
                }),
                NodeBuilder.Show(skillConfiguration.HP.Map(x => x > 0), new DynamicLabel()
                {
                    AutowrapMode = TextServer.AutowrapMode.Off,
                    Content = skillConfiguration.HP.Map(x => $"[color={SCTheme.Health.ToHtml()}][img=40x40]res://images/skin/icon-heart.svg[/img][b]" + (x * 10) + "%" + "[/b][/color]"),
                })
            ),
            new CallbackButton()
            {
                Callback = () =>
                {
                    var gameData = store.GameData;
                    var skills = gameData.skills.ToList();
                    skills.Add(new SkillBlueprint()
                    {
                        Name = skillConfiguration.Name,
                        AP = skillConfiguration.AP,
                        HP = skillConfiguration.HP,
                        MP = skillConfiguration.MP,
                        Cooldown = skillConfiguration.Cooldown,
                        Range = new Range()
                        {
                            Min = skillConfiguration.Range.Min,
                            Max = skillConfiguration.Range.Max
                        },
                        TargetRadius = new Range()
                        {
                            Min = skillConfiguration.TargetRadius.Min,
                            Max = skillConfiguration.TargetRadius.Max
                        },
                        SkillKind = skillConfiguration.SkillKind,
                        SkillKindConfiguration = skillConfiguration.SkillKindConfiguration,
                        Stability = skillConfiguration.Stability,
                        Visibility = skillConfiguration.Visibility,
                    });
                    gameData.skills = skills.ToArray();
                    store.GameData = gameData;
                },
                Text = "Craft"
            }
        );
    }
}

