
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;

public enum Menus
{
    [Description("none")]
    None = -1,
    [Description("equipment")]
    Equipment,
    [Description("inventory")]
    Inventory,
    [Description("designer")]
    Designer,
    [Description("parchment")]
    Parchment,
    [Description("map")]
    Map,
    [Description("test")]
    Test = 999,
}

[AutoRegister]
public class MenuState
{
    public ReactiveState<Menus> currentMenu = new(Menus.None);
}

public partial class MenuButton : BuilderComponent
{
    private ReactiveState<bool> isToggled = false;
    public Node Build(MenuState menuState)
    {
        var menus = new ReactiveState<List<Menus>>(Enum.GetValues(typeof(Menus)).Cast<Menus>().ToList());
        GD.Print(menus.Value);
        var button = new TextureButton()
        {

            AnchorTop = 0,
            AnchorLeft = 0,
            // TextureNormal = ResourceLoader.Load<Texture>("res://assets/ui/texture_button_normal.png"),
            // TexturePressed = ResourceLoader.Load<Texture>("res://assets/ui/texture_button_pressed.png"),
            // TextureHover = ResourceLoader.Load<Texture>("res://assets/ui/texture_button_hover.png"),
            CustomMinimumSize = new Vector2(100, 50),
        };
        button.Pressed += () =>
        {
            isToggled.Value = !isToggled.Value;
        };

        return NodeBuilder.CreateNode(new CanvasLayer
        {
            Name = GetType().Name,
            Offset = new Vector2(0, 0),
            Transform = new Transform2D(1, 0, 0, 1, 0, 0),
        }, button, new Label()
        {
            Text = "Menu",
            CustomMinimumSize = new Vector2(100, 50),
            OffsetTop = 0,
            OffsetLeft = 0,
        }, NodeBuilder.If(isToggled,

            NodeBuilder.Map(new VBoxContainer()
            {
                Position = new Vector2(100, 0),

            }, menus, (menu, _) =>
            {
                return NodeBuilder.CreateNode(() =>
                {
                    var button = new TextureButton()
                    {
                        CustomMinimumSize = new Vector2(100, 50),
                    };
                    button.Pressed += () =>
                    {
                        GD.Print("Pressed " + menu.ToString());
                        menuState.currentMenu.Value = menu;
                        isToggled.Value = false;
                    };
                    return button;
                }, new Label()
                {
                    Text = menu.ToString(),
                });
            }
        ))
        );
    }
}