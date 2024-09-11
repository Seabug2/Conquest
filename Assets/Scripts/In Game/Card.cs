using UnityEngine.EventSystems;
using UnityEngine;

public class Card : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    CardInfo info;

    [SerializeField] int id;
    public int ID => id;

    #region
    [SerializeField] string cardName;
    public string CardName => cardName;

    [SerializeField] string favorText;
    public string FavorText => favorText;

    [SerializeField] string description;
    public string Description => description;
    #endregion

    [SerializeField]
    NetworkPlayer owner = null;
    public NetworkPlayer Owner => owner;

    //소켓에 대한 정보
    // 0   1
    //
    // 3   2
    [Header("카드의 소켓"), Space(10)]
    [SerializeField] Socket[] sockets;
    public Socket[] Sockets => sockets;

    [Header("현재 카드가 놓여진 타일"), Space(10)]
    public Tile currentTile = null;
    public bool IsOnField
    {
        get { return currentTile != null; }
    }

    public bool isOpened = false;
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

    public CardHandler handler;
    public SpriteRenderer sprtRend;
    public Sprite front;

    private void Awake()
    {
        handler = GetComponent<CardHandler>();
        handler.enabled = false;

        sprtRend = GetComponent<SpriteRenderer>();
        front = sprtRend.sprite;
    }

    private void Start()
    {
        isOpened = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isOpened)
        {
            UIController.InfoUI.ShowCardInfo(id);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UIController.InfoUI.gameObject.SetActive(false);
    }
}
