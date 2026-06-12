using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace BX_Y_CSharp_SDK
{
    class Send_DynamicFile
    {
        //动态区  文件
        public static void Program_dynamicfile(int dynamic_type)
        {
            Console.WriteLine("time:" + DateTime.Now.Second + " " + DateTime.Now.Millisecond);
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

            IntPtr dynamic_area = LedYNetSdk.create_dynamic();
            string file = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "test.bmp";
            if (dynamic_type == 0) { file = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "test.bmp"; }
            else { file = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "test.txt"; }
            err = LedYNetSdk.add_dynamic_unit(dynamic_area, dynamic_type, display_effects, display_speed, stay_time, file, gif_flag, bg_color, 12, font, color, font_attributes, align_h, align_v, 0, 0, 0, "", "",0);
            //file = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "colorstr.png"; 
            //err = LedYNetSdk.add_dynamic_unit(dynamic_area, dynamic_type, display_effects, display_speed, stay_time, file, gif_flag, bg_color, 12, font, color, font_attributes, align_h, align_v, 0, 0, 0, "", "");
            //Console.WriteLine("add_dynamic_unit:" + err);
            err = LedYNetSdk.add_dynamic(program, dynamic_area, 0, 0, 0, Program.width, Program.height, "-1", 0, "", 255);
            //Console.WriteLine("add_dynamic:" + err);
            LedYNetSdk.delete_dynamic(dynamic_area);

            string m_aging_start_time = "";
            string m_aging_stop_time = "";
            string m_period_ontime = "";
            string m_period_offtime = "";
            err = LedYNetSdk.add_program_in_playlist(playlist, program, 1, 10, m_aging_start_time, m_aging_stop_time, m_period_ontime, m_period_offtime, 127);
            err = LedYNetSdk.update_dynamic(Program.ip, Program.port, Program.str, Program.str, playlist, "", 0, 0);
            //err = LedYNetSdk.save_dynamic_forid(Program.ip, Program.port, Program.str, Program.str, "0");
            if (err == 0)
            {
                Console.WriteLine("更新动态区文件成功！");
            }
            else
            {
                ////Console.WriteLine(Errer.GetError(err));
            }

            LedYNetSdk.delete_playlist(playlist);
            Console.WriteLine("time:" + DateTime.Now.Second + " " + DateTime.Now.Millisecond);

            //只更新动态区素材，在显示屏上已存在改动态区后才可以使用
            while (false)
            {
                IntPtr playlist1 = LedYNetSdk.create_playlist(Program.width, Program.height, Program.Type);
                IntPtr program1 = LedYNetSdk.create_program(name, "0xff000000");

                IntPtr dynamic_area1 = LedYNetSdk.create_dynamic();
                err = LedYNetSdk.add_dynamic_unit(dynamic_area1, 0, display_effects, display_speed, stay_time, file, gif_flag, bg_color, 12, font, color, font_attributes, align_h, align_v, 0, 0, 0, "", "",0);
                //Console.WriteLine("add_dynamic_unit:" + err);
                err = LedYNetSdk.add_dynamic(program1, dynamic_area1, 0, 0, 0, Program.width, Program.height, "-1", 0, "", 255);
                //Console.WriteLine("add_dynamic:" + err);
                LedYNetSdk.delete_dynamic(dynamic_area1);

                err = LedYNetSdk.add_program_in_playlist(playlist1, program1, 1, 10, m_aging_start_time, m_aging_stop_time, m_period_ontime, m_period_offtime, 127);
                Console.WriteLine("add_program_in_playlist:" + err);
                err = LedYNetSdk.update_dynamic_unit(Program.ip, Program.port, Program.str, Program.str, playlist1);
                Console.WriteLine("update_dynamic_small:" + err);

                LedYNetSdk.delete_playlist(playlist1);
            }
        }
        public static void Program_dynamicfile1()
        {
            Console.WriteLine("time:" + DateTime.Now.Second + " " + DateTime.Now.Millisecond);
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

            IntPtr dynamic_area = LedYNetSdk.create_dynamic();
            string file = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "test.mp4";
            err = LedYNetSdk.add_dynamic_unit(dynamic_area, 2, display_effects, display_speed, stay_time, file, gif_flag, bg_color, 12, font, color, font_attributes, align_h, align_v, 0, 0, 0, "", "", 0);
            //file = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "colorstr.png"; 
            //err = LedYNetSdk.add_dynamic_unit(dynamic_area, dynamic_type, display_effects, display_speed, stay_time, file, gif_flag, bg_color, 12, font, color, font_attributes, align_h, align_v, 0, 0, 0, "", "");
            //Console.WriteLine("add_dynamic_unit:" + err);
            err = LedYNetSdk.add_dynamic(program, dynamic_area, 0, 0, 0, Program.width, Program.height, "-1", 0, "", 255);
            //Console.WriteLine("add_dynamic:" + err);
            LedYNetSdk.delete_dynamic(dynamic_area);

            string m_aging_start_time = "";
            string m_aging_stop_time = "";
            string m_period_ontime = "";
            string m_period_offtime = "";
            err = LedYNetSdk.add_program_in_playlist(playlist, program, 1, 10, m_aging_start_time, m_aging_stop_time, m_period_ontime, m_period_offtime, 127);
            err = LedYNetSdk.update_dynamic(Program.ip, Program.port, Program.str, Program.str, playlist, "", 0, 0);
            err = LedYNetSdk.save_dynamic_forid(Program.ip, Program.port, Program.str, Program.str, "0");
            if (err == 0)
            {
                Console.WriteLine("更新动态区文件成功！");
            }
            else
            {
                ////Console.WriteLine(Errer.GetError(err));
            }

            LedYNetSdk.delete_playlist(playlist);
            Console.WriteLine("time:" + DateTime.Now.Second + " " + DateTime.Now.Millisecond);

            //只更新动态区素材，在显示屏上已存在改动态区后才可以使用
            while (false)
            {
                IntPtr playlist1 = LedYNetSdk.create_playlist(Program.width, Program.height, Program.Type);
                IntPtr program1 = LedYNetSdk.create_program(name, "0xff000000");

                IntPtr dynamic_area1 = LedYNetSdk.create_dynamic();
                err = LedYNetSdk.add_dynamic_unit(dynamic_area1, 0, display_effects, display_speed, stay_time, file, gif_flag, bg_color, 12, font, color, font_attributes, align_h, align_v, 0, 0, 0, "", "", 0);
                //Console.WriteLine("add_dynamic_unit:" + err);
                err = LedYNetSdk.add_dynamic(program1, dynamic_area1, 0, 0, 0, Program.width, Program.height, "-1", 0, "", 255);
                //Console.WriteLine("add_dynamic:" + err);
                LedYNetSdk.delete_dynamic(dynamic_area1);

                err = LedYNetSdk.add_program_in_playlist(playlist1, program1, 1, 10, m_aging_start_time, m_aging_stop_time, m_period_ontime, m_period_offtime, 127);
                Console.WriteLine("add_program_in_playlist:" + err);
                err = LedYNetSdk.update_dynamic_unit(Program.ip, Program.port, Program.str, Program.str, playlist1);
                Console.WriteLine("update_dynamic_small:" + err);

                LedYNetSdk.delete_playlist(playlist1);
            }
        }
    }
}
