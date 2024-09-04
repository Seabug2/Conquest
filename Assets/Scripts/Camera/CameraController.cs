using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraController : MonoBehaviour
{
    public static CameraController instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    //화면을 바꾸는 조작은 Local 플레이어에서만 가능
    CinemachineBrain brainCam;

    void Start()
    {
        brainCam = GetComponent<CinemachineBrain>();
        currentVcam = (CinemachineVirtualCamera)brainCam.ActiveVirtualCamera;
    }

    // 0 : 첫 번째 플레이어
    // 1 : 두 번째 플레이어
    // 2 : 세 번째 플레이어
    // 3 : 네 번째 플레이어
    [SerializeField] CinemachineVirtualCamera[] playersVcams;

    // 공통 덱 카메라
    [SerializeField] CinemachineVirtualCamera decksVcams;


    //자신의 필드 번호
    int homeViewIndex = -1;
    int currentCamIndex = -1;
    public int CurrentCamIndex
    {
        get { return currentCamIndex; }
        set
        {
            currentCamIndex = value;
            //현재 활성화 중인 카메라 우선도를 0으로
            brainCam.ActiveVirtualCamera.Priority = 0;

            if (value < 0)
            {
                decksVcams.Priority = 1;
                currentVcam = decksVcams;
            }
            else
            {
                playersVcams[value].Priority = 1;
                currentVcam = playersVcams[value];
            }
        }
    }

    public void Init(int i)
    {
        homeViewIndex = i;
        FocusOnDeck();
    }

    /// <summary>
    /// 카메라를 자신의 필드로 이동시킵니다.
    /// </summary>
    public void HomeView()
    {
        CurrentCamIndex = homeViewIndex;
    }

    /// <summary>
    /// 모두 공통 덱을 주목합니다.
    /// </summary>
    public void FocusOnDeck()
    {
        CurrentCamIndex = -1;
    }

    /// <summary>
    /// 지정하는 플레이어의 필드로 카메라를 이동
    /// </summary>
    /// <param name="i">이동하려는 플레이어의 번호</param>
    public void SetVCam(int i)
    {
        CurrentCamIndex = i;
    }

    /// <summary>
    /// 로컬 플레이어가 카메라를 앞, 또는 다음 플레이어의 필드로 이동합니다.
    /// </summary>
    /// <param name="i">-1 or +1</param>
    public void ShiftVCam(int dir)
    {
        CurrentCamIndex = GameManager.instance.GetAdjacentPlayer(currentCamIndex, dir);
    }

    [SerializeField]
    CinemachineVirtualCamera currentVcam;
}
