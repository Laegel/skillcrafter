using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

public enum TriggerKind
{
    [Description("Dialogue")]
    Dialogue,
    [Description("SpawnEntities")]
    SpawnEntities,
    [Description("UnlockZone")]
    UnlockZone,
    [Description("UnlockMap")]
    UnlockMap,
    [Description("AddToInventory")]
    AddToInventory,
    [Description("DiscardGameObject")]
    DiscardGameObject,
    [Description("Battle")]
    Battle,
    [Description("OpenBarterOffers")]
    OpenBarterOffers,
}

class TriggerSystem {
    public static void Trigger(TriggerKind triggerKind, Dictionary<string, object> data)
    {
        switch (triggerKind)
        {
            case TriggerKind.Dialogue:
                // DocumentManager.GetInstance().Dialogue = data;
                break;
            case TriggerKind.SpawnEntities:
                break;
            case TriggerKind.UnlockZone:
                break;
            case TriggerKind.UnlockMap:
                break;
            case TriggerKind.AddToInventory:
                break;
            case TriggerKind.DiscardGameObject:
                break;
            default:
                break;
        }
    }
}