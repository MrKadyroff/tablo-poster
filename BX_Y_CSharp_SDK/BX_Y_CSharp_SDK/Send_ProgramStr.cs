using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace BX_Y_CSharp_SDK
{
    class Send_ProgramStr
    {
        //发送字幕      Send one-line subtitles
        public static void Program_str()
        {
            try
            {
                int err = 0;
                IntPtr playlist = LedYNetSdk.create_playlist(Program.width, Program.height, Program.Type);
                string name = "program_0";
                IntPtr program = LedYNetSdk.create_program(name, "0xff000000");
                
                IntPtr area_tree = LedYNetSdk.create_text();
                //图片
                //string file = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "女.bmp";
                //err = LedYNetSdk.add_text_unit_img(area_tree, 5, 5, 128, file);
                //文本
                err = LedYNetSdk.add_text_unit_text(area_tree, 0, 15, "宋体", 12, "normal", "1", "0xffff0000", "0xff000000", "12 ");
                err = LedYNetSdk.add_text(program, area_tree, 0, 0, Program.width, Program.height, 100, 52, 1);
                LedYNetSdk.delete_text(area_tree);

                string m_aging_start_time = "";
                string m_aging_stop_time = "";
                string m_period_ontime = "";
                string m_period_offtime = "";
                err = LedYNetSdk.add_program_in_playlist(playlist, program, 1, 10, m_aging_start_time, m_aging_stop_time, m_period_ontime, m_period_offtime, 127);
                
                int send_style = 0;
                var szLocalTempDir = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
                long free_size = 0; long total_size = 0;
                byte[] playListName = new byte[1024];
                err = LedYNetSdk.send_program(Program.ip, Program.port, Program.str, Program.str, szLocalTempDir, playlist, send_style, ref free_size, ref total_size, IntPtr.Zero, playListName, 0, 1);

                if (err == 0)
                {
                    Console.WriteLine("更新节目字幕成功！");
                }
                else
                {
                    //Console.WriteLine(Errer.GetError(err));
                }
                LedYNetSdk.delete_playlist(playlist);
            }
            catch (Exception e) { Console.Write(e); }
        }

        //发送富文本字幕      Send one-line subtitles  
        public static void Program_str_line()
        {
            try
            {
                int err = 0;
                IntPtr playlist = LedYNetSdk.create_playlist(Program.width, Program.height, Program.Type);
                string name = "program_0";
                IntPtr program = LedYNetSdk.create_program(name, "0xff000000");
                //
                IntPtr area_tree = LedYNetSdk.create_rich_text();
                err = LedYNetSdk.add_rich_text_unit(area_tree, 0, 16, "", "0xff000000", "<span foreground='red' font='10'>te st</span>");
                err = LedYNetSdk.add_rich_text(program, area_tree, 0, 0, Program.areaw, Program.areah, 100, 1, 0);
                LedYNetSdk.delete_rich_text(area_tree);

                string m_aging_start_time = "";
                string m_aging_stop_time = "";
                string m_period_ontime = "";
                string m_period_offtime = "";
                err = LedYNetSdk.add_program_in_playlist(playlist, program, 1, 10, m_aging_start_time, m_aging_stop_time, m_period_ontime, m_period_offtime, 127);
                
                int send_style = 0;
                var szLocalTempDir = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
                long free_size = 0; long total_size = 0;
                byte[] playListName = new byte[1024];
                err = LedYNetSdk.send_program(Program.ip, Program.port, Program.str, Program.str, szLocalTempDir, playlist, send_style, ref free_size, ref total_size, IntPtr.Zero, playListName, 0, 1);

                if (err == 0)
                {
                    Console.WriteLine("更新节目富文本成功！");
                }
                else
                {
                    //Console.WriteLine(Errer.GetError(err));
                }
                LedYNetSdk.delete_playlist(playlist);
            }
            catch (Exception e) { Console.Write(e); }
        }

        /// <summary>
        /// 只发送节目，不播放
        /// </summary>
        public static void OnOnlySendProgram()
        {
            try
            {
                int err = 0;
                IntPtr playlist = LedYNetSdk.create_playlist(Program.width, Program.height, Program.Type);
                string name = "program_0";
                IntPtr program = LedYNetSdk.create_program(name, "0xff000000");

                IntPtr area_tree = LedYNetSdk.create_text();
                //文本
                err = LedYNetSdk.add_text_unit_text(area_tree, 0, 15, "宋体", 12, "normal", "1", "0xffff0000", "0xff000000", "这是只发送不播放的节目 ");
                err = LedYNetSdk.add_text(program, area_tree, 0, 0, Program.areaw, Program.areah, 100, 52, 1);
                LedYNetSdk.delete_text(area_tree);

                string m_aging_start_time = "";
                string m_aging_stop_time = "";
                string m_period_ontime = "";
                string m_period_offtime = "";
                err = LedYNetSdk.add_program_in_playlist(playlist, program, 1, 10, m_aging_start_time, m_aging_stop_time, m_period_ontime, m_period_offtime, 127);
                //只发节目发送标识4
                int send_style = 4;
                var szLocalTempDir = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
                long free_size = 0; long total_size = 0;
                byte[] playListName = new byte[1024];
                err = LedYNetSdk.send_program(Program.ip, Program.port, Program.str, Program.str, szLocalTempDir, playlist, send_style, ref free_size, ref total_size, IntPtr.Zero, playListName, 0, 1);
                string read_playlist = System.Text.Encoding.Unicode.GetString(playListName).Split('\0')[0];
                read_playlist = read_playlist.Replace("&#x0;", "");
                read_playlist = read_playlist.Replace("쳌", "");
                if (err == 0)
                {
                    Console.WriteLine("发送节目完成！" + "playlistname: " + read_playlist);
                }
                else
                {
                    //Console.WriteLine(Errer.GetError(err));
                }
                LedYNetSdk.delete_playlist(playlist);
            }
            catch (Exception e) { Console.Write(e); }
        }

        //发送离线节目      Send one-line subtitles
        public static void OnSendOnlineProgram()
        {
            try
            {
                int err = 0;
                IntPtr playlist = LedYNetSdk.create_playlist(Program.width, Program.height, Program.Type);
                string name = "program_0";
                IntPtr program = LedYNetSdk.create_program(name, "0xff000000");

                IntPtr area_tree = LedYNetSdk.create_text();
                //文本
                err = LedYNetSdk.add_text_unit_text(area_tree, 0, 15, "宋体", 12, "normal", "1", "0xffff0000", "0xff000000", "这是离线节目 ");
                err = LedYNetSdk.add_text(program, area_tree, 0, 0, Program.areaw, Program.areah, 100, 52, 1);
                LedYNetSdk.delete_text(area_tree);

                string m_aging_start_time = "";
                string m_aging_stop_time = "";
                string m_period_ontime = "";
                string m_period_offtime = "";
                err = LedYNetSdk.add_program_in_playlist(playlist, program, 1, 10, m_aging_start_time, m_aging_stop_time, m_period_ontime, m_period_offtime, 127);
                //离线节目发送标识3
                int send_style = 3;
                var szLocalTempDir = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
                long free_size = 0; long total_size = 0;
                //离线节目
                Program.DefualtProgramInfo defualtInfo = new Program.DefualtProgramInfo();
                defualtInfo.playmode = Program.OnGetPtr("net");
                defualtInfo.defaultprogramlist = Program.OnGetPtr(" ");
                defualtInfo.intervaltime = Program.OnGetPtr("5");
                defualtInfo.playtime = Program.OnGetPtr(" ");
                int dsize = Marshal.SizeOf(typeof(Program.DefualtProgramInfo));
                IntPtr dPtr = Marshal.AllocHGlobal(dsize);
                Marshal.StructureToPtr(defualtInfo, dPtr, false);
                byte[] playListName = new byte[1024];
                err = LedYNetSdk.send_program(Program.ip, Program.port, Program.str, Program.str, szLocalTempDir, playlist, send_style, ref free_size, ref total_size, dPtr, playListName, 0, 1);
                string read_playlist = System.Text.Encoding.Unicode.GetString(playListName).Split('\0')[0];
                read_playlist = read_playlist.Replace("&#x0;", "");
                read_playlist = read_playlist.Replace("쳌", "");
                if (err == 0)
                {
                    Console.WriteLine("发送离线节目成功！" + "playlistname: " + read_playlist);
                }
                else
                {
                    //Console.WriteLine(Errer.GetError(err));
                }
                LedYNetSdk.delete_playlist(playlist);
            }
            catch (Exception e) { Console.Write(e); }
        }

        public static void OnCloseOnlineProgram()
        {
            try
            {
                int err = LedYNetSdk.close_default_program(Program.ip, Program.port, Program.str, Program.str);

                if (err == 0)
                {
                    Console.WriteLine("关闭离线节目成功！");
                }
                else
                {
                    //Console.WriteLine(Errer.GetError(err));
                }
            }
            catch (Exception)
            {
                
                throw;
            }
        }
    }
}
