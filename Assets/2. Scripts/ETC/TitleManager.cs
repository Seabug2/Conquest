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
                    // ���� ���� ���� �ε��� + 1�� �ε� �õ�
                    int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
                    int nextSceneIndex = currentSceneIndex + 1;

                    // ���� �� �ε����� ���� ������ �ִ��� Ȯ��
                    if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
                    {
                        SceneManager.LoadScene(nextSceneIndex);
                    }
                    else
                    {
                        Debug.LogError("���� ���� �ε��� �� �����ϴ�. ���� ������ ���� �������� �ʽ��ϴ�.");
                    }
                }
            })
            .Play();
    }

    private bool IsSceneValid(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return false;

        // ���� ���忡 ���ԵǾ����� Ȯ��
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
