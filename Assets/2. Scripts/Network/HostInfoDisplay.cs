using System.Net;
using UnityEngine;
using TMPro;
using Mirror;

public class HostInfoDisplay : NetworkBehaviour
{
    TextMeshProUGUI text;

    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
        // ȣ��Ʈ�� ���۵Ǹ� Text UI�� Ȱ��ȭ�ϰ� IP �ּҸ� ǥ��
        string hostIpAddress = GetLocalIpAddress();
        text.text = $"Host : {hostIpAddress}";
    }

    public override void OnStartClient()
    {
        if (!isServer) gameObject.SetActive(false);
    }

    // IP �ּҸ� �������� �޼���
    private string GetLocalIpAddress()
    {
        string localIP = "Not Available";
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
        catch (System.Exception ex)
        {
            Debug.LogError("Failed to get IP Address: " + ex.Message);
        }
        return localIP;
    }
}
