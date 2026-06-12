using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace BX_Y_CSharp_SDK
{
    public class LedYNetSdk
    {
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int active_tts(byte[] ip, ushort port, string user_name, string user_pwd);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int active_tts_dwhand(IntPtr dwhand);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int add_animation(IntPtr program, int x, int y, int w, int h, int transparency, int display_effects, int display_density, int display_size, string direction, int display_speed, string animation_color, int taper, string file_path, string file_type);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int add_audio(IntPtr tree, IntPtr area_tree);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int add_audio_unit(IntPtr area_tree, int volume, string path);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int add_broder(IntPtr tree, IntPtr area_tree, int x, int y, int w, int h, int transparency, string areaXYWH = "");
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int add_broder_unit(IntPtr area_tree, int duration, int broder_w, int texture_w, int stunt_type, int stunt_speed, int flicker_grade, string src_path, string flicker_path);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int add_bulletin(string file_path, IntPtr bulletin, int x, int y, int w, int h, string name, int layout, int transparency, int font_size, string font_name, string font_color, string bg_color, int display_effects, int display_speed, int stay_time, string aging_start_time, string aging_end_time, string period_ontime, string period_offtime, string content, string font_align);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int add_calendar(IntPtr tree, IntPtr area_tree, int x, int y, int w, int h, int transparency, string bg_color, string time_equation, string positive_te, string adjustment);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int add_calendar_unit(IntPtr calendar_tree, string mode, string font_color, string font_name, int font_size, int x, int y, string font_attributes, string text_content);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int add_clock(IntPtr tree, IntPtr clock_area, int x, int y, int w, int h, int transparency, string time_equation, string positive_te, string adjustment, string hour_color, string minute_color, string second_color, string bg_image);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int add_clock_hour(IntPtr area_tree, string src_path, string h_color, int h_length, int h_width);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int add_clock_minute(IntPtr area_tree, string src_path, string m_color, int m_length, int m_width);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int add_clock_second(IntPtr area_tree, string src_path, string s_color, int s_length, int s_width);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int add_colorful_fontunit(IntPtr area_tree, string path, int display_effects, int display_speed, int stay_time, int wave_effects, int wave_count, int wave_speed, int wave_amplitude);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int add_colorful_hollowunit(IntPtr area_tree, int display_effects, int display_speed, int stay_time, string path);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int add_colorful_subtitle(IntPtr tree, IntPtr area_tree, int x, int y, int w, int h);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int add_count(IntPtr tree, int x, int y, int w, int h, int transparency, string bg_color, string time_equation, string positive_te, string target_date, string target_time, string content, string font_color, string font_name, int font_size, int content_x, int content_y, string font_attributes, string add_enable);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int add_db(IntPtr tree, IntPtr area_tree, int x, int y, int w, int h, int transparency);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int add_db_unit(IntPtr db_tree, IntPtr db_unit, IntPtr db_unit_info);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int add_db_unit_specifycell(IntPtr db_unit, IntPtr sepcify_cell);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int add_db_unit_specifycolumn(IntPtr db_unit, IntPtr sepcify_column);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int add_db_unit_specifyrow(IntPtr db_unit, IntPtr specify_row);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int add_dynamic(IntPtr tree, IntPtr area_tree, int dynamic_id, int x, int y, int w, int h, string relative_program, int run_mode, string update_frequency, int transparency);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int add_dynamic_unit(IntPtr dynamic_area, int dynamic_type, int display_effects, int display_speed, int stay_time, string file_path, int gif_flag, string bg_color, int font_size, string font_name, string font_color, string font_attributes, string align_h, string align_v, int volumn, int scale_mode, int rolation_mode, string key_list, string proxyService);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern void add_font(IntPtr font, IntPtr tls_font, string font_file, string font_name, IntPtr tls_infos);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern void add_insert_list(IntPtr playlist, int insert_list_count, int insert_list_duration);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int add_nvr(IntPtr program, IntPtr nvr_area, int x, int y, int w, int h, int volume_mode, int ratation_mode);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int add_nvr_unit(IntPtr nvr_area, string nvrid, string username, string upwd, string nvraddr, int playTime, int volume, string valid, int x, int y, int w, int h);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern void add_manage_audio(IntPtr audio, string audio_name, string audio_path);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern void add_manage_sensor(IntPtr sensor, int unit_type, int significant_digits, float unit_coefficient, float correction, string thresh_mode, int thresh, string sensor_addr, string fun_seq, int relay_type, int relay_switch);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int add_pic(IntPtr tree, IntPtr area_tree, int x, int y, int w, int h, int transparency);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int add_pic_unit(IntPtr area_tree, int stay_time, int display_effects, int display_speed, string path);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int add_program_in_playlist(IntPtr playlist, IntPtr program, int play_mode, int play_time, string aging_start_time, string aging_end_time, string period_ontime, string period_offtime, int play_week);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int add_sensor(IntPtr program, int x, int y, int w, int h, int transparency, string font_name, int font_size, string font_attributes, string font_color, string thresh_fontcolor, string bg_color, string content_sensor, int content_x, int content_y, int unit_type, int significant_digits, float unit_coefficient, float correction, string thresh_mode, int thresh, string sensor_addr, string fun_seq, int update_time);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int add_text(IntPtr tree, IntPtr area_tree, int x, int y, int w, int h, int transparency, int display_effects, int unit_type);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int add_text_unit_img(IntPtr area_tree, int stay_time, int display_speed, int last_move_width, string path);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int add_text_unit_text(IntPtr area_tree, int stay_time, int display_speed, string font_name, int font_size, string font_attributes, string font_alignment, string font_color, string bg_color, string content);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int add_time(IntPtr tree, IntPtr area_tree, int x, int y, int w, int h, int transparency, string bg_color, string time_equation, string positive_te, string adjustment);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int add_time_unit(IntPtr time_tree, string content, string font_color, string font_name, int font_size, int x, int y, string font_attributes);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern void add_tls_md5(IntPtr playlist, string md5, IntPtr tls_infos);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int add_tts_voice(byte[] ip, ushort port, string user_name, string user_pwd, string voice_text, int loop, int gender, int effect, int volume, int tone, int speed, int one, int stay_time);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int add_tts_voice_dwhand(IntPtr dwhand, string voice_text, int loop, int gender, int effect, int volume, int tone, int speed, int one, int stay_time);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern void add_turnonoff(IntPtr trunonoff, int action, string name);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int add_video(IntPtr tree, IntPtr area_tree, int x, int y, int w, int h, int volume_mode, int video_type, int ratation_mode, string clone_str, string crop_type);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern void add_video_md5(IntPtr playlist, string md5, string file_path);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int add_video_unit(IntPtr area_tree, int volume, int scale_mode, int source, int play_time, string path, string crop_type);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int add_weather(IntPtr tree, IntPtr area_tree, int x, int y, int w, int h, int transparency, int stay_time, int display_effects, int display_speed, string weatherURIHead, string cityID, string language, string fontFamily, string foreGround, int fontSize, int smallFontSize, string modifyCityName, int display_mode, int displayLines, int iconGrade, int temp_unit, int isDisplayCity, int isDisplayIcon, int isDisplayWeath, int isDisplayTemp, int isDisplayWind, int isDisplayAirIndex, int isDisplayPM);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int cancel_screen_cus_turnonoff(byte[] ip, ushort port, string user_name, string user_pwd);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int cancel_screen_cus_turnonoff_dwhand(IntPtr dwhand);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern void cancel_send_program(IntPtr playlist);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int change_password(byte[] ip, ushort port, string user_name, string user_pwd, string pwd);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int change_password_dwhand(IntPtr dwhand, string pwd, int style = 1);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int check_screen_info(byte[] ip, ushort port, string user_name, string user_pwd, string pid, ushort device_type);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int check_screen_info_dwhand(IntPtr dwhand, string pid, ushort device_type);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int check_time(byte[] ip, ushort port, string user_name, string user_pwd);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int check_time_dwhand(IntPtr dwhand);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int clear_all_program(byte[] ip, ushort port, string user_name, string user_pwd);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int clear_all_program_dwhand(IntPtr dwhand);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int clear_dynamic(byte[] ip, ushort port, string user_name, string user_pwd);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int clear_dynamic_dwhand(IntPtr dwhand);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int clear_material(byte[] ip, ushort port, string user_name, string user_pwd);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int clear_material_dwhand(IntPtr dwhand);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int connect_wifi(string barcode, string pid, string wifi_name, string wifi_pwd, ref int min_waitTime, ref int max_waitTime);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr create_audio();
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr create_broder();
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr create_bulletin(int x, int y, int w, int h, string name, int layout, int transparency, int font_size, string font_name, string font_color, string bg_color, int display_effects, int display_speed, int stay_time, string aging_start_time, string aging_end_time, string period_ontime, string period_offtime, string content, string font_align);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr create_calendar();
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr create_clock();
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr create_colortext();
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr create_db();
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr create_db_unit();
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr create_dynamic();
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr create_font();
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr create_font_tls();
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr create_nvr();
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr create_manage_audio();
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr create_manage_sensor();
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr create_pic();

        // 函数：	add_rich_text
        // 返回值：	成功返回0；失败返回错误号
        // 参数：	
        //			unsigned long program：节目句柄
        //			unsigned long text_area：富文本区分区句柄
        //			int x：分区x坐标
        //			int y：分区y坐标
        //			int w：分区宽度 
        //			int h：分区高度
        //			int transparency：分区透明度(0~100)：100 为完全不透明，默认100
        //			int display_effects：显示特技;建议只使用0，50，51这三种特技类型                
        // 说明：	添加富文本区
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int add_rich_text(IntPtr program, IntPtr rich_text_area, int x, int y, int w, int h, int transparency, int display_effects, int alignment_h);

        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr create_rich_text();
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern void delete_rich_text(IntPtr area_tree);
        // 函数：	add_rich_text_unit
        // 返回值：	成功返回0；失败返回错误号
        // 参数：	
        //			unsigned long rich_text_area：富文本分区句柄
        //			int stay_time：特技停留时间，以秒为单位
        //			int display_speed：特技速度等级，1~16,1为最快
        //			_TEXT_CHAR* bg_image_path：背景图片文件路径，上位机需要将背景图片和节目文件一起下发，该目录为控制卡存储背景图片的目录。
        //			_TEXT_CHAR* bg_color：背景色；
        //			_TEXT_CHAR* content：文本内容；需要显示的内容和文字属性，如字体类型、颜色、大小、行间距，如：<span foreground=’white’ font=’12’>上海仰邦</span>https://developer.gnome.org/pango/stable/pango-Markup.html
        // 说明：	添加富文本分区项
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int add_rich_text_unit(IntPtr rich_text_area, int stay_time, int display_speed, string bg_image_path, string bg_color, string content);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr create_playlist(int w, int h, int device_type);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr create_program(string name, string bg_color);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr create_radio();
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr create_sensor();
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr create_text();
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr create_time();
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr create_turnonoff();
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr create_video();
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr create_weather();
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern void delete_add_audio(IntPtr audio, string audio_name);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern void delete_add_font(IntPtr font, string font_name);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern void delete_add_sensor(IntPtr sensor, int sensor_index);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern void delete_audio(IntPtr area_tree);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern void delete_broder(IntPtr area_tree);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int delete_bulletin(byte[] ip, ushort port, string user_name, string user_pwd, string names);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int delete_bulletin_dwhand(IntPtr dwhand, string names);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern void delete_calendar(IntPtr area_tree);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern void delete_clock(IntPtr area_tree);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern void delete_colortext(IntPtr area_tree);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern void delete_create_audio(IntPtr audio);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern void delete_create_font(IntPtr font);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern void delete_create_sensor(IntPtr sensor);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern void delete_db(IntPtr area_tree);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern void delete_db_unit(IntPtr area_tree);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern void delete_dynamic(IntPtr dynamic_area);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int delete_font(byte[] ip, ushort port, string user_name, string user_pwd, IntPtr font);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int delete_font_dwhand(IntPtr dwhand, IntPtr font);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int delete_input_dynamic(byte[] ip, ushort port, string user_name, string user_pwd, string delete_list);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int delete_input_dynamic_dwhand(IntPtr dwhand, string delete_list);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern void delete_nvr(IntPtr area_tree);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern void delete_pic(IntPtr area_tree);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern void delete_playlist(IntPtr playlist);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern void delete_program(IntPtr program_area);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int delete_save_dynamic(byte[] ip, ushort port, string user_name, string user_pwd);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int delete_save_dynamic_dwhand(IntPtr dwhand);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern void delete_sensor(IntPtr area_tree);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern void delete_text(IntPtr area_tree);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern void delete_time(IntPtr area_tree);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int delete_tls(byte[] ip, ushort port, string tls_content, string fingerprint, string add_certificate_type);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int delete_tls_dwhand(IntPtr dwhand, string tls_content, string fingerprint, string add_certificate_type);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern void delete_turnonoff(IntPtr trunonoff);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern void delete_video(IntPtr area_tree);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern void delete_weather(IntPtr area_tree);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern void destroy_radio(IntPtr radios);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int disconnect_wifi(string barcode, string pid);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int download_file(byte[] ip, ushort port, string user_name, string user_pwd, string src_path, string dest_path);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int download_file_dwhand(IntPtr dwhand, string src_path, string dest_path);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int enable_screen_server(string barcode, string pid, int server_mode);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int enable_screen_server_dwhand(IntPtr dwhand, int server_mode);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int enable_uploaddownload(byte[] ip, ushort port, string user_name, string user_pwd, int enable_state, int upload_download);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int enable_uploaddownload_dwhand(IntPtr dwhand, int enable_state, int upload_download);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int set_screen_server_info_dwhand(IntPtr dwhand, string server_ip, string server_port);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int get_screen_server_info_dwhand(IntPtr dwhand, byte[] server_ip, byte[] server_port);
        [DllImport("kernel32", SetLastError = true)]
        public static extern bool FreeLibrary(IntPtr hModule);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int get_barcode(byte[] ip, ushort port, string user_name, string user_pwd, byte[] barcode);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int get_barcode_dwhand(IntPtr dwhand, byte[] barcode);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int get_bx_param(IntPtr dwhand, byte[] param);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int Get_CardList(byte[] datas, ref int data_count);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int get_firmware_version(byte[] ip, ushort port, string user_name, string user_pwd, byte[] firmware_version, byte[] app_version, byte[] fpga_version);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int get_firmware_version_dwhand(IntPtr dwhand, byte[] firmware_version, byte[] app_version, byte[] fpga_version);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int Get_Port_Barcode(string barcode);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int Get_Port_Pid(string pid);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int get_screen_capture(byte[] ip, ushort port, string user_name, string user_pwd, string dest_path, int capture_w, int capture_h);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int get_screen_capture_dwhand(IntPtr dwhand, string dest_path, int capture_w, int capture_h);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int get_screen_capture_dwhand1(IntPtr dwhand, string dest_path, int capture_w, int capture_h, ref int min_waitTime, ref int max_waitTime, byte[] download_path);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int copy_file_dwhand1(IntPtr dwhand, string file_path, string download_path);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int get_screen_cur_disk(byte[] ip, ushort port, string user_name, string user_pwd, byte[] curDisk);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int get_screen_cur_disk_dwhand(IntPtr dwhand, byte[] curDisk);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int get_screen_disk_info(byte[] ip, ushort port, string user_name, string user_pwd, string storage_media, ref long totalsize, ref long freesize, ref long usedsize);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int get_screen_disk_info_dwhand(IntPtr dwhand, string storage_media, ref long totalsize, ref long freesize, ref long usedsize);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int get_screen_disk_list(byte[] ip, ushort port, string user_name, string user_pwd, byte[] diskList);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int get_screen_disk_list_dwhand(IntPtr dwhand, byte[] diskList);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int get_screen_log(byte[] ip, ushort port, string user_name, string user_pwd, string dest_path, string dest_path1);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int get_screen_log_dwhand(IntPtr dwhand, string dest_path, string dest_path1);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int get_screen_mac(byte[] ip, ushort port, string user_name, string user_pwd, byte[] mac);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int get_screen_mac_dwhand(IntPtr dwhand, byte[] mac);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int get_screen_output_type(byte[] ip, ushort port, string user_name, string user_pwd, ref int screen_output_type);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int get_screen_output_type_dwhand(IntPtr dwhand, ref int screen_output_type);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int get_screen_parameters(byte[] ip, ushort port, string user_name, string user_pwd, byte[] datas);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int get_screen_parameters_dwhand(IntPtr dwhand, byte[] datas);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int get_screen_player_file(byte[] ip, ushort port, string user_name, string user_pwd, byte[] program_list, byte[] program_name);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int get_screen_player_file_dwhand(IntPtr dwhand, byte[] program_list, byte[] program_name);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int get_screen_player_mode(byte[] ip, ushort port, string user_name, string user_pwd, ref int screen_player_mode);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int get_screen_player_mode_dwhand(IntPtr dwhand, ref int screen_player_mode);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int get_screen_send_program_info(byte[] ip, ushort port, string user_name, string user_pwd, ref int screen_w, ref int screen_h, ref int fold_type, ref ushort screen_type);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int get_screen_send_program_info_dwhand(IntPtr dwhand, ref int screen_w, ref int screen_h, ref int fold_type, ref ushort screen_type);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int get_screen_status(byte[] ip, ushort port, string user_name, string user_pwd, ref int screen_onoff, ref int brigtness, ref int brigtness_mode, ref int volume, ref int screen_lockunlock, ref int program_lockunlock, ref int screen_output_type, ref int screen_player_mode, byte[] screen_time, byte[] screen_addr, byte[] screen_customer_onoff, byte[] screen_language, byte[] screen_gps);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int get_screen_status_dwhand(IntPtr dwhand, ref int screen_onoff, ref int brigtness, ref int brigtness_mode, ref int volume, ref int screen_lockunlock, ref int program_lockunlock, ref int screen_output_type, ref int screen_player_mode, byte[] screen_time, byte[] screen_addr, byte[] screen_customer_onoff, byte[] screen_language, byte[] screen_gps);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int get_sensor(byte[] ip, ushort port, string user_name, string user_pwd, byte[] datas, ref int datas_count);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int get_sensor_bus(byte[] ip, ushort port, string user_name, string user_pwd, byte[] sensor_bus);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int get_sensor_bus_dwhand(IntPtr dwhand, byte[] sensor_bus);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int get_sensor_dwhand(IntPtr dwhand, byte[] datas, ref int datas_count);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int get_ssid_list(string barcode, string pid, byte[] wifis);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int get_tls_param_dwhand(IntPtr dwhand, byte[] param);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int get_udp_stamp(string barcode, string pid, byte[] out_stamp, int is_comm = 0);
        [DllImport("kernel32.dll")]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int init_sdk();
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int install_font(byte[] ip, ushort port, string user_name, string user_pwd, IntPtr font, IntPtr tls_font, ref int min_waitTime, ref int max_waitTime);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int install_font_dwhand(IntPtr dwhand, IntPtr font, ref int min_waitTime, ref int max_waitTime);
        [DllImport("kernel32.dll")]
        public static extern IntPtr LoadLibrary(string lpFileName);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int lock_program(byte[] ip, ushort port, string user_name, string user_pwd, int programLock, string programName);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int lock_program_dwhand(IntPtr dwhand, int programLock, string programName);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int lock_screen(byte[] ip, ushort port, string user_name, string user_pwd, int screenLock);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int lock_screen_dwhand(IntPtr dwhand, int screenLock);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int make_program(IntPtr playlist, string tmp_path);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int net_getcode(byte[] ip, ushort port);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr net_login(byte[] ip, ushort port, string user_name, string user_pwd, int login_style = 0);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int net_logout(IntPtr post);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int only_clear_material_dwhand(IntPtr dwhand);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int play_audio(byte[] ip, ushort port, string user_name, string user_pwd, IntPtr audio, int loop_mode);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int play_bulletin(byte[] ip, ushort port, string user_name, string user_pwd, IntPtr bulletin_list);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int play_bulletin_dwhand(IntPtr dwhand, byte[] ip, ushort port, IntPtr bulletin_list);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int play_program(byte[] ip, ushort port, string user_name, string user_pwd, string src_path);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int play_program_dwhand(IntPtr dwhand, string src_path);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int query_bulletin(byte[] ip, ushort port, string user_name, string user_pwd, string names);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int query_bulletin_dwhand(IntPtr dwhand, string names);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int query_font(byte[] ip, ushort port, string user_name, string user_pwd, byte[] system_font, byte[] custom_font);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int query_font_dwhand(IntPtr dwhand, byte[] system_font, byte[] custom_font);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern void query_rate(IntPtr playlist, ref long total, ref long cur, ref int rate, ref int remainsec, ref int taskcount, ref int completecount);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int query_seeksensor(byte[] ip, ushort port, string user_name, string user_pwd, string sensor_bus, ref int min_waitTime, ref int max_waitTime);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int query_seeksensor_dwhand(IntPtr dwhand, string sensor_bus, ref int min_waitTime, ref int max_waitTime);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int query_wifi_status(string barcode, string pid, byte[] wifi_status);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int Refresh_CardInfo(byte[] ip, ushort port);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int release_sdk();
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int reset_password(byte[] ip, ushort port, string user_name, string user_pwd);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int reset_password_dwhand(IntPtr dwhand, string user_name);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int restart_app(byte[] ip, ushort port, string user_name, string user_pwd);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int restart_app_dwhand(IntPtr dwhand);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int restart_app_udp(string barcode, string pid, ref int min_waitTime, ref int max_waitTime);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int restart_boot(byte[] ip, ushort port, string user_name, string user_pwd);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int restart_boot_dwhand(IntPtr dwhand);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int save_dynamic(byte[] ip, ushort port, string user_name, string user_pwd, IntPtr dynamic_playlist);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int save_dynamic_dwhand(IntPtr dwhand, IntPtr dynamic_playlist);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int search_card(IntPtr datas, ref int data_count);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int search_cardRecv(IntPtr radios, byte[] datas, ref int data_count);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int search_cardSend(IntPtr radios);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int send_program(byte[] ip, ushort port, string user_name, string user_pwd, string tmp_path, IntPtr playlist, int send_style, ref long free_size, ref long total_size, int is_make = 0);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int send_program_dwhand(IntPtr dwhand, byte[] ip, string tmp_path, IntPtr playlist, int send_style, ref long free_size, ref long total_size, int is_make = 0);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int serial_screenonoff(int screen_onoff, byte[] json_data);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int set_ap_property(string barcode, string pid, string wifi_name, string wifi_pwd, byte[] wifi_ip);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int set_apn(byte[] ip, ushort port, string user_name, string user_pwd, string ppp_apn, string ppp_number, string ppp_username, string ppp_password);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int set_apn_dwhand(IntPtr dwhand, string ppp_apn, string ppp_number, string ppp_username, string ppp_password);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int set_bx_param(IntPtr dwhand, string param);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int set_fold_screen_size(byte[] ip, ushort port, string user_name, string user_pwd, int w, int h, int fold_type, int fold_count, int[] fold_h, int h_len, int[] fold_w, int w_len);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int set_fold_screen_size_dwhand(IntPtr dwhand, int w, int h, int fold_type, int fold_count, int[] fold_h, int h_len, int[] fold_w, int w_len);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int set_gp_mode(IntPtr dwhand, byte[] ip, ushort port, int gp_mode);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int set_gp_mode_dwhand(IntPtr dwhand, int gp_mode);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int set_jtc_mode(IntPtr dwhand, byte[] ip, ushort port, int jtc_mode, string jtc_protocol, string jtc_device_addr, ushort jtc_server_port, string porter_rate, int package_size);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int set_jtc_mode_dwhand(IntPtr dwhand, int jtc_mode, string jtc_protocol, string jtc_device_addr, string jtc_server_addr, ushort jtc_server_port, string porter_rate, int package_size);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int set_playlist_style(IntPtr playlist, int play_mode, int startH, int startM, int startS, int endH, int endM, int endS);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int set_relay_switch(byte[] ip, ushort port, string user_name, string user_pwd, int setOrCancel, int update_time, IntPtr sensors);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int set_relay_switch_dwhand(IntPtr dwhand, int setOrCancel, int update_time, IntPtr sensors);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int set_screen_auto_brightness(byte[] ip, ushort port, string user_name, string user_pwd, ushort[] brightness, int data_count, ushort[] sensor_brightness, int sensor_data_count, string sensor_addr);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int set_screen_auto_brightness_dwhand(IntPtr dwhand, ushort[] brightness, int data_count, ushort[] sensor_brightness, int sensor_data_count, string sensor_addr);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int set_screen_auto_ip(string barcode, string pid, ref int min_waitTime, ref int max_waitTime);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int set_screen_auto_wifi_ip(string barcode, string pid, ref int min_waitTime, ref int max_waitTime);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int set_screen_barcode(byte[] ip, ushort port, string user_name, string user_pwd, string barcode);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int set_screen_barcode_dwhand(IntPtr dwhand, string barcode);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int set_screen_brightness(byte[] ip, ushort port, string user_name, string user_pwd, int brightness);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int set_screen_brightness_dwhand(IntPtr dwhand, int brightness);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int set_screen_cus_brightness(byte[] ip, ushort port, string user_name, string user_pwd, ushort[] brightness, int data_count);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int set_screen_cus_brightness_dwhand(IntPtr dwhand, ushort[] brightness, int data_count);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int set_screen_cus_turnonoff(byte[] ip, ushort port, string user_name, string user_pwd, IntPtr trunonoff);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int set_screen_cus_turnonoff_dwhand(IntPtr dwhand, IntPtr trunonoff);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int set_screen_install_address(string barcode, string pid, string install_address);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int set_screen_ip(string barcode, string pid, byte[] ip, byte[] submark, byte[] gateway, byte[] dns_server, ref int min_waitTime, ref int max_waitTime);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int set_screen_ip_dwhand(IntPtr dwhand, byte[] ip, byte[] submark, byte[] gateway, byte[] dns_server, ref int min_waitTime, ref int max_waitTime);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int set_screen_ip_flag(byte[] ip, ushort port, string user_name, string user_pwd, int flag);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int set_screen_ip_flag_dwhand(IntPtr dwhand, int flag);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int set_screen_language(byte[] ip, ushort port, string user_name, string user_pwd, string controller_language);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int set_screen_language_dwhand(IntPtr dwhand, string controller_language);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int set_screen_led_flag(byte[] ip, ushort port, string user_name, string user_pwd, int flag);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int set_screen_led_flag_dwhand(IntPtr dwhand, int flag);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int set_screen_logo(byte[] ip, ushort port, string user_name, string user_pwd, int logoFlag, string logo_layout, int w, int h, string path);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int set_screen_logo_dwhand(IntPtr dwhand, byte[] ip, ushort port, int logoFlag, string logo_layout, int w, int h, string path);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int set_screen_mac(string barcode, string pid, string mac, ref int min_waitTime, ref int max_waitTime);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int set_screen_name(byte[] ip, ushort port, string user_name, string user_pwd, string controller_name);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int set_screen_name_dwhand(IntPtr dwhand, string controller_name);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int set_screen_output_type(byte[] ip, ushort port, string user_name, string user_pwd, int output_type);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int set_screen_output_type_dwhand(IntPtr dwhand, int output_type);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int set_screen_player_type(byte[] ip, ushort port, string user_name, string user_pwd, int player_type);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int set_screen_player_type_dwhand(IntPtr dwhand, int player_type);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int set_screen_server(string barcode, string pid, byte[] ip, ushort port, int server_mode);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int set_screen_server_dwhand(IntPtr dwhand, byte[] ip, ushort port, int server_mode);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int set_screen_shadow_net_ip(string barcode, string pid, byte[] ip, byte[] submark);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int set_screen_size(byte[] ip, ushort port, string user_name, string user_pwd, int w, int h, int screenrotation);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int set_screen_size_dwhand(IntPtr dwhand, int w, int h, int screenrotation);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int set_screen_storage_media(byte[] ip, ushort port, string user_name, string user_pwd, string storage_media);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int set_screen_storage_media_dwhand(IntPtr dwhand, string storage_media);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int set_screen_timezone(byte[] ip, ushort port, string user_name, string user_pwd, int timezoneflag, string timezone, string timezone_server, int timezone_interval);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int set_screen_timezone_dwhand(IntPtr dwhand, int timezoneflag, string timezone, string timezone_server, int timezone_interval);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int set_screen_turnonoff(byte[] ip, ushort port, string user_name, string user_pwd, int turnonoff);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int set_screen_turnonoff_dwhand(IntPtr dwhand, int turnonoff);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int set_screen_volumn(byte[] ip, ushort port, string user_name, string user_pwd, int volumn);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int set_screen_volumn_dwhand(IntPtr dwhand, int volumn);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int set_screen_wifi_ip(string barcode, string pid, byte[] ip, byte[] submark, byte[] gateway, byte[] dns_server, ref int min_waitTime, ref int max_waitTime);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int set_screen_wifi_mac(string barcode, string pid, string mac, ref int min_waitTime, ref int max_waitTime);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int set_screenonoff_switch(byte[] ip, ushort port, string user_name, string user_pwd, string relay_addr, int relay_type, int relay_switch);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int set_screenonoff_switch_dwhand(IntPtr dwhand, string relay_addr, int relay_type, int relay_switch);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int set_serial(byte[] ip, ushort port, string user_name, string user_pwd, string serial_mode, string baudrate, int timeout, string slave_device);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int set_serial_dwhand(IntPtr dwhand, string serial_mode, string baudrate, int timeout, string slave_device);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int set_screen_prompt_flag(byte[] ip, ushort port, string user_name, string user_pwd, string prompt_flag);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int set_screen_prompt_flag_dwhand(IntPtr dwhand, string prompt_flag);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern void set_ssl_flag(int flag, string ssl_file_path);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int set_tls(byte[] ip, ushort port, string src_file, IntPtr tls_infos, string tls_certificate, string fingerprint, string add_certificate_type);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int set_tls_dwhand(IntPtr dwhand, byte[] ip, ushort port, string src_file, IntPtr tls_infos, string fingerprint, string tls_certificate, string add_certificate_type);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int set_udp_stamp(string out_stamp, string sign);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int set_web_user_id(string barcode, string pid, string user_id);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int set_xser_cmd(IntPtr post, int cmd, string data, int len, byte[] recv_data, ref int recv_cmd, ref int recv_status, ref int recv_len);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr Start_Native_Server(int port);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr Start_Ssl_Server(int port, string certpath, string keypath);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int stop_play_audio(byte[] ip, ushort port, string user_name, string user_pwd);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int stop_play_bulletin(byte[] ip, ushort port, string user_name, string user_pwd);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int stop_play_bulletin_dwhand(IntPtr dwhand);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int stop_play_insert_list(byte[] ip, ushort port, string user_name, string user_pwd);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int stop_play_insert_list_dwhand(IntPtr dwhand);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int stop_play_program(byte[] ip, ushort port, string user_name, string user_pwd);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int stop_play_program_dwhand(IntPtr dwhand);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int Stop_Server(IntPtr iServer);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int update_dynamic(byte[] ip, ushort port, string user_name, string user_pwd, IntPtr dynamic_playlist, string immediately_play, int conver, int onlyUpdate);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int update_dynamic_dwhand(IntPtr dwhand, IntPtr dynamic_playlist, string immediately_play, int conver, int onlyUpdate);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int update_dynamic_small(byte[] ip, ushort port, string user_name, string user_pwd, IntPtr dynamic_playlist, string immediately_play, int conver, int onlyUpdate);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int update_dynamic_small_dwhand(IntPtr dwhand, IntPtr dynamic_playlist, string immediately_play, int conver, int onlyUpdate);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int update_dynamic_unit(byte[] ip, ushort port, string user_name, string user_pwd, IntPtr dynamic_playlist);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int update_dynamic_unit_dwhand(IntPtr dwhand, IntPtr dynamic_playlist);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int update_dynamic_unit_small(byte[] ip, ushort port, string user_name, string user_pwd, IntPtr dynamic_playlist);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int update_dynamic_unit_small_dwhand(IntPtr dwhand, IntPtr dynamic_playlist);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int update_firmware(byte[] ip, ushort port, string user_name, string user_pwd, string firmware_path, IntPtr tls_infos);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int update_firmware_dwhand(IntPtr dwhand, byte[] ip, ushort port, string firmware_path, IntPtr tls_infos);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int upload_audio_file(byte[] ip, ushort port, string user_name, string user_pwd, IntPtr audio);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int upload_file(byte[] ip, ushort port, string user_name, string user_pwd, string src_path, string dest_file_name, IntPtr tls_infos);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int upload_file_dwhand(IntPtr dwhand, byte[] ip, ushort port, string src_path, string dest_file_name, IntPtr tls_infos);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int delete_file(byte[] ip, ushort port, string user_name, string user_pwd, string delete_files);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int verify_switch(byte[] ip, ushort port, int flag, string tls_content, string fingerprint);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int verify_switch_dwhand(IntPtr dwhand, int flag, string tls_content, string fingerprint);

        #region proxy
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int set_proxy(byte[] ip, ushort port, string user_name, string user_pwd, string proxy_text);
        [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
        public static extern int set_proxy_dwhand(IntPtr dwhand, string proxy_text);
        #endregion


        //网络
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct BroadCast2
        {
            public int is_dhcp;
            public int wifi_is_dhcp;
            public int output_type;
            public int port;
            public int screen_w;
            public int screen_h;
            public int screen_volume;
            public int screen_brigtness;
            public int screen_brigtness_mode;
            public int screen_rotation;
            public int screen_file_verify_switch;
            public short screen_type;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public byte[] storagemedia;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 34)]
            public byte[] barcode;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
            public byte[] ip;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
            public byte[] source_ip;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
            public byte[] sub_mark;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
            public byte[] gateway;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
            public byte[] mac;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
            public byte[] local_ip;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 66)]
            public byte[] pid;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
            public byte[] local_net;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
            public byte[] ap_wifi_ip;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
            public byte[] wifi_ip;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
            public byte[] wifi_sub_mark;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
            public byte[] wifi_gateway;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
            public byte[] dns_server;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
            public byte[] network_device;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
            public byte[] network_mode;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public byte[] controller_name;
        }
        //屏参
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ControllerInfo
        {
            public int is_dhcp;
            public int wifi_is_dhcp;
            public int output_type;
            public int port;
            public int screen_w;
            public int screen_h;
            public int screen_volume;
            public int screen_brigtness;
            public int screen_brigtness_mode;
            public int fold_type;
            public int fold_count;
            public int logic_width;
            public int logic_height;
            public int screen_rotation;
            public int flag;
            public int screen_file_verify_switch;
            public ushort screen_type;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public byte[] storagemedia;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 34)]
            public byte[] barcode;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
            public byte[] ip;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
            public byte[] source_ip;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
            public byte[] sub_mark;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
            public byte[] gateway;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
            public byte[] mac;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
            public byte[] local_ip;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 66)]
            public byte[] pid;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
            public byte[] local_net;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
            public byte[] ap_wifi_ip;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
            public byte[] wifi_ip;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
            public byte[] wifi_sub_mark;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
            public byte[] wifi_gateway;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
            public byte[] dns_server;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
            public byte[] network_device;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
            public byte[] network_mode;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public byte[] controller_name;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public byte[] fold_width;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public byte[] fold_height;
        }


        public struct TLSInfo
        {
            public int offset;  //签名起始位置
            public int len;  //签名长度
            public IntPtr fingerprint;  //证书指纹
            public IntPtr sign;   //签名后的内容
            public IntPtr digest;   //签名认证方式（SHA1/MD5，暂时只支持SHA1）
        }

        public struct ControllerSensor
        {
            public float sensor_value;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
            public byte[] sensor_sequence;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
            public byte[] sensor_state;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
            public byte[] sensor_address;
        }
    }
}
