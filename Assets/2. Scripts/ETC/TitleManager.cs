using UnityEngine;
using Mirror;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class TitleManager : MonoBehaviour
{
    public Fade fade;
    public Animator title;
    public GameObject obj;

    private void OnDestroy()
    {
        commander.Cancel();
    }

    private void Awake()
    {
        title.gameObject.SetActive(false);
        obj.SetActive(false);
    }

    Commander commander = new Commander();

    private void Start()
    {
        commander
            .Add(() => fade.In(3f), 3.3f)
            .Add(() => title.gameObject.SetActive(true))
            .WaitWhile(() => title.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.5f)
            .WaitUntil(() => !(title.GetCurrentAnimatorStateInfo(0).normalizedTime < 1)
            || Input.anyKeyDown || Input.GetMouseButtonDown(0))
            .Add(() => obj.SetActive(true))
            .Play();
    }


    NetworkManager manager;
    public Button startHostButton;
    public void StartHost()
    {
        if (manager == null)
        {
            manager = NetworkManager.singleton;
        }

        try
        {
            manager.StartHost();
        }
        catch
        {
            print("서버 생성 실패");
            startHostButton.GetComponent<RectTransform>().DOPunchPosition(Vector3.one * 10, .5f, 30);
        }
    }
}
