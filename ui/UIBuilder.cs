using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public class NodeCounter
{
    private static int _count = 0;
    public static int GetCount()
    {
        return _count++;
    }
}

public class BoundReactiveState<N, T> : ReactiveState<N>
{
    private readonly ReactiveState<T> _watched;
    private readonly Func<T, N> _mapFunc;
    private readonly Func<N, T> _revertMapFunc;
    public new event Action<N> OnValueChanged;

    public BoundReactiveState(ReactiveState<T> watched, Func<T, N> mapFunc, Func<N, T> revertMapFunc)
        : base(mapFunc(watched.Value))
    {
        _watched = watched;
        _mapFunc = mapFunc;
        _revertMapFunc = revertMapFunc;

        _watched.OnValueChanged += (value) =>
        {
            base.Value = _mapFunc(value);
            OnValueChanged?.Invoke(base.Value);
        };
    }
    public override N Value
    {
        get => _mapFunc(_watched.Value);
        set
        {
            _watched.Value = _revertMapFunc(value);
        }
    }
}

public class ReactiveState<T>
{
    private T _value;
    public virtual T Value
    {
        get => _value;
        set
        {
            _value = value;
            OnValueChanged?.Invoke(_value);
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

    public ReactiveState<N> Bind<N>(Func<T, N> mapFunc, Func<N, T> revertMapFunc)
    {
        return new BoundReactiveState<N, T>(this, mapFunc, revertMapFunc);
    }

    public static ReactiveState<List<N>> Merge<N>(params ReactiveState<N>[] states)
    {
        var merged = new ReactiveState<List<N>>(new());

        foreach (var (state, index) in states.Select((x, i) => (x, i)))
        {
            state.OnValueChanged += (newValue) =>
            {
                var currentMerged = merged.Value;
                currentMerged[index] = newValue;
                merged.Value = currentMerged;
            };
            merged.Value.Add(state.Value);
        }

        return merged;
    }

    public static bool operator ==(ReactiveState<T> reactiveState, T target)
    {
        if (reactiveState is null) return false;
        return EqualityComparer<T>.Default.Equals(reactiveState.Value, target);
    }

    public static bool operator !=(ReactiveState<T> reactiveState, T target)
    {
        return !(reactiveState == target);
    }

    // Override Equals and GetHashCode for consistency
    public override bool Equals(object obj)
    {
        if (obj is ReactiveState<T> other)
        {
            return this == other.Value;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return _value?.GetHashCode() ?? 0;
    }

    public static implicit operator T(ReactiveState<T> reactiveState)
    {
        return reactiveState.Value;
    }

    // Optional: Implicit conversion from T to ReactiveState<T>
    public static implicit operator ReactiveState<T>(T value)
    {
        return new ReactiveState<T>(value);
    }
}

public static class NodeBuilder
{
    private static void AddChild(Node parent, Node child)
    {
        parent.AddChild(child);
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

    public static Node Watch<T>(Node parent, Func<Node> lazy, ReactiveState<T> state)
    {
        state.OnValueChanged += (x) =>
        {
            foreach (var child in parent.GetChildren())
            {
                parent.RemoveChild(child);
            }
            parent.AddChild(lazy());
        };
        parent.AddChild(lazy());

        return parent;
    }


    public static Node Watch<T>(Node parent, Func<Node> lazy, params ReactiveState<T>[] states)
    {
        var state = ReactiveState<T>.Merge(states);
        state.OnValueChanged += (x) =>
        {
            foreach (var child in parent.GetChildren())
            {
                parent.RemoveChild(child);
            }
            parent.AddChild(lazy());
        };
        parent.AddChild(lazy());

        return parent;
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
            var newChild = TryToGetMatch(map, target);
            AddChild(parent, newChild);
        };

        var initialChild = TryToGetMatch(map, target);
        AddChild(parent, initialChild);
        return parent;
    }

    public static Node Match<T>(ReactiveState<T> target, Node parent, Dictionary<T, Func<Node>> map)
    {
        target.OnValueChanged += newValue =>
        {
            foreach (var child in parent.GetChildren())
            {
                parent.RemoveChild(child);
            }
            var newChild = TryToGetMatch(map, target);
            AddChild(parent, newChild);
        };

        var initialChild = TryToGetMatch(map, target);
        AddChild(parent, initialChild);
        return parent;
    }

    private static Node TryToGetMatch<T>(Dictionary<T, Func<Node>> map, ReactiveState<T> target)
    {
        try
        {
            return map[target.Value]();
        }
        catch (System.Exception)
        {
            GD.Print($"Tried to match {target.Value} but arm is not defined");
            throw;
        }
    }
}
public abstract partial class BuilderComponent
{
    public int Key;
    public Node Child;

    public string Name {
        get => GetType().Name + "-" + NodeCounter.GetCount();
    }

    public Node BuildWithDependencies()
    {
        var method = GetType().GetMethod("Build") ?? throw new InvalidOperationException($"Method 'Build' not found in {GetType().Name}");
        var parameters = method.GetParameters();

        var args = parameters.Select(p => ServiceStorage.Resolve(p.ParameterType)).ToArray();

        return (Node)method.Invoke(this, args);
    }

    public static implicit operator Node(BuilderComponent component)
    {
        return component.BuildWithDependencies();
    }
}

public interface IAutoRegisterService { }

[AttributeUsage(AttributeTargets.Class)]
public class AutoRegisterAttribute : Attribute { }

public static class ServiceStorage
{
    private static readonly Dictionary<Type, object> _services = new();

    public static void Register<T>(T service) where T : class
    {
        _services[typeof(T)] = service;
    }

    public static T Resolve<T>() where T : class
    {
        if (_services.TryGetValue(typeof(T), out var service))
        {
            return service as T;
        }
        throw new InvalidOperationException($"Service of type {typeof(T)} is not registered.");
    }

    public static object Resolve(Type type)
    {
        if (_services.TryGetValue(type, out var service))
        {
            return service;
        }
        throw new InvalidOperationException($"Service of type {type} is not registered.");
    }


    public static void AutoRegisterServices()
    {
        // Get all types in the current assembly
        var types = Assembly.GetExecutingAssembly().GetTypes();

        foreach (var type in types)
        {
            // Option 1: Check for marker interface
            if (typeof(IAutoRegisterService).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
            {
                var instance = Activator.CreateInstance(type);
                Register(type, instance);
            }

            // Option 2: Check for custom attribute
            else if (type.GetCustomAttribute<AutoRegisterAttribute>() != null)
            {
                var instance = Activator.CreateInstance(type);
                Register(type, instance);
            }
        }
    }

    private static void Register(Type type, object instance)
    {
        _services[type] = instance;
    }
}