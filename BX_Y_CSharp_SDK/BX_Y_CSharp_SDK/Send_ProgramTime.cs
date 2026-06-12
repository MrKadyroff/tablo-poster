using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BX_Y_CSharp_SDK
{
    class Send_ProgramTime
    {
        //发送时间  Send time
        public static void Program_time()
        {
            int err;
            IntPtr playlist = LedYNetSdk.create_playlist(Program.width, Program.height, Program.Type);
            string name = "program_1";
            IntPtr program = LedYNetSdk.create_program(name, "0xff000000");
            //时间
            IntPtr time_area = LedYNetSdk.create_time();
            string content = "jfshafffdjksf";
            string content1 = "%Y年%m月%d日";
            string content2 = "星期%w";
            string content3 = "%H:%M:%S秒";
            string font = "simsun";
            string color = "0xff00fff0";
            string font_attributes = "normal";
            string bg_color = "0xff000000";
            string time_equation = "1:0:00";
            string positive_te = "true";
            string adjustment = ("+00:00:00:00");//用以调整时差；支持天数，格式“±dd:hh:mm:ss”
            err = LedYNetSdk.add_time_unit(time_area, content3, color, font, 12, 8, 24, font_attributes);
            //err = LedYNetSdk.add_time_unit(time_area, content2, color, font, 12, 0, 32, font_attributes);
            //Console.WriteLine("add_time_unit:" + err);
            //err = LedYNetSdk.add_time_unit(time_area, content3, color, font, 12, 0, 48, font_attributes);
            //Console.WriteLine("add_time_unit:" + err);
            //err = LedYNetSdk.add_time_unit(time_area, content, color, font, 12, 0, 64, font_attributes);
            //Console.WriteLine("add_time_unit:" + err);
            err = LedYNetSdk.add_time(program, time_area, 0, 0, Program.width, Program.height, 100, bg_color, time_equation, positive_te, adjustment);
            LedYNetSdk.delete_time(time_area);
            
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
                Console.WriteLine("更新节目图片成功！");
            }
            else
            {
                ////Console.WriteLine(Errer.GetError(err));
            }
            LedYNetSdk.delete_playlist(playlist);
        }
    }
}
