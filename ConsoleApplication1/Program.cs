using System;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;
using System.Data.SqlClient;
//using MySql.Data.MySqlClient.MySqlConnectionStringBuilder;


// SSL: https ://weblogs.asp.net/scottgu/tip-trick-enabling-ssl-on-iis7-using-self-signed-certificates


// https://msdn.microsoft.com/en-us/library/system.net.sockets%28v=vs.110%29.aspx


namespace WebSnif
{


    // https: //www.codeproject.com/Articles/380769/Creating-an-FTP-Server-in-Csharp-with-IPv-Support
    public class ClientConnection
    {
        private TcpClient _controlClient;

        private NetworkStream _controlStream;
        private StreamReader _controlReader;
        private StreamWriter _controlWriter;

        private string _username;

        public ClientConnection(TcpClient client)
        {
            _controlClient = client;

            _controlStream = _controlClient.GetStream();

            _controlReader = new StreamReader(_controlStream);
            _controlWriter = new StreamWriter(_controlStream);
        }

        public void HandleClient(object obj)
        {
            _controlWriter.WriteLine("220 Service Ready.");
            _controlWriter.Flush();

            string line;

            try
            {
                while (!string.IsNullOrEmpty(line = _controlReader.ReadLine()))
                {
                    string response = null;

                    string[] command = line.Split(' ');

                    string cmd = command[0].ToUpperInvariant();
                    string arguments = command.Length > 1 ? line.Substring(command[0].Length + 1) : null;

                    if (string.IsNullOrWhiteSpace(arguments))
                        arguments = null;

                    if (response == null)
                    {
                        switch (cmd)
                        {
                            case "USER":
                                response = User(arguments);
                                break;
                            case "PASS":
                                response = Password(arguments);
                                break;
                            case "CWD":
                                response = ChangeWorkingDirectory(arguments);
                                break;
                            case "CDUP":
                                response = ChangeWorkingDirectory("..");
                                break;
                            case "PWD":
                                response = "257 \"/\" is current directory.";
                                break;
                            case "QUIT":
                                response = "221 Service closing control connection";
                                break;

                            default:
                                response = "502 Command not implemented";
                                break;
                        }
                    }

                    if (_controlClient == null || !_controlClient.Connected)
                    {
                        break;
                    }
                    else
                    {
                        _controlWriter.WriteLine(response);
                        _controlWriter.Flush();

                        if (response.StartsWith("221"))
                        {
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        #region FTP Commands

        private string User(string username)
        {
            _username = username;

            return "331 Username ok, need password";
        }

        private string Password(string password)
        {
            if (true)
            {
                return "230 User logged in";
            }
            else
            {
                return "530 Not logged in";
            }
        }

        private string ChangeWorkingDirectory(string pathname)
        {
            return "250 Changed to new directory";
        }

        #endregion
    }



    //http://aspalliance.com/1563_Socket_Programming_in_C.all
    class SocketServer
    {
        public void Run()
        {
            StreamWriter streamWriter;
            StreamReader streamReader;
            NetworkStream networkStream;
            byte[] address = new byte[4] { 127, 0, 0, 1 };
            //byte[] address = new byte[4] { 10, 31, 248, 6 };
            IPAddress ipAddress = new IPAddress(address);
            TcpListener tcpListener = new TcpListener(ipAddress, 21);
            tcpListener.Start();
            Console.WriteLine("The Server has started on port 21");
            Socket serverSocket = tcpListener.AcceptSocket();
            try
            {
                if (serverSocket.Connected)
                {
                    Console.WriteLine("Client connected");



                    // Using the RemoteEndPoint property.
                    Console.WriteLine("I am connected to " + IPAddress.Parse(((IPEndPoint)serverSocket.RemoteEndPoint).Address.ToString()) + " on port number " + ((IPEndPoint)serverSocket.RemoteEndPoint).Port.ToString());

                    // Using the LocalEndPoint property.
                    Console.WriteLine("My local IpAddress is " + IPAddress.Parse(((IPEndPoint)serverSocket.LocalEndPoint).Address.ToString()) + " I am connected on port number " + ((IPEndPoint)serverSocket.LocalEndPoint).Port.ToString());





                    networkStream = new NetworkStream(serverSocket);
                    streamWriter = new StreamWriter(networkStream);
                    streamReader = new StreamReader(networkStream);
                    string fromClient;
                    while (true)
                    {
                        fromClient = streamReader.ReadLine();
                        Console.WriteLine(fromClient == null ? "fromClient er null" : fromClient);
                        //Console.WriteLine(streamReader.ReadLine());
                        if (fromClient == null || fromClient.StartsWith("QUIT"))
                            break;
                    }
                }
                if (serverSocket.Connected)
                    serverSocket.Close();
                //Console.Read();
            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex);
            }
        }
    }


    class SocketClient
    {
        public void Run()
        {
            TcpClient tcpClient;
            NetworkStream networkStream;
            StreamReader streamReader;
            StreamWriter streamWriter;
            try
            { //string hostname = "localhost";
                string hostname = "tv2.dk";
                int port = 80;
                tcpClient = new TcpClient(hostname, port);
                //tcpClient = new TcpClient("10.31.248.6", 5555);
                //tcpClient = new TcpClient("localhost", 5555);
                networkStream = tcpClient.GetStream();
                streamReader = new StreamReader(networkStream);
                streamWriter = new StreamWriter(networkStream);
                //streamWriter.WriteLine("Message from the Client...");
                //streamWriter.Flush();
                //streamWriter.WriteLine("QUIT");
                streamWriter.WriteLine("GET / HTTP/1.1");
                streamWriter.WriteLine("HOST: "+hostname+":" +port);
                streamWriter.WriteLine("");
                streamWriter.Flush();

                string line = streamReader.ReadLine();
                Console.WriteLine(line);



                for(int i=0; i<10; )
                {
                    line = streamReader.ReadLine();
                    if(line!=null && line.Length>0)
                    Console.WriteLine(line);
                    if(line==null)
                    {
                        i++;
                        Thread.Sleep(10);
                    }

                }



            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex);
            }
        }
    }


    class FtpClient
    {
        public void Run()
        {
            //public String GetFilesAsString(string folder, string fileExtension) {
            StringBuilder result = new StringBuilder();
            FtpWebRequest reqFTP;
            try
            {
                Console.WriteLine("FtpClient.Run()");
                //String ftpserver = "ftp://127.0.0.1/";
                String ftpserver = "ftp://sc01.sdc.dk/";

                reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(ftpserver));
                reqFTP.UsePassive = false;
                reqFTP.UseBinary = true;
                reqFTP.Credentials = new NetworkCredential("U3249", "8uhbvgy7");
                reqFTP.Method = WebRequestMethods.Ftp.ListDirectory;
                Console.WriteLine("FtpClient.Run(): wait for reqFTP.GetResponse()");
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();

                Console.WriteLine("FtpClient.Run(): after reqFTP.GetResponse()");

                StreamReader reader = new StreamReader(response.GetResponseStream(), System.Text.Encoding.UTF8);
                string line = "";

                while (reader.Peek() > -1)
                {
                    Console.WriteLine("FtpClient.Run(): reader.Peek()");
                    line = reader.ReadLine();
                    Console.WriteLine(line);
                }

                if (result.ToString().LastIndexOf('\n') >= 0)
                    result.Remove(result.ToString().LastIndexOf('\n'), 1);
                reader.Close();
                response.Close();

                //return result.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine("FtpClient.Run(): " + ex);
            }
            //return null;
            //}
        }

    }


    class SomeServer
    {
        public int port { get; set; }
        public void Server()
        {
            TcpListener listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            Console.WriteLine("Server: {0} lytter", port);
            while (true)
            {
                var client = listener.AcceptTcpClient();
                Console.WriteLine("Server: {0} efter listener.AcceptTcpClient()", port);
                using (client)
                {
                    Console.WriteLine("Server skal til at læse og skrive", port);
                    var reader = new StreamReader(client.GetStream());
                    var writer = new StreamWriter(client.GetStream());
                    writer.WriteLine("Server: {0}  200 READY", port);
                    writer.Flush();
                    Console.WriteLine("Server: {0} har skrevet \"200 READY\"", port);
                    while (true)
                    {
                        string line = reader.ReadLine();
                        Console.WriteLine("Server har læst", port);
                        if (line == "QUIT")
                            break;
                        writer.WriteLine("Server:" + port + "  From Thread[" + Thread.CurrentThread.ManagedThreadId + "] > " + line);
                        Console.WriteLine("Server: {0} har skrevet", port);
                        writer.Flush();
                    }
                }
            }
        }
    }




    class WebSniffer
    {


        //https://msdn.microsoft.com/en-us/library/system.net.sockets.udpclient%28v=vs.110%29.aspx
        public void udp()
        {
            // This constructor arbitrarily assigns the local port number.
            UdpClient udpClient = new UdpClient(11000);
            try
            {
                udpClient.Connect("www.contoso.com", 11000);

                // Sends a message to the host to which you have connected.
                Byte[] sendBytes = Encoding.ASCII.GetBytes("Is anybody there?");

                udpClient.Send(sendBytes, sendBytes.Length);

                // Sends a message to a different host using optional hostname and port parameters.
                UdpClient udpClientB = new UdpClient();
                udpClientB.Send(sendBytes, sendBytes.Length, "www.dr.dk", 11000);

                //IPEndPoint object will allow us to read datagrams sent from any source.
                IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

                // Blocks until a message returns on this socket from a remote host.
                Byte[] receiveBytes = udpClient.Receive(ref RemoteIpEndPoint);
                string returnData = Encoding.ASCII.GetString(receiveBytes);

                // Uses the IPEndPoint object to determine which of these two hosts responded.
                Console.WriteLine("This is the message you received " +
                                             returnData.ToString());
                Console.WriteLine("This message was sent from " +
                                            RemoteIpEndPoint.Address.ToString() +
                                            " on their port number " +
                                            RemoteIpEndPoint.Port.ToString());

                udpClient.Close();
                udpClientB.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void StartClientServer()
        {
            //SocketServer socketServer = new SocketServer();
            SocketClient socketClient = new SocketClient();
            //FtpClient ftpClient = new FtpClient();

            //Thread serverThread = new Thread(new ThreadStart(socketServer.Run));
            //serverThread.Start();

            //while (!serverThread.IsAlive)
            //    Console.WriteLine("Waiting for serverThread.IsAlive");

            Thread clientThread = new Thread(new ThreadStart(socketClient.Run));
            clientThread.Start();

            //Thread ftpThread = new Thread(new ThreadStart(ftpClient.Run));
            //ftpThread.Start();



            //serverThread.Join();
            //clientThread.Join();
            //ftpThread.Join();

            //t.IsBackground = true;

        }







        void print(byte[] data, int number)
        {
            /*
            foreach(byte b in data) {
                Console.Write((char)b);
            }*/
            for (int i = 0; i < number; i++)
            {
                /*if(data[i] == '\r')
                    Console.Write("[CR]");
                else if(data[i] == '\n')
                    Console.Write("[LF]");
                Console.Write((data[i]>=0 && data[i]<32)?'.':(char)data[i]);*/
                Console.Write((char)data[i]);
            }
        }

        public void Server()
        {
            int port = 88;
            //TcpListener listener = new TcpListener(IPAddress.Any, port);
            TcpListener listener = new TcpListener(IPAddress.Parse("127.0.0.1"), port);
            listener.Start();
            Console.WriteLine("Server: {0} lytter", port);
            Console.WriteLine("Server is listening on " + listener.LocalEndpoint);
            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
                //Console.WriteLine("LocalEndPoint: " + client.Client.LocalEndPoint);
                //Console.WriteLine("RemoteEndPoint: " + client.Client.RemoteEndPoint);
                //Console.WriteLine("familie: " + listener.LocalEndpoint.AddressFamily);
                //listener.Server.SendFile("c:\\sletMig.txt"); // det kan man ikke
                //client.Client.SendFile("c:\\sletMig.txt"); // fungerer fint
                Console.WriteLine("Server: {0} efter listener.AcceptTcpClient()", port);
                new Thread(() => {
                    using (client)
                    {
                        Console.WriteLine("Server: {0} skal til at læse og skrive", port);
                        var reader = new StreamReader(client.GetStream());
                        var writer = new StreamWriter(client.GetStream());
                        writer.WriteLine("LocalEndPoint (server): " + client.Client.LocalEndPoint + "  RemoteEndPoint (client): " + client.Client.RemoteEndPoint);
                        writer.WriteLine("Server: {0}  200 READY", port);
                        writer.Flush();
                        Console.WriteLine("Server: {0} har skrevet \"200 READY\"", port);
                        bool keepGoing = true;
                        while (keepGoing)
                        {
                            string line = reader.ReadLine();
                            Console.WriteLine("Server: {0} har læst", port);
                        Console.WriteLine(line);
                            if (line == "QUIT")
                                keepGoing = false;
                            writer.WriteLine("Server:" + port + "  From Thread[" + Thread.CurrentThread.ManagedThreadId + "] > " + line);
                            Console.WriteLine("Server:" + port + "  From Thread[" + Thread.CurrentThread.ManagedThreadId + "] > " + line);
                            //Console.WriteLine("Server: {0} har skrevet", port);
                            writer.Flush();
                        }
                        //client.Close(); // using(client) ought to do that, and it actually does
                        //Console.WriteLine("A Connected: " + client.Client.Connected);
                    }
                    //Console.WriteLine("B Connected: " + client.Client.Connected);
                }).Start();
                //Console.WriteLine("C Connected: " + client.Client.Connected);
            }
        }


        public void Snif()
        {
            byte[] address = new byte[4] { 127, 0, 0, 1 };
            IPAddress ipAddress = new IPAddress(address);
            TcpListener tcpListener = new TcpListener(ipAddress, 21);
            tcpListener.Start();
            Console.WriteLine("Efter tcpListener.Start()");


            const int receiveBufferSize = 1024;
            byte[] receiveBuffer = new byte[receiveBufferSize];
            TcpClient tcpClient = tcpListener.AcceptTcpClient();
            Console.WriteLine("Efter tcpListener.AcceptTcpClient()");
            StreamReader reader = new StreamReader(tcpClient.GetStream());
            StreamWriter writer = new StreamWriter(tcpClient.GetStream());
            Console.WriteLine("Efter new StreamReader/Writer(tcpClient.GetStream()");
            writer.WriteLine("200 Hallo - wie get es?");
            Console.WriteLine("Efter (\"200 Hallo - wie geht es?\")");
            Console.WriteLine(reader.ReadLine());
            Console.WriteLine("Efter Console.WriteLine(reader.ReadLine())");
            tcpClient.Close();
            /*
            Socket socket=tcpListener.AcceptSocket();
            Console.WriteLine("Efter tcpListener.AcceptSocket()");
            
            int received=socket.Receive(receiveBuffer);
            Console.WriteLine("Efter socket.Receive(receiveBuffer)");

            while(received==receiveBufferSize) {
                print(receiveBuffer, received);
                socket.Send(receiveBuffer);
                received = socket.Receive(receiveBuffer);
            }

            print(receiveBuffer, received);
            socket.Send(receiveBuffer, received, SocketFlags.None);

            socket.Close();
            */
        }

        public void StartServer1()
        {
            Thread t = new Thread(Server);
            t.IsBackground = true;
            t.Start();
        }
        /*
        public void Database() {
            MySqlConnectionStringBuilder conn = new MySqlConnectionStringBuilder();
            //conn.ser
        }*/



        public void StartServer()
        {
            SomeServer serverA = new SomeServer();
            serverA.port = 3333;

            SomeServer serverB = new SomeServer();
            serverB.port = 3333;

            Thread serverAThread = new Thread(new ThreadStart(serverA.Server));
            serverAThread.IsBackground = true;
            serverAThread.Start();

            Thread serverBThread = new Thread(new ThreadStart(serverB.Server));
            serverBThread.IsBackground = true;
            serverBThread.Start();
            Console.WriteLine("Severe er started");

        }

        public void UdpServer()
        {
            Console.WriteLine("UdpServer is running");
            byte[] data = new byte[1024];
            UdpClient udpClient = new UdpClient(new IPEndPoint(new IPAddress(new byte[] { 127, 0, 0, 1 }), 4444));

            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            data = udpClient.Receive(ref sender);
            Console.WriteLine("UdpServer: received from {0}:", sender.ToString());
            Console.WriteLine(Encoding.ASCII.GetString(data, 0, data.Length));

            string welcome = "Welcome to my test server";
            data = Encoding.ASCII.GetBytes(welcome);
            udpClient.Send(data, data.Length, sender);

            while (true)
            {
                data = udpClient.Receive(ref sender);

                Console.WriteLine("Server received: " + Encoding.ASCII.GetString(data, 0, data.Length));
                udpClient.Send(data, data.Length, sender);
            }
        }


        private System.Object lockThis = new System.Object();

        int clientNumberSource = 0;
        //public void UdpClient(int clientNumber) {
        public void UdpClient()
        {
            int clientNumber;
            //new Thread(() => {
            lock (lockThis)
            {
                clientNumber = ++clientNumberSource;
            }
            Console.WriteLine("UdpClient {0} is running", clientNumber);

            byte[] data = new byte[1024];
            UdpClient udpClient = new UdpClient(new IPEndPoint(new IPAddress(new byte[] { 127, 0, 0, 1 }), 3330 + clientNumber));

            IPEndPoint udpServer = new IPEndPoint(new IPAddress(new byte[] { 127, 0, 0, 1 }), 4444);

            string welcome = "Yaiy ...  I am #" + clientNumber;
            data = Encoding.ASCII.GetBytes(welcome);
            udpClient.Send(data, data.Length, udpServer);

            data = udpClient.Receive(ref udpServer);
            Console.WriteLine("UdpClient {0}: received from {1}:", clientNumber, udpServer.ToString());
            Console.WriteLine(Encoding.ASCII.GetString(data, 0, data.Length));

            Random random = new Random();

            int antal = 0;
            while (antal < 30)
            {
                string nogetData = "Client #" + clientNumber + " besked[" + (++antal) + "]";
                data = Encoding.ASCII.GetBytes(nogetData);
                udpClient.Send(data, data.Length, udpServer);

                data = udpClient.Receive(ref udpServer);
                Console.WriteLine("Client #" + clientNumber + " received: " + Encoding.ASCII.GetString(data, 0, data.Length));
                //Thread.Sleep(random.Next(500,600));
                //if(clientNumber==1) Thread.Sleep(1);

            }
            //});
        }




        void myDns()        {
            //string hostName = Dns.GetHostName();
            string hostName = "tv2.dk";
            try            {
                IPAddress[] ipAddress = Dns.GetHostEntry(hostName).AddressList;
                foreach (IPAddress address in ipAddress)
                    Console.WriteLine("{0}/{1}", hostName, address);
            }
            catch (Exception ex)            {
                Console.WriteLine("Error occurred: " + ex.Message);
            }
        }





        public void StartUdpClientServer()
        {
            Console.WriteLine("StartUdpClientServer A");
            Thread udpServerThread = new Thread(new ThreadStart(UdpServer));
            udpServerThread.IsBackground = true;
            udpServerThread.Start();

            Console.WriteLine("StartUdpClientServer B");

            Thread udpClientThreadA = new Thread(new ThreadStart(UdpClient));
            udpClientThreadA.IsBackground = true;
            udpClientThreadA.Start();

            Console.WriteLine("StartUdpClientServer C");

            Thread udpClientThreadB = new Thread(new ThreadStart(UdpClient));
            udpClientThreadB.IsBackground = true;
            udpClientThreadB.Start();

            Console.WriteLine("StartUdpClientServer D");

        }

        static void Main(string[] args)
        {
            //new WebSniffer().myDns();
            //new WebSniffer().Snif();
            //new WebSniffer().StartServer1();
            //new WebSniffer().StartUdpClientServer();
            new WebSniffer().StartClientServer();
            //new WebSniffer().udp();
            Console.WriteLine("Press a key ... any ole key will do.");
            //Console.Read();
            Console.ReadKey();
        }
    }
}
