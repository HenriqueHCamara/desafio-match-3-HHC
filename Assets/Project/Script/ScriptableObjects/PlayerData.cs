using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gazeus.DesafioMatch3
{
    [CreateAssetMenu(fileName = "PlayerData", menuName = "Data/PlayerData")]
    public class PlayerData : ScriptableObject
    {
        /// <summary>
        /// Utilizamos um SO para armazenar os dados do jogador pois assim podemos facilmente criar mais níveis sem necessitar alteração no código;
        /// Alem de podermos ter acesso aos dados do nível sem gerar dependencia entre classes e ter essa informação persistente entre os níveis.
        /// </summary>

        [SerializeField] int _maxPlayerLives;
        int _currentPlayerScore;
        int _playerLeaderboardScore;
        int _currentPlayerLives;

        public int CurrentPlayerScore { get => _currentPlayerScore; set => _currentPlayerScore = value; }
        public int CurrentPlayerLives { get => _currentPlayerLives; set => _currentPlayerLives = value; }
        public int PlayerLeaderboardScore { get => _playerLeaderboardScore; set => _playerLeaderboardScore = value; }

        private void OnEnable()
        {
            ResetPlayerData(); // At the start of every game, we reset the player data.
            _playerLeaderboardScore = SaveSystem.LoadHighScore(); // At the start of the game, we get the saved player highscore.
        }

        public void ResetPlayerData()
        {
            _currentPlayerScore = 0;
            _currentPlayerLives = _maxPlayerLives;
        }
    }
}
