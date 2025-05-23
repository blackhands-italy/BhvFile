using System.Drawing;

namespace BHVEditor
{
    /// <summary>节点：表示画布上的一个状态框。</summary>
    public class Node
    {
        public State State { get; }
        public string Name { get; set; }
        public Point Position { get; set; }

        public Node(State st, string name, Point pos)
        {
            State = st;
            Name = name;
            Position = pos;
        }

        /// <summary>获取节点中心点。</summary>
        public Point Center(Size size)
        {
            return new Point(Position.X + size.Width / 2, Position.Y + size.Height / 2);
        }
    }
}
