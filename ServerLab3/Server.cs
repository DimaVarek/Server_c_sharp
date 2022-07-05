using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;

namespace ServerLab3
{

    class Server
    {

        TcpListener listener;

        TextBox log;

        SortedSet<Socket> connections;
 

        public Server(TextBox _log)
        {
            log = _log;
            connections = new SortedSet<Socket>();

        }
        public void Start(int port)
        {
            //IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, port);
            //listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            
            IPAddress localAddr = IPAddress.Parse("127.0.0.1");
            listener = new TcpListener(localAddr, 5000);
            
            try
            {
                listener.Start();
                log.Invoke((MethodInvoker)delegate { log.AppendText("Server start" + Environment.NewLine); });


                while (true)
                {
                    // Set the event to nonsignaled state.  
                    Socket client = listener.AcceptSocket();

                    connections.Add(client);

                    LogClient(client);

                    Task t = new Task(() => ClientHandle(client));
                    t.Start();
                }

            }
            catch (Exception e)
            {
                MessageBox.Show("Server stopped listening " + e.Message);
            }
        }

        private void LogClient(Socket client)
        {
            var ip = (IPEndPoint)client.RemoteEndPoint;
            log.Invoke((MethodInvoker)delegate { log.AppendText(ip.Address.ToString() + ":" + ip.Port.ToString() + " connected to server" + Environment.NewLine); });
        }
        private void LogDisconnectClient(Socket client)
        {
            var ip = (IPEndPoint)client.RemoteEndPoint;
            log.Invoke((MethodInvoker)delegate { log.AppendText(ip.Address.ToString() + ":" + ip.Port.ToString() + " disconnected from server" + Environment.NewLine); });
        }

        public void Stop()
        {
            
            listener.Stop();
            connections.Clear();
        }
        public void ClientHandle(Socket client)
        {
            byte[] buffer = new byte[sizeof(int)];
            while (client.Receive(buffer, buffer.Length, SocketFlags.None) > 0)
            {
                switch ((Command)BitConverter.ToInt32(buffer, 0))
                {
                    case Command.Putfile:
                        Putfile(client);
                        break;
                    case Command.FillFile:
                        FillFile(client);
                        break;
                }
            }
            LogDisconnectClient(client);
            client.Shutdown(SocketShutdown.Both);
            client.Close();
            connections.Remove(client);
        }
        public enum Command
        {
            Putfile,
            FillFile,
        }

        private void Putfile(Socket handler)
        {
            string filename = RecvString(handler);

            bool result;
            string path = "C:/Users/Dima/Desktop/Учеба/8 семестр/Сетевые технологии/лабы/лаба3/proba/Server/ServerLab3/bin/Debug/net6.0-windows/filesystem/";

            try
            {
                FileStream fstream = new FileStream(path + filename, FileMode.Create);
                fstream.Close();
                result = true;
            }
            catch (Exception)
            {
                result = false;
            }
            handler.Send(BitConverter.GetBytes(result));
        }
        private void FillFile(Socket handler)
        {
            string filename = RecvString(handler);
            string infile = RecvString(handler);
            string path = "C:/Users/Dima/Desktop/Учеба/8 семестр/Сетевые технологии/лабы/лаба3/proba/Server/ServerLab3/bin/Debug/net6.0-windows/filesystem/";
            bool result;
            try
            {
                FileStream fstream = new FileStream(path + filename, FileMode.Create, FileAccess.Write);
                
                // преобразуем строку в байты
                byte[] buffer = Encoding.Default.GetBytes(infile);
                // запись массива байтов в файл
                fstream.Write(buffer, 0, buffer.Length);
                fstream.Close();
                result = true;
            }
            catch (Exception)
            {
                result = false;
            }
            handler.Send(BitConverter.GetBytes(result));

        }

        private void SendString(Socket handler, string str)
        {
            handler.Send(BitConverter.GetBytes(str.Length));
            handler.Send(Encoding.UTF8.GetBytes(str));
        }
        private string RecvString(Socket handler)
        {
            byte[] buffer = new byte[sizeof(int)];
            handler.Receive(buffer);
            if (BitConverter.ToInt32(buffer) > 0)
            {
                buffer = new byte[BitConverter.ToInt32(buffer)];
                handler.Receive(buffer);

                return Encoding.UTF8.GetString(buffer);
            }
            else
                return "";
        }

        private int RecvInt(Socket handler)
        {
            byte[] buffer = new byte[sizeof(int)];
            handler.Receive(buffer);
            return BitConverter.ToInt32(buffer);
        }
    }
}
