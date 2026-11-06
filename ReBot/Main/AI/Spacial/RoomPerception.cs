using MapGeneration;
using ReBot.Main.AI.Movement.Pathfinding;
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
            var cellWithinResult = PathfindingStore.CellWithins[botIdx];
            var prevCellWithinResult = PathfindingStore.PrevCellWithins[botIdx];

            var playerPosition = bot.PlayerState.Position;
            if (playerPosition.TryGetRoom(out var roomWithin))
            {
                // Room change check
                if (prevRoomWithin != null && prevRoomWithin != roomWithin)
                {
                    bot.RoomState.RoomLastVisitTime[prevRoomWithin.GetInstanceID()] = Time.time;
                }

                if (cellWithinResult != prevCellWithinResult || !cellWithinResult.HasValue)
                {
                    bot.RoomState.RoomEnterInstanceIds.Clear();
                    bot.RoomState.RoomEnterLocations.Clear();
                    bot.RoomState.RoomExitIsWithins.Clear();

                    foreach (var localCell in NavigationMesh.LocalMeshesByRoom[roomWithin.gameObject].Cells)
                    {
                        var transformCell = new TransformCell(localCell, roomWithin.transform);
                        foreach (var foreignCell in NavigationMesh.ForeignConnectedCells[transformCell])
                        {
                            var adjacentForeignCell = foreignCell.AdjacentCells.First();
                            var room = adjacentForeignCell.Transform.GetComponent<RoomIdentifier>();
                            if (room != roomWithin)
                            {
                                bot.RoomState.RoomEnterInstanceIds.Add(room.GetInstanceID());

                                var roomEnterPosition = adjacentForeignCell.MeanPosition;
                                var roomExitPosition = transformCell.MeanPosition;
                                var roomEnterExitDistance = Vector3.Distance(roomEnterPosition, roomExitPosition);

                                var roomExitIsWithin = cellWithinResult.HasValue && cellWithinResult.Value == transformCell;
                                roomEnterPosition = roomExitIsWithin ? roomEnterPosition : roomExitPosition;

                                bot.RoomState.RoomEnterLocations.Add(roomEnterPosition);
                                bot.RoomState.RoomExitIsWithins.Add(roomExitIsWithin);
                                bot.RoomState.RoomEnterExitDistances.Add(roomEnterExitDistance);
                            }
                        }
                    }
                }
            }

            prevRoomWithin = roomWithin;
            bot.RoomState.PrevRoomWithin = prevRoomWithin;
        }
    }
}
