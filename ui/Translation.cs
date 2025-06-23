using Godot;
using System;
using System.Collections.Generic;

public class Translation
{
    private static Dictionary<string, string> _translations = new Dictionary<string, string>();

    public static void LoadTranslations(string filePath)
    {
        _translations = JSONFile.Read<Dictionary<string, string>>(filePath);
    }

    public static string T(string key, Dictionary<string, object> parameters = null)
    {
        if (!_translations.ContainsKey(key))
        {
            return $"!{key}!";
        }

        var template = _translations[key];

        if (parameters != null)
        {
            foreach (var pair in parameters)
            {
                var pattern = $"{{{pair.Key}}}";
                string value;
                if (pair.Value is ReactiveState<int> reactiveState)
                {
                    value = reactiveState.Value.ToString();
                }
                else if (pair.Value is object objValue)
                {
                    value = objValue.ToString();
                }
                else if (pair.Value is string strValue)
                {
                    value = strValue;
                }
                else
                {
                    throw new ArgumentException($"Unsupported parameter type for key '{pair.Key}'.");
                }

                template = template.Replace(pattern, value);
            }
        }

        return template;
    }
}
