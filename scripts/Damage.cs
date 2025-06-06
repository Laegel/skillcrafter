using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

public enum DamageEssence
{
    Skillshot = 0,
    OverTime,
    Aura,
    Pierce,
    Leech,
    Ground,
}

public enum ControlEssence
{
    Attract = 6,
    Repel,
    Sweep,
}

public enum ActiveEssence
{
    Skillshot,
    OverTime,
    Aura,
    Pierce,
    Leech,
    Ground,
    Attract = 6,
    Repel,
    Sweep,
}


public enum MovementEssence
{
    Dash = 9,
    Teleport,
    Speed,
    Invisibility,
}


public enum BaseElement
{
    Fire,
    Water,
    Light,
    Dark,
}

public enum MixedElement
{
    Void = 4,
    Lightning,
    Steam,
    Lava,
    Ice,
    Acid,
}

public enum Element
{
    [Description("Neutral")]
    Neutral,
    [Description("Fire")]
    Fire,
    [Description("Water")]
    Water,
    [Description("Holy")]
    Holy,
    [Description("Curse")]
    Curse,
    [Description("Radiating")]
    Radiating,
    [Description("Steam")]
    Steam,
    [Description("Ice")]
    Ice,
    [Description("Poison")]
    Poison,
    [Description("Blast")]
    Blast,
}

public enum DamageType
{
    Direct,
    OverTime,
}

[Serializable]
public struct Skill
{

    public static (float, float, float) GetElementColor(Element element)
    {
        return element switch
        {
            Element.Curse => (0.29411764705882354f, 0.0196078431372549f, 0.7450980392156863f),
            Element.Fire => (0.8980392156862745f, 0, 0),
            Element.Holy => (1, 1, 1),
            Element.Water => (0.054901960784313725f, 0.5294117647058824f, 0.8f),
            Element.Radiating => (0.9882352941176471f, 0.7529411764705882f, 0.11764705882352941f),
            Element.Steam => (0.7803921568627451f, 0.8352941176470589f, 0.8784313725490196f),
            Element.Ice => (0.8627450980392157f, 0.9529411764705882f, 1),
            Element.Poison => (0.6196078431372549f, 1, 0),
            Element.Neutral => (1, 1, 1),
            Element.Blast => (1, 0.12549019607843137f, 0.12549019607843137f),
            _ => (0, 0, 0),
        };
    }
}

public enum ScopeKind
{
    Ring,
}

// public abstract class Scope : MonoBehaviour {
//     public virtual void Start()
//     {
//         name = "scope";
//         // scope.AddComponent<ScopeClick>();
//         transform.position = Vector3.zero;
//         transform.localScale = new Vector3(1, 1, 1);
//         transform.localPosition = new Vector3(0, 0, 0.1f);

//     }

//     public bool IsRendered(Transform parent)
//     {
//         return parent.Find("scope") != null;
//     }

//     public void Remove(Transform parent)
//     {
//         Destroy(parent.Find("scope").gameObject);
//     }

//     public abstract bool IsCaptured(Vector2 position);


// }

// public class RingScope : Scope
// {
//     public int innerRadius;
//     public int outerRadius;

//     public override bool IsCaptured(Vector2 position)
//     {
//         return (position - (Vector2)transform.position).sqrMagnitude >= innerRadius * innerRadius;
//     }

//     public override void Start()
//     {
//         base.Start();

//         var outerRadiusObject = new GameObject("outerRadius");
//         outerRadiusObject.transform.SetParent(transform);
//         outerRadiusObject.transform.localPosition = Vector3.zero;
//         outerRadiusObject.transform.localScale = new Vector3(outerRadius, outerRadius);
//         var spriteOuterRadius = outerRadiusObject.AddComponent<SpriteRenderer>();
//         var spriteOuter = Resources.Load<Sprite>("Editor/circle-inset");
//         spriteOuterRadius.sprite = spriteOuter;
//         spriteOuterRadius.color = new Color(0, 1, 1);

//         var collider = gameObject.AddComponent<CircleCollider2D>();
//         collider.radius = spriteOuterRadius.size.x * outerRadius / 2;
//         collider.isTrigger = true;

//         var innerRadiusObject = new GameObject("innerRadius");
//         innerRadiusObject.transform.SetParent(transform);
//         innerRadiusObject.transform.localPosition = Vector3.zero;
//         innerRadiusObject.transform.localScale = new Vector3(innerRadius, innerRadius);
//         var spriteInnerRadius = innerRadiusObject.AddComponent<SpriteRenderer>();
//         var spriteInner = Resources.Load<Sprite>("Editor/circle-outset");
//         spriteInnerRadius.sprite = spriteInner;
//         spriteInnerRadius.color = new Color(0, 1, 1);
//     }

// }

public enum SideEffect
{
    [Description("Burning")]
    Burning,
    [Description("Freezing")]
    Freezing,
    [Description("Poisoned")]
    Poisoned,
    [Description("Sick")]
    Sick,
    [Description("Shocked")]
    Shocked,
    [Description("Stunned")]
    Stunned,
}

public class SkillsReader
{
    public static Dictionary<string, SkillBlueprint> Get()
    {
        string filePath = "res://resources/skills/skills.json";
        
        var allSkills = JSONFile.Read<RootObject>(filePath).skills;

        var skills = new Dictionary<string, SkillBlueprint>();
        foreach (var skill in allSkills)
        {
            skills.Add(skill.name, skill);
        }
        return skills;
    }
}


[Serializable]
public class RootObject
{
    public SkillBlueprint[] skills;
}


[Serializable]
public class SkillRestrictions
{
    public int cooldown;
    public int perTurn;
    public int perTarget;
}

[Serializable]
public class Ingredient
{
    public string name;
    public int quantity;
}

[Serializable]
public class SkillBlueprint
{
    public string name;
    public Cost cost;
    public SkillRestrictions restrictions;
    public Range range;
    public int vision;
    public Range targetRadius;
    public Damage[] damages;
    public Movement? movement;
    public Effect[] effects;
    public Surface? surface;
    public string description;
    public Ingredient[]? ingredients;
    public int[]? stability;
    public string? unlockedBy;

    public bool IsSelfMovingSkill()
    {
        return movement?.target == "caster";
    }
}

[Serializable]
public class Cost
{
    public int ap;
    public int mp;
}

[Serializable]
public class Movement
{
    public string reference;
    public int strength;
    public string target;
}

[Serializable]
public class Range
{
    public int min;
    public int max;
}

[Serializable]
public class Damage
{
    public int min;
    public int max;
    public Element element;
    public DamageType damageType;
    public int duration;
}

[Serializable]
public class Effect
{
    public int rate;
    public SideEffect name;
}

[Serializable]
public class DamageResult
{
    public int amount;
    public Element element;
    // public bool isCritical;
}