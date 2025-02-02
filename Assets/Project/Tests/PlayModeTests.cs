using System.Collections;
using System.Collections.Generic;
using Gazeus.DesafioMatch3.Controllers;
using Gazeus.DesafioMatch3.Models;
using Gazeus.DesafioMatch3.ScriptableObjects;
using Gazeus.DesafioMatch3.Views;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Gazeus.DesafioMatch3.Test
{
    public class PlayModeTests
    {
        GameController _gameController;
        BoardView _boardView;
        LevelData _levelData;
        PlayerData _playerData;

        [SetUp]
        public void GenerateObjects()
        {
            // mocks the boardView
            _boardView = new GameObject().AddComponent<BoardView>();

            // mocks the boardContainer
            _boardView.BoardContainer = new GameObject().AddComponent<GridLayoutGroup>();

            TilePrefabRepository _tilePrefabRepository = ScriptableObject.CreateInstance<TilePrefabRepository>();
            _tilePrefabRepository.TileTypePrefabList = new GameObject[7]
            {
                new GameObject(),
                new GameObject(),
                new GameObject(),
                new GameObject(),
                new GameObject(),
                new GameObject(),
                new GameObject()
            };
            _boardView.TilePrefabRepository = _tilePrefabRepository;

            TileSpotView _tileSpotPrefab = new GameObject().AddComponent<TileSpotView>();
            _tileSpotPrefab.gameObject.AddComponent<Button>();
            var childObj = new GameObject();
            childObj.AddComponent<Animator>();
            childObj.transform.SetParent(_tileSpotPrefab.transform);
            _boardView.TileSpotPrefab = _tileSpotPrefab;

            // mocks a new level
            _levelData = ScriptableObject.CreateInstance<LevelData>();
            _levelData.LevelBoardSize = 10;
            _levelData.PowerupChance = 100;
            _levelData.LevelMechanic = LevelMechanic.Match3;
            _levelData.LevelMaxMovements = 100;
            _levelData.LevelTargetPoints = 100;
            _levelData.TileSpecialActions = new List<TileSpecialAction> { };
            _levelData.TileTypes = new List<int> { 3, 4, 5, 6 };

            // mocks the controller
            _gameController = new GameObject().AddComponent<GameController>();
            _gameController.GameLevelsData = new List<LevelData> { _levelData };
            _gameController.BoardView = _boardView;

            // mocks the player data
            _playerData = ScriptableObject.CreateInstance<PlayerData>();
            _playerData.CurrentPlayerLives = 10;
            _gameController.PlayerData = _playerData;
        }


        [UnityTest]
        public IEnumerator GenerateNewRandomLevelTest()
        {
            // Test the generation
            LevelData generatedLevelData = _gameController.GenerateRandomLevel();
            yield return null;

            // Assert that the level SO was successfuly created.
            Assert.AreNotEqual(null, generatedLevelData);
        }

        [UnityTest]
        public IEnumerator ClearLineSpecialActionTest()
        {
            _gameController.GameEngine.BoardTiles[1][2].Type = 1;
            _gameController.GameEngine.BoardTiles[2][2].Type = 2;
            _gameController.GameEngine.BoardTiles[3][2].Type = 1;

            _gameController.GameEngine.BoardTiles[2][1].Type = 1;

            _gameController.GameEngine.BoardTiles[2][1].Action = TileSpecialAction.ClearLines;
            _gameController.GameEngine.BoardTiles[2][2].Action = TileSpecialAction.ClearLines;

            _gameController.OnTileClick(1, 2);
            _gameController.OnTileClick(2, 2);

            yield return new WaitUntil(() => !_gameController.IsAnimating);
            yield return null;

            int matches = 0;
            for (int i = 0; i < _levelData.LevelBoardSize; i++)
            {
                for (int o = 0; o < _levelData.LevelBoardSize; o++)
                {
                    if (_gameController.GameEngine.MatchedTilesPosition[i][o] == true)
                    {
                        matches++;
                    }
                }
            }

            Assert.IsTrue(matches >= 19);
        }

        [UnityTest]
        public IEnumerator BombSpecialActionTest()
        {
            _gameController.GameEngine.BoardTiles[1][2].Type = 1;
            _gameController.GameEngine.BoardTiles[2][2].Type = 2;
            _gameController.GameEngine.BoardTiles[3][2].Type = 1;

            _gameController.GameEngine.BoardTiles[2][1].Type = 1;

            _gameController.GameEngine.BoardTiles[2][1].Action = TileSpecialAction.Bomb;
            _gameController.GameEngine.BoardTiles[2][2].Action = TileSpecialAction.Bomb;

            _gameController.OnTileClick(1, 2);
            _gameController.OnTileClick(2, 2);

            yield return new WaitUntil(() => !_gameController.IsAnimating);
            yield return null;

            int matches = 0;
            for (int i = 0; i < _levelData.LevelBoardSize; i++)
            {
                for (int o = 0; o < _levelData.LevelBoardSize; o++)
                {
                    if (_gameController.GameEngine.MatchedTilesPosition[i][o] == true)
                    {
                        matches++;
                    }
                }
            }

            Assert.IsTrue(matches >= 9);
        }

        [UnityTest]
        public IEnumerator TrapSpecialActionTest()
        {
            int currentPlayerLives = _playerData.CurrentPlayerLives;

            _gameController.GameEngine.BoardTiles[1][2].Type = 1;
            _gameController.GameEngine.BoardTiles[2][2].Type = 2;
            _gameController.GameEngine.BoardTiles[3][2].Type = 1;

            _gameController.GameEngine.BoardTiles[2][1].Type = 1;

            _gameController.GameEngine.BoardTiles[2][1].Action = TileSpecialAction.Death;
            _gameController.GameEngine.BoardTiles[2][2].Action = TileSpecialAction.Death;

            _gameController.OnTileClick(1, 2);
            _gameController.OnTileClick(2, 2);

            yield return new WaitUntil(() => !_gameController.IsAnimating);
            yield return null;

            int matches = 0;
            for (int i = 0; i < _levelData.LevelBoardSize; i++)
            {
                for (int o = 0; o < _levelData.LevelBoardSize; o++)
                {
                    if (_gameController.GameEngine.MatchedTilesPosition[i][o] == true)
                    {
                        matches++;
                    }
                }
            }

            Assert.IsTrue(_playerData.CurrentPlayerLives < currentPlayerLives);
        }

        [UnityTest]
        public IEnumerator ColorClearSpecialActionTest() 
        {
            _gameController.GameEngine.BoardTiles[1][2].Type = 1;
            _gameController.GameEngine.BoardTiles[2][2].Type = 2;
            _gameController.GameEngine.BoardTiles[3][2].Type = 1;

            _gameController.GameEngine.BoardTiles[2][1].Type = 1;

            _gameController.GameEngine.BoardTiles[2][1].Action = TileSpecialAction.ColorClear;
            _gameController.GameEngine.BoardTiles[2][2].Action = TileSpecialAction.ColorClear;

            _gameController.GameEngine.BoardTiles[6][6].Type = 1;
            _gameController.GameEngine.BoardTiles[6][7].Type = 1;
            _gameController.GameEngine.BoardTiles[6][8].Type = 1;
            _gameController.GameEngine.BoardTiles[6][9].Type = 1;


            _gameController.OnTileClick(1, 2);
            _gameController.OnTileClick(2, 2);

            yield return new WaitUntil(() => !_gameController.IsAnimating);
            yield return null;

            int matches = 0;
            for (int i = 0; i < _levelData.LevelBoardSize; i++)
            {
                for (int o = 0; o < _levelData.LevelBoardSize; o++)
                {
                    if (_gameController.GameEngine.MatchedTilesPosition[i][o] == true)
                    {
                        matches++;
                    }
                }
            }

            Assert.IsTrue(matches >= 7);
        }

        [UnityTest]
        public IEnumerator GivePlayerScorePointsAfterMatch() 
        {
            _gameController.GameEngine.BoardTiles[1][2].Type = 1;
            _gameController.GameEngine.BoardTiles[2][2].Type = 2;
            _gameController.GameEngine.BoardTiles[3][2].Type = 1;

            _gameController.GameEngine.BoardTiles[2][1].Type = 1;

            _gameController.OnTileClick(1, 2);
            _gameController.OnTileClick(2, 2);

            yield return new WaitUntil(() => !_gameController.IsAnimating);
            yield return null;

            Assert.IsTrue(_playerData.CurrentPlayerScore > 0);
        }

        [UnityTest]
        public IEnumerator Match4SpecialMechanicTest()
        {
            // mocks a new level
            LevelData _match4LevelData = ScriptableObject.CreateInstance<LevelData>();
            _match4LevelData.LevelBoardSize = 10;
            _match4LevelData.PowerupChance = 100;
            _match4LevelData.LevelMechanic = LevelMechanic.Match4;
            _match4LevelData.LevelMaxMovements = 100;
            _match4LevelData.LevelTargetPoints = 100;
            _match4LevelData.TileSpecialActions = new List<TileSpecialAction> { };
            _match4LevelData.TileTypes = new List<int> { 3, 4, 5, 6 };

            // mocks the controller
            _gameController.GameLevelsData.Add(_match4LevelData);
            _gameController.SetupGameForNextLevel();

            yield return new WaitUntil(() => !_gameController.IsLevelComplete);
            yield return new WaitForSeconds(2f);

            _gameController.GameEngine.BoardTiles[1][2].Type = 1;
            _gameController.GameEngine.BoardTiles[2][2].Type = 2;
            _gameController.GameEngine.BoardTiles[3][2].Type = 1;
            _gameController.GameEngine.BoardTiles[4][2].Type = 1;

            _gameController.GameEngine.BoardTiles[2][1].Type = 1;

            _gameController.OnTileClick(1, 2);
            _gameController.OnTileClick(2, 2);

            yield return new WaitUntil(() => !_gameController.IsAnimating);
            yield return null;

            int matches = 0;
            for (int i = 0; i < _levelData.LevelBoardSize; i++)
            {
                for (int o = 0; o < _levelData.LevelBoardSize; o++)
                {
                    if (_gameController.GameEngine.MatchedTilesPosition[i][o] == true)
                    {
                        matches++;
                    }
                }
            }

            Assert.IsTrue(matches >= 4);
        }

        [UnityTest]
        public IEnumerator MatchSquaredSpecialMechanicTest()
        {
            // mocks a new level
            LevelData _match4LevelData = ScriptableObject.CreateInstance<LevelData>();
            _match4LevelData.LevelBoardSize = 10;
            _match4LevelData.PowerupChance = 100;
            _match4LevelData.LevelMechanic = LevelMechanic.MatchSquared;
            _match4LevelData.LevelMaxMovements = 100;
            _match4LevelData.LevelTargetPoints = 100;
            _match4LevelData.TileSpecialActions = new List<TileSpecialAction> { };
            _match4LevelData.TileTypes = new List<int> { 3, 4, 5, 6 };

            // mocks the controller
            _gameController.GameLevelsData.Add(_match4LevelData);
            _gameController.SetupGameForNextLevel();

            yield return new WaitUntil(() => !_gameController.IsLevelComplete);
            yield return new WaitForSeconds(2f);

            _gameController.GameEngine.BoardTiles[1][2].Type = 1;
            _gameController.GameEngine.BoardTiles[1][3].Type = 1;
            _gameController.GameEngine.BoardTiles[2][2].Type = 2;
            _gameController.GameEngine.BoardTiles[2][3].Type = 1;

            _gameController.GameEngine.BoardTiles[2][1].Type = 1;

            _gameController.OnTileClick(1, 2);
            _gameController.OnTileClick(2, 2);

            yield return new WaitUntil(() => !_gameController.IsAnimating);
            yield return null;

            int matches = 0;
            for (int i = 0; i < _levelData.LevelBoardSize; i++)
            {
                for (int o = 0; o < _levelData.LevelBoardSize; o++)
                {
                    if (_gameController.GameEngine.MatchedTilesPosition[i][o] == true)
                    {
                        matches++;
                    }
                }
            }

            Assert.IsTrue(matches >= 4);
        }

        [UnityTest]
        public IEnumerator CompleteCurrentLevelAndGoToNextLevelTest() 
        {
            // mocks a new level
            LevelData _match4LevelData = ScriptableObject.CreateInstance<LevelData>();
            _match4LevelData.LevelBoardSize = 10;
            _match4LevelData.PowerupChance = 100;
            _match4LevelData.LevelMechanic = LevelMechanic.MatchSquared;
            _match4LevelData.LevelMaxMovements = 100;
            _match4LevelData.LevelTargetPoints = 100;
            _match4LevelData.TileSpecialActions = new List<TileSpecialAction> { };
            _match4LevelData.TileTypes = new List<int> { 3, 4, 5, 6 };

            // mocks the controller
            _gameController.GameLevelsData.Add(_match4LevelData);
            _gameController.SetupGameForNextLevel();

            yield return new WaitUntil(() => !_gameController.IsLevelComplete);
            yield return new WaitForSeconds(2f);

            Assert.AreNotEqual(_gameController.GameEngine.CurrentLevelData, _gameController.GameLevelsData[0]);
        }
    }
}
