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
            Debug.Log("���� ����");
            return null;
        }

        if (tiles[index].placedCard == null)
        {
            Debug.Log("ī�尡 ��ġ���� �ʾҽ��ϴ�.");
            return null;
        }

        return tiles[index].placedCard;
    }

    public bool IsEmpty(int index)
    {
        if (index < 0 || index >= tiles.Length)
        {
            Debug.Log("���� ����");
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
        //��� Ÿ���� �˻�
        for (int i = 0; i < tiles.Length; i++)
        {
            Tile t = tiles[i];

            //Ÿ���� ��� ������ Ȯ��
            for (int j = 0; j < 4; j++)
            {
                if (t.linkableSocket[j]) //�� ������ ���ᰡ���� ���¶��...
                {
                    // 0     1
                    //
                    //
                    // 3     2
                    switch (j)
                    {
                        case 0:
                            //"0��° ����"��
                            //"�ε��� - 3 ��° ī��"��
                            //"2��° ����"�� ����Ǿ� ����
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
                            //"1��° ����"��
                            //"�ε��� - 2 ��° ī��"��
                            //"3��° ����"�� ����Ǿ� ����
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
                            //"2��° ����"��
                            //"�ε��� + 3 ��° ī��"��
                            //"0��° ����"�� ����Ǿ� ����
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
                            //"3��° ����"��
                            //"�ε��� + 2 ��° ī��"��
                            //"1��° ����"�� ����Ǿ� ����
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
