using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.IO;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Crypto;
using System.Runtime.InteropServices;

namespace BX_Y_CSharp_SDK
{
    class TLSHelper
    {
        /// <summary>
        /// 获取MD5
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public string OnGetMd5(string fileName, out long size)
        {
            try
            {
                using (FileStream tmp_file = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    size = tmp_file.Length;
                    MD5 tmp_md5 = new MD5CryptoServiceProvider();
                    byte[] retVal = tmp_md5.ComputeHash(tmp_file);

                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < retVal.Length; i++)
                    {
                        sb.Append(retVal[i].ToString("x2"));
                    }
                    return sb.ToString();
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// 写文件
        /// </summary>
        /// <param name="file"></param>
        /// <param name="contents"></param>
        /// <returns></returns>
        public bool OnWriteFile(string file, string[] contents)
        {
            try
            {
                if (File.Exists(file))
                {
                    using (StreamWriter sw = File.AppendText(file))
                    {
                        foreach (string content in contents)
                        {
                            sw.WriteLine(content);
                        }
                        sw.Flush();
                    }
                }
                else
                {
                    using (FileStream tmp_fs = new FileStream(file, FileMode.Create, FileAccess.ReadWrite))
                    {
                        TextWriter tmp_tw = new StreamWriter(tmp_fs);

                        foreach (string content in contents)
                        {
                            tmp_tw.WriteLine(content);
                        }
                        tmp_tw.Flush();
                        tmp_fs.Flush();
                        tmp_tw.Dispose();
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public LedYNetSdk.TLSInfo OnCreateTLS(int offset, int len, string fingerprint, string sign, string digest = "sha1")
        {
            LedYNetSdk.TLSInfo info = new LedYNetSdk.TLSInfo();
            info.offset = offset;
            info.len = len;
            info.fingerprint = OnGetPtr(fingerprint);
            info.sign = OnGetPtr(sign);
            info.digest = OnGetPtr(digest);
            return info;
        }

        /// <summary>
        /// 签名文件
        /// </summary>
        /// <param name="filePath">待签名文件路径</param>
        /// <param name="privateKeyFilePath">私钥路径</param>
        /// <param name="privatePwd">私钥密码</param>
        /// <param name="lenRandom">签名长度</param>
        /// <param name="startRandom">签名起始位置</param>
        /// <param name="signData">签名后的内容</param>
        /// <param name="check_all">是否签名整个文件</param>
        /// <param name="check_input_param">是否签名传入的参数</param>
        /// <returns></returns>
        public int OnVerifyFileData(string filePath, string privateKeyFilePath, string privatePwd, out int lenRandom, out int startRandom, out string signData, bool check_all = false, bool check_input_param = false)
        {
            lenRandom = 0;
            startRandom = 0;
            signData = "";
            try
            {
                if (check_input_param)
                {
                    X509Certificate2 x509Certificate2 = OnVerifyPrivateKeyPwd(privateKeyFilePath, privatePwd);
                    if (x509Certificate2 != null)
                    {
                        byte[] randomData = System.Text.Encoding.Default.GetBytes(filePath);
                        return OnSignatureFormatter(x509Certificate2, randomData, ref signData) ? 0 : 1;
                    }
                    else
                    {
                        return 10;
                    }
                }
                else
                {
                    using (FileStream fsRead = new FileStream(filePath, FileMode.Open, System.IO.FileAccess.Read, FileShare.Read))
                    {
                        int totalLen = (int)fsRead.Length;
                        Random ran = new Random();
                        int startIndex = ran.Next(1, totalLen);
                        int endIndex = Math.Min(totalLen, startIndex + (8 * 1024 * 1024));
                        //随机值，签名长度，最大为文件长度(20200107如果长度随机值>8M，需要把长度限制在8M以内做签名)
                        lenRandom = ran.Next(1, totalLen);
                        //随机值，起始值，最大值为文件总长度 - 签名长度
                        startRandom = ran.Next(0, totalLen - lenRandom - 1);
                        startRandom = startIndex;
                        lenRandom = endIndex - startIndex;
                        if (check_all)
                        {
                            lenRandom = totalLen;
                            startRandom = 0;
                        }
                        byte[] randomData = new byte[lenRandom];
                        fsRead.Seek(startRandom, SeekOrigin.Begin);
                        //while (true)
                        //{
                        int r = fsRead.Read(randomData, 0, randomData.Length);
                        //    if (r <= 0)
                        //    {
                        //        break;
                        //    }
                        //}
                        X509Certificate2 x509Certificate2 = OnVerifyPrivateKeyPwd(privateKeyFilePath, privatePwd);
                        if (x509Certificate2 != null)
                        {
                            return OnSignatureFormatter(x509Certificate2, randomData, ref signData) ? 0 : 1;
                        }
                        else
                        {
                            return 10;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return -1;
            }
        }

        public string OnExport(string materialCeritificatePath, string materialCeritificatePwd, string certFilePath)
        {
            try
            {
                string privateKey = "";
                DateTime beginTime, endTime;
                ExportToCerFile(materialCeritificatePath, certFilePath, materialCeritificatePwd, out beginTime, out endTime);
                if (File.Exists(certFilePath))
                {
                    System.Security.Cryptography.X509Certificates.X509Certificate cert = new System.Security.Cryptography.X509Certificates.X509Certificate(certFilePath, materialCeritificatePwd);
                    string certResult = cert.GetCertHashString();
                    certResult = certResult.ToUpper();
                    int initLen = certResult.Length;
                    int len = initLen / 2 - 1;
                    for (int i = len * 2; i > 0; i = i - 2)
                    {
                        certResult = certResult.Insert(i, ":");
                    }
                    privateKey = certResult;
                }
                return privateKey;
            }
            catch (Exception)
            {

                throw;
            }
        }

        #region 验证私钥有效性（密码)
        /// <summary>
        /// 验证成功返回证书类；失败返回空
        /// </summary>
        /// <param name="strKeyPrivate"></param>
        /// <param name="privatePwd"></param>
        /// <returns></returns>
        public X509Certificate2 OnVerifyPrivateKeyPwd(string strKeyPrivate, string privatePwd)
        {
            try
            {
                X509Certificate2 c2 = new X509Certificate2(strKeyPrivate, privatePwd);
                return c2;
            }
            catch (System.Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// 签名和验证
        /// </summary>
        /// <param name="c2">验证成功的证书类</param>
        /// <param name="HashbyteSignature">待签名数据</param>
        /// <param name="strEncryptedSignatureData">签名后的内容</param>
        /// <returns>验证结果</returns>
        public bool OnSignatureFormatter(X509Certificate2 c2, byte[] HashbyteSignature, ref string strEncryptedSignatureData)
        {
            try
            {
                RSACryptoServiceProvider RSAalg = c2.PrivateKey as RSACryptoServiceProvider;
                //签名
                byte[] encryptedData = RSAalg.SignData(HashbyteSignature, new SHA1CryptoServiceProvider());
                strEncryptedSignatureData = Convert.ToBase64String(encryptedData);
                //验证
                return RSAalg.VerifyData(HashbyteSignature, new SHA1CryptoServiceProvider(), encryptedData);
            }
            catch (Exception ex) 
            { return false; }
        }

        /// <summary>
        /// 从证书库中导出公钥文件
        /// </summary>
        /// <param name="subjectName">证书名字</param>
        /// <param name="cerFileName">存放公钥的文件路径</param>
        public void ExportToCerFile(string subjectName, string cerFileName, string certPwd, out DateTime beginTime, out DateTime endTime)
        {
            try
            {
                X509Certificate2 c2 = new X509Certificate2(subjectName, certPwd);
                beginTime = c2.NotBefore;
                endTime = c2.NotAfter;
                byte[] bts = c2.RawData;
                string pemStr = "-----BEGIN CERTIFICATE-----" + "\r\n" + Convert.ToBase64String(bts) + "\r\n" + "-----END CERTIFICATE-----";
                Console.WriteLine(pemStr);

                using (FileStream tmp_fs = new FileStream(cerFileName, FileMode.Create, FileAccess.ReadWrite))
                {
                    TextWriter tmp_tw = new StreamWriter(tmp_fs);

                    tmp_tw.WriteLine("-----BEGIN CERTIFICATE-----");
                    tmp_tw.WriteLine(Convert.ToBase64String(bts));
                    tmp_tw.WriteLine("-----END CERTIFICATE-----");
                    tmp_tw.Flush();
                    tmp_fs.Flush();
                    tmp_tw.Dispose();
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public void ExportPrivateKey(string subjectName, string cerFileName, string certPwd)
        {
            try
            {
                var certificate = new  X509Certificate2(subjectName, certPwd, X509KeyStorageFlags.Exportable);
                var rsa = RSA.Create();
                rsa.FromXmlString(certificate.PrivateKey.ToXmlString(true));

                var bcKeyPair = DotNetUtilities.GetRsaKeyPair(rsa);
                var pkcs8Gen = new Pkcs8Generator(bcKeyPair.Private);
                var pemObj = pkcs8Gen.Generate();
                var pkcs8Out = new StreamWriter(cerFileName, false);
                var pemWriter = new PemWriter(pkcs8Out);
                pemWriter.WriteObject(pemObj);
                pkcs8Out.Close();
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        public void GetPrivateKey(string subjectName, string cerFileName, string certPwd)
        {
            try
            {
                using (var reader = File.OpenText(subjectName))
                {
                    var keyPair = (AsymmetricCipherKeyPair)new Org.BouncyCastle.OpenSsl.PemReader(reader).ReadObject();
                    //var keyPair = (RsaPrivateCrtKeyParameters)new Org.BouncyCastle.OpenSsl.PemReader(reader).ReadObject();
                    AsymmetricKeyParameter key = keyPair.Private;
                    Console.WriteLine(keyPair.ToString());
                }
                X509Certificate2 certificate = new X509Certificate2(subjectName, certPwd, X509KeyStorageFlags.Exportable);
                var v = certificate.PrivateKey as AsymmetricAlgorithm;
                Console.WriteLine(v.SignatureAlgorithm);
                Console.WriteLine(v.ToXmlString(true));
            }
            catch (System.Exception ex)
            {
            	
            }
        }

        #endregion

        private IntPtr OnGetPtr(string str)
        {
            //byte[] bgImages = System.Text.Encoding.Unicode.GetBytes(unit.bg_image);
            // //申请非拖管空间   
            //IntPtr m_ptr = Marshal.AllocHGlobal(bgImages.Length);  

            ////给非拖管空间清0 
            //Byte[] btZero = new Byte[bgImages .Length+ 1]; //一定要加1,否则后面是乱码，原因未找到   
            //Marshal.Copy(btZero, 0, m_ptr, btZero.Length);    
            ////给指针指向的空间赋值   
            //Marshal.Copy(bgImages, 0, m_ptr, bgImages.Length);
            byte[] bts = System.Text.Encoding.Unicode.GetBytes(str);
            int a = bts.Count();
            //申请非拖管空间   
            IntPtr m_ptr = Marshal.AllocHGlobal(bts.Length + (str.Length == 0 ? 2 : a / str.Length));
            //给非拖管空间清0 
            Byte[] btZero = new Byte[bts.Length + (str.Length == 0 ? 2 : a / str.Length)]; //一定要加1,否则后面是乱码，原因未找到   
            Marshal.Copy(btZero, 0, m_ptr, btZero.Length);
            //给指针指向的空间赋值   
            Marshal.Copy(bts, 0, m_ptr, bts.Length);
            return m_ptr;
        }
    }
}
