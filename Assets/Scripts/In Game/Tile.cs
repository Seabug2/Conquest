using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] int tileIndex;
    public int TileIndex => tileIndex;

    //���� Ÿ�Ͽ� ������ 
    public Card placedPiece = null;

    public Attribute[] ConnectableSocket { get; private set; }

    private void Start()
    {
        TileClear();
    }

    public void TileClear()
    {
        ConnectableSocket = new Attribute[]{ Attribute.isNull,Attribute.isNull, Attribute.isNull, Attribute.isNull };
        placedPiece = null;
    }

    public void DeployVillainPiece(Card villain)
    {
        villain.transform.position = transform.position;

        for(int i = 0; i < 4; i ++)
        {
            ConnectableSocket[i] = villain.Sockets[i].type;
        }

        placedPiece = villain;
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
