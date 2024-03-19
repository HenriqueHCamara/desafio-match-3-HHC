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
            yield return new WaitForSeconds(.1f);

            // Assert that the level SO was successfuly created.
            Assert.AreNotEqual(null, generatedLevelData);
        }

        [UnityTest]
        public IEnumerator ClearLineSpecialActionTest()
        {
            _levelData.TileSpecialActions = new List<TileSpecialAction> { TileSpecialAction.ClearLines };
            _levelData.TileTypes = new List<int> { 1, 2, 3 };
            _gameController.GameLevelsData = new List<LevelData> { _levelData };

            _gameController.GameEngine.BoardTiles[2][0].Type = 1;
            _gameController.GameEngine.BoardTiles[2][1].Type = 1;
            _gameController.GameEngine.BoardTiles[2][2].Type = 1;

            _gameController.GameEngine.BoardTiles[0][2].Type = 1;
            _gameController.GameEngine.BoardTiles[1][2].Type = 1;
            _gameController.GameEngine.BoardTiles[2][2].Type = 1;

            _gameController.GameEngine.BoardTiles[1][1].Type = 1;

            _gameController.GameEngine.BoardTiles[1][1].Action = TileSpecialAction.ClearLines;

            _gameController.OnTileClick(1, 1);
            _gameController.OnTileClick(2, 1);

            yield return new WaitForSeconds(2f);

            int matches = 0;
            for (int i = 0; i < _levelData.LevelBoardSize; i++)
            {
                for (int o = 0; o < _levelData.LevelBoardSize; o++)
                {
                    if(_gameController.GameEngine.MatchedSpecialTilesPosition[i][o] == true) 
                    {
                        matches++;
                    }
                }
            }

            Assert.AreEqual(21, matches);
        }
    }
}
