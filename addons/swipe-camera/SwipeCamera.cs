using System;
using System.Linq;
using Godot;

class Pointer
{
    public static bool GetPointerDown(InputEvent @event, out Vector2 position)
    {
        if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.ButtonIndex == MouseButton.Left && eventMouseButton.Pressed)
        {
            position = eventMouseButton.Position;
            return true;
        }
        else if (@event is InputEventScreenTouch eventScreenTouch && eventScreenTouch.Pressed)
        {
            position = eventScreenTouch.Position;
            return true;
        }

        position = default;
        return false;
    }

    public static bool GetPointerUp(InputEvent @event)
    {
        if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.ButtonIndex == MouseButton.Left && !eventMouseButton.Pressed)
        {
            return true;
        }
        else if (@event is InputEventScreenTouch eventScreenTouch && !eventScreenTouch.Pressed)
        {
            return true;
        }

        return false;
    }

    public static bool GetPointerMove(InputEvent @event, out Vector2 position)
    {
        if (@event is InputEventMouseMotion eventMouseMotion)
        {
            position = eventMouseMotion.Position;
            return true;
        }
        else if (@event is InputEventScreenDrag eventScreenDrag)
        {
            position = eventScreenDrag.Position;
            return true;
        }

        position = default;
        return false;
    }
}

[Tool]
public partial class SwipeCamera : Camera2D
{
    public bool CameraDragging = false;
    public bool CameraMoving = false;
    private Vector2? dragOrigin;
    public float dragSpeed = 1.0f;

    public bool PreventCameraDragging = false;


    public override void _Input(InputEvent @event)
    {
        if (PreventCameraDragging)
        {
            return;
        }
        if (Pointer.GetPointerDown(@event, out Vector2 pointerPosition))
        {
            dragOrigin = pointerPosition;
            CameraDragging = true;
            FocusTarget = null;
        }
        else if (Pointer.GetPointerUp(@event))
        {
            dragOrigin = null;
            CameraDragging = false;
            CameraMoving = false;
        }
        else if (Pointer.GetPointerMove(@event, out Vector2 movePosition))
        {
            if (CameraDragging)
            {
                CameraMoving = true;
                var pos = (movePosition - (Vector2)dragOrigin) * -dragSpeed;
                Position += pos;
                dragOrigin = movePosition;
            }
        }
    }

    public override void _Process(double delta)
    {
        if (FocusTarget != null)
        {
            Position = Position.Lerp(new Vector2(FocusTarget.Position.X, FocusTarget.Position.Y), 0.1f);
            return;
        }
    }
    //     public float dragSpeed = 15;
    //     public Vector3? dragOrigin;

    //     public bool cameraDragging = true;

    //     public bool IsBlockedByUI = false;
    //     private Tilemap tilemap;

    //     private MapItem currentMapItem;
    private Node2D FocusTarget;

    //     void Awake()
    //     {
    //         tilemap = GameObject.Find("Tilemap").GetComponent<Tilemap>();
    //         var maps = ZonesReader.Get();
    //         currentMapItem = maps[Zone.Instance.CurrentBattle.Item1].maps[Zone.Instance.CurrentBattle.Item2];
    //     }

    //     void Update()
    //     {
    //         // if (IsTilemapSizeSmallerThanScreen())
    //         // {
    //         //     FocusOnCenterOfMap();
    //         //     return;
    //         // }


    //         Vector2 mousePosition = new(Input.mousePosition.x, Input.mousePosition.y);

    //         float left = Screen.width * 0.2f;
    //         float right = Screen.width - (Screen.width * 0.2f);

    //         float top = Screen.height * 0.2f;
    //         float bottom = Screen.height - (Screen.height * 0.2f);

    //         if (mousePosition.x < left || mousePosition.x > right || mousePosition.y < top || mousePosition.y > bottom)
    //         {
    //             cameraDragging = true;
    //         }

    //         if (cameraDragging && !IsBlockedByUI)
    //         {
    //             if (Input.GetMouseButtonDown(0) && dragOrigin == null)
    //             {
    //                 dragOrigin = Input.mousePosition;
    //                 return;
    //             }

    //             if (!Input.GetMouseButton(0))
    //             {
    //                 dragOrigin = null;
    //                 return;
    //             }

    //             if (dragOrigin == null)
    //             {
    //                 return;
    //             }

    //             Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition - (Vector3)dragOrigin);
    //             Vector3 move = new(-pos.x * dragSpeed, -pos.y * dragSpeed, 0);
    //             Vector3 newPosition = transform.position + move;

    //             // var clampedPosition = GetClampedPosition(newPosition);
    //             transform.position = newPosition;
    //             dragOrigin = Input.mousePosition;


    //         }
    //     }

    //     private void FocusOnCenterOfMap()
    //     {
    //         var worldBounds = GetWorldBounds();
    //         var center = (worldBounds.Item1 + worldBounds.Item2 + worldBounds.Item3 + worldBounds.Item4) / 4;
    //         center.x += 1; // Padding because of right panel
    //         center.y += 1; // Padding so we see more of the topmost part of the map
    //         center.z = transform.position.z;
    //         transform.position = center;
    //     }


    //     private Vector3 GetClampedPosition(Vector3 position)
    //     {

    //         var bottomLeft = tilemap.CellToWorld(new Vector3Int(tilemap.cellBounds.min.x + tilemap.cellBounds.size.x / 2, tilemap.cellBounds.min.y));
    //         var topRight = tilemap.CellToWorld(new Vector3Int(tilemap.cellBounds.max.x - tilemap.cellBounds.size.x / 2, tilemap.cellBounds.max.y));

    //         float minX = bottomLeft.x;
    //         float minY = bottomLeft.y;
    //         float maxX = topRight.x;
    //         float maxY = topRight.y;

    //         return new(
    //             Mathf.Clamp(position.x, -minX + 12, -maxX - 10),
    //             Mathf.Clamp(position.y, minY + (float)10.5, maxY + 1),
    //             transform.position.z
    //         );
    //     }

    //     private void OnDrawGizmos()
    //     {
    //         if (tilemap != null)
    //         {
    //             var worldBounds = GetWorldBounds();
    //             Gizmos.color = Color.blue;
    //             Gizmos.DrawSphere(worldBounds.Item1, 0.2f);
    //             Gizmos.color = Color.red;
    //             Gizmos.DrawSphere(worldBounds.Item2, 0.2f);
    //             Gizmos.color = Color.green;
    //             Gizmos.DrawSphere(worldBounds.Item3, 0.2f);
    //             Gizmos.color = Color.yellow;
    //             Gizmos.DrawSphere(worldBounds.Item4, 0.2f);
    //             Gizmos.color = Color.gray;
    //             Gizmos.DrawSphere(worldBounds.Item5, 0.2f);


    //             Gizmos.color = Color.black;
    //             var coords = GetTilemapBounds();
    //             Gizmos.DrawSphere(coords.Item1, 0.2f);
    //             Gizmos.DrawSphere(coords.Item2, 0.2f);
    //             Gizmos.DrawSphere(coords.Item3, 0.2f);
    //             Gizmos.DrawSphere(coords.Item4, 0.2f);
    //         }
    //     }

    //     private (Vector3, Vector3, Vector3, Vector3, Vector3) GetWorldBounds()
    //     {
    //         tilemap.CompressBounds();
    //         var center = tilemap.CellToWorld(new Vector3Int((tilemap.cellBounds.min.x + tilemap.cellBounds.max.x) / 2, (tilemap.cellBounds.min.y + tilemap.cellBounds.max.y) / 2, 0));

    //         return (
    //             tilemap.CellToWorld(new Vector3Int(tilemap.cellBounds.min.x + tilemap.cellBounds.size.x / 2, tilemap.cellBounds.min.y)),
    //             tilemap.CellToWorld(new Vector3Int(tilemap.cellBounds.min.x, tilemap.cellBounds.max.y - tilemap.cellBounds.size.y / 2)),
    //             tilemap.CellToWorld(new Vector3Int(tilemap.cellBounds.max.x - tilemap.cellBounds.size.x / 2, tilemap.cellBounds.max.y)),
    //             tilemap.CellToWorld(new Vector3Int(tilemap.cellBounds.max.x, tilemap.cellBounds.min.y + tilemap.cellBounds.size.y / 2)),
    //             center
    //         );
    //     }

    //     private (Vector3, Vector3, Vector3, Vector3) GetTilemapBounds()
    //     {
    //         tilemap.CompressBounds();
    //         var min = tilemap.CellToWorld(tilemap.cellBounds.min);
    //         var max = tilemap.CellToWorld(tilemap.cellBounds.max);

    //         return (
    //             new Vector3(min.x, min.y),
    //             new Vector3(max.x + tilemap.cellBounds.size.x, min.y + tilemap.cellBounds.size.y / 2),
    //             new Vector3(min.x - tilemap.cellBounds.size.x, max.y - tilemap.cellBounds.size.y / 2),
    //             new Vector3(max.x, max.y)
    //         );
    //     }

    public void LookAt(Node2D target)
    {
        Position = target.Position;
    }

        public void Focus(Node2D target)
    {
        FocusTarget = target;
    }
}

