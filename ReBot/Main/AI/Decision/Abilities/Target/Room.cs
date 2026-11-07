using ReBot.Main.AI.Decision;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Decision.Abilities.Target;

internal struct Room(BotRef botRef) : ITarget
{
    public NativeList<Vector3> Locations { get; } = botRef.Value.RoomStore.RoomExitLocations;

    public ActionEnum Enum { get; } = ActionEnum.Room;
}
