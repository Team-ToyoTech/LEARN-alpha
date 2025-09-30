using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace LEARN_alpha
{
    public partial class Learn : Form
    {
        private const int PortRadius = 12;
        private const int LayoutMargin = 48;

        private readonly List<Port> ports = new();
        private readonly List<Connection> connections = new();
        private readonly List<ConnectionInfo> connectionInfos = new();

        private Port? draggingPort;
        private Point currentMousePosition;

        public Learn()
        {
            InitializeComponent();

            EnableDoubleBuffering(connectionCanvas);

            connectionCanvas.Paint += ConnectionCanvas_Paint;
            connectionCanvas.MouseDown += ConnectionCanvas_MouseDown;
            connectionCanvas.MouseMove += ConnectionCanvas_MouseMove;
            connectionCanvas.MouseUp += ConnectionCanvas_MouseUp;
            connectionCanvas.Resize += ConnectionCanvas_Resize;

            InitializePorts();
            UpdatePortLayout();
        }

        public IReadOnlyList<ConnectionInfo> ConnectionInfos => connectionInfos.AsReadOnly();

        public void LoadConnections(IEnumerable<ConnectionInfo> savedConnections)
        {
            ArgumentNullException.ThrowIfNull(savedConnections);

            InitializePorts();
            UpdatePortLayout();

            foreach (ConnectionInfo info in savedConnections)
            {
                Port? outputPort = ports.FirstOrDefault(port =>
                    !port.IsInput &&
                    port.Logic.Id == info.SourceLogicId &&
                    port.ConnectionType == info.SourceConnectionType);

                Port? inputPort = ports.FirstOrDefault(port =>
                    port.IsInput &&
                    port.Logic.Id == info.TargetLogicId &&
                    port.ConnectionType == info.TargetConnectionType);

                if (outputPort != null && inputPort != null)
                {
                    TryCreateConnection(outputPort, inputPort);
                }
            }

            connectionCanvas.Invalidate();
        }

        private static void EnableDoubleBuffering(Panel panel)
        {
            var doubleBufferedProperty = typeof(Panel).GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            doubleBufferedProperty?.SetValue(panel, true);
        }

        private void InitializePorts()
        {
            ports.Clear();
            connections.Clear();
            connectionInfos.Clear();

            for (int i = 0; i < 4; i++)
            {
                ports.Add(new Port(
                    logic: new Logic(i + 1),
                    connectionType: GetInputConnectionType(i),
                    isInput: true,
                    label: $"IN{i + 1}"));

                ports.Add(new Port(
                    logic: new Logic(101 + i),
                    connectionType: GetOutputConnectionType(i),
                    isInput: false,
                    label: $"OUT{i + 1}"));
            }
        }

        private static Logic.ConnectionType GetInputConnectionType(int index) => index switch
        {
            0 => Logic.ConnectionType.Input1,
            1 => Logic.ConnectionType.Input2,
            2 => Logic.ConnectionType.Input3,
            3 => Logic.ConnectionType.Input4,
            _ => throw new ArgumentOutOfRangeException(nameof(index), index, null)
        };

        private static Logic.ConnectionType GetOutputConnectionType(int index) => index switch
        {
            0 => Logic.ConnectionType.Output1,
            1 => Logic.ConnectionType.Output2,
            2 => Logic.ConnectionType.Output3,
            3 => Logic.ConnectionType.Output4,
            _ => throw new ArgumentOutOfRangeException(nameof(index), index, null)
        };

        private void ConnectionCanvas_Resize(object? sender, EventArgs e)
        {
            UpdatePortLayout();
        }

        private void UpdatePortLayout()
        {
            if (connectionCanvas.Width <= 0 || connectionCanvas.Height <= 0)
            {
                return;
            }

            int top = LayoutMargin;
            int bottom = Math.Max(LayoutMargin, connectionCanvas.Height - LayoutMargin);

            int inputCount = ports.Count(p => p.IsInput);
            int outputCount = ports.Count(p => !p.IsInput);

            float inputSpacing = inputCount > 1 ? (float)(bottom - top) / (inputCount - 1) : 0f;
            float outputSpacing = outputCount > 1 ? (float)(bottom - top) / (outputCount - 1) : 0f;

            int inputIndex = 0;
            int outputIndex = 0;

            foreach (Port port in ports)
            {
                float y = port.IsInput
                    ? top + inputIndex * inputSpacing
                    : top + outputIndex * outputSpacing;

                int x = port.IsInput
                    ? LayoutMargin
                    : connectionCanvas.Width - LayoutMargin;

                port.Center = new Point(x, (int)Math.Round(y));

                if (port.IsInput)
                {
                    inputIndex++;
                }
                else
                {
                    outputIndex++;
                }
            }

            connectionCanvas.Invalidate();
        }

        private void ConnectionCanvas_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            draggingPort = GetPortAtPoint(e.Location);
            currentMousePosition = e.Location;

            if (draggingPort != null)
            {
                connectionCanvas.Invalidate();
            }
        }

        private void ConnectionCanvas_MouseMove(object? sender, MouseEventArgs e)
        {
            currentMousePosition = e.Location;

            if (draggingPort != null)
            {
                connectionCanvas.Invalidate();
            }
        }

        private void ConnectionCanvas_MouseUp(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
            {
                draggingPort = null;
                return;
            }

            if (draggingPort == null)
            {
                return;
            }

            Port? targetPort = GetPortAtPoint(e.Location);

            if (targetPort != null && targetPort != draggingPort && draggingPort.IsInput != targetPort.IsInput)
            {
                Port outputPort = draggingPort.IsInput ? targetPort : draggingPort;
                Port inputPort = draggingPort.IsInput ? draggingPort : targetPort;

                TryCreateConnection(outputPort, inputPort);
            }

            draggingPort = null;
            connectionCanvas.Invalidate();
        }

        private void TryCreateConnection(Port outputPort, Port inputPort)
        {
            if (connections.Any(connection => connection.Output == outputPort && connection.Input == inputPort))
            {
                return;
            }

            connections.Add(new Connection(outputPort, inputPort));
            outputPort.Logic.Connect(inputPort.Logic, outputPort.ConnectionType);
            inputPort.Logic.Connect(outputPort.Logic, inputPort.ConnectionType);

            connectionInfos.Add(new ConnectionInfo(
                outputPort.Logic.Id,
                outputPort.ConnectionType,
                inputPort.Logic.Id,
                inputPort.ConnectionType));
        }

        private Port? GetPortAtPoint(Point location)
        {
            foreach (Port port in ports)
            {
                if (DistanceSquared(port.Center, location) <= PortRadius * PortRadius)
                {
                    return port;
                }
            }

            return null;
        }

        private static int DistanceSquared(Point a, Point b)
        {
            int dx = a.X - b.X;
            int dy = a.Y - b.Y;
            return dx * dx + dy * dy;
        }

        private void ConnectionCanvas_Paint(object? sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.Clear(connectionCanvas.BackColor);

            using Pen connectionPen = new(Color.DimGray, 2f);
            foreach (Connection connection in connections)
            {
                DrawConnection(e.Graphics, connectionPen, connection.Output.Center, connection.Input.Center);
            }

            if (draggingPort != null)
            {
                using Pen previewPen = new(Color.OrangeRed, 2f)
                {
                    DashStyle = DashStyle.Dash
                };
                DrawConnection(e.Graphics, previewPen, draggingPort.Center, currentMousePosition);
            }

            foreach (Port port in ports)
            {
                DrawPort(e.Graphics, port);
            }
        }

        private static void DrawConnection(Graphics graphics, Pen pen, Point start, Point end)
        {
            var midX = (start.X + end.X) / 2;
            var controlPoint1 = new Point(midX, start.Y);
            var controlPoint2 = new Point(midX, end.Y);
            graphics.DrawBezier(pen, start, controlPoint1, controlPoint2, end);
        }

        private void DrawPort(Graphics graphics, Port port)
        {
            Rectangle circleBounds = new(
                port.Center.X - PortRadius,
                port.Center.Y - PortRadius,
                PortRadius * 2,
                PortRadius * 2);

            Color baseColor = port.IsInput ? Color.SteelBlue : Color.SeaGreen;

            using SolidBrush brush = new(baseColor);
            using Pen outlinePen = new(Color.Black, 1.5f);

            graphics.FillEllipse(brush, circleBounds);
            graphics.DrawEllipse(outlinePen, circleBounds);

            using SolidBrush textBrush = new(Color.Black);
            SizeF labelSize = graphics.MeasureString(port.Label, Font);
            float labelX = port.IsInput
                ? circleBounds.Right + 8f
                : circleBounds.Left - 8f - labelSize.Width;

            float labelY = port.Center.Y - labelSize.Height / 2f;

            graphics.DrawString(port.Label, Font, textBrush, new PointF(labelX, labelY));
        }

        public record ConnectionInfo(int SourceLogicId, Logic.ConnectionType SourceConnectionType, int TargetLogicId, Logic.ConnectionType TargetConnectionType);

        private sealed class Port
        {
            public Port(Logic logic, Logic.ConnectionType connectionType, bool isInput, string label)
            {
                Logic = logic;
                ConnectionType = connectionType;
                IsInput = isInput;
                Label = label;
            }

            public Logic Logic { get; }

            public Logic.ConnectionType ConnectionType { get; }

            public bool IsInput { get; }

            public string Label { get; }

            public Point Center { get; set; }

            public override string ToString() => $"{Label} ({ConnectionType})";
        }

        private sealed class Connection
        {
            public Connection(Port output, Port input)
            {
                Output = output;
                Input = input;
            }

            public Port Output { get; }

            public Port Input { get; }
        }
    }
}
