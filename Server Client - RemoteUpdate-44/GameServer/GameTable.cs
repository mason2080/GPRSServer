using System;
using System.Windows.Forms;

namespace GameServer
{
    /// <summary>
    /// 负责向某桌发送棋子以及判断胜负等
    /// </summary>
    class GameTable
    {
        private const int None = -1;        //无棋子
        private const int Black = 0;        //黑色棋子
        private const int White = 1;        //白色棋子

        public Player[] gamePlayer;
        private int[,] grid = new int[15, 15];//15*15的方格
        private System.Timers.Timer timer;      //用于定时产生棋子
        private int NextdotColor = 0;           //应该产生黑棋子还是白棋子

        private ListBox listbox;
        Random rand = new Random();
        Service service;

        public GameTable(ListBox listbox) 
        {
            
            gamePlayer = new Player[2];
            gamePlayer[0]=new Player();//这样子才能调用类的构造函数进行相关的初始化
            gamePlayer[1]=new Player();

            timer = new System.Timers.Timer();
            timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);
            timer.Enabled = false;
            this.listbox = listbox;
            service = new Service(listbox);
            ResetGrid();
        }

        /// <summary>
        /// 重置grid
        /// </summary>
        public void ResetGrid()
        {
            for (int i = 0; i < grid.GetUpperBound(0); i++) 
            {
                for (int j = 0; j < grid.GetUpperBound(1); j++) 
                {
                    grid[i, j] = None;
                }
            }
            gamePlayer[0].grade = 0;
            gamePlayer[1].grade = 0;
        }

        public void StartTimer() 
        {
            timer.Start();
        }

        public void StopTimer() 
        {
            timer.Stop();
        }

        public void SetTimerLevel(int interval) 
        {
            timer.Interval = interval;
        }

        private void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            int x, y;
            //随机产生一个格内没有棋子的单元格位置
            do
            {
                x = rand.Next(15);
                y = rand.Next(15);

            } while (grid[x, y] != None);

            SetDot(x, y, NextdotColor);
            NextdotColor = (NextdotColor + 1) % 2;
        }

        private void SetDot(int x, int y, int NextdotColor)
        {
            //向两个用户发送产生的棋子信息，并判断是否有相邻棋子
            //发送格式：SetDot,行,列,颜色
            grid[x, y] = NextdotColor;
            service.SendToBoth(this, string.Format("SetDot,{0},{1},{2}", x, y, NextdotColor));
            /*----------以下判断当前行是否有相邻点----------*/
            int k1, k2; //k1循环初值，k2：循环终值
            if (x == 0) 
            {
                //如果是首行，只需要判断下边的点
                k1 = k2 = 1;
            }
            else if (x == grid.GetUpperBound(0))
            {
                k1 = k2 = grid.GetUpperBound(0) - 1;
            }
            else 
            {
                //如果是中间的行，上下两边的点都要判断
                k1 = x - 1; k2 = x + 1;
            }

            for (int i = k1; i <= k2; i += 2) 
            {
                if (grid[i, y] == NextdotColor) 
                {
                    ShowWin(NextdotColor);
                }
            }

            /*-------------以下判断当前列是否有相邻点------------------*/
            if (y == 0) 
            {
                k1 = k2 = 0;
            }
            else if (y == grid.GetUpperBound(1))
            {
                k1 = k2 = grid.GetUpperBound(1) - 1;
            }
            else 
            {
                k1 = y - 1; k2 = y + 1;
            }

            for (int j = k1; y <= k2; y += 2) 
            {
                ShowWin(NextdotColor);
            }

        }

        //出现相邻点的颜色为
        private void ShowWin(int NextdotColor)
        {
            timer.Enabled = false;
            gamePlayer[0].started = false;
            gamePlayer[1].started = false;
            this.ResetGrid();

            //发送格式：Win,相邻点的颜色,黑方成绩,白方成绩
            service.SendToBoth(this, string.Format("Win,{0},{1},{2}", NextdotColor, gamePlayer[0].grade, gamePlayer[1].grade));
        }

        public void UnsetDot(int i, int j, int color) 
        {
            //向两个用户发送消去棋子的信息
            //格式：UnsetDot,行,列,黑方成绩,白方成绩
            grid[i, j] = None;
            gamePlayer[color].grade++;
            string str = string.Format("UnsetDot,{0},{1},{2},{3}", i, j, gamePlayer[0].grade, gamePlayer[1].grade);
            service.SendToBoth(this, str);
        }

    }
}
