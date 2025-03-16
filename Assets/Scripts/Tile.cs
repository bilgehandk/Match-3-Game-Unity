using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class Tile : MonoBehaviour
{
    public int x;
    public int y;

    private Item item;

    public Item Item
    {
        get => item;

        set
        {
            if (item == value)
                return;

            item = value;
            icon.sprite = item.sprite;
        }
    }
    
    public Image icon;
    public Button button;

    public Tile Left => x > 0 ? Board.Instance.tiles[x - 1, y] : null;
    public Tile Right => x < Board.Instance.width - 1 ? Board.Instance.tiles[x + 1, y] : null;
    public Tile Top => y > 0 ? Board.Instance.tiles[x, y - 1] : null;
    public Tile Bottom => y < Board.Instance.height - 1 ? Board.Instance.tiles[x, y + 1] : null;

    public Tile[] Neighbors => new[]
    {
        Left,
        Right,
        Top,
        Bottom,
    };
    

    private void Start()
    {
        button.onClick.AddListener(() => Board.Instance.Select(this));
    }
    
    public void SetButtonColor(Color color)
    {
        var colors = button.colors;
        colors.normalColor = color;
        button.colors = colors;
    }

    public List<Tile> GetConnectedTiles(HashSet<Tile> exclude = null)
    {
        var result = new List<Tile> { this };

        if (exclude == null)
        {
            exclude = new HashSet<Tile> { this };
        }
        else
        {
            exclude.Add(this);
        }

        foreach (var neighbor in Neighbors)
        {
            if (neighbor == null || exclude.Contains(neighbor) || neighbor.Item != Item) continue;

            result.AddRange(neighbor.GetConnectedTiles(exclude));
        }

        return result;
    }

}
