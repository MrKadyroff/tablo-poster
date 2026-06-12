using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;

namespace BX_Y_CSharp_SDK
{
    class OtherFunctions
    {
        //音频
        public static void music()
        {
            IntPtr area = LedYNetSdk.create_manage_audio();
            LedYNetSdk.add_manage_audio(area, "1.mp3", "D:\\CloudMusic\\1.mp3");
            //上传
            int n = LedYNetSdk.upload_audio_file(Program.ip, Program.port, Program.str, Program.str, area);
            //播放
            n = LedYNetSdk.play_audio(Program.ip, Program.port, Program.str, Program.str, area, 0);
        }
        //下载播放文件
        public static void download_file()
        {
            byte[] program_list = new byte[128];
            byte[] program_name = new byte[128];
            //获取控制卡文件信息
            int err = LedYNetSdk.get_screen_player_file(Program.ip, Program.port, Program.str, Program.str, program_list, program_name);
            string ss = System.Text.Encoding.Unicode.GetString(program_list).Split('\0')[0];
            string sss = System.Text.Encoding.Unicode.GetString(program_name).Split('\0')[0];
            //保存到本地
            err = LedYNetSdk.download_file(Program.ip, Program.port, Program.str, Program.str, sss, "1.xml");
        }

        //查询固件版本    Query firmware version
        public static void get_firmware()
        {
            byte[] firmwareversion = new byte[64];
            byte[] app_version = new byte[64];
            byte[] fpga_version = new byte[60];
            int err = LedYNetSdk.get_firmware_version(Program.ip, Program.port, Program.str, Program.str, firmwareversion, app_version, fpga_version);
            if (err == 0)
            {
                Console.WriteLine("firmwareversion:" + Encoding.Default.GetString(firmwareversion));
                Console.WriteLine("app_version:" + Encoding.Default.GetString(app_version));
                Console.WriteLine("fpga_version:" + Encoding.Default.GetString(fpga_version));
            }
            else
            {
                Console.WriteLine(Errer.GetError(err));
            }
        }
        //校时命令  Timing Order
        public static void check_time()
        {
            int err = LedYNetSdk.check_time(Program.ip, Program.port, Program.str, Program.str);
            if (err == 0)
            {
                Console.WriteLine("校时成功");
            }
            else
            {
                Console.WriteLine(Errer.GetError(err));
            }
        }
        //取得屏幕参数    Get screen parameters
        public static void get_screen_parameters()
        {
            byte[] Data = new byte[1024 * 10];
            for (int n = 0; n < 1024; n++) { Data[n] = 0; }
            int err = LedYNetSdk.get_screen_parameters(Program.ip, Program.port, Program.str, Program.str, Data);
            IntPtr dec = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(LedYNetSdk.ControllerInfo)));
            Marshal.Copy(Data, Marshal.SizeOf(typeof(LedYNetSdk.ControllerInfo)) * 0, dec, Marshal.SizeOf(typeof(LedYNetSdk.ControllerInfo)));
            LedYNetSdk.ControllerInfo bc = (LedYNetSdk.ControllerInfo)Marshal.PtrToStructure(dec, typeof(LedYNetSdk.ControllerInfo));
            Marshal.FreeHGlobal(dec);
            string barcode = System.Text.Encoding.Unicode.GetString(bc.barcode).Split('\0')[0].Replace("&#x0;", "").Replace("쳌", "");
            Console.WriteLine("IP：" + Encoding.Unicode.GetString(bc.source_ip).Split('\0')[0].Replace("&#x0;", "").Replace("쳌", ""));
            Console.WriteLine("barcode：" + barcode);
            Console.WriteLine("port：" + bc.port);
            Console.WriteLine("width：" + bc.screen_w);
            Console.WriteLine("height：" + bc.screen_h);
            Console.WriteLine("screen_type：" + bc.screen_type);
            Console.WriteLine("screen_brigtness：" + bc.screen_brigtness);
        }
        //取得屏幕状态    Get screen status
        public static void get_screen_status()
        {
            int screen_onoff = 0;
            int brigtness = 0;
            int brigtness_mode = 0;
            int volume = 0;
            int screen_lockunlock = 0;
            int program_lockunlock = 0;
            int screen_output_type = 0;
            int screen_player_mode = 0;
            byte[] screen_time = new byte[1024];
            byte[] screen_addr = new byte[1024];
            byte[] screen_customer_onoff = new byte[1024];
            byte[] screen_language = new byte[1024]; byte[] screen_gps = new byte[1024];
            int err = LedYNetSdk.get_screen_status(Program.ip, Program.port, Program.str, Program.str, ref screen_onoff, ref brigtness, ref brigtness_mode, ref volume, ref screen_lockunlock, ref program_lockunlock, ref screen_output_type, ref screen_player_mode, screen_time, screen_addr, screen_customer_onoff, screen_language, screen_gps);
            if (err == 0)
            {
                if (screen_onoff == 0) { Console.WriteLine("开机"); } else { Console.WriteLine("关机"); }
                Console.WriteLine("亮度" + brigtness);
                if (brigtness_mode == 0) { Console.WriteLine("自动调亮"); } else { Console.WriteLine("手动调亮"); }
                Console.WriteLine("声音" + volume);
                if (screen_lockunlock == 1) { Console.WriteLine("屏幕锁定"); } else { Console.WriteLine("屏幕正常"); }
                if (program_lockunlock == 1) { Console.WriteLine("节目锁定"); } else { Console.WriteLine("节目正常"); }
                Console.WriteLine("时间" + System.Text.Encoding.Default.GetString(screen_time));
                Console.WriteLine("地址" + System.Text.Encoding.Default.GetString(screen_addr));
                Console.WriteLine("开关机" + System.Text.Encoding.Default.GetString(screen_customer_onoff));
            }
            else { Console.WriteLine(Errer.GetError(err)); }
        }
        //截取屏幕 
        public static void get_capture()
        {
            IntPtr dwhandPtr = LedYNetSdk.net_login(Program.ip, Program.port, Program.str, Program.str);
            
            //先截屏，再下载
            int minWaitTime = 0;
            int maxWaitTime = 0;
            byte[] screenAddr = new byte[128];
            var fielrname = "abc.bmp";
            int intResult = LedYNetSdk.get_screen_capture_dwhand1(dwhandPtr, fielrname, Program.width, Program.height, ref minWaitTime, ref maxWaitTime, screenAddr);

            string srcPath = System.Text.Encoding.Unicode.GetString(screenAddr).Split('\0')[0];
            var movename = "share/abc.bmp";

            DateTime maxtime_tick = DateTime.Now.AddSeconds(maxWaitTime);
            //if (minWaitTime > 0)
            //{
            //    //CommShowNoticeMessage(screensendinfo.ScreenInfo.Obj.unique_identifier, screensendinfo.ScreenInfo.Obj.ip, string.Format(GobalData.allcatalog.GetString("Restarting controller firmware (at least {0} seconds) ......"), minWaitTime), 30, null);
            //    Thread.Sleep(minWaitTime * 1000);
            //}
            //intResult = -1;
            ////如果最后失败，判断是否还在最大等待时间，以上是理想状态；有些电脑是有可能网络都还没恢复好，包根本就发不出去的，所以就不会有超时等待时间；所以再判断是否还在最大等待时间范围内；如果还在，则继续使用时间等待重发
            //while (maxtime_tick > DateTime.Now)
            //{
            //    Thread.Sleep(500);
            //    intResult = LedYNetSdk.download_file_dwhand(dwhandPtr, movename, fielrname);
            //    if (intResult == 0)
            //    {
            //        break;
            //    }
            //}
            while (maxtime_tick > DateTime.Now)
            {
                intResult = LedYNetSdk.copy_file_dwhand1(dwhandPtr, srcPath, movename);
                if (intResult == 0)
                {
                    intResult = LedYNetSdk.download_file_dwhand(dwhandPtr, movename, fielrname);
                    if (intResult == 0)
                    {
                        Console.WriteLine("截取成功！");
                    }
                    break;
                }
            }

            LedYNetSdk.net_logout(dwhandPtr);
            intResult = LedYNetSdk.delete_file(Program.ip, Program.port, Program.str, Program.str, movename);
        }

        //锁定节目  Lock-in Program
        public static void lock_program(int nlock, string program_name)
        {
            int err = LedYNetSdk.lock_program(Program.ip, Program.port, Program.str, Program.str, nlock, program_name);
            if (err == 0)
            {
                if (nlock == 1)
                {
                    Console.WriteLine("锁定节目设置成功！");
                }
                else
                {
                    Console.WriteLine("解锁节目设置成功！");
                }
            }
            else
            {
                Console.WriteLine(Errer.GetError(err));
            }
        }
        //锁定屏幕  Lock screen
        public static void lock_screen(int nlock)
        {
            int err = LedYNetSdk.lock_screen(Program.ip, Program.port, Program.str, Program.str, nlock);
            if (err == 0)
            {
                if (nlock == 1)
                {
                    Console.WriteLine("锁定屏幕设置成功！");
                }
                else
                {
                    Console.WriteLine("解锁屏幕设置成功！");
                }
            }
            else
            {
                Console.WriteLine(Errer.GetError(err));
            }
        }
        //格式化   reboot
        public static void reboot()
        {
            //int err = LedYNetSdk.reboot(Program.ip, Program.port, Program.str, Program.str);
        }
        //设置系统⾳量  Setting up System Quantity  
        public static void set_screen_volumn(int volumn)
        {
            int err = LedYNetSdk.set_screen_volumn(Program.ip, Program.port, Program.str, Program.str, volumn);
            if (err == 0)
            {
                Console.WriteLine("系统⾳量设置成功！");
            }
            else
            {
                Console.WriteLine(Errer.GetError(err));
            }
        }
        //手动调亮  Manual brightening
        public static void set_screen_brightness(int brightness)
        {
            int err = LedYNetSdk.set_screen_brightness(Program.ip, Program.port, Program.str, Program.str, brightness);
            if (err == 0)
            {
                Console.WriteLine("手动调亮设置成功！");
            }
            else
            {
                Console.WriteLine(Errer.GetError(err));
            }
        }
        //定时调亮  Timely brightening
        public static void set_screen_cus_brightness()
        {
            //半小时一个亮度值，00:00:00-00:29:59  00:30:00-00:59:59 ... 23:30:00-23:59:59   一天48个值,亮度值范围1-255
            ushort[] brightness = new ushort[48];
            for (int i = 0; i < 48; i++) { brightness[i] = 255; }
            int err = LedYNetSdk.set_screen_cus_brightness(Program.ip, Program.port, Program.str, Program.str, brightness, 48);
            if (err == 0)
            {
                Console.WriteLine("定时调亮设置成功！");
            }
            else
            {
                Console.WriteLine(Errer.GetError(err));
            }
        }
        //自动调亮   Auto dimming
        public static void set_screen_auto_brightness()
        {
            ushort[] brightness = new ushort[16];
            brightness[0] = 1;
            brightness[1] = 26;
            brightness[2] = 42;
            brightness[3] = 51;
            brightness[4] = 68;
            brightness[5] = 85;
            brightness[6] = 102;
            brightness[7] = 119;
            brightness[8] = 136;
            brightness[9] = 153;
            brightness[10] = 170;
            brightness[11] = 187;
            brightness[12] = 204;
            brightness[13] = 221;
            brightness[14] = 238;
            brightness[15] = 255;
            int data_count = 16;
            ushort[] sensor_brightness = new ushort[16];
            sensor_brightness[0] = 1;
            sensor_brightness[1] = 4369;
            sensor_brightness[2] = 8738;
            sensor_brightness[3] = 13107;
            sensor_brightness[4] = 17476;
            sensor_brightness[5] = 21845;
            sensor_brightness[6] = 26214;
            sensor_brightness[7] = 30583;
            sensor_brightness[8] = 34952;
            sensor_brightness[9] = 39321;
            sensor_brightness[10] = 43690;
            sensor_brightness[11] = 48059;
            sensor_brightness[12] = 52428;
            sensor_brightness[13] = 56797;
            sensor_brightness[14] = 61166;
            sensor_brightness[15] = 65535;
            int sensor_data_count = 16;
            string sensor_addr = "0x823";
            int err = LedYNetSdk.set_screen_auto_brightness(Program.ip, Program.port, Program.str, Program.str, brightness, data_count, sensor_brightness, sensor_data_count, sensor_addr);
        }
        //手动设置开关机   Manual setting of switch
        public static void set_screen_turnonoff(int turnonoff)
        {
            int err = LedYNetSdk.set_screen_turnonoff(Program.ip, Program.port, Program.str, Program.str, turnonoff);
            if (err == 0)
            {
                Console.WriteLine("手动开关机设置成功！");
            }
            else
            {
                Console.WriteLine(Errer.GetError(err));
            }
        }
        //定时开关机 Timing switch
        public static void set_screen_cus_turnonoff()
        {
            ushort[] brightness = new ushort[48];
            for (int i = 0; i < 48; i++) { brightness[i] = 255; }
            IntPtr trunonoff = LedYNetSdk.create_turnonoff();
            LedYNetSdk.add_turnonoff(trunonoff, 1, "14:42:00");
            LedYNetSdk.add_turnonoff(trunonoff, 0, "14:43:00");
            LedYNetSdk.add_turnonoff(trunonoff, 1, "14:44:00");
            LedYNetSdk.add_turnonoff(trunonoff, 0, "14:45:00");
            int err = LedYNetSdk.set_screen_cus_turnonoff(Program.ip, Program.port, Program.str, Program.str, trunonoff);
            if (err == 0)
            {
                Console.WriteLine("定时开关机设置成功！");
            }
            else
            {
                Console.WriteLine(Errer.GetError(err));
            }
            LedYNetSdk.delete_turnonoff(trunonoff);
        }
        //取消定时开关机,保留取消时屏幕状态     Cancel the timer switch and keep the screen status when canceling
        public static void cancel_screen_cus_turnonoff()
        {
            int err = LedYNetSdk.cancel_screen_cus_turnonoff(Program.ip, Program.port, Program.str, Program.str);
            if (err == 0)
            {
                Console.WriteLine("取消定时开关机设置成功！");
            }
            else
            {
                Console.WriteLine(Errer.GetError(err));
            }
        }

        //添加字库 Timing switch
        public static void set_font()
        {
            IntPtr area = LedYNetSdk.create_font();
            LedYNetSdk.add_font(area, IntPtr.Zero, "digital-7.ttf", "Digital-7.ttf", IntPtr.Zero);
            int min = 0, max = 0;
            int err = LedYNetSdk.install_font(Program.ip, Program.port, Program.str, Program.str, area, IntPtr.Zero, ref min, ref max);
            if (err == 0)
            {
                Console.WriteLine("添加字库成功！");
            }
            else
            {
                Console.WriteLine(Errer.GetError(err));
            }
            LedYNetSdk.delete_add_font(area, "Digital-7.ttf");
            LedYNetSdk.delete_create_font(area);
        }
        //查询字库 Timing switch
        public static void get_font()
        {
            try
            {
                byte[] system_font = new byte[1024];
                for (int i = 0; i < 1024; i++) { system_font[i] = 0; }
                byte[] custom_font = new byte[1024];
                for (int i = 0; i < 1024; i++) { custom_font[i] = 0; }
                int err = LedYNetSdk.query_font(Program.ip, Program.port, Program.str, Program.str, system_font, custom_font);
                string sr = System.Text.Encoding.Unicode.GetString(system_font);
                string[] systemFonts = System.Text.Encoding.Unicode.GetString(system_font).Split('\0')[0].Split(new char[] { '[' }, StringSplitOptions.RemoveEmptyEntries);
                //string[] customFonts = System.Text.Encoding.Unicode.GetString(custom_font).Split('\0')[0].Split(new char[] { '[' }, StringSplitOptions.RemoveEmptyEntries);
                string fonts = "";
                fonts += "system:[";
                for (int i = 1; i < systemFonts.Length; i++)
                {
                    string[] fontNames = systemFonts[i].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    string[] family = fontNames[0].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    if (family[0].Equals("family", StringComparison.CurrentCultureIgnoreCase))
                    {
                        fonts += family[1] + ";";
                    }
                }
                //fonts += "]custom:[";
                //for (int i = 1; i < customFonts.Length; i++)
                //{
                //    string[] fontNames = customFonts[i].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                //    string[] family = fontNames[0].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                //    if (family[0].Equals("family", StringComparison.CurrentCultureIgnoreCase))
                //    {
                //        fonts += family[1] + ";";
                //    }
                //}
                fonts += "]";
                if (err == 0)
                {
                    Console.WriteLine("查询字库成功！");
                }
                else
                {
                    Console.WriteLine(Errer.GetError(err));
                }
            }catch(Exception){}
        }
        //删除字库 Timing switch
        public static void del_font()
        {
            IntPtr area = LedYNetSdk.create_font();
            LedYNetSdk.add_font(area, IntPtr.Zero, "digital-7.ttf", "Digital-7.ttf", IntPtr.Zero);
            int err = LedYNetSdk.delete_font(Program.ip, Program.port, Program.str, Program.str, area);
            if (err == 0)
            {
                Console.WriteLine("删除字库成功！");
            }
            else
            {
                Console.WriteLine(Errer.GetError(err));
            }
            LedYNetSdk.delete_add_font(area, "Digital-7.ttf");
            LedYNetSdk.delete_create_font(area);
        }
        //设置系统语言
        public static void set_screen_language()
        {
            /*
             * zh_CN 简体中文 
             * zh_TW 繁体中文
             * en_US 英文 
             * ru_RU 俄文
             * vi_VN 越南文
             * */
            int err = LedYNetSdk.set_screen_language(Program.ip, Program.port, Program.str, Program.str, "zh_CN");
            if (err == 0)
            {
                Console.WriteLine("设置系统语言成功！");
            }
            else
            {
                Console.WriteLine(Errer.GetError(err));
            }
        }
        //设置控制卡IP
        public static void set_screen_ip()
        {
            string barcode = "C0Y1A02103130189";
            string pid = "";
            byte[] ip = Encoding.ASCII.GetBytes("192.168.89.162");
            byte[] submark = Encoding.ASCII.GetBytes("255.255.255.0");
            byte[] gateway = Encoding.ASCII.GetBytes("192.168.89.1");
            byte[] dns_server = Encoding.ASCII.GetBytes("192.168.89.1");
            int min_waitTime = 0, max_waitTime = 0;
            int err = LedYNetSdk.set_screen_ip(barcode, pid, ip, submark, gateway, dns_server, ref min_waitTime, ref max_waitTime);
            if (err == 0)
            {
                Thread.Sleep(max_waitTime * 1000);
                Console.WriteLine("设置系统语言成功！");
            }
            else
            {
                Console.WriteLine(Errer.GetError(err));
            }
        }
        //搜索同网段局域网控制卡
        public static void SearchCards()
        {
            IntPtr UDPSearchSecondData = Marshal.AllocHGlobal(1024 * 38);
            int data_count = 0;

            int err = LedYNetSdk.search_card(UDPSearchSecondData, ref data_count);
            byte[] datas = new byte[1024 * 38];
            Marshal.Copy(UDPSearchSecondData, datas, 0, 1024 * 38);
            for (int i = 0; i < data_count; i++)
            {
                IntPtr dec = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(LedYNetSdk.BroadCast2)));
                Marshal.Copy(datas, Marshal.SizeOf(typeof(LedYNetSdk.BroadCast2)) * i, dec, Marshal.SizeOf(typeof(LedYNetSdk.BroadCast2)));
                LedYNetSdk.BroadCast2 bc = (LedYNetSdk.BroadCast2)Marshal.PtrToStructure(dec, typeof(LedYNetSdk.BroadCast2));
                Marshal.FreeHGlobal(dec);
                string pid = System.Text.Encoding.Unicode.GetString(bc.pid).Split('\0')[0].Replace("&#x0;", "").Replace("쳌", "");
                string barcode = System.Text.Encoding.Unicode.GetString(bc.barcode).Split('\0')[0].Replace("&#x0;", "").Replace("쳌", "");
                if (string.IsNullOrEmpty(pid) || bc.port < 0 || bc.port > 65535 || bc.screen_type < 0 || bc.screen_type > 65535 || pid.Length != 32)
                {
                    continue;
                }
                Console.WriteLine("IP：" + Encoding.Unicode.GetString(bc.source_ip).Split('\0')[0].Replace("&#x0;", "").Replace("쳌", ""));
                Console.WriteLine("barcode：" + barcode);
                Console.WriteLine("port：" + bc.port);
                Console.WriteLine("width：" + bc.screen_w);
                Console.WriteLine("height：" + bc.screen_h);
                Console.WriteLine("screen_type：" + bc.screen_type);
            }
        }
        //设置控制卡mac地址
        public static void set_screen_mac()
        {
            string barcode = "C0Y1A02103130189";
            string pid = "";
            string mac = "";
            byte[] ip = Encoding.ASCII.GetBytes("192.168.89.162");
            byte[] submark = Encoding.ASCII.GetBytes("255.255.255.0");
            byte[] gateway = Encoding.ASCII.GetBytes("192.168.89.1");
            byte[] dns_server = Encoding.ASCII.GetBytes("192.168.89.1");
            int min_waitTime = 0, max_waitTime = 0;
            int err = LedYNetSdk.set_screen_mac(barcode, pid, mac, ref min_waitTime, ref max_waitTime);
            if (err == 0)
            {
                Thread.Sleep(max_waitTime * 1000);
                Console.WriteLine("设置控制卡mac地址成功！");
            }
            else
            {
                Console.WriteLine(Errer.GetError(err));
            }
        }
        //设置logo
        public static void set_screen_logo()
        {
            string file = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "bgcolor.png";
            int err = LedYNetSdk.set_screen_logo(Program.ip, Program.port, Program.str, Program.str, 1, "center", 64, 32, file);
            if (err == 0)
            {
                Console.WriteLine("设置控制卡logo成功！");
            }
            else
            {
                Console.WriteLine(Errer.GetError(err));
            }
        }
        //查询传感器 
        public static void get_sensor()
        {
            try
            {
                byte[] system_font = new byte[1024];
                for (int i = 0; i < 1024; i++) { system_font[i] = 0; }
                byte[] custom_font = new byte[1024];
                for (int i = 0; i < 1024; i++) { custom_font[i] = 0; }
                byte[] sensor_bus = new byte[1024];
                int err = LedYNetSdk.get_sensor_bus(Program.ip, Program.port, Program.str, Program.str, sensor_bus);
                string sr = System.Text.Encoding.Unicode.GetString(sensor_bus);
                string sensorbus = System.Text.Encoding.Unicode.GetString(sensor_bus).Split('\0')[0];
                byte[] datas = new byte[1024*10]; int datas_count = 0;
                 int min_waitTime = 0; int max_waitTime = 0;
                 err = LedYNetSdk.query_seeksensor(Program.ip, Program.port, Program.str, Program.str, sensorbus, ref min_waitTime, ref max_waitTime);
                 Thread.Sleep(min_waitTime*1000);
                err = LedYNetSdk.get_sensor(Program.ip, Program.port, Program.str, Program.str, datas, ref datas_count);
                string [] srr=System.Text.Encoding.Unicode.GetString(datas).Split('\0');
                string[] systemFonts = System.Text.Encoding.Unicode.GetString(datas).Split('\0')[0].Split(new char[] { '[' }, StringSplitOptions.RemoveEmptyEntries);
                //string[] customFonts = System.Text.Encoding.Unicode.GetString(custom_font).Split('\0')[0].Split(new char[] { '[' }, StringSplitOptions.RemoveEmptyEntries);
                List<LedYNetSdk.ControllerSensor> listt = new List<LedYNetSdk.ControllerSensor>();
                for (int i = 0; i < datas_count; i++)
                {
                    IntPtr dec = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(LedYNetSdk.ControllerSensor)));
                    Marshal.Copy(datas, Marshal.SizeOf(typeof(LedYNetSdk.ControllerSensor)) * i, dec, Marshal.SizeOf(typeof(LedYNetSdk.ControllerSensor)));
                    LedYNetSdk.ControllerSensor bc = (LedYNetSdk.ControllerSensor)Marshal.PtrToStructure(dec, typeof(LedYNetSdk.ControllerSensor));
                    listt.Add(bc);
                    Marshal.FreeHGlobal(dec);
                    Console.WriteLine("sensor_value：" + bc.sensor_value);
                    Console.WriteLine("sensor_sequence：" + Encoding.Unicode.GetString(bc.sensor_sequence).Split('\0')[0].Replace("&#x0;", "").Replace("쳌", ""));
                    Console.WriteLine("sensor_state：" + Encoding.Unicode.GetString(bc.sensor_state).Split('\0')[0].Replace("&#x0;", "").Replace("쳌", ""));
                    Console.WriteLine("sensor_address：" + Encoding.Unicode.GetString(bc.sensor_address).Split('\0')[0].Replace("&#x0;", "").Replace("쳌", ""));
                }
                if (err == 0)
                {
                    Console.WriteLine("查询传感器成功！");
                }
                else
                {
                    Console.WriteLine(Errer.GetError(err));
                }
            }
            catch (Exception) { }
        }
        //清除节目显示
        public static void clearprogram() 
        {
            int err = LedYNetSdk.clear_all_program(Program.ip, Program.port, Program.str, Program.str);
        }
        //清除动态区显示
        public static void cleardynamic()
        {
            int err = LedYNetSdk.clear_dynamic(Program.ip, Program.port, Program.str, Program.str);
        }
    }
}

