using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BaiduCSharp1
{
    //用于链接终端设备的socket服务端
    class DeviceSocketServer
    {
        public Form1 parentForm; //主窗口
        public Socket mainSocket;//链接终端设备的socket
        public int clientCount = 0;//终端设备链接计数，也作为锁用
        public System.Collections.ArrayList workerSocketList = ArrayList.Synchronized(new System.Collections.ArrayList());//终端设备socket列表
        public AsyncCallback pfnWorkerCallBack;

        public DeviceSocketServer(Form1 form)
        {
            parentForm = form;
         
        }

        public void Start()
        {
            try
            {
                IPAddress ipAddress = IPAddress.Parse(INIhelp.GetValue("臂挂服务端IP地址"));
                int port = int.Parse(INIhelp.GetValue("臂挂服务端端口"));

                //创建监听Socket
                mainSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                //邦定IP
                IPEndPoint ipLocal = new IPEndPoint(ipAddress, port);
                mainSocket.Bind(ipLocal);

                //开始监听
                mainSocket.Listen(5);

                //创建Call Back为任意客户端连接
                mainSocket.BeginAccept(new AsyncCallback(OnDeviceConnect), null);

            }
            catch (SocketException se)
            {
                parentForm.AppendReceivedMsg(se.Message);
            }
        }

        //回调函数，终端设备连接时被调用
        public virtual void OnDeviceConnect(IAsyncResult asyn)
        {
            try
            {
                // 创建一个新的 Socket 
                Socket workerSocket = mainSocket.EndAccept(asyn);

                // 递增客户端数目
                Interlocked.Increment(ref clientCount);

                // 添加到客户端数组中
                workerSocketList.Add(workerSocket);
                parentForm.AppendReceivedMsg("Client " + clientCount.ToString() + " 链接成功");
                //发送一个消息，暂时不用
                //string msg = "Welcome 客户端 " + clientCount + "\n";
                //SendMsgToClient(msg, clientCount);
                //byte[] byData = new byte[] { 0x48, 0x54, 0x00, 0x11, 0x00, 0x01, 0x01, 0x09, 0xFF, 0xFF, 0x07, 0xE5, 0x0C, 0x07, 0x0, 0x09, 0x38, 0x24, 0x2B };
                //byte[] byData = new byte[] { 0x48, 0x54, 0x00, 0x0A, 0x00, 0x01, 0x01, 0x02, 0x07, 0xDC, 0x24, 0x2B };
                byte[] byData;
                int lengthByData;

                parentForm.GetMsgTime(out byData, out lengthByData);

                Send(workerSocket, byData, byData.Length);

                //刷新已连接的客户端列表,用于更新界面，暂时不用
                //RefreshConnectedClientList();

                //指定这个Socket处理接收到的数据，注意当前socket还没有接到任何数据，还不知道其对应的终端设备ID
                WaitForData(workerSocket, clientCount);
                parentForm.AppendReceivedMsg("发送数据到Client " + clientCount.ToString() + ",数据类型:01 09");

                // Main Socket继续等待客户端的连接
                mainSocket.BeginAccept(new AsyncCallback(OnDeviceConnect), null);
            }
            catch (ObjectDisposedException)
            {
                System.Diagnostics.Debugger.Log(0, "1", "\n 客户端连接: Socket 已关闭\n");
            }
            catch (SocketException se)
            {
                parentForm.AppendReceivedMsg(se.Message);
            }
            catch (Exception se)
            {
                parentForm.AppendReceivedMsg(se.Message);
            }
        }

        //等待终端设备的数据
        public void WaitForData(System.Net.Sockets.Socket skt, int clientNumber)
        {
            try
            {
                if (pfnWorkerCallBack == null)
                    pfnWorkerCallBack = new AsyncCallback(OnDataReceived);

                SocketPacket theSocPkt = new SocketPacket(skt, clientNumber);//内联类
                skt.BeginReceive(theSocPkt.dataBuffer, 0, theSocPkt.dataBuffer.Length, SocketFlags.None, pfnWorkerCallBack, theSocPkt);
                
            }
            catch (SocketException se)
            {
                parentForm.AppendReceivedMsg(se.Message);
            }
        }

        //Call Back, Socket检测到任意终端设备客户端写入数据时
        public virtual void OnDataReceived(IAsyncResult asyn)
        {
            SocketPacket socketData = (SocketPacket)asyn.AsyncState;
            try
            {
                int iRx = socketData.currSocket.EndReceive(asyn);
                //char[] chars = new char[iRx + 1];
                //接收到数据长度，上传到界面
                //AppendReceivedMsg( "Client " + socketData.clientNO + " DataLength:" + charLen.ToString());
                parentForm.AppendReceivedMsg("Client " + socketData.clientNO + " DataLength:" + iRx.ToString());
                //AppendReceivedMsg(Environment.NewLine + "Client " + socketData.clientNO + " Data:" + new System.String(chars));

                //System.Text.Decoder decoder = System.Text.Encoding.UTF8.GetDecoder();
                //int charLen = decoder.GetChars(socketData.dataBuffer, 0, iRx, chars, 0);

                //System.String szData = new System.String(chars);
                //从目前看来，不能捕获客户端主动断开的消息，会进入一个死循环，看看是否可以根据接收到的数据长度为零进行处理
                //现在只能在这里增加一个当接收到的数据长度为0时，发送一条数据给客户端，用于及时检测到客户端断开，
                if (iRx <= 0)
                {
                    //一个电量查询的语句，但是终端编号和校验位都不对
                    byte[] byData = new byte[] { 0x48, 0x54, 0x00, 0x08, 0x00, 0x01, 0x01, 0x23, 0x00, 0x00 };
                    socketData.currSocket.Send(byData);
                }
                else//应该将处理函数放在这里
                {
                    //测试使用，将收到的数据返回给定位终端
                    //Send(socketData.currSocket, socketData.dataBuffer, iRx);
                    //打印接收到的消息，用于调试
                    string strTemp = parentForm.ConvertBytetoCString(socketData.dataBuffer, iRx);
                    System.Diagnostics.Debug.Write(strTemp + "\r\n");

                    //在这里直接调用socket类进行数据解析，数据量少，不额外增加解析线程
                    Buffer.BlockCopy(socketData.dataBuffer, 0, socketData.parseBuffer, socketData.parseBufferLength, iRx);//将数据放入解析缓冲区
                    socketData.parseBufferLength = socketData.parseBufferLength + iRx;//修改解析缓冲区的数据长度

                    //帧头      数据长度    帧序号    指令号    指令内容         校验数据
                    //2个字节   2个字节     2个字节   2个字节   N个字节          2个字节
                    //48 54     00 0D       00 02     00 FF     00 1A 00 09 01   3C D9
                    while (socketData.parseBufferLength >= 10)//数据长度至少为10个字节，才够一包数据，进行数据解析
                    {
                        if (socketData.parseBuffer[0] == 0x48 && socketData.parseBuffer[1] == 0x54)//找到消息头
                        {
                            //解析数据长度
                            int nLength = socketData.parseBuffer[2] * 256 + socketData.parseBuffer[3];//解析数据长度
                            nLength = nLength + 2;
                            if (nLength > socketData.parseBufferLength)//本报数据还没有收全
                                break;

                            //帧序号
                            int nFrameNum = socketData.parseBuffer[4] * 256 + socketData.parseBuffer[5];

                            //指令号

                            //设置指令号
                            byte[] bTemp = new byte[2];
                            bTemp[0] = socketData.parseBuffer[6];
                            bTemp[1] = socketData.parseBuffer[7];

                            string strCommID = parentForm.ConvertBytetoCString(bTemp, 2);
                            System.Diagnostics.Debug.Write("接收终端"+ socketData.clientNO.ToString() + "指令号"+strCommID + "\r\n");

                            //指令号 指令内容
                            //48 54 00 0D 00 00 01 FF 07 DC 01 09 01 CF C7
                            //01 FF 终端编号【2个字节】+设置指令号【2个字节】+结果【1个字节】
                            if (socketData.parseBuffer[6] == 0x01 && socketData.parseBuffer[7] == 0xFF)//解析回执信息
                            {
                                //终端编号【2个字节】
                                int deviceID = socketData.parseBuffer[8] * 256 + socketData.parseBuffer[9];

                                //设置指令号
                                byte[] bTemp1 = new byte[2];
                                bTemp1[0] = socketData.parseBuffer[10];
                                bTemp1[1] = socketData.parseBuffer[11];

                                string strSetCommID = parentForm.ConvertBytetoCString(bTemp1, 2);
                                //结果
                                int nResult = socketData.parseBuffer[12];

                                //回执信息，没有进一步的处理，只是在界面上进行显示
                                parentForm.AppendReceivedMsg("Client " + socketData.clientNO + " DataType = 01FF 终端编号:" + deviceID.ToString() + " 原指令号:" + strSetCommID + " 回执结果" + nResult.ToString());

                            }
                            //48 54 00 11 01 47 01 3A 07 DC 07 E6 01 04 0F 0C 2B 09 98
                            //0x013A 终端编号【2个字节】+时间戳【7个字节】
                            else if (socketData.parseBuffer[6] == 0x01 && socketData.parseBuffer[7] == 0x3A)//解析心跳信息
                            {
                                //终端编号【2个字节】
                                int deviceID = socketData.parseBuffer[8] * 256 + socketData.parseBuffer[9];

                                //时间戳【7个字节】 //时间戳：2017-09-27 10:50:01 转为7字节07 E1, 09, 1C, 0A, 32, 01。
                                int year = socketData.parseBuffer[10] * 256 + socketData.parseBuffer[11];
                                int month = socketData.parseBuffer[12];
                                int day = socketData.parseBuffer[13];
                                int hour = socketData.parseBuffer[14];
                                int minute = socketData.parseBuffer[15];
                                int second = socketData.parseBuffer[16];

                                DateTime dt = new DateTime(year, month, day, hour, minute, second);

                                //服务器（中间件）在接收到到心跳消息后应立即回复终端。终端在丢失3个应答心跳消息后，将断开当前连接，重新连接服务器。
                                //0x0110 终端编号【2个字节】
                                //48 54 00 0B 00 02 01 06 07 DC 16 DA E8
                                byte[] byData;
                                int lengthByData;

                                parentForm.GeHeartReceipt(socketData.parseBuffer ,out  byData, out lengthByData);
                                socketData.currSocket.Send(byData);

                             
                                parentForm.AppendReceivedMsg("Client " + socketData.clientNO + " DataType = 013A 心跳信息,终端编号:" + deviceID.ToString() + "并返回回执");
                            }
                            //0x0137  终端编号【2个字节】+时间戳【7个字节】+有源标签编码【5个字节】+定位信息【不定长】
                            //48 54 00 16 04 92 01 37 07 DC 07 E6 01 05 0C 1E 06 00 00 00 00 00 09 40
                            //48 54 00 93 08 22 01 37 07 DC 07 E6 01 05 0C 25 23 00 00 00 00 00 24 47 4E 47 47 41 2C 30 34 34 30 32 39 2E 30 30 30 2C 34 30 30 33 2E 30 38 35 32 34 34 2C 4E 2C 31 31 36 31 37 2E 37 31 30 37 38 35 2C 45 2C 31 2C 31 35 2C 30 2E 37 39 38 2C 33 32 2E 33 35 39 2C 4D 2C 30 2E 30 30 30 2C 4D 2C 2C 2A 34 31 0D 0A 7C 24 47 4E 56 54 47 2C 32 35 33 2E 36 2C 54 2C 30 30 30 2E 30 2C 4D 2C 30 30 30 2E 30 2C 4E 2C 30 30 34 2E 34 2C 4B 2A 35 32 0D 0A 58 B0
                            //$GNGGA,044029.000,4003.085244,N,11617.710785,E,1,15,0.798,32.359,M,0.000,M,,*41 0d 0a |$GNVTG,253.6,T,000.0,M,000.0,N,004.4,K * 52
                            //48 54 00 94 04 96 01 37 07 DC 07 E6 01 05 0C 1E 08 00 00 00 00 00 24 47 4E 47 47 41 2C 30 34 33 33 30 32 2E 30 30 30 2C 34 30 30 33 2E 30 38 36 37 39 30 2C 4E 2C 31 31 36 31 37 2E 36 38 39 39 32 30 2C 45 2C 31 2C 30 38 2C 31 2E 31 34 33 2C 34 30 2E 38 30 34 2C 4D 2C 30 2E 30 30 30 2C 4D 2C 2C 2A 34 38 0D 0A 7C 24 47 4E 56 54 47 2C 38 37 32 39 2E 34 2C 54 2C 30 30 30 2E 30 2C 4D 2C 30 30 30 2E 30 2C 4E 2C 30 30 33 2E 35 2C 4B 2A 36 36 0D 0A 17 CC 
                            //$GNGGA,043302.000,4003.086790,N,11617.689920,E,1,08,1.143,40.804,M,0.000,M,,*48 0d 0a |$GNVTG,8729.4,T,000.0,M,000.0,N,003.5,K * 66
                            else if (socketData.parseBuffer[6] == 0x01 && socketData.parseBuffer[7] == 0x37)//值解析定位数据
                            {
                                //终端编号【2个字节】
                                int deviceID = socketData.parseBuffer[8] * 256 + socketData.parseBuffer[9];

                                //时间戳【7个字节】 //时间戳：2017-09-27 10:50:01 转为7字节07 E1, 09, 1C, 0A, 32, 01。
                                int year = socketData.parseBuffer[10] * 256 + socketData.parseBuffer[11];
                                int month = socketData.parseBuffer[12];
                                int day = socketData.parseBuffer[13];
                                int hour = socketData.parseBuffer[14];
                                int minute = socketData.parseBuffer[15];
                                int second = socketData.parseBuffer[16];

                                DateTime dt = new DateTime(year, month, day, hour, minute, second);

                                //有源标签编码【5个字节】
                                //按照提供的程序，第一位是表示的电池状态
                                int nRFIDPower = socketData.parseBuffer[17];
                                int nRFIDCode = socketData.parseBuffer[18] * 0x1000000+ socketData.parseBuffer[19] * 0x10000 + socketData.parseBuffer[20] * 0x100+ socketData.parseBuffer[21];

                                

                                //定位信息【不定长】 //定位信息：GPS坐标信息。经度（小数7位表示）|纬度（小数7位表示）|海拔（小数2位表示）
                                //实际是GGA和VTG消息
                                if (nLength - 24 > 6)//剩余的字节至少包含$GNGGA和VTG,其实可以限制至少0x93+2的长度，应该不会有比这个短的
                                {
                                    string strNMEA = Encoding.ASCII.GetString(socketData.parseBuffer, 22, nLength -24);
                                    string[] strNMEAArray = strNMEA.Split(',');//直接按照逗号分割，不区分GGA和VTG，找到所有的数据帧
                                    if (strNMEAArray.Length > 0)
                                    {
                                        if(strNMEAArray[0] == "$GNGGA")
                                        {
                                            //时分秒 hhmmss.sss 044029.000
                                            strTemp = strNMEAArray[1];
                                            DateTime dtUTCNow = DateTime.UtcNow;
                                            DateTime dtDevice = new DateTime(dtUTCNow.Year, dtUTCNow.Month, dtUTCNow.Day,
                                                int.Parse(strTemp.Substring(0, 2)),
                                                int.Parse(strTemp.Substring(2, 2)),
                                                int.Parse(strTemp.Substring(4, 2)),
                                                int.Parse(strTemp.Substring(7, 3)),
                                                DateTimeKind.Utc);

                                            if (dtDevice - dtUTCNow > new TimeSpan(12, 0, 0))
                                            {
                                                dtDevice.AddDays(-1.0);
                                            }

                                            if (dtDevice - dtUTCNow < new TimeSpan(-12, 0, 0))
                                            {
                                                dtDevice.AddDays(1.0);
                                            }

                                            dtDevice = dtDevice.AddHours(8.0);

                                            //纬度，格式为ddmm.mmmm(第一位是零也将传送)，4003.086790
                                            double dLatitude = Convert.ToDouble(strNMEAArray[2].Substring(0,2))+
                                                Convert.ToDouble(strNMEAArray[2].Substring(2))/60;

                                            //经度，格式为dddmm.mmmm(第一位零也将传送)；11617.689920
                                            double dLangtitude = Convert.ToDouble(strNMEAArray[4].Substring(0, 3)) +
                                                Convert.ToDouble(strNMEAArray[4].Substring(3)) / 60;

                                            // 定位质量指示，   0 – 无效1 – 单点定位2 – 差分定位4 – RTK 固定解定位5 – RTK 浮点解定位6 – 推算定位
                                            int nQuality = Convert.ToInt32(strNMEAArray[6]);

                                            //参与定位的卫星数量
                                            int nNumSV = Convert.ToInt32(strNMEAArray[7]);

                                            //HDOP 1.01 水平精度因子
                                            double dHDOP = Convert.ToDouble(strNMEAArray[8]);

                                            //椭球高
                                            double dHeirht = Convert.ToDouble(strNMEAArray[9]);

                                            //差分临期
                                            double diffAge = 0;
                                            if (strNMEAArray[13] != "")
                                            {
                                                diffAge = Convert.ToDouble(strNMEAArray[13]);
                                            }
                                            

                                            //以真北为参考基准的地面航向
                                            double dDirection = Convert.ToDouble(strNMEAArray[15]);

                                            //地面速率，单位为 km/h
                                            double dSpeed = Convert.ToDouble(strNMEAArray[21]);

                                            //更新人员坐标信息
                                            foreach (var temp in parentForm.persons)
                                            {
                                                if (temp.DeviceID == deviceID)
                                                {
                                                    temp.SetPosition(dLangtitude, dLatitude);//更新了系统中的位置,注意百度地图坐标有个偏移，测量之后可以进行转换
                                                }


                                            }
                                         System.Diagnostics.Debug.Write("接收终端" + socketData.clientNO.ToString() + "定位时间" + dtDevice.ToString() + "定位状态" + nQuality.ToString() + "\r\n");
                                        }

 

                                    }
                                }

                                //将解析的定位数据回传到界面用于更新相关信息


                                //解析数据，上传到界面
                                //AppendReceivedMsg( "Client " + socketData.clientNO + " DataLength:" + charLen.ToString());
                                parentForm.AppendReceivedMsg("Client " + socketData.clientNO + " DataType = 0137 终端编号:" + deviceID.ToString() + " 时间:" + dt.ToString() + " " + iRx.ToString());
                                //AppendReceivedMsg(Environment.NewLine + "Client " + socketData.clientNO + " Data:" + new System.String(chars));

                               

                                //暂时放在这里，当结束到定位消息的时候进行设置
                                byte[] byData;
                                int lengthByData;

                                parentForm.GeSetPositionSpan(socketData.parseBuffer, out byData, out lengthByData);
                                socketData.currSocket.Send(byData);


                            }
                            else//其他类型信息暂时不进行处理
                            {

                                //没有进一步的处理，只是在界面上进行显示
                                parentForm.AppendReceivedMsg("Client " + socketData.clientNO + " DataType = :" + strCommID  + "未解析");
                            }

                            Buffer.BlockCopy(socketData.parseBuffer, nLength, socketData.parseBuffer, 0, socketData.parseBufferLength - nLength);//将解析缓冲区数据向前移动一位
                            socketData.parseBufferLength = socketData.parseBufferLength - nLength;//修改解析缓冲区的数据长度
                        }
                        else//消息头不对，向前移动一位
                        {
                            Buffer.BlockCopy(socketData.parseBuffer, 1, socketData.parseBuffer, 0, socketData.parseBufferLength - 1);//将解析缓冲区数据向前移动一位
                            socketData.parseBufferLength = socketData.parseBufferLength - 1;//修改解析缓冲区的数据长度
                        }
                    }




                    //For Debug
                    //string replyMsg = "Server 回复:" + szData.ToUpper();

                    //回复客户端信息，不需要
                    //string replyMsg = "Server 回复: 接收完成";
                    //byte[] byData = System.Text.Encoding.UTF8.GetBytes(replyMsg);

                    //Socket workerSocket = (Socket)socketData.currSocket;
                    //workerSocket.Send(byData);


                    //这个地方对原来的调用进行了优化，原来新创建很多socketData，其实很费资源的
                    //WaitForData(socketData.currSocket, socketData.clientNO);
                    socketData.currSocket.BeginReceive(socketData.dataBuffer, 0, socketData.dataBuffer.Length, SocketFlags.None, pfnWorkerCallBack, socketData);



                }//else


            }//try
            catch (ObjectDisposedException)
            {
                parentForm.AppendReceivedMsg("数据接收时: Socket 已关闭");
            }
            catch (SocketException se)
            {
                if (se.ErrorCode == 10054) // 连接被管道重置
                {
                    //socketData.currSocket.Close();

                    string msg = "Client " + socketData.clientNO + " 断开连接" + "\n";
                    parentForm.AppendReceivedMsg(msg);

                    //这个似乎有问题吧，只是将socket设置为空，待垃圾回收？没有关闭相应的socket
                    Socket workerSocket = (Socket)workerSocketList[socketData.clientNO - 1];
                    //workerSocket.Shutdown(SocketShutdown.Both);
                    workerSocket.Close();
                    workerSocket = null;
                    //workerSocketList[socketData.clientNO - 1] = null;
                    //RefreshConnectedClientList();
                }
                else if (se.ErrorCode == 10053)
                {
                    parentForm.AppendReceivedMsg(se.Message);

                    //这个似乎有问题吧，只是将socket设置为空，待垃圾回收？没有关闭相应的socket
                    Socket workerSocket = (Socket)workerSocketList[socketData.clientNO - 1];
                    //workerSocket.Shutdown(SocketShutdown.Both);
                    workerSocket.Close();
                    workerSocket = null;
                    // workerSocketList[socketData.clientNO - 1] = null;
                    //RefreshConnectedClientList();
                }
            }
            catch (Exception se)
            {
                parentForm.AppendReceivedMsg(se.Message);
                //这个似乎有问题吧，只是将socket设置为空，待垃圾回收？没有关闭相应的socket
                Socket workerSocket = (Socket)workerSocketList[socketData.clientNO - 1];
                //workerSocket.Shutdown(SocketShutdown.Both);
                workerSocket.Close();
                workerSocket = null;
                //workerSocketList[socketData.clientNO - 1] = null;
                //RefreshConnectedClientList();
            }
        }

        public void Send(Socket handler, byte[] byteData, int length)
        {

            // Convert the string data to byte data using ASCII encoding.
            //byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.

            handler.BeginSend(byteData, 0, length, 0,  new AsyncCallback(SendCallback), handler);

        }

        public void SendCallback(IAsyncResult ar)

        {
            try
            {

                // Retrieve the socket from the state object.

                Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.

                int bytesSent = handler.EndSend(ar);

                parentForm.AppendReceivedMsg("Sent bytes to client Length "+bytesSent.ToString());

                //这里发送后关闭socket是不对的，
                //handler.Shutdown(SocketShutdown.Both);
                //handler.Close();

            }

            catch (Exception e)
            {

                Console.WriteLine(e.ToString());

            }

        }

        public void Close()
        {
            if (mainSocket != null)
               // mainSocket.Shutdown(SocketShutdown.Both);
                mainSocket.Close();

            Socket workerSocket = null;
            for (int i = 0; i < workerSocketList.Count; i++)
            {
                workerSocket = (Socket)workerSocketList[i];
                if (workerSocket != null)
                {
                   // workerSocket.Shutdown(SocketShutdown.Both);
                    workerSocket.Close();
                    workerSocket = null;
                }
            }
        }

        //内联类，就是定义了一个buff，其他用处不大，后续可以将设备ID之类的信息保存进来，用于与人员的匹配
        internal class SocketPacket
        {
            public System.Net.Sockets.Socket currSocket;
            public int clientNO;

            public byte[] dataBuffer = new byte[65535];//接收数据的字段，socket本身一包不会这么长，但是还是按照华拓定义的长度来定义
            public byte[] parseBuffer = new byte[65535 * 2];//解析数据的字段，华拓消息类型中字段长度是两个字节，所以这里应该至少为两个字节的两倍
            public int parseBufferLength;//解析数据字段内的数据长度

            public SocketPacket(System.Net.Sockets.Socket socket, int clientNumber)
            {
                currSocket = socket;
                clientNO = clientNumber;
                parseBufferLength = 0;
            }
        }


        public void SendAllClient(byte[] byteData, int length)
        {
            Socket workerSocket = null;
            for (int i = 0; i < workerSocketList.Count; i++)
            {
                workerSocket = (Socket)workerSocketList[i];
                if (workerSocket != null)
                {
                    Send(workerSocket, byteData, length);
                    
                }
            }
        }

        //解析数据返回一个uint
        public uint GetBitU(byte[] buff, uint pos, uint len)
        {
            uint bits = 0;
            uint i;
            for (i = pos; i < pos + len; i++)
                bits = (uint)((bits << 1) + ((buff[i / 8] >> (int)(7 - i % 8)) & 1u));
            return bits;
        }

    }
}
