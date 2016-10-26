using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FTPDemoLibrary
{
    public class FTPConnect
    {
        #region Initialization
        /// <summary>
        /// Initializes a new instance of the <see cref="FTPConnect"/> class.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="user"></param>
        /// <param name="password"></param>
        public FTPConnect(string host, string user, string password)
        {
            this.host = host;
            this.user = user;
            this.password = password;
        }
        #endregion

        #region Properties
        public string host = null;
        public string user = null;
        private string password = null;

        private FtpWebRequest ftpRequest = null;
        private FtpWebResponse ftpResponse = null;
        private Stream ftpStream = null;
        private int bufferSize = 2048;
        #endregion

        #region Methods
        /// <summary>
        /// Upload file from local address to remote address by FTP.
        /// </summary>
        /// <param name="remoteFile"></param>
        /// <param name="localFile"></param>
        public void upload(string remoteFile, string localFile)
        {
            try
            {
                /* Open file to read */
                FileInfo objLocalFile = new FileInfo(localFile);

                /* Creat an FTP Request. */
                ftpRequest = (FtpWebRequest)FtpWebRequest.Create(host + "/" + remoteFile);

                /* Log in to the FTP server with the User Name and Password provided. */
                ftpRequest.Credentials = new NetworkCredential(user, password);

                /* When is doubt, use these option. */
                ftpRequest.UseBinary = true; // Value that specifies the data type for file transfer.
                ftpRequest.UsePassive = true; // Behavior of a client application's data transfer process.
                ftpRequest.KeepAlive = true; /* Value that specifies whether the control connection to the FTP
                                           server is closed after the request completes. */

                /* Set Time out for request upload. */
                ftpRequest.Timeout = 600000 * 3;
                ftpRequest.ContentLength = objLocalFile.Length;

                /* Specifies the Type of FTP request. */
                ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;

                /* Establish Return Communication with the FTP server. */
                ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();

                /* Get the FTP server's response stream. */
                ftpStream = ftpRequest.GetRequestStream();

                /* Open File Stream to Read the Uploaded File. */
                FileStream remoteFileStream = objLocalFile.OpenRead();
                
                /* Buffer for the Downloaded File. */
                byte[] byteBuffer = new byte[bufferSize];
                int bytesSent = remoteFileStream.Read(byteBuffer, 0, bufferSize);

                /* Upload file by sending the Buffered Data until the Transfer is complete. */
                try
                {
                    while (bytesSent != 0)
                    {
                        ftpStream.Write(byteBuffer, 0, bytesSent);
                        bytesSent = remoteFileStream.Read(byteBuffer, 0, bufferSize);
                    }

                    Console.WriteLine("Success!");
                }
                catch(Exception ex)
                {
                    Console.WriteLine("Error occurs when upload file: " + ex.ToString());
                    Console.WriteLine("Unsuccess!");
                }

                /* Resource cleanup. */
                ftpResponse.Close();
                remoteFileStream.Close();
                ftpStream.Close();
                ftpRequest = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occurs: {0}", ex.ToString());
            }
        }

        /// <summary>
        /// Gets content of remote file.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public string getContent(string fileName)
        {
            string contentFile = null; // contain content of remote file.
            try
            {
                /* Gets size of remote File. */
                int bufferSizeFile = getSizeFile(fileName);
            
                /* Creat an FTP request. */
                ftpRequest = (FtpWebRequest)FtpWebRequest.Create(host + "/" + fileName);

                /* Specifies the Type of FTP request. */

                /* Log in to FTP server with the User Name and Password provided. */
                ftpRequest.Credentials = new NetworkCredential(user, password);

                /* When is doubt, use these option. */
                ftpRequest.UseBinary = true; // Value that specifies the data type for file transfer.
                ftpRequest.UsePassive = true; // Behavior of a client application's data transfer process.
                ftpRequest.KeepAlive = true; /* Value that specifies whether the control connection to the FTP
                                           server is closed after the request completes. */

                /* Establish return communication with the FTP server. */
                ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
                
                /* Get the FTP Server's Response Stream. */
                ftpStream = ftpResponse.GetResponseStream();

                /* Buffer for the download data. */
                byte[] byteBuffer = new byte[bufferSizeFile];
                int bytesRead = ftpStream.Read(byteBuffer, 0, bufferSizeFile);
                
                /* Download the file by writting the buffered data until the transfer is complete. */
                try
                {
                    while(bytesRead > 0)
                    {
                        contentFile = System.Text.Encoding.UTF8.GetString(byteBuffer);
                        bytesRead = ftpStream.Read(byteBuffer, 0, bufferSizeFile);
                    }
                    Console.WriteLine(contentFile);
                    Console.WriteLine("Get Content of File successes!");
                }
                catch(Exception ex)
                {
                    Console.WriteLine("Un Success!");
                    Console.WriteLine("Error in get content of File: {0}", ex.ToString());
                }

                /* Resource cleanup. */
                ftpResponse.Close();
                ftpStream.Close();
                ftpRequest = null;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error: {0}", ex.ToString());
            }

            return contentFile;
        }

        /// <summary>
        /// Download the remote file.
        /// </summary>
        /// <param name="remoteFile"></param>
        /// <param name="localFile"></param>
        public void download(string remoteFile, string localFile)
        {
            try
            {
                /* Gets size of remote File. */
                int bufferSizeFile = getSizeFile(remoteFile);

                /* Creat an FTP request. */
                ftpRequest = (FtpWebRequest)FtpWebRequest.Create(host + "/" + remoteFile);

                /* Specifies the Type of FTP request. */
                ftpRequest.Method = WebRequestMethods.Ftp.DownloadFile;

                /* Log in to FTP server with the User Name and Password provided. */
                ftpRequest.Credentials = new NetworkCredential(user, password);

                /* When is doubt, use these option. */
                ftpRequest.UseBinary = true; // Value that specifies the data type for file transfer.
                ftpRequest.UsePassive = true; // Behavior of a client application's data transfer process.
                ftpRequest.KeepAlive = true; /* Value that specifies whether the control connection to the FTP
                                           server is closed after the request completes. */

                /* Establish return communication with the FTP server. */
                ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();

                /* Get the FTP Server's Response Stream. */
                ftpStream = ftpResponse.GetResponseStream();

                /* Open the File Stream to write the download file. */
                FileStream localFileStream = new FileStream(localFile, FileMode.Create);

                /* Buffer for the download data. */
                byte[] byteBuffer = new byte[bufferSizeFile];
                int bytesRead = ftpStream.Read(byteBuffer, 0, bufferSizeFile);
              
                /* Download the file by writting the buffered data until the transfer is complete. */
                try
                {
                    while (bytesRead > 0)
                    {
                        localFileStream.Write(byteBuffer, 0, bytesRead);
                        bytesRead = ftpStream.Read(byteBuffer, 0, bufferSizeFile);
                    }
                    
                    Console.WriteLine("Download success!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Un Success!");
                    Console.WriteLine("Error in download File: {0}", ex.ToString());
                }

                /* Resource cleanup. */
                ftpResponse.Close();
                localFileStream.Close();
                ftpStream.Close();
                ftpRequest = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: {0}", ex.ToString());
            }
        }

        /// <summary>
        /// Gets the size of remote file.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public int getSizeFile(string fileName)
        {
            int sizeFile = 0;

            try
            {
                /* Creat an FTP request. */
                ftpRequest = (FtpWebRequest)FtpWebRequest.Create(host + "/" + fileName);

                /* Specifies the Type of FTP request. */
                ftpRequest.Method = WebRequestMethods.Ftp.GetFileSize;

                /* Log in to FTP Server with the User Name and PassWord was provided. */
                ftpRequest.Credentials = new NetworkCredential(user, password);

                /* When is doubt, using these option. */
                ftpRequest.UseBinary = true;
                ftpRequest.UsePassive = true;
                ftpRequest.KeepAlive = true;

                /* Establish return communication with the FTP server. */
                ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();

                /* Establish return communication with the FTP server. */
                ftpStream = ftpResponse.GetResponseStream();

                /* Get the FTP server's Response Stream. */
                StreamReader ftpReader = new StreamReader(ftpStream);

                /* Get the size of File */
                sizeFile = (int)ftpResponse.ContentLength;

                /* Read the Full Response stream. */
                string fileInfo = null;
                try
                {
                    while(ftpReader.Peek() != -1)
                    {
                        fileInfo += ftpReader.ReadLine() + "\n";
                    }

                    Console.WriteLine(fileInfo);
                }
                catch(Exception ex)
                {
                    Console.WriteLine("File not exists.");
                    Console.WriteLine(ex.Message);
                }
                /* Resource cleanup. */
                ftpReader.Close();
                ftpStream.Close();
                ftpResponse.Close();
                ftpRequest = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }

            return sizeFile;
        }
        #endregion
    }
}
