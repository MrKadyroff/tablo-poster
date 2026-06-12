using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
                err = LedYNetSdk.add_text_unit_text(area_tree, 0, 15, "宋体", 12, "normal", "1", "0xffff0000", "0xff000000", "465");
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
                err = LedYNetSdk.send_program(Program.ip, Program.port, Program.str, Program.str, szLocalTempDir, playlist, send_style, ref free_size, ref total_size);

                if (err == 0)
                {
                    Console.WriteLine("更新节目字幕成功！");
                }
                else
                {
                    Console.WriteLine(Errer.GetError(err));
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
                err = LedYNetSdk.add_rich_text(program, area_tree, 0, 0, Program.width, Program.height, 100, 1, 0);
                LedYNetSdk.delete_rich_text(area_tree);

                string m_aging_start_time = "";
                string m_aging_stop_time = "";
                string m_period_ontime = "";
                string m_period_offtime = "";
                err = LedYNetSdk.add_program_in_playlist(playlist, program, 1, 10, m_aging_start_time, m_aging_stop_time, m_period_ontime, m_period_offtime, 127);
                
                int send_style = 0;
                var szLocalTempDir = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
                long free_size = 0; long total_size = 0;
                err = LedYNetSdk.send_program(Program.ip, Program.port, Program.str, Program.str, szLocalTempDir, playlist, send_style, ref free_size, ref total_size);

                if (err == 0)
                {
                    Console.WriteLine("更新节目富文本成功！");
                }
                else
                {
                    Console.WriteLine(Errer.GetError(err));
                }
                LedYNetSdk.delete_playlist(playlist);
            }
            catch (Exception e) { Console.Write(e); }
        }
    }
}
