using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace GameClient
{
    public partial class FormPlaying : Form
    {
        private int tableIndex;
        private int side;
        private int[,] grid = new int[15, 15];//保存颜色，用于消点时进行判断
        private Bitmap blackBitmap;
        private Bitmap whiteBitmap;

        //是否从服务器接收的命令
        private bool isReceiveCommand = false;
        private Service service;
        delegate void LabelDelegate(Label label, string str);
        delegate void ButtonDelegate(Button button, bool flag);
        delegate void RadioButtonDelegate(RadioButton radioButton, bool flag);
        delegate void SetDotDelegate(int i, int j, int dotColor);
        LabelDelegate labelDelegate;
        ButtonDelegate buttonDelegate;
        RadioButtonDelegate radioButtonDelegate;

        public FormPlaying(int tableIndex,int side,StreamWriter sw)
        {
            InitializeComponent();
            this.tableIndex = tableIndex;
            this.side = side;
            labelDelegate = new LabelDelegate(SetLabel);
            buttonDelegate = new ButtonDelegate(SetButton);
            radioButtonDelegate = new RadioButtonDelegate(SetRadioButton);
            blackBitmap = new Bitmap("Image\\black.gif");
            whiteBitmap = new Bitmap("Image\\white.gif");
            service = new Service(listBox1, sw);
        }

        public void SetLabel(Label label, string str)
        {
            if (label.InvokeRequired == true)
            {
                this.Invoke(labelDelegate, label, str);
            }
            else
            {
                label.Text = str;
            }
        }
        private void SetButton(Button button, bool flag)
        {
            if (button.InvokeRequired == true)
            {
                this.Invoke(buttonDelegate, button, flag);
            }
            else
            {
                button.Enabled = flag;
            }
        }
        private void SetRadioButton(RadioButton radioButton, bool flag)
        {
            if (radioButton.InvokeRequired == true)
            {
                this.Invoke(radioButtonDelegate, radioButton, flag);
            }
            else
            {
                radioButton.Checked = flag;
            }
        }

        public void SetDot(int i, int j, int dotColor) 
        {
            service.SetListBox(string.Format("{0},{1},{2}",i,j,dotColor));
            grid[i, j] = dotColor;
            pictureBox1.Invalidate();
        }

        private void FormPlaying_Load(object sender, EventArgs e)
        {
            radioButton3.Checked = true;
            for (int i = 0; i < grid.GetUpperBound(0); i++) 
            {
                for (int j = 0; j < grid.GetUpperBound(1); j++) 
                {
                    grid[i, j] = DotColor.None;
                }
            }

            labelSide0.Text = "";
            labelSide1.Text = "";
            labelGrade0.Text = "";
            labelGrade1.Text = "";
        }

        public void Restart(string str)
        {
            MessageBox.Show(str, "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            ResetGrid();
            SetButton(buttonStart, true);
        }
        private void ResetGrid()
        {
            SetLabel(labelGrade0, "");
            SetLabel(labelGrade1, "");
            for (int i = 0; i <= grid.GetUpperBound(0); i++)
            {
                for (int j = 0; j <= grid.GetUpperBound(1); j++)
                {
                    grid[i, j] = DotColor.None;
                }
            }
            pictureBox1.Invalidate();
        }
        public void UnsetDot(int x, int y)
        {
            grid[x / 20 - 1, y / 20 - 1] = DotColor.None;
            pictureBox1.Invalidate();
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            for (int i = 0; i < grid.GetUpperBound(0); i++) 
            {
                for (int j = 0; j < grid.GetUpperBound(1); j++) 
                {
                    if (grid[i, j] != DotColor.None) 
                    {
                        if (grid[i, j] == DotColor.Black)
                        {
                            g.DrawImage(blackBitmap, (i + 1) * 20, (j + 1) * 20);
                        }
                        else 
                        {
                            g.DrawImage(whiteBitmap, (i + 1) * 20, (j + 1) * 20);
                        }
                    }
                }
            }
        }

        public void SetLevel(string ss)
        {
            isReceiveCommand = true;
            switch (ss)
            {
                case "1":
                    SetRadioButton(radioButton1, true);
                    break;
                case "2":
                    SetRadioButton(radioButton2, true);
                    break;
                case "3":
                    SetRadioButton(radioButton3, true);
                    break;
                case "4":
                    SetRadioButton(radioButton4, true);
                    break;
                case "5":
                    SetRadioButton(radioButton5, true);
                    break;
            }
            isReceiveCommand = false;
        }

        private void radioButton_CheckedChanged(object sender, EventArgs e)
        {
            //isReceiveCommand为true表明是接收服务器设置的难度级别触发的此事件
            //就不需要再向服务器发送
            if (isReceiveCommand == false)
            {
                RadioButton radiobutton = (RadioButton)sender;
                if (radiobutton.Checked == true)
                {
                    //设置难度级别
                    //格式：Time,桌号,难度级别
                    service.SendToServer(string.Format("Level,{0},{1}",
                        tableIndex, radiobutton.Name[radiobutton.Name.Length - 1]));
                }
            }
        }

        private void buttonSend_Click(object sender, EventArgs e)
        {
            //字符串格式：Talk,桌号,对话内容
            service.SendToServer(string.Format("Talk,{0},{1}", tableIndex, textBox1.Text));
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                //字符串格式：Talk,桌号,对话内容
                service.SendToServer(string.Format("Talk,{0},{1}", tableIndex, textBox1.Text));
            }
        }

        private void buttonHelp_Click(object sender, EventArgs e)
        {
            string str =
               "\n本游戏每两人为一组。游戏玩法：用鼠标点击你所在方颜色的点，每\n\n" +
               "消失一个，得一分。当任何一方在同行或同列出现两个相邻的点时，游\n\n" +
               "戏就结束了，此时得分多的为胜方。注意：点击对方的点无效。\n";
            MessageBox.Show(str, "帮助信息");

        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            service.SendToServer(string.Format("Start,{0},{1}", tableIndex, side));
            this.buttonStart.Enabled = false;
        }

        private void buttonExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void FormPlaying_FormClosing(object sender, FormClosingEventArgs e)
        {
            //格式：GetUp,桌号,座位号
            service.SendToServer(string.Format("GetUp,{0},{1}", tableIndex, side));
        }

        //FormRoom中的线程调用此方法关闭此窗体
        public void StopFormPlaying()
        {
            Application.Exit();
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            int x = e.X / 20;
            int y = e.Y / 20;
            if (!(x < 1 || x > 15 || y < 1 || y > 15)) 
            {
                if (grid[x - 1, y - 1] != DotColor.None) 
                {
                    int color = grid[x - 1, y - 1];
                    //发送格式：UnsetDot,桌号,座位号,行,列,颜色
                    service.SendToServer(string.Format(
                       "UnsetDot,{0},{1},{2},{3},{4}", tableIndex, side, x - 1, y - 1, color));
                }
            }
        }

        //参数格式：座位号，labelSide显示的信息，listbox显示的信息
        public void SetTableSideText(string sideString, string labelSideString, string listBoxString) 
        {
            string s = "白方";
            if (sideString == "0") 
            {
                s = "黑方";
            }
            //判断自己是黑方还是白方
            if (sideString == side.ToString()) 
            {
                SetLabel(labelSide1, s + labelSideString);
            }
            else
            {
                SetLabel(labelSide0, s + labelSideString);
            }
            service.SetListBox(listBoxString);
        }

        //参数格式：grade0为黑方成绩，grade1为白方成绩
        public void SetGradeText(string str0, string str1)
        {
            if (side == DotColor.Black)
            {
                SetLabel(labelGrade1, str0);
                SetLabel(labelGrade0, str1);
            }
            else
            {
                SetLabel(labelGrade0, str0);
                SetLabel(labelGrade1, str1);
            }
        }
        public void ShowTalk(string talkMan, string str)
        {
            service.SetListBox(string.Format("{0}说：{1}", talkMan, str));
        }
        public void ShowMessage(string str)
        {
            service.SetListBox(str);
        }


    }
}
