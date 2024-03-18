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
            _boardView.TileSpotPrefab = _tileSpotPrefab;

            // mocks a new level
            LevelData _levelData = ScriptableObject.CreateInstance<LevelData>();
            _levelData.LevelBoardSize = 10;
            _levelData.PowerupChance = 99;
            _levelData.LevelMechanic = LevelMechanic.Match3;
            _levelData.LevelMaxMovements = 100;
            _levelData.LevelTargetPoints = 100;
            _levelData.TileSpecialActions = new List<TileSpecialAction>();


            // mocks the controller
            _gameController = new GameObject().AddComponent<GameController>();
            _gameController.GameLevelsData = new List<LevelData> { _levelData };
            _gameController.BoardView = _boardView;
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
    }
}
