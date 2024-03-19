using System;
using System.Collections.Generic;
using Gazeus.DesafioMatch3.Models;
using UnityEditor;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Core
{
    public class GameService
    {
        public event Action onSpecialTileDeathEvent;

        private List<List<Tile>> _boardTiles;
        private List<int> _tilesTypes;
        private int _tileCount;
        private TileSpecialAction _specialActionToExecute;

        private int _SpecialPowerupChance = 46; // The chance for special Tiles to appear
        private LevelData _currentLevelData;
        private List<List<bool>> _matchedTilesPosition = new List<List<bool>>();

        public List<List<Tile>> BoardTiles { get => _boardTiles; set => _boardTiles = value; }
        public TileSpecialAction SpecialActionToExecute { get => _specialActionToExecute; set => _specialActionToExecute = value; }
        public List<List<bool>> MatchedTilesPosition { get => _matchedTilesPosition; set => _matchedTilesPosition = value; }

        public bool IsValidMovement(int fromX, int fromY, int toX, int toY)
        {
            List<List<Tile>> newBoard = CopyBoard(_boardTiles);

            (newBoard[toY][toX], newBoard[fromY][fromX]) = (newBoard[fromY][fromX], newBoard[toY][toX]);

            switch (_currentLevelData.LevelMechanic)
            {
                case LevelMechanic.Match3:
                    for (int y = 0; y < newBoard.Count; y++)
                    {
                        for (int x = 0; x < newBoard[y].Count; x++)
                        {
                            if (x > 1 &&
                                newBoard[y][x].Type == newBoard[y][x - 1].Type &&
                                newBoard[y][x - 1].Type == newBoard[y][x - 2].Type)
                            {
                                return true; //Is Line
                            }

                            if (y > 1 &&
                                newBoard[y][x].Type == newBoard[y - 1][x].Type &&
                                newBoard[y - 1][x].Type == newBoard[y - 2][x].Type)
                            {
                                return true; // Is Column
                            }
                        }
                    }
                    break;
                case LevelMechanic.Match4:
                    for (int y = 0; y < newBoard.Count; y++)
                    {
                        for (int x = 0; x < newBoard[y].Count; x++)
                        {
                            if (x > 2 &&
                                newBoard[y][x].Type == newBoard[y][x - 1].Type &&
                                newBoard[y][x - 1].Type == newBoard[y][x - 2].Type &&
                                newBoard[y][x - 2].Type == newBoard[y][x - 3].Type
                                )
                            {
                                return true; //Is Line
                            }

                            if (y > 2 &&
                                newBoard[y][x].Type == newBoard[y - 1][x].Type &&
                                newBoard[y - 1][x].Type == newBoard[y - 2][x].Type &&
                                newBoard[y - 2][x].Type == newBoard[y - 3][x].Type
                                )
                            {
                                return true; // Is Column
                            }
                        }
                    }
                    break;
                case LevelMechanic.MatchSquared:
                    for (int y = 0; y < newBoard.Count; y++)
                    {
                        for (int x = 0; x < newBoard[y].Count; x++)
                        {
                            if ((x > 0 && y > 0) &&
                                newBoard[y][x].Type == newBoard[y][x - 1].Type &&
                                newBoard[y][x].Type == newBoard[y - 1][x].Type &&
                                newBoard[y][x].Type == newBoard[y - 1][x - 1].Type
                                )
                            {
                                if ((x == toX || x == fromX) && (y == toY || y == fromY) ||
                                    (x - 1 == toX || x - 1 == fromX) && (y == toY || y == fromY) ||
                                    (x == toX || x == fromX) && (y - 1 == toY || y - 1 == fromY) ||
                                    (x - 1 == toX || x - 1 == fromX) && (y - 1 == toY || y - 1 == fromY)
                                    )
                                {
                                    return true;
                                }
                            }
                        }
                    }
                    break;
            }

            return false;
        }

        public List<List<Tile>> StartGame(LevelData levelData)
        {
            _currentLevelData = levelData;
            _tilesTypes = levelData.TileTypes;
            _SpecialPowerupChance = levelData.PowerupChance;
            _boardTiles = CreateBoard(levelData.LevelBoardSize, levelData.LevelBoardSize, _tilesTypes);

            return _boardTiles;
        }

        public List<BoardSequence> SwapTile(int fromX, int fromY, int toX, int toY)
        {
            List<List<Tile>> newBoard = CopyBoard(_boardTiles);

            (newBoard[toY][toX], newBoard[fromY][fromX]) = (newBoard[fromY][fromX], newBoard[toY][toX]);

            List<BoardSequence> boardSequences = new();
            List<List<bool>> matchedTiles = FindMatches(newBoard, _currentLevelData);

            SetSpecialAction(newBoard, matchedTiles, fromX, fromY); // We check twice to see if the special tile was the selected tile 
            SetSpecialAction(newBoard, matchedTiles, toX, toY); // or the swaped tile

            /// Here we treat the special action that involve Tiles manipulation
            if (_specialActionToExecute != TileSpecialAction.None)
            {
                switch (_specialActionToExecute)
                {
                    case TileSpecialAction.ClearLines:

                        for (int y = 0; y < newBoard.Count; y++)
                        {
                            matchedTiles[y][toX] = true;
                        }
                        for (int x = 0; x < newBoard.Count; x++)
                        {
                            matchedTiles[toY][x] = true;
                        }

                        break;
                    case TileSpecialAction.Bomb:

                        for (int y = -1; y <= 1; y++)
                        {
                            for (int x = -1; x <= 1; x++)
                            {
                                int neighborTileY = toY + y;
                                int neighborTileX = x + toX;

                                /// Check if the neighbor indices are within bounds
                                if (neighborTileY >= 0 && neighborTileY < newBoard.Count &&
                                    neighborTileX >= 0 && neighborTileX < newBoard.Count &&
                                    (neighborTileY != toY || neighborTileX != toX))
                                {
                                    matchedTiles[neighborTileY][neighborTileX] = true;
                                }
                            }
                        }
                        break;
                    case TileSpecialAction.ColorClear:
                        for (int y = 0; y < newBoard.Count; y++)
                        {
                            for (int x = 0; x < newBoard.Count; x++)
                            {
                                if (newBoard[y][x].Type == newBoard[toY][toX].Type)
                                {
                                    matchedTiles[y][x] = true;
                                }
                            }
                        }

                        break;
                    case TileSpecialAction.Death:
                        onSpecialTileDeathEvent.Invoke();
                        break;
                }

                _specialActionToExecute = TileSpecialAction.None;
            }

            MatchedTilesPosition = matchedTiles;

            while (HasMatch(matchedTiles))
            {
                //Cleaning the matched tiles
                List<Vector2Int> matchedPosition = new();
                for (int y = 0; y < newBoard.Count; y++)
                {
                    for (int x = 0; x < newBoard[y].Count; x++)
                    {
                        if (matchedTiles[y][x])
                        {
                            matchedPosition.Add(new Vector2Int(x, y));
                            newBoard[y][x] = new Tile { Id = -1, Type = -1, Action = TileSpecialAction.None };
                        }
                    }
                }

                // Dropping the tiles
                Dictionary<int, MovedTileInfo> movedTiles = new();
                List<MovedTileInfo> movedTilesList = new();
                for (int i = 0; i < matchedPosition.Count; i++)
                {
                    int x = matchedPosition[i].x;
                    int y = matchedPosition[i].y;
                    if (y > 0)
                    {
                        for (int j = y; j > 0; j--)
                        {
                            Tile movedTile = newBoard[j - 1][x];
                            newBoard[j][x] = movedTile;
                            if (movedTile.Type > -1)
                            {
                                if (movedTiles.ContainsKey(movedTile.Id))
                                {
                                    movedTiles[movedTile.Id].To = new Vector2Int(x, j);
                                }
                                else
                                {
                                    MovedTileInfo movedTileInfo = new()
                                    {
                                        From = new Vector2Int(x, j - 1),
                                        To = new Vector2Int(x, j)
                                    };
                                    movedTiles.Add(movedTile.Id, movedTileInfo);
                                    movedTilesList.Add(movedTileInfo);
                                }
                            }
                        }

                        newBoard[0][x] = new Tile
                        {
                            Id = -1,
                            Type = -1,
                            Action = TileSpecialAction.None
                        };
                    }
                }

                // Filling the board
                List<AddedTileInfo> addedTiles = new();
                for (int y = newBoard.Count - 1; y > -1; y--)
                {
                    for (int x = newBoard[y].Count - 1; x > -1; x--)
                    {
                        if (newBoard[y][x].Type == -1)
                        {
                            int tileType = UnityEngine.Random.Range(0, _tilesTypes.Count);
                            Tile tile = newBoard[y][x];
                            tile.Action = TileSpecialAction.None;
                            if (UnityEngine.Random.Range(0, 100) < _SpecialPowerupChance && _currentLevelData.TileSpecialActions.Count > 0)
                                tile.Action = _currentLevelData.TileSpecialActions[UnityEngine.Random.Range(0, _currentLevelData.TileSpecialActions.Count)]; // Adds a special action to a tile
                            tile.Id = _tileCount++;
                            tile.Type = _tilesTypes[tileType];
                            addedTiles.Add(new AddedTileInfo
                            {
                                Position = new Vector2Int(x, y),
                                Type = tile.Type,
                                SpecialAction = tile.Action
                            });
                        }
                    }
                }

                BoardSequence sequence = new()
                {
                    MatchedPosition = matchedPosition,
                    MovedTiles = movedTilesList,
                    AddedTiles = addedTiles
                };

                boardSequences.Add(sequence);
                matchedTiles = FindMatches(newBoard, _currentLevelData);
            }

            _boardTiles = newBoard;

            return boardSequences;
        }

        public void SetSpecialAction(List<List<Tile>> newBoard, List<List<bool>> matchedTiles, int x, int y)
        {
            if (matchedTiles[y][x])
            {
                if (newBoard[y][x].Action != TileSpecialAction.None)
                {
                    _specialActionToExecute = newBoard[y][x].Action;
                }
            }
        }

        private static List<List<Tile>> CopyBoard(List<List<Tile>> boardToCopy)
        {
            List<List<Tile>> newBoard = new(boardToCopy.Count);
            for (int y = 0; y < boardToCopy.Count; y++)
            {
                newBoard.Add(new List<Tile>(boardToCopy[y].Count));
                for (int x = 0; x < boardToCopy[y].Count; x++)
                {
                    Tile tile = boardToCopy[y][x];
                    newBoard[y].Add(new Tile { Id = tile.Id, Type = tile.Type, Action = tile.Action });
                }
            }

            return newBoard;
        }

        private List<List<Tile>> CreateBoard(int width, int height, List<int> tileTypes)
        {
            List<List<Tile>> board = new(height);
            _tileCount = 0;
            for (int y = 0; y < height; y++)
            {
                board.Add(new List<Tile>(width));
                for (int x = 0; x < width; x++)
                {
                    board[y].Add(new Tile { Id = -1, Type = -1 });
                }
            }

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    List<int> noMatchTypes = new(tileTypes.Count);
                    for (int i = 0; i < tileTypes.Count; i++)
                    {
                        noMatchTypes.Add(_tilesTypes[i]);
                    }

                    switch (_currentLevelData.LevelMechanic)
                    {
                        case LevelMechanic.Match3:

                            if (x > 1 &&
                                board[y][x - 1].Type == board[y][x - 2].Type)
                            {
                                noMatchTypes.Remove(board[y][x - 1].Type);
                            }

                            if (y > 1 &&
                                board[y - 1][x].Type == board[y - 2][x].Type)
                            {
                                noMatchTypes.Remove(board[y - 1][x].Type);
                            }
                            break;
                        case LevelMechanic.Match4:
                            if (x > 2 &&
                                board[y][x - 1].Type == board[y][x - 2].Type &&
                                board[y][x - 2].Type == board[y][x - 3].Type
                                )
                            {
                                noMatchTypes.Remove(board[y][x - 1].Type);
                                noMatchTypes.Remove(board[y][x - 2].Type);
                            }

                            if (y > 2 &&
                                board[y - 1][x].Type == board[y - 2][x].Type &&
                                board[y - 2][x].Type == board[y - 3][x].Type
                                )
                            {
                                noMatchTypes.Remove(board[y - 1][x].Type);
                                noMatchTypes.Remove(board[y - 2][x].Type);
                            }
                            break;
                        case LevelMechanic.MatchSquared:
                            if (x > 0 && y > 0 &&
                                board[y][x - 1].Type == board[y - 1][x].Type
                                )
                            {
                                noMatchTypes.Remove(board[y - 1][x - 1].Type);
                            }
                            break;
                        default:
                            break;
                    }

                    board[y][x].Id = _tileCount++;
                    board[y][x].Type = noMatchTypes[UnityEngine.Random.Range(0, noMatchTypes.Count)];
                    board[y][x].Action = TileSpecialAction.None;
                    if (UnityEngine.Random.Range(0, 100) < _SpecialPowerupChance && _currentLevelData.TileSpecialActions.Count > 0)
                        board[y][x].Action = _currentLevelData.TileSpecialActions[UnityEngine.Random.Range(0, _currentLevelData.TileSpecialActions.Count)]; // Adds a special action to a tile
                }
            }

            return board;
        }

        public BoardSequence ClearBoard()
        {
            List<List<Tile>> newBoard = CopyBoard(_boardTiles);

            List<List<bool>> matchedTiles = new();
            for (int y = 0; y < newBoard.Count; y++)
            {
                matchedTiles.Add(new List<bool>(newBoard[y].Count));
                for (int x = 0; x < newBoard.Count; x++)
                {
                    matchedTiles[y].Add(true);
                }
            }

            //Cleaning the matched tiles
            List<Vector2Int> matchedPosition = new();
            for (int y = 0; y < newBoard.Count; y++)
            {
                for (int x = 0; x < newBoard[y].Count; x++)
                {
                    if (matchedTiles[y][x])
                    {
                        matchedPosition.Add(new Vector2Int(x, y));
                        newBoard[y][x] = new Tile { Id = -1, Type = -1, Action = TileSpecialAction.None };
                    }
                }
            }

            BoardSequence sequence = new()
            {
                MatchedPosition = matchedPosition,
                MovedTiles = null,
                AddedTiles = null
            };

            return sequence;

        }

        private static List<List<bool>> FindMatches(List<List<Tile>> newBoard, LevelData levelData)
        {
            List<List<bool>> matchedTiles = new();
            for (int y = 0; y < newBoard.Count; y++)
            {
                matchedTiles.Add(new List<bool>(newBoard[y].Count));
                for (int x = 0; x < newBoard.Count; x++)
                {
                    matchedTiles[y].Add(false);
                }
            }

            switch (levelData.LevelMechanic)
            {
                case LevelMechanic.Match3:
                    for (int y = 0; y < newBoard.Count; y++)
                    {
                        for (int x = 0; x < newBoard[y].Count; x++)
                        {
                            if (x > 1 &&
                                newBoard[y][x].Type == newBoard[y][x - 1].Type &&
                                newBoard[y][x - 1].Type == newBoard[y][x - 2].Type
                                )
                            {
                                matchedTiles[y][x] = true;
                                matchedTiles[y][x - 1] = true;
                                matchedTiles[y][x - 2] = true;
                            }

                            if (y > 1 &&
                                newBoard[y][x].Type == newBoard[y - 1][x].Type &&
                                newBoard[y - 1][x].Type == newBoard[y - 2][x].Type)
                            {
                                matchedTiles[y][x] = true;
                                matchedTiles[y - 1][x] = true;
                                matchedTiles[y - 2][x] = true;
                            }
                        }
                    }
                    break;
                case LevelMechanic.Match4:
                    for (int y = 0; y < newBoard.Count; y++)
                    {
                        for (int x = 0; x < newBoard[y].Count; x++)
                        {
                            if (x > 2 &&
                                newBoard[y][x].Type == newBoard[y][x - 1].Type &&
                                newBoard[y][x - 1].Type == newBoard[y][x - 2].Type &&
                                newBoard[y][x - 2].Type == newBoard[y][x - 3].Type
                                )
                            {
                                matchedTiles[y][x] = true;
                                matchedTiles[y][x - 1] = true;
                                matchedTiles[y][x - 2] = true;
                                matchedTiles[y][x - 3] = true;
                            }

                            if (y > 2 &&
                                newBoard[y][x].Type == newBoard[y - 1][x].Type &&
                                newBoard[y - 1][x].Type == newBoard[y - 2][x].Type &&
                                newBoard[y - 2][x].Type == newBoard[y - 3][x].Type
                                )
                            {
                                matchedTiles[y][x] = true;
                                matchedTiles[y - 1][x] = true;
                                matchedTiles[y - 2][x] = true;
                                matchedTiles[y - 3][x] = true;
                            }
                        }
                    }
                    break;
                case LevelMechanic.MatchSquared:
                    for (int y = 0; y < newBoard.Count; y++)
                    {
                        for (int x = 0; x < newBoard[y].Count; x++)
                        {
                            if (x > 0 && y > 0 &&
                                newBoard[y][x].Type == newBoard[y][x - 1].Type &&
                                newBoard[y][x].Type == newBoard[y - 1][x].Type &&
                                newBoard[y][x].Type == newBoard[y - 1][x - 1].Type
                                )
                            {
                                matchedTiles[y][x] = true;
                                matchedTiles[y - 1][x] = true;
                                matchedTiles[y][x - 1] = true;
                                matchedTiles[y - 1][x - 1] = true;
                            }
                        }
                    }
                    break;
                default:
                    break;
            }


            return matchedTiles;
        }

        private static bool HasMatch(List<List<bool>> list)
        {
            for (int y = 0; y < list.Count; y++)
            {
                for (int x = 0; x < list[y].Count; x++)
                {
                    if (list[y][x])
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
