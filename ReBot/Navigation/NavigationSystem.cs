using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.Handlers;
using MapGeneration;
using MEC;
using ReBot.Navigation.Mesh;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ReBot.Navigation
{
    public class NavigationSystem
    {
        public static readonly NavigationSystem Instance = new();

        public string BaseDir { get; set; }
        public string MeshFileName { get; } = "navmesh.slnmf";
        public bool Initialized { get; private set; } = false;
        public event Action OnMeshesConnected;
        public event Action OnMeshesReset;

        public void Init()
        {
            ServerEvents.MapGenerated += OnMapGenerated;
            ServerEvents.RoundRestarted += OnRoundRestarted;

            if (SeedSynchronizer.MapGenerated)
            {
                LoadConnectMeshes();
            }

            Initialized = true;
        }

        public void Teardown()
        {
            ServerEvents.MapGenerated -= OnMapGenerated;
            ServerEvents.RoundRestarted -= OnRoundRestarted;

            Initialized = false;

            ResetMeshes();
        }

        private void OnMapGenerated(MapGeneratedEventArgs args)
        {
            Timing.RunCoroutine(LoadConnectMeshesAsync());
        }

        private void OnRoundRestarted()
        {
            ResetMeshes();
        }

        private IEnumerator<float> LoadConnectMeshesAsync()
        {
            yield return Timing.WaitUntilTrue(() => SeedSynchronizer.MapGenerated);

            LoadConnectMeshes();
        }

        public void LoadConnectMeshes()
        {
            Debug.Log($"Initializing meshes.");
            NavigationMesh.InitMeshes();

            Debug.Log($"Loading meshes.");
            LoadMeshes(MeshFileName);

            Debug.Log($"Connecting cells between rooms and adding cells with door obstacles.");
            var stringBuilder = new StringBuilder();
            foreach (var door in DoorVariant.AllDoors)
            {
                if (door is CheckpointDoor or ElevatorDoor)
                {
                    continue;
                }

                if (door.Rooms.Length == 2)
                {
                    var doorCenterPosition = door.transform.position + Vector3.up;  // assuming pivot point is located at the bottom of all doors

                    var edgeInFront = NavigationMesh.GetNearestEdge(doorCenterPosition, door.Rooms[0]);
                    var edgeInBack = NavigationMesh.GetNearestEdge(doorCenterPosition, door.Rooms[1]);

                    if (edgeInFront != null && edgeInBack != null)
                    {
                        // Connect
                        var cellInFront = NavigationMesh.LocalMeshesByRoom[door.Rooms[0].gameObject].Cells
                            .Select(lc => new TransformCell(lc, door.Rooms[0].transform))
                            .First(c => c.Local.Edges.Any(e => e == edgeInFront.Value.Local));

                        var cellInBack = NavigationMesh.LocalMeshesByRoom[door.Rooms[1].gameObject].Cells
                            .Select(lc => new TransformCell(lc, door.Rooms[1].transform))
                            .First(c => c.Local.Edges.Any(e => e == edgeInBack.Value.Local));

                        NavigationMesh.ForeignConnectedCells[cellInFront].Add(cellInBack);
                        NavigationMesh.ForeignConnectedCellEdges[cellInFront].Add(cellInBack, edgeInBack.Value);

                        NavigationMesh.ForeignConnectedCells[cellInBack].Add(cellInFront);
                        NavigationMesh.ForeignConnectedCellEdges[cellInBack].Add(cellInFront, edgeInFront.Value);

                        // Add to cells with obstacle
                        Span<TransformCell> cells = [cellInFront, cellInBack];
                        foreach (var cell in cells)
                        {
                            NavigationMesh.CellsWithSolidObstacles.Add(cell, door.transform);
                        }
                    }
                }
                else
                {
                    const float PlayerRadius = .5f;
                    var cellResult = NavigationMesh.GetCellWithinOrClosest(door.transform.position + Vector3.up, PlayerRadius);
                    if (!cellResult.HasValue)
                    {
                        var doorRoomNames = stringBuilder.Clear().AppendJoin(", ", door.Rooms.Select(r => r.gameObject.name));
                        Debug.LogWarning($"Could not find nav cell for door {door} at ({doorRoomNames}) to create obstacle entry with");
                        continue;
                    }

                    Debug.Log($"{door}, {door.transform.position}");

                    var cell = cellResult.Value;
                    NavigationMesh.CellsWithSolidObstacles.Add(cell, door.transform);
                }
            }

            Debug.Log($"Connecting cells between elevator destinations and adding elevation cells.");
            var elevatorGroups = Enum.GetValues(typeof(ElevatorGroup));
            foreach (ElevatorGroup group in elevatorGroups)
            {
                var elevatorDoors = ElevatorDoor.GetDoorsForGroup(group);
                if (elevatorDoors.Count != 2)
                {
                    Debug.LogWarning($"Irregular elevator level count ({elevatorDoors.Count}) of group {group}");
                    continue;
                }

                if (!ElevatorChamber.TryGetChamber(group, out var chamber))
                {
                    Debug.LogWarning($"Could not get elevator chamber of group {group}");
                    continue;
                }

                var doorTransform = elevatorDoors[0].transform;
                var doorPosition = doorTransform.position + Vector3.up;
                var doorForward = doorTransform.forward;

                for (int i = 1; i <= 3; i++)
                {
                    var doorForwardX = doorForward * i;

                    var coords = RoomUtils.PositionToCoords(doorPosition - doorForwardX);
                    if (!RoomIdentifier.RoomsByCoords.TryGetValue(coords, out var room))
                    {
                        RoomUtils.TryGetRoom(doorPosition + doorForward, out room);
                        Debug.LogWarning($"Adding more room coords for {room}");
                        RoomIdentifier.RoomsByCoords.Add(coords, room);
                        NavigationMesh.AddedRoomCoords.Add(coords);
                        break;
                    }
                }

                var cellAt0InShaftResult = NavigationMesh.GetCellWithinOrClosest(doorPosition - doorForward);

                if (!cellAt0InShaftResult.HasValue)
                {
                    Debug.LogWarning($"Could not determine elevator level cell for group {group} at door {elevatorDoors[0]}");
                    continue;
                }

                doorTransform = elevatorDoors[1].transform;
                doorPosition = doorTransform.position + Vector3.up;
                doorForward = doorTransform.forward;

                for (int i = 1; i <= 3; i++)
                {
                    var doorForwardX = doorForward * i;

                    var coords = RoomUtils.PositionToCoords(doorPosition - doorForwardX);
                    if (!RoomIdentifier.RoomsByCoords.TryGetValue(coords, out var room))
                    {
                        RoomUtils.TryGetRoom(doorPosition + doorForward, out room);
                        Debug.LogWarning($"Adding more room coords for {room}");
                        RoomIdentifier.RoomsByCoords.Add(coords, room);
                        NavigationMesh.AddedRoomCoords.Add(coords);
                        break;
                    }
                }

                var cellAt1InShaftResult = NavigationMesh.GetCellWithinOrClosest(doorPosition - doorForward);

                if (!cellAt1InShaftResult.HasValue)
                {
                    Debug.LogWarning($"Could not determine elevator level cell for group {group} at door {elevatorDoors[1]}");
                    continue;
                }

                var cellAt0InShaft = cellAt0InShaftResult.Value;
                var cellAt1InShaft = cellAt1InShaftResult.Value;

                // Connect
                NavigationMesh.ForeignConnectedCells[cellAt0InShaft].Add(cellAt1InShaft);
                NavigationMesh.ForeignConnectedCells[cellAt1InShaft].Add(cellAt0InShaft);

                Debug.Log($"{group} elevation cells connections added between {cellAt0InShaft.MeanPosition} and {cellAt1InShaft.MeanPosition}");

                // Add elevation obstacles
                var panelAt0 = elevatorDoors[0].GetComponentInChildren<ElevatorPanel>();
                var panelAt1 = elevatorDoors[1].GetComponentInChildren<ElevatorPanel>();

                var travelPanel = chamber.GetComponentInChildren<ElevatorPanel>();

                NavigationMesh.ElevationObstacles.Add((cellAt0InShaft, cellAt1InShaft, panelAt0, travelPanel));
                NavigationMesh.ElevationObstacles.Add((cellAt1InShaft, cellAt0InShaft, panelAt1, travelPanel));

                Debug.Log($"{group} elevation obstacles added between call panels at {panelAt0.transform.position} and {panelAt1.transform.position} with travel panel at {travelPanel.transform.position}");
            }
            Debug.Log($"Connecting cells and adding cells with obstacles cells finished.");

            OnMeshesConnected?.Invoke();
        }

        public void ResetMeshes()
        {
            Debug.Log($"Resetting meshes.");
            NavigationMesh.ResetMeshes();

            Debug.Log($"Removing added room coords");
            foreach (var coords in NavigationMesh.AddedRoomCoords)
            {
                RoomIdentifier.RoomsByCoords.Remove(coords);
            }
            NavigationMesh.AddedRoomCoords.Clear();

            OnMeshesReset?.Invoke();
        }

        public void LoadMeshes(string fileName)
        {
            var path = Path.Combine(BaseDir, fileName);

            if (!File.Exists(path))
            {
                return;
            }

            using var fileStream = File.OpenRead(path);
            using var binaryReader = new BinaryReader(fileStream);

            NavigationMesh.ReadMeshes(binaryReader);
        }

        public void SaveMeshes(string fileName)
        {
            var path = Path.Combine(BaseDir, fileName);

            using var fileStream = File.Open(path, FileMode.Create, FileAccess.Write);
            using var binaryWriter = new BinaryWriter(fileStream);

            NavigationMesh.WriteMeshes(binaryWriter);
        }
    }
}