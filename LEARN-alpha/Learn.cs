using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace LEARN_alpha
{
    public partial class Learn : Form
    {
        public Learn()
        {
            InitializeComponent();
            InitializeTools();
        }

        private enum ToolType
        {
            Pointer,
            Pen,
            Eraser,
            GateAnd,
            GateOr,
            GateNot,
            GateXor,
            GateNand,
            GateNor,
            GateXnor
        }

        private const double GateScale = 0.5;

        private sealed class GateElement
        {
            private Point location;

            public ToolType Type { get; }
            public Bitmap Image { get; }
            public Size DisplaySize { get; }
            public Rectangle Bounds => new Rectangle(Location, DisplaySize);

            public IReadOnlyList<GateConnector> Connectors => connectors;

            public Point Location
            {
                get => location;
                set
                {
                    location = value;
                    foreach (var connector in connectors)
                    {
                        connector.Refresh(location);
                    }
                }
            }

            private readonly GateConnector[] connectors;

            public GateElement(ToolType type, Bitmap image, Point location, Size displaySize, IReadOnlyList<GateConnectorTemplate> connectorTemplates)
            {
                Type = type;
                Image = image;
                DisplaySize = displaySize;
                connectors = connectorTemplates.Select(template => new GateConnector(template)).ToArray();
                Location = location;
            }
        }

        private sealed class GateConnectorTemplate
        {
            public GateConnectorTemplate(ConnectorRole role, Point relativeAnchor, Size hitSize)
            {
                Role = role;
                RelativeAnchor = relativeAnchor;
                HitSize = hitSize;
            }

            public ConnectorRole Role { get; }
            public Point RelativeAnchor { get; }
            public Size HitSize { get; }
        }

        private sealed class GateConnector
        {
            private readonly GateConnectorTemplate template;
            private Rectangle bounds;
            private Point anchor;

            public GateConnector(GateConnectorTemplate template)
            {
                this.template = template;
            }

            public ConnectorRole Role => template.Role;

            public Rectangle Bounds => bounds;

            public Point Anchor => anchor;

            public void Refresh(Point ownerLocation)
            {
                anchor = new Point(ownerLocation.X + template.RelativeAnchor.X, ownerLocation.Y + template.RelativeAnchor.Y);
                bounds = new Rectangle(anchor.X - template.HitSize.Width / 2, anchor.Y - template.HitSize.Height / 2, template.HitSize.Width, template.HitSize.Height);
            }
        }

        private sealed class WireSegment
        {
            public WireSegment(Point[] points)
            {
                Points = points;
            }

            public Point[] Points { get; set; }
            public WireAttachment? StartAttachment { get; set; }
            public WireAttachment? EndAttachment { get; set; }
        }

        private sealed class WireAttachment
        {
            public WireAttachment(GateElement gate, GateConnector connector)
            {
                Gate = gate;
                Connector = connector;
            }

            public GateElement Gate { get; }
            public GateConnector Connector { get; }
        }

        private enum ConnectorRole
        {
            Input,
            Output
        }

        private readonly List<WireSegment> wires = new List<WireSegment>();
        private readonly List<GateElement> gates = new List<GateElement>();
        private readonly Dictionary<ToolType, Bitmap> gateImages = new Dictionary<ToolType, Bitmap>();
        private readonly Dictionary<ToolType, Button> toolButtons = new Dictionary<ToolType, Button>();

        private ToolType currentTool = ToolType.Pointer;

        private bool isDrawing = false;
        private bool isErasing = false;
        private Point drawingStart;
        private Point drawingCurrent;

        private GateElement? pointerGateSelection;
        private Size pointerGateOffset;
        private int pointerWireIndex = -1;
        private Point[]? pointerWireOriginal;
        private Point pointerDragStart;

        private Point? gatePreviewLocation;

        private void OnLogicGateMenuButtonClick(object? sender, EventArgs e)
        {
            gatePanel.Visible = !gatePanel.Visible;
            UpdateLogicGateMenuButtonState();
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            // C 키로 지우기
            if (e.KeyCode == Keys.C)
            {
                wires.Clear();
                gates.Clear();
                ResetPointerState();
                gatePreviewLocation = null;
                Invalidate();
            }
        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) { return; }

            switch (currentTool)
            {
                case ToolType.Pen:
                    isDrawing = true;
                    drawingStart = e.Location;
                    drawingCurrent = e.Location;
                    Capture = true;
                    Invalidate();
                    break;
                case ToolType.Eraser:
                    isErasing = true;
                    EraseAt(e.Location);
                    Capture = true;
                    break;
                case ToolType.Pointer:
                    BeginPointerDrag(e.Location);
                    break;
                default:
                    PlaceGate(e.Location);
                    break;
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            switch (currentTool)
            {
                case ToolType.Pen:
                    if (!isDrawing) { return; }
                    drawingCurrent = e.Location;
                    Invalidate();
                    break;
                case ToolType.Eraser:
                    if (!isErasing) { return; }
                    EraseAt(e.Location);
                    break;
                case ToolType.Pointer:
                    UpdatePointerDrag(e.Location);
                    break;
                default:
                    gatePreviewLocation = e.Location;
                    Invalidate();
                    break;
            }
        }

        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) { return; }

            switch (currentTool)
            {
                case ToolType.Pen:
                    if (!isDrawing) { return; }
                    isDrawing = false;
                    Capture = false;

                    bool preferHorizontalFirst = PreferHorizontalFirst(drawingStart, e.Location);
                    var trio = BuildOrthogonalPath(drawingStart, e.Location, preferHorizontalFirst);
                    if (trio != null)
                    {
                        var wire = new WireSegment(trio);
                        SnapWireAttachments(wire);
                        wires.Add(wire);
                    }
                    Invalidate();
                    break;
                case ToolType.Eraser:
                    isErasing = false;
                    Capture = false;
                    break;
                case ToolType.Pointer:
                    EndPointerDrag();
                    break;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (var pen = new Pen(Color.Black, 2))
            using (var previewPen = new Pen(Color.Gray, 1) { DashStyle = DashStyle.Dash })
            {
                // 확정된 선들
                foreach (var wire in wires)
                {
                    DrawOrthogonal(g, pen, wire.Points);
                }

                foreach (var gate in gates)
                {
                    g.DrawImage(gate.Image, gate.Bounds);
                }

                // 드래그 중 미리보기
                if (isDrawing)
                {
                    bool preferHorizontalFirst = PreferHorizontalFirst(drawingStart, drawingCurrent);
                    var trio = BuildOrthogonalPath(drawingStart, drawingCurrent, preferHorizontalFirst);
                    if (trio != null)
                    {
                        DrawOrthogonal(g, previewPen, trio);
                    }
                }

                if (gatePreviewLocation.HasValue && IsGateTool(currentTool))
                {
                    var img = GetGateImage(currentTool);
                    if (img != null)
                    {
                        var size = GetGateDisplaySize(img);
                        var aligned = AlignGateLocation(gatePreviewLocation.Value, size);
                        DrawGatePreview(g, img, aligned, size);
                    }
                }
            }
        }

        private static void DrawOrthogonal(Graphics g, Pen pen, Point[] pts)
        {
            if (pts == null || pts.Length < 2) { return; }
            if (pts.Length == 2)
            {
                g.DrawLine(pen, pts[0], pts[1]);
            }
            else
            {
                g.DrawLines(pen, pts);
            }
        }

        private void InitializeTools()
        {
            toolButtons.Clear();
            toolButtons[ToolType.Pointer] = pointerButton;
            toolButtons[ToolType.Pen] = penButton;
            toolButtons[ToolType.Eraser] = eraserButton;
            toolButtons[ToolType.GateAnd] = andGateButton;
            toolButtons[ToolType.GateOr] = orGateButton;
            toolButtons[ToolType.GateNot] = notGateButton;
            toolButtons[ToolType.GateXor] = xorGateButton;
            toolButtons[ToolType.GateNand] = nandGateButton;
            toolButtons[ToolType.GateNor] = norGateButton;
            toolButtons[ToolType.GateXnor] = xnorGateButton;

            foreach (var pair in toolButtons)
            {
                var button = pair.Value;
                button.Tag = pair.Key;
                button.FlatStyle = FlatStyle.Flat;
                button.FlatAppearance.BorderSize = 0;
                button.Margin = new Padding(3);
                button.Click += OnToolButtonClick;
            }

            logicGateMenuButton.FlatStyle = FlatStyle.Flat;
            logicGateMenuButton.FlatAppearance.BorderSize = 0;
            logicGateMenuButton.Margin = new Padding(3);

            SetCurrentTool(ToolType.Pointer);
        }

        private void OnToolButtonClick(object? sender, EventArgs e)
        {
            if (sender is Button button && button.Tag is ToolType tool)
            {
                SetCurrentTool(tool);

                if (gatePanel.Visible && IsGateTool(tool))
                {
                    gatePanel.Visible = false;
                    UpdateLogicGateMenuButtonState();
                }
            }
        }

        private void SetCurrentTool(ToolType tool)
        {
            currentTool = tool;
            foreach (var pair in toolButtons)
            {
                pair.Value.BackColor = pair.Key == currentTool ? Color.FromArgb(210, 230, 255) : SystemColors.ControlLight;
            }

            UpdateLogicGateMenuButtonState();

            isDrawing = false;
            isErasing = false;
            gatePreviewLocation = null;
            ResetPointerState();
            Capture = false;
            Invalidate();
        }

        private void UpdateLogicGateMenuButtonState()
        {
            logicGateMenuButton.BackColor = gatePanel.Visible ? Color.FromArgb(210, 230, 255) : SystemColors.ControlLight;
        }

        private void ResetPointerState()
        {
            pointerGateSelection = null;
            pointerWireIndex = -1;
            pointerWireOriginal = null;
            pointerGateOffset = Size.Empty;
            pointerDragStart = Point.Empty;
        }

        private void BeginPointerDrag(Point location)
        {
            ResetPointerState();

            pointerGateSelection = HitTestGate(location);
            if (pointerGateSelection != null)
            {
                pointerGateOffset = new Size(location.X - pointerGateSelection.Location.X, location.Y - pointerGateSelection.Location.Y);
                pointerDragStart = location;
                Capture = true;
                return;
            }

            pointerWireIndex = FindWireIndex(location);
            if (pointerWireIndex >= 0)
            {
                pointerWireOriginal = wires[pointerWireIndex].Points.Select(p => p).ToArray();
                wires[pointerWireIndex].StartAttachment = null;
                wires[pointerWireIndex].EndAttachment = null;
                pointerDragStart = location;
                Capture = true;
            }
        }

        private void UpdatePointerDrag(Point location)
        {
            if (pointerGateSelection != null)
            {
                pointerGateSelection.Location = new Point(location.X - pointerGateOffset.Width, location.Y - pointerGateOffset.Height);
                RefreshAttachmentsForGate(pointerGateSelection);
                Invalidate();
                return;
            }

            if (pointerWireIndex >= 0 && pointerWireOriginal != null)
            {
                int dx = location.X - pointerDragStart.X;
                int dy = location.Y - pointerDragStart.Y;
                var updated = pointerWireOriginal.Select(p => new Point(p.X + dx, p.Y + dy)).ToArray();
                wires[pointerWireIndex].Points = updated;
                Invalidate();
            }
        }

        private void EndPointerDrag()
        {
            if (pointerGateSelection != null)
            {
                Capture = false;
                RefreshAttachmentsForGate(pointerGateSelection);
                Invalidate();
            }
            else if (pointerWireIndex >= 0)
            {
                Capture = false;
                if (pointerWireIndex >= 0 && pointerWireIndex < wires.Count)
                {
                    var wire = wires[pointerWireIndex];
                    SnapWireAttachments(wire);
                }
                Invalidate();
            }

            ResetPointerState();
        }

        private void PlaceGate(Point location)
        {
            if (!IsGateTool(currentTool))
            {
                return;
            }

            var image = GetGateImage(currentTool);
            var size = GetGateDisplaySize(image);
            var aligned = AlignGateLocation(location, size);
            var gate = CreateGateElement(currentTool, image, aligned, size);
            gates.Add(gate);
            foreach (var wire in wires)
            {
                SnapWireAttachments(wire);
            }
            gatePreviewLocation = location;
            Invalidate();
        }

        private void DrawGatePreview(Graphics g, Image image, Point location, Size size)
        {
            using var attrs = new ImageAttributes();
            var matrix = new ColorMatrix
            {
                Matrix33 = 0.5f
            };
            attrs.SetColorMatrix(matrix);

            var destination = new Rectangle(location, size);
            g.DrawImage(image, destination, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attrs);
        }

        private static Point AlignGateLocation(Point cursor, Size size)
        {
            return new Point(cursor.X - size.Width / 2, cursor.Y - size.Height / 2);
        }

        private GateElement CreateGateElement(ToolType tool, Bitmap image, Point location, Size displaySize)
        {
            var templates = BuildGateConnectorTemplates(tool, displaySize);
            return new GateElement(tool, image, location, displaySize, templates);
        }

        private static IReadOnlyList<GateConnectorTemplate> BuildGateConnectorTemplates(ToolType tool, Size displaySize)
        {
            int hitSize = Math.Max(12, Math.Min(displaySize.Width, displaySize.Height) / 4);
            int margin = Math.Max(4, (int)Math.Round(displaySize.Width * 0.1));
            margin = Math.Min(margin, displaySize.Width / 2);
            int leftX = margin;
            int rightX = Math.Max(margin, displaySize.Width - margin);
            int midY = displaySize.Height / 2;

            var connectors = new List<GateConnectorTemplate>();

            if (tool == ToolType.GateNot)
            {
                connectors.Add(new GateConnectorTemplate(ConnectorRole.Input, new Point(leftX, midY), new Size(hitSize, hitSize)));
                connectors.Add(new GateConnectorTemplate(ConnectorRole.Output, new Point(rightX, midY), new Size(hitSize, hitSize)));
                return connectors;
            }

            int topY = (int)Math.Round(displaySize.Height * 0.35);
            int bottomY = (int)Math.Round(displaySize.Height * 0.65);

            connectors.Add(new GateConnectorTemplate(ConnectorRole.Input, new Point(leftX, topY), new Size(hitSize, hitSize)));
            connectors.Add(new GateConnectorTemplate(ConnectorRole.Input, new Point(leftX, bottomY), new Size(hitSize, hitSize)));
            connectors.Add(new GateConnectorTemplate(ConnectorRole.Output, new Point(rightX, midY), new Size(hitSize, hitSize)));
            return connectors;
        }

        private static bool IsGateTool(ToolType tool)
        {
            return tool == ToolType.GateAnd
                || tool == ToolType.GateOr
                || tool == ToolType.GateNot
                || tool == ToolType.GateXor
                || tool == ToolType.GateNand
                || tool == ToolType.GateNor
                || tool == ToolType.GateXnor;
        }

        private Bitmap GetGateImage(ToolType tool)
        {
            if (!IsGateTool(tool))
            {
                throw new ArgumentOutOfRangeException(nameof(tool), tool, null);
            }

            if (gateImages.TryGetValue(tool, out var cached))
            {
                return cached;
            }

            string name = tool switch
            {
                ToolType.GateAnd => "AND",
                ToolType.GateOr => "OR",
                ToolType.GateNot => "NOT",
                ToolType.GateXor => "XOR",
                ToolType.GateNand => "NAND",
                ToolType.GateNor => "NOR",
                ToolType.GateXnor => "XNOR",
                _ => throw new ArgumentOutOfRangeException(nameof(tool), tool, null)
            };

            string baseDirectory = AppContext.BaseDirectory;
            string primaryPath = Path.Combine(baseDirectory, $"{name}.png");

            Bitmap? image = LoadBitmapIfExists(primaryPath);
            if (image == null)
            {
                string assetsFolder = Path.Combine(baseDirectory, "Assets");
                string assetsPath = Path.Combine(assetsFolder, $"{name}.png");

                image = LoadBitmapIfExists(assetsPath);
                if (image == null)
                {
                    Directory.CreateDirectory(assetsFolder);
                    image = CreateFallbackGateImage(name);
                    TrySaveFallback(image, assetsPath);
                }
            }

            gateImages[tool] = image;
            return image;
        }

        private static Size GetGateDisplaySize(Image image)
        {
            int width = Math.Max(1, (int)Math.Round(image.Width * GateScale));
            int height = Math.Max(1, (int)Math.Round(image.Height * GateScale));
            return new Size(width, height);
        }

        private (GateElement gate, GateConnector connector)? FindConnectorHit(Point location)
        {
            for (int i = gates.Count - 1; i >= 0; i--)
            {
                var gate = gates[i];
                foreach (var connector in gate.Connectors)
                {
                    if (connector.Bounds.Contains(location))
                    {
                        return (gate, connector);
                    }
                }
            }

            return null;
        }

        private void SnapWireAttachments(WireSegment wire)
        {
            if (wire.Points == null || wire.Points.Length == 0)
            {
                wire.StartAttachment = null;
                wire.EndAttachment = null;
                return;
            }

            var startHit = FindConnectorHit(wire.Points[0]);
            if (startHit.HasValue)
            {
                wire.StartAttachment = new WireAttachment(startHit.Value.gate, startHit.Value.connector);
                ApplyStartAttachment(wire);
            }
            else
            {
                wire.StartAttachment = null;
            }

            int lastIndex = wire.Points.Length - 1;
            var endHit = FindConnectorHit(wire.Points[lastIndex]);
            if (endHit.HasValue)
            {
                wire.EndAttachment = new WireAttachment(endHit.Value.gate, endHit.Value.connector);
                ApplyEndAttachment(wire);
            }
            else
            {
                wire.EndAttachment = null;
            }
        }

        private static void ApplyStartAttachment(WireSegment wire)
        {
            if (wire.StartAttachment == null)
            {
                return;
            }

            Point anchor = wire.StartAttachment.Connector.Anchor;
            if (wire.Points.Length == 0)
            {
                wire.Points = new[] { anchor };
                return;
            }

            wire.Points[0] = anchor;

            if (wire.Points.Length >= 2)
            {
                Point next = wire.Points[1];
                if (Math.Abs(next.X - anchor.X) <= Math.Abs(next.Y - anchor.Y))
                {
                    wire.Points[1] = new Point(anchor.X, next.Y);
                }
                else
                {
                    wire.Points[1] = new Point(next.X, anchor.Y);
                }
            }
        }

        private static void ApplyEndAttachment(WireSegment wire)
        {
            if (wire.EndAttachment == null)
            {
                return;
            }

            Point anchor = wire.EndAttachment.Connector.Anchor;
            if (wire.Points.Length == 0)
            {
                wire.Points = new[] { anchor };
                return;
            }

            int lastIndex = wire.Points.Length - 1;
            wire.Points[lastIndex] = anchor;

            if (wire.Points.Length >= 2)
            {
                int beforeIndex = lastIndex - 1;
                Point previous = wire.Points[beforeIndex];
                if (Math.Abs(previous.X - anchor.X) <= Math.Abs(previous.Y - anchor.Y))
                {
                    wire.Points[beforeIndex] = new Point(anchor.X, previous.Y);
                }
                else
                {
                    wire.Points[beforeIndex] = new Point(previous.X, anchor.Y);
                }
            }
        }

        private void RefreshAttachmentsForGate(GateElement gate)
        {
            foreach (var wire in wires)
            {
                if (wire.StartAttachment != null && wire.StartAttachment.Gate == gate)
                {
                    ApplyStartAttachment(wire);
                }

                if (wire.EndAttachment != null && wire.EndAttachment.Gate == gate)
                {
                    ApplyEndAttachment(wire);
                }
            }
        }

        private void RemoveAttachmentsForGate(GateElement gate)
        {
            foreach (var wire in wires)
            {
                if (wire.StartAttachment != null && wire.StartAttachment.Gate == gate)
                {
                    wire.StartAttachment = null;
                }

                if (wire.EndAttachment != null && wire.EndAttachment.Gate == gate)
                {
                    wire.EndAttachment = null;
                }
            }
        }

        private static Bitmap? LoadBitmapIfExists(string path)
        {
            if (!File.Exists(path))
            {
                return null;
            }

            try
            {
                return new Bitmap(path);
            }
            catch
            {
                return null;
            }
        }

        private static Bitmap CreateFallbackGateImage(string label)
        {
            var bitmap = new Bitmap(96, 64);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.White);
                using var borderPen = new Pen(Color.Black, 2);
                g.DrawRectangle(borderPen, 1, 1, bitmap.Width - 2, bitmap.Height - 2);

                using var font = new Font(FontFamily.GenericSansSerif, 20, FontStyle.Bold, GraphicsUnit.Pixel);
                var size = g.MeasureString(label, font);
                float x = (bitmap.Width - size.Width) / 2f;
                float y = (bitmap.Height - size.Height) / 2f;
                g.DrawString(label, font, Brushes.Black, x, y);
            }

            return bitmap;
        }

        private static void TrySaveFallback(Bitmap bitmap, string path)
        {
            try
            {
                bitmap.Save(path);
            }
            catch
            {
                // 무시: 실행 환경에 따라 쓸 수 없는 경우가 있을 수 있다.
            }
        }

        private GateElement? HitTestGate(Point location)
        {
            for (int i = gates.Count - 1; i >= 0; i--)
            {
                var gate = gates[i];
                if (gate.Bounds.Contains(location))
                {
                    return gate;
                }
            }

            return null;
        }

        private int FindWireIndex(Point location)
        {
            for (int i = wires.Count - 1; i >= 0; i--)
            {
                if (IsPointNearSegment(location, wires[i].Points))
                {
                    return i;
                }
            }

            return -1;
        }

        private void EraseAt(Point location)
        {
            bool removed = false;

            for (int i = wires.Count - 1; i >= 0; i--)
            {
                if (IsPointNearSegment(location, wires[i].Points))
                {
                    wires.RemoveAt(i);
                    removed = true;
                }
            }

            for (int i = gates.Count - 1; i >= 0; i--)
            {
                var gate = gates[i];
                if (gate.Bounds.Contains(location))
                {
                    RemoveAttachmentsForGate(gate);
                    gates.RemoveAt(i);
                    removed = true;
                }
            }

            if (removed)
            {
                Invalidate();
            }
        }

        private static bool IsPointNearSegment(Point point, Point[] segment)
        {
            if (segment.Length < 2)
            {
                return false;
            }

            const double threshold = 6.0;
            for (int i = 0; i < segment.Length - 1; i++)
            {
                if (DistancePointToSegment(point, segment[i], segment[i + 1]) <= threshold)
                {
                    return true;
                }
            }

            return false;
        }

        private static double DistancePointToSegment(Point point, Point a, Point b)
        {
            if (a == b)
            {
                return Distance(point, a);
            }

            double dx = b.X - a.X;
            double dy = b.Y - a.Y;
            double t = ((point.X - a.X) * dx + (point.Y - a.Y) * dy) / (dx * dx + dy * dy);
            t = Math.Max(0, Math.Min(1, t));
            double projX = a.X + t * dx;
            double projY = a.Y + t * dy;
            return Distance(point, new Point((int)Math.Round(projX), (int)Math.Round(projY)));
        }

        private static double Distance(Point a, Point b)
        {
            int dx = a.X - b.X;
            int dy = a.Y - b.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        private void OnMouseLeave(object? sender, EventArgs e)
        {
            if (IsGateTool(currentTool))
            {
                gatePreviewLocation = null;
                Invalidate();
            }
        }

        // 가로/세로 이동량 비교: |Δx| >= |Δy| 이면 수평→수직, 아니면 수직→수평
        private static bool PreferHorizontalFirst(Point a, Point b)
        {
            int dx = Math.Abs(b.X - a.X);
            int dy = Math.Abs(b.Y - a.Y);
            return dx >= dy;
        }

        // 시작점 a와 끝점 b를 "대각선 없이" 연결하는 경로를 만든다.
        // preferHorizontalFirst = true  => 수평→수직
        // preferHorizontalFirst = false => 수직→수평
        private static Point[] BuildOrthogonalPath(Point a, Point b, bool preferHorizontalFirst)
        {
            if (a == b)
            {
                return new Point[] { a, b };
            }

            // 이미 수평 또는 수직으로 일직선인 경우
            if (a.X == b.X || a.Y == b.Y)
            {
                return new Point[] { a, b };
            }

            if (preferHorizontalFirst)
            {
                Point elbow = new Point(b.X, a.Y); // 수평으로 맞춘 뒤 수직
                return new Point[] { a, elbow, b };
            }
            else
            {
                Point elbow = new Point(a.X, b.Y); // 수직으로 맞춘 뒤 수평
                return new Point[] { a, elbow, b };
            }
        }
    }
}
