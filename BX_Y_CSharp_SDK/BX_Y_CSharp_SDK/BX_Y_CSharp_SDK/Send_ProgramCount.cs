using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BX_Y_CSharp_SDK
{
    class Send_ProgramCount
    {
        //发送计时      send count
        public static void Program_count()
        {
            IntPtr playlist = LedYNetSdk.create_playlist(Program.width, Program.height, Program.Type);
            string name = "program_1";
            IntPtr program = LedYNetSdk.create_program(name, "0xff000000");


            int x = 0;//坐标起始X
            int y = 0;//坐标起始Y
            int w = Program.width;//区域宽度
            int h = Program.height;//区域高度
            int transparency = 255;//透明度
            string bg_color = "0xff000000";
            string time_equation = "00:00:00";//时差，格式“hh:mm:ss”
            string positive_te = "end";//正，负时差：”true“，”false“
            string target_date = "2022-12-30";
            string target_time = "12:00:00";
            string content = "dd天hh时mm分ss秒";
            string font_color = "0xffff0000";
            string font_name = "宋体";
            int font_size = 12;
            string font_attributes = "bold";
            int content_x = 0;
            int content_y = 16;
            string add_enable = "yes";
            int err = LedYNetSdk.add_count(program, x, y, w, h, transparency, bg_color, time_equation, positive_te, target_date, target_time, content, font_color, font_name, font_size, content_x, content_y, font_attributes, add_enable);
            
            string m_aging_start_time = "";
            string m_aging_stop_time = "";
            string m_period_ontime = "";
            string m_period_offtime = "";
            LedYNetSdk.add_program_in_playlist(playlist, program, 1, 10, m_aging_start_time, m_aging_stop_time, m_period_ontime, m_period_offtime, 127);

            int send_style = 0;
            var szLocalTempDir = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            long free_size = 0; long total_size = 0;
            err = LedYNetSdk.send_program(Program.ip, Program.port, Program.str, Program.str, szLocalTempDir, playlist, send_style, ref free_size, ref total_size);

            if (err == 0)
            {
                Console.WriteLine("更新节目计时成功！");
            }
            else
            {
                Console.WriteLine(Errer.GetError(err));
            }
            LedYNetSdk.delete_playlist(playlist);
        }
    }
}
