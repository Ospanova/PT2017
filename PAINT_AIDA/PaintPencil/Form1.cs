using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PaintPencil
{
    public partial class Form1 : Form
    {
        Point prevPoint;
        Point currentPoint;
        Shapes currentShape = Shapes.Free;
        GraphicsPath gp = new GraphicsPath();
        Graphics g;
        Bitmap bmp;
        Queue<Point> q = new Queue<Point>();
        Color originColor;
        Color fillColor;
        Color prevColor = Color.Black;
        Pen c_pen = new Pen(Color.White, 3);
        public Pen p { get; private set; }
        private Random _rnd = new Random();
        int radius = 6;
        public Form1()
        {
            InitializeComponent();
            F4();
        }
        
        private void F4(Bitmap tmpBmp = null)
        {
            p = new Pen(prevColor, 3);
            if (tmpBmp == null)
                bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            else
                bmp = tmpBmp;
            pictureBox1.Image = bmp;
            g = Graphics.FromImage(pictureBox1.Image);


        }
        private void F1(Point point)
        {
            q.Enqueue(point);
            GoFloodFill();
        }
        private void Step(Point p)
        {
            if (p.X < 0) return;
            if (p.Y < 0) return;
            if (p.X >= pictureBox1.Width) return;
            if (p.Y >= pictureBox1.Height) return;
            if (bmp.GetPixel(p.X, p.Y) != originColor) return;
            bmp.SetPixel(p.X, p.Y, fillColor);
            q.Enqueue(p);
        }
        private void F3(Point u)
        {
            
        }

       
        private void GoFloodFill()
        {
            while (q.Count > 0)
            {
                Point cur = q.Dequeue();

                Step(new Point(cur.X, cur.Y + 1));
                Step(new Point(cur.X + 1, cur.Y));
                Step(new Point(cur.X - 1, cur.Y));
                Step(new Point(cur.X, cur.Y - 1));
            }
            pictureBox1.Refresh();

        }
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            prevPoint = e.Location;
            switch (currentShape)
            {
                case Shapes.Free:
                    break;
                case Shapes.Zalivka:
                    originColor = bmp.GetPixel(e.X, e.Y);
                    fillColor = p.Color;
                    F1(e.Location);
                    break;
                case Shapes.Clearing:
                    //F3(e.Location);

                    break;
                case Shapes.Spray:
                    break;
                default:
                    break;
            }
        }

        private void Spray_paint(object sender, MouseEventArgs e)
        {
            for (int i = 0; i < 100; ++i)
            {
                double theta = _rnd.NextDouble() * (Math.PI * 2);
                double r = (_rnd.NextDouble() * radius +(int) p.Width);

                double x = e.X + Math.Cos(theta) * r;
                double y = e.Y + Math.Sin(theta) * r;

                gp.AddEllipse(new Rectangle((int)x - 1, (int)y - 1, 1, 1));
            }

        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                switch (currentShape)
                {
                    case Shapes.Free:
                        currentPoint = e.Location;
                        g.DrawLine(p, prevPoint, currentPoint);
                        prevPoint = currentPoint;
                        
                        break;
                    case Shapes.Line:
                        currentPoint = e.Location;
                        
                        gp.Reset();
                        gp.AddLine(prevPoint, currentPoint);
                        
                        break;

                    case Shapes.Zalivka:
                        
                        break;
                    case Shapes.Ellipse:
                        currentPoint = e.Location;

                        gp.Reset();
                        Rectangle r = new Rectangle(prevPoint.X, prevPoint.Y, (int)(currentPoint.X - prevPoint.X), (int)currentPoint.Y - prevPoint.Y);

                        gp.AddEllipse(r);
                        break;
                    case Shapes.Rectangle:
                        currentPoint = e.Location;
                        
                        gp.Reset();
                        
                        Rectangle r2 = new Rectangle(prevPoint.X, prevPoint.Y,(int)(currentPoint.X - prevPoint.X), (int)currentPoint.Y - prevPoint.Y);
                        if (r2.Height <= 0 && r2.Width <= 0)
                        {
                            r2.Height = prevPoint.Y - currentPoint.Y;
                            r2.Y = prevPoint.Y - r2.Height;
                            r2.Width = prevPoint.X - currentPoint.X;
                            r2.X = prevPoint.X - r2.Width;
                        }
                        else if (r2.Height <= 0 && r2.Width > 0)
                        {
                            r2.Height = prevPoint.Y - currentPoint.Y;
                            r2.Y = prevPoint.Y - r2.Height;
                        }
                        else if (r2.Width <= 0 && r2.Height > 0)
                        {
                            r2.Width = prevPoint.X - currentPoint.X;
                            r2.X = prevPoint.X - r2.Width;
                        }
                        
                        gp.AddRectangle(r2);
                        break;
                    case Shapes.Triangle:
                        currentPoint = e.Location;

                        gp.Reset();

                        Rectangle r3 = new Rectangle(prevPoint.X, prevPoint.Y, (int)(currentPoint.X - prevPoint.X), (int)currentPoint.Y - prevPoint.Y);
                        Point p1 = new Point ( prevPoint.X + ((int)(currentPoint.X - prevPoint.X)/2), prevPoint.Y);
                        Point p2 = new Point (prevPoint.X, prevPoint.Y + (int)currentPoint.Y - prevPoint.Y);
                        Point p3 = new Point (prevPoint.X + (int)(currentPoint.X - prevPoint.X), prevPoint.Y + (int)currentPoint.Y - prevPoint.Y);
                        Point[] points = { p1, p2, p3};
                        gp.AddPolygon(points);
                        break;
                    case Shapes.Clearing:
                        currentPoint = e.Location;
                        g.DrawLine(c_pen, prevPoint, currentPoint);
                        prevPoint = currentPoint;
                        break;
                    case Shapes.Spray:
                            Spray_paint(sender, e);
                        break;

                    default:
                        break;
                }
            }
            mouseLocationLabel.Text = string.Format("X = {0}; Y = {1}", e.X, e.Y);
            pictureBox1.Refresh();
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            g.DrawPath(p, gp);
            gp.Reset();
        }


        private void colorBtn_Click(object sender, EventArgs e)
        {
            ColorDialog c = new ColorDialog();
            if (c.ShowDialog() == DialogResult.OK)
            {
                p.Color = c.Color;
            }
        }

        private void lineBtn_Click(object sender, EventArgs e)
        {
            currentShape = Shapes.Line;
        }

        private void freeBtn_Click(object sender, EventArgs e)
        {
            currentShape = Shapes.Free;
        }
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawPath(p, gp);
        }
 

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            currentShape = Shapes.Rectangle;
        }
        private void EllipseBtn (object sender, EventArgs e)
        {
            currentShape = Shapes.Ellipse;
        }

        private void TriangleBtn(object sender, EventArgs e)
        {
            currentShape = Shapes.Triangle;
        }
        private void Zaliv_btn(object sender, EventArgs e)
        {
            currentShape = Shapes.Zalivka;
        }
        private void Clear_btn (object sender, EventArgs e)
        {
            currentShape = Shapes.Clearing;
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {

            OpenFileDialog file = new OpenFileDialog();
            if (file.ShowDialog() == DialogResult.OK)
            {
                Bitmap tmpBmp = new Bitmap(Image.FromFile(file.FileName), new Size(300, 300));
                bmp = new Bitmap(Width, Height);

                using (Graphics g = Graphics.FromImage(bmp))
                
                {
                    g.DrawImage(tmpBmp, new Point(0, 0));
                }
            }
            F4(bmp);
            
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                pictureBox1.Image.Save(sfd.FileName);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            g.Clear(Color.White);

        }

        private void button3_Click(object sender, EventArgs e)
        {
            currentShape = Shapes.Spray;
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {


        }
        

        private void trackBar1_MouseCaptureChanged(object sender, EventArgs e)
        {
            p.Width = this.trackBar1.Value;
        }

        
    }


    public enum Shapes
    {
        Free,
        Line,
        Ellipse,
        Rectangle,
        Triangle, 
        Zalivka, 
        Clearing,
        Spray
    }
}
