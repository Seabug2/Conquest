using UnityEngine;

public class Tile : MonoBehaviour
{
    /// <summary>
    /// 자신의 위치에 놓여진 토큰을 저장
    /// </summary>
    public Token PlacedToken = null;
    [SerializeField] int tileIndex;
    public int TileIndex => tileIndex;


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;

        // Gizmo가 나타날 위치를 계산합니다.
        Vector3 position = transform.position;

        // 씬 뷰에 텍스트를 표시합니다.
        UnityEditor.Handles.Label(position, tileIndex.ToString());
    }
}
