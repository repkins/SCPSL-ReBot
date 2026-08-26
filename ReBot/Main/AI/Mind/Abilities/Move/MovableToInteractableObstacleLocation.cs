using ReBot.Main.AI.Spacial;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities.Move;

internal struct MovableToInteractableObstacleLocation(BotRef botRef) : IMovableToTarget
{
    MovableTo<InteractableObstacleLocation> movableToInteractableObstacleLocation = new(botRef, new(botRef));

    public bool GoesTo(int targetLocationIdx, NativeList<ImpactingBelief> impactingBeliefs)
    {
        return movableToInteractableObstacleLocation.GoesTo(targetLocationIdx, impactingBeliefs);
    }

    public bool MovesTo(int targetLocationIdx)
    {
        return movableToInteractableObstacleLocation.MovesTo(targetLocationIdx);
    }
}

internal struct InteractableObstacleLocation(BotRef botRef) : ITarget
{
    public NativeList<Vector3> Locations { get; } = ObstacleState.ObstacleLocations;

    public readonly BeliefEnum MovedToEnum => BeliefEnum.MovedToObstacleInteractionLocation;
}
