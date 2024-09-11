using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] int tileIndex;
    public int TileIndex => tileIndex;

    //현재 타일에 전개한 
    public Card placedCard = null;

    public bool[] linkableSocket = new bool[4]; 
    [SerializeField, Header("연결 소켓")]
    Socket[] sockets = new Socket[4];
    public Socket this[int index]
    {
        get
        {
            if (index < 0 || index >= sockets.Length)
            {
                Debug.Log("인덱스 오류");
                return null;
            }
            return sockets[index];
        }

        set
        {
            if (index < 0 || index >= sockets.Length)
            {
                Debug.Log("인덱스 오류");
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

        // Gizmo가 나타날 위치를 계산합니다.
        Vector3 position = transform.position;
        Gizmos.DrawWireCube(transform.position, new Vector3(.6f, .8f, .1f));
        // 씬 뷰에 텍스트를 표시합니다.
        UnityEditor.Handles.Label(position, tileIndex.ToString());
    }
#endif
}
