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
        // ���Խ��� �̿��Ͽ� ���ڸ� ����� �������� ����
        ipf.text = Regex.Replace(input, "[^0-9]", "");
    }
}

public class NetworkConnector : MonoBehaviour
{
    public Button startHostButton;
    //ȣ��Ʈ ������
    public Button cancelButton;

    public Button startGuestButton;
    //�Խ�Ʈ
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
    //�ε� UI Ȱ��ȭ

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
            ShowError("��ȿ�� IP �ּҸ� �Է����ּ���.");
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
            print("���� ���� ����");
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
    /// IP �ּ��� �� �κ��� ��ȿ���� �˻��մϴ�.
    /// </summary>
    /// <param name="part">IP �ּ��� �� �κ�</param>
    /// <returns>��ȿ�ϸ� true, �ƴϸ� false</returns>
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
            //ȣ��Ʈ ������ �����ϸ�...

            if (NetworkServer.active && NetworkClient.active)
            {
                OnCompleteConnection.Invoke();
                hostAddress.text = $"Host : {GetLocalIpAddress()}";
                hostAddress.gameObject.SetActive(true);
            }
        }
        catch
        {
            print("���� ���� ���� / IP Ȯ�� ��");
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
    /// ���� �޽����� ����ڿ��� ǥ���մϴ�.
    /// </summary>
    /// <param name="message">���� �޽���</param>
    private void ShowError(string message)
    {
        // ���⼭ UI�� ���� ���� �޽����� ǥ���� �� �ֽ��ϴ�.
        Debug.LogError(message);
        // ���� ���, �˾� â�� ���ų� �ؽ�Ʈ UI�� ������Ʈ�� �� �ֽ��ϴ�.
    }
}
