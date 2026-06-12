using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BX_Y_CSharp_SDK
{
    class Send_ProgramPictures
    {
        //发送图片   Send pictures
        public static void Program_img()
        {
            try
            {
                IntPtr playlist = LedYNetSdk.create_playlist(Program.width, Program.height, Program.Type);
                string name = "program_0";
                IntPtr program = LedYNetSdk.create_program(name, "0xff000000");
                //图片
                IntPtr pic_area = LedYNetSdk.create_pic();
                string file = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "test.bmp";

                //图元
                int err = LedYNetSdk.add_pic_unit(pic_area, 0, 0, 16, file);
                //图片分区
                err = LedYNetSdk.add_pic(program, pic_area, 0, 0, Program.width, Program.height, 100);
                LedYNetSdk.delete_pic(pic_area);
                
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
                    Console.WriteLine("更新节目图片成功！");
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
