using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Deck : NetworkBehaviour
{
    Villain[] villains = new Villain[54];
    public Villain[] Villains => villains;

    public readonly SyncList<int> deckRemaining = new SyncList<int>();
    public int Count { get { return deckRemaining.Count; } }

    private void Start()
    {
        if (isServer)
            for (int i = 0; i < 54; i++)
            {
                deckRemaining.Add(i);
            }
    }

    /// <summary>
    /// SyncList를 동기화화기 위해서 서버에서만 실행되어야 함
    /// </summary>
    /// <param name="drawnId">덱에서 꺼낼 ID</param>
    [Server]
    public void CmdDraw(int drawnId)
    {
        deckRemaining.Remove(drawnId);
    }
}
