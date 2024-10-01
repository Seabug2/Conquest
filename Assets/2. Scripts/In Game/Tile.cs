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

        card.SprtRend.sortingLayerName = "Default";
        card.transform.localScale = Vector3.one;

        card.SetTargetPosition(transform.position);
        card.SetTargetQuaternion(transform.rotation);
        card.DoMove(() =>
        {
            int order = card.ownerOrder;
            CameraController.instance.DoShake(order, 0.2f, 1.5f);

            Player player = GameManager.GetPlayer(order);
            if (player != null && player.Hand.Count == 0)
            {
                player.Hand.SetHandlingState();
                player.CmdEndTurn(player.Hand.IsLimitOver);
            }
        }, 0.34f);
    }
    #endregion

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
