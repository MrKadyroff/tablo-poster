using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace BX_Y_CSharp_SDK
{
    class SSL
    {
        private static TLSHelper tlsHelper = new TLSHelper();
        /// <summary>
        /// Author：Judy
        /// 启动服务器模式（加密模式）
        /// </summary>
        static void OnStartSSLServer()
        {
            try
            {
                //服务器IP
                byte[] ip = Encoding.ASCII.GetBytes("192.168.89.100");
                //服务器端口
                ushort port = 8136;
                int err = 0;
                //端口，启动服务器(加密)
                DateTime beginTime, endTime;
                //加密通讯证书文件
                string commCertFile = @"F:\rsa\fileVerify.pfx";
                string certFilePath = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + @"\acccert.pem";
                tlsHelper.ExportToCerFile(commCertFile, certFilePath, "111111", out beginTime, out endTime);
                string path = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + @"\acccert.key";
                tlsHelper.ExportPrivateKey(commCertFile, path, "111111");

                IntPtr er = LedYNetSdk.Start_Ssl_Server(port, certFilePath, path);
                List<LedYNetSdk.BroadCast2> BroadCastlist = new List<LedYNetSdk.BroadCast2>();
                byte[] datas = new byte[1024 * 64];
                int data_count = 0;
                //等待控制卡上线
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
                int port1 = LedYNetSdk.Get_Port_Pid(pid1); ;
                OnCheckTime(ip, (ushort)port1, "guest");
                ////关闭服务器
                LedYNetSdk.Stop_Server(er);

            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// 校时（ssl）
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="str"></param>
        static void OnCheckTime(byte[] ip, ushort port, string str)
        {
            //LedYNetSdk.set_ssl_flag(1, System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + @"\acccert.pem");
            int err = LedYNetSdk.check_time(ip, (ushort)port, str, str);
            Console.WriteLine("OnCheckTime:" + err);
        }

        /// <summary>
        /// Author：Judy
        /// 素材认证的发送节目流程
        /// </summary>
        /// <param name="ip">通讯控制器IP</param>
        /// <param name="port">端口</param>
        /// <param name="str">用户名和密码</param>
        /// <param name="playlist">播放列表句柄</param>
        /// <param name="privateKey">私钥文件</param>
        static void OnSendProgram(byte[] ip, ushort port, string str, IntPtr playlist, string privateKey)
        {
            //保存节目临时目录，根据自己电脑而定
            var szLocalTempDir = @"F:\Temp\";
            //签名认证
            //素材认证，节目和playlist都要认证
            LedYNetSdk.make_program(playlist, szLocalTempDir);
            string playlistFile = szLocalTempDir + "lists\\";
            string programlistFile = szLocalTempDir + "programs\\";
            DirectoryInfo di = new DirectoryInfo(programlistFile);
            DirectoryInfo[] programlist = di.GetDirectories();
            foreach (DirectoryInfo pDI in programlist)
            {
                FileInfo[] pfis = pDI.GetFiles("*.xml");
                foreach (FileInfo fi in pfis)
                {
                    long size = 0;
                    string vutMD5 = tlsHelper.OnGetMd5(fi.FullName, out size);
                    //组织签名文件内容                                  
                    int offset = 0; int len = 0; string signData = "";
                    bool checkAll = false;
                    if (size < 1 * 1024)
                    {
                        checkAll = true;
                    }
                    int verify = tlsHelper.OnVerifyFileData(fi.FullName, @"F:\rsa\fileVerify.pfx", "111111", out len, out offset, out signData, checkAll);
                    if (verify == 0)
                    {
                        string[] strs = new string[1];
                        strs[0] = privateKey + ",sha1," + @"programs/" + (vutMD5 + Path.GetExtension(fi.FullName)) + "," + offset + "," + len + "," + signData;
                        tlsHelper.OnWriteFile(System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "abc.txt", strs);
                    }
                    LedYNetSdk.TLSInfo tInfo = tlsHelper.OnCreateTLS(offset, len, privateKey, signData);
                    int rowsize = Marshal.SizeOf(typeof(LedYNetSdk.TLSInfo));
                    IntPtr rowptr = Marshal.AllocHGlobal(rowsize);
                    Marshal.StructureToPtr(tInfo, rowptr, false);
                    LedYNetSdk.add_tls_md5(playlist, vutMD5, (IntPtr)rowptr);
                }
            }
            di = new DirectoryInfo(playlistFile);
            FileInfo[] fis = di.GetFiles("*.xml");
            foreach (FileInfo fi in fis)
            {
                long size = 0;
                string vutMD5 = tlsHelper.OnGetMd5(fi.FullName, out size);
                //组织签名文件内容                                    
                int offset = 0; int len = 0; string signData = "";
                bool checkAll = false;
                if (size < 1 * 1024)
                {
                    checkAll = true;
                }
                int verify = tlsHelper.OnVerifyFileData(fi.FullName, @"F:\rsa\fileVerify.pfx", "111111", out len, out offset, out signData, checkAll);
                if (verify == 0)
                {
                    string[] strs = new string[1];
                    strs[0] = privateKey + ",sha1," + @"lists/" + (vutMD5 + Path.GetExtension(fi.FullName)) + "," + offset + "," + len + "," + signData;
                    tlsHelper.OnWriteFile(System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "abc.txt", strs);
                }
                LedYNetSdk.TLSInfo tInfo = tlsHelper.OnCreateTLS(offset, len, privateKey, signData);
                int rowsize = Marshal.SizeOf(typeof(LedYNetSdk.TLSInfo));
                IntPtr rowptr = Marshal.AllocHGlobal(rowsize);
                Marshal.StructureToPtr(tInfo, rowptr, false);
                LedYNetSdk.add_tls_md5(playlist, vutMD5, (IntPtr)rowptr);
            }

            int send_style = 0;
            long free_size = 0; long total_size = 0;
            //发送节目
            int err = LedYNetSdk.send_program(ip, port, str, str, szLocalTempDir, playlist, send_style, ref free_size, ref total_size, 1);
            Console.WriteLine("send_program:" + err);
            //释放句柄操作
            LedYNetSdk.cancel_send_program(playlist);
            LedYNetSdk.delete_playlist(playlist);
        }


        /// <summary>
        /// Author：Judy
        /// 创建文本信息（单行字幕）
        /// 单行文本信息
        /// <param name="ip">通讯控制器IP</param>
        /// <param name="port">通讯端口</param>
        /// <param name="str">用户名和密码（默认用户名和密码一致）</param>
        /// </summary>
        static void OnCreateText2TextUnit(byte[] ip, ushort port, string str)
        {
            try
            {
                int err = 0;
                //创建播放列表句柄
                IntPtr playlist = LedYNetSdk.create_playlist(Program.width, Program.height, Program.Type);
                string name = "program_0";
                //创建节目句柄
                IntPtr program = LedYNetSdk.create_program(name, "0xff000000");
                //创建字幕分区句柄
                IntPtr area_tree = LedYNetSdk.create_text();
                string file = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "红.bmp";

                //签名认证
                long size = 0;
                //获取文件的MD5
                string vutMD5 = tlsHelper.OnGetMd5(file, out size);
                //从pfx中提取出的证书文件
                string certFilePath = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + @"\keykey.pem";
                //使用工具生成的pfx文件
                string privateKey = tlsHelper.OnExport(@"F:\rsa\fileVerify.pfx", "111111", certFilePath);
                //组织签名文件内容                                    
                int offset = 0; int len = 0; string signData = "";
                bool checkAll = false;
                if (size < 1 * 1024)
                {
                    checkAll = true;
                }
                int verify = tlsHelper.OnVerifyFileData(file, @"F:\rsa\fileVerify.pfx", "111111", out len, out offset, out signData, checkAll);
                if (verify == 0)
                {
                    string[] strs = new string[1];
                    strs[0] = privateKey + ",sha1," + "share/" + (vutMD5 + Path.GetExtension(file)) + "," + offset + "," + len + "," + signData;
                    tlsHelper.OnWriteFile(System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "abc.txt", strs);
                }
                LedYNetSdk.TLSInfo tInfo = tlsHelper.OnCreateTLS(offset, len, privateKey, signData);
                int rowsize = Marshal.SizeOf(typeof(LedYNetSdk.TLSInfo));
                IntPtr rowptr = Marshal.AllocHGlobal(rowsize);
                Marshal.StructureToPtr(tInfo, rowptr, false);
                //添加素材认证句柄
                LedYNetSdk.add_tls_md5(playlist, vutMD5, (IntPtr)rowptr);

                //将文字信息添加到分区中（可以添加多个）
                err = LedYNetSdk.add_text_unit_text(area_tree, 5, 5, "SimSun", 12, "normal", "0", "0xffff0000", "0xff000000", "显45示数据123456");
                //Console.WriteLine("add_text_unit_text:" + err);
                //将字幕分区添加到节目中
                err = LedYNetSdk.add_text(program, area_tree, 0, 0, 64, 32, 100, 50, 1);
                Console.WriteLine("add_text:" + err);

                string m_aging_start_time = "";
                string m_aging_stop_time = "";
                string m_period_ontime = "";
                string m_period_offtime = "";
                //将节目添加到播放列表中
                err = LedYNetSdk.add_program_in_playlist(playlist, program, 1, 10, m_aging_start_time, m_aging_stop_time, m_period_ontime, m_period_offtime, 127);
                Console.WriteLine("add_program_in_playlist:" + err);

                OnSendProgram(ip, port, str, playlist, privateKey);
            }
            catch (Exception)
            {
                throw;
            }
        }


        /// <summary>
        /// Author：Judy
        /// 发送字幕（自己生成图片）
        /// 自行将文字生成图片
        /// add_text_unit_img接口中倒数第二个参数为最后一张图片需要移动的宽度（比如一串文字生成了两张图片，第二张的图片没有达到分区大小时，请指定这个参数）
        /// <param name="ip">通讯控制器IP</param>
        /// <param name="port">通讯端口</param>
        /// <param name="str">用户名和密码（默认用户名和密码一致）</param>
        /// </summary>
        static void OnCreateText2ImgUnit(byte[] ip, ushort port, string str)
        {
            try
            {
                int err = 0;
                //创建播放列表句柄
                IntPtr playlist = LedYNetSdk.create_playlist(Program.width, Program.height, Program.Type);
                string name = "program_0";
                //创建节目句柄
                IntPtr program = LedYNetSdk.create_program(name, "0xff000000");
                //创建字幕分区句柄
                IntPtr area_tree = LedYNetSdk.create_text();
                string file = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "红.bmp";

                //签名认证
                long size = 0;
                //获取文件的MD5
                string vutMD5 = tlsHelper.OnGetMd5(file, out size);
                //从pfx中提取出的证书文件
                string certFilePath = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + @"\keykey.pem";
                //使用工具生成的pfx文件
                string privateKey = tlsHelper.OnExport(@"F:\rsa\fileVerify.pfx", "111111", certFilePath);
                //组织签名文件内容                                    
                int offset = 0; int len = 0; string signData = "";
                bool checkAll = false;
                if (size < 1 * 1024)
                {
                    checkAll = true;
                }
                int verify = tlsHelper.OnVerifyFileData(file, @"F:\rsa\fileVerify.pfx", "111111", out len, out offset, out signData, checkAll);
                if (verify == 0)
                {
                    string[] strs = new string[1];
                    strs[0] = privateKey + ",sha1," + "share/" + (vutMD5 + Path.GetExtension(file)) + "," + offset + "," + len + "," + signData;
                    tlsHelper.OnWriteFile(System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "abc.txt", strs);
                }
                LedYNetSdk.TLSInfo tInfo = tlsHelper.OnCreateTLS(offset, len, privateKey, signData);
                int rowsize = Marshal.SizeOf(typeof(LedYNetSdk.TLSInfo));
                IntPtr rowptr = Marshal.AllocHGlobal(rowsize);
                Marshal.StructureToPtr(tInfo, rowptr, false);
                //添加素材认证句柄
                LedYNetSdk.add_tls_md5(playlist, vutMD5, (IntPtr)rowptr);

                //将生成的图片信息添加到分区中（可以添加多个）倒数第二个参数为最后一张图片需要移动的宽度（比如一串文字生成了两张图片，第二张的图片没有达到分区大小时，请指定这个参数）
                err = LedYNetSdk.add_text_unit_img(area_tree, 0, 16, 64, file);
                //Console.WriteLine("add_text_unit_img:" + err);
                //将字幕分区添加到节目中
                err = LedYNetSdk.add_text(program, area_tree, 0, 0, 64, 32, 100, 0, 0);
                Console.WriteLine("add_text:" + err);

                string m_aging_start_time = "";
                string m_aging_stop_time = "";
                string m_period_ontime = "";
                string m_period_offtime = "";
                //将节目添加到播放列表中
                err = LedYNetSdk.add_program_in_playlist(playlist, program, 1, 10, m_aging_start_time, m_aging_stop_time, m_period_ontime, m_period_offtime, 127);
                Console.WriteLine("add_program_in_playlist:" + err);

                OnSendProgram(ip, port, str, playlist, privateKey);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Author：Judy
        /// 创建图文分区相关信息
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="str"></param>
        static void OnCreateImg(byte[] ip, ushort port, string str)
        {
            try
            {
                IntPtr playlist = LedYNetSdk.create_playlist(Program.width, Program.height, Program.Type);
                string name = "program_0";
                IntPtr program = LedYNetSdk.create_program(name, "0xff000000");
                //图片
                IntPtr pic_area = LedYNetSdk.create_pic();
                string file = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "红.bmp";

                //签名认证
                long size = 0;
                string vutMD5 = tlsHelper.OnGetMd5(file, out size);
                string certFilePath = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + @"\keykey.pem";
                string privateKey = tlsHelper.OnExport(@"F:\rsa\fileVerify.pfx", "111111", certFilePath);
                //组织签名文件内容                                    
                int offset = 0; int len = 0; string signData = "";
                bool checkAll = false;
                if (size < 1 * 1024)
                {
                    checkAll = true;
                }
                int verify = tlsHelper.OnVerifyFileData(file, @"F:\rsa\fileVerify.pfx", "111111", out len, out offset, out signData, checkAll);
                if (verify == 0)
                {
                    string[] strs = new string[1];
                    strs[0] = privateKey + ",sha1," + "share/" + (vutMD5 + Path.GetExtension(file)) + "," + offset + "," + len + "," + signData;
                    tlsHelper.OnWriteFile(System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "abc.txt", strs);
                }
                LedYNetSdk.TLSInfo tInfo = tlsHelper.OnCreateTLS(offset, len, privateKey, signData);
                int rowsize = Marshal.SizeOf(typeof(LedYNetSdk.TLSInfo));
                IntPtr rowptr = Marshal.AllocHGlobal(rowsize);
                Marshal.StructureToPtr(tInfo, rowptr, false);
                LedYNetSdk.add_tls_md5(playlist, vutMD5, (IntPtr)rowptr);

                //图元
                int err = LedYNetSdk.add_pic_unit(pic_area, 50, 5, 16, file);
                Console.WriteLine("add_pic_unit:" + err);
                //图片分区
                err = LedYNetSdk.add_pic(program, pic_area, 0, 0, 64, 32, 100);
                Console.WriteLine("add_pic:" + err);


                //IntPtr textPtr = LedYNetSdk.create_text();
                //LedYNetSdk.add_text_unit_text(textPtr, 5, 16, "Simsun", 12, "", "", "", "", "");

                string m_aging_start_time = "";
                string m_aging_stop_time = "";
                string m_period_ontime = "";
                string m_period_offtime = "";
                err = LedYNetSdk.add_program_in_playlist(playlist, program, 1, 10, m_aging_start_time, m_aging_stop_time, m_period_ontime, m_period_offtime, 127);
                Console.WriteLine("add_program_in_playlist:" + err);

                OnSendProgram(ip, port, str, playlist, privateKey);
            }
            catch (Exception e) { Console.Write(e); }
        }


        static void OnProgram_time_line(byte[] ip, ushort port, string str)
        {
            try
            {
                int err = 0;
                //创建播放列表句柄
                IntPtr playlist = LedYNetSdk.create_playlist(Program.width, Program.height, Program.Type);
                string name = "program_0";
                //创建节目句柄
                IntPtr program = LedYNetSdk.create_program(name, "0xff000000");
                string content1 = "%Y年%m月%d日";
                IntPtr time_area = LedYNetSdk.create_time();

                string file = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "红.bmp";

                //签名认证
                long size = 0;
                //获取文件的MD5
                string vutMD5 = tlsHelper.OnGetMd5(file, out size);
                //从pfx中提取出的证书文件
                string certFilePath = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + @"\keykey.pem";
                //使用工具生成的pfx文件
                string privateKey = tlsHelper.OnExport(@"F:\rsa\fileVerify.pfx", "111111", certFilePath);
                //组织签名文件内容                                    
                int offset = 0; int len = 0; string signData = "";
                bool checkAll = false;
                if (size < 1 * 1024)
                {
                    checkAll = true;
                }
                int verify = tlsHelper.OnVerifyFileData(file, @"F:\rsa\fileVerify.pfx", "111111", out len, out offset, out signData, checkAll);
                if (verify == 0)
                {
                    string[] strs = new string[1];
                    strs[0] = privateKey + ",sha1," + "share/" + (vutMD5 + Path.GetExtension(file)) + "," + offset + "," + len + "," + signData;
                    tlsHelper.OnWriteFile(System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "abc.txt", strs);
                }
                LedYNetSdk.TLSInfo tInfo = tlsHelper.OnCreateTLS(offset, len, privateKey, signData);
                int rowsize = Marshal.SizeOf(typeof(LedYNetSdk.TLSInfo));
                IntPtr rowptr = Marshal.AllocHGlobal(rowsize);
                Marshal.StructureToPtr(tInfo, rowptr, false);
                //添加素材认证句柄
                LedYNetSdk.add_tls_md5(playlist, vutMD5, (IntPtr)rowptr);
                string font = "simsun";
                string color = "0xff00fff0";
                string font_attributes = "normal";
                string bg_color = "0xffff0000";
                string time_equation = "1:0:00";
                string positive_te = "true";
                string adjustment = ("+00:00:00:00");//用以调整时差；支持天数，格式“±dd:hh:mm:ss”
                err = LedYNetSdk.add_time_unit(time_area, content1, color, font, 12, 0, 16, font_attributes);
                Console.WriteLine("add_time_unit:" + err);
                err = LedYNetSdk.add_time(program, time_area, 0, 0, 64, 32, 100, bg_color, time_equation, positive_te, adjustment);
                Console.WriteLine("add_time:" + err);

                string m_aging_start_time = "";
                string m_aging_stop_time = "";
                string m_period_ontime = "";
                string m_period_offtime = "";
                //将节目添加到播放列表中
                err = LedYNetSdk.add_program_in_playlist(playlist, program, 1, 10, m_aging_start_time, m_aging_stop_time, m_period_ontime, m_period_offtime, 127);
                Console.WriteLine("add_program_in_playlist:" + err);

                OnSendProgram(ip, port, str, playlist, privateKey);
            }
            catch (Exception)
            {
                throw;
            }
        }

        static void OnProgram_str_line(byte[] ip, ushort port, string str)
        {
            try
            {
                int err = 0;
                //创建播放列表句柄
                IntPtr playlist = LedYNetSdk.create_playlist(Program.width, Program.height, Program.Type);
                string name = "program_0";
                //创建节目句柄
                IntPtr program = LedYNetSdk.create_program(name, "0xff000000");
                IntPtr area_tree = LedYNetSdk.create_rich_text();

                err = LedYNetSdk.add_rich_text_unit(area_tree, 0, 16, "", "0xff000000", "<span foreground='blue' font='20'>以!遵守.,。，</span>");
                Console.WriteLine("add_rich_text_unit:" + err);
                err = LedYNetSdk.add_rich_text(program, area_tree, 0, 0, 64, 32, 100, 51, 2);
                Console.WriteLine("add_rich_text:" + err);
                LedYNetSdk.delete_rich_text(area_tree);

                string m_aging_start_time = "";
                string m_aging_stop_time = "";
                string m_period_ontime = "";
                string m_period_offtime = "";
                //将节目添加到播放列表中
                err = LedYNetSdk.add_program_in_playlist(playlist, program, 1, 10, m_aging_start_time, m_aging_stop_time, m_period_ontime, m_period_offtime, 127);
                Console.WriteLine("add_program_in_playlist:" + err);

                //从pfx中提取出的证书文件
                string certFilePath = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + @"\keykey.pem";
                //使用工具生成的pfx文件
                string privateKey = tlsHelper.OnExport(@"F:\rsa\fileVerify.pfx", "111111", certFilePath);
                OnSendProgram(ip, port, str, playlist, privateKey);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
