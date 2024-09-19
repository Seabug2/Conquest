using UnityEngine;
using Mirror;

[RequireComponent(typeof(NetworkIdentity))]
public partial class Card : NetworkBehaviour
{
    public int id;
    public string cardName;

    [SyncVar]
    public int ownerOrder = -1;
    public Player Owner => GameManager.Player(ownerOrder);

    //���Ͽ� ���� ����
    // 0   1
    //
    // 3   2

    //[Header("ī���� ����"), Space(10)]
    //[SerializeField]

    readonly Socket[] sockets = new Socket[4];
    public Socket[] Sockets => sockets;

    [Header("���� ī�尡 ������ Ÿ��"), Space(10)]
    Tile currentTile = null;
    public Tile CurrentTile => currentTile;
    public void SetTile(Tile currentTile)
    {
        this.currentTile = currentTile;
    }
    public bool IsOnField
    {
        get { return currentTile != null; }
    }


    bool isOpened = false;
    public bool IsOpened
    {
        get
        {
            return isOpened;
        }
        set
        {
            isOpened = value;
            SprtRend.sprite = isOpened ? front : GameManager.instance.cardBackFace;
        }
    }

    public bool IsOnMouse = false;

    public SpriteRenderer SprtRend { get; private set; }
    public Sprite front;

    private void Awake()
    {
        SprtRend = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        IsOpened = false;
    }
}
