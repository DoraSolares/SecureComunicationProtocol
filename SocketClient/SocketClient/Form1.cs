using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace SocketClient
{
    public partial class Form1 : Form
    {
        //IPHostEntry ipHostInfo = Dns.GetHostEntry("DESKTOP-9EGCNMG");
        //IPAddress ipAddress = IPAddress.Parse("192.168.56.101");
        

        private static int _hostPort = 2000;
        private static string _hostName = "192.168.128.1";
        private static string ServerCertificateName = "MySslSocketCertificate";

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
           
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

        static bool ValidateCertificate(Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
            { return true; }
            // ignore chain errors as where self signed
            if (sslPolicyErrors == SslPolicyErrors.RemoteCertificateChainErrors)
            { return true; }
            return false;
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            // Mensaje codificado 
            string mensaje = txtRequest.Text;
            msg("Request: " + mensaje);
            if (mensaje != "status" && mensaje != "hardware")
            {
                msg("Request unknown '" + mensaje + "', try with 'status' or 'hardware'");
                return;
            }
            byte[] msag = Encoding.ASCII.GetBytes(mensaje);

            var clientCertificate = getServerCert();
            var clientCertificateCollection = new X509CertificateCollection(new X509Certificate[] { clientCertificate });
            using (var client = new TcpClient(_hostName, _hostPort))
            using (var sslStream = new SslStream(client.GetStream(), false, ValidateCertificate))
            {
                sslStream.AuthenticateAsClient(ServerCertificateName, clientCertificateCollection, SslProtocols.Tls12, false);
                sslStream.Write(msag);

                var inputBuffer = new byte[4096];
                var inputBytes = 0;
                while (inputBytes == 0)
                {
                    inputBytes = sslStream.Read(inputBuffer, 0, inputBuffer.Length);
                }
                var men = Encoding.UTF8.GetString(inputBuffer, 0, inputBytes);

                if (mensaje == "status")
                {
                    ServerStatus obj = JsonConvert.DeserializeObject<ServerStatus>(men);
                    msg("GPU: ");
                    msg("\t" + obj.GPUName);
                    msg("\t Status - " + obj.GPUStatus);
                    msg("\t Driver version - " + obj.GPUDriverVersion);

                    msg("Processor:");
                    msg("\t" + obj.ProcessorName);
                    msg("\t Current clock speed - " + obj.ProcessorCurrentClockSpeed);

                    msg("Operanting System:");
                    msg("\t" + obj.OS);
                    msg("\t OS version - " + obj.OSVersion);
                }
                else if (mensaje == "hardware")
                {
                    ServerHardware obj = JsonConvert.DeserializeObject<ServerHardware>(men);

                    msg("GPU: ");
                    msg("\t" + obj.GPUName);
                    msg("\t DevideID - " + obj.GPUDeviceID);
                    msg("\t Adapter RAM - " + obj.GPUAdapterRAM);
                    msg("\t Video Architecture - " + obj.GPUArchitecture);
                    msg("\t Video Processor - " + obj.GPUProcessor);

                    msg("Processor:");
                    msg("\t" + obj.ProcessorName);
                    msg("\t Manufacturer - " + obj.ProcessorManufacturer);
                    msg("\t Current clock speed - " + obj.ProcessorCurrentClockSpeed);
                    msg("\t Number of cores - " + obj.ProcessorNumberOfCores);
                    msg("\t Number of enabled cores - " + obj.ProcessorNumberOfEnabledCores);
                    msg("\t Number of logical processor - " + obj.NumberOfLogicalProcessors);
                    msg("\t Architecture - " + obj.ProcessorArchitecture);
                }
            }

            var listener = new TcpListener(IPAddress.Any, _hostPort);
            listener.Start();

            
                
                
            

            // Recibe una respuesta

            txtRequest.Text = "";
            txtRequest.Focus();
        }

        public void msg(string mesg)
        {
            txtPrompt.Text = txtPrompt.Text + Environment.NewLine + " >> " + mesg;
        } 
    }
}
