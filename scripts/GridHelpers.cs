using Godot;

public class GridHelper
{
    public static Direction GetRelativePosition(Vector2I positionA, Vector2I positionB)
    {
        if (positionB.X < positionA.X)
        {
            return Direction.Left;
        }
        else if (positionB.X > positionA.X)
        {
            return Direction.Right;
        }
        else if (positionB.Y > positionA.Y)
        {
            return Direction.Top;
        }
        else if (positionB.Y < positionA.Y)
        {
            return Direction.Bottom;
        }
        else
        {
            // The positions are the same
            return Direction.Left;
        }
    }

    public static Vector3I StringToVector3I(string input)
    {
        if (input.StartsWith("(") && input.EndsWith(")"))
        {
            input = input[1..^1];
        }

        string[] sArray = input.Split(',');

        var result = new Vector3I(
            int.Parse(sArray[0]),
            int.Parse(sArray[1]),
            int.Parse(sArray[2]));

        return result;
    }

    public static Vector2I StringToVector2I(string input)
    {
        if (input.StartsWith("(") && input.EndsWith(")"))
        {
            input = input[1..^1];
        }

        string[] sArray = input.Split(',');

        var result = new Vector2I(
            int.Parse(sArray[0]),
            int.Parse(sArray[1])
        );

        return result;
    }
}

class BattleMap
{
    public static TileMapLayer tilemap;

    public static Vector3 EntityPositionToCellPosition(Vector3 position)
    {
        return new Vector3(position.X, position.Y + 64, 0);
    }

    public static Vector2I EntityPositionToCellCoordinates(Vector2 position)
    {
        return tilemap.LocalToMap(Caster.Into(EntityPositionToCellPosition(Caster.Into(position))));
    }

    public static Vector2 CellPositionToEntityPosition(Vector2 position)
    {
        // return new Vector2(position.X, position.Y);
        return new Vector2(position.X - 12, position.Y - 64);
    }

    public static Vector2 CellCoordinatesToEntityPosition(Vector2I position)
    {
        return CellPositionToEntityPosition(tilemap.MapToLocal(position));
    }
}