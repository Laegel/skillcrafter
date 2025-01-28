using System;
using Godot;

class Caster {
    public static T Into<F, T>(F from, T target) where T : class {
        return Into(from, target);
    }

    public static Vector3I Into(Vector2I from) {
        return new(from.X, from.Y, 0);
    }

    public static Vector2I Into(Vector3I from) {
        return new(from.X, from.Y);
    }


    public static Vector3 Into(Vector2 from)
    {
        return new(from.X, from.Y, 0);
    }

    public static Vector2 Into(Vector3 from)
    {
        return new(from.X, from.Y);
    }
}
