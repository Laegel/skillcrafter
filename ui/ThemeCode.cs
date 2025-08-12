using System;
using System.Linq;
using System.Text.RegularExpressions;
using Godot;

public partial class ThemeCode : RichTextEffect
{
    private string className;
    public string bbcode = "theme";

    public override bool _ProcessCustomFX(CharFXTransform charFx)
    {
        var classNames = ParseClasses(charFx.Env["classes"].ToString());

        foreach (var className in classNames)
        {
            var bgMatch = Regex.Match(className, @"^bg-(\w+)$");
            if (bgMatch.Success)
            {
                var colorName = bgMatch.Groups[1].Value;
                charFx.Color = ParseColor(colorName);
            }

            var textMatch = Regex.Match(className, @"^text-([\w-]+)$");
            if (textMatch.Success)
            {
                var colorName = textMatch.Groups[1].Value;
                charFx.Color = ParseColor(colorName);
            }
        }


        return true;
    }

    private string[] ParseClasses(string classes)
    {
        return classes.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    }

    private Color ParseColor(string colorName)
    {
        var items = colorName.Split('-');
        var modifier = 100;
        String key;
        try
        {
            var lastItem = items.Last();
            modifier = int.Parse(lastItem);
            key = colorName.Replace("-" + lastItem, "");
        }
        catch (System.Exception)
        {
            key = colorName;
        }
        return key.ToLower() switch
        {
            "red" => new Color(1, 0, 0),
            "green" => new Color(0, 1, 0),
            "blue" => new Color(0, 0, 1),
            "health" => SCTheme.Health,
            "movement" => SCTheme.Movement,
            "ability" => SCTheme.Ability,
            "content" => SCTheme.WithTransparency(SCTheme.Content, modifier / 100f),
            "rarity-banal" => SCTheme.GetColorByRarity(Rarity.Banal),
            "rarity-common" => SCTheme.GetColorByRarity(Rarity.Common),
            "rarity-uncommon" => SCTheme.GetColorByRarity(Rarity.Uncommon),
            "rarity-rare" => SCTheme.GetColorByRarity(Rarity.Rare),
            "rarity-mythical" => SCTheme.GetColorByRarity(Rarity.Mythical),
            "rarity-unique" => SCTheme.GetColorByRarity(Rarity.Unique),
            "quality-worn" => SCTheme.GetColorByQuality(Quality.Worn),
            "quality-common" => SCTheme.GetColorByQuality(Quality.Common),
            "quality-perfected" => SCTheme.GetColorByQuality(Quality.Perfected),
            "quality-masterpiece" => SCTheme.GetColorByQuality(Quality.Masterpiece),
            "quality-mythical" => SCTheme.GetColorByQuality(Quality.Mythical),
            "quality-unique" => SCTheme.GetColorByQuality(Quality.Unique),
            _ => new Color(0, 0, 0) // Default color (black)
        };
    }
}