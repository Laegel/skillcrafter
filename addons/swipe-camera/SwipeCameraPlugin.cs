#if TOOLS
using Godot;
using System;

[Tool]
public partial class SwipeCameraPlugin : EditorPlugin
{
	public override void _EnterTree()
	{
		// Initialization of the plugin goes here.
		// Add the new type with a name, a parent type, a script and an icon.
		var script = GD.Load<Script>("addons/swipe-camera/SwipeCamera.cs");
		var texture = GD.Load<Texture2D>("icon.svg");
		AddCustomType("SwipeCamera", "Camera2D", script, texture);
	}

	public override void _ExitTree()
	{
		// Clean-up of the plugin goes here.
		// Always remember to remove it from the engine when deactivated.
		RemoveCustomType("SwipeCamera");
	}
}
#endif