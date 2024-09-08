using UnityEngine;

public class Field : MonoBehaviour
{
    [SerializeField]
    Tile[] tiles;

    private void Start()
    {
        if (tiles == null)
        {
            tiles = GetComponentsInChildren<Tile>();
            System.Array.Sort(tiles, (tile1, tile2) => tile1.TileIndex.CompareTo(tile2.TileIndex));
        }
    }

    [SerializeField]
    int seatNum;
    public int SeatNum => seatNum;

    public Tile this[int index]
    {
        get
        {
            if (index < 0 || index >= tiles.Length)
            {
                Debug.LogWarning($"Invalid index {index}. Returning null.");
                return null;
            }
            return tiles[index];
        }
    }
}
