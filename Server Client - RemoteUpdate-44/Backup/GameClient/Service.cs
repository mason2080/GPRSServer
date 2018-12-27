using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace GameClient
{
    /// <summary>
    /// 提供公用的方法
    /// </summary>
    class Service
    {
        ListBox listbox;
        StreamWriter sw;
        public Service(ListBox listbox, StreamWriter sw) 
        {
            this.listbox = listbox;
            this.sw = sw;
        }

        public void SendToServer(string str) 
        {
            try
            {
                sw.WriteLine(str);
                sw.Flush();
            }
            catch 
            {
                SetListBox(str);
            }
        }

        delegate void ListBoxCallBack(string str);
        public void SetListBox(string str)
        {
            if (listbox.InvokeRequired)
            {
                ListBoxCallBack d = new ListBoxCallBack(SetListBox);
                listbox.Invoke(d, str);
            }
            else 
            {
                listbox.Items.Add(str);
                listbox.SelectedIndex = listbox.Items.Count - 1;
                listbox.ClearSelected();
            }
        }
    }
}
