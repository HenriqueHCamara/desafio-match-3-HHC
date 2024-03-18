using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gazeus.DesafioMatch3
{
    /// <summary>
    /// The class that controlls the Game UI
    /// Here we will update the UI according to the actions that happen in the game.
    /// </summary>
    public class GameInfoView : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _currentPlayerLives;
        [SerializeField] TextMeshProUGUI _playerScore;
        [SerializeField] TextMeshProUGUI _currentLevel;
        [SerializeField] TextMeshProUGUI _levelTargetScore;
        [SerializeField] TextMeshProUGUI _levelMovements;
        [SerializeField] TextMeshProUGUI _playerHighScore;
        [SerializeField] TextMeshProUGUI _levelMatchType;
        [SerializeField] PlayerData _playerData;

        // Start is called before the first frame update
        void Start()
        {
            UpdatePlayerScore();
            UpdatePlayerLives();
        }

        public void UpdatePlayerScore()
        {
            _playerScore.text = _playerData.CurrentPlayerScore.ToString();
            _playerHighScore.text = _playerData.PlayerLeaderboardScore.ToString();
        }

        public void UpdatePlayerLives()
        {
            _currentPlayerLives.text = _playerData.CurrentPlayerLives.ToString();
        }

        public void UpdateMovements(int currentMovements) 
        {
            _levelMovements.text = currentMovements.ToString();
        }

        public void UpdateTargetScore(int currentScore, int targetScore) 
        {
            _levelTargetScore.text = currentScore.ToString() + " / " + targetScore.ToString();
        }

        public void SetLevelInfo(LevelData levelData, int levelIndex)
        {
            _currentLevel.text = (levelIndex + 1).ToString();
            _levelMovements.text = levelData.LevelMaxMovements.ToString();
            _levelTargetScore.text = levelData.LevelTargetPoints.ToString();
            _levelMatchType.text = levelData.LevelMechanic.ToString();
        }
    }
}
