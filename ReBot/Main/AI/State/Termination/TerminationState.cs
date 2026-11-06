using System;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.State.Termination;

public struct TerminationState() : IDisposable
{
    public NativeList<int> EnemyInstanceIds = new(5, Allocator.Persistent);
    public NativeList<Vector3> EnemyLocations = new(5, Allocator.Persistent);
    public NativeList<Vector3> EnemyVelocities = new(5, Allocator.Persistent);

    public NativeList<EnemyRef> EnemiesWithinSight = new(Allocator.Persistent);
    public NativeList<EnemyRef> EnemiesWithinDistance = new(Allocator.Persistent);
    public float EnemyMaxSightDistance = 30f;

    public NativeList<(Vector3, Vector3)> ChangedTargetPositions = new(1, Allocator.Persistent);

    public bool ReplaceOrAddEnemy(int roleInstanceId, Vector3 enemyPosition, out EnemyRef enemyRef)
    {
        if (EnemyInstanceIds.IndexOf(roleInstanceId) is var existingEnemyIdx && existingEnemyIdx > -1)
        {
            Replace(existingEnemyIdx, enemyPosition);

            enemyRef = new(existingEnemyIdx);
            return true;
        }
        else
        {
            return Add(roleInstanceId, enemyPosition, out enemyRef);
        }
    }

    public void RemoveEnemyAtSwapback(EnemyRef enemyRef)
    {
        EnemyInstanceIds.RemoveAtSwapBack(enemyRef.Idx);
        EnemyVelocities.RemoveAtSwapBack(enemyRef.Idx);
        EnemyLocations.RemoveAtSwapBack(enemyRef.Idx);
    }

    public bool Add(int roleInstanceId, Vector3 enemyPosition, out EnemyRef enemyRef)
    {
        if (EnemyInstanceIds.Length >= EnemyInstanceIds.Capacity)
        {
            enemyRef = default;
            return false;
        }

        enemyRef = new(EnemyInstanceIds.Length);
        EnemyInstanceIds.Add(roleInstanceId);
        EnemyVelocities.Add(Vector3.zero);
        EnemyLocations.Add(enemyPosition);

        return true;
    }

    public void Replace(int enemyIdx, Vector3 enemyPosition)
    {
        var prevPosition = EnemyLocations[enemyIdx];
        EnemyLocations[enemyIdx] = enemyPosition;
        EnemyVelocities[enemyIdx] = Vector3.Normalize(enemyPosition - prevPosition) * 2f;

        ChangedTargetPositions.Add((prevPosition, enemyPosition));
    }

    public void Dispose()
    {
        EnemyInstanceIds.Dispose();
        EnemyLocations.Dispose();
        EnemyVelocities.Dispose();
        EnemiesWithinSight.Dispose();
        EnemiesWithinDistance.Dispose();
        ChangedTargetPositions.Dispose();
    }
}
