using System;
using System.Collections.Generic;
using System.Reflection;

using Godot;
using Newtonsoft.Json;
using Map = System.Collections.Generic.Dictionary<string, MapCell>;

public class TilemapExtractor
{

    public static Map ExtractTiles(Node2D tilemap, Node2D mainGrid = null, int index = 1)
    {
        // Node2D, Sprite2D, TileMapLayer
        var mainGround = mainGrid.GetNode<TileMapLayer>("Ground");

        var tiles = new Map();
        var groundTilemap = tilemap.GetNode<TileMapLayer>("Ground");

        var sectionCells = tilemap.GetNode<Node2D>("SectionCells");

        var obstacles = tilemap.GetNode<Node2D>("Obstacles");
        var foregroundStoppers = tilemap.GetNode<TileMapLayer>("ForegroundStoppers");
        var backgroundStoppers = tilemap.GetNode<TileMapLayer>("BackgroundStoppers");
        var contraptions = tilemap.GetNode("Contraptions");
        var gap = tilemap.GetNodeOrNull<TileMapLayer>("Gap");

        // var enemySlots = tilemap.GetNode("EnemySlots");
        // var playerSlots = tilemap.GetNode("PlayerSlots");
        var entitySlots = tilemap.GetNodeOrNull<Node2D>("EntitySlots");

        // var playerSlotsTilemap = playerSlots.GetNode<TileMapLayer>("TileMap");
        // var playerSpawnSlotTilemap = playerSpawnSlot.GetNode<TileMapLayer>("TileMap");
        // var enemySlotsTilemap = enemySlots.GetNode<TileMapLayer>("TileMap");

        // var obstacleTiles = new List<Vector2I>();
        // foreach (var obstacle in obstacles.GetChildren())
        // {
        //     obstacleTiles.Add(groundTilemap.WorldToCell(obstacle.GetChild(0).GetChild(0).GetChild(0).position) + new Vector3Int(1, 0, 0));
        // }


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
            // var child = sectionCell.GetChild(0).GetChild(0).GetChild<Node2D>(0);
            if (sectionCell.HasMeta("triggers"))
            {
                var triggersMeta = sectionCell.GetMeta("triggers");
                newTile.metadata["triggers"] = triggersMeta.ToString();
            }
            // var previousParent = child.Parent;
            // child.Parent = null;
            // Coordinates are wrong here?
            // mainGround.LocalToMap(mainGround.ToLocal(entity.GlobalPosition))
            
            tiles.Add((mainGround.LocalToMap(mainGround.ToLocal(sectionCell.GlobalPosition)) + new Vector2(0, -1)).ToString(), newTile);
            // child.Parent = previousParent;
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

        // if (contraptions != null)
        // {
        //     foreach (Node2D contraption in contraptions.GetChildren())
        //     {
        //         contraption.AddToGroup("Contraption");
        //         var isOccupied = false;
        //         var customProperties = contraption.Get("CustomProperties") as Dictionary<string, Variant>;
        //         if (customProperties != null && customProperties.ContainsKey("class"))
        //         {
        //             switch (customProperties["class"].ToString())
        //             {
        //                 case "chest":
        //                     var contraptionInstance = contraption.AddChild(new Chest());
        //                     customProperties.TryGetCustomProperty("contains", out var containedItem);
        //                     contraptionInstance.containedItem = containedItem.ToString();

        //                     if (customProperties.TryGetCustomProperty("hidden", out var isHidden))
        //                     {
        //                         contraptionInstance.isHidden = true;
        //                         contraptionInstance.detectionLevel = int.Parse(isHidden.ToString());
        //                     }
        //                     isOccupied = true;
        //                     break;
        //                 default:
        //                     break;
        //             }
        //         }
        //         var newTile = new MapCell
        //         {
        //             kind = CellKind.Contraption,
        //             isOccupied = isOccupied,
        //             metadata = new() {
        //                 {"contraption", contraption.Name}
        //             }
        //         };
        //         var child = contraption.GetChild(0).GetChild<Node2D>(0);
        //         tiles.Add(mainGround.MapToLocal((Vector2I)child.Position).ToString(), newTile);
        //     }
        // }

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

            var newPos = cellPos;
            var oldTileSource = groundTilemap.GetCellSourceId(cellPos);

            var atlasCoords = groundTilemap.GetCellAtlasCoords(cellPos);
            mainGround.SetCell(TransferPosition(groundTilemap, mainGround, newPos), oldTileSource, atlasCoords);
            var cell = TransferPosition(groundTilemap, mainGround, newPos);

            // Got to merge tilesets and get tile IDs?
            // if (playerSlotsTilemap.GetTile(cellPos))
            // {
            //     newTile.kind = CellKind.Empty;
            //     newTile.isOccupied = false;
            //     newTile.entitySlot = EntitySlot.Player;
            // }
            // else if (enemySlotsTilemap.GetTile(cellPos))
            // {
            //     newTile.kind = CellKind.Empty;
            //     newTile.isOccupied = true;
            //     newTile.entitySlot = EntitySlot.Enemy;
            // }
            // else if (playerSpawnSlotTilemap.GetTile(cellPos))
            // {
            //     newTile.kind = CellKind.Empty;
            //     newTile.isOccupied = true;
            //     newTile.entitySlot = EntitySlot.PlayerSpawn;
            // }
            // if (obstacleTiles.Contains(cellPos))
            // {
            //     newTile.kind = CellKind.Obstacle;
            // }
            // else
            {
                newTile.Kind = CellKind.Empty;
                newTile.isOccupied = false;
            }

            if (!tiles.ContainsKey(cell.ToString()) || newTile.Kind == CellKind.Gap)
            {
                tiles.Add(cell.ToString(), newTile);
            }
        }

        // if (gap != null)
        // {
        //     var gapTilemap = gap.GetComponent<Tilemap>();
        //     foreach (var cellPos in gapTilemap.cellBounds.allPositionsWithin)
        //     {
        //         if (gapTilemap.GetTile<SuperTile>(cellPos) != null)
        //         {
        //             var newPos = gapTilemap.CellToWorld(cellPos);
        //             var newTile = new MapCell()
        //             {
        //                 metadata = new(),
        //                 kind = CellKind.Gap,
        //             };
        //             tiles.Add(mainGround.WorldToCell(newPos).ToString(), newTile);
        //         }
        //     }
        // }

        foreach (var tilemapStopper in new[] { foregroundStoppers, backgroundStoppers })
        {
            if (tilemapStopper != null)
            {
                foreach (var cellPos in tilemapStopper.GetUsedCells())
                {
                    var oldTile = tilemapStopper.GetCellTileData(cellPos);
                    if (oldTile != null)
                    {
                        var newPos = cellPos;
                        var oldTileSource = tilemapStopper.GetCellSourceId(cellPos);

                        var atlasCoords = tilemapStopper.GetCellAtlasCoords(cellPos);
                        var translatedPosition = TransferPosition(tilemapStopper, mainGround, newPos);
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

    private static TileSetAtlasSource MergeTileAtlasSources(List<TileSetAtlasSource> sources)
    {
        var newAtlasSource = new TileSetAtlasSource();

        foreach (var source in sources)
        {
            var texture = source.GetRuntimeTexture();
            var textureSize = texture.GetSize();


        }

        return newAtlasSource;
    }


    private static Vector2I TransferPosition(TileMapLayer source, TileMapLayer target, Vector2I position)
    {
        return target.LocalToMap(target.ToLocal(source.ToGlobal(source.MapToLocal(position))));
    }

    // Helper function to get max X and Y coordinates
    static int GetMaxX(TileMapLayer tilemap)
    {
        int maxX = 0;
        // foreach (Transform child in tilemap.transform)
        // {
        //     if (child.name == "Ground" || child.name == "Obstacles")
        //     {
        //         foreach (Transform chunk in child.transform)
        //         {
        //             string[] coords = chunk.name.Replace("Chunk (", "").Replace(")", "").Split(',');
        //             int x = int.Parse(coords[0]);
        //             if (x > maxX) maxX = x;
        //         }
        //     }
        // }
        return maxX + 1; // add 1 to account for 0-based indexing
    }

    static int GetMaxY(TileMapLayer tilemap)
    {
        int maxY = 0;
        // foreach (Transform child in tilemap.transform)
        // {
        //     if (child.name == "Ground" || child.name == "Obstacles")
        //     {
        //         foreach (Transform chunk in child.transform)
        //         {
        //             string[] coords = chunk.name.Replace("Chunk (", "").Replace(")", "").Split(',');
        //             int y = int.Parse(coords[1]);
        //             if (y > maxY) maxY = y;
        //         }
        //     }
        // }
        return maxY + 1; // add 1 to account for 0-based indexing
    }
}