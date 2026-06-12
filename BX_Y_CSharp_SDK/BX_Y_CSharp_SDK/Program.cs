using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;

namespace BX_Y_CSharp_SDK
{
    public class Program
    {
        //控制卡IP
        public static byte[] ip = Encoding.ASCII.GetBytes("192.168.89.133");
        //控制卡类型
        public static int Type = 8536;
        //控制卡宽度
        public static int width = 64;
        //控制卡高度
        public static int height = 32;
        //分区宽度
        public static int areaw = 32;
        //分区高度
        public static int areah = 32;
        //端口
        public static ushort port = 80;
        //用户名，密码
        public static string str = "guest";
        //是否执行
        public static bool BL = true;
        public static int err = 0;
        static void Main(string[] args)
        {
            //FileStream fs = new FileStream("log.txt", FileMode.OpenOrCreate);
            //StreamWriter sw = new StreamWriter(fs);
            LedYNetSdk.init_sdk();
            //err = LedYNetSdk.get_screen_player_mode(ip, 6080, str, str, ref areaw);
            //Send_DynamicFile.Program_dynamicfile(0);
            //Send_DynamicStr.Program_dynamictest1();
            //err = LedYNetSdk.upload_file(ip, 80, str, str, "Picture/Lane0.bmp", "Lane0", IntPtr.Zero);
                    Console.Write("请输入控制卡IP：");
                    Program.ip = Encoding.ASCII.GetBytes(Console.ReadLine());
                    Console.Write("请输入控制卡port：");
                    Program.port = ushort.Parse(Console.ReadLine());
                    //int d=LedYNetSdk.add_tts_voice(ip, 80, str, str,"123",1,1,0,100,50,50,0,1);
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
                    Console.WriteLine("14.BX-C01");
                    Console.WriteLine("15.BX-C04");
                    Console.WriteLine("16.BX-C08");
                    Console.WriteLine("17.BX-C08A");
                    Console.WriteLine("18.BX-C1");
                    Console.WriteLine("19.BX-C1A");
                    Console.WriteLine("20.BX-C2");
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
                        case 14:
                            Program.Type = 33024;//C01
                            break;
                        case 15:
                            Program.Type = 33025;//C04
                            break;
                        case 16:
                            Program.Type = 33026;//C08
                            break;
                        case 17:
                            Program.Type = 33027;//C08A
                            break;
                        case 18:
                            Program.Type = 33028;//C1
                            break;
                        case 19:
                            Program.Type = 33029;//C1A
                            break;
                        case 20:
                            Program.Type = 33030;//C2
                            break;
                        default:
                            Console.WriteLine("类型错误！！！");
                            BL = false;
                            break;
                    }
                    if (BL)
                    {
                        int run = 0;
                        Console.Write("请输入控制卡宽度：");
                        Program.width = int.Parse(Console.ReadLine());
                        Console.Write("请输入控制卡高度：");
                        Program.height = int.Parse(Console.ReadLine());
                        Console.Write("请输入分区宽度：");
                        Program.areaw = int.Parse(Console.ReadLine());
                        Console.Write("请输入分区高度：");
                        Program.areah = int.Parse(Console.ReadLine());
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
                                        Console.Write("请选择发送内容（0.图片，1.时间，2.表盘，3.视频，4.字幕，5.农历，6.炫彩字，7.计时，8.富文本，10.只发送节目不播放（字幕区），11.发送默认节目（字幕区）），12.关闭默认节目");
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
                                            //只上传节目不播放
                                            case 10:
                                                Send_ProgramStr.OnOnlySendProgram();
                                                break;
                                            //上传离线节目
                                            case 11:
                                                Send_ProgramStr.OnSendOnlineProgram();
                                                break;
                                            //关闭离线节目
                                            case 12:
                                                Send_ProgramStr.OnCloseOnlineProgram();
                                                break;
                                            default:
                                                Console.WriteLine("类型错误！！！");
                                                BL = false;
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
                                                BL = false;
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
                                        Console.WriteLine("24.锁定节目");
                                        Console.WriteLine("25.解锁节目");
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
                                            case 24:
                                                Console.WriteLine("请输入要锁定的节目名称");
                                        
                                                string programName  = Console.ReadLine();
                                                OtherFunctions.OnUnOrLockProgram(1, programName);
                                                break;
                                            case 25:
                                                Console.WriteLine("请输入要解锁的节目名称");

                                                string unprogramName = Console.ReadLine();
                                                OtherFunctions.OnUnOrLockProgram(0, unprogramName);
                                                break;
                                            default:
                                                Console.WriteLine("类型错误！！！");
                                                BL = false;
                                                break;
                                        }
                                        #endregion
                                        break;
                                    default:
                                        Console.WriteLine("类型错误！！！");
                                        BL = false;
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
            //LedYNetSdk.save_dynamic_forid(ip, port, str, str,"0");
            ////StartServer(); ;
            //OnStartSSLServer();

            //        byte[] ip1 = Encoding.GetEncoding("GBK").GetBytes("192.168.89.188");
            //        byte[] subnetMask = Encoding.GetEncoding("GBK").GetBytes("255.255.255.0");
            //        byte[] gateway = Encoding.GetEncoding("GBK").GetBytes("192.168.89.1");
            //        int min_waitTime = 0;
            //        int max_waitTime = 0;
            //        //err = LedYNetSdk.set_screen_ip("C0Y2L02007310046", "501130303447363002D4DFD5C5A48977", ip1, subnetMask, gateway, gateway, ref min_waitTime, ref max_waitTime);

            //清除显示屏所有节目
            LedYNetSdk.clear_all_program(ip, (ushort)port, str, str);
            //清除显示所有动态区
            LedYNetSdk.clear_dynamic(ip, (ushort)port, str, str);
            //LedYNetSdk.download_file(ip, (ushort)port, str, str, "", "");


            LedYNetSdk.release_sdk();
            //Console.ReadKey();
        }

        public struct DefualtProgramInfo
        {
            public IntPtr playmode;
            public IntPtr defaultprogramlist;
            public IntPtr playtime;
            public IntPtr intervaltime;
        }

        public static IntPtr OnGetPtr(string str)
        {
            byte[] bts = System.Text.Encoding.Unicode.GetBytes(str);
            int a = bts.Count();
            //申请非拖管空间   
            IntPtr m_ptr = Marshal.AllocHGlobal(bts.Length + (str.Length == 0 ? 2 : a / str.Length));
            //给非拖管空间清0 
            Byte[] btZero = new Byte[bts.Length + (str.Length == 0 ? 2 : a / str.Length)]; //一定要加1,否则后面是乱码   
            Marshal.Copy(btZero, 0, m_ptr, btZero.Length);
            //给指针指向的空间赋值   
            Marshal.Copy(bts, 0, m_ptr, bts.Length);
            return m_ptr;
        }
    }
}
