using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Godot;

using SkillEffectConfiguration = ReactiveState<System.Collections.Generic.Dictionary<string, object>>;

public static class SkillEffectHelper
{
    public static SkillEffectConfiguration FromSkillEffect(SkillEffects skillEffect)
    {
        return skillEffect switch
        {
            SkillEffects.DealDamage1 => DealDamage.Init(),
            SkillEffects.DealDamage2 => DealDamage.Init(),
            SkillEffects.DealDamage3 => DealDamage.Init(),
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
    }

    public static SkillEffectConfiguration RandomizeFromSkillEffect(SkillEffects skillEffect)
    {
        return skillEffect switch
        {
            SkillEffects.DealDamage1 => DealDamage.Randomize(),
            SkillEffects.DealDamage2 => DealDamage.Randomize(),
            SkillEffects.DealDamage3 => DealDamage.Randomize(),
            SkillEffects.CreateSurface => CreateSurface.Randomize(),
            // SkillKinds.Attract => throw new NotImplementedException(),
            // SkillKinds.Repel => throw new NotImplementedException(),
            // SkillKinds.Activate => throw new NotImplementedException(),
            // SkillKinds.Move => throw new NotImplementedException(),
            // SkillKinds.ApplyDebuff => throw new NotImplementedException(),
            // SkillKinds.ApplyBuff => throw new NotImplementedException(),
            // SkillKinds.CreateSurface => throw new NotImplementedException(),
            // SkillKinds.ConsumeSurface => throw new NotImplementedException(),
            _ => DealDamage.Randomize(),
        };
    }
}

public enum SkillKinds
{
    [Description("Active")]
    Active,
    [Description("Passive")]
    Passive,
    [Description("Reflexive")]
    Reflexive,
    [Description("Weapon")]
    Weapon,
}

public enum SkillEffects
{
    // Active
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

    // Passive
    [Description("Absorb")]
    Absorb,
    [Description("WhenOnSurfaceCostLess")]
    WhenOnSurfaceCostLess,

    // Reflexive
    [Description("OnGettingHit")]
    OnGettingHit,
    [Description("OnHit")]
    OnHit,
    [Description("OnEnemyGettingClose")]
    OnEnemyGettingClose,

    // Weapon
    [Description("DealWeaponDamage")]
    DealWeaponDamage,
}


public partial class SkillConfiguration
{
    public ReactiveState<SkillKinds> SkillKind = SkillKinds.Active;
    public ReactiveState<string> Name = "New Skill";
    public ReactiveState<SkillEffects> SkillEffect = SkillEffects.DealDamage1;
    public SkillEffectConfiguration SkillEffectConfiguration = DealDamage.Init();
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
        new Dropdown(Enum.GetValues(typeof(SkillEffects))
                .Cast<SkillEffects>()
                .Select(x => x.GetEnumDescription())
                .ToArray())
        {
            SelectedItem = skillConfiguration.SkillEffect.Map(x => (int)x),
            OnChanged = (index) =>
            {
                var newSkillEffect = (SkillEffects)index;
                skillConfiguration.SkillEffectConfiguration = SkillEffectHelper.FromSkillEffect(newSkillEffect);
                skillConfiguration.SkillEffect.Value = newSkillEffect;

            },
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
                {SkillConfigurationTabs.Spec, () => NodeBuilder.Match(skillConfiguration.SkillEffect, new VBoxContainer(), new() {
                    {SkillEffects.DealDamage1, () => new DealDamage() {
                        SkillKindConfiguration = skillConfiguration.SkillEffectConfiguration,

                    }},
                    {SkillEffects.DealDamage2, () => new DealDamage() {
                        SkillKindConfiguration = skillConfiguration.SkillEffectConfiguration,
                        Strength = 2,
                    }},
                    {SkillEffects.DealDamage3, () => new DealDamage() {
                        SkillKindConfiguration = skillConfiguration.SkillEffectConfiguration,
                        Strength = 3,
                    }},
                    {SkillEffects.CreateSurface, () => new CreateSurface() {
                        SkillKindConfiguration = skillConfiguration.SkillEffectConfiguration,
                    }},
                    { SkillEffects.Attract, () => {
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
    public SkillEffectConfiguration SkillKindConfiguration;
    public int Strength = 1;

    public static SkillEffectConfiguration Init()
    {
        return new(new()
        {
            { "damage", (new ReactiveState<int>(1), new ReactiveState<int>(2)) },
            { "element", new ReactiveState<Element>(Element.Fire)}
        });
    }

    public static SkillEffectConfiguration Randomize()
    {
        var min = Random.Int(1, 5);
        return new(new()
        {
            { "damage", (new ReactiveState<int>(min), new ReactiveState<int>(Random.Int(min, 10))) },
            { "element", new ReactiveState<Element>(Random.Enum<Element>())}
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
            new Dropdown(Enum.GetValues(typeof(Element))
                .Cast<Element>()
                .Select(x => x.GetEnumDescription())
                .ToArray())
            {
                OnChanged = (index) =>
                {
                    element.Value = (Element)index;
                },
                SelectedItem = element.Map(x => (int)x),
            }
        );
    }
}

public partial class CreateSurface : BuilderComponent
{
    public SkillEffectConfiguration SkillKindConfiguration;
    public int Strength = 1;

    public static SkillEffectConfiguration Init()
    {
        return new(new()
        {
            { "element", new ReactiveState<Element>(Element.Fire)}
        });
    }

    public static SkillEffectConfiguration Randomize()
    {
        var possibleSurfaces = new Element[] { Element.Water, Element.Fire, Element.Air, Element.Earth };
        return new(new()
        {
            { "element", new ReactiveState<Element>(possibleSurfaces[Random.Int(0, possibleSurfaces.Length - 1)])}
        });
    }

    public Node Build()
    {
        var element = (ReactiveState<Element>)SkillKindConfiguration.Value["element"];

        return NodeBuilder.CreateNode(new VBoxContainer(),
            new Label() { Text = "Element" },
            new Dropdown(Enum.GetValues(typeof(Element))
                .Cast<Element>()
                .Select(x => x.GetEnumDescription())
                .ToArray())
            {
                OnChanged = (index) =>
                {
                    element.Value = (Element)index;
                },
                SelectedItem = element.Map(x => (int)x),
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
        */
        var image = GD.Load<Texture2D>($"res://images/skin/icon-sword.svg");
        return NodeBuilder.CreateNode(new VBoxContainer
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill,
        },
        NodeBuilder.CreateNode(() =>
            {
                var gridContainer = new GridContainer()
                {
                    Columns = 2,
                    OffsetTop = SCTheme.GridItemSize * 2 - 48,
                    OffsetRight = SCTheme.GridItemSize - 20,
                    OffsetBottom = 648,
                };
                // gridContainer.AddThemeConstantOverride("h_separation", border);
                // gridContainer.AddThemeConstantOverride("v_separation", border);

                return gridContainer;
            }, new ItemComponent()
            {
                BackgroundImage = image
            }),
            new SkillTooltip()
            {
                SkillConfiguration = skillConfiguration
            },
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
                        SkillEffect = skillConfiguration.SkillEffect,
                        SkillEffectConfiguration = skillConfiguration.SkillEffectConfiguration,
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

