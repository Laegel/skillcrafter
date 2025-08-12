using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Godot;

using TurnBasedSkills = System.Collections.Generic.List<(TurnSkillRestrictions, SkillBlueprint)?>;


public enum State
{
    None,
    Idle,
    Walk,
    Run,
    Rise,
    Attack,
    Cast
}

public class MovementEffect
{
    public string reference;
    public Entity target;
    public int cells;

    public MovementEffect(string reference, Entity target, int cells)
    {
        this.reference = reference;
        this.target = target;
        this.cells = cells;
    }
}

public class SurfaceEffect
{
    public Surface surface;
}

public class OverTimeEffect
{
    public int duration;
    public Entity caster;
    public Damage damage;
    public string name;
}

public enum Disposition {
    Ally,
    Passive,
    Enemy,
}

public partial class Entity : Interactive
{
    public State previousState, currentState;

    [Export] public int maxHealthPoints;
    [Export] public int healthPoints;
    [Export] public int maxAbilityPoints;
    [Export] public int abilityPoints;
    [Export] public int maxMovementPoints;
    [Export] public int movementPoints;
    [Export] public int power;
    [Export] public int armor;
    [Export] public Disposition disposition;
    public List<Vector2I> OccupiedCells;

    public List<TurnSideEffect> sideEffects = new();
    public List<TurnOverTimeEffect> overTimeEffects = new();
    public TurnBasedSkills skills = new();
    public List<string> skillNames;

    private Dictionary<Element, int> resistance = new() {
        {Element.Fire, 0},
        {Element.Water, 0},
        // {Element.Ice, 0},
        // {Element.Radiating, 0},
        {Element.Holy, 0},
        {Element.Curse, 0},
        // {Element.Steam, 0},
        // {Element.Poison, 0},
        {Element.Neutral, 0}
    };

    public Dictionary<Element, int> Resistance { get { return resistance; }
        set
        {
            resistance = value
                .Concat(Enum.GetValues(typeof(Element)).Cast<Element>().Except(value.Keys)
                .Select(element => new KeyValuePair<Element, int>(element, 0)))
                .OrderBy(kv => kv.Key)
                .ToDictionary(kv => kv.Key, kv => kv.Value);
        }
    }

    public Vector2 direction = new(1, 0);
    public float movementSpeedModifier = 1f;

    public void LateUpdate()
    {
        bool stateChanged = previousState != currentState;
        previousState = currentState;
        if (stateChanged)
        {
            HandleStateChanged();
        }
    }

    public bool CanCast(SkillBlueprint skill)
    {
        return skills.Find(item => item.Value.Item2.Name == skill.Name).Value.Item1.cooldown == 0 &&
             abilityPoints >= skill.AP &&
             movementPoints >= skill.MP;
    }

    public CastResult Cast(SkillBlueprint skill, List<Entity> targets)
    {
        var currentTargets = targets;
        var movementTargets = new List<Entity>();
        // if (skill.movement?.target == "caster")
        // {
        //     movementTargets.Add(this);
        // } else {
        //     movementTargets = targets;
        // }


        RunAnimation(State.Attack);

        SurfaceEffect mainSurfaceEffect = null;
        var surfaceEffects = new List<SurfaceEffect>();

        // var overTimeEffects = new List<OverTimeEffect>();
        var damages = new List<DamageResult>();
        // foreach (var target in targets)
        // {
        //     foreach (var damage in skill.damages)
        //     {
        //         if (damage.damageType == DamageType.OverTime) {
        //             overTimeEffects.Add(new OverTimeEffect() {
        //                 caster = this,
        //                 damage = damage,
        //                 duration = damage.duration,
        //                 name = skill.name,
        //             });
        //             continue;
        //         }
        //         var elementResistance = target.Resistance[damage.element];
        //         var armor = target.armor;
        //         var baseAmount = Random.Range(damage.min, damage.max) * power;
        //         var resistanceFactor = 1 - (elementResistance / 100.0f);
        //         var amount = (int)(baseAmount / armor * resistanceFactor);
        //         damages.Add(new DamageResult()
        //         {
        //             amount = amount,
        //             element = damage.element,
        //         });
        //     }
        // }

        // foreach (var damage in skill.damages)
        // {
        //     var surfaceEffect = ElementToSurfaceEffect(damage.element);
        //     if (surfaceEffect != null) {
        //         surfaceEffects.Add(surfaceEffect);
        //     }
        // }

        // }
        // else
        // {
        //     RunAnimation(State.Cast);
        // }

        // StartCoroutine(ResetIdleState());

        // if (skill.movement != null)
        // {
        //     return new CastResult
        //     {
        //         movementEffects = movementTargets.Select(target => new MovementEffect(
        //             skill.movement.reference,
        //             target,
        //             skill.movement.strength
        //         )).ToList(),
        //         sideEffects = skill.effects == null ? new List<Effect>() : skill.effects.ToList(),
        //         damages = damages,
        //         mainSurfaceEffect = mainSurfaceEffect,
        //         surfaceEffects = surfaceEffects,
        //         overTimeEffects = overTimeEffects,
        //     };
        // } else {
        //     return new CastResult
        //     {
        //         movementEffects = new List<MovementEffect>(),
        //         sideEffects = skill.effects == null ? new List<Effect>() : skill.effects.ToList(),
        //         damages = damages,
        //         mainSurfaceEffect = mainSurfaceEffect,
        //         surfaceEffects = surfaceEffects,
        //         overTimeEffects = overTimeEffects,
        //     };
        // }

        switch (skill.SkillEffect)
        {
            case SkillEffects.DealDamage1:
            case SkillEffects.DealDamage2:
            case SkillEffects.DealDamage3:
                break;
            case SkillEffects.CreateSurface:
                var surface = (ReactiveState<Element>)skill.SkillEffectConfiguration["element"];
                mainSurfaceEffect = new SurfaceEffect { surface = SurfaceEnumConverter.FromElementToSurface(surface.Value) };
                break;
            default:
                break;
        }
        return new CastResult()
        {
            Damages = damages,
            MovementEffects = new(),
            SurfaceEffects = new(),
            MainSurfaceEffect = mainSurfaceEffect,
            OverTimeEffects = new(),
            SideEffects = new(),
        };
    }

    private SurfaceEffect? ElementToSurfaceEffect(Element element)
    {

        switch (element)
        {
            case Element.Fire: return new SurfaceEffect
            {
                surface = Surface.Fire
            };
            case Element.Water: return new SurfaceEffect
            {
                surface = Surface.Water
            };
            // case Element.Ice: return new SurfaceEffect
            // {
            //     surface = Surface.Ice
            // };
            // case Element.Radiating: return new SurfaceEffect
            // {
            //     surface = Surface.Radiating
            // };
            // case Element.Poison: return new SurfaceEffect
            // {
            //     surface = Surface.Poison
            // };

        }
        return null;
    }

    public float GetRateFromEffect(Characteristic characteristic)
    {
        var effects = new Dictionary<string, (Characteristic, int)>() {
            {"Focus", (Characteristic.Stability, 10)},
            {"Evasion", (Characteristic.Evasion, 10)},
            {"Heavy", (Characteristic.MoveResistance, 10)},
            {"Massive", (Characteristic.MoveResistance, 100)},
        };
        return overTimeEffects.Sum(x =>
        {
            var effect = effects[x.name];
            return effect.Item1 == characteristic ? effect.Item2 : 0;
        });
    }

    // private System.Collections.IEnumerator ResetIdleState()
    // {
    //     yield return new WaitForSeconds(0.8f);
    //     RunAnimation(State.Idle);
    // }

    // public Entity[] GetTargets()
    // {
    //     Entity[] entities;

    //     Func<Entity, bool> isEntity = (Entity entity) => entity.tag == "Entity";
    //     Func<Entity, bool> isNotPlayer = (Entity entity) => entity != this;

    //     entities = Physics2D.OverlapCircleAll(transform.position, 10)
    //         .Select(item => item.gameObject)
    //         .Where(isEntity)
    //         .Where(isNotPlayer)
    //         .ToArray();

    //     return entities;
    // }

    // public void ComputeDamageInput(Element element, bool isCritical, int power, int tier)
    // {
    //     const float CRITICAL_MODIFIER = 1.5f;
    //     var value =
    //       (Math.Pow(power * tier, 2) / this.resistance) *
    //       (isCritical ? CRITICAL_MODIFIER : 1f);
    //     var valueWithComboModifier = value + (value * comboCounter) / 10;
    //     var valueWithElementModifier =
    //       valueWithComboModifier +
    //       valueWithComboModifier * -(elementAffinities[element] || 0);

    //     return Math.Ceiling(valueWithElementModifier);
    // }

    public void TakeDamage(int damages, Element element, bool isCritical)
    {
        var damageManager = GetNode<DamageManager>("DamageManager");
        var color = SCTheme.GetElementColor(element);
        damageManager.Create(damages.ToString() + (isCritical ? "!" : ""), color, SCTheme.GetElementOutlineColor(element));
        if (damages >= healthPoints)
        {
            healthPoints = 0;
        }
        else
        {
            healthPoints -= damages;
        }
    }

    public void Kill()
    {
        healthPoints = 0;
    }

    public void ChangeDirection(Vector2I target)
    {
        var skeleton = GetNode<Skeleton2D>("Skeleton");
        var direction = GridHelper.GetRelativePosition(BattleMap.EntityPositionToCellCoordinates(Position), target) switch
        {
            Direction.Left => -1,
            Direction.Top => -1,
            _ => 1
        };
        skeleton.Scale = new Vector2(Mathf.Abs(skeleton.Scale.X) * direction, skeleton.Scale.Y);
    }

    public void Move()
    {
        currentState = State.Run;
    }

    public void Stop()
    {
        currentState = State.Idle;
    }


    void RunAnimation(State newState)
    {
        // var skeletonAnimation = ((SpineSprite)GetNode("skeleton")).GetAnimationState();
        // // var skeletonAnimation = transform.Find("skeleton").GetComponent<Spine.Unity.SkeletonAnimation>();
        // string stateName = null;
        // switch (newState)
        // {
        //     case State.Idle:
        //         stateName = "idle";
        //         break;
        //     case State.Walk:
        //         stateName = "walk";
        //         break;
        //     case State.Run:
        //         stateName = "running";
        //         break;
        //     case State.Rise:
        //         stateName = "rise";
        //         break;
        //     case State.Attack:
        //         //     var attachmentName = "";
        //         //     if (skeletonAnimation.Skeleton.Slots.Items.Count() > 9)
        //         //     {
        //         //         attachmentName = skeletonAnimation.Skeleton.Slots.Items[10].Attachment.ToString();
        //         //     }
        //         //     stateName = attachmentName.StartsWith("images/bow") ? "attacking/bow" : "attacking/sword";
        //         //     break;
        //         // case State.Cast:
        //         stateName = "casting/simple";
        //         break;
        //     default:
        //         break;
        // }
        // skeletonAnimation.AnimationName = stateName;
    }

    void HandleStateChanged()
    {
        string stateName = null;
        switch (currentState)
        {
            case State.Idle:
                stateName = "idle";
                break;
            case State.Walk:
                stateName = "walk";
                break;
            case State.Run:
                stateName = "running";
                break;
            case State.Rise:
                stateName = "rise";
                break;
            case State.Attack:
                stateName = "attacking/bow";
                break;
            case State.Cast:
                stateName = "casting/simple";
                break;
            default:
                break;
        }
        // transform.Find("skeleton").GetComponent<Spine.Unity.SkeletonAnimation>().AnimationName = stateName;
    }

}

public class CastResult
{
    public List<MovementEffect> MovementEffects;
    public List<Effect> SideEffects;
    public List<OverTimeEffect> OverTimeEffects;
    public List<DamageResult> Damages;
    public SurfaceEffect MainSurfaceEffect;
    public List<SurfaceEffect> SurfaceEffects;
}
