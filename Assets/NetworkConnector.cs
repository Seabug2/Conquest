using System.Net;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;
using Mirror;
using TMPro;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using UnityEngine.SceneManagement;

[System.Serializable]
public class IpInputField
{
    public InputField ipf;

    public void ValidateInput(string input)
    {
        // 정규식을 이용하여 숫자만 남기고 나머지를 제거
        ipf.text = Regex.Replace(input, "[^0-9]", "");
    }
}

public class NetworkConnector : MonoBehaviour
{
    public Button startHostButton;
    //호스트 생성시
    public Button cancelButton;

    public Button startGuestButton;
    //게스트
    public IpInputField[] ipInputs = new IpInputField[4];

    NetworkManager manager;

    public TextMeshProUGUI hostAddress;
    public TextMeshProUGUI playerCounter;

    public void PlayerCounter()
    {
        int playerCount = GameManager.instance.ConnectedPlayerCount();

        playerCounter.text = $"( {playerCount} / 4)";
    }

    public UnityEvent OnCompleteConnection;
    //로딩 UI 활성화

    public UnityEvent OnDisconnection;


    private void Start()
    {
        manager = NetworkManager.singleton;
        ActiveInputField();
    }

    public void ActiveInputField()
    {
        foreach (IpInputField i in ipInputs)
        {
            i.ipf.gameObject.SetActive(true);
            i.ipf.onValueChanged.RemoveAllListeners();
            i.ipf.onValueChanged.AddListener(i.ValidateInput);
        }
    }

    public void StartGuest()
    {
        string ip = BuildIpAddress();
        if (string.IsNullOrEmpty(ip))
        {
            ShowError("유효한 IP 주소를 입력해주세요.");
            startGuestButton.GetComponent<RectTransform>().DOPunchPosition(Vector3.one * 10, .5f, 30);
            return;
        }

        manager.networkAddress = ip;

        try
        {
            manager.StartClient();

            if (NetworkClient.active)
            {
                OnCompleteConnection.Invoke();
            }
        }
        catch
        {
            print("서버 생성 실패");
            startGuestButton.GetComponent<RectTransform>().DOPunchPosition(Vector3.one * 10, .5f, 30);
        }
    }
    private string BuildIpAddress()
    {
        string[] ipParts = new string[ipInputs.Length];
        for (int i = 0; i < ipInputs.Length; i++)
        {
            string part = ipInputs[i].ipf.text.Trim();
            if (string.IsNullOrEmpty(part) || !IsValidIpPart(part))
            {
                return string.Empty;
            }
            ipParts[i] = part;
        }
        return string.Join(".", ipParts);
    }
    /// <summary>
    /// IP 주소의 각 부분이 유효한지 검사합니다.
    /// </summary>
    /// <param name="part">IP 주소의 한 부분</param>
    /// <returns>유효하면 true, 아니면 false</returns>
    private bool IsValidIpPart(string part)
    {
        if (int.TryParse(part, out int num))
        {
            return num >= 0 && num <= 255;
        }
        return false;
    }
    public void StartHost()
    {
        try
        {
            manager.StartHost();
            //호스트 연결이 성공하면...

            if (NetworkServer.active && NetworkClient.active)
            {
                OnCompleteConnection.Invoke();
                hostAddress.text = $"Host : {GetLocalIpAddress()}";
                hostAddress.gameObject.SetActive(true);
            }
        }
        catch
        {
            print("서버 연결 실패 / IP 확인 요");
            startHostButton.GetComponent<RectTransform>().DOPunchPosition(Vector3.one * 10, .5f, 30);
        }
    }

    public static string GetLocalIpAddress()
    {
        string localIP = string.Empty;
        try
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                    break;
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to get IP Address: " + e.Message);
        }
        return localIP;
    }

    public void Stop()
    {
        if (NetworkClient.active)
        {
            if (NetworkServer.active)
            {
                manager.StopHost();
            }
            else
            {
                manager.StopClient();
            }
            GameManager.instance = null;
        }
    }

    public void ReturnToTitle()
    {
        Stop();
        SceneManager.LoadScene(0);
    }

    public void Disconect()
    {
        Stop();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// 오류 메시지를 사용자에게 표시합니다.
    /// </summary>
    /// <param name="message">오류 메시지</param>
    private void ShowError(string message)
    {
        // 여기서 UI를 통해 오류 메시지를 표시할 수 있습니다.
        Debug.LogError(message);
        // 예를 들어, 팝업 창을 띄우거나 텍스트 UI를 업데이트할 수 있습니다.
    }
}
