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
        Key = 0;
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

    public Node Build(SkillSlotsState skillSlotsState, TreeService treeService)
    {
        var If = NodeBuilder.Show;
        var border = 4;
        ReactiveState<int> selectedSkill = -1;

        var marginContainer =
        NodeBuilder.CreateNode(
            new CanvasLayer
            {
                Name = GetType().Name,
                Transform = new Transform2D(1, 0, 0, 1, 591, 324),
            },
            NodeBuilder.Watch<int>(new CanvasLayer()
            {
                Transform = new Transform2D(1, 0, 0, 1, 591, 324),
            }, () =>
            {
                if (selectedSkill == -1 || skillSlotsState.equipedSkills.Value[selectedSkill].Item2 == null)
                {
                    return new Node(); 
                }
                var node = treeService.GetNodeRecursively<Button>("ItemComponent" + selectedSkill.Value);

                var currentSkill = skillSlotsState.equipedSkills.Value[selectedSkill].Item2;
                return NodeBuilder.CreateNode(new BoxContainer(), new SkillTooltip()
                {
                    SkillConfiguration = new SkillConfiguration()
                    {
                        Name = currentSkill.Name,
                        Range = new(currentSkill.Range.Min, currentSkill.Range.Max),
                        TargetRadius = new(currentSkill.TargetRadius.Min, currentSkill.TargetRadius.Max),
                        AP = currentSkill.AP,
                        MP = currentSkill.MP,
                        HP = currentSkill.HP,
                        Stability = currentSkill.Stability,
                        SkillEffect = currentSkill.SkillEffect,
                        SkillEffectConfiguration = currentSkill.SkillEffectConfiguration,
                        Visibility = currentSkill.Visibility,
                        Cooldown = currentSkill.Cooldown,
                    }
                });
            }, new ReactiveState<int>[]{ selectedSkill }),
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
                    }, skillSlotsState.equipedSkills, (item, i) =>
                    {
                        var itemComponent = new ItemComponent
                        {
                            Key = i,
                            OnClick = () => onPressSkill(i),
                            onLongPress = () =>
                            {
                                selectedSkill.Value = i;
                            },
                            Child = new Label()
                            {
                                Text = item.Item2 != null ? item.Item2.Name : "Empty",
                            }
                        };
                        return itemComponent;
                    })
                )
            )
        );
        return marginContainer;
    }
}
