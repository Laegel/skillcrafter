using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public partial class Chest : Contraption
{
    public string ContainedItem;

    public override List<(TriggerKind, Dictionary<string, object> data)> Trigger()
    {
        return new() {
            (TriggerKind.AddToInventory, new Dictionary<string, object>() { { "item", ContainedItem } }),
            (TriggerKind.DiscardGameObject, new Dictionary<string, object>() { { "name", Name } }),
            (TriggerKind.Dialogue, new Dictionary<string, object>() { { "text", "chest" }, { "variables", new Dictionary<string, string>() { { "item", ContainedItem } } } }),
        };
    }
}