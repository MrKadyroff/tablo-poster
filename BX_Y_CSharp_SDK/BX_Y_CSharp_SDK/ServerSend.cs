using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;

namespace BX_Y_CSharp_SDK
{
    class ServerSend
    {
        //启动服务器,已停用
        public static void StartServer()
        {
            Console.Write("请输入服务器IP：");
            //服务器IP
            byte[] serverip= Encoding.ASCII.GetBytes(Console.ReadLine());
            Console.Write("请输入服务器port：");
            //服务器端口
            ushort port = ushort.Parse(Console.ReadLine());
            int err = 0;
            //端口，启动服务器
            IntPtr er = LedYNetSdk.Start_Native_Server(port);
            List<LedYNetSdk.BroadCast2> BroadCastlist = new List<LedYNetSdk.BroadCast2>();
            byte[] datas = new byte[1024 * 64];
            int data_count = 0;
            //等待控制卡上线sd
            while (true)
            {
                LedYNetSdk.Get_CardList(datas, ref data_count);
                if (data_count > 0) { break; }
                Thread.Sleep(1000);
            }
            //控制卡上线数据处理
            for (int i = 0; i < data_count; i++)
            {
                //LedYNetSdk.BroadCast2 BroadCast;
                IntPtr dec = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(LedYNetSdk.BroadCast2)));
                Marshal.Copy(datas, Marshal.SizeOf(typeof(LedYNetSdk.BroadCast2)) * i, dec, Marshal.SizeOf(typeof(LedYNetSdk.BroadCast2)));
                LedYNetSdk.BroadCast2 bc = (LedYNetSdk.BroadCast2)Marshal.PtrToStructure(dec, typeof(LedYNetSdk.BroadCast2));
                BroadCastlist.Add(bc);
                Marshal.FreeHGlobal(dec);
                string pid = System.Text.Encoding.Unicode.GetString(bc.pid).Split('\0')[0].Replace("&#x0;", "").Replace("쳌", "");
                string barcode = System.Text.Encoding.Unicode.GetString(bc.barcode).Split('\0')[0].Replace("&#x0;", "").Replace("쳌", "");
                if (string.IsNullOrEmpty(pid) || bc.port < 0 || bc.port > 65535 || bc.screen_type < 0 || bc.screen_type > 65535 || pid.Length != 32)
                {
                    continue;
                }
            }
            //根据控制卡条形码获取通讯端口
            string barcode1 = System.Text.Encoding.Unicode.GetString(BroadCastlist[0].barcode).Split('\0')[0].Replace("&#x0;", "").Replace("쳌", "");
            int port2 = LedYNetSdk.Get_Port_Barcode(barcode1);
            //根据控制卡唯一码获取通讯端口  以这个为主
            string pid1 = System.Text.Encoding.Unicode.GetString(BroadCastlist[0].pid).Split('\0')[0].Replace("&#x0;", "").Replace("쳌", "");
            int port1 = LedYNetSdk.Get_Port_Pid(pid1);


            //数据发送步骤和固定IP方法一样
            Program.ip=serverip;
            Program.port = (ushort)port1;
            Console.WriteLine("请选择控制卡型号 ");
            Console.WriteLine("0.BX-Y04 ");
            Console.WriteLine("1.BX-Y08 ");
            Console.WriteLine("2.BX-Y2 ");
            Console.WriteLine("3.BX-Y2L ");
            Console.WriteLine("4.BX-Y3 ");
            Console.WriteLine("5.BX-Y5E ");
            Console.WriteLine("6.BX-Y1 ");
            Console.WriteLine("7.BX-Y3X ");
            Console.WriteLine("8.BX-Y1L ");
            Console.WriteLine("9.BX-YL5 ");
            Console.WriteLine("10.BX-Y1A ");
            Console.WriteLine("11.BX-Y08A");
            Console.WriteLine("12.BX-Y3A ");
            Console.WriteLine("13.BX-Y3E");
            Console.Write("请输入控制卡型号:");
            switch (int.Parse(Console.ReadLine()))
            {
                case 0:
                    Program.Type = 8280;//Y04
                    break;
                case 1:
                    Program.Type = 8536;//Y08
                    break;
                case 2:
                    Program.Type = 8792;//Y2
                    break;
                case 3:
                    Program.Type = 9304;//Y2L
                    break;
                case 4:
                    Program.Type = 9048;//Y3
                    break;
                case 5:
                    Program.Type = 10584;//Y5E
                    break;
                case 6:
                    Program.Type = 9560;//Y1
                    break;
                case 7:
                    Program.Type = 9816;//Y3X
                    break;
                case 8:
                    Program.Type = 10072;//Y1L
                    break;
                case 9:
                    Program.Type = 10840;//YL5
                    break;
                case 10:
                    Program.Type = 11608;//Y1A
                    break;
                case 11:
                    Program.Type = 11352;//Y08A
                    break;
                case 12:
                    Program.Type = 10328;//Y3A
                    break;
                case 13:
                    Program.Type = 11096;//Y3E
                    break;
                default:
                    Console.WriteLine("类型错误！！！");
                    Program.BL = false;
                    break;
            }
            if (Program.BL)
            {
                int run = 0;
                Console.Write("请输入控制卡宽度：");
                Program.width = int.Parse(Console.ReadLine());
                Console.Write("请输入控制卡高度：");
                Program.height = int.Parse(Console.ReadLine());
                do
                {
                    try
                    {
                        Console.WriteLine("请选择");
                        Console.WriteLine("0.节目【支持图文，时间，表盘，视频。。。多种数据显示，整体更新】");
                        Console.WriteLine("1.动态区【32个动态区，可独立更新，适合频繁更新图文数据】");
                        Console.WriteLine("2.其它【校时，截取屏幕。。。。】");
                        Console.Write("请选择发送方式：");
                        switch (int.Parse(Console.ReadLine()))
                        {
                            case 0:
                                Console.Write("请选择发送内容（0.图片，1.时间，2.表盘，3.视频，4.字幕，5.农历，6.炫彩字，7.计时，8.富文本）：");
                                #region
                                switch (int.Parse(Console.ReadLine()))
                                {
                                    //节目图片
                                    case 0:
                                        Send_ProgramPictures.Program_img();
                                        break;
                                    //节目时间
                                    case 1:
                                        Send_ProgramTime.Program_time();
                                        break;
                                    //节目表盘
                                    case 2:
                                        Send_ProgramClock.Program_clock();
                                        break;
                                    //节目视频
                                    case 3:
                                        Send_ProgramVideo.Program_video();
                                        break;
                                    //节目字幕
                                    case 4:
                                        Send_ProgramStr.Program_str();
                                        break;
                                    //节目农历
                                    case 5:
                                        Send_ProgramCalendar.Program_calendar();
                                        break;
                                    //节目炫彩字
                                    case 6:
                                        Send_ProgramColorStr.Programstr();
                                        break;
                                    //节目计时
                                    case 7:
                                        Send_ProgramCount.Program_count();
                                        break;
                                    //节目富文本
                                    case 8:
                                        Send_ProgramStr.Program_str_line();
                                        break;
                                    //节目文本带边框
                                    case 9:
                                        //border.Program_border();
                                        break;
                                    default:
                                        Console.WriteLine("类型错误！！！");
                                        Program.BL = false;
                                        break;
                                }
                                #endregion
                                break;
                            case 1:
                                Console.Write("请选择发送内容（0.图片，1.txt文件【UTF-8】，2.字符串）：");
                                #region
                                switch (int.Parse(Console.ReadLine()))
                                {
                                    //动态区图片
                                    case 0:
                                        Send_DynamicFile.Program_dynamicfile(0);
                                        break;
                                    //动态区txt文件
                                    case 1:
                                        Send_DynamicFile.Program_dynamicfile(1);
                                        break;
                                    //动态区文本
                                    case 2:
                                        Send_DynamicStr.Program_dynamictest();
                                        break;
                                    //动态区json分区
                                    //case 3:
                                    //    Send_Dynamic.OnCreateJsonDynamic();
                                    //    break;
                                    default:
                                        Console.WriteLine("类型错误！！！");
                                        Program.BL = false;
                                        break;
                                }
                                #endregion
                                break;
                            case 2:
                                Console.WriteLine("请选择发送内容");
                                Console.WriteLine("0.校时");
                                Console.WriteLine("1.屏幕截取");
                                Console.WriteLine("2.自动调亮，");
                                Console.WriteLine("3.音频设置");
                                Console.WriteLine("4.取得屏幕状态");
                                Console.WriteLine("5.加载字库");
                                Console.WriteLine("6.查询字库");
                                Console.WriteLine("7.删除字库");
                                Console.WriteLine("8.开机");
                                Console.WriteLine("9.关机");
                                Console.WriteLine("10.定时开关机");
                                Console.WriteLine("11.取消定时开关机");
                                Console.WriteLine("12.强制调亮");
                                Console.WriteLine("13.定时调亮");
                                Console.WriteLine("14.查询固件版本");
                                Console.WriteLine("15.设置系统语言");
                                Console.WriteLine("16.设置控制卡IP");
                                Console.WriteLine("17.搜索同网段局域网控制卡");
                                Console.WriteLine("18.设置控制卡mac地址");
                                Console.WriteLine("19.取得屏幕参数");
                                Console.WriteLine("20.设置logo");
                                Console.WriteLine("21.查询传感器");
                                Console.WriteLine("22.清除显示节目");
                                Console.WriteLine("23.清除显示动态区");
                                Console.Write("请选择发送内容：");
                                #region
                                switch (int.Parse(Console.ReadLine()))
                                {
                                    //校时
                                    case 0:
                                        OtherFunctions.check_time();
                                        break;
                                    //屏幕截取
                                    case 1:
                                        OtherFunctions.get_capture();
                                        break;
                                    //自动调亮
                                    case 2:
                                        OtherFunctions.set_screen_auto_brightness();
                                        break;
                                    //音频设置
                                    case 3:
                                        OtherFunctions.music();
                                        break;
                                    //取得屏幕状态
                                    case 4:
                                        OtherFunctions.get_screen_status();
                                        break;
                                    //加载字库
                                    case 5:
                                        OtherFunctions.set_font();
                                        break;
                                    //查询字库
                                    case 6:
                                        OtherFunctions.get_font();
                                        break;
                                    //删除字库
                                    case 7:
                                        OtherFunctions.del_font();
                                        break;
                                    //开机
                                    case 8:
                                        OtherFunctions.set_screen_turnonoff(1);
                                        break;
                                    //关机
                                    case 9:
                                        OtherFunctions.set_screen_turnonoff(0);
                                        break;
                                    //定时开关机
                                    case 10:
                                        OtherFunctions.set_screen_cus_turnonoff();
                                        break;
                                    //取消定时开关机
                                    case 11:
                                        OtherFunctions.cancel_screen_cus_turnonoff();
                                        break;
                                    //强制调亮
                                    case 12:
                                        Console.Write("请输入亮度值(1-255):");
                                        OtherFunctions.set_screen_brightness(int.Parse(Console.ReadLine()));
                                        break;
                                    //定时调亮
                                    case 13:
                                        OtherFunctions.set_screen_cus_brightness();
                                        break;
                                    //查询固件版本
                                    case 14:
                                        OtherFunctions.get_firmware();
                                        break;
                                    //设置系统语言
                                    case 15:
                                        OtherFunctions.set_screen_language();
                                        break;
                                    //设置控制卡IP
                                    case 16:
                                        OtherFunctions.set_screen_ip();
                                        break;
                                    //搜索同网段局域网控制卡
                                    case 17:
                                        OtherFunctions.SearchCards();
                                        break;
                                    //设置控制卡mac地址
                                    case 18:
                                        OtherFunctions.set_screen_mac();
                                        break;
                                    //取得屏幕参数
                                    case 19:
                                        OtherFunctions.get_screen_parameters();
                                        break;
                                    //设置logo
                                    case 20:
                                        OtherFunctions.set_screen_logo();
                                        break;
                                    //查询传感器
                                    case 21:
                                        OtherFunctions.get_sensor();
                                        break;
                                    //清除显示
                                    case 22:
                                        OtherFunctions.clearprogram();
                                        break;
                                    case 23:
                                        OtherFunctions.cleardynamic();
                                        break;
                                    default:
                                        Console.WriteLine("类型错误！！！");
                                        Program.BL = false;
                                        break;
                                }
                                #endregion
                                break;
                            default:
                                Console.WriteLine("类型错误！！！");
                                Program.BL = false;
                                break;
                        }
                        Console.WriteLine("是否重新发送（1.确定，0.退出）：");
                        run = int.Parse(Console.ReadLine());
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("数据错误，请重新选择！");
                        run = 1;
                    }
                } while (run == 1 ? true : false);
            }


            //关闭服务器
            LedYNetSdk.Stop_Server(er);
        }
    }
}
