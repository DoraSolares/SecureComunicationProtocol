using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Management;
using Newtonsoft.Json;

namespace SocketServerSSL
{
    class Program
    {
        private static int _listeningPort = 2000;
        static void Main()
        {
            ManagementObjectSearcher myVideoObject = new ManagementObjectSearcher("select * from Win32_VideoController");
            ManagementObjectSearcher myProcessorObject = new ManagementObjectSearcher("select * from Win32_Processor");
            ManagementObjectSearcher myOperativeSystemObject = new ManagementObjectSearcher("select * from Win32_OperatingSystem");

            var serverCertificate = getServerCert();

            var listener = new TcpListener(IPAddress.Any, _listeningPort);
            listener.Start();

            while (true)
            {
                using (var client = listener.AcceptTcpClient())
                using (var sslStream = new SslStream(client.GetStream(), false, ValidateCertificate))
                {
                    sslStream.AuthenticateAsServer(serverCertificate, true, SslProtocols.Tls12, false);

                    var inputBuffer = new byte[4096];
                    var inputBytes = 0;
                    while (inputBytes == 0)
                    {
                        inputBytes = sslStream.Read(inputBuffer, 0, inputBuffer.Length);
                    }
                    var inputMessage = Encoding.UTF8.GetString(inputBuffer, 0, inputBytes);

                    string resp = "";
                    ManagementObject video = null;
                    ManagementObject processor = null;
                    ManagementObject os = null;

                    foreach (ManagementObject obj in myVideoObject.Get())
                    {
                        video = obj;
                    }
                    foreach (ManagementObject obj in myProcessorObject.Get())
                    {
                        processor = obj;
                    }
                    foreach (ManagementObject obj in myOperativeSystemObject.Get())
                    {
                        os = obj;
                    }
                    // Le respondemos al emisor 
                    if (inputMessage == "status")
                    {
                        ServerStatus server = new ServerStatus()
                        {
                            GPUName = video["Name"].ToString(),
                            GPUStatus = video["Status"].ToString(),
                            GPUDriverVersion = video["DriverVersion"].ToString(),
                            ProcessorName = processor["Name"].ToString(),
                            ProcessorCurrentClockSpeed = processor["CurrentClockSpeed"].ToString(),
                            OS = os["Caption"].ToString(),
                            OSVersion = os["Version"].ToString()
                        };
                        string jsonData = JsonConvert.SerializeObject(server);
                        resp = jsonData;
                    }
                    else if (inputMessage == "hardware")
                    {
                        ServerHardware server = new ServerHardware()
                        {
                            GPUName = video["Name"].ToString(),
                            GPUDeviceID = video["DeviceID"].ToString(),
                            GPUAdapterRAM = video["AdapterRAM"].ToString(),
                            GPUProcessor = video["VideoProcessor"].ToString(),
                            GPUArchitecture = video["VideoArchitecture"].ToString(),
                            ProcessorName = processor["Name"].ToString(),
                            ProcessorManufacturer = processor["Manufacturer"].ToString(),
                            ProcessorCurrentClockSpeed = processor["CurrentClockSpeed"].ToString(),
                            ProcessorNumberOfCores = processor["NumberOfCores"].ToString(),
                            ProcessorNumberOfEnabledCores = processor["NumberOfEnabledCore"].ToString(),
                            NumberOfLogicalProcessors = processor["NumberOfLogicalProcessors"].ToString(),
                            ProcessorArchitecture = processor["Architecture"].ToString()
                        };
                        string jsonData = JsonConvert.SerializeObject(server);
                        resp = jsonData;
                    }

                    byte[] msg = Encoding.ASCII.GetBytes(resp);
                    sslStream.Write(msg);
                    Console.WriteLine("GOT Data: {0}", inputMessage);
                }
            }
        }

        static bool ValidateCertificate(Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            // For this sample under Windows 7 I also get
            // a remote cert not available error, so we
            // just do a return true here to signal that
            // we are trusting things. In the real world,
            // this would be very bad practice.
            return true;
            if (sslPolicyErrors == SslPolicyErrors.None)
            { return true; }
            // we don't have a proper certificate tree
            if (sslPolicyErrors ==
                  SslPolicyErrors.RemoteCertificateChainErrors)
            { return true; }
            return false;
        }

        private static X509Certificate getServerCert()
        {
            X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly);

            X509Certificate2 foundCertificate = null;
            foreach (X509Certificate2 currentCertificate in store.Certificates)
            {
                if (currentCertificate.IssuerName.Name != null && currentCertificate.IssuerName.Name.Equals("CN=MySslSocketCertificate"))
                {
                    foundCertificate = currentCertificate;
                    break;
                }
            }
            return foundCertificate;
        }
    }
}
