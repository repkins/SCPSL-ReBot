using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ReBot.Navigation.Mesh
{
    internal partial class NavigationMesh
    {
        public List<Vertex> Vertices { get; } = new();
        public event Action<Ref<Vertex>> VertexCreated;
        public event Action<Ref<Vertex>> VertexDeleted;

        public List<Cell> Cells { get; } = new();
        public event Action<Ref<Cell>> CellCreated;
        public event Action<Ref<Cell>> CellDeleted;

        private NavigationMesh()
        {
            VertexDeleted += RemoveVertexFromCells;
        }

        #region Mesh manipulation

        public Ref<Vertex> AddVertex(Vector3 position)
        {
            var newVertex = new Vertex(position);
            Vertices.Add(newVertex);

            var newVertexRef = new Ref<Vertex>(Vertices.IndexOf(newVertex), this);
            VertexCreated?.Invoke(newVertexRef);

            return newVertexRef;
        }

        public bool DeleteVertex(Ref<Vertex> vertexRef)
        {
            if (vertexRef.Idx < 0)
            {
                return false;
            }

            Vertices.RemoveAt(vertexRef.Idx);

            VertexDeleted?.Invoke(vertexRef);

            return true;
        }

        private void RemoveVertexFromCells(Ref<Vertex> vertexRef)
        {
            for (int cellIdx = 0; cellIdx < Cells.Count; cellIdx++)
            {
                Cells[cellIdx].RemoveVertex(vertexRef);

                RemoveAdjacentCells(new(cellIdx, this));
                AddAdjacentCells(new(cellIdx, this));
            }
        }

        public bool MoveVertex(Ref<Vertex> vertexRef, Vector3 newPosition)
        {
            Vertices[vertexRef.Idx] = new(newPosition);

            return true;
        }

        public Ref<Cell> MakeCell(IEnumerable<Ref<Vertex>> vertexRefs)
        {
            var newCell = new Cell(vertexRefs);
            Cells.Add(newCell);

            var newCellRef = new Ref<Cell>(Cells.IndexOf(newCell), this);
            AddAdjacentCells(newCellRef);

            CellCreated?.Invoke(newCellRef);

            return newCellRef;
        }

        private void AddAdjacentCells(Ref<Cell> newCellRef)
        {
            for (int adjacentCellIdx = 0; adjacentCellIdx < Cells.Count; adjacentCellIdx++)
            {
                foreach (var edge in Cells[newCellRef.Idx].Edges)
                {
                    var adjacentEdge = new Edge(edge.To, edge.From);
                    if (Cells[adjacentCellIdx].Edges.Contains(adjacentEdge))
                    {
                        Cells[newCellRef.Idx].AddAdjacentCell(new(adjacentCellIdx, this), adjacentEdge);
                        Cells[adjacentCellIdx].AddAdjacentCell(newCellRef, edge);
                    }
                }
            }
        }

        public bool RemoveCell(Ref<Cell> cellRef)
        {
            if (cellRef.Idx < 0)
            {
                return false;
            }

            Cells.RemoveAt(cellRef.Idx);
            RemoveAdjacentCells(cellRef);

            CellDeleted?.Invoke(cellRef);

            return true;
        }

        private void RemoveAdjacentCells(Ref<Cell> cellRef)
        {
            for (int otherCellIdx = 0; otherCellIdx < Cells.Count; otherCellIdx++)
            {
                Cells[cellRef.Idx].RemoveAdjacentCell(new(otherCellIdx, this));
                Cells[otherCellIdx].RemoveAdjacentCell(cellRef);
            }
        }

        public void AddVertexToCell(Ref<Cell> cellRef, Ref<Vertex> vertexRef, Ref<Vertex> beforeVertexRef)
        {
            Cells[cellRef.Idx].AddVertex(vertexRef, beforeVertexRef);

            RemoveAdjacentCells(cellRef);
            AddAdjacentCells(cellRef);
        }

        #endregion
        #region Mesh reading/writing

        public void ReadMesh(BinaryReader binaryReader, byte version)
        {
            ///
            /// Vertices reading
            /// 

            var vertexCount = binaryReader.ReadInt32();

            for (var j = 0; j < vertexCount; j++)
            {
                var vertexPosition = new Vector3()
                {
                    x = binaryReader.ReadSingle(),
                    y = binaryReader.ReadSingle(),
                    z = binaryReader.ReadSingle()
                };

                AddVertex(vertexPosition);
            }

            ///
            /// Cells reading
            ///

            var cellsCount = binaryReader.ReadInt32();

            for (var j = 0; j < cellsCount; j++)
            {
                var cellVerticesCount = binaryReader.ReadInt32();
                var cellVertices = new int[cellVerticesCount];
                for (var k = 0; k < cellVerticesCount; k++)
                {
                    cellVertices[k] = binaryReader.ReadInt32();
                }

                MakeCell(cellVertices.Select(vertexIdx => new Ref<Vertex>(vertexIdx, this)));
            }
        }

        public void WriteMesh(BinaryWriter binaryWriter)
        {
            binaryWriter.Write(Vertices.Count);
            for (int vertexIdx = 0; vertexIdx < Vertices.Count; vertexIdx++)
            {
                binaryWriter.Write(Vertices[vertexIdx].Position.x);
                binaryWriter.Write(Vertices[vertexIdx].Position.y);
                binaryWriter.Write(Vertices[vertexIdx].Position.z);
            }

            binaryWriter.Write(Cells.Count);
            for (int cellIdx = 0; cellIdx < Cells.Count; cellIdx++)
            {
                binaryWriter.Write(Cells[cellIdx].Vertices.Count);
                foreach (var vertexIdx in Cells[cellIdx].Vertices.Select(cellVertexRef => cellVertexRef.Idx))
                {
                    binaryWriter.Write(vertexIdx);
                }
            }
        }

        #endregion
    }
}
