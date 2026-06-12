using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BX_Y_CSharp_SDK
{
    class Send_ProgramClock
    {
        //发送表盘      Send clock
        public static void Program_clock()
        {
            int err = 0;
            IntPtr playlist = LedYNetSdk.create_playlist(Program.width, Program.height, Program.Type);
            string name = "program_0";
            IntPtr program = LedYNetSdk.create_program(name, "0xff000000");
            IntPtr area_tree = LedYNetSdk.create_clock();
            //时针
            err = LedYNetSdk.add_clock_hour(area_tree, "", "0xFFFF0000", 12, 2);
            //分针
            err = LedYNetSdk.add_clock_minute(area_tree, "", "0xFFFFFF00", 10, 1);
            //秒针
            err = LedYNetSdk.add_clock_second(area_tree, "", "0xFF00FF00", 8, 1);

            //表盘
            int x = 0;//坐标起始X
            int y = 0;//坐标起始Y
            int w = Program.width;//区域宽度
            int h = Program.height;//区域高度
            int transparency = 255;//透明度
            string time_equation = "00:00:00";//时差，格式“hh:mm:ss”
            string positive_te = "true";//正，负时差：”true“，”false“
            string hour_color = "0xffff0000";//时针颜⾊
            string minute_color = "0xffff0000";//分针颜⾊
            string second_color = "0xffff0000";//秒针颜⾊
            string adjustment = ("+00:00:00:00");//用以调整时差；支持天数，格式“±dd:hh:mm:ss”
            string bg_image = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "clock\\1.png";//背景图⽚路径
            err = LedYNetSdk.add_clock(program, area_tree, x, y, w, h, transparency, time_equation, positive_te,adjustment, hour_color, minute_color, second_color, bg_image);
            LedYNetSdk.delete_clock(area_tree);
            //文字
            //IntPtr time_area = LedYNetSdk.create_time();
            //string content = "仰邦科技";
            //string font = "宋体";
            //string color = "0xffff0000";
            //string font_attributes = "bold";
            //string bg_color = "0xff000000";
            //err = LedYNetSdk.add_time_unit(time_area, content, color, font, 12, 0, 16, font_attributes);
            //Console.WriteLine("add_time_unit:" + err);
            //err = LedYNetSdk.add_time(program, time_area, 0, 0, 128, 32, 255, bg_color, time_equation, positive_te);
            //Console.WriteLine("add_time:" + err);

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
                Console.WriteLine("更新节目表盘成功！");
            }
            else
            {
                Console.WriteLine(Errer.GetError(err));
            }
            LedYNetSdk.delete_playlist(playlist);
        }
    }
}
