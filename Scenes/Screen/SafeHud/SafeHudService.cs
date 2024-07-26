using Godot;
using KludgeBox;
using KludgeBox.Events;
using KludgeBox.Networking;
using NeonWarfare.NetOld.Server;

namespace NeonWarfare;

[GamePacket]
public class ClientWantToBattlePacket : BinaryPacket;

public static class SafeHudService
{

    [EventListener(ListenerSide.Server)]
    public static void OnClientWantToBattlePacket(ClientWantToBattlePacket clientWantToBattlePacket)
    {
        Netplay.SendToAll(new ServerChangeWorldPacket(ServerChangeWorldPacket.ServerWorldType.Battle));
        BattleGameMainScene battleGameMainScene = ServerRoot.Instance.PackedScenes.GameMainScenes.BattleWorld.Instantiate<BattleGameMainScene>();
        ServerRoot.Instance.Game.ChangeMainScene(battleGameMainScene);
        ClientBattleWorld clientBattleWorld = battleGameMainScene.ClientBattleWorld;
        
        ServerRoot.Instance.Game.NetworkEntityManager.Clear();
        
        foreach (PlayerServerInfo playerServerInfo in ServerRoot.Instance.Server.PlayerServerInfo.Values)
        {
            Player player = ServerRoot.Instance.PackedScenes.World.Player.Instantiate<Player>();
            player.Position = Vec(Rand.Range(-100, 100), Rand.Range(-100, 100));
            player.Rotation = Mathf.DegToRad(Rand.Range(0, 360));
            
            if (clientBattleWorld.GetChild<Camera>() == null)
            {
                clientBattleWorld.Player = player;
                
                var camera = new Camera(); //TODO del from server!!
                camera.Position = player.Position;
                camera.TargetNode = player;
                camera.Zoom = Vec(0.65);
                camera.SmoothingPower = 1.5;
                clientBattleWorld.AddChild(camera);
                camera.Enabled = true;
            }

            playerServerInfo.Player = player;
            clientBattleWorld.AddChild(player);
            long newPlayerNid = ServerRoot.Instance.Game.NetworkEntityManager.AddEntity(player);
            
            Netplay.Send(playerServerInfo.Id, 
                new ServerSpawnPlayerPacket(newPlayerNid, player.Position.X, player.Position.Y, player.Rotation));

            foreach (PlayerServerInfo allyServerInfo in ServerRoot.Instance.Server.PlayerServerInfo.Values)
            {
                if (allyServerInfo.Id == playerServerInfo.Id) continue;
                Netplay.Send(allyServerInfo.Id, new ServerSpawnAllyPacket(newPlayerNid, player.Position.X, player.Position.Y, player.Rotation));
            }
        }
    }
}