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

    //ȭ���� �ٲٴ� ������ Local �÷��̾���� ����
    CinemachineBrain brainCam;

    void Start()
    {
        brainCam = GetComponent<CinemachineBrain>();
        currentVcam = (CinemachineVirtualCamera)brainCam.ActiveVirtualCamera;
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
    public int CurrentCamIndex
    {
        get { return currentCamIndex; }
        set
        {
            currentCamIndex = value;
            //���� Ȱ��ȭ ���� ī�޶� �켱���� 0����
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
    /// ī�޶� �ڽ��� �ʵ�� �̵���ŵ�ϴ�.
    /// </summary>
    public void HomeView()
    {
        CurrentCamIndex = homeViewIndex;
    }

    /// <summary>
    /// ��� ���� ���� �ָ��մϴ�.
    /// </summary>
    public void FocusOnDeck()
    {
        CurrentCamIndex = -1;
    }

    /// <summary>
    /// �����ϴ� �÷��̾��� �ʵ�� ī�޶� �̵�
    /// </summary>
    /// <param name="i">�̵��Ϸ��� �÷��̾��� ��ȣ</param>
    public void SetVCam(int i)
    {
        CurrentCamIndex = i;
    }

    /// <summary>
    /// ���� �÷��̾ ī�޶� ��, �Ǵ� ���� �÷��̾��� �ʵ�� �̵��մϴ�.
    /// </summary>
    /// <param name="i">-1 or +1</param>
    public void ShiftVCam(int dir)
    {
        CurrentCamIndex = GameManager.instance.GetAdjacentPlayer(currentCamIndex, dir);
    }

    [SerializeField]
    CinemachineVirtualCamera currentVcam;
}
