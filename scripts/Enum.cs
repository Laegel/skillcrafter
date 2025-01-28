using System;
using System.ComponentModel;

public static class Extensions
{
    public static string GetEnumDescription(this Enum enumValue)
    {
        var field = enumValue.GetType().GetField(enumValue.ToString());
        if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
        {
            return attribute.Description;
        }
        throw new ArgumentException($"Item not found: {enumValue}", nameof(enumValue));
    }

    public static T GetEnumValueByDescription<T>(this string description) where T : Enum
    {
        foreach (Enum enumItem in Enum.GetValues(typeof(T)))
        {
            if (enumItem.GetEnumDescription() == description)
            {
                return (T)enumItem;
            }
        }
        throw new ArgumentException($"Not found: {description}", nameof(description));
    }
}