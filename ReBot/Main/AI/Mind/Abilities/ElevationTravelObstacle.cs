using ReBot.Main.AI.Mind.Abilities.Move;
using ReBot.Main.AI.Spacial;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities;

public struct ElevationTravelObstacle(BotRef botRef)
{
    Vector3 playerPosition = botRef.Value.PlayerState.Position;

    NativeHashMap<Vector3, ObstacleRef> obstaclesByTarget = botRef.Value.ObstacleState.ObstaclesByTarget;
    NativeList<Vector3> obstacleLocations = ObstacleState.ObstacleLocations;

    NativeList<ObstacleRef> elevationTravelObstacles = ObstacleState.ElevationTravelObstacles;
    NativeList<Vector3> elevationTravelEnterLocations = ObstacleState.ElevationTravelEnterLocations;
    NativeList<Vector3> elevationTravelInteractLocations = ObstacleState.ElevationTravelInteractLocations;

    NativeReference<(bool, Vector3)> interactPositionRef = botRef.Value.InteractionState.InteractWithPositionRef;
    NativeReference<(bool, Vector3)> lookAtPositionRef = botRef.Value.MovementState.LookAtPositionRef;
    NativeReference<(bool, Vector3)> moveToPositionRef = botRef.Value.MovementState.MoveToPositionRef;

    public bool TryGetElevationIdx(ObstacleRef obstacleRef, out int elevationIdx)
    {
        if (elevationTravelObstacles.IndexOf(obstacleRef) is var resultElevationIdx && resultElevationIdx > -1)
        {
            elevationIdx = resultElevationIdx;
            return true;
        }

        elevationIdx = default;
        return false;
    }

    public bool Overcomes(int elevationIdx, NativeList<ImpactingBelief> impactingBeliefs)
    {
        var cost = 5f;

        impactingBeliefs.Add(cost, BeliefEnum.TravelingOnElevator, elevationIdx);

        return false;
    }

    public bool Travels(int elevationIdx, NativeList<ImpactingBelief> impactingBeliefs)
    {
        var elevationEnterPosition = elevationTravelEnterLocations[elevationIdx];

        if (Vector3.Distance(elevationEnterPosition, playerPosition) < 1f)
        {
            var travelInteractPosition = elevationTravelInteractLocations[elevationIdx];
            lookAtPositionRef.Value = (true, travelInteractPosition);
            interactPositionRef.Value = (true, travelInteractPosition);

            return true;
        }
        else
        {
            var cost = 0f;

            impactingBeliefs.Add(cost, BeliefEnum.WithinElevator, elevationIdx);
        }

        return false;
    }

    internal struct EnterLocation(BotRef botRef) : ITarget
    {
        public NativeList<Vector3> Locations { get; } = ObstacleState.ElevationTravelEnterLocations;

        public readonly BeliefEnum MovedToEnum => BeliefEnum.MovedToElevator;
    }
}
