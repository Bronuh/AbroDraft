using Godot;
using KludgeBox;
using KludgeBox.Events;
using KludgeBox.Networking;
using NeonWarfare;
using NeonWarfare.Net;
using NeonWarfare.NetOld;

public partial class ClientGame
{
	
	public Network Network { get; private set; }
	public NetworkEntityManager NetworkEntityManager { get; private set; } = new();
	
	public void InitNetwork()
	{
		Network = new();
		AddChild(Network);
		Network.Initialize(GetTree().GetMultiplayer() as SceneMultiplayer);
	} 

	public void ConnectToServer(string host, int port)
	{
		Error error = Network.SetClient(host, port);
		if (error == Error.Ok)
		{
			Log.Info($"Network successfully created.");
		}
		else
		{
			Log.Error($"Create network with result: {error}");
		}
	}
	
	[EventListener(ListenerSide.Client)]
	public static void OnConnectedToServerEvent(ConnectedToServerEvent connectedToServerEvent)
	{
		ClientRoot.Instance.GetWindow().MoveToForeground();
		ClientRoot.Instance.Game.ClearLoadingScreen(); //TODO в идеале вызывать только после синхронизации всех стартовых объектов (сервер должен отправить специальный пакет о том, что синхронизация закончена)
	}
}
