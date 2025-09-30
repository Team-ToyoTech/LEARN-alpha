using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LEARN_alpha
{
    internal class Logic
    {
        List<Logic> in1 = new List<Logic>();
        List<Logic> in2 = new List<Logic>();
        List<Logic> out1 = new List<Logic>();
        List<Logic> out2 = new List<Logic>();

        int id = 0;

        public Logic(int id)
        {
            this.id = id;
        }

        public static bool operator==(Logic a, Logic b)
        {
            return a.id == b.id;
        }

        public static bool operator!=(Logic a, Logic b)
        {
            return a.id != b.id;
        }

        public enum ConnectionType
        {
            Input1,
            Input2,
            Output1,
            Output2
        }

        public void Connect(Logic other, ConnectionType connectionType)
        {
            if(other == null) throw new ArgumentNullException(nameof(other));
            switch (connectionType)
            {
                case ConnectionType.Input1:
                    in1.Add(other);
                    break;
                case ConnectionType.Input2:
                    in2.Add(other);
                    break;
                case ConnectionType.Output1:
                    out1.Add(other);
                    break;
                case ConnectionType.Output2:
                    out2.Add(other);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(connectionType), connectionType, null);
            }
        }
    }
}
