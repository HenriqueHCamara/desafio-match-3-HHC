using Gazeus.DesafioMatch3.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gazeus.DesafioMatch3
{
    public enum LevelMechanic { Match3, Match4, MatchSquared }

    [CreateAssetMenu(fileName = "Level", menuName = "Data/LevelData")]

    public class LevelData : ScriptableObject
    {
        /// <summary>
        /// Utilizamos um SO para armazenar os dados de cada nível pois assim podemos facilmente criar mais níveis sem necessitar alteração no código;
        /// Alem de podermos ter acesso aos dados do nível sem gerar dependencia entre classes.
        /// </summary>

        [SerializeField] int _LevelTargetPoints = 20;
        [SerializeField] int _levelMaxMovements = 10;
        [SerializeField, Range(5, 10), Tooltip("Width X Height")] int _levelBoardSize = 5;
        [SerializeField] List<int> _tileTypes = new List<int>() { 1, 2, 3, 4, 5, 6 };
        [SerializeField, Range(0, 100)] int _powerupChance = 25;
        [SerializeField] List<TileSpecialAction> _tileSpecialActions;
        [SerializeField] LevelMechanic _levelMechanic = LevelMechanic.Match3;

        public int LevelMaxMovements { get => _levelMaxMovements; set => _levelMaxMovements = value; }
        public int LevelTargetPoints { get => _LevelTargetPoints; set => _LevelTargetPoints = value; }
        public int LevelBoardSize { get => _levelBoardSize; set => _levelBoardSize = value; }
        public List<int> TileTypes { get => _tileTypes; set => _tileTypes = value; }
        public int PowerupChance { get => _powerupChance; set => _powerupChance = value; }
        public List<TileSpecialAction> TileSpecialActions { get => _tileSpecialActions; set => _tileSpecialActions = value; }
        public LevelMechanic LevelMechanic { get => _levelMechanic; set => _levelMechanic = value; }
    }
}
