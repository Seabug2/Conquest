using UnityEngine;
using Cinemachine;

public class CamMover : MonoBehaviour
{
    CinemachineBrain brainCam;
    CinemachineVirtualCamera vCam;
    CinemachineTransposer transposer;
    Vector3 originState;

    public float moveSize = .5f;

    const float zoomMax = 1f;
    const float zoomMin = 5.5f;
    const float horizontalLimit = 2.2f;
    const float verticalTopLimit = 2.5f;
    const float verticalBottomLimit = -3.3f;

    private void Awake()
    {
        brainCam = Camera.main.GetComponent<CinemachineBrain>();
        vCam = GetComponent<CinemachineVirtualCamera>();

        transposer = vCam.GetCinemachineComponent<CinemachineTransposer>();
        originState = transposer.m_FollowOffset;

        //불필요할 땐 비활성화하여 Update가 실행되지 않게 함
        enabled = false;
    }

    private void OnEnable()
    {
        transposer.m_FollowOffset = originState;
    }

    private void Update()
    {
        if (vCam != (CinemachineVirtualCamera)brainCam.ActiveVirtualCamera) return;

        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            float zoom = Input.GetAxis("Mouse ScrollWheel");
            float current = transposer.m_FollowOffset.y;
            current = current + zoom;
            current = Mathf.Clamp(current, zoomMax, zoomMin);
            transposer.m_FollowOffset = new Vector3(transposer.m_FollowOffset.x, current, transposer.m_FollowOffset.z);
        }

        if (Input.GetMouseButton(2))
        {
            float x = transposer.m_FollowOffset.x;
            x = Mathf.Clamp(x - Input.GetAxis("Mouse X") * moveSize, -horizontalLimit, horizontalLimit);
            //x = x * moveSize;
            float z = transposer.m_FollowOffset.z;
            z = Mathf.Clamp(z - Input.GetAxis("Mouse Y") * moveSize, verticalBottomLimit, verticalTopLimit);
            //z = z* moveSize;

            Vector3 delta = new Vector3(x, transposer.m_FollowOffset.y, z);

            transposer.m_FollowOffset = delta;
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            transposer.m_FollowOffset = originState;
        }
    }
}
