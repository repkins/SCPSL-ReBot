using MapGeneration;
using ReBot.Main.AI.State.Movement.Pathfinding;
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
            var cellWithinResult = PathfindingStore.CellWithins[botIdx];
            var prevCellWithinResult = PathfindingStore.PrevCellWithins[botIdx];

            var playerPosition = bot.PlayerStore.Position;
            if (playerPosition.TryGetRoom(out var roomWithin))
            {
                // Room change check
                if (prevRoomWithin != null && prevRoomWithin != roomWithin)
                {
                    bot.RoomStore.RoomLastVisitTime[prevRoomWithin.GetInstanceID()] = Time.time;
                }

                if (cellWithinResult != prevCellWithinResult || !cellWithinResult.HasValue)
                {
                    bot.RoomStore.RoomEnterInstanceIds.Clear();
                    bot.RoomStore.RoomEnterLocations.Clear();
                    bot.RoomStore.RoomExitLocations.Clear();
                    bot.RoomStore.RoomExitIsWithins.Clear();

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
                                bot.RoomStore.RoomExitLocations.Add(transformCell.MeanPosition);

                                var roomExitIsWithin = cellWithinResult.HasValue && cellWithinResult.Value == transformCell;
                                bot.RoomStore.RoomExitIsWithins.Add(roomExitIsWithin);
                            }
                        }
                    }
                }
            }

            prevRoomWithin = roomWithin;
            bot.RoomStore.PrevRoomWithin = prevRoomWithin;
        }
    }
}
