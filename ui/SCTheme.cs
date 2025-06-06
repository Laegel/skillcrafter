using Godot;

class SCTheme
{
    public static readonly Color Base100 = new(0.058823529411764705f, 0.09019607843137255f, 0.16470588235294117f);
    public static readonly Color Base200 = new(0.047058823529411764f, 0.0784313725490196f, 0.1450980392156863f);
    public static readonly Color Base300 = new(0.0392156862745098f, 0.06666666666666667f, 0.12549019607843137f);
    public static readonly Color Neutral = new(0.11764705882352941f, 0.1607843137254902f, 0.23137254901960785f);
    public static readonly Color Ability = new(1, 0.843137254f, 0f);
    public static readonly Color Movement = new(0.5058823529411764f, 0.5490196078431373f, 0.9725490196078431f);
    public static readonly Color Health = new(0.984313725490196f, 0.4392156862745098f, 0.5215686274509804f);

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
}