using Floor18.App.Entities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using System;
using System.Collections.Generic;

namespace Floor18.App.Solver
{
    public class FloorPlanDrawer
    {
        private Graph graph;
        private int canvasWidth = 300;
        private int canvasHeight = 400;
        private int padding = 5;  // فاصله بین اتاق‌ها

        public FloorPlanDrawer(Graph graph)
        {
            this.graph = graph;
        }

        public void Draw(string outputPath)
        {
            using (var bitmap = new Bitmap(canvasWidth, canvasHeight))
            using (var graphics = Graphics.FromImage(bitmap))
            using (var font = new System.Drawing.Font("Arial", 10))
            using (var pen = new Pen(Color.Black, 2))
            using (var brush = new SolidBrush(Color.LightBlue))
            {
                graphics.Clear(Color.White);

                // چیدمان اتاق‌ها به صورت پویا در فضای مستطیلی
                var positions = GenerateRoomPositions();

                foreach (var vertex in graph.Vertices.Values)
                {
                    if (positions.TryGetValue(vertex.Id, out var roomInfo))
                    {
                        var rect = new Rectangle(roomInfo.Position.X, roomInfo.Position.Y, roomInfo.Width, roomInfo.Height);
                        graphics.FillRectangle(brush, rect);
                        graphics.DrawRectangle(pen, rect);

                        // نمایش نام اتاق
                        var textSize = graphics.MeasureString(vertex.Id, font);
                        var textPosition = new PointF(
                            roomInfo.Position.X + (roomInfo.Width - textSize.Width) / 2,
                            roomInfo.Position.Y + (roomInfo.Height - textSize.Height) / 2
                        );
                        graphics.DrawString(vertex.Id, font, Brushes.Black, textPosition);
                    }
                }

                bitmap.Save(outputPath);
            }
        }

        // تابع تولید موقعیت‌های اتاق‌ها با رعایت هم‌جواری و پر کردن فضای ۳۰۰ در ۴۰۰
        private Dictionary<string, RoomInfo> GenerateRoomPositions()
        {
            var positions = new Dictionary<string, RoomInfo>();
            int rows = (int)Math.Sqrt(graph.Vertices.Count);
            int cols = (int)Math.Ceiling(graph.Vertices.Count / (double)rows);

            int roomWidth = (canvasWidth - (cols + 1) * padding) / cols;
            int roomHeight = (canvasHeight - (rows + 1) * padding) / rows;

            var startPoints = new Dictionary<string, Point>
        {
            { "RoomNW", new Point(0, 0) },
            { "RoomNE", new Point(canvasWidth - roomWidth, 0) },
            { "RoomSW", new Point(0, canvasHeight - roomHeight) },
            { "RoomSE", new Point(canvasWidth - roomWidth, canvasHeight - roomHeight) }
        };

            Queue<(Vertex, Point)> queue = new Queue<(Vertex, Point)>();
            var visited = new HashSet<string>();

            var startVertex = graph.Vertices.Values.GetEnumerator();
            startVertex.MoveNext();

            queue.Enqueue((startVertex.Current, new Point(canvasWidth / 2, canvasHeight / 2)));

            while (queue.Count > 0)
            {
                var (vertex, position) = queue.Dequeue();

                if (visited.Contains(vertex.Id)) continue;
                visited.Add(vertex.Id);

                int width = roomWidth;
                int height = roomHeight;
                positions[vertex.Id] = new RoomInfo(position, width, height);

                int offsetX = width + padding;
                int offsetY = height + padding;

                int neighborIndex = 0;
                foreach (var edge in vertex.Edges)
                {
                    if (!visited.Contains(edge.End.Id))
                    {
                        Point newPosition = position;

                        switch (neighborIndex % 4)
                        {
                            case 0: newPosition.Offset(offsetX, 0); break;
                            case 1: newPosition.Offset(-offsetX, 0); break;
                            case 2: newPosition.Offset(0, offsetY); break;
                            case 3: newPosition.Offset(0, -offsetY); break;
                        }

                        if (IsWithinBounds(newPosition, width, height))
                        {
                            queue.Enqueue((edge.End, newPosition));
                        }
                        neighborIndex++;
                    }
                }
            }

            return positions;
        }

        private bool IsWithinBounds(Point position, int width, int height)
        {
            return position.X >= 0 && position.X + width <= canvasWidth &&
                   position.Y >= 0 && position.Y + height <= canvasHeight;
        }
    }

    // ساختار کمکی برای اطلاعات اتاق
    public class RoomInfo
    {
        public Point Position { get; }
        public int Width { get; }
        public int Height { get; }

        public RoomInfo(Point position, int width, int height)
        {
            Position = position;
            Width = width;
            Height = height;
        }
    }
}

