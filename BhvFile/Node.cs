using System.Drawing;

namespace BHVEditor
{
    /// <summary>节点：表示画布上的一个状态框。</summary>
    public class Node
    {
        public State State { get; }
        public string Name { get; set; }
        public Point Position { get; set; }
        public Size Size { get; set; } // 新增：每个节点独立的大小

        public Node(State st, string name, Point pos, Size size)
        {
            State = st;
            Name = name;
            Position = pos;
            Size = size; // 初始化大小
        }

        /// <summary>获取节点中心点。</summary>
        public Point Center(Size nodeSize) // 修改：使用自己的 Size
        {
            return new Point(Position.X + Size.Width / 2, Position.Y + Size.Height / 2);
        }
    }
}