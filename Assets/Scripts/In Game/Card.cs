using UnityEngine;
using Mirror;

public partial class Card : NetworkBehaviour
{
    [SerializeField]
    CardInfo info;

    [SerializeField] int id;
    public int ID => id;

    [SerializeField]
    Player owner = null;
    public Player Owner => owner;

    public Card SetOwner(Player owner)
    {
        this.owner = owner;
        return this;
    }

    //소켓에 대한 정보
    // 0   1
    //
    // 3   2
    [Header("카드의 소켓"), Space(10)]
    [SerializeField] Socket[] sockets;
    public Socket[] Sockets => sockets;

    [Header("현재 카드가 놓여진 타일"), Space(10)]
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
            sprtRend.sprite = isOpened ? front : GameManager.instance.cardBackFace;
        }
    }

    public SpriteRenderer sprtRend { get; private set; }
    public Sprite front { get; private set; }
    private void Awake()
    {
        sprtRend = GetComponent<SpriteRenderer>();
        front = sprtRend.sprite;
    }

    private void Start()
    {
        IsOpened = false;
    }
}
