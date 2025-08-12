using System;
using System.Linq;
using Godot;

public partial class Dropdown : OptionButton
{
    private string[] _items;

    public ReactiveState<int> SelectedItem
    {
        get => Selected;
        set
        {
            // Selected = value;
            GD.Print("set " + value.Value);
            Select(value);

        }
    }
    public Action<int> OnChanged;

    public Dropdown(string[] items)
    {
        _items = items;
        foreach (var item in _items)
        {
            AddItem(item);
        }
        ItemSelected += (index) =>
        {
            OnChanged((int)index);

        };
    }

}

public partial class TextInput : LineEdit
{
    public ReactiveState<string> Value;

    public TextInput()
    {
        TextChanged += (newText) =>
        {
            if (Value != null)
            {
                Value.Value = newText;
            }
        };
    }
}

public partial class DynamicLabel : RichTextLabel
{
    public DynamicLabel()
    {
        BbcodeEnabled = true;
        FitContent = true;
    }

    public override void _Ready()
    {
        base._Ready();
        InstallEffect(new ThemeCode());
    }
    public int FontSize
    {
        get => GetThemeFontSize("normal_font_size");
        set
        {
            AddThemeFontSizeOverride("normal_font_size", value);
        }
    }

    private ReactiveState<string> _content = new();

    public ReactiveState<string> Content
    {
        get => _content;
        set
        {
            _content = value;

            if (_content != null)
            {
                Text = _content;
            }
            _content.OnValueChanged += (newValue) =>
            {
                Text = newValue;
            };
        }
    }

}

public partial class StabilityBar : BuilderComponent
{
    private ReactiveState<(int, int, int)> _stability;
    public Vector2 CustomMinimumSize;

    public StabilityBar(ReactiveState<(int, int, int)> stability)
    {
        _stability = stability;
    }

    private int[] TupleToArray()
    {
        return new int[] {
            _stability.Value.Item1,
            _stability.Value.Item2,
            _stability.Value.Item3,
            100
        };
    }

    public Node Build()
    {
        var container = new HBoxContainer
        {
            Name = Name,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ShrinkCenter,
            CustomMinimumSize = CustomMinimumSize,
        };

        var redBar = CreateBar(SCTheme.Error, 0);
        var orangeBar = CreateBar(SCTheme.Warning, 1);
        var yellowBar = CreateBar(SCTheme.Success, 2);
        var greenBar = CreateBar(SCTheme.Primary, 3);

        _stability.OnValueChanged += (newStability) =>
        {
            redBar.SizeFlagsStretchRatio = GetValue(0) / 10;
            orangeBar.SizeFlagsStretchRatio = GetValue(1) / 10;
            yellowBar.SizeFlagsStretchRatio = GetValue(2) / 10;
            greenBar.SizeFlagsStretchRatio = GetValue(3) / 10;
        };

        return NodeBuilder.CreateNode(container,
            redBar, orangeBar, yellowBar
            , greenBar
        );
    }

    private float GetWidth(int index, ReactiveState<(int, int, int)> stability)
    {
        var widths = TupleToArray();

        if (index == 0)
        {
            return CustomMinimumSize.X * widths[index] / 100;
        }

        return CustomMinimumSize.X * (widths[index] - widths
            .Take(index)
            .Sum()) / 100;
    }


    private float GetValue(int index)
    {
        var widths = TupleToArray();

        if (index == 0)
        {
            return widths[index];
        }
        else if (index == 3)
        {
            return 100 - widths[2];
        }

        return widths[index] - widths
            .Take(index)
            .Sum();
    }

    private ColorRect CreateBar(Color colorHex, int index)
    {
        return new ColorRect
        {

            Color = colorHex,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsStretchRatio = GetValue(index) / 10,
            SizeFlagsVertical = Control.SizeFlags.Fill,
        };
    }
}



public partial class NumericRangeInput : BuilderComponent
{
    private ReactiveState<int> _minValue;
    private ReactiveState<int> _maxValue;
    private ReactiveState<int> _minLimit = new();
    private ReactiveState<int> _maxLimit = new();
    public NumericRangeInput((ReactiveState<int>, ReactiveState<int>) value, int min, int max)
    {
        _minValue = value.Item1;
        _maxValue = value.Item2;
        _minLimit.Value = min;
        _maxLimit.Value = max;
    }

    public Node Build()
    {
        var container = new HBoxContainer
        {
            Name = Name,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill,
        };

        var minInput = new NumericInput(_minValue, _minLimit, _maxValue)
        {
            CustomMinimumSize = new Vector2(100, 50),
        };

        var maxInput = new NumericInput(_maxValue, _minValue, _maxLimit, reversed: true)
        {
            CustomMinimumSize = new Vector2(100, 50),
        };

        return NodeBuilder.CreateNode(container,
            minInput, maxInput
        );
    }
}

public partial class NumericInput : BuilderComponent
{
    private ReactiveState<int> _value;
    private ReactiveState<int> _min;
    private ReactiveState<int> _max;
    private bool _reversed;
    public Vector2 CustomMinimumSize { get; set; } = new Vector2(100, 50);

    public NumericInput(ReactiveState<int> value, ReactiveState<int> min, ReactiveState<int> max, bool reversed = false)
    {
        _value = value;
        _min = min;
        _max = max;
        _reversed = reversed;
    }

    private void HandleChange(int newValue)
    {
        if (newValue < _min || newValue > _max)
        {
            return;
        }
        _value.Value = newValue;
    }

    public Node Build()
    {
        var container = new VBoxContainer
        {
            Name = Name,
            CustomMinimumSize = new Vector2(100, 50),
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill,
        };

        var valueLabel = new DynamicLabel()
        {
            Content = _value.Map(value => value.ToString()),
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
        };

        var buttonContainer = new HBoxContainer
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
        };

        if (_reversed)
        {
            buttonContainer.AddChild(CreateDecrementButton());
            buttonContainer.AddChild(CreateIncrementButton());
        }
        else
        {
            buttonContainer.AddChild(CreateIncrementButton());
            buttonContainer.AddChild(CreateDecrementButton());
        }

        return NodeBuilder.CreateNode(container,
            valueLabel, buttonContainer
        );
    }

    private Button CreateIncrementButton()
    {
        var incrementButton = new Button
        {
            Text = "+",
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
        };

        incrementButton.Pressed += () =>
        {
            HandleChange(_value + 1);
        };

        return incrementButton;
    }

    private Button CreateDecrementButton()
    {
        var decrementButton = new Button
        {
            Text = "-",
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
        };

        decrementButton.Pressed += () =>
        {
            HandleChange(_value - 1);
        };

        return decrementButton;
    }
}

public partial class CallbackButton : Button
{
    public Action Callback
    {
        get => null;
        set
        {
            Pressed += value;
        }
    }

}

public partial class CallbackCheckBox : CheckBox
{
    public Action<bool> Callback
    {
        get => null;
        set
        {
            Pressed += () =>
            {
                value(ButtonPressed);
            };
        }
    }

}

public partial class Icon : Sprite2D
{
    public Icon()
    {
        Scale = new(0.05f, 0.05f);
    }

    public string Kind
    {
        get => null;
        set
        {
            Texture = GD.Load<Texture2D>($"res://images/skin/icon-{value}.svg");
        }
    }
}

public class Gauge : BuilderComponent
{
    public ReactiveState<float> Value;
    public Vector2 CustomMinimumSize;
    public Color Color;

    public Gauge(ReactiveState<float> value, ReactiveState<Color> color)
    {
        Value = value;
        Color = color;
    }

    public Node Build()
    {
        var container = new Control
        {
            Name = Name,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ShrinkCenter,
            CustomMinimumSize = CustomMinimumSize,
        };

        var background = new ColorRect
        {
            Color = new Color(0.2f, 0.2f, 0.2f),
            CustomMinimumSize = container.CustomMinimumSize
        };

        var emptyBar = new Control()
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.Fill,
            SizeFlagsStretchRatio = Mathf.Clamp(10 - Value.Value, 0, 1),
        };

        var foreground = new ColorRect
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.Fill,
            SizeFlagsStretchRatio = Mathf.Clamp(Value.Value, 0, 1),
            Color = Color
        };
        var foregroundWrapper = NodeBuilder.CreateNode(new HBoxContainer()
        {
            CustomMinimumSize = container.CustomMinimumSize,
        }, foreground, emptyBar
        );

        Value.OnValueChanged += newValue =>
        {
            foreground.SizeFlagsStretchRatio = Mathf.Clamp(newValue, 0, 1);
            foreground.SizeFlagsStretchRatio = Mathf.Clamp(10 - Value.Value, 0, 1);
        };

        return NodeBuilder.CreateNode(container, background, foregroundWrapper);
    }
}

public partial class VerticalContainer : BuilderComponent
{
    public int Separation = 4;
    public Node Build()
    {
        var container = new VBoxContainer()
        {
            Name = Name,
        };
        container.AddThemeConstantOverride("separation", Separation);
        return container;
    }
}

public partial class SkillTooltip : BuilderComponent
{
    public SkillConfiguration SkillConfiguration;
    public bool IsConfigurable = false;
    public Node Build()
    {
        return NodeBuilder.CreateNode(new VBoxContainer
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill,
        },
            IsConfigurable ? new TextInput
            {
                PlaceholderText = "Skill Name",
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                Value = SkillConfiguration.Name,
            } : new DynamicLabel
            {
                Content = SkillConfiguration.Name,
                FontSize = 20,
                AutowrapMode = TextServer.AutowrapMode.Off,
            },

            new HSeparator(),
            new StabilityBar(SkillConfiguration.Stability)
            {
                CustomMinimumSize = new Vector2(500, 20)
            },
            new DynamicLabel
            {
                Content = SkillConfiguration.SkillEffect.Map(x => x.GetEnumDescription()),
            },
            NodeBuilder.Match(SkillConfiguration.SkillEffect, new VBoxContainer(), new()
            {
                { SkillEffects.DealDamage1, () => {
                    var damage = ((ReactiveState<int>, ReactiveState<int>))SkillConfiguration.SkillEffectConfiguration.Value["damage"];
                    var element = (ReactiveState<Element>)SkillConfiguration.SkillEffectConfiguration.Value["element"];
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
                } },
                {
                    SkillEffects.CreateSurface, () => {
                    var element = (ReactiveState<Element>)SkillConfiguration.SkillEffectConfiguration.Value["element"];
                    return NodeBuilder.CreateNode(new VBoxContainer(),

                        NodeBuilder.Watch(new HBoxContainer(), () => {
                            return new RichTextLabel() {
                                BbcodeEnabled = true, FitContent = true, AutowrapMode = TextServer.AutowrapMode.Off, Text = Translation.T("createSurface", new() {
                                { "element", element.Value.GetEnumDescription().ToLower() },
                                { "elementColor", SCTheme.GetElementColor(element).ToHtml() }
                            }) };
                        }, element.Map(x => (int)x))
                    );
                } },
            }),

            NodeBuilder.Watch(new HBoxContainer(), () =>
            {
                return new DynamicLabel()
                {
                    AutowrapMode = TextServer.AutowrapMode.Off,
                    Text = Translation.T("range", new() {
                        { "min", SkillConfiguration.Range.Min },
                        { "max", SkillConfiguration.Range.Max }
                    })
                };
            }, SkillConfiguration.Range.Min, SkillConfiguration.Range.Max),
            NodeBuilder.Watch(new HBoxContainer(), () =>
            {
                return new DynamicLabel()
                {
                    AutowrapMode = TextServer.AutowrapMode.Off,
                    Text = Translation.T("targetRadius", new() {
                        { "min", SkillConfiguration.TargetRadius.Min },
                        { "max", SkillConfiguration.TargetRadius.Max }
                    })
                };
            }, SkillConfiguration.TargetRadius.Min, SkillConfiguration.TargetRadius.Max),
            new DynamicLabel
            {
                Content = SkillConfiguration.Visibility.Map(x => "[theme classes=text-content-60]" + (x ? "Requires visibility" : "Doesn't require visibility") + "[/theme]"),
            },
            NodeBuilder.CreateNode(new HBoxContainer(),
                NodeBuilder.Show(SkillConfiguration.AP.Map(x => x > 0), new DynamicLabel()
                {
                    AutowrapMode = TextServer.AutowrapMode.Off,
                    Content = SkillConfiguration.AP.Map(x => "[theme classes=text-ability][img=40x40]res://images/skin/icon-star.svg[/img][b]" + x + "[/b][/theme]"),
                }),
                NodeBuilder.Show(SkillConfiguration.MP.Map(x => x > 0), new DynamicLabel()
                {
                    AutowrapMode = TextServer.AutowrapMode.Off,
                    Content = SkillConfiguration.MP.Map(x => "[theme classes=text-movement][img=40x40]res://images/skin/icon-boot.svg[/img][b]" + x + "[/b][/theme]"),
                }),
                NodeBuilder.Show(SkillConfiguration.HP.Map(x => x > 0), new DynamicLabel()
                {
                    AutowrapMode = TextServer.AutowrapMode.Off,
                    Content = SkillConfiguration.HP.Map(x => "[theme classes=text-health][img=40x40]res://images/skin/icon-heart.svg[/img][b]" + (x * 10) + "%" + "[/b][/theme]"),
                })
            )
        );
    }
}