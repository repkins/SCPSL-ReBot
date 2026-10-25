using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace ReBot.Main.AI.Mind;

internal struct FindActionJob<TMind, TMindFactory> : IJob
    where TMind : IMind
    where TMindFactory : IMind.IFactory<TMind>, new()
{
    public required BotRef botRef;

    public void Execute()
    {
        var actionPathfinding = new ActionPathfinding();

        var mindFactory = new TMindFactory();
        var mind = mindFactory.Create(botRef);

        var beliefsToEnable = new NativeList<BeliefRef>(4, Allocator.Temp);
        var impactingActions = new NativeList<ImpactingAction>(4, Allocator.Temp);

        var goalImpactingActions = new NativeList<ImpactingAction>(1, Allocator.Temp);
        mind.ProcessGoals(beliefsToEnable);

        foreach (var goalBelief in beliefsToEnable)
        {
            mind.ProcessEnablingBelief(goalBelief, goalImpactingActions);
        }

        foreach (var (cost, multiplier, impactingAction) in goalImpactingActions)
        {
            actionPathfinding.EnqueueIfLess(cost, multiplier, impactingAction);
        }

        var actionToEnable = default(ActionRef);
        while (beliefsToEnable.Length > 0 && actionPathfinding.TryDequeueMin(out actionToEnable))
        {
            beliefsToEnable.Clear();

            mind.ProcessImpactingAction(actionToEnable, beliefsToEnable);

            foreach (var belief in beliefsToEnable)
            {
                mind.ProcessEnablingBelief(belief, impactingActions);
            }

            foreach (var (cost, multiplier, impactingAction) in impactingActions)
            {
                actionPathfinding.EnqueueIfLess(actionToEnable, cost, multiplier, impactingAction);
            }

            impactingActions.Clear();
        }

        if (MindStore.EnabledActionRefs[botRef.Idx] != actionToEnable)
        {
            MindStore.EnabledActionRefs[botRef.Idx] = actionToEnable;

            PrintVisitedActions(actionPathfinding, goalImpactingActions);
        }
    }

    private void PrintVisitedActions(ActionPathfinding actionPathfinding, NativeList<ImpactingAction> goalImpactingActions)
    {
        var impactingActionStack = new NativeList<(NativeParallelMultiHashMapIterator<ActionRef> iterator, ActionRef actionToEnable)>(Allocator.Temp);
        var actionAction = MindStore.EnabledActionRefs[botRef.Idx];

        foreach (var (_, _, goalImpactingAction) in goalImpactingActions)
        {
            var impactingAction = goalImpactingAction;
            var actionToEnable = default(ActionRef);
            var actionToEnableGeneralCost = 0f;

            do
            {
                var prefix = new string(' ', 4 * impactingActionStack.Length);
                var (generalCost, multiplier) = actionPathfinding.generalCosts[impactingAction];
                var cost = generalCost - actionToEnableGeneralCost;

                var actionPart = (ActionEnum)((byte)impactingAction.Enum & 0x0F);
                var subjectPart = (ActionEnum)((byte)impactingAction.Enum & 0xF0);

                Debug.Log($"{prefix}{actionPart}|{subjectPart}({impactingAction.Idx}), {cost} ({multiplier}x){(impactingAction == actionAction ? " <--" : "")}");

                actionToEnable = impactingAction;
                if (actionPathfinding.visitedImpactingActions.TryGetFirstValue(actionToEnable, out impactingAction, out var impactingIterator))
                {
                    impactingActionStack.Add((impactingIterator, actionToEnable));
                    (actionToEnableGeneralCost, _) = actionPathfinding.generalCosts[actionToEnable];
                }
                else
                {
                    while (impactingActionStack.Length > 0
                        && impactingActionStack[^1] is var actionToEnableFrame)
                    {
                        actionToEnable = actionToEnableFrame.actionToEnable;
                        (actionToEnableGeneralCost, _) = actionPathfinding.generalCosts[actionToEnable];

                        if (actionPathfinding.visitedImpactingActions.TryGetNextValue(out impactingAction, ref actionToEnableFrame.iterator))
                        {
                            impactingActionStack[^1] = actionToEnableFrame;
                            break;
                        }
                        else
                        {
                            impactingActionStack.RemoveAt(impactingActionStack.Length - 1);
                        }
                    }

                }
            }
            while (impactingActionStack.Length > 0);
        }
    }
}
