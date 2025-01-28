using Godot;
using Newtonsoft.Json;

class JSON
{
    public static T Read<T>(string filePath)
    {
        var text = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
        return JsonConvert.DeserializeObject<T>(text.GetAsText());
    }

    public static void Write<T>(string filePath, T value)
    {
        FileAccess.Open(filePath, FileAccess.ModeFlags.Write).StoreString(JsonConvert.SerializeObject(value));
    }
}

