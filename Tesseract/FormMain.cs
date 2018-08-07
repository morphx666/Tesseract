using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Tesseract {
    public partial class FormMain : Form {
        Matrix[] vertices;
        private Matrix rotationM;
        double angle = 0;
        const double ToRad = Math.PI / 180.0;
        List<Tuple<int, int>> linesIndexes = new List<Tuple<int, int>>();

        public FormMain() {
            InitializeComponent();

            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.SetStyle(ControlStyles.UserPaint, true);
        }

        private void FormMain_Load(object sender, EventArgs e) {
            //vertices = new Matrix[] {
            //    new Matrix(new double[] { -1, -1, -1}), // Back face
            //    new Matrix(new double[] { -1,  1, -1}),
            //    new Matrix(new double[] {  1,  1, -1}),
            //    new Matrix(new double[] {  1, -1, -1}),

            //    new Matrix(new double[] { -1, -1,  1}), // Front face
            //    new Matrix(new double[] { -1,  1,  1}),
            //    new Matrix(new double[] {  1,  1,  1}),
            //    new Matrix(new double[] {  1, -1,  1}),
            //};

            vertices = new Matrix[] {
                new Matrix(new double[] { -1, -1, -1,  1}), // Back face
                new Matrix(new double[] { -1,  1, -1,  1}),
                new Matrix(new double[] {  1,  1, -1,  1}),
                new Matrix(new double[] {  1, -1, -1,  1}),

                new Matrix(new double[] { -1, -1,  1,  1}), // Front face
                new Matrix(new double[] { -1,  1,  1,  1}),
                new Matrix(new double[] {  1,  1,  1,  1}),
                new Matrix(new double[] {  1, -1,  1,  1}),

                new Matrix(new double[] { -1, -1, -1, -1}), // 4th dimension, face #1
                new Matrix(new double[] { -1,  1, -1, -1}),
                new Matrix(new double[] {  1,  1, -1, -1}),
                new Matrix(new double[] {  1, -1, -1, -1}),

                new Matrix(new double[] { -1, -1,  1, -1}), // 4th dimension, face #2
                new Matrix(new double[] { -1,  1,  1, -1}),
                new Matrix(new double[] {  1,  1,  1, -1}),
                new Matrix(new double[] {  1, -1,  1, -1})
            };

            //Matrix m = new Matrix(
            //        new double[][] {
            //            new double[] { 3, 0, 2},
            //            new double[] { 2, 0, -2},
            //            new double[] { 0, 1, 1}
            //        }
            //    );

            for(int i = 0; i < vertices.Length; i++) {
                for(int j = i + 1; j < vertices.Length; j++) {
                    if(Distance(vertices[i], vertices[j]) == 2) {
                        linesIndexes.Add(new Tuple<int, int>(i, j));
                    }
                }
            }

            rotationM = Matrix.Identity(vertices[0].Rows, vertices[0].Rows);

            Thread renderer = new Thread(() => {
                while(true) {
                    Thread.Sleep(30);
                    this.Invalidate();
                }
            }) { IsBackground = true };
            renderer.Start();
        }

        private void Form_Paint(object sender, PaintEventArgs e) {
            Graphics g = e.Graphics;
            Matrix p;
            PointF[] pts = new PointF[vertices.Length];
            int pi = -1;

            g.Clear(Color.Black);
            g.TranslateTransform(this.DisplayRectangle.Width / 2, this.DisplayRectangle.Height / 2);

            foreach(Matrix v in vertices) {
                p = v * rotationM.Rotate(0.6 * angle * ToRad, 0)  // Rotate X axis
                      * rotationM.Rotate(1.0 * angle * ToRad, 3); // Rotate W axis

                // Project down from v.Rows dimensions to 2 dimensions
                for(int di = 0; di < v.Rows - 2; di++) {
                    double d = 1.0 / (2.5 - p.Data[v.Rows - di - 1][0]);
                    p *= (Matrix.Identity(p.Rows, p.Rows) * d);
                }
                p *= 1000;

                pts[++pi] = new PointF((float)p.Data[0][0], (float)p.Data[1][0]);
                g.FillEllipse(Brushes.White, pts[pi].X - 4, pts[pi].Y - 4, 8, 8);
            }

            linesIndexes.ForEach((t) => g.DrawLine(Pens.White, pts[t.Item1], pts[t.Item2]));

            angle += 1.0;
        }

        private double Distance(Matrix v1, Matrix v2) {
            double d = 0;
            for(int i = 0; i < v1.Rows; i++) {
                d += Math.Pow(v2.Data[i][0] - v1.Data[i][0], 2);
            }

            return Math.Sqrt(d);
        }
    }
}
