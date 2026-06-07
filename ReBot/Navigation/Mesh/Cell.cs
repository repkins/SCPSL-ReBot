using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ReBot.Navigation.Mesh
{
    internal struct Cell : IEquatable<Cell>
    {
        public List<Ref<Vertex>> Vertices { get; } = new();

        public List<Ref<Cell>> AdjacentCells { get; } = new();
        public Dictionary<Ref<Cell>, Edge> AdjacentCellEdges { get; } = new();

        public readonly IEnumerable<Edge> Edges => Vertices.Zip(Vertices.Skip(1), (v1, v2) => new Edge(v1, v2))
            .Append(new Edge(Vertices.Last(), Vertices.First()));

        public Vector3 MeanPosition => Vertices.Select(v => v.Mesh.Vertices[v.Idx].Position)
            .Aggregate(Vector3.zero, (a, u) => a + u) / Vertices.Count;

        public Cell(IEnumerable<Ref<Vertex>> vertices)
        {
            Vertices.AddRange(vertices);
        }

        public readonly bool Equals(Cell other)
        {
            return Vertices.SequenceEqual(other.Vertices);
        }

        public bool ContainsEdge(Edge edge)
        {
            return Edges.Contains(edge);
        }

        public void AddVertex(Ref<Vertex> vertex, Ref<Vertex> beforeVertex)
        {
            var atIdx = Vertices.IndexOf(beforeVertex);
            Vertices.Insert(atIdx, vertex);
        }

        public void RemoveVertex(Ref<Vertex> vertex)
        {
            Vertices.Remove(vertex);
        }

        public void AddAdjacentCell(Ref<Cell> adjacentCell, Edge adjacentEdge)
        {
            AdjacentCells.Add(adjacentCell);
            AdjacentCellEdges.Add(adjacentCell, adjacentEdge);
        }

        public void RemoveAdjacentCell(Ref<Cell> adjacentCell)
        {
            AdjacentCells.Remove(adjacentCell);
            AdjacentCellEdges.Remove(adjacentCell);
        }

        public override string ToString()
        {
            return $"{nameof(Cell)}{MeanPosition}";
        }
    }
}
