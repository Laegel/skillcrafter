using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using TurnBasedSkills = System.Collections.Generic.List<(TurnSkillRestrictions, SkillBlueprint)>;
using EquipedGear = System.Collections.Generic.Dictionary<GearSlot, GameDataGear>;
using Godot;

public class GearSlotState
{
    public static ReactiveState<EquipedGear> equipedGear = new();

    public static void Set(GearSlot slot, string value)
    {
        // var gearDictionary = Parse();
        var gearDictionary = equipedGear.Value;
        gearDictionary[slot] = new GameDataGear { name = value };
        equipedGear.Value = gearDictionary;
    }

    // private static Dictionary<string, string> Parse()
    // {
    //     if (string.IsNullOrEmpty(equipedGear.Value))
    //     {
    //         return new Dictionary<string, string>();
    //     }

    //     try
    //     {
    //         return JsonSerializer.Deserialize<Dictionary<string, string>>(equipedGear.Value) ?? new Dictionary<string, string>();
    //     }
    //     catch (JsonException)
    //     {
    //         return new Dictionary<string, string>();
    //     }
    // }
}


public class SkillSlotsState
{
    public static ReactiveState<TurnBasedSkills> equipedSkills = new();

    public static void Set(int slot, (TurnSkillRestrictions, SkillBlueprint) value)
    {
        var skillDictionary = equipedSkills.Value;
        skillDictionary[slot] = value;
        equipedSkills.Value = skillDictionary;
    }
}