using Godot;

public partial class MenuDesigner : BuilderComponent
{
    public override Node Build()
    {
        return NodeBuilder.CreateNode(new Control
        {
            Name = nameof(MenuEquipment),
            CustomMinimumSize = new Vector2(200, 200),
        });
    }
}
