using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayGizmo : MonoBehaviour
{
    public Camera mainCamera;
    public Vector2 screenPosition;  // ���ϴ� ȭ�� ��ǥ (���콺 ��ġ ��)

    // ������ ���̸� �����մϴ�.
    public float rayLength = 10f;

    private void OnDrawGizmos()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        // ȭ�� ��ǥ���� ���� ��ǥ�� ��ȯ�ϴ� Ray�� �����մϴ�.
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        //Ray ray = mainCamera.ScreenPointToRay(screenPosition);

        // Gizmos ������ �����մϴ�.
        Gizmos.color = Color.red;

        // Ray�� ���� ������ �� ������ ������ �׸��ϴ�.
        Gizmos.DrawLine(ray.origin, ray.origin + ray.direction * rayLength);
    }
}
