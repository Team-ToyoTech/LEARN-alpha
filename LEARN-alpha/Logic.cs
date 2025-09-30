using System;
using System.Collections.Generic;

namespace LEARN_alpha
{
    internal class Logic
    {
        private readonly List<Logic> _input1 = new();
        private readonly List<Logic> _input2 = new();
        private readonly List<Logic> _input3 = new();
        private readonly List<Logic> _input4 = new();
        private readonly List<Logic> _output1 = new();
        private readonly List<Logic> _output2 = new();
        private readonly List<Logic> _output3 = new();
        private readonly List<Logic> _output4 = new();

        private readonly int id;

        public Logic(int id)
        {
            this.id = id;
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
            return connectionType switch
            {
                ConnectionType.Input1 => _input1,
                ConnectionType.Input2 => _input2,
                ConnectionType.Input3 => _input3,
                ConnectionType.Input4 => _input4,
                ConnectionType.Output1 => _output1,
                ConnectionType.Output2 => _output2,
                ConnectionType.Output3 => _output3,
                ConnectionType.Output4 => _output4,
                _ => throw new ArgumentOutOfRangeException(nameof(connectionType), connectionType, null)
            };
        }

        public void Connect(Logic other, ConnectionType connectionType)
        {
            ArgumentNullException.ThrowIfNull(other);

            List<Logic> connectionList = connectionType switch
            {
                ConnectionType.Input1 => _input1,
                ConnectionType.Input2 => _input2,
                ConnectionType.Input3 => _input3,
                ConnectionType.Input4 => _input4,
                ConnectionType.Output1 => _output1,
                ConnectionType.Output2 => _output2,
                ConnectionType.Output3 => _output3,
                ConnectionType.Output4 => _output4,
                _ => throw new ArgumentOutOfRangeException(nameof(connectionType), connectionType, null)
            };

            if (!connectionList.Contains(other))
            {
                connectionList.Add(other);
            }
        }
    }
}
