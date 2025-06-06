
using Godot;

public partial class MenuContainer : BuilderComponent
{
    public override Node Build()
    {
        return NodeBuilder.CreateNode(new CanvasLayer
        {
            Name = GetType().Name,
            Offset = new Vector2(0, 0),
            Transform = new Transform2D(1, 0, 0, 1, 0, 0),
        }, NodeBuilder.Match(MenuState.currentMenu, new() {
            { Menus.None, () => new Node()},
            { Menus.Equipment, () => Wrap(new MenuEquipment().Build(), false)},
            { Menus.Inventory, () => Wrap(new MenuInventory().Build())},
            { Menus.Designer, () => new MenuDesigner().Build()},
            { Menus.Map, () => new MenuMap().Build()},
        }));
    }

    private Node Wrap(Node child, bool fullMode = true)
    {
        var size = DisplayServer.WindowGetSize();
        var wrapper = new ColorRect
        {
            Color = SCTheme.Base100,
            CustomMinimumSize = new Vector2(fullMode ? size.X : size.X - 200, size.Y),
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill,

        };
        var marginContainer = new MarginContainer();
        marginContainer.AddThemeConstantOverride("margin_top", 50);

        return NodeBuilder.CreateNode(
            wrapper, NodeBuilder.CreateNode(
            marginContainer, child)
        );
    }
}