﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Tesseract {
    public partial class FormMain : Form {
        private (string Name, Matrix[] Vertices)[] shapes;
        private Matrix[] vertices;
        private int shapesIndex = 0;
        private Matrix rotationM;
        private double angle = 0;
        private const double ToRad = Math.PI / 180.0;
        private readonly List<Tuple<int, int>> linesIndexes = new List<Tuple<int, int>>();

        private bool renderWithAlpha = true;
        private bool renderVertices = false;
        private int vertexSize = 4;

        private PointF[] pts;
        private double[] zs;

        private Color c = Color.White;
        private double zoom = 1.0;

        public FormMain() {
            InitializeComponent();

            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.SetStyle(ControlStyles.UserPaint, true);

            shapes = new (string, Matrix[])[] {
                ("Square", Shapes.Square()),
                ("Cube", Shapes.Cube()),
                ("Hypercube", Shapes.Hypercube())
            };

            SetupShape();

            Task.Run(() => {
                while(true) {
                    Thread.Sleep(30);
                    this.Invalidate();
                }
            });

            this.KeyDown += (object s, KeyEventArgs e1) => {
                switch(e1.KeyCode) {
                    case Keys.Left:
                        shapesIndex--;
                        if(shapesIndex < 0) shapesIndex = shapes.Length - 1;
                        lock(linesIndexes) SetupShape();
                        break;
                    case Keys.Right:
                        shapesIndex++;
                        if(shapesIndex == shapes.Length) shapesIndex = 0;
                        lock(linesIndexes) SetupShape();
                        break;
                    case Keys.Escape:
                        Application.Exit();
                        break;
                }
            };

            this.MouseWheel += (object s1, MouseEventArgs e1) => {
                zoom += 0.1 * (e1.Delta > 0 ? 1 : (zoom > 0.1 ? -1 : 0));
            };

            this.Paint += (object s, PaintEventArgs e1) => {
                Graphics g = e1.Graphics;

                int h = this.Font.Height;
                g.DrawString($"Current Shape:     {shapes[shapesIndex].Name}", this.Font, Brushes.Gainsboro, 5, 5 + h * 0);
                g.DrawString( "Left/Right Arrow:  Change Shape", this.Font, Brushes.Gainsboro, 5, 5 + h * 1);
                g.DrawString( "Escape:            Exit", this.Font, Brushes.Gainsboro, 5, 5 + h * 2);

                lock(linesIndexes) RenderShape(g);
            };
        }

        private void SetupShape() {
            linesIndexes.Clear();
            vertices = shapes[shapesIndex].Vertices;

            // This will only work for squares, cubes, hypercubes, etc...
            for(int i = 0; i < vertices.Length; i++)
                for(int j = i + 1; j < vertices.Length; j++)
                    if(Distance(vertices[i], vertices[j]) == 2)
                        linesIndexes.Add(new Tuple<int, int>(i, j));

            pts = new PointF[vertices.Length];
            zs = new double[vertices.Length];
            rotationM = Matrix.Identity(vertices[0].Rows, vertices[0].Rows);
        }

        private void RenderShape(Graphics g) {
            Matrix p = null;
            int dimensions = vertices[0].Rows;

            g.TranslateTransform(this.DisplayRectangle.Width / 2, this.DisplayRectangle.Height / 2);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            for(int i = 0; i < vertices.Length; i++) {
                switch(dimensions) {
                    case 4:
                        p = vertices[i] * rotationM.Rotate(0.6 * angle * ToRad, 0)  // Rotate X axis
                                        * rotationM.Rotate(1.0 * angle * ToRad, 3); // Rotate W axis
                        break;
                    case 3:
                        p = vertices[i] * rotationM.Rotate(0.6 * angle * ToRad, 0)  // Rotate X axis
                                        * rotationM.Rotate(1.0 * angle * ToRad, 1); // Rotate Y axis
                        p *= 0.6;
                        break;
                    case 2:
                        p = vertices[i] * rotationM.Rotate(0.6 * angle * ToRad, 1);  // Rotate XY plane
                        p *= 0.2;
                        break;
                }

                // Project down from v.Rows dimensions to 2 dimensions
                for(int di = 0; di < p.Rows - 2; di++) {
                    double d = 1.0 / (2.5 - p.Data[p.Rows - di - 1][0]);
                    p *= (Matrix.Identity(p.Rows, p.Rows) * d);
                }
                p *= 1000 * zoom;

                pts[i] = new PointF((float)p.Data[0][0], (float)p.Data[1][0]);
                zs[i] = p.Rows > 2 ? p.Data[2][0] : 0;
                //g.FillEllipse(Brushes.White, pts[i].X - 1, pts[i].Y - 1, 3, 3);
            }

            if(renderWithAlpha && dimensions > 2) {
                double minZ = zs.Min();
                double maxZ = zs.Max();
                linesIndexes.ForEach((t) => DrawLineAlpha(g, c, pts[t.Item1], pts[t.Item2], zs[t.Item1], zs[t.Item2], minZ, maxZ, 4));
            } else {
                using(Pen pc = new Pen(c)) {
                    linesIndexes.ForEach((t) => {
                        g.DrawLine(pc, pts[t.Item1], pts[t.Item2]);
                        // FIXME: The last vertex is not rendered
                        if(renderVertices) g.FillEllipse(Brushes.White, pts[t.Item1].X - vertexSize / 2, pts[t.Item1].Y - vertexSize / 2, vertexSize, vertexSize);
                    });
                }
            }

            angle += 1.0;
        }

        private void DrawLineAlpha(Graphics g, Color c, PointF p1, PointF p2, double z1, double z2, double minZ, double maxZ, double s = 1.0) {
            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;
            double a = Math.Atan2(dy, dx);
            double ca = Math.Cos(a);
            double sa = Math.Sin(a);
            double r = Math.Sqrt(dx * dx + dy * dy); // Radius
            double x = p1.X;
            double y = p1.Y;
            double li;
            double t;

            for(double rs = s; rs < r; rs += s) {
                p2.X = (float)(x + rs * ca);
                p2.Y = (float)(y + rs * sa);

                li = rs / r;
                li = (1 - li) * z1 + li * z2;               // Linear interpolation between z1 and z2 across r
                a = Map(li, minZ, maxZ, 40.0, 255.0);       // Alpha
                t = Map(li, minZ, maxZ, 1.0, vertexSize);   // Pen thickness

                using(Pen zp = new Pen(Color.FromArgb((int)a, c), (float)t)) {
                    g.DrawLine(zp, p1, p2);
                };
                p1 = p2;
            }

            if(renderVertices) {
                // FIXME: It appears that some vertices are being rendered more than once
                using(SolidBrush zb = new SolidBrush(Color.FromArgb((int)Map(z1, minZ, maxZ, 45.0, 255.0), c))) {
                    g.FillEllipse(zb, p1.X - vertexSize / 2, p1.Y - vertexSize / 2, vertexSize, vertexSize);
                }
            }
        }

        private double Map(double v, double rmin, double rmax, double min, double max) {
            return min + (v - rmin) / (rmax - rmin) * (max - min);
        }

        private double Distance(Matrix v1, Matrix v2) { // Assuming m x 1 (m dimensions vectors)
            double d = 0;
            for(int i = 0; i < v1.Rows; i++)
                d += Math.Pow(v2.Data[i][0] - v1.Data[i][0], 2);

            return Math.Sqrt(d);
        }
    }
}