namespace LEARN_alpha
{
	internal class Logic
	{
        readonly List<(Logic logic, ConnectionType type)> in1 = [];
        readonly List<(Logic logic, ConnectionType type)> in2 = [];
        readonly List<(Logic logic, ConnectionType type)> out1 = [];
        readonly List<(Logic logic, ConnectionType type)> out2 = [];

		bool in1State = false;
		bool in2State = false;
		bool out1State = false;
        readonly bool out2State = false;

        readonly LogicType type;
        readonly int id = 0;

		public Logic(int id, LogicType Type)
		{
			this.id = id;
			type = Type;
		}

		public bool Toggle(ConnectionType Type, bool state)
		{
			switch (Type)
			{
				case ConnectionType.Input1:
					in1State = state;
					break;
				case ConnectionType.Input2: 
					in2State = state;
					break;
				default:
					return false;

			}
			switch(type)
			{
				case LogicType.AND:
					if (in1State && in2State) out1State = true;
					else out1State = false;
					break;
				case LogicType.OR:
					if (in2State || in1State) out1State = true;
					else out1State = false;
					break;
				case LogicType.NOT:
					if(!in1State) out1State = true;
					else out1State = false;
					break;
				case LogicType.NAND:
					if (!(in1State && in2State)) out1State = true;
					else out1State = false;
					break;
				case LogicType.NOR:
					if (!(in2State || in1State)) out1State = true;
					else out1State = false;
					break;
				case LogicType.XOR:
					if (in1State != in2State) out1State = true;
					else out1State = false;
					break;
				case LogicType.XNOR:
					if (in1State == in2State) out1State = true;
					else out1State = false;
					break;
            }
			foreach (var (logic, type) in out1)
			{
				logic.Toggle(type, out1State);
            }
			foreach(var (logic, type) in out2)
			{
				logic.Toggle(type, out2State);
            }
            return true;
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

		public void Connect(Logic other, ConnectionType connectionType, ConnectionType theirconnection)
		{
			ArgumentNullException.ThrowIfNull(other);

			switch (connectionType)
			{
				case ConnectionType.Input1:
					in1.Add((other, theirconnection));
					break;
				case ConnectionType.Input2:
					in2.Add((other, theirconnection));
					break;
				case ConnectionType.Output1:
					out1.Add((other, theirconnection));
					break;
				case ConnectionType.Output2:
					out2.Add((other, theirconnection));
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(connectionType), connectionType, null);
			}
		}
	}
}
