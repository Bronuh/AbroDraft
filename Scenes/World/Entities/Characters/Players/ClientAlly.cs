using Godot;
using KludgeBox;
using KludgeBox.Events;

namespace NeonWarfare;

public partial class ClientAlly : ClientCharacter 
{
    [Export] [NotNull] public Sprite2D ShieldSprite { get; private set; }
    
    public ClientAllyProfile AllyProfile { get; private set; }
    
    public void InitOnProfile(ClientAllyProfile allyProfile)
    {
        AllyProfile = allyProfile;
    }
    
    public void OnSpawnPacket(float x, float y, float dir)
    {
        Position = Vec(x, y);
        Rotation = dir;
    }
}