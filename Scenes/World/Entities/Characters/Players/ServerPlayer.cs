using Godot;
using KludgeBox;
using KludgeBox.Networking;

namespace NeonWarfare;

public partial class ServerPlayer : ServerCharacter
{
    public ServerPlayerProfile PlayerProfile { get; private set; }
    
    public void InitOnProfile(ServerPlayerProfile playerProfile)
    {
        PlayerProfile = playerProfile;
        //TODO MaxHp = maxHp и т.д, у клиента аналогично
    }

    public void Init()
    {
        Position = Vec(400, 400); //TODO временно для теста, потом удалить и у следующей строки убрать знак +
        Position += Vec(Rand.Range(-100, 100), Rand.Range(-100, 100)); 
        Rotation = Mathf.DegToRad(Rand.Range(0, 360));
        
        //У нового игрока спауним его самого
        Network.SendToClient(PlayerProfile.Id, 
            new ClientPlayer.SC_PlayerSpawnPacket(Nid, Position.X, Position.Y, Rotation));
        
        //У всех остальных игроков спауним нового игрока
        Network.SendToAllExclude(PlayerProfile.Id, new ClientAlly.SC_AllySpawnPacket(Nid, Position.X, Position.Y, Rotation, PlayerProfile.Id));
    }
}