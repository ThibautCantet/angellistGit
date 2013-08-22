namespace AntVoice.Common.DataAccess.FtpClient
{
    using AntVoice.Platform.Tools.Logging;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;

    public class FtpContext
    {
        private string _ftpUrl;
        private string _username;
        private string _password;

        public FtpContext(string ftpUrl, string username, string password)
        {
            _ftpUrl = ftpUrl;
            _username = username;
            _password = password;
        }

        public byte[] GetFileData(string file, string folder = "")
        {
            byte[] result;
            string url = GetUrl(file, folder);
            FtpWebRequest ftp = GetClient(url, WebRequestMethods.Ftp.DownloadFile);

            Log.Debug("FtpContext.GetFile", "Downloading file", file, folder);
            using (FtpWebResponse response = ftp.GetResponse() as FtpWebResponse)
            {
                using (Stream stream = response.GetResponseStream())
                {
                    result = ReadFully(stream);
                    Log.Debug("FtpContext.GetFile", "File downloaded");
                }
            }

            return result;
        }

        public void SaveFile(byte[] fileData, string fileName, string folder = "")
        {
            Log.Debug("FtpContext.SaveFile", "Sending over FTP...", fileName);
            FtpWebRequest request = GetClient(GetUrl(fileName,folder), WebRequestMethods.Ftp.UploadFile);

            using (Stream writer = request.GetRequestStream())
            {
                writer.Write(fileData, 0, fileData.Length);
            }

            using (FtpWebResponse ftpResponse = (FtpWebResponse)request.GetResponse())
            {
                Log.Debug("FtpContext.SaveFile", "File tranferred successfully.", fileName);
            }
        }

        public void DeleteFile(string fileName, string folder = "")
        {
            string url = GetUrl(fileName, folder);
            FtpWebRequest ftp = GetClient(url, WebRequestMethods.Ftp.DeleteFile);

            using (ftp.GetResponse())
            {
                Log.Debug("FtpContext.DeleteFile", "File deleted", fileName);
            }
        }

        public List<string> GetFileList(string directory = "")
        {
            List<string> result = new List<string>();
            FtpWebRequest ftp = GetClient(GetUrl(directory), WebRequestMethods.Ftp.ListDirectory);

            Log.Debug("FtpContext.GetFileList", "Getting all files from directory", GetUrl(directory));
            using (FtpWebResponse response = ftp.GetResponse() as FtpWebResponse)
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    string line = reader.ReadLine();

                    while (!string.IsNullOrEmpty(line))
                    {
                        if (line.LastIndexOf("/") != -1)
                        {
                            line = line.Substring(line.LastIndexOf("/") + 1);
                        }

                        result.Add(line);
                        line = reader.ReadLine();
                    }
                }
            }

            Log.Debug("FtpContext.GetFileList", "Listed files", result.Count);
            return result;
        }

        private FtpWebRequest GetClient(string url, string method)
        {
            FtpWebRequest request = WebRequest.Create(url) as FtpWebRequest;
            request.Method = method;
            request.Credentials = new NetworkCredential(_username, _password);
            request.KeepAlive = true;
            request.UseBinary = true;
            request.UsePassive = true;
            request.Proxy = null;

            return request;
        }

        private string GetUrl(string folder)
        {
            return _ftpUrl + "/" + folder;
        }

        private string GetUrl(string file, string folder = "")
        {
            if (folder == "")
            {
                return _ftpUrl + "/" + file;
            }
            else
            {
                return _ftpUrl + "/" + folder + "/" + file;
            }
        }


        private static byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
    }
}
