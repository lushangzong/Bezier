using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace Bezier
{
    
    public partial class Form1 : Form
    {
        //显示在坐标以及储存的坐标都是左下坐标系，
        //在储存点的时候会对点的坐标进行计算，绘图的时候再变换回来
        public Form1()
        {
            InitializeComponent();
        }

        //全局变量用来储存左键点击的点
        List<PointF> PointF_list = new List<PointF>();

        //标记鼠标的位置，记录鼠标移动的时候鼠标的位置
        PointF mouse_position;

        //用来判断是否进行右键点击，初始值为false
        bool right_click = false;

        //用来判断是否处于需要拖动状态，初始值为false
        bool down = false;

        List<List<PointF>> stack = new List<List<PointF>>();//堆栈存放列表

        //阈值,判断折线是否可以代替曲线
        double Threshold = 1;

        //阈值,用来检索鼠标范围内的点
        double area = 10;

        //是否显示端点，初始值为否
        bool check = false;

        //笔的宽度
        float width = 1;

        //绘制曲线时的颜色
        Color arc = Color.FromArgb(255, 0, 0, 0);

        //鼠标点击就记录点击的坐标
        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            //左键点击要么把点的位置存起来，要么移动点
            if(e.Button == MouseButtons.Left)
            {
                //没有点击过右键则记录点击位置
                if(!right_click)
                {
                    //变换坐标
                    mouse_position = e.Location;
                    mouse_position.Y = pictureBox1.Height - mouse_position.Y;
                    PointF_list.Add(mouse_position);
                }  
            }
            //右键点击就改变判断值
            if(e.Button == MouseButtons.Right)
            {
                right_click = true;
                rubber();
            } 
        }

        //实现橡皮筋的效果，当鼠标移动时不断更新bitmap
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {

            //更新鼠标位置
            mouse_position = e.Location;
            mouse_position.Y = pictureBox1.Height - mouse_position.Y;

            //右键没有点击，需要绘图
            if (!right_click)
            {
                pictureBox1.Cursor = Cursors.Cross;

                //绘制连线
                rubber();

                label1.Text = "提示:鼠标左键输入，右键结束，Backspace键回退" + "   " + "X:" + e.X.ToString() + " " + "Y:" + (pictureBox1.Height - e.Y).ToString();
            }
            //右键点击了需要实现引力场移动
            if(right_click)
            {
                pictureBox1.Cursor = Cursors.Cross;

                //改变提示信息
                label1.Text = "提示:可以移动顶点调整曲线形状" + "   " + "X:" + e.X.ToString() + " " + "Y:" + (pictureBox1.Height - e.Y).ToString();

                //遍历所有的点找到鼠标附近的点
                for (int i=0;i<PointF_list.Count();i++)
                {
                    //找到在检索范围内的点
                    if(Math.Abs(PointF_list[i].X - mouse_position.X) <= area & Math.Abs(PointF_list[i].Y - mouse_position.Y) <= area)
                    {
                        //改变鼠标形状
                        pictureBox1.Cursor = Cursors.SizeAll;

                        //并且一直按住左键
                        if (down)
                        {
                            PointF_list[i] = mouse_position;
                            rubber();
                        }
                        break;
                    }
                }
            } 
        }

        //按了backspace键则消除一个点
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Back)
            {
                if(PointF_list.Count>0)
                {
                    PointF_list.RemoveAt(PointF_list.Count - 1);
                }

                //更新图层
                rubber();
            }
        }

        //创建新图层，绘制连线
        //包括折线和曲线，以及端点
        public void rubber()
        {
            //创建bitmap
            Bitmap image = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            pictureBox1.Image = image;
            Graphics g = Graphics.FromImage(image);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            //画直线时的颜色
            Pen mypen = new Pen(Color.FromArgb(255, 0, 0, 0),width);

            SolidBrush myBrush = new SolidBrush(Color.FromArgb(255, 0, 0, 0));

            //如果有多个点
            if (PointF_list.Count >= 1)
            {
                PointF PointF1 = new PointF();
                PointF PointF2 = new PointF();
                //先将列表中的点按照顺序连接起来
                for (int i = 0; i < PointF_list.Count - 1; i++)
                {
                    PointF1 = PointF_list[i];
                    PointF2 = PointF_list[i+1];

                    //将坐标变换到原来的坐标系
                    PointF1.Y = pictureBox1.Height - PointF1.Y;
                    PointF2.Y = pictureBox1.Height - PointF2.Y;

                    g.DrawLine(mypen, PointF1, PointF2);
                }

                //没有点击右键之前需要连接鼠标位置
                if(!right_click)
                {
                    //再将最后一个点和鼠标位置的点相连
                    PointF1 = PointF_list[PointF_list.Count - 1];
                    PointF2 = mouse_position;

                    //将坐标变换到原来的坐标系
                    PointF1.Y = pictureBox1.Height - PointF1.Y;
                    PointF2.Y = pictureBox1.Height - PointF2.Y;
                    g.DrawLine(mypen, PointF1, PointF2);
                }

                //需要绘制端点
                if(check)
                {
                    //为每个点绘制一个矩形
                    for (int i=0;i<PointF_list.Count();i++)
                    {
                        PointF[] polygon = new PointF[4];
                        polygon[0] = new PointF(PointF_list[i].X-5, pictureBox1.Height-PointF_list[i].Y+5);
                        polygon[1] = new PointF(PointF_list[i].X +5, pictureBox1.Height - PointF_list[i].Y + 5);
                        polygon[2] = new PointF(PointF_list[i].X + 5, pictureBox1.Height - PointF_list[i].Y - 5);
                        polygon[3] = new PointF(PointF_list[i].X - 5, pictureBox1.Height - PointF_list[i].Y - 5);
                       
                        g.FillPolygon(myBrush, polygon);
                    }
                }
                
            }

            //超过三个点就需要绘制曲线
            if(PointF_list.Count()>=3)
            {
                Pen mypen1 = new Pen(arc,width);

               //添加初始值
                stack.Add(PointF_list);

                while (stack.Count() != 0)
                {
                    //每次循环都将弹出最后一个元素
                    List < PointF > p = stack[stack.Count()-1].ToList();
                    stack.RemoveAt(stack.Count() - 1);

                    //折线某个点到首尾直线的距离最大值，当最大值小于某个数时可以带起曲线
                    double max = 0;

                    //遍历点列表，找到最大距离
                    for (int i = 1; i < p.Count - 1; i++)
                    {
                        //更新最大距离
                        if (Distance(p[i], p[0], p[p.Count - 1]) > max)
                        {
                            max = Distance(p[i], p[0], p[p.Count - 1]);
                        }
                    }

                    //如果小于阈值就绘制折线代替曲线
                    if (max <= Threshold)
                    {
                        //将坐标变换到绘图坐标系
                        PointF PointF1 = p[0];
                        PointF PointF2 = p[p.Count - 1];
                        PointF1.Y = pictureBox1.Height - PointF1.Y;
                        PointF2.Y = pictureBox1.Height - PointF2.Y;

                        //绘图
                        g.DrawLine(mypen1, PointF1, PointF2);
                    }
                    //如果没有小于阈值的话就调用分割函数，将原来点序列分割
                    else
                    {
                        //创建新的数组
                        List<PointF> Q = new List<PointF>();
                        List<PointF> R = new List<PointF>();

                        //调用函数为数组赋值
                        split(p, out Q, out  R);

                        //将切割后的数组加入栈中
                        stack.Add(Q);
                        stack.Add(R);
                    }
                }
            }
        }

        //分割函数，将一个点列表分割为两个相同大小的列表
        //传入第一个列表为需要分割的列表，后两个列表为结果
        //为了返回值，必须加out
        public void split( List<PointF> P, out List<PointF> Q, out List<PointF> R)
        {
            int count = P.Count - 1;

            //为了保证大小先copy一遍
            Q = P.ToList();
            R = P.ToList();

            //开始切割
            for (int i = 1; i <= count; i++)
            {
                R[count + 1 - i] = Q[count];
                for (int j = count; j >= i; j--)
                {
                    //计算中点
                    float x = (Q[j - 1].X + Q[j].X) / 2;
                    float y = (Q[j - 1].Y + Q[j].Y) / 2;
                    Q[j] = new PointF(x, y);
                }
            }
            R[0] = Q[count];
        }

        //计算距离 传入三个点，计算第一个点到后两个点的直线的距离
        private double Distance(PointF a, PointF b, PointF c)
        {
            //已知一个点P(X0, Y0), 求点到直线Ax + By + C = 0的距离公式为：
            //d = [AX0 + BY0 + C的绝对值] /[(A ^ 2 + B ^ 2)的算术平方根]
            double d = (Math.Abs((c.Y - b.Y) * a.X + (b.X - c.X) * a.Y + ((c.X * b.Y) - (b.X * c.Y))))
                / (Math.Sqrt(Math.Pow(c.Y - b.Y, 2) + Math.Pow(b.X - c.X, 2)));
            return d;
        }

        //选择颜色
        private void color_button_Click(object sender, EventArgs e)
        {
            //选择颜色，绘图
            colorDialog.ShowDialog();
            arc = colorDialog.Color;
            rubber();

        }

        //选择线的粗细
        private void comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            comboBox.Enabled = false;
            comboBox.Enabled = true;

            //根据选项来选择笔的粗细
            if (comboBox.SelectedIndex == 0)
            {
                width = 1;
            }
            if(comboBox.SelectedIndex==1)
            {
                width = 2;
            }

            //绘图
            rubber();
        }

        //点击右键后生效
        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            //之前点击过右键
            if (right_click)
            {
                down = false;
            }
        }

        //点击右键后生效
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            //之前点击过右键则是需要拖动
            if (right_click)
            {
                down = true;
            }
        }

        //是否需要绘制端点
        private void checkBox_CheckedChanged(object sender, EventArgs e)
        {
            //改变判断值后绘图
            if(checkBox.Checked==false)
            {
                check = false;
            }
            if(checkBox.Checked == true)
            {
                check = true;
            }
            rubber();
        }

        //初始化
        private void Draw_button_MouseClick(object sender, MouseEventArgs e)
        {
            //创建bitmap
            Bitmap image = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            pictureBox1.Image = image;

            //清空
            PointF_list.Clear();

            right_click = false;

            down = false;
        }

        //退出
        private void Out_button_MouseClick(object sender, MouseEventArgs e)
        {
            this.Close();
        }
    }
}
