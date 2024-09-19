using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] int tileIndex;
    public int TileIndex => tileIndex;

    //현재 타일에 전개한 
    public Card placedCard = null;

    public bool[] linkableSocket = new bool[4];
    public Tile[] linkedTile = new Tile[4];

    [SerializeField, Header("연결 소켓")]
    Socket[] sockets = new Socket[4];
    public Socket this[int index]
    {
        get
        {
            if (index < 0 || index >= sockets.Length)
            {
                Debug.Log("인덱스 오류");
                return null;
            }
            return sockets[index];
        }

        set
        {
            if (index < 0 || index >= sockets.Length)
            {
                Debug.Log("인덱스 오류");
            }
            sockets[index] = value;
        }
    }

    public void TileClear()
    {
        sockets = new Socket[4];
        placedCard = null;
    }

    public bool IsSetable(Card _card)
    {
        if (placedCard != null) return false; //카드가 이미 놓여 있으면 X

        for (int i = 0; i < 4; i++)
        {
            //놓으려는 카드와 타일의 소켓을 비교한다
            if (linkableSocket[i]) continue; //연결된 소켓이 없는 모서리는 검사 생략

            //타일 소켓이 비어있으면서
            if (this.sockets[i].attribute == Attribute.isEmpty)
            {
                // 카드의 소켓이 활성화된 상태가 아니라면 X
                if (!_card.Sockets[i].isLinked)
                    return false;
            }
            //타일 소켓이 연결된 상태면서
            else
            {
                if (this.sockets[i].attribute != _card.Sockets[i].attribute)
                    return false;
            }
        }

        return true;
    }

    public void ActiveTile(Card _card, bool _isActive)
    {
        if (_isActive)
        {
            if (IsSetable(_card))
            {
                transform.localScale = Vector3.one * 1.2f;
                //카드를 드래그하여 타일로 이동시키면 카드를 타일에 둘 수 있게 됨
                return;
            }
        }
        transform.localScale = Vector3.one;
    }

    public void SetCard(Card _card)
    {
        placedCard = _card;

        for (int i = 0; i < 4; i++)
        {
            if (linkableSocket[i]) continue; //연결된 소켓이 없는 모서리는 검사 생략

            sockets[i].attribute = _card.Sockets[i].attribute;
        }
    }

    public void Discard()
    {
        placedCard = null;

        for (int i = 0; i < 4; i++)
        {
            if (linkedTile[i].placedCard != null) continue;
            sockets[i].attribute = Attribute.isEmpty;
        }
    }
}
