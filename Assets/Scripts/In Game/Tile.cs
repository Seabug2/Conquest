using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] int tileIndex;
    public int TileIndex => tileIndex;

    //���� Ÿ�Ͽ� ������ 
    public Card placedCard = null;

    public bool[] linkableSocket = new bool[4];
    public Tile[] linkedTile = new Tile[4];

    [SerializeField, Header("���� ����")]
    Socket[] sockets = new Socket[4];
    public Socket this[int index]
    {
        get
        {
            if (index < 0 || index >= sockets.Length)
            {
                Debug.Log("�ε��� ����");
                return null;
            }
            return sockets[index];
        }

        set
        {
            if (index < 0 || index >= sockets.Length)
            {
                Debug.Log("�ε��� ����");
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
        if (placedCard != null) return false; //ī�尡 �̹� ���� ������ X

        for (int i = 0; i < 4; i++)
        {
            //�������� ī��� Ÿ���� ������ ���Ѵ�
            if (linkableSocket[i]) continue; //����� ������ ���� �𼭸��� �˻� ����

            //Ÿ�� ������ ��������鼭
            if (this.sockets[i].attribute == Attribute.isEmpty)
            {
                // ī���� ������ Ȱ��ȭ�� ���°� �ƴ϶�� X
                if (!_card.Sockets[i].isLinked)
                    return false;
            }
            //Ÿ�� ������ ����� ���¸鼭
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
                //ī�带 �巡���Ͽ� Ÿ�Ϸ� �̵���Ű�� ī�带 Ÿ�Ͽ� �� �� �ְ� ��
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
            if (linkableSocket[i]) continue; //����� ������ ���� �𼭸��� �˻� ����

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
