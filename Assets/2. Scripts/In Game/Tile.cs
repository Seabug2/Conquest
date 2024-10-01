using UnityEngine;
using UnityEngine.UI;
using Mirror;

[RequireComponent(typeof(NetworkIdentity))]
public class Tile : NetworkBehaviour
{
    /*
     * 타일의 번호
     * 놓여진 카드, 타일에 놓여진 카드가 있는지 없는지 반환하는 bool 값
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

    //게임 로직의 핵심...
    public bool IsSetable(Card _card)
    {
        //카드가 이미 놓여져있는 타일이면 카드를 내려 둘 수 없다.
        if (PlacedCard != null) return false; //카드가 이미 놓여 있으면 X

        //타일에 놓여져 있는 카드가 없는 경우,
        for (int i = 0; i < 4; i++)
        {
            //연결된 타일이 없거나, 연결된 타일이 비어있을 때
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

    #region 카드 놓기
    //로컬 클라이언트에서 지목된 타일에게 Rpc 명령을 내리기 위해
    //권한제한이 해제된 Command 메서드를 호출한다.

    //카드에서 raycast를 사용하여 검출한 후, 카드의 id를 매개변수로 하여 이 메서드를 호출
    [Command(requiresAuthority = false)]
    public void CmdSetCard(int _id)
    {
        RpcSetCard(_id);
    }

    [ClientRpc]
    public void RpcSetCard(int _id)
    {
        Card card = GameManager.Card(_id);
        UIManager.GetUI<LineMessage>().PopUp($"{card.cardName} 등장!", 1.5f);

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

    // 에디터에서만 기즈모 그리기
#if UNITY_EDITOR
    private const float maxLineLength = 1f;  // 기즈모 선의 최대 길이
                                             // 각 인덱스에 대한 색상 배열
    Color[] colors = { Color.red, Color.blue, Color.yellow, Color.green };

    private void OnDrawGizmosSelected()
    {
        // 자신의 위치
        Vector3 startPosition = transform.position;

        // for문으로 배열을 순회하면서 기즈모 그리기
        for (int i = 0; i < linkedTile.Length; i++)
        {
            if (linkedTile[i] == null) return;// null이 아닌 경우에만 그리기

            Gizmos.color = colors[i];  // 인덱스에 맞는 색상 선택

            // 타일의 위치
            Vector3 targetPosition = linkedTile[i].transform.position;

            /*
            // 방향 벡터 계산 후 정규화 (Normalize) 및 최대 길이로 제한
            Vector3 direction = (targetPosition - startPosition).normalized;
            float distance = Vector3.Distance(startPosition, targetPosition);

            // 최대 길이로 제한된 타겟 위치 계산
            if (distance > maxLineLength)
            {
                targetPosition = startPosition + direction * maxLineLength;
            }
            */

            // 선 그리기
            Gizmos.DrawLine(startPosition, targetPosition);
        }
    }
#endif
}
