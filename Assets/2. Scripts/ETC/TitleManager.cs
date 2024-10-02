using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class TitleManager : MonoBehaviour
{
    public Fade fade;
    public Animator title;
    public GameObject messageRoot;
    public Text message;
    public string nextSceneName = string.Empty;

    private void Start()
    {
        string origin = message.text;
        message.text = string.Empty;
        new Commander()
            .Add(() =>
            {
                title.gameObject.SetActive(false);
                messageRoot.SetActive(false);
                fade.In(3f);
            }
            , 3.3f)
            .Add(() => title.gameObject.SetActive(true))
            .WaitWhile(() => title.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
            .Add(() =>
            {
                messageRoot.SetActive(true);
                message.DOText(origin, 2f);
            })
            .WaitUntil(() => Input.anyKeyDown || Input.GetMouseButtonDown(0))
            .Add(() => {

                message.DOKill();
                message.text = origin;
                fade.Out(3f);
            }
            , 3.3f)
            .Add(() =>
            {
                if (IsSceneValid(nextSceneName))
                {
                    SceneManager.LoadScene(nextSceneName);
                }
                else
                {
                    // 현재 씬의 빌드 인덱스 + 1로 로드 시도
                    int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
                    int nextSceneIndex = currentSceneIndex + 1;

                    // 다음 씬 인덱스가 빌드 설정에 있는지 확인
                    if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
                    {
                        SceneManager.LoadScene(nextSceneIndex);
                    }
                    else
                    {
                        Debug.LogError("다음 씬을 로드할 수 없습니다. 빌드 설정에 씬이 존재하지 않습니다.");
                    }
                }
            })
            .Play();
    }

    private bool IsSceneValid(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return false;

        // 씬이 빌드에 포함되었는지 확인
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneFileName = System.IO.Path.GetFileNameWithoutExtension(scenePath);

            if (sceneFileName.Equals(sceneName, System.StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
