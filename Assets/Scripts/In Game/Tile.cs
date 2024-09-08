using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] int tileIndex;
    public int TileIndex => tileIndex;

    //현재 타일에 전개한 
    public Card placedCard = null;

    [SerializeField, Header("타일에 연결된 소켓")]
    Socket[] socket = new Socket[4];
    public Socket[] Socket => socket;

    private void Start()
    {
        TileClear();
    }

    public void TileClear()
    {
        socket = new Socket[4];
        placedCard = null;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;

        // Gizmo가 나타날 위치를 계산합니다.
        Vector3 position = transform.position;
        Gizmos.DrawWireCube(transform.position, new Vector3(.6f, .1f, .8f));
        // 씬 뷰에 텍스트를 표시합니다.
        UnityEditor.Handles.Label(position, tileIndex.ToString());
    }
#endif
}
