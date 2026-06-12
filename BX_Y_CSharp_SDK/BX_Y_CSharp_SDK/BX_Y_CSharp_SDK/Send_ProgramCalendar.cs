using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BX_Y_CSharp_SDK
{
    class Send_ProgramCalendar
    {
        //发送农历      Send calendar
        public static void Program_calendar()
        {
            IntPtr playlist = LedYNetSdk.create_playlist(Program.width, Program.height, Program.Type);
            string name = "program_1";
            IntPtr program = LedYNetSdk.create_program(name, "0xff000000");

            //农历
            IntPtr calendar_area = LedYNetSdk.create_calendar();
            string mode = "heavenlystem";
            string font_color = "0xffff0000";
            string font_name = "宋体";
            int font_size = 12;
            string font_attributes = "bold";
            string text_content = "";
            int err = LedYNetSdk.add_calendar_unit(calendar_area, mode, font_color, font_name, font_size, 0, 16, font_attributes, text_content);
            //mode = "lunarcalendar";
            //err = LedYNetSdk.add_calendar_unit(calendar_area, mode, font_color, font_name, font_size, 0, 32, font_attributes, text_content);
            //mode = "solarterms";
            //err = LedYNetSdk.add_calendar_unit(calendar_area, mode, font_color, font_name, font_size, 0, 48, font_attributes, text_content);
            ////文字
            //mode = "text";
            //text_content = "仰邦科技";
            //err = LedYNetSdk.add_calendar_unit(calendar_area, mode, font_color, font_name, font_size, 0, 64, font_attributes, text_content);
            int transparency = 255;//透明度
            string bg_color = "0xff000000";
            string time_equation = "00:00:00";//时差，格式“hh:mm:ss”
            string positive_te = "true";//正，负时差：”true“，”false“
            string adjustment = ("+00:00:00:00");//用以调整时差；支持天数，格式“±dd:hh:mm:ss”
            err = LedYNetSdk.add_calendar(program, calendar_area, 0, 0, Program.width, Program.height, transparency, bg_color, time_equation, positive_te, adjustment);
            LedYNetSdk.delete_calendar(calendar_area);

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
                Console.WriteLine("更新节目农历成功！");
            }
            else
            {
                Console.WriteLine(Errer.GetError(err));
            }
            LedYNetSdk.delete_playlist(playlist);
        }
    }
}
