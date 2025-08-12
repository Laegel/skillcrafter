
using Godot;

public partial class MenuContainer : BuilderComponent
{
    public Node Build(MenuState menuState)
    {
        return NodeBuilder.CreateNode(new CanvasLayer
        {
            Name = GetType().Name,
            Offset = new Vector2(0, 0),
            Transform = new Transform2D(1, 0, 0, 1, 0, 0),
        }, NodeBuilder.Match(menuState.currentMenu, new() {
            { Menus.None, () => new Node()},
            { Menus.Equipment, () => Wrap(new MenuEquipment(), false)},
            { Menus.Inventory, () => Wrap(new MenuInventory())},
            { Menus.Designer, () => Wrap(new MenuDesigner())},
            { Menus.Map, () => Wrap(new MenuMap())},
            { Menus.Parchment, () => Wrap(new ParchmentComponent())},
            { Menus.Test, () => {
                var label = new Label()  { Text = "TEST"};
                return label;
            }}
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