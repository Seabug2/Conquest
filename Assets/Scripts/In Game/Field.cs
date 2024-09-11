using UnityEngine;

public class Field : MonoBehaviour
{
    [SerializeField]
    int seatNum;
    public int SeatNum => seatNum;

    [SerializeField]
    Tile[] tiles;

    public Card GetCard(int index)
    {
        if (index < 0 || index >= tiles.Length)
        {
            Debug.Log("범위 오류");
            return null;
        }

        if (tiles[index].placedCard == null)
        {
            Debug.Log("카드가 배치되지 않았습니다.");
            return null;
        }

        return tiles[index].placedCard;
    }

    public bool IsEmpty(int index)
    {
        if (index < 0 || index >= tiles.Length)
        {
            Debug.Log("범위 오류");
            return true;
        }

        return tiles[index].placedCard == null;
    }

    private void Start()
    {
        SocketSetting();

        //if (tiles == null)
        //{
        //    tiles = GetComponentsInChildren<Tile>();
        //    System.Array.Sort(tiles, (tile1, tile2) => tile1.TileIndex.CompareTo(tile2.TileIndex));
        //}
    }

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

    void SocketSetting()
    {
        //모든 타일을 검사
        for (int i = 0; i < tiles.Length; i++)
        {
            Tile t = tiles[i];

            //타일의 모든 소켓을 확인
            for (int j = 0; j < 4; j++)
            {
                if (t.linkableSocket[j]) //각 소켓이 연결가능한 상태라면...
                {
                    // 0     1
                    //
                    //
                    // 3     2
                    switch (j)
                    {
                        case 0:
                            //"0번째 소켓"은
                            //"인덱스 - 3 번째 카드"의
                            //"2번째 소켓"과 연결되어 있음
                            if (tiles[i - 3][2] == null)
                            {
                                t[0] = new Socket();
                            }
                            else
                            {
                                t[0] = tiles[i - 3][2];
                            }
                            break;

                        case 1:
                            //"1번째 소켓"은
                            //"인덱스 - 2 번째 카드"의
                            //"3번째 소켓"과 연결되어 있음
                            if (tiles[i - 2][3] == null)
                            {
                                t[1] = new Socket();
                            }
                            else
                            {
                                t[1] = tiles[i - 3][3];
                            }
                            break;

                        case 2:
                            //"2번째 소켓"은
                            //"인덱스 + 3 번째 카드"의
                            //"0번째 소켓"과 연결되어 있음
                            if (tiles[i + 3][0] == null)
                            {
                                t[2] = new Socket();
                            }
                            else
                            {
                                t[2] = tiles[i + 3][0];
                            }
                            break;

                        case 3:
                            //"3번째 소켓"은
                            //"인덱스 + 2 번째 카드"의
                            //"1번째 소켓"과 연결되어 있음
                            if (tiles[i + 2][1] == null)
                            {
                                t[3] = new Socket();
                            }
                            else
                            {
                                t[3] = tiles[i + 2][1];
                            }
                            break;
                    }
                }
            }
        }
    }
}
