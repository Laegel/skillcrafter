using Godot;
using System;
using System.ComponentModel;

public enum Surface
{
	[Description("Water")]
	Water,
	[Description("Earth")]
	Earth,
	[Description("Fire")]
	Fire,
	[Description("Air")]
	Air,
	[Description("Poison")]
	Poison,
	[Description("Lava")]
	Lava,
	[Description("RadiatingWater")]
	RadiatingWater,
	[Description("Ice")]
	Ice,
	[Description("Fog")]
	Fog,
	[Description("Smoke")]
	Smoke,
	// [Description("Mud")]
	// Mud,
}


public static class SurfaceEnumConverter
{
	public static Surface FromElementToSurface(Element element)
	{
		return element switch
        {
            Element.Fire => Surface.Fire,
            Element.Water => Surface.Water,
            Element.Air => Surface.Air,
			Element.Earth => Surface.Earth,
            Element.Neutral => throw new NotImplementedException(),
            Element.Holy => throw new NotImplementedException(),
            Element.Curse => throw new NotImplementedException(),
            Element.Radiating => throw new NotImplementedException(),
            Element.Steam => throw new NotImplementedException(),
            Element.Ice => throw new NotImplementedException(),
            Element.Poison => throw new NotImplementedException(),
            Element.Blast => throw new NotImplementedException()
        };
	}
}

public partial class Surfaces : Node2D
{

	public void SetSurface(Vector2 worldPosition, Surface surface, Texture2D texture = null)
	{
		var surfaceName = surface.GetEnumDescription().ToSnakeCase();
		var surfaceScene = GD.Load<PackedScene>($"res://resources/zones/surfaces/{surfaceName}.tscn");
		var node = surfaceScene.Instantiate<Sprite2D>();
		node.Name = "surface-" + worldPosition.ToString();
		node.Position = worldPosition;
		switch (surface)
		{
			case Surface.Earth:
				node.Texture = texture;
				break;
			default:
				break;
		}
		AddChild(node);
	}

	public void RemoveSurface(Vector2 worldPosition)
	{
		RemoveChild(GetNode("surface-" + worldPosition.ToString()));
	}
}
