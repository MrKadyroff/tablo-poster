using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BX_Y_CSharp_SDK
{
    public class Errer
    {
        #region
        public static string GetError(int err)
        {
            string errer = "";
            switch (err)
            {
                case bxyq_err.ERR_HTTP_REQUEST_EMPTY_HTTP:
                    errer = "请求参数为空！";
                    break;
                case bxyq_err.ERR_HTTP_REQUEST_METHOD_HTTP:
                    errer = "请求方法错误！";
                    break;
                case bxyq_err.ERR_PROTOCOL_PARSE:
                    errer = "协议解析错误！";
                    break;
                case bxyq_err.ERR_PROTOCOL_NAME:
                    errer = "协议名错误！";
                    break;
                case bxyq_err.ERR_PROTOCOL_VERSION:
                    errer = "协议版本错误！";
                    break;
                case bxyq_err.ERR_PID_PID:
                    errer = "PID错误！";
                    break;
                case bxyq_err.ERR_BARCODE:
                    errer = "控制器 barcode 错误！";
                    break;
                case bxyq_err.ERR_HTTP_REQUEST_PARAMETER_KEY:
                    errer = "协议请求报文键错误！";
                    break;
                case bxyq_err.ERR_CONFIG_PARSE:
                    errer = "配置文件解析错误！";
                    break;
                case bxyq_err.ERR_PERMISSION:
                    errer = "权限不够！";
                    break;
                case bxyq_err.ERR_INVALID_AUTHENTICATION:
                    errer = "用户认证失效！";
                    break;
                case bxyq_err.ERR_ACCESS_VIOLATION:
                    errer = "非法访问！";
                    break;
                case bxyq_err.ERR_IO_READ_WRITE:
                    errer = "输入输出操作错误！";
                    break;
                case bxyq_err.ERR_COMMAND_PARAMETER_KEY:
                    errer = "请求命令参数错误！";
                    break;
                case bxyq_err.ERR_COMMAND_CALL:
                    errer = "请求命令调用错误！";
                    break;
                case bxyq_err.ERR_COMMAND_PROCESS:
                    errer = "请求命令处理错误！";
                    break;
                case bxyq_err.ERR_COMMAND_NOT_EXISTS:
                    errer = "请求命令不存在！";
                    break;
                case bxyq_err.ERR_COMMAND_PARAMETER_EMPTY:
                    errer = "请求命令参数为空！";
                    break;
                case bxyq_err.ERR_COMMAND_EXECUTE:
                    errer = "系统命令执行错误！";
                    break;
                case bxyq_err.ERR_COMMAND_PARAMETER_VALUE:
                    errer = "请求命令参数值错误！";
                    break;
                case bxyq_err.ERR_USER_NOT_EXISTS:
                    errer = "用户不存在！";
                    break;
                case bxyq_err.ERR_USER_PASSWORD:
                    errer = "密码错误！";
                    break;
                case bxyq_err.ERR_STORAGE_MEDIA_NOT_EXISTS:
                    errer = "媒体存储介质不存在！";
                    break;
                case bxyq_err.ERR_FILE_PATH:
                    errer = "文件路径错误！";
                    break;
                case bxyq_err.ERR_MAC_FORMAT_MAC:
                    errer = "地址格式错误！";
                    break;
                case bxyq_err.ERR_UDP_TRANSMIT_UDP:
                    errer = "转发错误！";
                    break;
                case bxyq_err.ERR_VERIFICATION_CODE:
                    errer = "验证码错误！";
                    break;
                case bxyq_err.ERR_NO_FIRMWARE:
                    errer = "固件不存在！";
                    break;
                case bxyq_err.ERR_USER_WORK_PATH:
                    errer = "用户工作目录创建失败！";
                    break;
                case bxyq_err.ERR_PLAYER_CMD:
                    errer = "播放器执行指令出错！";
                    break;
                case bxyq_err.ERR_GET_WIFI_LIST:
                    errer = "获取热点列表失败！";
                    break;
                case bxyq_err.ERR_WIFI_CONNECT_TIMEOUT:
                    errer = "热点连接超时！";
                    break;
                case bxyq_err.ERR_HOTSPOT_NOT_FOUND:
                    errer = "热点未找到！";
                    break;
                case bxyq_err.ERR_WIFI_PASSWORD:
                    errer = "热点密码错误！";
                    break;
                case bxyq_err.ERR_NETWORK_RESTART:
                    errer = "网络正在重启中！";
                    break;
                case bxyq_err.ERR_Unknow:
                    errer = "通讯失败，请检查网络！";
                    break;
                default:
                    errer = "未知错误！";
                    break;
            }
            return errer;
        }
        public class bxyq_err
        {
            public const int ERR_Unknow = -1;
            public const int ERR_HTTP_REQUEST_EMPTY_HTTP = 0x01;
            public const int ERR_HTTP_REQUEST_METHOD_HTTP = 0x02;
            public const int ERR_PROTOCOL_PARSE = 0x03;
            public const int ERR_PROTOCOL_NAME = 0x04;
            public const int ERR_PROTOCOL_VERSION = 0x05;
            public const int ERR_PID_PID = 0x06;
            public const int ERR_BARCODE = 0x07;
            public const int ERR_HTTP_REQUEST_PARAMETER_KEY = 0x08;
            public const int ERR_CONFIG_PARSE = 0x09;
            public const int ERR_PERMISSION = 0x0a;
            public const int ERR_INVALID_AUTHENTICATION = 0x0b;
            public const int ERR_ACCESS_VIOLATION = 0x0c;
            public const int ERR_IO_READ_WRITE = 0x0d;
            public const int ERR_COMMAND_PARAMETER_KEY = 0x0e;
            public const int ERR_COMMAND_CALL = 0x0f;
            public const int ERR_COMMAND_PROCESS = 0x10;
            public const int ERR_COMMAND_NOT_EXISTS = 0x11;
            public const int ERR_COMMAND_PARAMETER_EMPTY = 0x12;
            public const int ERR_COMMAND_EXECUTE = 0x13;
            public const int ERR_COMMAND_PARAMETER_VALUE = 0x14;
            public const int ERR_USER_NOT_EXISTS = 0x15;
            public const int ERR_USER_PASSWORD = 0x16;
            public const int ERR_STORAGE_MEDIA_NOT_EXISTS = 0x17;
            public const int ERR_FILE_PATH = 0x18;
            public const int ERR_MAC_FORMAT_MAC = 0x19;
            public const int ERR_UDP_TRANSMIT_UDP = 0x1a;
            public const int ERR_VERIFICATION_CODE = 0x1b;
            public const int ERR_NO_FIRMWARE = 0x1c;
            public const int ERR_USER_WORK_PATH = 0x1d;
            public const int ERR_PLAYER_CMD = 0x1e;
            public const int ERR_GET_WIFI_LIST = 0x1f;
            public const int ERR_WIFI_CONNECT_TIMEOUT = 0x20;
            public const int ERR_HOTSPOT_NOT_FOUND = 0x21;
            public const int ERR_WIFI_PASSWORD = 0x22;
            public const int ERR_NETWORK_RESTART = 0x23;
        }
        #endregion
    }
}
