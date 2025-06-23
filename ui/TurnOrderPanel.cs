using Godot;
using TurnOrder = ReactiveState<System.Collections.Generic.List<ReactiveState<TurnState>>>;

public partial class TurnOrderPanel : BuilderComponent
{
    public TurnOrder TurnOrder;
    public Node Build()
    {
        var size = DisplayServer.WindowGetSize();
        return NodeBuilder.CreateNode(new CanvasLayer
        {
            Name = GetType().Name,
            Offset = new(0, 0),
            Transform = new Transform2D(1, 0, 0, 1, 0, 50),

        },
        NodeBuilder.CreateNode(new VBoxContainer() {}, 
        NodeBuilder.CreateNode(new ScrollContainer
        {
            CustomMinimumSize = new(100, size.Y - 100),
            SizeFlagsVertical = Control.SizeFlags.ExpandFill,
            AnchorTop = 0,
            AnchorLeft = 0,
            OffsetLeft = 0,
            OffsetTop = 0,
            Position = new(0, 0)

        }, NodeBuilder.CreateNode(new VBoxContainer()
        {
            SizeFlagsVertical = Control.SizeFlags.ExpandFill
        },
            NodeBuilder.Map(new VBoxContainer()
        {
            SizeFlagsVertical = Control.SizeFlags.ExpandFill
        }, TurnOrder, (turn, index) =>
        {
            return new TurnOrderItem()
            {
                Key = index,
                TurnState = turn,
            };
        }))),
        new Button()
        {
            Text = "End Turn"
        }
        ));
    }

}

public partial class TurnOrderItem : BuilderComponent
{
    public ReactiveState<TurnState> TurnState;

    public Node Build()
    {
        var valueState = new ReactiveState<float>(0.5f);

        var textureRect = new EntityPortrait()
        {
            TurnState = TurnState,
            Name = GetType().Name + "-" + Key,
            Size = new(100, 50)
        };

        return NodeBuilder.CreateNode(new VerticalContainer()
        {
            Separation = 0
        },
            NodeBuilder.CreateNode(textureRect, new DynamicLabel()
            {
                AutowrapMode = TextServer.AutowrapMode.Off,
                Content = $"[color=#fff]{TurnState.Value.Entity.Name}[/color]",
            }
            ),
            new Gauge(valueState, SCTheme.Health)
            {
                CustomMinimumSize = new(100, 10)
            }
        );
    }
}

public partial class EntityPortrait : TextureRect
{
    public TurnState TurnState;
    public override void _Ready()
    {
        var viewport = new SubViewport();
        AddChild(viewport);

        viewport.Size = (Vector2I)Size;
        viewport.AddChild(new ColorRect() { Color = SCTheme.Base300, CustomMinimumSize = viewport.Size });
        var skeleton = TurnState.Entity.GetNode<Skeleton2D>("Skeleton").Duplicate() as Skeleton2D;
        skeleton.Position = new(Size.X / 2, Size.Y);
        viewport.AddChild(skeleton);

        skeleton.ForceUpdateTransform();

        Texture = viewport.GetTexture();

    }
}