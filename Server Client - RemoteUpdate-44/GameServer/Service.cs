using System.Windows.Forms;

namespace GameServer
{
    /// <summary>
    /// 提供公用的方法
    /// </summary>
    class Service
    {
        private ListBox listbox;
        //用于线程间的互操作
        private delegate void SetListBoxCallback(string str);
        private SetListBoxCallback setListBoxCallBack;
        public Service(ListBox listbox) 
        {
            this.listbox = listbox;
            setListBoxCallBack = new SetListBoxCallback(SetListBox);
        }

        public void SetListBox(string str) 
        {
            //比较调用SetListBox方法的线程和创建listBox1的线程是否同一个线程
            //如果不是，则listBox1的InvokeRequired为true

            //结果为false，直接执行
            if (listbox.Items.Count > 100)
            {
                listbox.Items.Clear();
            }

            if (listbox.InvokeRequired)
            {
                //结果为true，则通过代理执行else中的代码,并传入需要的参数
                listbox.Invoke(setListBoxCallBack, str);
                listbox.Update();
            }
            else 
            {

                listbox.Items.Add(str);
                listbox.SelectedIndex = listbox.Items.Count - 1;
                listbox.ClearSelected();
            }
        }

        /// <summary>
        /// 向某一人发送信息
        /// </summary>
        /// <param name="user"></param>
        /// <param name="str"></param>
        public void SendToOne(User user, string str) 
        {
            try
            {
                user.sw.WriteLine(str);
                user.sw.Flush();
                SetListBox(string.Format("向{0}发送{1}", user.userName, str));

            }
            catch 
            {
                SetListBox(string.Format("向{0}发送信息失败", user.userName));
            }
        }

        /// <summary>
        /// 向一桌人发送信息
        /// </summary>
        /// <param name="gameTable"></param>
        /// <param name="str"></param>
        public void SendToBoth(GameTable gameTable, string str) 
        {
            for (int i = 0; i < 2; i++) 
            {
                if (gameTable.gamePlayer[i].someone) 
                {
                    SendToOne(gameTable.gamePlayer[i].user, str);
                }
            }
        }

        /// <summary>
        /// 向所有人发送信息
        /// </summary>
        /// <param name="userList"></param>
        /// <param name="str"></param>
        public void SendToAll(System.Collections.Generic.List<User> userList, string str) 
        {
            for (int i = 0; i < userList.Count; i++) 
            {
                SendToOne(userList[i], str);
            }
        }



    }
}
