using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BX_Y_CSharp_SDK
{
    class Send_ProgramColorStr
    {
        //炫彩字
        public static void Programstr()
        {
            try
            {
                int err = 0;
                IntPtr playlist = LedYNetSdk.create_playlist(Program.width, Program.height, Program.Type);
                string name = "program_0";
                IntPtr program = LedYNetSdk.create_program(name, "0xff000000");
                //图片
                IntPtr area_tree = LedYNetSdk.create_colortext();
                LedYNetSdk.add_colorful_fontunit(area_tree, "colorstr.png", 0, 16, 5, 0, 16, 1, 50);
                LedYNetSdk.add_colorful_hollowunit(area_tree, 5, 5, 0, "bgcolor.png");
                LedYNetSdk.add_colorful_subtitle(program, area_tree, 0, 0, Program.width, Program.height);
                LedYNetSdk.delete_colortext(area_tree);

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
                    Console.WriteLine("更新节目炫彩字成功！");
                }
                else
                {
                    ////Console.WriteLine(Errer.GetError(err));
                }
                LedYNetSdk.delete_playlist(playlist);
            }
            catch (Exception e) { Console.Write(e); }
        }
    }
}
