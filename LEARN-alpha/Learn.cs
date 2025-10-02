using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

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
            GateXor
        }

        private const double GateScale = 0.5;

        private sealed class GateElement
        {
            public ToolType Type { get; }
            public Bitmap Image { get; }
            public Point Location { get; set; }
            public Size DisplaySize { get; }
            public Rectangle Bounds => new Rectangle(Location, DisplaySize);

            public GateElement(ToolType type, Bitmap image, Point location, Size displaySize)
            {
                Type = type;
                Image = image;
                Location = location;
                DisplaySize = displaySize;
            }
        }

        private readonly List<Point[]> segments = new List<Point[]>();
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
        private int pointerSegmentIndex = -1;
        private Point[]? pointerSegmentOriginal;
        private Point pointerDragStart;

        private Point? gatePreviewLocation;

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            // C 키로 지우기
            if (e.KeyCode == Keys.C)
            {
                segments.Clear();
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
                        segments.Add(trio);
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
                foreach (var s in segments)
                {
                    DrawOrthogonal(g, pen, s);
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

            foreach (var pair in toolButtons)
            {
                var button = pair.Value;
                button.Tag = pair.Key;
                button.FlatStyle = FlatStyle.Flat;
                button.FlatAppearance.BorderSize = 0;
                button.Margin = new Padding(3);
                button.Click += OnToolButtonClick;
            }

            SetCurrentTool(ToolType.Pointer);
        }

        private void OnToolButtonClick(object? sender, EventArgs e)
        {
            if (sender is Button button && button.Tag is ToolType tool)
            {
                SetCurrentTool(tool);
            }
        }

        private void SetCurrentTool(ToolType tool)
        {
            currentTool = tool;
            foreach (var pair in toolButtons)
            {
                pair.Value.BackColor = pair.Key == currentTool ? Color.FromArgb(210, 230, 255) : SystemColors.ControlLight;
            }

            isDrawing = false;
            isErasing = false;
            gatePreviewLocation = null;
            ResetPointerState();
            Capture = false;
            Invalidate();
        }

        private void ResetPointerState()
        {
            pointerGateSelection = null;
            pointerSegmentIndex = -1;
            pointerSegmentOriginal = null;
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

            pointerSegmentIndex = FindSegmentIndex(location);
            if (pointerSegmentIndex >= 0)
            {
                pointerSegmentOriginal = segments[pointerSegmentIndex].Select(p => p).ToArray();
                pointerDragStart = location;
                Capture = true;
            }
        }

        private void UpdatePointerDrag(Point location)
        {
            if (pointerGateSelection != null)
            {
                pointerGateSelection.Location = new Point(location.X - pointerGateOffset.Width, location.Y - pointerGateOffset.Height);
                Invalidate();
                return;
            }

            if (pointerSegmentIndex >= 0 && pointerSegmentOriginal != null)
            {
                int dx = location.X - pointerDragStart.X;
                int dy = location.Y - pointerDragStart.Y;
                var updated = pointerSegmentOriginal.Select(p => new Point(p.X + dx, p.Y + dy)).ToArray();
                segments[pointerSegmentIndex] = updated;
                Invalidate();
            }
        }

        private void EndPointerDrag()
        {
            if (pointerGateSelection != null || pointerSegmentIndex >= 0)
            {
                Capture = false;
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
            gates.Add(new GateElement(currentTool, image, aligned, size));
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

        private static bool IsGateTool(ToolType tool)
        {
            return tool == ToolType.GateAnd || tool == ToolType.GateOr || tool == ToolType.GateNot || tool == ToolType.GateXor;
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

        private int FindSegmentIndex(Point location)
        {
            for (int i = segments.Count - 1; i >= 0; i--)
            {
                if (IsPointNearSegment(location, segments[i]))
                {
                    return i;
                }
            }

            return -1;
        }

        private void EraseAt(Point location)
        {
            bool removed = false;

            for (int i = segments.Count - 1; i >= 0; i--)
            {
                if (IsPointNearSegment(location, segments[i]))
                {
                    segments.RemoveAt(i);
                    removed = true;
                }
            }

            for (int i = gates.Count - 1; i >= 0; i--)
            {
                if (gates[i].Bounds.Contains(location))
                {
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
