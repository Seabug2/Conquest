using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Mirror;
using System.IO;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Threading;

public enum GamePhase
{
    Begin,
    DraftPhase,
    PlayerPhase,
    End
}


[RequireComponent(typeof(NetworkIdentity))]
public class GameManager : NetworkBehaviour
{
    #region �̱���
    public static GameManager instance;

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

    #region �ʱ�ȭ
    string filePath = "DeckIdList.json";
    DeckIDList deckIdList;

    private void Start()
    {
        filePath = Path.Combine(Application.persistentDataPath, filePath);
        deckIdList = LoadDeckIdList();
        CardSetting();

        CurrentPhase = GamePhase.Begin;
    }
    #endregion

    #region ����
    [SerializeField] bool[] isAcknowledged = new bool[maxPlayer];

    [Command(requiresAuthority = false)]
    public void CmdReply(int order)
    {
        Debug.Log($"{order} ���� ��");

        if (order < 0 || order >= maxPlayer)
        {
            Debug.LogError($"�ε��� ���� ({order}) : ���� �ʰ�");
            return;
        }

        isAcknowledged[order] = true;
        Debug.Log($"{order} : {isAcknowledged[order]}");
    }

    public bool IsAllReceived()
    {
        for (int i = 0; i < maxPlayer; i++)
        {
            Player player = GetPlayer(i);
            if (player == null || player.isGameOver) continue;
            if (!isAcknowledged[i]) return false;
        }

        return true;
    }

    [Server]
    public void ResetAcknowledgements()
    {
        for (int i = 0; i < maxPlayer; i++)
        {
            isAcknowledged[i] = false;
        }
    }

    [Server]
    public async UniTask WaitForAllAcknowledgements(int timeoutMilliseconds = 20000)
    {
        // CancellationTokenSource ����
        var cts = new CancellationTokenSource();

        // ������ �ð� �Ŀ� CancellationToken�� �����
        cts.CancelAfter(timeoutMilliseconds);

        // ������ �ð� ���� ������ ��ٸ��� �����Ѵ�.
        await UniTask.WaitUntil(IsAllReceived, cancellationToken: cts.Token).SuppressCancellationThrow();

        if (!IsAllReceived())
        {
            Debug.LogWarning("������ ���� ���� �÷��̾ �ֽ��ϴ�. ���� ���¸� Ȯ���մϴ�.");
            CheckDisconnectedPlayers();
        }
        else
        {
            Debug.Log("���� �Ϸ�!");
        }

        ResetAcknowledgements();
    }

    [Server]
    public void CheckDisconnectedPlayers()
    {
        for (int i = 0; i < maxPlayer; i++)
        {
            //������ �� 
            if (isAcknowledged[i]) continue;

            Player player = GetPlayer(i);
            if (player == null)
            {
                Debug.LogError($"Player {i}�� ������ �����߽��ϴ�...");
            }
            else if (player.isGameOver)
            {
                // ���⿡�� �ش� �÷��̾��� ���� ���¿� ���� �߰� ó��
                Debug.LogError($"Player {i}�� Ż�������Ƿ� �׳� �Ѿ�ϴ�.");
            }
            else
            {
                //TODO �������� �ʴ� �÷��̾ ���� ����ó�� �ʿ�
                Debug.LogError($"Player {i}�� ������ �����ϴ�...");
            }
        }
    }

    #endregion

    #region �÷��̾�
    public const int maxPlayer = 4;

    [SerializeField, Header("�÷��̾� ����Ʈ")]
    List<Player> players = new();

    public static Player GetPlayer(int i)
    {
        if (i < 0 || i >= maxPlayer)
        {
            Debug.LogError("�߸��� �ε���");
            return null;
        }
        return instance.players[i];
    }

    public static Player LocalPlayer { get; private set; }

    public void AddPlayer(Player _player)
    {
        if (_player.isLocalPlayer)
        {
            LocalPlayer = _player;
        }

        players.Add(_player);

        if (isServer && players.Count == maxPlayer)
        {
            StartGame();
        }
    }

    [Server]
    public void RemovePlayer(Player _player)
    {
        if (players.Contains(_player))
        {
            //������ ���� �������� ���� ���¶��,
            if (!IsPlaying)
            {
                //�÷��̾� ����Ʈ���� �����Ͽ� ����Ʈ�� ũ�⸦ ������� �Ѵ�.
                players.Remove(_player);
            }
            else
            {
                int i = players.IndexOf(_player);
                players[i] = null;
            }
        }

        if (isServer)
        {
            if (AliveCount > 1)
            {
                if (_player.isMyTurn)
                {
                    EndTurn(_player.Order);
                    return;
                }
                else
                {
                    isAcknowledged[_player.Order] = true;
                }
            }
            else
            {
                //� �÷��̾ ������ �������� ��
                //���� ������ �� ���̶�� �� ��� ������ ���� ����.
                RpcEndGame(LastPlayer());
                return;
            }
        }
    }

    [Server]
    int LastPlayer()
    {
        int last = -1;

        foreach (Player p in players)
        {
            if (p != null && !p.isGameOver)
            {
                last = p.Order;
            }
        }

        return last;
    }

    [Server]
    public void PlayerShuffle()
    {
        int rand;
        Player tmp;
        for (int i = 0; i < maxPlayer; i++)
        {
            rand = UnityEngine.Random.Range(i, maxPlayer); // i���� ���� �ε������� ����
            tmp = players[i];
            players[i] = players[rand];
            players[rand] = tmp;
        }


        for (int i = 0; i < maxPlayer; i++)
        {
            players[i].RpcSetOrder(i);

            //ȣ��Ʈ�� �ƴ� ���������� ������ ����ȭ �� �� �ֵ���...
            if (isServerOnly)
            {
                players[i].SetOrder(i);
            }
        }
    }

    [Server]
    async void StartGame()
    {
        PlayerShuffle(); //�÷��̾��� order�� �ٲٸ� hook�� ���� Reply()�� ���� �� �ִ�.

        await WaitForAllAcknowledgements();

        RpcSortPlayerList();

        //ȣ��Ʈ�� �ƴ� ������ ��쿡�� ����ȭ �� �� �ֵ���...
        if (isServerOnly)
        {
            players.Sort((a, b) => a.Order.CompareTo(b.Order));
        }

        await WaitForAllAcknowledgements();

        IsPlaying = true;

        RpcStartGame();

        await WaitForAllAcknowledgements();

        deck.ServerDraftPhase();
    }

    [ClientRpc]
    void RpcSortPlayerList()
    {
        players.Sort((a, b) => a.Order.CompareTo(b.Order));
        CmdReply(LocalPlayer.Order);
    }

    public static int AliveCount
    {
        get
        {
            int alivePlayerCount = maxPlayer;

            foreach (Player p in instance.players)
            {
                //Ŭ���̾�Ʈ�� �����߰ų�,
                //���� ������ �÷��̾�
                if (p == null || p.isGameOver)
                {
                    alivePlayerCount--;
                }
            }

            return alivePlayerCount;
        }
    }
    #endregion

    #region ī��
    [Header("ī�� �޸� �̹���"), Space(10)]
    public Sprite cardBackFace;

    [Header("ī��"), Space(10)]
    public Card[] cards;

    readonly Dictionary<int, Card> dict_Card = new();
    public static Card Card(int id) => instance.dict_Card[id];
    public static int TotalCard => instance.cards.Length;

    void CardSetting()
    {
        if (cards.Length == 0) return;

        TextAsset csvFile = Resources.Load<TextAsset>("Conquest_Info");

        if (csvFile == null)
        {
            Debug.LogError("CSV ������ ã�� �� �����ϴ�.");
            return;
        }

        string[] dataLines = csvFile.text.Split(new char[] { '\n' });
        if (dataLines.Length <= 1)
        {
            Debug.LogError("CSV ���Ͽ� �����Ͱ� �����մϴ�.");
            return;
        }

        //ī�忡 ȿ���� �Ҵ��ϱ� ���� �Ŵ���
        AbilityManager abilityManager = new AbilityManager();

        for (int i = 0; i < 54; i++)
        {
            cards[i].id = deckIdList[i];
            dict_Card.Add(deckIdList[i], cards[i]);

            int lineIndex = cards[i].id + 1;
            if (lineIndex >= dataLines.Length)
            {
                Debug.LogError($"������ ���� �ε����� ������ ������ϴ�: {lineIndex}");
                continue;
            }

            string line = dataLines[lineIndex].Trim();
            if (string.IsNullOrWhiteSpace(line))
            {
                Debug.LogWarning($"�� ���� �߰ߵǾ����ϴ�: {lineIndex}");
                continue;
            }



            string[] data = line.Split(',');
            //if (data.Length < �ּ�����_������_����)
            //{
            //    Debug.LogError($"�����Ͱ� �����մϴ�: {lineIndex}");
            //    continue;
            //}



            string imageName = data[1];
            Sprite cardFront = Resources.Load<Sprite>("Card/" + imageName);  // Resources/Card �������� �̹��� �ε�

            if (cardFront == null)
            {
                cards[i].front = null;
                Debug.LogError($"�̹����� ã�� �� �����ϴ�: {imageName}");
            }
            else
            {
                cards[i].front = cardFront;  // ī���� front ��������Ʈ �Ҵ�
            }

            cards[i].name = data[2];
            cards[i].cardName = data[2];

            cards[i].Sockets[0] = new Socket(ParseAttribute(data[3]), data[4].Equals("1"));
            cards[i].Sockets[1] = new Socket(ParseAttribute(data[5]), data[6].Equals("1"));
            cards[i].Sockets[2] = new Socket(ParseAttribute(data[7]), data[8].Equals("1"));
            cards[i].Sockets[3] = new Socket(ParseAttribute(data[9]), data[10].Equals("1"));


            // TODO ī�� ȿ�� ������ �Ϸ��ϸ� ����
            continue;

            if (int.TryParse(data[11], out int abilityId))
            {
                string[] extractedData = data.Skip(12).Take(4).ToArray();
                cards[i].ability = abilityManager.Create(abilityId, extractedData);
            }
        }
    }

    Attribute ParseAttribute(string value)
    {
        if (int.TryParse(value, out int result) && System.Enum.IsDefined(typeof(Attribute), result))
        {
            return (Attribute)result;
        }
        return Attribute.IsEmpty;  // ��ȯ ���� �� �⺻�� ��ȯ
    }
    #endregion

    #region ����
    int currentOrder;

    public static int CurrentOrder => instance.currentOrder;

    [Server]
    public void SetCurrentOrder(int newOrder)
    {
        currentOrder = newOrder;
        RpcSetCurrentOrder(newOrder);
    }

    [ClientRpc]
    public void RpcSetCurrentOrder(int newOrder)
    {
        GetPlayer(currentOrder).isMyTurn = false;

        currentOrder = newOrder;

        GetPlayer(currentOrder).isMyTurn = true;
        GetPlayer(currentOrder).hasTurn = true;

        CmdReply(LocalPlayer.Order);
    }

    [SyncVar] int firstOrder = 0;
    public static int FirstOrder => instance.firstOrder;

    /// <summary>
    /// ���ο� ���带 �����ϱ� ���� ���ʸ� ó������ �ǵ����ϴ�.
    /// </summary>
    [Server]
    public void SetNewRound()
    {
        //���ο� ���带 �����ϱ� ����
        //�̹� ������ ù ��° �÷��̾ ã�´�.
        ResetAcknowledgements();
        ResetHasTurn();

        //������ ù ��° �÷��̾ ��ȿ������ Ȯ��
        if (players[firstOrder] == null || players[firstOrder].isGameOver)
        {
            do
            {
                firstOrder = (firstOrder + (IsClockwise ? 1 : maxPlayer - 1)) % maxPlayer;
            }
            while (players[firstOrder] == null || players[firstOrder].isGameOver);
        }

        SetCurrentOrder(firstOrder);
    }

    public int NextOrder(int _Order)
    {
        do
        {
            _Order = (_Order + (IsClockwise ? 1 : maxPlayer - 1)) % maxPlayer;
        }
        while (players[_Order] == null || players[_Order].isGameOver);

        return _Order;
    }

    public int PreviousOrder(int _Order)
    {
        do
        {
            _Order = (_Order + (IsClockwise ? maxPlayer - 1 : 1)) % maxPlayer;
        }
        while (players[_Order] == null || players[_Order].isGameOver);

        return _Order;
    }

    [SyncVar]
    public bool IsClockwise = true;

    #endregion

    public GamePhase CurrentPhase = GamePhase.Begin;

    public static Deck deck;

    public static Dictionary<int, Field> dict_Field = new();

    public static Dictionary<int, Hand> dict_Hand = new();

    /// <summary>
    /// �̸� �����ص� ���¸� �����Ͽ� ��Ȱ�뼺�� ���Դϴ�.
    /// </summary>
    public readonly ICardState noneState = new NoneState();

    #region ����
    [SyncVar]
    public bool IsPlaying = false;

    [Server]
    public bool RoundFinished()
    {
        foreach (Player p in instance.players)
        {
            if (p == null) continue;

            //���ʸ� ������ ���� �÷��̾ �ִٸ�...
            if (!p.hasTurn)
            {
                return false;
            }
        }

        ResetHasTurn();
        return true;
    }

    [Server]
    public void ResetHasTurn()
    {
        foreach (Player p in instance.players)
        {
            if (p != null)
                p.hasTurn = false;
        }
    }

    [Space(20)]
    [Header("���� ���� �̺�Ʈ")]
    public UnityEvent OnStartEvent;

    [ClientRpc]
    public void RpcStartGame()
    {
        new Commander()
            .Add(() =>
            {
                CameraController.instance.FocusOnCenter();
                UIManager.GetUI<Fade>().In(3f);
            })
            .WaitSeconds(3.3f)
            .Add(() =>
            {
                UIManager.GetUI<Timer>().Active(1.8f);
                UIManager.GetUI<HeadLine>().On(1.8f);
            })
            .WaitSeconds(1.8f)
            .Add(() => UIManager.GetUI<LineMessage>().ForcePopUp("������ 1���� �Ǽ���!", 3f)
            , 3.3f)
            .Add(() =>
            {
                Debug.Log(LocalPlayer.Order + "�� ���ö���");
                CmdReply(LocalPlayer.Order);
            })
            .Play();
    }

    [Server]
    public void EndTurn(int order)
    {
        if (isServerOnly)
        {
            CurrentPhase = GamePhase.End;
        }

        //������ ���� ���
        if (AliveCount == 1)
        {
            IsPlaying = false;
            RpcEndGame(LastPlayer());
            return;
        }

        //��� �� ���� ���ʸ� ���� ��� => ���� ���� �ð�!
        if (RoundFinished())
        {
            deck.ServerDraftPhase();
            return;
        }

        //���� ���ʰ� ���� ���
        SetCurrentOrder(NextOrder(order));
        GetPlayer(currentOrder).RpcStartTurn();
    }

    [Header("���� ���� �̺�Ʈ")]
    public UnityEvent OnEndGame;

    [ClientRpc]
    void RpcEndGame(int lastPlayerOrder)
    {
        CameraController.instance.CurrentCamIndex = lastPlayerOrder;
        CameraController.instance.MoveLock(true);
        OnEndGame?.Invoke();

        if (LocalPlayer.isGameOver)
        {
            UIManager.GetUI<LineMessage>().ForcePopUp("�й�...", 5);
        }
        else
        {
            UIManager.GetUI<LineMessage>().ForcePopUp("����� �¸��Դϴ�!", 5);
        }
    }
    #endregion


    /*
    public static Dictionary<int, List<Card>> dict_HandLimitStack = new Dictionary<int, List<Card>> {
    { 0, new List<Card>() },
    { 1, new List<Card>() },
    { 2, new List<Card>() },
    { 3, new List<Card>() }
    };

    /// <summary>
    /// �ڽ��� �� ���� �÷��̾ �ߵ��� ī���� �� ���� ȿ���� ���� ����Ͽ� �ڽ��� �� ���� ���� �����ɴϴ�.
    /// </summary>
    public static int GetPlayerHandLimit(int i)
    {
        int @default = 6;
        int previousOrder = PreviousOrder(i);
        foreach (Card c in dict_HandLimitStack[previousOrder])
        {
            @default -= 1;
            //@default += c.Limit...;
        }
        return @default;
    }

    public static Dictionary<int, List<Card>> dict_OnDrawEvent = new Dictionary<int, List<Card>> {
    { 0, new List<Card>() },
    { 1, new List<Card>() },
    { 2, new List<Card>() },
    { 3, new List<Card>() }
    };

    public static Dictionary<int, List<Card>> dict_OnDeployedEvent = new Dictionary<int, List<Card>> {
    { 0, new List<Card>() },
    { 1, new List<Card>() },
    { 2, new List<Card>() },
    { 3, new List<Card>() }
    };
    */


    #region Json : �� ����Ʈ
    public DeckIDList LoadDeckIdList()
    {
        if (File.Exists(filePath))
        {
            // ������ �����ϸ� ���Ͽ��� JSON�� �а� ��ü�� ��ȯ
            string json = File.ReadAllText(filePath);
            return JsonUtility.FromJson<DeckIDList>(json);
        }
        else
        {
            // ������ ������ �⺻ DeckIDList ��ü�� ����
            DeckIDList newDeck = new DeckIDList();
            SaveDeckIdList(newDeck);  // �⺻ DeckIDList�� ���Ϸ� ����
            return newDeck;
        }
    }

    // ���� ����
    public void SaveDeckIdList(DeckIDList deckIdList)
    {
        // JSON ���ڿ��� ��ȯ
        string json = JsonUtility.ToJson(deckIdList, true);

        // ���Ϸ� ����
        File.WriteAllText(filePath, json);

        Debug.Log($"DeckIdList �����: {filePath}");
    }
    #endregion
}

[Serializable]
public class DeckIDList
{
    public int[] idList;

    public int this[int i]
    {
        get
        {
            if (i < 0 || i >= idList.Length)
            {
                Debug.LogError("���� ����");
                return 0;
            }
            return idList[i];
        }
    }

    public DeckIDList()
    {
        idList = new int[54];

        for (int i = 0; i < 54; i++)
        {
            idList[i] = i;
        }
    }

    public DeckIDList(int[] idList)
    {
        this.idList = idList;
    }
}