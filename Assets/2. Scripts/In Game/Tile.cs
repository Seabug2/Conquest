using UnityEngine;
using Mirror;

[RequireComponent(typeof(NetworkIdentity))]
public class Tile : NetworkBehaviour
{
    /*
     * Ÿ���� ��ȣ
     * ������ ī��, Ÿ�Ͽ� ������ ī�尡 �ִ��� ������ ��ȯ�ϴ� bool ��
     * 
     */

    [SerializeField] int tileIndex;
    public int TileIndex => tileIndex;

    public Card PlacedCard { get; private set; }
    public bool IsFilled => PlacedCard != null;

    public readonly Tile[] linkedTile = new Tile[4];
    public bool LinkableSocket(int i) => linkedTile[i] != null;

    readonly Socket[] linkedSocket = new Socket[4];

    public Socket this[int index]
    {
        get
        {
            if (index < 0 || index >= linkedSocket.Length)
            {
                Debug.Log("�ε��� ����");
                return null;
            }
            return linkedSocket[index];
        }
    }

    public bool IsSetable(Card _card)
    {
        if (IsFilled) return false; //ī�尡 �̹� ���� ������ X

        for (int i = 0; i < 4; i++)
        {
            //�������� ī��� Ÿ���� ������ ���Ѵ�
            if (LinkableSocket(i)) continue; //����� ������ ���� �𼭸��� �˻� ����

            //Ÿ�� ������ ��������鼭
            if (linkedSocket[i].attribute == Attribute.isEmpty)
            {
                // ī���� ������ Ȱ��ȭ�� ���°� �ƴ϶�� X
                if (!_card.Sockets[i].isActive)
                    return false;
            }
            //Ÿ�� ������ ����� ���¸鼭
            else
            {
                if (linkedSocket[i].attribute != _card.Sockets[i].attribute)
                    return false;
            }
        }

        return true;
    }

    public void SetSocket()
    {
        for(int i = 0; i < linkedSocket.Length;  i++)
        {
            if (linkedTile[i] == null) continue;

            int link = (i + 2) % 4;
            if (linkedTile[i][link] == null)
            {
                linkedSocket[i] = new();
            }
            else
            {
                linkedSocket[i] = linkedTile[i][link];
            }
        }
    }

    public void ShowPlaceableTiles(Card _card, bool _isActive)
    {
        if (_isActive)
        {
            if (IsSetable(_card))
            {
                //ī�带 ���� �� �ִ� Ÿ������ ǥ����
                transform.localScale = Vector3.one * 1.2f;
                //ī�带 �巡���Ͽ� Ÿ�Ϸ� �̵���Ű�� ī�带 Ÿ�Ͽ� �� �� �ְ� ��
                return;
            }
        }
        transform.localScale = Vector3.one;
    }

    public void SetCard(Card _card)
    {
        PlacedCard = _card;

        for (int i = 0; i < 4; i++)
        {
            if (linkedSocket[i].attribute == Attribute.unlinked) continue;

            linkedSocket[i].attribute = _card.Sockets[i].attribute;
        }
    }

    public void ClearCard()
    {
        PlacedCard = null;

        for (int i = 0; i < linkedSocket.Length; i++)
        {
            if (linkedSocket[i].attribute == Attribute.unlinked) continue;

            if (linkedTile[i] == null || linkedTile[i].PlacedCard == null)
            {
                linkedSocket[i].attribute = Attribute.isEmpty;
            }
        }
    }

    #region ī�� ����
    //���� Ŭ���̾�Ʈ���� ����� Ÿ�Ͽ��� Rpc ����� ������ ����
    //���������� ������ Command �޼��带 ȣ���Ѵ�.

    //ī�忡�� raycast�� ����Ͽ� ������ ��, ī���� id�� �Ű������� �Ͽ� �� �޼��带 ȣ��
    [Command(requiresAuthority = false)]
    public void CmdSetCard(int _id)
    {
        RpcSetCard(_id);
    }

    [ClientRpc]
    public void RpcSetCard(int _id)
    {
        Card card = GameManager.Card(_id);

        SetCard(card);

        /*
         * ī�带 ���� ����, ���� ī�带 ������ �� ���� ���´�.
        commander.Clear()
            .Add(() =>
            {
                //UIMaster.Message.ForcePopUp($"{card.name} ����!", )
            });
        //���� ī�忡�� �� �� �ִ� ī�尡 �����ִ��� Ȯ��
        //�� �̻� �� �� �ִ� ī�尡 ���ٸ� �ٷ� ���ʸ� ��ģ��.
         *���� �� �� ���Ҵٸ� ���� ī�带 ������ �� �ְ� �Ѵ�.
        */

        card.iCardState = GameManager.instance.noneState;

        card.currentTile = this;
        card.SetTargetPosition(transform.position);
        card.SetTargetQuaternion(transform.rotation);
        card.DoMove();
    }
    #endregion

    // �����Ϳ����� ����� �׸���
#if UNITY_EDITOR
    private const float maxLineLength = 1f;  // ����� ���� �ִ� ����
    private void OnDrawGizmosSelected()
    {
        if (linkedTile.Length != 4)
            return;

        // �� �ε����� ���� ���� �迭
        Color[] colors = { Color.red, Color.blue, Color.yellow, Color.green };

        // �ڽ��� ��ġ
        Vector3 startPosition = transform.position;

        // for������ �迭�� ��ȸ�ϸ鼭 ����� �׸���
        for (int i = 0; i < linkedTile.Length; i++)
        {
            if (linkedTile[i] != null)  // null�� �ƴ� ��쿡�� �׸���
            {
                Gizmos.color = colors[i];  // �ε����� �´� ���� ����

                // Ÿ���� ��ġ
                Vector3 targetPosition = linkedTile[i].transform.position;

                // ���� ���� ��� �� ����ȭ (Normalize) �� �ִ� ���̷� ����
                Vector3 direction = (targetPosition - startPosition).normalized;
                float distance = Vector3.Distance(startPosition, targetPosition);

                // �ִ� ���̷� ���ѵ� Ÿ�� ��ġ ���
                if (distance > maxLineLength)
                {
                    targetPosition = startPosition + direction * maxLineLength;
                }

                // �� �׸���
                Gizmos.DrawLine(startPosition, targetPosition);
            }
        }
    }
#endif
}
