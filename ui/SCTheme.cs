using Godot;

class SCTheme
{
    public static readonly Color Error = new("#fb7085");
    public static readonly Color Warning = new("#f4bf51");
    public static readonly Color Success = new("#2fd4bf");
    public static readonly Color Primary = new("#3abdf7");
    public static readonly Color Base100 = new("#252526");
    public static readonly Color Base200 = new(0.047058823529411764f, 0.0784313725490196f, 0.1450980392156863f);
    public static readonly Color Base300 = new("#282829");
    public static readonly Color Neutral = new(0.11764705882352941f, 0.1607843137254902f, 0.23137254901960785f);
    public static readonly Color Ability = new("#FFD700");
    public static readonly Color Movement = new("#818CF8");
    public static readonly Color Health = new("#FB7085");
    public static readonly Color Content = new("#CCCCCC");

    public static readonly int GridItemSize = 100;
    public static readonly int GridItemBorder = 4;

    public static Color WithTransparency(Color color, float alpha)
    {
        var newColor = color;
        newColor.A = alpha;
        return newColor;
    }

    public static Color GetColorByQuality(Quality quality)
    {
        return quality switch
        {
            Quality.Worn => new Color("#CCCCCC"),
            Quality.Common => new Color("#3FA66B"),
            Quality.Perfected => new Color("#3A8FB7"),
            Quality.Masterpiece => new Color("#8C6BB1"),
            Quality.Mythical => new Color("#C4A72D"),
            Quality.Unique => new Color("#C05C5C"),
            _ => new Color(1, 1, 1)
        };
    }

    public static Color GetColorByRarity(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Banal => new Color("#CCCCCC"),
            Rarity.Common => new Color("#3FA66B"),
            Rarity.Uncommon => new Color("#3A8FB7"),
            Rarity.Rare => new Color("#8C6BB1"),
            Rarity.Mythical => new Color("#C4A72D"),
            Rarity.Unique => new Color("#C05C5C"),
            _ => new Color(1, 1, 1)
        };
    }

    public static Color GetElementColor(Element element)
    {
        return element switch
        {
            Element.Neutral => new Color("#CCCCCC"),
            Element.Fire => new Color("#FF4500"), // Bright red-orange for fire
            Element.Water => new Color("#4682B4"), // Steel blue for water
            Element.Air => new Color("#87CEEB"), // Sky blue for air
            Element.Earth => new Color("#8B4513"), // Saddle brown for earth
            Element.Holy => new Color("#FFFFE0"), // Light yellow for holy
            Element.Curse => new Color("#4B0082"), // Indigo for curse
            Element.Radiating => new Color("#FFD700"), // Gold for radiating
            Element.Ice => new Color("#00FFFF"), // Cyan for ice
            Element.Poison => new Color("#556B2F"), // Dark olive green for poison
            Element.Blast => new Color("#FF6347"), // Tomato red for blast
        };
    }

    public static Color GetElementOutlineColor(Element element)
    {
        return element switch
        {
            Element.Neutral => new Color("#A9A9A9"), // Dark gray for neutral outline
            Element.Fire => new Color("#B22222"), // Firebrick for fire outline
            Element.Water => new Color("#2F4F4F"), // Dark slate gray for water outline
            Element.Air => new Color("#4682B4"), // Steel blue for air outline
            Element.Earth => new Color("#654321"), // Dark brown for earth outline
            Element.Holy => new Color("#DAA520"), // Goldenrod for holy outline
            Element.Curse => new Color("#800080"), // Purple for curse outline
            Element.Radiating => new Color("#B8860B"), // Dark goldenrod for radiating outline
            Element.Ice => new Color("#5F9EA0"), // Cadet blue for ice outline
            Element.Poison => new Color("#6B8E23"), // Olive drab for poison outline
            Element.Blast => new Color("#CD5C5C"), // Indian red for blast outline
        };
    }
}