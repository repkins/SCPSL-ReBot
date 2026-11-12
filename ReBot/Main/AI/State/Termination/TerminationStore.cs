using System;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.State.Termination;

internal struct TerminationStore() : IDisposable
{
    public NativeList<int> EnemyInstanceIds = new(5, Allocator.Persistent);
    public NativeList<Vector3> EnemyLocations = new(5, Allocator.Persistent);
    public NativeList<Vector3> EnemyVelocities = new(5, Allocator.Persistent);
    public NativeList<float> EnemySensedTimes = new(5, Allocator.Persistent);

    public NativeList<EnemyRef> EnemiesWithinSight = new(Allocator.Persistent);
    public NativeList<EnemyRef> EnemiesWithinDistance = new(Allocator.Persistent);
    public float EnemyMaxSightDistance = 30f;

    public NativeList<(Vector3, Vector3)> ChangedTargetPositions = new(1, Allocator.Persistent);

    public bool ReplaceOrAddEnemy(int roleInstanceId, Vector3 enemyPosition, float sensedTime, out EnemyRef enemyRef)
    {
        if (EnemyInstanceIds.IndexOf(roleInstanceId) is var existingEnemyIdx && existingEnemyIdx > -1)
        {
            Replace(existingEnemyIdx, enemyPosition, sensedTime);

            enemyRef = new(existingEnemyIdx);
            return true;
        }
        else
        {
            return Add(roleInstanceId, enemyPosition, sensedTime, out enemyRef);
        }
    }

    public void RemoveEnemyAtSwapback(EnemyRef enemyRef)
    {
        EnemyInstanceIds.RemoveAtSwapBack(enemyRef.Idx);
        EnemyVelocities.RemoveAtSwapBack(enemyRef.Idx);
        EnemyLocations.RemoveAtSwapBack(enemyRef.Idx);
        EnemySensedTimes.RemoveAtSwapBack(enemyRef.Idx);
    }

    public bool Add(int roleInstanceId, Vector3 enemyPosition, float sensedTime, out EnemyRef enemyRef)
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
        EnemySensedTimes.Add(sensedTime);

        return true;
    }

    public void Replace(int enemyIdx, Vector3 enemyPosition, float sensedTime)
    {
        var prevPosition = EnemyLocations[enemyIdx];
        EnemyLocations[enemyIdx] = enemyPosition;
        EnemyVelocities[enemyIdx] = Vector3.Normalize(enemyPosition - prevPosition) * 2f;
        EnemySensedTimes[enemyIdx] = sensedTime;

        ChangedTargetPositions.Add((prevPosition, enemyPosition));
    }

    public void Dispose()
    {
        EnemyInstanceIds.Dispose();
        EnemyLocations.Dispose();
        EnemyVelocities.Dispose();
        EnemySensedTimes.Dispose();
        EnemiesWithinSight.Dispose();
        EnemiesWithinDistance.Dispose();
        ChangedTargetPositions.Dispose();
    }
}
