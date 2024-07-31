using Godot;
using KludgeBox;
using NeonWarfare;
using NeonWarfare.NetOld;

public partial class ServerGame : Node2D
{
	
	public World World { get; private set; }
	
	public override void _Ready()
	{
		NotNullChecker.CheckProperties(this);
		
		ChangeMainScene(new ServerSafeWorld());
	}
	
	public void ChangeMainScene(World serverWorld)
	{
		World?.QueueFree();
		World = serverWorld;
		AddChild(World);
	}
}
