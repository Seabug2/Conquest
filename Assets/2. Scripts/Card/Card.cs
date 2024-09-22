using UnityEngine;
using Mirror;

[RequireComponent(typeof(NetworkIdentity))]
public partial class Card : NetworkBehaviour
{
    #region 카드정보
    [SerializeField] int id;
    public int ID => id;

    public string cardName;
    
    // 0   1
    //
    // 3   2
    public readonly Socket[] Sockets = new Socket[4];
    #endregion
    
    public int ownerOrder;
    public Tile currentTile = null;

    public bool IsOnField => currentTile != null;

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

    #region 초기화
    void Awake()
    {
        SprtRend = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        GameManager.instance.AddCard(this);
        IsOpened = false;
    }
    #endregion
}