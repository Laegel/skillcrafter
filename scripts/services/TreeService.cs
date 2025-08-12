using Godot;

[AutoRegister]
public partial class TreeService
{
    public Window Root;

    public Node GetNode(NodePath path)
    {
        return Root.GetNode(path);
    }

    public Node GetNodeOrNull(NodePath path)
    {
        return Root.GetNodeOrNull(path);
    }

    public T GetNode<T>(NodePath path) where T : class
    {
        return (T)(object)GetNode(path);
    }

    public T GetNodeOrNull<T>(NodePath path) where T : class
    {
        return GetNodeOrNull(path) as T;
    }

    public T GetNodeRecursively<T>(string nodeName, Node parent = null) where T : class
    {
        parent ??= Root;
        foreach (Node child in parent.GetChildren())
        {
            if (child.Name == nodeName)
            {
                return child as T;
            }

            var foundNode = GetNodeRecursively<T>(nodeName, child);

            if (foundNode != null)
            {
                return foundNode;
            }
        }

        return null;
    }
}
