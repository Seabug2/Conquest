using UnityEngine;
using Mirror;

[RequireComponent(typeof(NetworkIdentity))]
public partial class Card : NetworkBehaviour
{
    public int id;
    public string cardName;

    [SyncVar]
    public int ownerOrder = -1;
    public Player Owner
    {
        get
        {
            return GameManager.Player(ownerOrder);
        }
    }

    //���Ͽ� ���� ����
    // 0   1
    //
    // 3   2
    [Header("ī���� ����"), Space(10)]
    [SerializeField] Socket[] sockets = new Socket[4];
    public Socket[] Sockets => sockets;

    [Header("���� ī�尡 ������ Ÿ��"), Space(10)]
    Tile currentTile = null;
    public Tile CurrentTile => currentTile;

    public Card SetTile(Tile currentTile)
    {
        this.currentTile = currentTile;
        return this;
    }

    public bool IsOnField
    {
        get { return currentTile != null; }
    }

    [SerializeField]
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
        //Front = SprtRend.sprite;
    }

    private void Start()
    {
        IsOpened = false;
    }
}
