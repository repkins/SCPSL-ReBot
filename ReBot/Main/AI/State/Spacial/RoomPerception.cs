using MapGeneration;
using ReBot.Navigation.Mesh;
using System.Linq;
using UnityEngine;

namespace ReBot.Main.AI.State.Spacial;

internal struct RoomPerception(int botCount)
{
    public void Process()
    {
        for (var botIdx = 0; botIdx < botCount; botIdx++)
        {
            var bot = BotStore.Bots[botIdx];

            var prevRoomWithin = bot.RoomStore.PrevRoomWithin;

            var playerPosition = bot.PlayerStore.Position;
            if (playerPosition.TryGetRoom(out var roomWithin) && roomWithin != prevRoomWithin)
            {
                // Room change check
                if (prevRoomWithin != null)
                {
                    bot.RoomStore.RoomLastVisitTime[prevRoomWithin.GetInstanceID()] = Time.time;
                }

                bot.RoomStore.RoomEnterInstanceIds.Clear();
                bot.RoomStore.RoomEnterLocations.Clear();

                foreach (var localCell in NavigationMesh.LocalMeshesByRoom[roomWithin.gameObject].Cells)
                {
                    var transformCell = new TransformCell(localCell, roomWithin.transform);
                    foreach (var foreignCell in NavigationMesh.ForeignConnectedCells[transformCell])
                    {
                        var adjacentForeignCell = foreignCell.AdjacentCells.First();
                        var room = adjacentForeignCell.Transform.GetComponent<RoomIdentifier>();
                        if (room != roomWithin)
                        {
                            bot.RoomStore.RoomEnterInstanceIds.Add(room.GetInstanceID());
                            bot.RoomStore.RoomEnterLocations.Add(adjacentForeignCell.MeanPosition);
                        }
                    }
                }
            }

            prevRoomWithin = roomWithin;
            bot.RoomStore.PrevRoomWithin = prevRoomWithin;
        }
    }
}
