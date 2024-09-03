using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] int tileIndex;
    public int TileIndex => tileIndex;

    //���� Ÿ�Ͽ� ������ 
    public Piece placedPiece = null;

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

    public void DeployVillainPiece(Piece villain)
    {
        villain.transform.position = transform.position;

        for(int i = 0; i < 4; i ++)
        {
            ConnectableSocket[i] = villain.Sockets[i].type;
        }

        placedPiece = villain;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;

        // Gizmo�� ��Ÿ�� ��ġ�� ����մϴ�.
        Vector3 position = transform.position;
        Gizmos.DrawWireCube(transform.position, new Vector3(.6f, .1f, .8f));
        // �� �信 �ؽ�Ʈ�� ǥ���մϴ�.
        UnityEditor.Handles.Label(position, tileIndex.ToString());
    }
}
