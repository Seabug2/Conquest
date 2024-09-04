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
    /// SyncList�� ����ȭȭ�� ���ؼ� ���������� ����Ǿ�� ��
    /// </summary>
    /// <param name="drawnId">������ ���� ID</param>
    [Server]
    public void CmdDraw(int drawnId)
    {
        deckRemaining.Remove(drawnId);
    }
}
