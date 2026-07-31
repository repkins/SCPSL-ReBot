using ReBot.Main.AI.Spacial;
using System.Runtime.Remoting;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities;

public struct OvercomesElevationTravelObstacle()
{
    public void Process(int elevationIdx, NativeList<ImpactingBelief> impactingBeliefs)
    {
        var cost = 5f;

        impactingBeliefs.Add(cost, BeliefEnum.TravelingOnElevator, elevationIdx);
    }
}
