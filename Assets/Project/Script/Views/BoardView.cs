using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Gazeus.DesafioMatch3.Models;
using Gazeus.DesafioMatch3.ScriptableObjects;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace Gazeus.DesafioMatch3.Views
{
    public class BoardView : MonoBehaviour
    {
        public event Action<int, int> TileClicked;

        [SerializeField] private GridLayoutGroup _boardContainer;
        [SerializeField] private TilePrefabRepository _tilePrefabRepository;
        [SerializeField] private TileSpotView _tileSpotPrefab;

        private GameObject[][] _tiles;
        private TileSpotView[][] _tileSpots;
        private List<Animator> _hintAnimators = new List<Animator>();

        public GridLayoutGroup BoardContainer { get => _boardContainer; set => _boardContainer = value; }
        public TilePrefabRepository TilePrefabRepository { get => _tilePrefabRepository; set => _tilePrefabRepository = value; }
        public TileSpotView TileSpotPrefab { get => _tileSpotPrefab; set => _tileSpotPrefab = value; }

        public void CreateBoard(List<List<Tile>> board)
        {
            _boardContainer.constraintCount = board[0].Count;
            _tiles = new GameObject[board.Count][];
            _tileSpots = new TileSpotView[board.Count][];

            for (int y = 0; y < board.Count; y++)
            {
                _tiles[y] = new GameObject[board[0].Count];
                _tileSpots[y] = new TileSpotView[board[0].Count];

                for (int x = 0; x < board[0].Count; x++)
                {
                    TileSpotView tileSpot = Instantiate(_tileSpotPrefab);
                    tileSpot.transform.SetParent(_boardContainer.transform, false);
                    tileSpot.SetPosition(x, y);
                    tileSpot.Clicked += TileSpot_Clicked;

                    _tileSpots[y][x] = tileSpot;

                    int tileTypeIndex = board[y][x].Type;
                    if (tileTypeIndex > -1)
                    {
                        GameObject tilePrefab = _tilePrefabRepository.TileTypePrefabList[tileTypeIndex];
                        GameObject tile = Instantiate(tilePrefab);
                        if (tile.transform.childCount > 0)
                            tile.transform.GetChild(0).GetComponent<UnityEngine.UI.Image>().sprite = _tilePrefabRepository.TileSpecialImagePrefabList[(int)board[y][x].Action]; // If the Tile has an Special Action, we give it the aprropriate sprite.
                        tileSpot.SetTile(tile);

                        _tiles[y][x] = tile;
                    }
                }
            }
        }

        public void DestroyBoard(int boardSize)
        {
            for (int y = 0; y < boardSize; y++)
            {
                for (int x = 0; x < boardSize; x++)
                {
                    TileSpotView tileSpot = _tileSpots[y][x];

                    Destroy(tileSpot.gameObject);
                    _tileSpots[y][x] = null;
                }
            }
        }

        public Tween CreateTile(List<AddedTileInfo> addedTiles)
        {
            Sequence sequence = DOTween.Sequence();
            for (int i = 0; i < addedTiles.Count; i++)
            {
                AddedTileInfo addedTileInfo = addedTiles[i];
                Vector2Int position = addedTileInfo.Position;

                TileSpotView tileSpot = _tileSpots[position.y][position.x];

                GameObject tilePrefab = _tilePrefabRepository.TileTypePrefabList[addedTileInfo.Type];
                GameObject tile = Instantiate(tilePrefab);
                tile.transform.GetChild(0).GetComponent<UnityEngine.UI.Image>().sprite = _tilePrefabRepository.TileSpecialImagePrefabList[(int)addedTileInfo.SpecialAction];

                tileSpot.SetTile(tile);

                _tiles[position.y][position.x] = tile;

                tile.transform.localScale = Vector2.zero;
                sequence.Join(tile.transform.DOScale(1.0f, 0.2f));
            }

            return sequence;
        }

        public Tween DestroyTiles(List<Vector2Int> matchedPosition)
        {
            Sequence sequence = DOTween.Sequence();

            for (int i = 0; i < matchedPosition.Count; i++)
            {
                Vector2Int position = matchedPosition[i];
                _tileSpots[position.y][position.x].transform.GetChild(0).GetComponent<Animator>().SetTrigger("Explode");

                _tiles[position.y][position.x].transform.localScale = Vector2.one;
                GameObject tileToDestroy = _tiles[position.y][position.x];
                sequence.Join(_tiles[position.y][position.x].transform.DOScale(0.0f, 0.2f).OnComplete(() =>
                {
                    Destroy(tileToDestroy);
                }));
                _tiles[position.y][position.x] = null;

            }

            return DOVirtual.DelayedCall(0.2f, () => { });
        }

        public Tween MoveTiles(List<MovedTileInfo> movedTiles)
        {
            GameObject[][] tiles = new GameObject[_tiles.Length][];
            for (int y = 0; y < _tiles.Length; y++)
            {
                tiles[y] = new GameObject[_tiles[y].Length];
                for (int x = 0; x < _tiles[y].Length; x++)
                {
                    tiles[y][x] = _tiles[y][x];
                }
            }

            Sequence sequence = DOTween.Sequence();
            for (int i = 0; i < movedTiles.Count; i++)
            {
                MovedTileInfo movedTileInfo = movedTiles[i];

                Vector2Int from = movedTileInfo.From;
                Vector2Int to = movedTileInfo.To;

                sequence.Join(_tileSpots[to.y][to.x].AnimatedSetTile(_tiles[from.y][from.x]));

                tiles[to.y][to.x] = _tiles[from.y][from.x];
            }

            _tiles = tiles;

            return sequence;
        }

        public Tween SwapTiles(int fromX, int fromY, int toX, int toY)
        {
            Sequence sequence = DOTween.Sequence();
            sequence.Append(_tileSpots[fromY][fromX].AnimatedSetTile(_tiles[toY][toX]));
            sequence.Join(_tileSpots[toY][toX].AnimatedSetTile(_tiles[fromY][fromX]));

            (_tiles[toY][toX], _tiles[fromY][fromX]) = (_tiles[fromY][fromX], _tiles[toY][toX]);

            return sequence;
        }

        /// <summary>
        /// Plays a Hint animation on the tile spot child object
        /// </summary>
        /// <param name="positions"> A vector containing the two tiles positions, the checked tile and the pair tile </param>
        public void AnimateHint(int[] positions)
        {
            if (positions != null)
            {
                Animator checkedTile = _tileSpots[positions[0]][positions[1]].transform.GetChild(0).GetComponent<Animator>();
                checkedTile.Play("Base Layer.HintAnim", 0);
                _hintAnimators.Add(checkedTile);

                Animator swapPairTile = _tileSpots[positions[2]][positions[3]].transform.GetChild(0).GetComponent<Animator>();
                swapPairTile.Play("Base Layer.HintAnim", 0);
                _hintAnimators.Add(swapPairTile);
            }
        }

        /// <summary>
        /// We set the animator of each tile that had it's hint animation playing to the idle animation
        /// </summary>
        public void StopHintAnimation()
        {
            foreach (var item in _hintAnimators)
            {
                item.Play("Base Layer.Idle", 0);
            }

            _hintAnimators.Clear();
        }

        #region Events
        private void TileSpot_Clicked(int x, int y)
        {
            TileClicked(x, y);
        }
        #endregion
    }
}
