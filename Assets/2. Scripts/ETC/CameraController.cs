using UnityEngine;
using UnityEngine.EventSystems;
using Cinemachine;
using DG.Tweening;

public class CameraController : MonoBehaviour
{
    #region �̱���
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
    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
    #endregion

    //ȭ���� �ٲٴ� ������ Local �÷��̾���� ����
    CinemachineBrain brainCam;
    // 0 : ù ��° �÷��̾�
    // 1 : �� ��° �÷��̾�
    // 2 : �� ��° �÷��̾�
    // 3 : �� ��° �÷��̾�
    [SerializeField] CinemachineVirtualCamera[] playersVcams = new CinemachineVirtualCamera[4];
    readonly CinemachineBasicMultiChannelPerlin[] perlinNoise = new CinemachineBasicMultiChannelPerlin[4];
    private Physics2DRaycaster raycaster;
    public Physics2DRaycaster Raycaster => raycaster;

    void Start()
    {
        brainCam = GetComponent<CinemachineBrain>();
        raycaster = GetComponent<Physics2DRaycaster>();
        for (int i = 0; i < playersVcams.Length; i++)
        {
            if (playersVcams[i] != null)
                perlinNoise[i] = playersVcams[i].GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        }
    }


    // ���� �� ī�޶�
    [SerializeField] CinemachineVirtualCamera centerCamera;

    //�ڽ��� �ʵ� ��ȣ
    int HomeViewIndex => GameManager.instance.LocalPlayer.order;
    int currentCamIndex = -1;

    public int CurrentCamIndex
    {
        get { return currentCamIndex; }
        set
        {
            currentCamIndex = value;

            //���� Ȱ��ȭ ���� ī�޶� �켱���� 0����
            brainCam.ActiveVirtualCamera.Priority = 0;

            if (value < 0 || value >= playersVcams.Length)
            {
                //������ ī�޶� �̵��Ѵ�.
                centerCamera.Priority = 1;
            }
            else
            {
                playersVcams[value].Priority = 1;
            }
        }
    }

    /// <summary>
    /// ī�޶� �ڽ��� �ʵ�� �̵���ŵ�ϴ�.
    /// </summary>
    public void FocusOnHome()
    {
        moveLock = false;
        CurrentCamIndex = HomeViewIndex;
    }

    /// <summary>
    /// ��� ���� ���� �ָ��մϴ�.
    /// </summary>
    public void FocusOnCenter()
    {
        CurrentCamIndex = -1;
    }

    /// <summary>
    /// �����ϴ� �÷��̾��� �ʵ�� ī�޶� �̵�
    /// </summary>
    /// <param name="i">�̵��Ϸ��� �÷��̾��� ��ȣ</param>
    public void FocusOnPlayerField(int i)
    {
        CurrentCamIndex = i;
    }

    /// <summary>
    /// ���� �÷��̾ ī�޶� ��, �Ǵ� ���� �÷��̾��� �ʵ�� �̵��մϴ�.
    /// </summary>
    /// <param name="i">-1 or +1</param>
    public void ShiftVCam(int dir)
    {
        int sidePlayerNumber = currentCamIndex;
        do
        {
            sidePlayerNumber = (sidePlayerNumber + dir + 4) % 4;
        } while (GameManager.GetPlayer(sidePlayerNumber).isGameOver);

        CurrentCamIndex = sidePlayerNumber;
    }

    public void DoShake(int i, float duration, float power)
    {
        //if (i != CurrentCamIndex) return;

        CinemachineBasicMultiChannelPerlin p = perlinNoise[i];

        if (DOTween.IsTweening(p))
            p.DOKill();

        p.DOKill();
        p.m_AmplitudeGain = power;

        DOTween.To(() => p.m_AmplitudeGain, x => p.m_AmplitudeGain = x, 0f, duration);
    }

    [SerializeField]
    bool moveLock = false;

    public void MoveLock(bool _isLocked)
    {
        moveLock = _isLocked;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            DoShake(currentCamIndex, 0.5f, 10);
        }

        if (moveLock) return;

        if (Input.GetKeyDown(KeyCode.LeftArrow))
            ShiftVCam(-1);
        else if (Input.GetKeyDown(KeyCode.RightArrow))
            ShiftVCam(1);
        else if (Input.GetKeyDown(KeyCode.C))
            FocusOnHome();
        else if (Input.GetKeyDown(KeyCode.D))
            FocusOnCenter();
    }
}
