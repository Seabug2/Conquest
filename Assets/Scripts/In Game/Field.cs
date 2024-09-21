using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Field : MonoBehaviour
{
    [SerializeField] int seatNum;

    readonly Dictionary<int, Tile> dict_Tile = new();

    public Tile this[int key]
    {
        get
        {
            if (!dict_Tile.ContainsKey(key))
            {
                Debug.LogWarning($"Invalid index {key}. Returning null.");
                return null;
            }
            return dict_Tile[key];
        }
    }

    private void Awake()
    {
        TileSet();
    }

    void TileSet()
    {
        Tile[] tiles = GetComponentsInChildren<Tile>();

        foreach (Tile t in tiles)
        {
            dict_Tile.Add(t.TileIndex, t);
        }

        for (int i = 0; i < dict_Tile.Count; i++)
        {
            int n = i % 5;

            if (n != 0)
            {
                if (i - 3 >= 0)
                    dict_Tile[i].linkedTile[0] = dict_Tile[i - 3];

                if (i + 2 < dict_Tile.Count)
                    dict_Tile[i].linkedTile[3] = dict_Tile[i + 2];
            }

            if (n != 2)
            {
                if (i - 2 >= 0)
                    dict_Tile[i].linkedTile[1] = dict_Tile[i - 2];

                if (i + 3 < dict_Tile.Count)
                    dict_Tile[i].linkedTile[2] = dict_Tile[i + 3];
            }
        }

        foreach (Tile t in tiles)
        {
            t.SetSocket();
        }
    }

    private void Start()
    {
        GameManager.dict_Field.Add(seatNum, this);
    }

    public void ShowPlaceableTiles(Card _card, bool _isActive)
    {
        foreach (Tile t in dict_Tile.Values)
        {
            t.ShowPlaceableTiles(_card, _isActive);
        }
    }
}