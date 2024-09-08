using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] int tileIndex;
    public int TileIndex => tileIndex;

    //���� Ÿ�Ͽ� ������ 
    public Card placedCard = null;

    [SerializeField, Header("Ÿ�Ͽ� ����� ����")]
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

        // Gizmo�� ��Ÿ�� ��ġ�� ����մϴ�.
        Vector3 position = transform.position;
        Gizmos.DrawWireCube(transform.position, new Vector3(.6f, .1f, .8f));
        // �� �信 �ؽ�Ʈ�� ǥ���մϴ�.
        UnityEditor.Handles.Label(position, tileIndex.ToString());
    }
#endif
}
