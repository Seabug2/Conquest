using UnityEngine;
using UnityEngine.UI;
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
    public bool IsEmpty => PlacedCard == null;

    public readonly Tile[] linkedTile = new Tile[4];

    Socket EmptySocket = new(Attribute.IsEmpty, false);

    SpriteRenderer sprtRend;

    private void Start()
    {
        sprtRend = GetComponent<SpriteRenderer>();
    }

    public Socket LinkedSocket(int i)
    {
        if (linkedTile[i] == null) return EmptySocket;

        int j = (i + 2) % 4;
        return linkedTile[i].PlacedCard.Sockets[j];
    }

    //���� ������ �ٽ�...
    public bool IsSetable(Card _card)
    {
        //ī�尡 �̹� �������ִ� Ÿ���̸� ī�带 ���� �� �� ����.
        if (PlacedCard != null) return false; //ī�尡 �̹� ���� ������ X

        //Ÿ�Ͽ� ������ �ִ� ī�尡 ���� ���,
        for (int i = 0; i < 4; i++)
        {
            //����� Ÿ���� ���ų�, ����� Ÿ���� ������� ��
            if (linkedTile[i] == null || linkedTile[i].IsEmpty)
            {
                if (!_card.Sockets[i].isActive)
                {
                    return false;
                }
                continue;
            }

            if (!linkedTile[i].PlacedCard.IsOpened) return false;

            if (!LinkedSocket(i).attribute.Equals(_card.Sockets[i].attribute)) return false;
        }

        return true;
    }

    public void ShowPlaceableTiles(Card _card, bool _isActive)
    {
        if (_isActive && IsSetable(_card))
        {
            transform.localScale = Vector3.one * 1.2f;
            sprtRend.color = Color.yellow;
            return;
        }

        transform.localScale = Vector3.one;
        sprtRend.color = Color.white;
    }

    public void Clear()
    {
        PlacedCard.currentTile = null;
        PlacedCard = null;
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
        UIManager.GetUI<LineMessage>().PopUp($"{card.cardName} ����!", 1.5f);

        card.iCardState = GameManager.instance.noneState;
        card.IsOpened = true;

        PlacedCard = card;
        card.currentTile = this;

        card.SprtRend.sortingLayerName = "Field";
        card.transform.localScale = Vector3.one;

        card.SetTargetPosition(transform.position);
        card.SetTargetQuaternion(transform.rotation);
        card.DoMove(() =>
        {
            int order = card.ownerOrder;
            CameraController.instance.DoShake(order, 0.2f, 1.5f);

            Player player = GameManager.GetPlayer(order);
            if (player.isLocalPlayer)
            {
                if (AroundCheck(out Tile completedTile))
                {
                    CameraController.instance.Freeze(true);
                    UIManager.GetUI<Timer>().Pause();
                    completedTile.OnCompleteLink();
                    return;
                }

                if (player != null && player.Hand.Count == 0)
                {
                    UIManager.GetUI<LineMessage>().ForcePopUp("��� ������ ī�尡 �����ϴ�!", 2f);
                    player.ClientEndTurn();
                }
            }
        }, 0.34f);
    }
    #endregion

    bool AroundCheck(out Tile completedTile)
    {
        if (CompleteLink())
        {
            completedTile = this;
            return true;
        }
        foreach (Tile t in linkedTile)
        {
            if (t != null && t.CompleteLink())
            {
                completedTile = t;
                return true;
            }
        }
        completedTile = null;
        return false;
    }

    bool CompleteLink()
    {
        if (IsEmpty) return false;
        foreach (Tile t in linkedTile)
        {
            if (t == null || t.IsEmpty) return false;
        }
        return true;
    }

    readonly Commander commander = new();
    private void OnDestroy()
    {
        commander.Cancel();
    }

    //[Command(requiresAuthority =false)]
    void OnCompleteLink()
    {
        int[] linkedIds = new int[5];
        for (int i = 0; i < 4; i++)
        {
            linkedIds[i] = linkedTile[i].PlacedCard.id;
        }
        linkedIds[4] = PlacedCard.id;
        CmdOnCompleteLink(linkedIds);
    }

    [Command(requiresAuthority = false)]
    void CmdOnCompleteLink(int[] ids)
    {
        RpcOnCompleteLink(ids);
    }

    [ClientRpc]
    void RpcOnCompleteLink(int[] ids)
    {
        int order = PlacedCard.ownerOrder;
        commander
            .Refresh()
            .Add(() =>
            {
                CameraController.instance.FocusOnPlayerField(PlacedCard.ownerOrder);
                CameraController.instance.Freeze(true);
                foreach (int i in ids)
                {
                    Card card = GameManager.Card(i);
                    card.currentTile.PlacedCard = null;
                    card.currentTile = null;
                }
                UIManager.GetUI<LineMessage>().ForcePopUp("��ũ �ϼ�!", 2f);
            }
            , 2.2f)
            .Add(() =>
            {
                if (GameManager.GetPlayer(order).isLocalPlayer)
                    GameManager.Deck.CmdReturnCard(ids, true);
            })
            .WaitSeconds(1f)
            .Add(() =>
            {
                UIManager.GetUI<Timer>().Resume();
                CameraController.instance.Freeze(false);
            })
            .Play();
    }


    // �����Ϳ����� ����� �׸���
#if UNITY_EDITOR
    private const float maxLineLength = 1f;  // ����� ���� �ִ� ����
                                             // �� �ε����� ���� ���� �迭
    Color[] colors = { Color.red, Color.blue, Color.yellow, Color.green };

    private void OnDrawGizmosSelected()
    {
        // �ڽ��� ��ġ
        Vector3 startPosition = transform.position;

        // for������ �迭�� ��ȸ�ϸ鼭 ����� �׸���
        for (int i = 0; i < linkedTile.Length; i++)
        {
            if (linkedTile[i] == null) return;// null�� �ƴ� ��쿡�� �׸���

            Gizmos.color = colors[i];  // �ε����� �´� ���� ����

            // Ÿ���� ��ġ
            Vector3 targetPosition = linkedTile[i].transform.position;

            /*
            // ���� ���� ��� �� ����ȭ (Normalize) �� �ִ� ���̷� ����
            Vector3 direction = (targetPosition - startPosition).normalized;
            float distance = Vector3.Distance(startPosition, targetPosition);

            // �ִ� ���̷� ���ѵ� Ÿ�� ��ġ ���
            if (distance > maxLineLength)
            {
                targetPosition = startPosition + direction * maxLineLength;
            }
            */

            // �� �׸���
            Gizmos.DrawLine(startPosition, targetPosition);
        }
    }
#endif
}
