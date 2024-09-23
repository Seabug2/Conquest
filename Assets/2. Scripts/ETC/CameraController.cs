using UnityEngine;
using UnityEngine.EventSystems;
using Cinemachine;
using DG.Tweening;

public class CameraController : MonoBehaviour
{
    #region 싱글톤
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

    //화면을 바꾸는 조작은 Local 플레이어에서만 가능
    CinemachineBrain brainCam;
    // 0 : 첫 번째 플레이어
    // 1 : 두 번째 플레이어
    // 2 : 세 번째 플레이어
    // 3 : 네 번째 플레이어
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


    // 공통 덱 카메라
    [SerializeField] CinemachineVirtualCamera centerCamera;

    //자신의 필드 번호
    int HomeViewIndex => GameManager.instance.LocalPlayer.order;
    int currentCamIndex = -1;

    public int CurrentCamIndex
    {
        get { return currentCamIndex; }
        set
        {
            currentCamIndex = value;

            //현재 활성화 중인 카메라 우선도를 0으로
            brainCam.ActiveVirtualCamera.Priority = 0;

            if (value < 0 || value >= playersVcams.Length)
            {
                //덱으로 카메라를 이동한다.
                centerCamera.Priority = 1;
            }
            else
            {
                playersVcams[value].Priority = 1;
            }
        }
    }

    /// <summary>
    /// 카메라를 자신의 필드로 이동시킵니다.
    /// </summary>
    public void FocusOnHome()
    {
        moveLock = false;
        CurrentCamIndex = HomeViewIndex;
    }

    /// <summary>
    /// 모두 공통 덱을 주목합니다.
    /// </summary>
    public void FocusOnCenter()
    {
        CurrentCamIndex = -1;
    }

    /// <summary>
    /// 지정하는 플레이어의 필드로 카메라를 이동
    /// </summary>
    /// <param name="i">이동하려는 플레이어의 번호</param>
    public void FocusOnPlayerField(int i)
    {
        CurrentCamIndex = i;
    }

    /// <summary>
    /// 로컬 플레이어가 카메라를 앞, 또는 다음 플레이어의 필드로 이동합니다.
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
