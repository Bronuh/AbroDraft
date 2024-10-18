using Godot;
using KludgeBox;
using KludgeBox.Events;
using KludgeBox.Networking;
using NeonWarfare.NetOld.Server;

namespace NeonWarfare;

public static class PlayerMovementService
{

    [EventListener(ListenerSide.Server)]
    public static void OnClientMovementPlayerPacket(ClientMovementPlayerPacket clientMovementPlayerPacket)
    {
        Player player = ServerRoot.Instance.Game.World.NetworkEntityManager.GetNode<Player>(clientMovementPlayerPacket.Nid);
        Vector2 newPosition = Vec(clientMovementPlayerPacket.X, clientMovementPlayerPacket.Y);
        long nid = ServerRoot.Instance.Game.World.NetworkEntityManager.GetNid(player);
        

        player.Position = newPosition;
        player.Rotation = clientMovementPlayerPacket.Dir;
        
        foreach (PlayerServerInfo playerServerInfo in ServerRoot.Instance.Game.Server.PlayerServerInfo.Values)
        {
            if (playerServerInfo.Player == player) continue; //Отправляем коры игрока всем кроме самого игрока

            ServerMovementEntityPacket packet = new ServerMovementEntityPacket(
                nid,
                player.Position.X,
                player.Position.Y,
                player.Rotation,
                clientMovementPlayerPacket.MovementDir,
                clientMovementPlayerPacket.MovementSpeed);
            Network.SendToClient(playerServerInfo.Id, packet);
        }
    }
}