using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Text.RegularExpressions;
using DG.Tweening;
using UnityEngine.Events;
using Mirror;

public class IpInputField
{
    public InputField ipf;
    public IpInputField nextInputField; // 다음 인풋필드를 참조
    public Button startGuestButton;     // 마지막 필드에서 버튼으로 이동

    public IpInputField()
    {
        ipf = null;
        nextInputField = null;
        startGuestButton = null;
    }

    public void ValidateInput(string input)
    {
        // 정규식을 이용하여 숫자만 남기고 나머지를 제거
        ipf.text = Regex.Replace(input, "[^0-9]", "");

        // 숫자 3개가 입력되면 다음 인풋필드 또는 버튼으로 포커스 이동
        if (ipf.text.Length == 3)
        {
            if (nextInputField != null)
            {
                nextInputField.ipf.Select();
            }
            else if (startGuestButton != null)
            {
                startGuestButton.Select(); // 마지막 필드일 때 버튼으로 포커스 이동
            }
        }
    }
}

public class InputIPAddressManager : MonoBehaviour
{
    NetworkManager manager;

    IpInputField[] ipInputs;
    Button startGuestButton;

    int currentPointer = 0;

    private void Start()
    {
        manager = NetworkManager.singleton;
        startGuestButton = GetComponentInChildren<Button>();
        ActiveInputField();
    }

    public void ActiveInputField()
    {
        InputField[] inputFields = GetComponentsInChildren<InputField>(true);
        int length = inputFields.Length;

        ipInputs = new IpInputField[length];
        
        for (int i = 0; i < length; i++)
        {
            ipInputs[i] = new();
        }

        for (int i = 0; i < length; i++)
        {
            ipInputs[i].ipf = inputFields[i];
            ipInputs[i].ipf.onValueChanged.AddListener(ipInputs[i].ValidateInput);

            // 마지막 인풋필드가 아닌 경우에만 다음 필드를 설정
            if (i < ipInputs.Length - 1)
            {
                ipInputs[i].nextInputField = ipInputs[i + 1];
            }
            else
            {
                ipInputs[i].startGuestButton = startGuestButton; // 마지막 인풋필드일 때 버튼으로 이동
            }
        }

        EventSystem.current.SetSelectedGameObject(ipInputs[currentPointer].ipf.gameObject);
    }

    void Update()
    {
        // Tab 키가 눌렸을 때
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            // Shift 키가 눌린 상태라면 이전 필드로 이동
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                currentPointer = Mathf.Clamp(currentPointer - 1, 0, ipInputs.Length - 1);
            }
            // Shift 없이 Tab만 눌리면 다음 필드로 이동
            else
            {
                currentPointer = Mathf.Clamp(currentPointer + 1, 0, ipInputs.Length - 1);
            }

            EventSystem.current.SetSelectedGameObject(ipInputs[currentPointer].ipf.gameObject);
            return;
        }
        // 좌우 방향키로 이동
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            currentPointer = Mathf.Clamp(currentPointer - 1, 0, ipInputs.Length - 1);
            EventSystem.current.SetSelectedGameObject(ipInputs[currentPointer].ipf.gameObject);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            currentPointer = Mathf.Clamp(currentPointer + 1, 0, ipInputs.Length - 1);
            EventSystem.current.SetSelectedGameObject(ipInputs[currentPointer].ipf.gameObject);
            return;
        }

        // Enter 또는 Keypad Enter 키가 눌리면 버튼 클릭
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            startGuestButton.onClick.Invoke();
            return;
        }
    }

    public void StartGuest()
    {
        string ip = BuildIpAddress();
        if (string.IsNullOrEmpty(ip))
        {
            startGuestButton.GetComponent<RectTransform>().DOPunchPosition(Vector3.one * 10, .5f, 30);
            ShowError("유효한 IP 주소를 입력해주세요.");
            return;
        }

        manager.networkAddress = ip;

        try
        {
            manager.StartClient();
        }
        catch
        {
            print("서버 연결 실패");
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
