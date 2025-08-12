using TurnBasedSkills = System.Collections.Generic.List<(TurnSkillRestrictions, SkillBlueprint)>;
using EquipedGear = System.Collections.Generic.Dictionary<GearSlot, GameDataGear>;

[AutoRegister]
public class GearSlotState
{
    public ReactiveState<EquipedGear> equipedGear = new();

    public void Set(GearSlot slot, string value)
    {
        var gearDictionary = equipedGear.Value;
        gearDictionary[slot] = new GameDataGear { name = value };
        equipedGear.Value = gearDictionary;
    }
}

[AutoRegister]
public class SkillSlotsState
{
    public ReactiveState<TurnBasedSkills> equipedSkills = new();

    public void Set(int slot, (TurnSkillRestrictions, SkillBlueprint) value)
    {
        var skillDictionary = equipedSkills.Value;
        skillDictionary[slot] = value;
        equipedSkills.Value = skillDictionary;
    }
}

[AutoRegister]
public class ParchmentState
{
    public Parchment Parchment;
}