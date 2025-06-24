using System.Collections.Generic;

using Godot;
using Map = System.Collections.Generic.Dictionary<string, MapCell>;

public class TilemapExtractor
{

    public static Map ExtractTiles(Node2D tilemap, Node2D mainGrid = null, int index = 1)
    {
        var mainGround = mainGrid.GetNode<TileMapLayer>("Ground");

        var tiles = new Map();
        var groundTilemap = tilemap.GetNode<TileMapLayer>("Ground");

        var sectionCells = tilemap.GetNode<Node2D>("SectionCells");

        var obstacles = tilemap.GetNode<Node2D>("Obstacles");
        var foregroundStoppers = tilemap.GetNode<TileMapLayer>("ForegroundStoppers");
        var backgroundStoppers = tilemap.GetNode<TileMapLayer>("BackgroundStoppers");
        var contraptions = tilemap.GetNode("Contraptions");
        var gap = tilemap.GetNodeOrNull<TileMapLayer>("Gap");

        var entitySlots = tilemap.GetNodeOrNull<Node2D>("EntitySlots");

        var obstacleTiles = new List<Vector2I>();
        foreach (Sprite2D obstacle in obstacles.GetChildren())
        {
            obstacleTiles.Add(mainGround.LocalToMap(mainGround.ToLocal(obstacle.GlobalPosition)));
        }


        foreach (Sprite2D sectionCell in sectionCells.GetChildren())
        {
            var newTile = new MapCell
            {
                Kind = CellKind.Empty,
                isOccupied = false,
                metadata = new() {
                    {"section", sectionCell.Name.ToString().StartsWith("tile") ? null : sectionCell.Name}
                }
            };
            if (sectionCell.HasMeta("triggers"))
            {
                var triggersMeta = sectionCell.GetMeta("triggers");
                newTile.metadata["triggers"] = triggersMeta.ToString();
            }
            
            tiles.Add((mainGround.LocalToMap(mainGround.ToLocal(sectionCell.GlobalPosition)) + new Vector2(0, -1)).ToString(), newTile);
        }

        foreach (var tile in tiles)
        {
            if (tile.Value.metadata["section"] != null)
            {
                var section = tile.Value.metadata["section"].ToString();
                var adjacentTiles = new[] {
                    GridHelper.StringToVector2I(tile.Key) + new Vector2I(0, -1),
                    GridHelper.StringToVector2I(tile.Key) + new Vector2I(0, 1),
                    GridHelper.StringToVector2I(tile.Key) + new Vector2I(1, 0),
                    GridHelper.StringToVector2I(tile.Key) - new Vector2I(1, 0),
                };
                foreach (var adjacentPos in adjacentTiles)
                {
                    if (tiles.ContainsKey(adjacentPos.ToString()))
                    {
                        var adjacentTile = tiles[adjacentPos.ToString()];
                        if (adjacentTile.metadata["section"] == null)
                        {
                            adjacentTile.metadata["section"] = section;
                        }
                        if (tile.Value.metadata.ContainsKey("triggers"))
                        {
                            adjacentTile.metadata["triggers"] = tile.Value.metadata["triggers"].ToString();
                        }
                    }
                }

            }
        }


        if (entitySlots != null)
        {
            foreach (Node2D entity in entitySlots.GetChildren())
            {
                var entityName = entity.Name.ToString();
                var newTile = new MapCell
                {
                    Kind = CellKind.Empty,
                    entitySlot = EntitySlot.Entity,
                    isOccupied = true,
                    metadata = new() {
                    {"entity", char.IsUpper(entityName[0]) || entityName.Contains('_') ? entityName : entityName + "_" + Time.GetUnixTimeFromSystem()}
                }
                };
                tiles.Add((mainGround.LocalToMap(mainGround.ToLocal(entity.GlobalPosition)) + new Vector2(0, -1)).ToString(), newTile);
            }
        }

        if (contraptions != null)
        {
            foreach (Sprite2D contraption in contraptions.GetChildren())
            {
                contraption.AddToGroup("interactives");
                contraption.AddToGroup("contraptions");
                
                var isOccupied = false;
                var customProperties = contraption.GetMetaList();
                if (customProperties != null && customProperties.Contains("class"))
                {
                    var contraptionClass = contraption.GetMeta("class");
                    switch (contraptionClass.ToString())
                    {
                        case "chest":
                            var chest = new Chest();
                            contraption.AddChild(chest);
                            var containedItem = contraption.GetMeta("contains");
                            chest.ContainedItem = containedItem.ToString();
                            var isHidden = contraption.GetMeta("hidden");

                            if (isHidden.AsBool())
                            {
                                chest.isHidden = true;
                                chest.detectionLevel = int.Parse(isHidden.ToString());
                            }
                            isOccupied = true;
                            break;
                        default:
                            break;
                    }
                }
                var newTile = new MapCell
                {
                    Kind = CellKind.Contraption,
                    isOccupied = isOccupied,
                    metadata = new() {
                        {"contraption", contraption.Name}
                    }
                };
                var relocated = mainGround.LocalToMap(mainGround.ToLocal(contraption.GlobalPosition));
                tiles.Add(relocated.ToString(), newTile);
            }
        }

        foreach (var cellPos in groundTilemap.GetUsedCells())
        {
            var oldTile = groundTilemap.GetCellTileData(cellPos);

            if (oldTile == null)
            {
                continue;
            }
            var newTile = new MapCell()
            {
                metadata = new()
            };

            var oldTileSource = groundTilemap.GetCellSourceId(cellPos);

            var atlasCoords = groundTilemap.GetCellAtlasCoords(cellPos);
            mainGround.SetCell(TransferPosition(groundTilemap, mainGround, cellPos), oldTileSource, atlasCoords);
            var cell = TransferPosition(groundTilemap, mainGround, cellPos);

            if (obstacleTiles.Contains(cellPos))
            {
                newTile.Kind = CellKind.Obstacle;
            }
            else
            {
                newTile.Kind = CellKind.Empty;
                newTile.isOccupied = false;
            }

            if (!tiles.ContainsKey(cell.ToString()) || newTile.Kind == CellKind.Gap)
            {
                tiles.Add(cell.ToString(), newTile);
            }
        }

        if (gap != null)
        {
            foreach (var cellPos in gap.GetUsedCells())
            {
                var tile = gap.GetCellTileData(cellPos);
                if (tile != null)
                {
                    var newTile = new MapCell()
                    {
                        metadata = new(),
                        Kind = CellKind.Gap,
                    };
                    tiles.Add(TransferPosition(gap, mainGround, cellPos).ToString(), newTile);
                }
            }
        }

        foreach (var tilemapStopper in new[] { foregroundStoppers, backgroundStoppers })
        {
            if (tilemapStopper != null)
            {
                foreach (var cellPos in tilemapStopper.GetUsedCells())
                {
                    var oldTile = tilemapStopper.GetCellTileData(cellPos);
                    if (oldTile != null)
                    {
                        var oldTileSource = tilemapStopper.GetCellSourceId(cellPos);

                        var atlasCoords = tilemapStopper.GetCellAtlasCoords(cellPos);
                        var translatedPosition = TransferPosition(tilemapStopper, mainGround, cellPos);
                        mainGround.SetCell(translatedPosition, oldTileSource, atlasCoords);

                        var newTile = new MapCell()
                        {
                            metadata = new(),
                            Kind = CellKind.Wall,
                        };
                        if (tiles.ContainsKey(translatedPosition.ToString()))
                        {
                            GD.Print("Clashing tiles " + translatedPosition.ToString());
                        }
                        else
                        {
                            tiles.Add(translatedPosition.ToString(), newTile);
                        }
                    }
                }
            }
        }
        return tiles;
    }

    private static Vector2I TransferPosition(TileMapLayer source, TileMapLayer target, Vector2I position)
    {
        return target.LocalToMap(target.ToLocal(source.ToGlobal(source.MapToLocal(position))));
    }
}