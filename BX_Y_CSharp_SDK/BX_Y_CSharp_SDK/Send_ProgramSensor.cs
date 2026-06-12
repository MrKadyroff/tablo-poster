using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BX_Y_CSharp_SDK
{
    class Send_ProgramSensor
    {
        //发送传感器区域
        public static void Program_str()
        {
            try
            {
                int err = 0;
                IntPtr playlist = LedYNetSdk.create_playlist(Program.width, Program.height, Program.Type);
                string name = "program_0";
                IntPtr program = LedYNetSdk.create_program(name, "0xff000000");
                //创建传感器分区
                //IntPtr area_tree = LedYNetSdk.create_sensor();
                //节目添加传感器数据
                err = LedYNetSdk.add_sensor(program, 0, 0, 64, 32, 100, "宋体", 12, "normal", "0xFFFFFFFF", "0xFFFFFFFF", "0x00000000", "%%d ℃", 0, 0, 0, 0, 0, 0, "1", -1, "0x0100", "2", 5);
                //销毁传感器句柄
                //LedYNetSdk.delete_sensor(area_tree);

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
                    Console.WriteLine("更新节目字幕成功！");
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
