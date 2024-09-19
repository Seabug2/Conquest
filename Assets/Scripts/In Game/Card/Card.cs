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

    //소켓에 대한 정보
    // 0   1
    //
    // 3   2

    //[Header("카드의 소켓"), Space(10)]
    //[SerializeField]

    readonly Socket[] sockets = new Socket[4];
    public Socket[] Sockets => sockets;

    [Header("현재 카드가 놓여진 타일"), Space(10)]
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
