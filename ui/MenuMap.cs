using Godot;

public partial class MenuMap : BuilderComponent
{
    public Node Build()
    {
        return NodeBuilder.CreateNode(new Control
        {
            Name = nameof(MenuEquipment),
            CustomMinimumSize = new Vector2(200, 200),
        });
    }
}
