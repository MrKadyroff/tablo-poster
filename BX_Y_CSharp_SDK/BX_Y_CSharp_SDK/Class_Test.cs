using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BX_Y_CSharp_SDK
{
    class Class_Test
    {
        //动态区  文件
        public static void Program_dynamicfile()
        {
        //控制卡IP
            Program.ip = Encoding.ASCII.GetBytes("192.168.89.23");
        //控制卡类型
            Program.Type = 8792;
        //控制卡宽度
        Program.width = 384;
        //控制卡高度
        Program.height = 128;
        Program.port = 80;
            IntPtr playlist = LedYNetSdk.create_playlist(Program.width, Program.height, Program.Type);
            string name = "program_1";
            IntPtr program = LedYNetSdk.create_program(name, "0xff000000");

            int display_effects = 0;
            int display_speed = 16;
            int stay_time = 0;
            int gif_flag = 0;
            string bg_color = "0xff000000";
            string color = "0xffff0000";
            string font_attributes = "normal";
            string font = "SimSun";
            string align_h = "0";
            string align_v = "0";
            int err = 0;
            string file = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "txt/time0.txt";

            IntPtr dynamic_area0 = LedYNetSdk.create_dynamic();
            err = LedYNetSdk.add_dynamic_unit(dynamic_area0, 1, display_effects, display_speed, stay_time, file, gif_flag, bg_color, 10, font, color, font_attributes, align_h, align_v, 0, 0, 0, "", "");
            err = LedYNetSdk.add_dynamic(program, dynamic_area0, 0, 102,0,80,15, "-1", 0, "", 255);
            LedYNetSdk.delete_dynamic(dynamic_area0);

            string file1 = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "txt/time1.txt";
            IntPtr dynamic_area1 = LedYNetSdk.create_dynamic();
            err = LedYNetSdk.add_dynamic_unit(dynamic_area1, 1, display_effects, display_speed, stay_time, file1, gif_flag, bg_color, 10, font, color, font_attributes, align_h, align_v, 0, 0, 0, "", "");
            err = LedYNetSdk.add_dynamic(program, dynamic_area1, 1, 112,13,67,15, "-1", 0, "", 255);
            LedYNetSdk.delete_dynamic(dynamic_area1);

            string file2 = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "txt/t1.txt";
            IntPtr dynamic_area2 = LedYNetSdk.create_dynamic();
            err = LedYNetSdk.add_dynamic_unit(dynamic_area2, 1, display_effects, display_speed, stay_time, file2, gif_flag, bg_color, 10, font, color, font_attributes, align_h, align_v, 0, 0, 0, "", "");
            err = LedYNetSdk.add_dynamic(program, dynamic_area2, 2, 0,0,108,24, "-1", 0, "", 255);
            LedYNetSdk.delete_dynamic(dynamic_area2);

            string file3 = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "txt/t2.txt";
            IntPtr dynamic_area3 = LedYNetSdk.create_dynamic();
            err = LedYNetSdk.add_dynamic_unit(dynamic_area3, 1, display_effects, display_speed, stay_time, file3, gif_flag, bg_color, 10, font, color, font_attributes, align_h, align_v, 0, 0, 0, "", "");
            err = LedYNetSdk.add_dynamic(program, dynamic_area3, 3, 0,24,94,24, "-1", 0, "", 255);
            LedYNetSdk.delete_dynamic(dynamic_area3);

            string file4 = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "txt/t3.txt";
            IntPtr dynamic_area4 = LedYNetSdk.create_dynamic();
            err = LedYNetSdk.add_dynamic_unit(dynamic_area4, 1, display_effects, display_speed, stay_time, file4, gif_flag, bg_color, 10, font, color, font_attributes, align_h, align_v, 0, 0, 0, "", "");
            err = LedYNetSdk.add_dynamic(program, dynamic_area4, 4, 0,48,94,24, "-1", 0, "", 255);
            LedYNetSdk.delete_dynamic(dynamic_area4);

            string file5 = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "txt/t5.txt";
            IntPtr dynamic_area5 = LedYNetSdk.create_dynamic();
            err = LedYNetSdk.add_dynamic_unit(dynamic_area5, 1, display_effects, display_speed, stay_time, file5, gif_flag, bg_color, 10, font, color, font_attributes, align_h, align_v, 0, 0, 0, "", "");
            err = LedYNetSdk.add_dynamic(program, dynamic_area5, 5, 118,24,84,24, "-1", 0, "", 255);
            LedYNetSdk.delete_dynamic(dynamic_area5);

            IntPtr dynamic_area6 = LedYNetSdk.create_dynamic();
            err = LedYNetSdk.add_dynamic_unit(dynamic_area6, 1, display_effects, display_speed, stay_time, file5, gif_flag, bg_color, 10, font, color, font_attributes, align_h, align_v, 0, 0, 0, "", "");
            err = LedYNetSdk.add_dynamic(program, dynamic_area6, 6, 118,48,84,24, "-1", 0, "", 255);
            LedYNetSdk.delete_dynamic(dynamic_area6);

            string file6 = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "txt/1.txt";
            IntPtr dynamic_area7 = LedYNetSdk.create_dynamic();
            err = LedYNetSdk.add_dynamic_unit(dynamic_area7, 1, display_effects, display_speed, stay_time, file6, gif_flag, bg_color, 10, font, color, font_attributes, align_h, align_v, 0, 0, 0, "", "");
            err = LedYNetSdk.add_dynamic(program, dynamic_area7, 7, 94,28,24,24, "-1", 0, "", 255);
            LedYNetSdk.delete_dynamic(dynamic_area7);

            string file7 = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "txt/2.txt";
            IntPtr dynamic_area8 = LedYNetSdk.create_dynamic();
            err = LedYNetSdk.add_dynamic_unit(dynamic_area8, 1, display_effects, display_speed, stay_time, file7, gif_flag, bg_color, 10, font, color, font_attributes, align_h, align_v, 0, 0, 0, "", "");
            err = LedYNetSdk.add_dynamic(program, dynamic_area8, 8, 94,52,24,24, "-1", 0, "", 255);
            LedYNetSdk.delete_dynamic(dynamic_area8);

            string file8 = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "txt/scroll.txt";
            IntPtr dynamic_area9 = LedYNetSdk.create_dynamic();
            err = LedYNetSdk.add_dynamic_unit(dynamic_area9, 1, display_effects, display_speed, stay_time, file8, gif_flag, bg_color, 10, font, color, font_attributes, align_h, align_v, 0, 0, 0, "", "");
            err = LedYNetSdk.add_dynamic(program, dynamic_area9, 9, 0,72,192,24, "-1", 0, "", 255);
            LedYNetSdk.delete_dynamic(dynamic_area9);

            IntPtr dynamic_area10 = LedYNetSdk.create_dynamic();
            err = LedYNetSdk.add_dynamic_unit(dynamic_area10, 1, display_effects, display_speed, stay_time, file, gif_flag, bg_color, 10, font, color, font_attributes, align_h, align_v, 0, 0, 0, "", "");
            err = LedYNetSdk.add_dynamic(program, dynamic_area10, 10, 294,0,80,15, "-1", 0, "", 255);
            LedYNetSdk.delete_dynamic(dynamic_area10);

            IntPtr dynamic_area11 = LedYNetSdk.create_dynamic();
            err = LedYNetSdk.add_dynamic_unit(dynamic_area11, 1, display_effects, display_speed, stay_time, file1, gif_flag, bg_color, 10, font, color, font_attributes, align_h, align_v, 0, 0, 0, "", "");
            err = LedYNetSdk.add_dynamic(program, dynamic_area11, 11, 304,13,67,15, "-1", 0, "", 255);
            LedYNetSdk.delete_dynamic(dynamic_area11);

            IntPtr dynamic_area12 = LedYNetSdk.create_dynamic();
            err = LedYNetSdk.add_dynamic_unit(dynamic_area12, 1, display_effects, display_speed, stay_time, file2, gif_flag, bg_color, 10, font, color, font_attributes, align_h, align_v, 0, 0, 0, "", "");
            err = LedYNetSdk.add_dynamic(program, dynamic_area12, 12, 192,0,108,24, "-1", 0, "", 255);
            LedYNetSdk.delete_dynamic(dynamic_area12);

            IntPtr dynamic_area13 = LedYNetSdk.create_dynamic();
            err = LedYNetSdk.add_dynamic_unit(dynamic_area13, 1, display_effects, display_speed, stay_time, file3, gif_flag, bg_color, 10, font, color, font_attributes, align_h, align_v, 0, 0, 0, "", "");
            err = LedYNetSdk.add_dynamic(program, dynamic_area13, 13, 192,24,94,24, "-1", 0, "", 255);
            LedYNetSdk.delete_dynamic(dynamic_area13);

            IntPtr dynamic_area14 = LedYNetSdk.create_dynamic();
            err = LedYNetSdk.add_dynamic_unit(dynamic_area14, 1, display_effects, display_speed, stay_time, file4, gif_flag, bg_color, 10, font, color, font_attributes, align_h, align_v, 0, 0, 0, "", "");
            err = LedYNetSdk.add_dynamic(program, dynamic_area14, 14,192,48,94,24, "-1", 0, "", 255);
            LedYNetSdk.delete_dynamic(dynamic_area14);

            IntPtr dynamic_area15 = LedYNetSdk.create_dynamic();
            err = LedYNetSdk.add_dynamic_unit(dynamic_area15, 1, display_effects, display_speed, stay_time, file5, gif_flag, bg_color, 10, font, color, font_attributes, align_h, align_v, 0, 0, 0, "", "");
            err = LedYNetSdk.add_dynamic(program, dynamic_area15, 15, 310,24,74,24, "-1", 0, "", 255);
            LedYNetSdk.delete_dynamic(dynamic_area15);

            IntPtr dynamic_area16 = LedYNetSdk.create_dynamic();
            err = LedYNetSdk.add_dynamic_unit(dynamic_area16, 1, display_effects, display_speed, stay_time, file5, gif_flag, bg_color, 10, font, color, font_attributes, align_h, align_v, 0, 0, 0, "", "");
            err = LedYNetSdk.add_dynamic(program, dynamic_area16, 16, 310,48,74,24, "-1", 0, "", 255);
            LedYNetSdk.delete_dynamic(dynamic_area16);

            IntPtr dynamic_area17 = LedYNetSdk.create_dynamic();
            err = LedYNetSdk.add_dynamic_unit(dynamic_area17, 1, display_effects, display_speed, stay_time, file6, gif_flag, bg_color, 10, font, color, font_attributes, align_h, align_v, 0, 0, 0, "", "");
            err = LedYNetSdk.add_dynamic(program, dynamic_area17, 17, 286, 28, 24, 24, "-1", 0, "", 255);
            LedYNetSdk.delete_dynamic(dynamic_area17);

            IntPtr dynamic_area18 = LedYNetSdk.create_dynamic();
            err = LedYNetSdk.add_dynamic_unit(dynamic_area18, 1, display_effects, display_speed, stay_time, file7, gif_flag, bg_color, 10, font, color, font_attributes, align_h, align_v, 0, 0, 0, "", "");
            err = LedYNetSdk.add_dynamic(program, dynamic_area18, 18, 286, 52, 24, 24, "-1", 0, "", 255);
            LedYNetSdk.delete_dynamic(dynamic_area18);

            IntPtr dynamic_area19 = LedYNetSdk.create_dynamic();
            err = LedYNetSdk.add_dynamic_unit(dynamic_area19, 1, display_effects, display_speed, stay_time, file8, gif_flag, bg_color, 10, font, color, font_attributes, align_h, align_v, 0, 0, 0, "", "");
            err = LedYNetSdk.add_dynamic(program, dynamic_area19, 19, 192, 72, 192,24, "-1", 0, "", 255);
            LedYNetSdk.delete_dynamic(dynamic_area19);

            string m_aging_start_time = "";
            string m_aging_stop_time = "";
            string m_period_ontime = "";
            string m_period_offtime = "";
            err = LedYNetSdk.add_program_in_playlist(playlist, program, 1, 10, m_aging_start_time, m_aging_stop_time, m_period_ontime, m_period_offtime, 127);
            err = LedYNetSdk.update_dynamic(Program.ip, Program.port, Program.str, Program.str, playlist, "", 0, 0);
            if (err == 0)
            {
                Console.WriteLine("更新动态区文件成功！");
            }
            else
            {
                Console.WriteLine(Errer.GetError(err));
            }

            LedYNetSdk.delete_playlist(playlist);
        }
        public static void Program_dynamicfile2()
        {
            //控制卡IP
            Program.ip = Encoding.ASCII.GetBytes("192.168.89.23");
            //控制卡类型
            Program.Type = 8792;
            //控制卡宽度
            Program.width = 384;
            //控制卡高度
            Program.height = 128;
            Program.port = 80;
            IntPtr playlist = LedYNetSdk.create_playlist(Program.width, Program.height, Program.Type);
            string name = "program_1";
            IntPtr program = LedYNetSdk.create_program(name, "0xff000000");

            int display_effects = 0;
            int display_speed = 16;
            int stay_time = 0;
            int gif_flag = 0;
            string bg_color = "0xff000000";
            string color = "0xffff0000";
            string font_attributes = "normal";
            string font = "SimSun";
            string align_h = "0";
            string align_v = "0";
            int err = 0;
            string file6 = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "txt/1.txt";
            IntPtr dynamic_area7 = LedYNetSdk.create_dynamic();
            err = LedYNetSdk.add_dynamic_unit(dynamic_area7, 1, display_effects, display_speed, stay_time, file6, gif_flag, bg_color, 10, font, color, font_attributes, align_h, align_v, 0, 0, 0, "", "");
            err = LedYNetSdk.add_dynamic(program, dynamic_area7, 7, 94, 28, 24, 24, "-1", 0, "", 255);
            LedYNetSdk.delete_dynamic(dynamic_area7);

            string file7 = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "txt/2.txt";
            IntPtr dynamic_area8 = LedYNetSdk.create_dynamic();
            err = LedYNetSdk.add_dynamic_unit(dynamic_area8, 1, display_effects, display_speed, stay_time, file7, gif_flag, bg_color, 10, font, color, font_attributes, align_h, align_v, 0, 0, 0, "", "");
            err = LedYNetSdk.add_dynamic(program, dynamic_area8, 8, 94, 52, 24, 24, "-1", 0, "", 255);
            LedYNetSdk.delete_dynamic(dynamic_area8);

            IntPtr dynamic_area17 = LedYNetSdk.create_dynamic();
            err = LedYNetSdk.add_dynamic_unit(dynamic_area17, 1, display_effects, display_speed, stay_time, file6, gif_flag, bg_color, 10, font, color, font_attributes, align_h, align_v, 0, 0, 0, "", "");
            err = LedYNetSdk.add_dynamic(program, dynamic_area17, 17, 286, 28, 24, 24, "-1", 0, "", 255);
            LedYNetSdk.delete_dynamic(dynamic_area17);

            IntPtr dynamic_area18 = LedYNetSdk.create_dynamic();
            err = LedYNetSdk.add_dynamic_unit(dynamic_area18, 1, display_effects, display_speed, stay_time, file7, gif_flag, bg_color, 10, font, color, font_attributes, align_h, align_v, 0, 0, 0, "", "");
            err = LedYNetSdk.add_dynamic(program, dynamic_area18, 18, 286, 52, 24, 24, "-1", 0, "", 255);
            LedYNetSdk.delete_dynamic(dynamic_area18);

            string m_aging_start_time = "";
            string m_aging_stop_time = "";
            string m_period_ontime = "";
            string m_period_offtime = "";
            err = LedYNetSdk.add_program_in_playlist(playlist, program, 1, 10, m_aging_start_time, m_aging_stop_time, m_period_ontime, m_period_offtime, 127);
            err = LedYNetSdk.update_dynamic(Program.ip, Program.port, Program.str, Program.str, playlist, "", 0, 0);
            if (err == 0)
            {
                Console.WriteLine("更新动态区文件成功！");
            }
            else
            {
                Console.WriteLine(Errer.GetError(err));
            }

            LedYNetSdk.delete_playlist(playlist);
        }
    }
}
