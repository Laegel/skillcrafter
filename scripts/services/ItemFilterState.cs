using System;

[AutoRegister]
public class ItemFilterState
{
    private string _state;

    public ItemType Type
    {
        get => Enum.Parse<ItemType>(_state.Split(':')[0]);
        set
        {
            var category = Category;
            var newState = $"{value}:{category}";
            if (_state != newState)
            {
                _state = newState;
                OnValueChanged?.Invoke();
            }
        }
    }

    public int Category
    {
        get => int.Parse(_state.Split(':')[1]);
        set
        {
            var type = Type;
            var newState = $"{type}:{value}";
            if (_state != newState)
            {
                _state = newState;
                OnValueChanged?.Invoke();
            }
        }
    }

    public void Set(ItemType type, int category)
    {
        _state = $"{type}:{category}";
        OnValueChanged?.Invoke();
    }

    public event Action OnValueChanged;
}
