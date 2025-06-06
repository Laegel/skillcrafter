using Godot;
using System;
using System.Collections.Generic;
using System.Reflection;

public class NodeCounter
{
    private static int _count = 0;
    public static int GetCount()
    {
        return _count++;
    }
}

public class ReactiveState<T>
{
    private T _value;
    public T Value
    {
        get => _value;
        set
        {
            // GD.Print($"Comparing: {value} with {_value}");
            // if (!DeepEquals(_value, value))
            // {
            //     GD.Print($"Updated value!");
                _value = value;
                OnValueChanged?.Invoke(_value);
            // }
        }
    }

    public event Action<T> OnValueChanged;

    public ReactiveState(T initialValue = default)
    {
        _value = initialValue;
    }

    public ReactiveState<N> Map<N>(Func<T, N> mapFunc)
    {
        var mappedValue = new ReactiveState<N>(mapFunc(_value));
        OnValueChanged += newValue =>
        {
            mappedValue.Value = mapFunc(newValue);
        };
        return mappedValue;
    }

    private bool DeepEquals(object obj1, object obj2)
    {
        if (obj1 == null || obj2 == null)
            return obj1 == obj2;

        if (obj1.Equals(obj2))
            return true;

        var type = obj1.GetType();
        if (type != obj2.GetType())
            return false;

        if (type.IsPrimitive || type == typeof(string))
            return obj1.Equals(obj2);

        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            // Skip properties with parameters (e.g., indexers)
            if (property.GetIndexParameters().Length > 0)
                continue;

            try
            {
                var value1 = property.GetValue(obj1);
                var value2 = property.GetValue(obj2);
                if (!DeepEquals(value1, value2))
                    return false;
            }
            catch
            {
                // Skip inaccessible properties
                continue;
            }
        }

        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            try
            {
                var value1 = field.GetValue(obj1);
                var value2 = field.GetValue(obj2);
                if (!DeepEquals(value1, value2))
                    return false;
            }
            catch
            {
                // Skip inaccessible fields
                continue;
            }
        }

        return true;
    }
}


public static class NodeBuilder
{
    private static void AddChild(Node parent, Node child)
    {
        // if (child is BuilderComponent builderComponent)
        // {
        //     parent.AddChild(builderComponent.Build());
        // }
        // else
        // {
        parent.AddChild(child);
        // }
    }

    public static Node CreateNode(Node node, params Node[] children)
    {
        foreach (var child in children)
        {
            AddChild(node, child);
        }

        return node;
    }

    public static Node CreateNode(Func<Node> lazy, params Node[] children)
    {
        var node = lazy();
        foreach (var child in children)
        {
            AddChild(node, child);
        }

        return node;

    }


    public static Node Map(Node parent, List<Node> target)
    {
        for (int i = 0; i < target.Count; i++)
        {
            var newChild = target[i];
            AddChild(parent, newChild);
        }
        return parent;
    }

    public static Node Map(Func<Node> lazy, List<Node> target)
    {
        var parent = lazy();

        for (int i = 0; i < target.Count; i++)
        {
            var newChild = target[i];
            AddChild(parent, newChild);
        }
        return parent;
    }


    public static Node Map<T>(Node parent, ReactiveState<List<T>> target, Func<T, int, Node> iterator)
    {
        target.OnValueChanged += newChildren =>
        {
            foreach (var child in parent.GetChildren())
            {
                parent.RemoveChild(child);
            }
            for (int i = 0; i < newChildren.Count; i++)
            {

                var newChild = iterator(newChildren[i], i);
                AddChild(parent, newChild);
            }
        };

        for (int i = 0; i < target.Value.Count; i++)
        {
            var newChild = iterator(target.Value[i], i);
            AddChild(parent, newChild);
        }
        return parent;
    }

    public static Node Map<T>(Func<Node> lazy, ReactiveState<List<T>> target, Func<T, int, Node> iterator)
    {
        var parent = lazy();
        target.OnValueChanged += newChildren =>
        {
            foreach (var child in parent.GetChildren())
            {
                parent.RemoveChild(child);
            }
            for (int i = 0; i < newChildren.Count; i++)
            {

                var newChild = iterator(newChildren[i], i);
                AddChild(parent, newChild);
            }
        };

        for (int i = 0; i < target.Value.Count; i++)
        {
            var newChild = iterator(target.Value[i], i);
            AddChild(parent, newChild);
        }
        return parent;
    }

    public static Node If(ReactiveState<bool> target, Node child)
    {
        var parent = new Node2D()
        {
            Name = "If_" + NodeCounter.GetCount()
        };
        target.OnValueChanged += newValue =>
        {
            if (newValue)
            {
                AddChild(parent, child);
            }
            else
            {
                parent.RemoveChild(child);
            }
        };

        if (target.Value)
        {
            AddChild(parent, child);
        }
        return parent;
    }

    public static Node Show(ReactiveState<bool> target, CanvasItem child)
    {
        target.OnValueChanged += newValue =>
        {
            if (newValue)
            {
                child.Visible = true;
            }
            else
            {
                child.Visible = false;
            }
        };

        if (target.Value)
        {
            child.Visible = true;
        }
        else
        {
            child.Visible = false;
        }
        return child;
    }

    public static Node Match<T>(ReactiveState<T> target, Dictionary<T, Func<Node>> map)
    {
        var parent = new Node2D()
        {
            Name = "Match_" + NodeCounter.GetCount()
        };
        target.OnValueChanged += newValue =>
        {
            foreach (var child in parent.GetChildren())
            {
                parent.RemoveChild(child);
            }
            var newChild = map[target.Value]();
            AddChild(parent, newChild);
        };

        var initialChild = map[target.Value]();
        AddChild(parent, initialChild);
        return parent;
    }
}
public abstract partial class BuilderComponent
{
    public int key;
    public Node child;
    public abstract Node Build();

}