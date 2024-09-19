using UnityEngine;
using Cinemachine;
using DG.Tweening;

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
    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    //ȭ���� �ٲٴ� ������ Local �÷��̾���� ����
    CinemachineBrain brainCam;

    void Start()
    {
        brainCam = GetComponent<CinemachineBrain>();

        for (int i = 0; i < playersVcams.Length; i++)
        {
            perlinNoise[i] = playersVcams[i].GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        }
    }

    // 0 : ù ��° �÷��̾�
    // 1 : �� ��° �÷��̾�
    // 2 : �� ��° �÷��̾�
    // 3 : �� ��° �÷��̾�
    [SerializeField] CinemachineVirtualCamera[] playersVcams;
    readonly CinemachineBasicMultiChannelPerlin[] perlinNoise = new CinemachineBasicMultiChannelPerlin[4];

    // ���� �� ī�޶�
    [SerializeField] CinemachineVirtualCamera decksVcams;


    //�ڽ��� �ʵ� ��ȣ
    int homeViewIndex;
    int currentCamIndex;

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
                decksVcams.Priority = 1;
            }
            else
            {
                playersVcams[value].Priority = 1;
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
        moveLock = false;
        CurrentCamIndex = homeViewIndex;
    }

    /// <summary>
    /// ��� ���� ���� �ָ��մϴ�.
    /// </summary>
    public void FocusOnDeck()
    {
        moveLock = true;
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
        int sidePlayerNumber = currentCamIndex;
        do
        {
            sidePlayerNumber = (sidePlayerNumber + dir + 4) % 4;
        } while (GameManager.Player(sidePlayerNumber).isGameOver);

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

    public bool moveLock = false;

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
            HomeView();
    }
}
