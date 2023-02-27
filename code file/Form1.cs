using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace BaiduCSharp1
{


    public partial class Form1 : Form
    {
        public delegate void UpdateReceiveMsgCallback(string msg);//接收需要显示信息的委托

        //public delegate void UpdateDevicePosition(int deviceID, DateTime dt, int rfidID, double longtitude, double latitude);//接收终端设备定位数据的委托


        public List<Person> persons = new List<Person>();//人员信息列表

        DeviceSocketServer deviceSocketServer;//终端设备socket服务端
        BaseSocketServer baseSocketServer;//基准站socket服务端

        public Form1()
        {
            InitializeComponent();


        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //读取配置文件----------------------------------------------------------------------------------------

            //读取人员信息，目前只有5个用户
            persons.Clear();
            persons.Add(new Person(INIhelp.GetValue("用户1"), int.Parse(INIhelp.GetValue("设备1"))));
            persons.Add(new Person(INIhelp.GetValue("用户2"), int.Parse(INIhelp.GetValue("设备2"))));
            persons.Add(new Person(INIhelp.GetValue("用户3"), int.Parse(INIhelp.GetValue("设备3"))));
            persons.Add(new Person(INIhelp.GetValue("用户4"), int.Parse(INIhelp.GetValue("设备4"))));
            persons.Add(new Person(INIhelp.GetValue("用户5"), int.Parse(INIhelp.GetValue("设备5"))));


            //添加Treview信息
            TreeNode tn = new TreeNode();
            tn.Text = INIhelp.GetValue("组名");
            treeView1.Nodes.Add(tn);

            foreach (var temp in persons)
            {
                TreeNode tnchild = new TreeNode();
                tnchild.Text = temp.Name;
                tnchild.Name = temp.DeviceID.ToString();
                tn.Nodes.Add(tnchild);
            }

            //添加textbox信息
            textboxAppend("配置文件读取成功，TreeView信息添加成功");


            //打开百度地图----------------------------------------------------------------------------------------
            try
            {
                string str_url = Application.StartupPath + "\\baidumap.html";
                //string str_url = Application.StartupPath + "\\baiduglmap.html"; 
                //Uri url = new Uri(str_url);
                //webBrowser1.Url = url;
                //webBrowser1.ObjectForScripting = this;

                webBrowser1.Navigate(str_url);

                textboxAppend("百度地图加载成功");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "异常", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }


            webBrowser1.ScriptErrorsSuppressed = false;

            // webBrowser1.Document.GetElementById("address").InnerText = tboxAddress.Text;//设置地址

            // webBrowser1.Document.InvokeScript("GotoSepcialAddress");//跳转

            //启动设备端socket---------------------------------------------------------------------
            deviceSocketServer = new DeviceSocketServer(this);
            deviceSocketServer.Start();

            //启动基准站端socket---------------------------------------------------------------------
            baseSocketServer = new BaseSocketServer(this);
            baseSocketServer.Start();

            //启动定时器----------------------------------------------------------------------------
            timer2.Interval = 1000;
            timer2.Start();


        }


        //添加文本框信息
        private void textboxAppend(string str)
        {

            int n = textBox1.Lines.GetUpperBound(0);
            if (n >= 200)
            {
                textBox1.Clear();
            }

            textBox1.AppendText(str + "\r\n");
        }

        //定时器
        private void timer2_Tick(object sender, EventArgs e)
        {
            foreach (var temp in persons)
            {


                if (temp.BUpdatePosition)
                {
                    //更新地图显示
                    string s1 = temp.Longtitude.ToString();
                    string s2 = temp.Latitude.ToString();
                    string s3 = temp.Name;
                    string s4 = temp.DeviceID.ToString();
                    string s5 = persons.IndexOf(temp).ToString();
                    Object[] objArray = new Object[5];
                    objArray[0] = s1;
                    objArray[1] = s2;
                    objArray[2] = s3;
                    objArray[3] = s4;
                    objArray[4] = s5;

                    temp.BUpdatePosition = false;

                    webBrowser1.Document.InvokeScript("addMarker", objArray);//跳转
                }
 
            }

            /*     测试程序，生成随机数，显示图标位置      
            //更新位置显示信息
            //构造两组位置，用于测试界面地图显示
            double longtitude1 = 103.662803;
            double latitude1 = 36.147378;


            System.Random r = new Random();//生成随机数 ;

            foreach (var temp in persons)
            {
                double longSpan = r.Next(0, 100) / 10000.0 + longtitude1;
                double latiSpan = r.Next(0, 100) / 10000.0 + latitude1;

                temp.SetPosition(longSpan, latiSpan);//更新了系统中的位置

                //更新地图显示
                string s1 = temp.Longtitude.ToString();
                string s2 = temp.Latitude.ToString();
                string s3 = temp.Name;
                string s4 = temp.DeviceID.ToString();
                string s5 = persons.IndexOf(temp).ToString();
                Object[] objArray = new Object[5];
                objArray[0] = s1;
                objArray[1] = s2;
                objArray[2] = s3;
                objArray[3] = s4;
                objArray[4] = s5;

                webBrowser1.Document.InvokeScript("addMarker", objArray);//跳转

            }
            */
        }

        //显示接收终端信息
        public void AppendReceivedMsg(string msg)
        {
            if (InvokeRequired)
                textBox1.BeginInvoke(new UpdateReceiveMsgCallback(UpdateReceivedMsg), msg);
            else
                UpdateReceivedMsg(msg);
        }

        private void UpdateReceivedMsg(string msg)
        {
            textboxAppend(DateTime.Now + "接收到数据：" + msg);
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            //在这里应该关闭所有socket，释放资源
            deviceSocketServer.Close();
            baseSocketServer.Close();
        }

        public void ReceiveBaseData(byte[] byteData, int length)//接收基准站差分数据
        {
            //将接收到的数据进行打包，目前是收到直接转发，并不进行将所有数据组包
            byte[] byteSend = new byte[length + 10];
            byteSend[0] = 0x48;
            byteSend[1] = 0x54;

            int nTemp = length + 8;//长度
            byteSend[2] = (byte)((nTemp >> 8) & 0xFF);
            byteSend[3] = (byte)(nTemp  & 0xFF);

            byteSend[4] = (byte)0x00;//帧号
            byteSend[5] = (byte)0x01;

            byteSend[6] = (byte)0x01;//指令号
            byteSend[7] = (byte)0x12;

            Buffer.BlockCopy(byteData, 0, byteSend, 8, length);//将数据放入解析缓冲区

            UInt16 testCRC;
            testCRC = CmdCRC.CCITT(byteSend, length + 8);

            byteSend[length + 8] = Convert.ToByte((testCRC & 0xFF00) >> 8);//校验位
            byteSend[length + 9] = Convert.ToByte((testCRC & 0x00FF));
          

            deviceSocketServer.SendAllClient(byteSend, length + 10);
        }


        public void GetMsgTime(out byte[] buff, out int lengthBuf)//打包终端登录后直接返回的信息，这个在接口文件中没有，根据接收到的平台信息分析
        { 
            System.DateTime currentTime = System.DateTime.Now;
            int year = currentTime.Year;
            int month = currentTime.Month;
            int day = currentTime.Day;
            int hour = currentTime.Hour;
            int minute = currentTime.Minute;
            int second = currentTime.Second;


            byte[] testByte = new byte[] {0x48, 0x54, 0x00, 0x11, 0x00, 0x02, 0x01, 0x09, 0x09, 0x09, 0x07, 0xE5, 0x0C, 0x18, 0x0E, 0x0E, 0x19, 0x72, 0x2E };//定义一个数组
            int testBytePoint = 10;

            testByte[testBytePoint] = (byte)((year >> 8) & 0xFF);//年
            testBytePoint++;

            testByte[testBytePoint] = (byte)(year & 0xFF);
            testBytePoint++;

            testByte[testBytePoint] = (byte)(month  & 0xFF);//月
            testBytePoint++;

            testByte[testBytePoint] = (byte)(day & 0xFF);//日
            testBytePoint++;

            testByte[testBytePoint] = (byte)(hour & 0xFF);//时
            testBytePoint++;

            testByte[testBytePoint] = (byte)(minute & 0xFF);//分
            testBytePoint++;

            testByte[testBytePoint] = (byte)(second & 0xFF);//秒
            testBytePoint++;

            UInt16 testCRC;
            testCRC = CmdCRC.CCITT(testByte, testBytePoint);

            testByte[testBytePoint] = Convert.ToByte((testCRC & 0xFF00) >> 8);//校验位
            testBytePoint++;

            testByte[testBytePoint] = Convert.ToByte((testCRC & 0x00FF));
            testBytePoint++;

            buff = testByte;
            lengthBuf = testBytePoint;

        }

        public void GeHeartReceipt(byte [] receiveBuff, out byte[] buff, out int lengthBuf)//打包心跳回执信息，根据接收到的消息进行打包，跟接口文档打不一致，多一个字节
        {
            //0x0110      终端编号【2个字节】,但是参考的源程序消息ID的定义是01 00
            // 48 54 00 0B 00 02 01 10 07 DC 00 94 21
            // 48 54 00 0B 00 02 01 10 07 DC 00 94 21
            //byte[] testByte = new byte[] { 0x48, 0x54, 0x00, 0x0A, 0x00, 0x01, 0x01, 0x10, 0x09, 0x09, 0x72, 0x2E };//定义一个数组
            byte[] testByte = new byte[] { 0x48, 0x54, 0x00, 0x0B, 0x00, 0x02, 0x01, 0x10, 0x09, 0x09, 0x00, 0x72, 0x2E };//定义一个数组

            //序号
            testByte[4] = receiveBuff[4];
            testByte[5] = receiveBuff[5];

            //终端编号
            testByte[8] = receiveBuff[8];
            testByte[9] = receiveBuff[9];

            int testBytePoint = 11;
            UInt16 testCRC;
            testCRC = CmdCRC.CCITT(testByte, testBytePoint);

            testByte[testBytePoint] = Convert.ToByte((testCRC & 0xFF00) >> 8);//校验位
            testBytePoint++;

            testByte[testBytePoint] = Convert.ToByte((testCRC & 0x00FF));
            testBytePoint++;

            buff = testByte;
            lengthBuf = testBytePoint;

        }

        public void GeSetPositionSpan(byte[] receiveBuff, out byte[] buff, out int lengthBuf)//打包设置定位回传消息
        {
            //0x0106     终端编号【2个字节】,但是参考的源程序消息ID的定义是01 00
            //48 54 00 0B 00 02 01 06 07 DC 16 DA E8
            //byte[] testByte = new byte[] { 0x48, 0x54, 0x00, 0x0A, 0x00, 0x01, 0x01, 0x10, 0x09, 0x09, 0x72, 0x2E };//定义一个数组
            byte[] testByte = new byte[] { 0x48, 0x54, 0x00, 0x0B, 0x00, 0x02, 0x01, 0x06, 0x09, 0x09, 0x16, 0x72, 0x2E };//定义一个数组

            //序号
            testByte[4] = receiveBuff[4];
            testByte[5] = receiveBuff[5];

            //终端编号
            testByte[8] = receiveBuff[8];
            testByte[9] = receiveBuff[9];

            int testBytePoint = 11;
            UInt16 testCRC;
            testCRC = CmdCRC.CCITT(testByte, testBytePoint);

            testByte[testBytePoint] = Convert.ToByte((testCRC & 0xFF00) >> 8);//校验位
            testBytePoint++;

            testByte[testBytePoint] = Convert.ToByte((testCRC & 0x00FF));
            testBytePoint++;

            buff = testByte;
            lengthBuf = testBytePoint;

        }

        public string ConvertBytetoCString(byte[] buff, int lengthBuff)
        {
            string strResult = "";
            string strTemp;

            for (int i = 0; i < lengthBuff; i++)
            {
                strTemp = buff[i].ToString("X2");
                strResult += strTemp;
                strResult += " ";
            }

            return strResult;



        }

        private void 人员管理ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //MessageBox.Show("人员管理");
            FormPerson formPerson = new FormPerson();

            formPerson.listView1.BeginUpdate();   //数据更新，UI暂时挂起，直到EndUpdate绘制控件，可以有效避免闪烁并大大提高加载速度
            for (int i = 0; i < 5; i++)   //添加10行数据
            {
                ListViewItem lvi = new ListViewItem();

                lvi.Text =  i.ToString();
                lvi.SubItems.Add(persons[i].Name);
                lvi.SubItems.Add(persons[i].DeviceID.ToString());

                formPerson.listView1.Items.Add(lvi);
            }

            formPerson.listView1.EndUpdate();  //结束数据处理，UI界面一次性绘制。


            formPerson.ShowDialog();
        }

        private void 设备管理_Click(object sender, EventArgs e)
        {
            //MessageBox.Show("设备管理");
            FormDevice formDevice = new FormDevice();

            formDevice.listView1.BeginUpdate();   //数据更新，UI暂时挂起，直到EndUpdate绘制控件，可以有效避免闪烁并大大提高加载速度

            ListViewItem lv1 = new ListViewItem();
            lv1.Text = "1";
            lv1.SubItems.Add("基准站");
            lv1.SubItems.Add(INIhelp.GetValue("基准站服务端IP地址"));
            lv1.SubItems.Add(INIhelp.GetValue("基准站服务端端口"));
            formDevice.listView1.Items.Add(lv1);

            ListViewItem lv2 = new ListViewItem();
            lv2.Text = "2";
            lv2.SubItems.Add("终端设备");
            lv2.SubItems.Add(INIhelp.GetValue("臂挂服务端IP地址"));
            lv2.SubItems.Add(INIhelp.GetValue("臂挂服务端端口"));
            formDevice.listView1.Items.Add(lv2);


            formDevice.listView1.EndUpdate();  //结束数据处理，UI界面一次性绘制。

            formDevice.ShowDialog();
        }

   
        private void 帮助ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("本软件为朱若冲所有！");
        }

    }
}
