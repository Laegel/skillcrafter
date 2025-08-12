using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Godot;
using NUnit.Framework.Internal;

using SkillEffectConfiguration = ReactiveState<System.Collections.Generic.Dictionary<string, object>>;
/*
The rarer, the lesser constraints it gets
White
- may modify damage element and power
- big constraints
    - no visibility modifier
    - no range modifier
    - no target radius modifier
- basic skill effects
    - passives with big conditions (e.g. 10% to reduce cost by half when standing on a given surface)
- skills power (up to 60%)
Green
- smaller constraints
    - no visibility modifier
    - no range modifier
    - no target radius modifier
- skills power (up to 70%)
Blue
- skills power (up to 80%)
Purple
- skills power (up to 90%)
Gold
- less contraints, very powerful skill effects
- skills power (up to 100%)
Red
- access to the designer?
Rainbow (if need be)
*/

public class Parchment : StorageItem
{
    public Rarity Rarity;
    public List<SkillKinds> PossibleSkillKinds;
    public bool CanChangeStrength;
    public bool CanChangeRange;
    public bool CanChangeTargetRadius;
    public bool CanChangeVisibility;
    public bool CanChangeElement;

    public bool CanReduceCost;
    public SkillConfiguration SkillConfiguration;

    public static Parchment Randomize()
    {
        var isSelfTargeting = Random.Boolean();
        var rarity = Random.Enum<Rarity>();
        // var skillEffect = Random.Enum<SkillEffects>();
        var possibleSkillEffects = new SkillEffects[] { SkillEffects.DealDamage1, SkillEffects.CreateSurface };

        var skillEffect = possibleSkillEffects[Random.Int(0, possibleSkillEffects.Length - 1)];
        return new Parchment()
        {
            Rarity = rarity,
            CanChangeRange = Random.Boolean(),
            CanChangeStrength = Random.Boolean(),
            CanChangeTargetRadius = Random.Boolean(),
            CanChangeVisibility = Random.Boolean(),
            CanChangeElement = Random.Boolean(),
            CanReduceCost = Random.Boolean(),
            PossibleSkillKinds = new()
            { { SkillKinds.Active } },
            SkillConfiguration = new SkillConfiguration()
            {
                Range = isSelfTargeting ? (0, 0) : Random.Range(2, 10).ToTuple(),
                TargetRadius = isSelfTargeting ? (2, Random.Int(2, 10)) : Random.Range(2, 5).ToTuple(),
                Visibility = (int)rarity <= 2 || Random.Boolean(),
                AP = Random.Int(1, 5),
                SkillEffect = skillEffect,
                SkillEffectConfiguration = SkillEffectHelper.RandomizeFromSkillEffect(skillEffect),
            },
        };
    }
}

public partial class ParchmentComponent : BuilderComponent
{

    public Node Build(Storage store, ParchmentState parchmentState)
    {
        var parchment = parchmentState.Parchment;
        var resourceSlots = parchment.CanChangeRange.ToInt() + parchment.CanChangeTargetRadius.ToInt() + parchment.CanChangeVisibility.ToInt() + parchment.CanChangeStrength.ToInt();
        var skillConfiguration = parchment.SkillConfiguration;
        // One resource slot per modifyable entry
        var resourceSlotItems = new List<Node>(resourceSlots);
        var plusSign = ResourceLoader.Load<Texture2D>($"res://images/skin/plus.png");
        var newTexture = WithColor(plusSign, SCTheme.Base300);
        
        for (var i = 0; i < resourceSlots; ++i)
        {
            resourceSlotItems.Add(new ItemComponent()
            {
                BackgroundImage = newTexture,
                borderColor = SCTheme.Content,
            });
        }

        return NodeBuilder.CreateNode(new VBoxContainer
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill,
        },
            new SkillTooltip()
            {
                SkillConfiguration = skillConfiguration,
                IsConfigurable = true,
            }, NodeBuilder.Map(() =>
            {
                var gridContainer = new GridContainer()
                {
                    Columns = resourceSlots,
                    OffsetTop = SCTheme.GridItemSize * 2 - 48,
                    OffsetRight = SCTheme.GridItemSize - 20,
                    OffsetBottom = 648,
                };
                gridContainer.AddThemeConstantOverride("h_separation", SCTheme.GridItemBorder);
                gridContainer.AddThemeConstantOverride("v_separation", SCTheme.GridItemBorder);

                return gridContainer;
            }, resourceSlotItems),

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
                    var index = Array.IndexOf(store.GameData.Parchments, parchment);
                    gameData.Parchments = store.GameData.Parchments.Where((val, idx) => idx != index).ToArray();

                    store.GameData = gameData;
                },
                Text = "Craft"
            });
    }

    private Texture2D WithColor(Texture2D texture, Color newColor)
    {
        Image image = texture.GetImage();

        for (int y = 0; y < image.GetHeight(); y++)
        {
            for (int x = 0; x < image.GetWidth(); x++)
            {
                Color color = image.GetPixel(x, y);
                image.SetPixel(x, y, color * newColor); // Red tint
            }
        }

        return ImageTexture.CreateFromImage(image);
    }
}