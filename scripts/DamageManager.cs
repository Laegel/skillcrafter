using System.Collections;
using System.Collections.Generic;

using Godot;

public partial class DamageManager : CenterContainer
{
    public void Create(string text, Color color, Color outline)
    {
        var panel = new PanelContainer();
        var sb = new StyleBoxEmpty();
        panel.AddThemeStyleboxOverride("panel", sb);

        var label = new RichTextLabel
        {
            Text = $"[outline_size=2][outline_color={outline.ToHtml()}][color={color.ToHtml()}]{text}[/color][/outline_color][/outline_size]",
            BbcodeEnabled = true,
            FitContent = true,
            AutowrapMode = TextServer.AutowrapMode.Off
        };
        label.AddThemeFontSizeOverride("normal_font_size", 22);
        panel.AddChild(label);
        AddChild(panel);



        panel.Modulate = new Color(1, 1, 1, 0);
        var endPos = panel.Position + new Vector2(10, 10);
        // panel.Position = startPos + new Vector2(-10, 0);

        var tween = CreateTween();

        tween.TweenProperty(panel, "modulate:a", 1.0f, 0.3f)
             .SetTrans(Tween.TransitionType.Sine)
             .SetEase(Tween.EaseType.Out);

        // tween.TweenProperty(panel, "position", endPos, 0.4f)
        //      .SetTrans(Tween.TransitionType.Sine)
        //      .SetEase(Tween.EaseType.Out);

        tween.TweenInterval(0.8f);

        tween.TweenProperty(panel, "modulate:a", 0.0f, 0.4f)
             .SetTrans(Tween.TransitionType.Sine)
             .SetEase(Tween.EaseType.In);

        tween.TweenCallback(Callable.From(() => panel.QueueFree()));
    }
}
