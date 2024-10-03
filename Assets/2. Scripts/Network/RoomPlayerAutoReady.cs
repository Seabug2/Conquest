using Mirror;

public class RoomPlayerAutoReady : NetworkRoomPlayer
{
    public override void OnStartLocalPlayer()
    {
        CmdChangeReadyState(true);
    }
}
