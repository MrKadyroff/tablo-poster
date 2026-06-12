using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BX_Y_CSharp_SDK
{
    class Send_ProgramVideo
    {
        //发送视频  Send Video
        public static void Program_video()
        {
            IntPtr playlist = LedYNetSdk.create_playlist(Program.width, Program.height, Program.Type);
            string name = "program_1";
            IntPtr program = LedYNetSdk.create_program(name, "0xff000000");

            //视频
            string VideoFile = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "test.mp4";
            IntPtr video_area = LedYNetSdk.create_video();
            int err = LedYNetSdk.add_video_unit(video_area, 2, 1, 0, 1000, VideoFile, "");
            string clone_str = "";
            err = LedYNetSdk.add_video(program, video_area, 0, 0, Program.width, Program.height, 0, 0, 0, clone_str, "");
            LedYNetSdk.delete_video(video_area);

            string m_aging_start_time = "";
            string m_aging_stop_time = "";
            string m_period_ontime = "";
            string m_period_offtime = "";
            err = LedYNetSdk.add_program_in_playlist(playlist, program, 1, 10, m_aging_start_time, m_aging_stop_time, m_period_ontime, m_period_offtime, 127);
            //string storage_media = "emmc";
            //err = LedYNetSdk.set_screen_storage_media(ip, 80, str, str, storage_media);

            int send_style = 0;
            var szLocalTempDir = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            long free_size = 0; long total_size = 0;
            err = LedYNetSdk.send_program(Program.ip, Program.port, Program.str, Program.str, szLocalTempDir, playlist, send_style, ref free_size, ref total_size);

            if (err == 0)
            {
                Console.WriteLine("更新节目视频成功！");
            }
            else
            {
                Console.WriteLine(Errer.GetError(err));
            }
            LedYNetSdk.delete_playlist(playlist);
        }
    }
}
