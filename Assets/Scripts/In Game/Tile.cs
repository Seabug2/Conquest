using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] int tileIndex;
    public int TileIndex => tileIndex;

    //���� Ÿ�Ͽ� ������ 
    public Card placedCard = null;

    public bool[] linkableSocket = new bool[4]; 
    [SerializeField, Header("���� ����")]
    Socket[] sockets = new Socket[4];
    public Socket this[int index]
    {
        get
        {
            if (index < 0 || index >= sockets.Length)
            {
                Debug.Log("�ε��� ����");
                return null;
            }
            return sockets[index];
        }

        set
        {
            if (index < 0 || index >= sockets.Length)
            {
                Debug.Log("�ε��� ����");
            }
            sockets[index] = value;
        }
    }

    private void Start()
    {
        TileClear();
    }

    public void TileClear()
    {
        sockets = new Socket[4];
        placedCard = null;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;

        // Gizmo�� ��Ÿ�� ��ġ�� ����մϴ�.
        Vector3 position = transform.position;
        Gizmos.DrawWireCube(transform.position, new Vector3(.6f, .8f, .1f));
        // �� �信 �ؽ�Ʈ�� ǥ���մϴ�.
        UnityEditor.Handles.Label(position, tileIndex.ToString());
    }
#endif
}
