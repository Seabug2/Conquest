using UnityEngine;
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
                Debug.Log("인덱스 오류");
                return null;
            }
            return linkedSocket[index];
        }
    }

    public bool IsSetable(Card _card)
    {
        if (IsFilled) return false; //카드가 이미 놓여 있으면 X

        for (int i = 0; i < 4; i++)
        {
            //놓으려는 카드와 타일의 소켓을 비교한다
            if (LinkableSocket(i)) continue; //연결된 소켓이 없는 모서리는 검사 생략

            //타일 소켓이 비어있으면서
            if (linkedSocket[i].attribute == Attribute.isEmpty)
            {
                // 카드의 소켓이 활성화된 상태가 아니라면 X
                if (!_card.Sockets[i].isActive)
                    return false;
            }
            //타일 소켓이 연결된 상태면서
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
                //카드를 놓을 수 있는 타일임을 표시함
                transform.localScale = Vector3.one * 1.2f;
                //카드를 드래그하여 타일로 이동시키면 카드를 타일에 둘 수 있게 됨
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

        SetCard(card);

        /*
         * 카드를 내는 순간, 손의 카드를 조작할 수 없게 막는다.
        commander.Clear()
            .Add(() =>
            {
                //UIMaster.Message.ForcePopUp($"{card.name} 등장!", )
            });
        //손의 카드에서 낼 수 있는 카드가 남아있는지 확인
        //더 이상 낼 수 있는 카드가 없다면 바로 차례를 마친다.
         *아직 할 게 남았다면 손의 카드를 조작할 수 있게 한다.
        */

        card.iCardState = GameManager.instance.noneState;

        card.currentTile = this;
        card.SetTargetPosition(transform.position);
        card.SetTargetQuaternion(transform.rotation);
        card.DoMove();
    }
    #endregion

    // 에디터에서만 기즈모 그리기
#if UNITY_EDITOR
    private const float maxLineLength = 1f;  // 기즈모 선의 최대 길이
    private void OnDrawGizmosSelected()
    {
        if (linkedTile.Length != 4)
            return;

        // 각 인덱스에 대한 색상 배열
        Color[] colors = { Color.red, Color.blue, Color.yellow, Color.green };

        // 자신의 위치
        Vector3 startPosition = transform.position;

        // for문으로 배열을 순회하면서 기즈모 그리기
        for (int i = 0; i < linkedTile.Length; i++)
        {
            if (linkedTile[i] != null)  // null이 아닌 경우에만 그리기
            {
                Gizmos.color = colors[i];  // 인덱스에 맞는 색상 선택

                // 타일의 위치
                Vector3 targetPosition = linkedTile[i].transform.position;

                // 방향 벡터 계산 후 정규화 (Normalize) 및 최대 길이로 제한
                Vector3 direction = (targetPosition - startPosition).normalized;
                float distance = Vector3.Distance(startPosition, targetPosition);

                // 최대 길이로 제한된 타겟 위치 계산
                if (distance > maxLineLength)
                {
                    targetPosition = startPosition + direction * maxLineLength;
                }

                // 선 그리기
                Gizmos.DrawLine(startPosition, targetPosition);
            }
        }
    }
#endif
}
