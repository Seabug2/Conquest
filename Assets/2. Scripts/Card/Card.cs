using UnityEngine;
using Mirror;

[RequireComponent(typeof(NetworkIdentity))]
public partial class Card
{
    #region 카드정보
    [SyncVar(hook = nameof(Register))]
    public int id = -1;

    void Register(int _, int @new)
    {
        if(GameManager.instance != null)
        {
            GameManager.instance.RegisterCard(@new, this);
        }
    }

    public string cardName;

    // 0   1
    //
    // 3   2
    public Socket[] Sockets = new Socket[4];
    #endregion

    public int ownerOrder;
    public Tile currentTile = null;

    public IAbility ability = null;

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
            SprtRend.sprite = isOpened ? front : back;
        }
    }

    public bool IsOnMouse = false;

    SpriteRenderer sprtRend;
    public SpriteRenderer SprtRend
    {
        get
        {
            if (sprtRend == null)
            {
                sprtRend = GetComponent<SpriteRenderer>();
            }
            return sprtRend;
        }
    }

    Sprite front;
    public Sprite Front => front;
    Sprite back;

    public void SetSprite(Sprite front, Sprite back)
    {
        this.front = front;
        this.back = back;
    }
}