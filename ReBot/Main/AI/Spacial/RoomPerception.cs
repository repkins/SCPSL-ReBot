using MapGeneration;
using ReBot.Navigation.Mesh;
using System.Linq;
using UnityEngine;

namespace ReBot.Main.AI.Spacial;

internal struct RoomPerception(int botCount)
{
    public void Process()
    {
        for (var botIdx = 0; botIdx < botCount; botIdx++)
        {
            var bot = BotStore.Bots[botIdx];

            var playerPosition = bot.PlayerState.Position;

            if (!RoomUtils.TryGetRoom(playerPosition, out var roomWithin))
            {
                Debug.LogWarning($"Could not determine room bot currently in");
                continue;
            }

            // Room change check
            var prevRoomWithin = bot.RoomState.PrevRoomWithin;
            if (roomWithin != prevRoomWithin && prevRoomWithin != null)
            {
                bot.RoomState.RoomLastVisitTime[prevRoomWithin] = Time.time;
            }

            // Reached different room check
            if (roomWithin == prevRoomWithin)
            {
                continue;
            }

            prevRoomWithin = roomWithin;
            bot.RoomState.PrevRoomWithin = prevRoomWithin;

            var enteringPositions = NavigationMesh.LocalMeshesByRoom[roomWithin.gameObject].Cells
                .Select(localCell => new TransformCell(localCell, roomWithin.transform))
                .SelectMany(transformCell => NavigationMesh.ForeignConnectedCells[transformCell])
                .Where(foreignCell => foreignCell.Transform.GetComponent<RoomIdentifier>())
                .Select(foreignCell => foreignCell.AdjacentCells.First())

                .Where(foreignCell => foreignCell.Transform.GetComponent<RoomIdentifier>().Zone == roomWithin.Zone)
                .Where(foreignCell => foreignCell.Transform.GetComponent<RoomIdentifier>().Name == RoomName.Unnamed
                                || foreignCell.Transform.GetComponent<RoomIdentifier>().Name != roomWithin.Name)
                //.Where(foreignCell => foreignCell.Room.Identifier.Shape != RoomShape.Endroom || zoneTransitionRoomNames.Contains(foreignCell.Room.Identifier.Name))
                .OrderBy(foreignCell => bot.RoomState.RoomLastVisitTime.TryGetValue(foreignCell.Transform.GetComponent<RoomIdentifier>(), out var time) ? time : -Random.Range(0f, 4f))
                .Select(a => a.MeanPosition);

            bot.RoomState.RoomEnterLocations.Clear();

            foreach (var enteringPosition in enteringPositions)
            {
                bot.RoomState.RoomEnterLocations.Add(enteringPosition);
            }
        }
    }
}
