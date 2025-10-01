using System.Drawing.Drawing2D;

namespace LEARN_alpha
{
    public partial class Learn : Form
    {
        public Learn()
        {
            InitializeComponent();
        }

        private readonly List<Point[]> segments = new List<Point[]>();
        private bool isDragging = false;
        private Point start;
        private Point current;

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            // C 키로 지우기
            if (e.KeyCode == Keys.C)
            {
                segments.Clear();
                Invalidate();
            }
        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) { return; }
            isDragging = true;
            start = e.Location;
            current = e.Location;
            Capture = true;
            Invalidate();
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!isDragging) { return; }
            current = e.Location;
            Invalidate();
        }

        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            if (!isDragging || e.Button != MouseButtons.Left) { return; }
            isDragging = false;
            Capture = false;

            // Shift가 눌려있지 않으면 수평→수직, 눌려있으면 수직→수평
            bool preferHorizontalFirst = (ModifierKeys & Keys.Shift) == 0;

            var trio = BuildOrthogonalPath(start, e.Location, preferHorizontalFirst);
            if (trio != null)
            {
                segments.Add(trio);
            }
            Invalidate();
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

                // 드래그 중 미리보기
                if (isDragging)
                {
                    bool preferHorizontalFirst = (ModifierKeys & Keys.Shift) == 0;
                    var trio = BuildOrthogonalPath(start, current, preferHorizontalFirst);
                    if (trio != null)
                    {
                        DrawOrthogonal(g, previewPen, trio);
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
