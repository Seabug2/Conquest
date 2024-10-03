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
    public IpInputField nextInputField; // ���� ��ǲ�ʵ带 ����
    public Button startGuestButton;     // ������ �ʵ忡�� ��ư���� �̵�

    public IpInputField()
    {
        ipf = null;
        nextInputField = null;
        startGuestButton = null;
    }

    public void ValidateInput(string input)
    {
        // ���Խ��� �̿��Ͽ� ���ڸ� ����� �������� ����
        ipf.text = Regex.Replace(input, "[^0-9]", "");

        // ���� 3���� �ԷµǸ� ���� ��ǲ�ʵ� �Ǵ� ��ư���� ��Ŀ�� �̵�
        if (ipf.text.Length == 3)
        {
            if (nextInputField != null)
            {
                nextInputField.ipf.Select();
            }
            else if (startGuestButton != null)
            {
                startGuestButton.Select(); // ������ �ʵ��� �� ��ư���� ��Ŀ�� �̵�
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

            // ������ ��ǲ�ʵ尡 �ƴ� ��쿡�� ���� �ʵ带 ����
            if (i < ipInputs.Length - 1)
            {
                ipInputs[i].nextInputField = ipInputs[i + 1];
            }
            else
            {
                ipInputs[i].startGuestButton = startGuestButton; // ������ ��ǲ�ʵ��� �� ��ư���� �̵�
            }
        }

        EventSystem.current.SetSelectedGameObject(ipInputs[currentPointer].ipf.gameObject);
    }

    void Update()
    {
        // Tab Ű�� ������ ��
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            // Shift Ű�� ���� ���¶�� ���� �ʵ�� �̵�
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                currentPointer = Mathf.Clamp(currentPointer - 1, 0, ipInputs.Length - 1);
            }
            // Shift ���� Tab�� ������ ���� �ʵ�� �̵�
            else
            {
                currentPointer = Mathf.Clamp(currentPointer + 1, 0, ipInputs.Length - 1);
            }

            EventSystem.current.SetSelectedGameObject(ipInputs[currentPointer].ipf.gameObject);
            return;
        }
        // �¿� ����Ű�� �̵�
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

        // Enter �Ǵ� Keypad Enter Ű�� ������ ��ư Ŭ��
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
            ShowError("��ȿ�� IP �ּҸ� �Է����ּ���.");
            return;
        }

        manager.networkAddress = ip;

        try
        {
            manager.StartClient();
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
