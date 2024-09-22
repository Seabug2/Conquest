using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayGizmo : MonoBehaviour
{
    public Camera mainCamera;
    public Vector2 screenPosition;  // 원하는 화면 좌표 (마우스 위치 등)

    // 레이의 길이를 설정합니다.
    public float rayLength = 10f;

    private void OnDrawGizmos()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        // 화면 좌표에서 월드 좌표로 변환하는 Ray를 생성합니다.
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        //Ray ray = mainCamera.ScreenPointToRay(screenPosition);

        // Gizmos 색상을 설정합니다.
        Gizmos.color = Color.red;

        // Ray의 시작 지점과 끝 지점을 선으로 그립니다.
        Gizmos.DrawLine(ray.origin, ray.origin + ray.direction * rayLength);
    }
}
