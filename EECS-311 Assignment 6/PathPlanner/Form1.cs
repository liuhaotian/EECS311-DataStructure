using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace PathPlanner
{
    public partial class Form1 : Form
    {
        public UndirectedGraph Graph { get; set; }
        private Node start, end;
        private List<Node> highlightedPath;

        public Form1()
        {
            InitializeComponent();
            Graph = UndirectedGraph.FromSpreadsheet("test.csv");
        }

        readonly Brush black = new SolidBrush(Color.Black);
        readonly Pen thinBlack = new Pen(new SolidBrush(Color.Black), 1);
        readonly Pen thickGreen = new Pen(new SolidBrush(Color.Green), 3);

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;

            g.TranslateTransform(50, 100);

            if (Graph != null)
            {
                foreach (var n in Graph.Nodes)
                {
                    g.DrawString(n.Name, Font, black, n.Position);
                    foreach (var edge in n.Edges)
                    {
                        g.DrawLine(EdgeOnPath(edge)?thickGreen:thinBlack, edge.A.Position, edge.B.Position);
                    }
                }
            }
            g.ResetTransform();
        }

        private bool EdgeOnPath(UndirectedEdge edge)
        {
            if (highlightedPath == null || highlightedPath.Count == 0)
                return false;
            int i = highlightedPath.IndexOf(edge.A);
            if (i < 0)
                return false;
            return highlightedPath[Math.Max(i - 1, 0)] == edge.B ||
                   highlightedPath[Math.Min(i + 1, highlightedPath.Count - 1)] == edge.B;
        }

        private void FromBox_TextChanged(object sender, EventArgs e)
        {
            start = Graph.FindNode(FromBox.Text);
            UpdateButton();
        }

        private void ToBox_TextChanged(object sender, EventArgs e)
        {
            end = Graph.FindNode(ToBox.Text);
            UpdateButton();
        }

        void UpdateButton()
        {
            FindPathButton.Enabled = start != null && end != null;
        }

        private void FindPathButton_Click(object sender, EventArgs e)
        {
            try
            {
                highlightedPath = Graph.FindPath(start, end);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
            Invalidate();  // Forces screen to redraw
        }
    }
}
