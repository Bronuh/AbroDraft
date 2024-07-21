using Godot;
using System;
using NeonWarfare;

public interface IWorldMainScene
{
	public World GetWorld();
	public Hud GetHud();
	public Node2D GetAsNode2D();
}
