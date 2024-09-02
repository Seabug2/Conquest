using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraController : MonoBehaviour
{
    //화면을 바꾸는 조작은 Local 플레이어에서만 가능
    CinemachineBrain brainCam;

    void Start()
    {
        brainCam = GetComponent<CinemachineBrain>();
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

    public void Init(int i)
    {
        homeViewIndex = i;
        currentCamIndex = homeViewIndex;
        FocusOnDeck();
    }

    /// <summary>
    /// 카메라를 자신의 필드로 이동시킵니다.
    /// </summary>
    public void HomeView()
    {
        currentCamIndex = homeViewIndex;
        SetVCam(homeViewIndex);
    }
    /// <summary>
    /// 모두 공통 덱을 주목합니다.
    /// </summary>
    public void FocusOnDeck()
    {
        brainCam.ActiveVirtualCamera.Priority = 0;
        decksVcams.Priority = 1;
        currentCamIndex = -1;
    }

    /// <summary>
    /// 특정 번호의 플레이어 화면으로 이동
    /// </summary>
    /// <param name="i">이동하려는 화면의 플레이어 번호</param>
    public void SetVCam(int i)
    {
        //현재 활성화 중인 화면의 우선도를 0으로 만든다.
        brainCam.ActiveVirtualCamera.Priority = 0;
        //화면을 바꾼다.
        currentCamIndex = i;
        //playersVcams[currentCamIndex].Priority = 1;
    }

    /// <summary>
    /// 원하는 진행 방향의 플레이어 화면으로 이동
    /// </summary>
    /// <param name="i">-1 or +1</param>
    public void ShiftVCam(int i)
    {
        if (currentCamIndex == -1)
        {
            currentCamIndex = homeViewIndex;
        }
        else
        {
            currentCamIndex = (currentCamIndex + 4 + i) % 4;
        }

        brainCam.ActiveVirtualCamera.Priority = 0;
        playersVcams[currentCamIndex].Priority = 1;
    }
}
