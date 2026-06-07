namespace ReBot.Navigation.Mesh
{
    internal struct Edge(Ref<Vertex> from, Ref<Vertex> to)
    {
        public Ref<Vertex> From = from;
        public Ref<Vertex> To = to;

        public override readonly bool Equals(object obj)
        {
            return obj is Edge edge && (From, To).Equals((edge.From, edge.To));
        }

        public override readonly int GetHashCode()
        {
            return System.HashCode.Combine(From, To);
        }

        public static bool operator ==(Edge left, Edge right)
        {
            return (left.From, left.To) == (right.From, right.To);
        }

        public static bool operator !=(Edge left, Edge right)
        {
            return !(left == right);
        }

        public override readonly string ToString()
        {
            return (From, To).ToString();
        }
    }
}
