using UnityEngine;

public class Tile : MonoBehaviour
{
    /// <summary>
    /// �ڽ��� ��ġ�� ������ ��ū�� ����
    /// </summary>
    public Token PlacedToken = null;
    [SerializeField] int tileIndex;
    public int TileIndex => tileIndex;


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;

        // Gizmo�� ��Ÿ�� ��ġ�� ����մϴ�.
        Vector3 position = transform.position;

        // �� �信 �ؽ�Ʈ�� ǥ���մϴ�.
        UnityEditor.Handles.Label(position, tileIndex.ToString());
    }
}
