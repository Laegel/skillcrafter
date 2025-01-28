public enum CraftKind
{
    Skill,
    Gear,
}

public class Requirement
{
    public int quantity;
}

public class RandomResourceRequirement : Requirement
{
    public Rarity rarity;
}

public class SpecificResourceRequirement : Requirement
{
    public string name;
}


public class BarterOffer
{
    public string item;
    public int quantity;
    public RandomResourceRequirement[] randomRequirements;
    public SpecificResourceRequirement[] specificRequirements;
}