using UnityEngine;

namespace Gazeus.DesafioMatch3.ScriptableObjects
{
    [CreateAssetMenu(fileName = "TilePrefabRepository", menuName = "Gameplay/TilePrefabRepository")]
    public class TilePrefabRepository : ScriptableObject
    {
        [SerializeField] private GameObject[] _tileTypePrefabList;
        [SerializeField] private Sprite[] _tileSpecialImagePrefabList;

        public GameObject[] TileTypePrefabList => _tileTypePrefabList;
        public Sprite[] TileSpecialImagePrefabList => _tileSpecialImagePrefabList; // the list of Special action images, must correspond to the Special Action Enum.
    }
}
