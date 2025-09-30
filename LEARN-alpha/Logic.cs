namespace LEARN_alpha
{
    internal class Logic
    {
        List<(Logic logic, ConnectionType type)> in1 = [];
        List<(Logic logic, ConnectionType type)> in2 = [];
        List<(Logic logic, ConnectionType type)> out1 = [];
        List<(Logic logic, ConnectionType type)> out2 = [];

        bool in1State = false;
        bool in2State = false;
        bool out1State = false;
        bool out2State = false;

        LogicType type;
        int id = 0;

        public Logic(int id, LogicType Type)
        {
            this.id = id;
            type = Type;
        }

        public static bool operator==(Logic a, Logic b)
        {
            return a.id == b.id;
        }

        public static bool operator!=(Logic a, Logic b)
        {
            return a.id != b.id;
        }

        public enum LogicType
        {
            AND,
            OR,
            NOT,
            NAND,
            NOR,
            XOR,
            XNOR
        }

        public enum ConnectionType
        {
            Input1,
            Input2,
            Output1,
            Output2
        }

        public void Connect(Logic other, ConnectionType connectionType, ConnectionType myconnection)
        {
            ArgumentNullException.ThrowIfNull(other);

            switch (connectionType)
            {
                case ConnectionType.Input1:
                    in1.Add((other, myconnection));
                    break;
                case ConnectionType.Input2:
                    in2.Add((other, myconnection));
                    break;
                case ConnectionType.Output1:
                    out1.Add((other, myconnection));
                    break;
                case ConnectionType.Output2:
                    out2.Add((other, myconnection));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(connectionType), connectionType, null);
            }
        }
    }
}
