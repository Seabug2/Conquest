using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] int tileIndex;
    public int TileIndex => tileIndex;

    //현재 타일에 전개한 
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

        // Gizmo가 나타날 위치를 계산합니다.
        Vector3 position = transform.position;
        Gizmos.DrawWireCube(transform.position, new Vector3(.6f, .1f, .8f));
        // 씬 뷰에 텍스트를 표시합니다.
        UnityEditor.Handles.Label(position, tileIndex.ToString());
    }
}
