using Godot;
using KludgeBox;
using NeonWarfare.Net;

namespace NeonWarfare;

public partial class ClientRoot
{
	
	public ClientGame Game { get; private set; }
	public MainMenuMainScene MainMenu { get; private set; }

	private void SetMainScene(ClientGame game)
	{
		ClearStoredNode();
		Game = game;
		AddChild(game);
	}
	
	private void SetMainScene(MainMenuMainScene mainScene)
	{
		ClearStoredNode();
		MainMenu = mainScene;
		AddChild(mainScene);
	}

	private void ClearStoredNode()
	{
		Game?.QueueFree();
		Game = null;
		
		MainMenu?.QueueFree();
		MainMenu = null;
	}
	
	public void CreateClientGame(string host, int port)
	{
		ClientGame clientGame = new ClientGame();
		SetMainScene(clientGame);
		clientGame.ConnectToServer(host, port);
	}
	
	public void CreateMainMenu()
	{
		var mainMenu = PackedScenes.Client.Screens.MainMenuPackedScene;
		SetMainScene(mainMenu.Instantiate<MainMenuMainScene>());
	}
}