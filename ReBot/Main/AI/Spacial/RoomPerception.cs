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

            var prevRoomWithin = bot.RoomState.PrevRoomWithin;

            var playerPosition = bot.PlayerState.Position;
            if (playerPosition.TryGetRoom(out var roomWithin) && roomWithin != prevRoomWithin)
            {
                // Room change check
                if (prevRoomWithin != null)
                {
                    bot.RoomState.RoomLastVisitTime[prevRoomWithin.GetInstanceID()] = Time.time;
                }

                var enteringRooms = NavigationMesh.LocalMeshesByRoom[roomWithin.gameObject].Cells
                    .Select(localCell => new TransformCell(localCell, roomWithin.transform))
                    .SelectMany(transformCell => NavigationMesh.ForeignConnectedCells[transformCell])
                    .Select(foreignCell => (ForeignCell: foreignCell.AdjacentCells.First(), Room: foreignCell.Transform.GetComponent<RoomIdentifier>()))
                    .Where(t => t.Room != roomWithin)

                    //.Where(foreignCell => foreignCell.Room.Identifier.Shape != RoomShape.Endroom || zoneTransitionRoomNames.Contains(foreignCell.Room.Identifier.Name))
                    .OrderBy(t => 
                        bot.RoomState.RoomLastVisitTime.TryGetValue(t.Room.GetInstanceID(), out var time)
                            ? time 
                            : -Random.Range(0f, 4f)
                    )
                    .Select(t => (t.ForeignCell.MeanPosition, t.Room.GetInstanceID()));

                bot.RoomState.RoomInstanceIds.Clear();
                bot.RoomState.RoomEnterLocations.Clear();
                foreach (var (enteringPosition, enteringInstanceId) in enteringRooms)
                {
                    bot.RoomState.RoomInstanceIds.Add(enteringInstanceId);
                    bot.RoomState.RoomEnterLocations.Add(enteringPosition);
                }
            }

            prevRoomWithin = roomWithin;
            bot.RoomState.PrevRoomWithin = prevRoomWithin;
        }
    }
}
