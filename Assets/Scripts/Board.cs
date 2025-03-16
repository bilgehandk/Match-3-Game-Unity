using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEditor.VersionControl;
using UnityEngine;
using Random = UnityEngine.Random;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

public sealed class Board : MonoBehaviour
{
    public static Board Instance { get; private set; }

    [SerializeField] private AudioClip collectionSound;

    [SerializeField] private AudioSource audioSource;

    public Row[] rows;

    public Tile[,] tiles { get; private set; }

    public int width => tiles.GetLength(0);
    public int height => tiles.GetLength(1);

    private const float TweenDuration = 0.25f; 

    private List<Tile> _selection = new List<Tile>();

    public void Awake()
    {
        Instance = this;
        DOTween.SetTweensCapacity(1250, 50);
    }

    private async void Start()
    {
        await ItemManager.Initialize();
        tiles = new Tile[rows.Max(row => row.tiles.Length), rows.Length];

        for (var y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var tile = rows[y].tiles[x];

                tile.x = x;
                tile.y = y;
                
                tile.Item = ItemManager.Items[Random.Range(0, ItemManager.Items.Count)];

                tiles[x, y] = tile;
            }
        }
    }

    private void Update()
    {
        if (tiles == null || tiles[0, 0] == null) return;
    }

    public async void Select(Tile tile)
    {
        if (_selection.Contains(tile))
            return;

        if (_selection.Count == 0)
        {
            _selection.Add(tile);
            tile.SetButtonColor(Color.cyan); // Seçilen tile'ın rengini açık mavi yap
        }
        else if (_selection.Count == 1)
        {
            if (Array.IndexOf(_selection[0].Neighbors, tile) != -1)
            {
                _selection.Add(tile);
                tile.SetButtonColor(Color.cyan); // Seçilen tile'ın rengini açık mavi yap
            }
            else
            {
                _selection[0].SetButtonColor(Color.white); // Rengi sıfırla
                _selection.Clear();
                _selection.Add(tile);
                tile.SetButtonColor(Color.cyan); // Seçilen tile'ın rengini açık mavi yap
            }
        }

        if (_selection.Count < 2) return;

        Debug.Log($"Selected Tiles at ({_selection[0].x}, {_selection[0].y}) and ({_selection[1].x}, {_selection[1].y})");

        await Swap(_selection[0], _selection[1]);

        if (CanPop())
        {
            Pop();
        }
        else
        {
            await Swap(_selection[0], _selection[1]);
        }

        _selection[0].SetButtonColor(Color.white); // Rengi sıfırla
        _selection[1].SetButtonColor(Color.white); // Rengi sıfırla
        _selection.Clear();
    }

    public async Task Swap(Tile tile1, Tile tile2)
    {
        var icon1 = tile1.icon;
        var icon2 = tile2.icon;

        var icon1Transform = icon1.transform;
        var icon2Transform = icon2.transform;

        var sequence = DOTween.Sequence();

        sequence.Join(icon1Transform.DOMove(icon2Transform.position, TweenDuration))
            .Join(icon2Transform.DOMove(icon1Transform.position, TweenDuration));

        await sequence.Play().AsyncWaitForCompletion();
    
        icon1Transform.SetParent(tile2.transform);
        icon2Transform.SetParent(tile1.transform);

        tile1.icon = icon2;
        tile2.icon = icon1;

        (tile1.Item, tile2.Item) = (tile2.Item, tile1.Item);
    }

    private bool CanPop()
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (tiles[x, y].GetConnectedTiles().Skip(1).Count() >= 2)
                    return true;
            }
        }

        return false;
    }

    private async void Pop()
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var tile = tiles[x, y];

                var connectedTiles = tile.GetConnectedTiles();

                if (connectedTiles.Skip(1).Count() < 2)
                    continue;

                var deflateSequence = DOTween.Sequence();

                foreach (var connectedTile in connectedTiles)
                    deflateSequence.Join(connectedTile.icon.transform.DOScale(Vector3.zero, TweenDuration));
                
                ScoreCounter.Instance.Score += tile.Item.value * connectedTiles.Count;
                audioSource.PlayOneShot(collectionSound);

                await deflateSequence.Play().AsyncWaitForCompletion();
                
                var inflateSequence = DOTween.Sequence();

                foreach (var connectedTile in connectedTiles)
                {
                    connectedTile.Item = ItemManager.Items[Random.Range(0, ItemManager.Items.Count)];

                    inflateSequence.Join(connectedTile.icon.transform.DOScale(Vector3.one, TweenDuration));
                }

                await inflateSequence.Play().AsyncWaitForCompletion();

                x = 0;
                y = 0;
            }
        }
    }
}
