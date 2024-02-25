using AbroDraft.Scripts.Content;
using AbroDraft.Scripts.EventBus;
using Godot;
using KludgeBox;
using KludgeBox.Events;

namespace AbroDraft.Scenes.World.BattleWorld;

[GameService]
public class BattleWorldService
{
    
    [GameEventListener]
    public void OnBattleWorldReadyEvent(BattleWorldReadyEvent battleWorldReadyEvent)
    {
        BattleWorld battleWorld = battleWorldReadyEvent.BattleWorld;
        
        battleWorld.Player = Root.Root.Instance.PackedScenes.World.Player.Instantiate<Entities.Character.Player.Player>();
        battleWorld.Player.Position = Vec(500, 500);
		
        var camera = new Camera.Camera(); //TODO to camera service
        camera.Position = battleWorld.Player.Position;
        camera.TargetNode = battleWorld.Player;
        camera.Zoom = Vec(0.3);
        camera.SmoothingPower = 1.5;
        battleWorld.AddChild(camera);
        camera.Enabled = true;

        var floor = battleWorld.Floor;
        floor.Camera = camera;
		
        Node2D ally = Root.Root.Instance.PackedScenes.World.Ally.Instantiate<Node2D>();
        ally.Position = Vec(600, 600);
        battleWorld.AddChild(ally);
        battleWorld.AddChild(battleWorld.Player); // must be here to draw over the floor
        
        if (Rand.Chance(0.5))//TODO to music service (battle music service)
        {
            PlayBattleMusic1();
        }
        else
        {
            PlayBattleMusic2();
        }
    }

    [GameEventListener]
    public void OnBattleWorldProcessEvent(BattleWorldProcessEvent battleWorldProcessEvent) { }
    
    public void PlayBattleMusic1()
    {
        var music = Audio2D.PlayMusic(Music.WorldBgm1, 0.5f);
        music.Finished += PlayBattleMusic2;
    }
    
    public void PlayBattleMusic2()
    {
        var music = Audio2D.PlayMusic(Music.WorldBgm2, 0.5f);
        music.Finished += PlayBattleMusic1;
    }
}