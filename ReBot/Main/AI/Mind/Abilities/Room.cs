using ReBot.Main.AI.Mind.Abilities.Move;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities;

internal struct Room
{
    internal struct EnterLocation(BotRef botRef) : ITarget
    {
        public NativeList<Vector3> Locations { get; } = botRef.Value.RoomState.RoomEnterLocations;

        public readonly BeliefEnum MovedToEnum => BeliefEnum.MovedToRoomEnterLocation;
    }
}
