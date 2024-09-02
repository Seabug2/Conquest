using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraController : MonoBehaviour
{
    //ȭ���� �ٲٴ� ������ Local �÷��̾���� ����
    CinemachineBrain brainCam;

    void Start()
    {
        brainCam = GetComponent<CinemachineBrain>();
    }

    // 0 : ù ��° �÷��̾�
    // 1 : �� ��° �÷��̾�
    // 2 : �� ��° �÷��̾�
    // 3 : �� ��° �÷��̾�
    [SerializeField] CinemachineVirtualCamera[] playersVcams;

    // ���� �� ī�޶�
    [SerializeField] CinemachineVirtualCamera decksVcams;

    //�ڽ��� �ʵ� ��ȣ
    int homeViewIndex = -1;
    int currentCamIndex = -1;

    public void Init(int i)
    {
        homeViewIndex = i;
        currentCamIndex = homeViewIndex;
        FocusOnDeck();
    }

    /// <summary>
    /// ī�޶� �ڽ��� �ʵ�� �̵���ŵ�ϴ�.
    /// </summary>
    public void HomeView()
    {
        currentCamIndex = homeViewIndex;
        SetVCam(homeViewIndex);
    }
    /// <summary>
    /// ��� ���� ���� �ָ��մϴ�.
    /// </summary>
    public void FocusOnDeck()
    {
        brainCam.ActiveVirtualCamera.Priority = 0;
        decksVcams.Priority = 1;
        currentCamIndex = -1;
    }

    /// <summary>
    /// Ư�� ��ȣ�� �÷��̾� ȭ������ �̵�
    /// </summary>
    /// <param name="i">�̵��Ϸ��� ȭ���� �÷��̾� ��ȣ</param>
    public void SetVCam(int i)
    {
        //���� Ȱ��ȭ ���� ȭ���� �켱���� 0���� �����.
        brainCam.ActiveVirtualCamera.Priority = 0;
        //ȭ���� �ٲ۴�.
        currentCamIndex = i;
        //playersVcams[currentCamIndex].Priority = 1;
    }

    /// <summary>
    /// ���ϴ� ���� ������ �÷��̾� ȭ������ �̵�
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
