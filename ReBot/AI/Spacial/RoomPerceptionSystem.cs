using MapGeneration;
using ReBot.Navigation.Mesh;
using System.Linq;
using UnityEngine;

namespace ReBot.AI.Spacial;

internal class RoomPerceptionSystem
{
    public static void Process()
    {
        for (var botIdx = 0; botIdx < BotStore.BotHubs.Count; botIdx++)
        {
            var playerPosition = PlayerStore.PlayerPositions[botIdx];

            if (!RoomUtils.TryGetRoom(playerPosition, out var roomWithin))
            {
                Debug.LogWarning($"Could not determine room bot currently in");
                return;
            }

            // Room change check
            var prevRoomWithin = RoomStore.PrevRoomWithin[botIdx];
            if (roomWithin != prevRoomWithin && prevRoomWithin != null)
            {
                RoomStore.RoomLastVisitTime[prevRoomWithin] = Time.time;
            }

            // Reached different room check
            if (roomWithin == prevRoomWithin)
            {
                continue;
            }

            prevRoomWithin = roomWithin;
            RoomStore.PrevRoomWithin[botIdx] = prevRoomWithin;

            var enteringPositions = NavigationMesh.LocalMeshesByRoom[roomWithin.gameObject].Cells
                .Select(localCell => new TransformCell(localCell, roomWithin.transform))
                .SelectMany(transformCell => NavigationMesh.ForeignConnectedCells[transformCell])
                .Where(foreignCell => foreignCell.Transform.GetComponent<RoomIdentifier>())
                .Select(foreignCell => foreignCell.AdjacentCells.First())

                .Where(foreignCell => foreignCell.Transform.GetComponent<RoomIdentifier>().Zone == roomWithin.Zone)
                .Where(foreignCell => foreignCell.Transform.GetComponent<RoomIdentifier>().Name == RoomName.Unnamed
                                || foreignCell.Transform.GetComponent<RoomIdentifier>().Name != roomWithin.Name)
                //.Where(foreignCell => foreignCell.Room.Identifier.Shape != RoomShape.Endroom || zoneTransitionRoomNames.Contains(foreignCell.Room.Identifier.Name))
                .OrderBy(foreignCell => RoomStore.RoomLastVisitTime.TryGetValue(foreignCell.Transform.GetComponent<RoomIdentifier>(), out var time) ? time : -Random.Range(0f, 4f))
                .Select(a => a.MeanPosition);

            foreach (var enteringPosition in enteringPositions)
            {
                var targetIdx = LocationStore.TargetPositions[botIdx].Length;
                LocationStore.TargetPositions[botIdx].Add(enteringPosition);

                RoomStore.RoomEnterLocationTargets[botIdx].Add(new(targetIdx));
            }
        }
    }
}
