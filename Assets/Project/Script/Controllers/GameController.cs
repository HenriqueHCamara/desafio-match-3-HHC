using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Gazeus.DesafioMatch3.Core;
using Gazeus.DesafioMatch3.Models;
using Gazeus.DesafioMatch3.Views;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UIElements;

namespace Gazeus.DesafioMatch3.Controllers
{
    public class GameController : MonoBehaviour
    {
        [SerializeField] private BoardView _boardView;
        [SerializeField] private GameInfoView _gameInfoView;
        [SerializeField] private PlayerData _playerData;
        [SerializeField] private List<LevelData> _gameLevelsData;

        private int _boardHeight = 10;
        private int _boardWidth = 10;
        private GameService _gameEngine;
        private bool _isAnimating;
        private int _selectedX = -1;
        private int _selectedY = -1;
        private Coroutine hintCoroutine;
        private int _currentLevelIndexer = 0;
        private int _remainingMovements;
        private bool _isLevelComplete = false;
        private int _currentLevelScore = 0;

        public List<LevelData> GameLevelsData { get => _gameLevelsData; set => _gameLevelsData = value; }
        public BoardView BoardView { get => _boardView; set => _boardView = value; }
        public GameService GameEngine { get => _gameEngine; set => _gameEngine = value; }
        public PlayerData PlayerData { get => _playerData; set => _playerData = value; }

        #region Unity
        private void Awake()
        {
            _gameEngine = new GameService();
            _gameEngine.onSpecialTileDeathEvent += TakeALifeFromPlayer;
            if (_boardView)
                _boardView.TileClicked += OnTileClick;
        }

        private void OnDestroy()
        {
            _gameEngine.onSpecialTileDeathEvent -= TakeALifeFromPlayer;
            if (_boardView)
                _boardView.TileClicked -= OnTileClick;
        }

        private void Start()
        {
            StartNewLevel();
        }
        #endregion

        private void StartNewLevel()
        {
            if (_gameLevelsData.Count > 0)
            {
                SetLevelInfo(_currentLevelIndexer);
                _boardHeight = _gameLevelsData[_currentLevelIndexer].LevelBoardSize;
                _boardWidth = _gameLevelsData[_currentLevelIndexer].LevelBoardSize;
                _remainingMovements = _gameLevelsData[_currentLevelIndexer].LevelMaxMovements;
                _isLevelComplete = false;
                List<List<Tile>> board = _gameEngine.StartGame(_gameLevelsData[_currentLevelIndexer]);
                _boardView.CreateBoard(board);
                hintCoroutine = StartCoroutine(GiveMovementHint());
            }
        }

        private void AnimateBoard(List<BoardSequence> boardSequences, int index, Action onComplete)
        {
            BoardSequence boardSequence = boardSequences[index];

            Sequence sequence = DOTween.Sequence();
            sequence.Append(_boardView.DestroyTiles(boardSequence.MatchedPosition));
            sequence.Append(_boardView.MoveTiles(boardSequence.MovedTiles));
            sequence.Append(_boardView.CreateTile(boardSequence.AddedTiles));

            index += 1;

            _playerData.CurrentPlayerScore += boardSequence.MatchedPosition.Count; // We add the score to the player data.
            _currentLevelScore += boardSequence.MatchedPosition.Count; // We add the score to the level current score
            if (_gameInfoView) _gameInfoView.UpdateTargetScore(_currentLevelScore, _gameLevelsData[_currentLevelIndexer].LevelTargetPoints); // We update the UI to show the player how much progress he made in this turn.

            OnTileSwapAddpointsCounter(); // For the player total points made during the Swaping Round,
                                          // we add points based on the number of matched tiles,
                                          // calling our view to display the increasing counter by played sequence.

            if (index < boardSequences.Count)
            {
                sequence.onComplete += () => AnimateBoard(boardSequences, index, onComplete);
            }
            else
            {
                sequence.onComplete += () => onComplete();
            }
        }

        private void DestroyBoard(BoardSequence destructionSequence, Action onComplete)
        {
            BoardSequence boardSequence = destructionSequence;

            Sequence sequence = DOTween.Sequence();
            sequence.Append(_boardView.DestroyTiles(boardSequence.MatchedPosition)); // We destroy all the remaining Tiles.

            _playerData.CurrentPlayerScore += boardSequence.MatchedPosition.Count; // We add the score to the player data.

            sequence.onComplete += () => onComplete();
        }

        public void OnTileClick(int x, int y)
        {
            if (_isAnimating) return;

            if (_selectedX > -1 && _selectedY > -1)
            {
                if (Mathf.Abs(_selectedX - x) + Mathf.Abs(_selectedY - y) > 1)
                {
                    _selectedX = -1;
                    _selectedY = -1;
                }
                else
                {
                    _isAnimating = true;
                    _boardView.SwapTiles(_selectedX, _selectedY, x, y).onComplete += () =>
                    {
                        bool isValid = _gameEngine.IsValidMovement(_selectedX, _selectedY, x, y);
                        if (isValid)
                        {
                            if (hintCoroutine != null)
                            {
                                StopCoroutine(hintCoroutine); // If the player makes a valid move, we stop giving the hint.
                                hintCoroutine = null;
                                _boardView.StopHintAnimation();
                            }

                            _remainingMovements--; // if it is a valid movement, we decrease the remaining movements the player can make.
                            if (_gameInfoView) _gameInfoView.UpdateMovements(_remainingMovements);

                            List<BoardSequence> swapResult = _gameEngine.SwapTile(_selectedX, _selectedY, x, y);
                            AnimateBoard(swapResult, 0, () =>
                            {
                                _isAnimating = false;

                                // After the board animation is concluded, we check if the player has finished, or lost the level.
                                if (_currentLevelScore >= _gameLevelsData[_currentLevelIndexer].LevelTargetPoints)
                                {
                                    SetupGameForNextLevel();
                                }
                                else
                                {
                                    hintCoroutine = StartCoroutine("GiveMovementHint");
                                    if (_remainingMovements == 0)
                                    {
                                        // If there are no remaining movements, we take a life from the player and we go to the next level.
                                        TakeALifeFromPlayer();
                                        if (_playerData.CurrentPlayerLives == 0)
                                            GameOver();
                                        else
                                            SetupGameForNextLevel();
                                        return;
                                    }
                                    else if (_playerData.CurrentPlayerLives == 0)
                                        GameOver();
                                }
                            });
                        }
                        else
                        {
                            _boardView.SwapTiles(x, y, _selectedX, _selectedY).onComplete += () => _isAnimating = false;
                        }
                        _selectedX = -1;
                        _selectedY = -1;
                    };
                }
            }
            else
            {
                _selectedX = x;
                _selectedY = y;
            }
        }

        /// <summary>
        /// If the player couldnt complete the level, we give him a game over, saving his score (if he got past his old max score) and restart the game from scratch
        /// </summary>
        private void GameOver()
        {
            if (_playerData.CurrentPlayerScore > _playerData.PlayerLeaderboardScore) // We save the player score to a file if he loses the game
            {
                SaveSystem.SaveHighScore(_playerData.CurrentPlayerScore);
            }

            // If the player loses the game, we save his highscore and start anew from the begining.
            BoardSequence destroySequence = _gameEngine.ClearBoard();
            DestroyBoard(destroySequence, () =>
            {
                // We setup the next level.
                _boardView.DestroyBoard(_gameLevelsData[_currentLevelIndexer].LevelBoardSize);
                _currentLevelIndexer = 0;
                _currentLevelScore = 0;
                _playerData.ResetPlayerData();
                if (_gameInfoView)
                {
                    _gameInfoView.UpdatePlayerLives();
                    _gameInfoView.UpdatePlayerScore();
                }
                StartNewLevel();
            });

        }

        /// <summary>
        /// We clean the board and load the next level
        /// </summary>
        private void SetupGameForNextLevel()
        {
            _isLevelComplete = true;
            BoardSequence destroySequence = _gameEngine.ClearBoard();
            DestroyBoard(destroySequence, () =>
            {
                // We setup the next level.
                _boardView.DestroyBoard(_gameLevelsData[_currentLevelIndexer].LevelBoardSize);
                _currentLevelIndexer++;
                if (_currentLevelIndexer >= _gameLevelsData.Count)
                    _gameLevelsData.Add(GenerateRandomLevel()); // If there are no more premade levels, we generate a new one.

                if (_playerData.CurrentPlayerLives == 0) // if the player lost a live in his last move, but concluded the level, we give him a life as a gift
                    _playerData.CurrentPlayerLives++;

                _currentLevelScore = 0;
                StartNewLevel();
            });
        }

        /// <summary>
        /// Creates a new level data SO with randomized parameters.
        /// </summary>
        /// <returns> The new randomized level data </returns>
        public LevelData GenerateRandomLevel()
        {
            LevelData newGeneratedLevel = ScriptableObject.CreateInstance<LevelData>();

            newGeneratedLevel.PowerupChance = UnityEngine.Random.Range(10, 66);
            newGeneratedLevel.LevelMaxMovements = UnityEngine.Random.Range(15, 36);
            newGeneratedLevel.LevelTargetPoints = UnityEngine.Random.Range(100, 401);
            newGeneratedLevel.LevelBoardSize = UnityEngine.Random.Range(5, 11);
            newGeneratedLevel.TileTypes = new List<int> { 1, 2, 3 };
            for (int i = 4; i < 7; i++)
            {
                if (UnityEngine.Random.Range(0, 100) > 50)
                    newGeneratedLevel.TileTypes.Add(i);
            }
            newGeneratedLevel.TileSpecialActions = new List<TileSpecialAction>();
            if (newGeneratedLevel.PowerupChance > 0)
            {
                for (int i = 1; i < Enum.GetNames(typeof(TileSpecialAction)).Length; i++)
                {
                    if (UnityEngine.Random.Range(0, 100) > 50)
                    {
                        newGeneratedLevel.TileSpecialActions.Add((TileSpecialAction)i);
                    }
                }
            }

            if (newGeneratedLevel.LevelBoardSize > 6)
            {
                newGeneratedLevel.LevelMechanic = (LevelMechanic)UnityEngine.Random.Range(0, Enum.GetNames(typeof(LevelMechanic)).Length);
            }

            return newGeneratedLevel;
        }

        /// <summary>
        /// After some given time, gives the player a visual feedback of a possible combination to be made
        /// </summary>
        public IEnumerator GiveMovementHint()
        {
            yield return new WaitForSeconds(2f); // Waits a while to show a hint

            _boardView.AnimateHint(FindMatchesInLoop());
        }

        /// <summary>
        /// Searches for the first valid match in the board 
        /// </summary>
        /// <returns> return the checked Tile and The Tile that has a valid swap positions</returns>
        private int[] FindMatchesInLoop()
        {
            for (int a = 0; a < _boardHeight; a++)
            {
                for (int b = 0; b < _boardWidth; b++)
                {
                    int testingTileX = b;
                    int testingTileY = a;

                    for (int y = -1; y <= 1; y++)
                    {
                        for (int x = -1; x <= 1; x++)
                        {
                            int neighborTileY = testingTileY + y;
                            int neighborTileX = x + testingTileX;

                            if (Mathf.Abs(testingTileX - neighborTileX) + Mathf.Abs(testingTileY - neighborTileY) > 1)
                                continue;

                            /// Check if the neighbor indices are within bounds
                            if (neighborTileY >= 0 && neighborTileY < _boardHeight &&
                        neighborTileX >= 0 && neighborTileX < _boardWidth &&
                            (neighborTileY != testingTileY || neighborTileX != testingTileX))
                            {
                                if (_gameEngine.IsValidMovement(testingTileX, testingTileY, neighborTileX, neighborTileY))
                                {
                                    return new int[] { testingTileY, testingTileX, neighborTileY, neighborTileX }; // Stop looking after first Match
                                }
                            }
                        }
                    }
                }
            }

            // If there are no valid movements, the game soft-locked, so we give it as a win to the player and we go to the next level
            _playerData.CurrentPlayerScore += _gameLevelsData[_currentLevelIndexer].LevelTargetPoints - _currentLevelScore; // we give the player the remaining level points
            if (_gameInfoView) _gameInfoView.UpdatePlayerScore();
            SetupGameForNextLevel(); // and we go to the next level
            return null;
        }


        /// <summary>
        /// We call our GameInfoView to update the player score graphic
        /// </summary>
        private void OnTileSwapAddpointsCounter()
        {
            if (_gameInfoView) _gameInfoView.UpdatePlayerScore();
        }

        /// <summary>
        /// We take a life from the player and update the UI
        /// </summary>
        private void TakeALifeFromPlayer()
        {
            _playerData.CurrentPlayerLives -= 1;
            if (_gameInfoView) _gameInfoView.UpdatePlayerLives();
        }

        /// <summary>
        /// We update the UI with the level information
        /// </summary>
        /// <param name="levelIndex"> The level index, wich comes from the order of the level list </param>
        private void SetLevelInfo(int levelIndex)
        {
            if (_gameInfoView) _gameInfoView.SetLevelInfo(_gameLevelsData[levelIndex], levelIndex);
        }

        private void OnApplicationQuit()
        {
            if (_playerData)
            {
                if (_playerData.CurrentPlayerScore > _playerData.PlayerLeaderboardScore) // We save the player score to a file if he exits the game
                {
                    SaveSystem.SaveHighScore(_playerData.CurrentPlayerScore);
                }
            }
        }
    }
}
