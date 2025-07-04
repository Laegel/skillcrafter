using Godot;

class SCTheme
{
    public static readonly Color Error = new("#fb7085");
    public static readonly Color Warning = new("#f4bf51");
    public static readonly Color Success = new("#2fd4bf");
    public static readonly Color Primary = new("#3abdf7");
    public static readonly Color Base100 = new(0.058823529411764705f, 0.09019607843137255f, 0.16470588235294117f);
    public static readonly Color Base200 = new(0.047058823529411764f, 0.0784313725490196f, 0.1450980392156863f);
    public static readonly Color Base300 = new(0.0392156862745098f, 0.06666666666666667f, 0.12549019607843137f);
    public static readonly Color Neutral = new(0.11764705882352941f, 0.1607843137254902f, 0.23137254901960785f);
    public static readonly Color Ability = new("#FFD700");
    public static readonly Color Movement = new("#818CF8");
    public static readonly Color Health = new("#FB7085");

    public static readonly int GridItemSize = 100;

    public static Color GetColorByQuality(Quality quality)
    {
        return quality switch
        {
            Quality.Worn => new Color(1, 1, 1),
            Quality.Common => new Color(0, 1, 0),
            Quality.Perfected => new Color(0, 0, 1),
            Quality.Masterpiece => new Color(1, 0, 1),
            Quality.Mythical => new Color(0, 1, 1),
            Quality.Unique => new Color(1, 0, 0),
            _ => new Color(1, 1, 1)
        };
    }

    public static Color GetColorByRarity(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Banal => new Color(1, 1, 1),
            Rarity.Common => new Color(0, 1, 0),
            Rarity.Uncommon => new Color(0, 0, 1),
            Rarity.Rare => new Color(1, 0, 1),
            Rarity.Mythical => new Color(0, 1, 1),
            Rarity.Unique => new Color(1, 0, 0),
            _ => new Color(1, 1, 1)
        };
    }

    public static Color GetElementColor(Element element)
    {
        return element switch
        {
            Element.Neutral => new Color("#CCCCCC"),
            Element.Fire => new Color("#B8554F"),
            Element.Water => new Color("#80A4BE"),
            Element.Air => new Color("#9498B5"),
            Element.Earth => new Color("#E09C4F"),
            Element.Holy => new Color("#FFFFFF"),
            Element.Curse => new Color("#000000"),
            Element.Radiating => new Color(1, 1, 1),
            Element.Steam => new Color(1, 1, 1),
            Element.Ice => new Color(1, 1, 1),
            Element.Poison => new Color(1, 1, 1),
            Element.Blast => new Color(1, 1, 1),
        };
    }
}