using System;
using System.Collections.Generic;

namespace LEARN_alpha
{
    internal class Logic
    {
        private readonly Dictionary<ConnectionType, List<Logic>> _connections;

        private readonly int id;

        public Logic(int id)
        {
            this.id = id;
            _connections = new Dictionary<ConnectionType, List<Logic>>();

            foreach (ConnectionType type in Enum.GetValues(typeof(ConnectionType)))
            {
                _connections[type] = new List<Logic>();
            }
        }

        public int Id => id;

        public static bool operator ==(Logic? a, Logic? b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            if (a is null || b is null)
            {
                return false;
            }

            return a.id == b.id;
        }

        public static bool operator !=(Logic? a, Logic? b)
        {
            return !(a == b);
        }

        public override bool Equals(object? obj)
        {
            return obj is Logic other && other.id == id;
        }

        public override int GetHashCode()
        {
            return id.GetHashCode();
        }

        public enum ConnectionType
        {
            Input1,
            Input2,
            Input3,
            Input4,
            Output1,
            Output2,
            Output3,
            Output4
        }

        public IReadOnlyList<Logic> GetConnections(ConnectionType connectionType)
        {
            if (!_connections.TryGetValue(connectionType, out var connections))
            {
                throw new ArgumentOutOfRangeException(nameof(connectionType), connectionType, null);
            }

            return connections;
        }

        public void Connect(Logic other, ConnectionType connectionType)
        {
            ArgumentNullException.ThrowIfNull(other);

            if (!_connections.TryGetValue(connectionType, out var connections))
            {
                throw new ArgumentOutOfRangeException(nameof(connectionType), connectionType, null);
            }

            if (!connections.Contains(other))
            {
                connections.Add(other);
            }
        }
    }
}
