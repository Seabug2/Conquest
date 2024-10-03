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
        // 호스트가 시작되면 Text UI를 활성화하고 IP 주소를 표시
        string hostIpAddress = GetLocalIpAddress();
        text.text = $"Host : {hostIpAddress}";
    }

    public override void OnStartClient()
    {
        if (!isServer) gameObject.SetActive(false);
    }

    // IP 주소를 가져오는 메서드
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
