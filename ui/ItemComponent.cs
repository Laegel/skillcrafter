using System;
using Godot;

public partial class ItemComponent : BuilderComponent
{

    private Timer timer;
    // private TextureButton button;
    private Button button;
    private bool isLongPress = false;
    public Color borderColor = new(0, 0, 0, 0);
    public Texture2D backgroundImage = new();
    public Action onClick = () => { };
    public Action onLongPress = () => { };

    private void ClearTimers()
    {
        foreach (var child in button.GetChildren())
        {
            if (child is Timer t)
            {
                t.Stop();
                button.RemoveChild(t);
            }
        }
    }

    public Node Build()
    {
        button = new Button()
        {
            Name = GetType().Name + Key,
            CustomMinimumSize = new Vector2(SCTheme.GridItemSize, SCTheme.GridItemSize),
        };

        button.ButtonDown += () =>
        {
            isLongPress = false;
            timer = new Timer()
            {
                WaitTime = 0.5f,
                OneShot = true,
                Autostart = true,
            };
            timer.Timeout += () =>
            {
                isLongPress = true;
                onLongPress();
                ClearTimers();
            };
            button.AddChild(timer);
        };
        button.MouseExited += () =>
        {
            if (timer != null)
            {
                ClearTimers();
            }
        };

        button.ButtonUp += () =>
        {
            if (isLongPress)
            {
                return;
            }
            onClick();
            ClearTimers();

        };

        var border = 2;
        var style = new StyleBoxFlat
        {
            BgColor = SCTheme.Base300,
        };
        style.SetBorderWidthAll(border);
        style.BorderColor = borderColor;
        button.AddThemeStyleboxOverride("normal", style);
        button.AddThemeStyleboxOverride("hover", style);
        button.AddThemeStyleboxOverride("focus", style);
        button.AddThemeStyleboxOverride("pressed", style);

        return NodeBuilder.CreateNode(button, NodeBuilder.CreateNode(
            () =>
            {
                var shaderMaterial = new ShaderMaterial();

                var shader = new Shader
                {
                    Code = FileAccess.Open("res://radial.shader", FileAccess.ModeFlags.Read).GetAsText()
                };

                shaderMaterial.Shader = shader;
                shaderMaterial.SetShaderParameter("inner_color", SCTheme.Base300);
                shaderMaterial.SetShaderParameter("outer_color", borderColor);
                return new Panel()
                {
                    Material = shaderMaterial,
                    CustomMinimumSize = new Vector2(SCTheme.GridItemSize - border * 3, SCTheme.GridItemSize - border * 3),
                    OffsetTop = border * 1.5f,
                    OffsetLeft = border * 1.5f,
                    MouseFilter = TextureRect.MouseFilterEnum.Ignore,
                };
            }, NodeBuilder.CreateNode(new TextureRect
            {
                Texture = backgroundImage,
                CustomMinimumSize = new Vector2(SCTheme.GridItemSize - border * 3, SCTheme.GridItemSize - border * 3),
                ExpandMode = TextureRect.ExpandModeEnum.FitWidthProportional,
                StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
                MouseFilter = Control.MouseFilterEnum.Ignore,
            }, Child == null ? new Label() {Text = ""} : Child)
            )
        );
    }
}