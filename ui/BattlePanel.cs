using Godot;

using System;
using System.Collections.Generic;
using System.Linq;

using TurnBasedSkills = System.Collections.Generic.List<(TurnSkillRestrictions, SkillBlueprint)?>;



public partial class BattlePanel : BuilderComponent
{
    private ReactiveState<List<string>> items = new(new List<string>());
    // private ReactiveState<TurnBasedSkills> skills = new(new List<(TurnSkillRestrictions, SkillBlueprint)?>());
    private Action<int> onPressSkill;
    public BattlePanel(Action<int> onPressSkill)
    {
        this.Key = 0;
        this.onPressSkill = onPressSkill;

        // this.skills.Value = skills;
        // items.Value = Enumerable.Range(0, 10).Select(i =>
        // {
        //     if (skills.Count > i && skills[i].HasValue)
        //     {
        //         return skills[i].Value.Item2.name;
        //     }
        //     else
        //     {
        //         return "Item " + i;
        //     }
        // }).ToList();
    }

    public Node Build(SkillSlotsState skillSlotsState)
    {
        var If = NodeBuilder.Show;
        // var M = NodeBuilder.Map;
        // var image = Image.LoadFromFile("res://resources/400x400.png");
        var border = 4;

        var marginContainer =
        NodeBuilder.CreateNode(
            new CanvasLayer
            {
                Name = GetType().Name,
                Offset = new Vector2(591, 324),
                Transform = new Transform2D(1, 0, 0, 1, 591, 324),
            },
            NodeBuilder.CreateNode(
                new VBoxContainer
                {
                    AnchorLeft = 1.0f,
                    AnchorRight = 1.0f,
                    OffsetLeft = -791.0f,
                    OffsetTop = -324.0f,
                    OffsetRight = -591.0f,
                    OffsetBottom = 324.0f,
                    GrowHorizontal = Control.GrowDirection.Begin,
                    GrowVertical = Control.GrowDirection.Both,
                    CustomMinimumSize = new(SCTheme.GridItemSize * 2 - 40, 0),
                },
                NodeBuilder.CreateNode(
                    new ColorRect()
                    {
                        Color = SCTheme.Base100,
                        SizeFlagsVertical = Control.SizeFlags.ExpandFill,
                        CustomMinimumSize = new(SCTheme.GridItemSize * 2 - 40, SCTheme.GridItemSize * 2),
                    },
                    NodeBuilder.CreateNode(
                        new VBoxContainer
                        {
                            SizeFlagsVertical = Control.SizeFlags.ExpandFill,
                        },
                        NodeBuilder.CreateNode(
                            new ColorRect
                            {
                                Color = SCTheme.Health,
                                SizeFlagsVertical = Control.SizeFlags.ExpandFill,
                                CustomMinimumSize = new Vector2(SCTheme.GridItemSize * 2 - 40, SCTheme.GridItemSize - 28),

                            },
                            new Label()
                            {
                                Text = "XX/XX HP",
                                SizeFlagsVertical = Control.SizeFlags.ExpandFill,
                                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                            }
                        ),
                        NodeBuilder.CreateNode(
                            new HBoxContainer
                            {
                                // SizeFlagsVertical = Control.SizeFlags.ExpandFill,
                            },
                            NodeBuilder.CreateNode(
                                new ColorRect
                                {
                                    Color = SCTheme.Ability,
                                    SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                                    CustomMinimumSize = new Vector2(SCTheme.GridItemSize, SCTheme.GridItemSize - 28),
                                },
                                new Label()
                                {
                                    Text = "X AP",
                                }
                            ),
                            NodeBuilder.CreateNode(
                                new ColorRect
                                {
                                    Color = SCTheme.Movement,
                                    SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                                    CustomMinimumSize = new Vector2(SCTheme.GridItemSize, SCTheme.GridItemSize - 28),
                                },
                                new Label()
                                {
                                    Text = "X MP",
                                }
                            )
                        )
                    ),
                    NodeBuilder.Map(() =>
                    {
                        var gridContainer = new GridContainer()
                        {
                            Columns = 2,
                            OffsetTop = SCTheme.GridItemSize * 2 - 48,
                            OffsetRight = SCTheme.GridItemSize - 20,
                            OffsetBottom = 648,
                        };
                        gridContainer.AddThemeConstantOverride("h_separation", border);
                        gridContainer.AddThemeConstantOverride("v_separation", border);

                        return gridContainer;
                    }, skillSlotsState.equipedSkills, (item, i) => new ItemComponent
                    {
                        Key = i,
                        onClick = () => onPressSkill(i),
                        onLongPress = () => { },
                        Child = new Label()
                        {
                            Text = item.Item2 != null ? item.Item2.Name : "Empty",
                        }
                    })
                )
            )
        );
        return marginContainer;
    }
}
