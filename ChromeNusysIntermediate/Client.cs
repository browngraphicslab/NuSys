using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace ChromeNusysIntermediate
{
    class Client
    {
        private TcpClient tcpclnt;
        //a timer that will try to connect to nusys until the client succeeds
        private Timer reconnectTimer = new Timer();
        private bool isConnected = false;

        public Client()
        {
            //we will try to reconnect every 10 seconds while not connected
            reconnectTimer.Interval = 10000;
            reconnectTimer.Start();
            reconnectTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            Start();
        }

        private void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            Start();
        }

        private void Start()
        {
            //find the local ip address
            IPHostEntry host;
            string localIP = "?";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily.ToString() == "InterNetwork")
                {
                    localIP = ip.ToString();
                }
            }

            //try connecting to Nusys
            try
            {
                tcpclnt = new TcpClient();
                tcpclnt.Connect(localIP, 4370);
                reconnectTimer.Stop();
                isConnected = true;
                
            }
            catch (Exception e)
            {
                
            }

        }

        //send the given string via TCP to nusys
        public void Send(string message)
        {
            if (isConnected)
            {
                Stream stm = tcpclnt.GetStream();

                ASCIIEncoding asen = new ASCIIEncoding();
                byte[] ba = asen.GetBytes(message);

                stm.Write(ba, 0, ba.Length);
            }
        }

        public string Receive()
        {
            var result = string.Empty;

            try
            {
                Stream stm = tcpclnt.GetStream();
                byte[] bb = new byte[100];
                int k = 0;

                k = stm.Read(bb, 0, 100);

                for (int i = 0; i < k; i++)
                    result += Convert.ToChar(bb[i]);
            }
            catch (Exception ex)
            {
                Console.WriteLine("connection terminated.");
                return "";
            }

            return result;
        }



        public void Shutdown()
        {
            tcpclnt.Close();
        }
    }
}
