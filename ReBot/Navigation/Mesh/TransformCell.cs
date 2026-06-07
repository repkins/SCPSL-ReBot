using ReBot.Navigation.Mesh;
using ReBot.Collections.Generic;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ReBot.Navigation.Mesh
{
    internal partial record struct TransformCell(Ref<Cell> Local, Transform Transform)
    {
        public readonly SelectingEnumerable<IEnumerable<Ref<Cell>>, TransformCell, Ref<Cell>> AdjacentCells
            => new(Local.Mesh.Cells[Local.Idx].AdjacentCells, ToTransformCell);

        public readonly SelectingDictionary<TransformCell, TransformEdge, Ref<Cell>, Edge> AdjacentCellEdges
            => new(Local.Mesh.Cells[Local.Idx].AdjacentCellEdges, ToLocalCell, ToTransformCell, ToTransformEdge);

        public readonly Vector3 MeanPosition => Transform.TransformPoint(Local.Mesh.Cells[Local.Idx].MeanPosition);

        public TransformCell((Ref<Cell> Cell, Transform Tranform) tuple)
            : this(tuple.Cell, tuple.Tranform)
        {
        }

        public bool IsPositionWithin(Vector3 position)
        {
            throw new NotImplementedException();
        }

        [Obsolete]
        public static bool operator ==(TransformCell left, TransformCell? right)
        {
            throw new NotSupportedException();
        }
        [Obsolete]
        public static bool operator !=(TransformCell left, TransformCell? right)
        {
            throw new NotSupportedException();
        }

        private readonly TransformCell ToTransformCell(Ref<Cell> localCell) => new(localCell, Transform);
        private readonly TransformEdge ToTransformEdge(Edge localEdge) => new(localEdge, Transform);
        private readonly Ref<Cell> ToLocalCell(TransformCell transformCell) => transformCell.Local;

        public override readonly string ToString()
        {
            return $"{nameof(TransformCell)}({Local}, {Transform.gameObject.name})";
        }
    }
}
