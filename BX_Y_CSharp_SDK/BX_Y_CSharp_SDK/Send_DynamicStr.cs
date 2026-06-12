using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace BX_Y_CSharp_SDK
{
    class Send_DynamicStr
    {
        //动态区 字符串
        public static void Program_dynamictest()
        {
            //while(true){
            IntPtr playlist = LedYNetSdk.create_playlist(Program.width, Program.height, Program.Type);
            string name = "program_1";
            IntPtr program = LedYNetSdk.create_program(name, "0xff000000");

            int display_effects = 1;
            int display_speed = 1;
            int stay_time = 5;
            int gif_flag = 0;
            string bg_color = "0xFF000000";
            string color = "0xffff0000";
            string font_attributes = "normal";
            string font = "宋体";
            string align_h = "2";
            string align_v = "2";
            int err = 0;

            //LedYNetSdk.upload_file(Program.ip, Program.port, Program.str, Program.str, System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "test.mp4", "test", IntPtr.Zero); 
            IntPtr dynamic_area = LedYNetSdk.create_dynamic();
            Random ran = new Random();
            byte[] b = System.Text.Encoding.UTF8.GetBytes("5555");
            string file = Convert.ToBase64String(b);
            err = LedYNetSdk.add_dynamic_unit(dynamic_area, 1, display_effects, display_speed, stay_time, file, gif_flag, bg_color, 10, font, color, font_attributes, align_h, align_v, 100, 0, 0, "","",0);
            //Console.WriteLine("add_dynamic_unit:" + err);
            err = LedYNetSdk.add_dynamic(program, dynamic_area, 0, 0, 0, Program.width, Program.height, "-1", 0, "", 100);
            //Console.WriteLine("add_dynamic:" + err);
            LedYNetSdk.delete_dynamic(dynamic_area);
            //IntPtr dynamic_area2 = LedYNetSdk.create_dynamic();
            //byte[] b1 = System.Text.Encoding.UTF8.GetBytes("456");
            //string file1 = Convert.ToBase64String(b1);
            //err = LedYNetSdk.add_dynamic_unit(dynamic_area2, 1, display_effects, display_speed, stay_time, file1, gif_flag, bg_color, 10, font, color, font_attributes, align_h, align_v, 0, 0, 0, "", "");
            ////Console.WriteLine("add_dynamic_unit:" + err);
            //err = LedYNetSdk.add_dynamic(program, dynamic_area2, 1, 32, 0, 32, 32, "-1", 0, "", 255);
            ////Console.WriteLine("add_dynamic:" + err);
            //LedYNetSdk.delete_dynamic(dynamic_area2);

            string m_aging_start_time = "";
            string m_aging_stop_time = "";
            string m_period_ontime = "";
            string m_period_offtime = "";
            err = LedYNetSdk.add_program_in_playlist(playlist, program, 1, 10, m_aging_start_time, m_aging_stop_time, m_period_ontime, m_period_offtime, 127);
            err = LedYNetSdk.update_dynamic_small(Program.ip, Program.port, Program.str, Program.str, playlist, "", 0, 0);
            //err = LedYNetSdk.save_dynamic_forid(Program.ip, Program.port, Program.str, Program.str, "0");
            if (err == 0)
            {
                Console.WriteLine("更新动态区字符串成功！");
            }
            else
            {
                //Console.WriteLine(Errer.GetError(err));
            }
            LedYNetSdk.delete_playlist(playlist);
            Thread.Sleep(10000);

            //只更新动态区素材，在显示屏上已存在改动态区后才可以使用
            //while (false)
            //{
                IntPtr playlist1 = LedYNetSdk.create_playlist(Program.width, Program.height, Program.Type);
                IntPtr program1 = LedYNetSdk.create_program(name, "0xff000000");

                IntPtr dynamic_area1 = LedYNetSdk.create_dynamic();
                b = System.Text.Encoding.UTF8.GetBytes("123456");//Guid.NewGuid().ToString());
                file = Convert.ToBase64String(b); color = "0xffffff00";
                err = LedYNetSdk.add_dynamic_unit(dynamic_area1, 1, display_effects, display_speed, stay_time, file, gif_flag, bg_color, 12, font, color, font_attributes, align_h, align_v, 0, 0, 0, "","", 0);
                //Console.WriteLine("add_dynamic_unit:" + err);
                err = LedYNetSdk.add_dynamic(program1, dynamic_area1, 0, 0, 0, Program.width, Program.height, "-1", 0, "", 255);
                //Console.WriteLine("add_dynamic:" + err);
                LedYNetSdk.delete_dynamic(dynamic_area1);

                err = LedYNetSdk.add_program_in_playlist(playlist1, program1, 1, 10, m_aging_start_time, m_aging_stop_time, m_period_ontime, m_period_offtime, 127);
                Console.WriteLine("add_program_in_playlist:" + err);
                err = LedYNetSdk.update_dynamic_unit_small(Program.ip, Program.port, Program.str, Program.str, playlist1);
                Console.WriteLine("update_dynamic_small:" + err);

                LedYNetSdk.delete_playlist(playlist1);
            //}//}
        }

        public static void Program_dynamictest1()
        {
            IntPtr dwhandPtr = LedYNetSdk.net_login(Program.ip, Program.port, Program.str, Program.str);
            Console.WriteLine("time:" +DateTime.Now.Second+" "+ DateTime.Now.Millisecond);
            IntPtr playlist = LedYNetSdk.create_playlist(Program.width, Program.height, Program.Type);
            string name = "program_1";
            IntPtr program = LedYNetSdk.create_program(name, "0xff000000");

            int display_effects = 52;
            int display_speed = 16;
            int stay_time = 0;
            int gif_flag = 0;
            string bg_color = "0xFF000000";
            string color = "0xffff0000";
            string font_attributes = "normal";
            string font = "宋体";
            string align_h = "2";
            string align_v = "2";
            int err = 0;

            IntPtr dynamic_area = LedYNetSdk.create_dynamic();
            Random ran = new Random();
            byte[] b = System.Text.Encoding.UTF8.GetBytes("rtsp://wowzaec2demo.streamlock.net/vod/mp4:BigBuckBunny_115k.mov");
            string file = Convert.ToBase64String(b);
            err = LedYNetSdk.add_dynamic_unit(dynamic_area, 1, display_effects, display_speed, stay_time, file, gif_flag, bg_color, 10, font, color, font_attributes, align_h, align_v, 0, 0, 1, "", "", 0);
            //Console.WriteLine("add_dynamic_unit:" + err);
            err = LedYNetSdk.add_dynamic(program, dynamic_area, 0, 0, 0, Program.width, Program.height, "-1", 0, "", 100);
            //Console.WriteLine("add_dynamic:" + err);
            LedYNetSdk.delete_dynamic(dynamic_area);

            string m_aging_start_time = "";
            string m_aging_stop_time = "";
            string m_period_ontime = "";
            string m_period_offtime = "";
            err = LedYNetSdk.add_program_in_playlist(playlist, program, 1, 10, m_aging_start_time, m_aging_stop_time, m_period_ontime, m_period_offtime, 127);
            err = LedYNetSdk.update_dynamic_small_dwhand(dwhandPtr, playlist, "", 0, 0);
            //err = LedYNetSdk.save_dynamic_forid(Program.ip, Program.port, Program.str, Program.str, "0");
            if (err == 0)
            {
                Console.WriteLine("更新动态区字符串成功！");
            }
            else
            {
                //Console.WriteLine(Errer.GetError(err));
            }
            LedYNetSdk.delete_playlist(playlist);
            Console.WriteLine("time:" + DateTime.Now.Second + " " + DateTime.Now.Millisecond);
            LedYNetSdk.net_logout(dwhandPtr);
        }
    }
}
