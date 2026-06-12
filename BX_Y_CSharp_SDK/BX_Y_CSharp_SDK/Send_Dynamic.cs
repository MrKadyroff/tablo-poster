using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace BX_Y_CSharp_SDK
{
    class Send_Dynamic
    {
        /// <summary>
        /// Author：Judy
        /// 更新动态区
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public static void OnUpdateDynamic(byte[] ip, ushort port)
        {
            try
            {
                IntPtr DynamicAreaPlayList = LedYNetSdk.create_playlist(Program.width, Program.height, Program.Type);
                IntPtr program = LedYNetSdk.create_program("p1", "0xff000000");

                IntPtr intPtr = LedYNetSdk.create_dynamic();
                string filePath = @"E:\Picture\20200611140739.png";
                string content = "";
                string tmp_exten_name = Path.GetExtension(filePath);
                int unitType = 0;

                content = filePath;
                if (tmp_exten_name.Equals(".txt"))
                {
                    using (StreamReader sr = new StreamReader(filePath, Encoding.UTF8))
                    {
                        string txtContent = sr.ReadToEnd();
                        byte[] arr = Encoding.UTF8.GetBytes(txtContent);
                        content = Convert.ToBase64String(arr);
                        sr.Close();
                    }
                    unitType = 1;
                }
                else
                {
                    unitType = 0;
                }
                //改成了表单形式，传入路径即可put.file_path
                int resultUnit = LedYNetSdk.add_dynamic_unit(intPtr, unitType, 0, 16, 1, content,
                    0, "0x00000000", 12, "Simsun", "0xFFFF00FF", "", "Left", "Top", 0, 0, 0, "","", 0);

                LedYNetSdk.add_dynamic(program, intPtr, 0, 0, 0, 1920, 682,
                    "", 0, "", 100);
                //Console.WriteLine("add_dynamic:" + err);
                LedYNetSdk.delete_dynamic(intPtr);

                resultUnit = LedYNetSdk.add_program_in_playlist(DynamicAreaPlayList, program, 1, 10, "", "", "", "", 127);

                //更新动态区
                if (!tmp_exten_name.Equals(".txt"))
                {
                    resultUnit = LedYNetSdk.update_dynamic(ip, port, "guest", "guest",
                                    DynamicAreaPlayList, "", 0, 0);
                }
                else
                {
                    resultUnit = LedYNetSdk.update_dynamic_small(ip, port, "guest", "guest",
                                    DynamicAreaPlayList, "", 0, 0);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Author：Judy
        /// 更新动态区素材
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public static void OnUpdateDynamicUnit(byte[] ip, ushort port)
        {
            try
            {
                IntPtr DynamicAreaPlayList = LedYNetSdk.create_playlist(Program.width, Program.height, Program.Type);
                IntPtr program = LedYNetSdk.create_program("p1", "0xff000000");

                IntPtr intPtr = LedYNetSdk.create_dynamic();
                string filePath = @"E:\Picture\20200611140825.png";
                string content = "";
                string tmp_exten_name = Path.GetExtension(filePath);
                int unitType = 0;

                content = filePath;
                if (tmp_exten_name.Equals(".txt"))
                {
                    using (StreamReader sr = new StreamReader(filePath, Encoding.UTF8))
                    {
                        string txtContent = sr.ReadToEnd();
                        byte[] arr = Encoding.UTF8.GetBytes(txtContent);
                        content = Convert.ToBase64String(arr);
                        sr.Close();
                    }
                    unitType = 1;
                }
                else
                {
                    unitType = 0;
                }
                //改成了表单形式，传入路径即可put.file_path
                int resultUnit = LedYNetSdk.add_dynamic_unit(intPtr, unitType, 0, 16, 1, content,
                    0, "0x00000000", 12, "Simsun", "0xFFFF00FF", "", "Left", "Top", 0, 0, 0, "", "", 0);

                LedYNetSdk.add_dynamic(program, intPtr, 0, 0, 0, 1024, 600,
                    "", 0, "", 100);
                //Console.WriteLine("add_dynamic:" + err);
                LedYNetSdk.delete_dynamic(intPtr);

                resultUnit = LedYNetSdk.add_program_in_playlist(DynamicAreaPlayList, program, 1, 10, "", "", "", "", 127);

                //更新动态区
                if (!tmp_exten_name.Equals(".txt"))
                {
                    resultUnit = LedYNetSdk.update_dynamic_unit(ip, port, "guest", "guest",
                                    DynamicAreaPlayList);
                }
                else
                {
                    resultUnit = LedYNetSdk.update_dynamic_unit_small(ip, port, "guest", "guest",
                                    DynamicAreaPlayList);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }



        /// <summary>
        /// Author：Judy
        /// 网络数据分区（使用动态区方式处理）
        /// Base64编码的获取网络数据的格式：使用 " 分割,索引使用 [index] 表达
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="str"></param>
        public static void OnCreateJsonDynamic()
        {
            try
            {
                IntPtr playlist = LedYNetSdk.create_playlist(Program.width, Program.height, Program.Type);
                string name = "program_0";
                IntPtr program = LedYNetSdk.create_program(name, "0xff000000");
                IntPtr intPtr = LedYNetSdk.create_dynamic();
                //网络数据分区（动态区类型4/URLText）
                int unitType = 4;
                string url = "http://121.199.50.130:8180/projectDataServer/passengerServer/queryRealTimePassengerByUser?loginName=xmnpt&loginPwd=123456";
                string urlstr = "cmVzdWx0IlswXSJzaG9wSWQ=";
                //改成了表单形式，传入路径即可put.file_path
                int resultUnit = LedYNetSdk.add_dynamic_unit(intPtr, unitType, 0, 16, 5, url,
                    0, "0x00000000", 12, "Simsun", "0xFFFF00FF", "", "Left", "Top", 0, 0, 0, urlstr, "", 0);

                LedYNetSdk.add_dynamic(program, intPtr, 0, 0, 0, Program.width, Program.height, "", 0, "", 100);

                LedYNetSdk.delete_dynamic(intPtr);

                resultUnit = LedYNetSdk.add_program_in_playlist(playlist, program, 1, 10, "", "", "", "", 127);

                //更新动态区
                resultUnit = LedYNetSdk.update_dynamic_small(Program.ip, Program.port, Program.str, Program.str, playlist, "", 0, 0);
                if (resultUnit == 0)
                {
                    Console.WriteLine("更新动态区字符串成功！");
                }
                else
                {
                    Console.WriteLine("更新动态区字符串失败，错误码:" + resultUnit);
                }
                LedYNetSdk.delete_playlist(playlist);
            }
            catch (Exception)
            {
                throw;
            }
        }


        //删除所有动态区   Delete all dynamic zones
        static void Program_cleardynamic(byte[] ip, ushort port, string str)
        {
            int err = LedYNetSdk.clear_dynamic(ip, (ushort)port, "guest", "guest");
        }
    }
}
