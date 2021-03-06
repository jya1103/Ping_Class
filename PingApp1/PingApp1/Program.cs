﻿//http://justinmklam.com/posts/2018/02/ping-sweeper/
//https://stackoverflow.com/questions/31066195/how-to-pass-parameter-to-cmd-exe-and-get-the-result-back-into-c-sharp-windows-ap
          
using System;
using System.Diagnostics;
using System.Threading;  //
using System.Net.NetworkInformation;

using System.Runtime.InteropServices; // Necessary!
using System.Net;
using System.IO;
using System.Text;
using System.Net.Sockets;
using System.Linq;

namespace ConsoleApplication1
{
    class Program
    {
        static CountdownEvent countdown;   //using System.Threading;
        static int upCount = 0;
        static readonly object lockObj = new object();
        const bool resolveNames = true;
      
        public static void Main(string[] args)
        {     //EASY 
              //Ping Class | PingReply Class
              //Using the System.Net.NetworkInformation namespace, 
              //we can easily use the Ping.Send() command to check if a remote address is alive
           
       Console.WriteLine("Start..");
           
            if (args.Length == 0)
                //   throw new ArgumentException("Ping needs a host or IP Address.");
              
            Console.BackgroundColor = ConsoleColor.DarkGreen;
            Console.ForegroundColor = ConsoleColor.White;

            string gateway = "192.168.1.1";
            try
            {
                IPHostEntry entry = Dns.GetHostEntry(gateway);
                if (entry != null)
                {
                    Console.WriteLine("Gateway name: "+entry.HostName);
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine("Gateway unknown host ! Not every IP has a name");
                Console.WriteLine("Ping needs a host or IP Address.");
                //log exception (manage it)
            }
            //detect ip Addresses of localhost where process is running
            Console.WriteLine(Dns.GetHostName());
            IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());
            foreach (IPAddress addr in localIPs)
            {
                Console.WriteLine("Detect:" +addr);
            }

            /*
            //The only way to know your public IP is to ask someone else to tell you

            string direction;
            WebRequest request = WebRequest.Create("http://checkip.dyndns.org/");
            WebResponse response = request.GetResponse();
            StreamReader stream = new StreamReader(response.GetResponseStream());
            direction = stream.ReadToEnd();
            stream.Close();
            response.Close(); //Search for the ip in the html
            int first = direction.IndexOf("Address: ") + 9;
            int last = direction.LastIndexOf("");
            direction = direction.Substring(first, last - first);
            Console.WriteLine("checkip: " + direction);
            */

            IPAddress ip = Dns.GetHostAddresses(Dns.GetHostName())
                .Where(address => address.AddressFamily == AddressFamily.InterNetwork).First();

            Console.WriteLine(Dns.GetHostName());
            IPAddress[] localIP = Dns.GetHostAddresses(Dns.GetHostName());
            foreach (IPAddress addr in localIP)
            {
                if (addr.AddressFamily == AddressFamily.InterNetwork)
                {
                    Console.WriteLine("....."+addr);
                }
            }

            string machinename = "netgear";
            //Console.WriteLine(System.Net.Dns.GetHostEntry(who).HostName);
            string netBiosName = System.Environment.MachineName;
            Console.WriteLine("This Machine: " +netBiosName);
            IPAddress[] ipaddress = Dns.GetHostAddresses(machinename);
           
            foreach (IPAddress ipa in ipaddress)
            {
                 Console.WriteLine("IP address of Machine " + machinename +" is " + ipa.ToString());
            }

            Console.BackgroundColor = ConsoleColor.Magenta;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Doing a sync ping sweep to find devices that were physically connected through ethernet.");



            //timeout  amount of time, in milliseconds, that ping waits for each reply.
            int timeout = 100;   //in ms, 100 ms = 1 s
            Ping pp = new Ping();
            //public class PingReply
            //Héritage Object -->PingReply
            // PingReply Class //PingReply reply = pingSender.Send (args[0], timeout, buffer, options);
            //  PingOptions options = new PingOptions();
            //  options.DontFragment = true;  
            // string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";// Create a buffer of 32 bytes of data to be transmitted
           string data = "aaaaaaaaaaaaaaaa";
            byte[] buffer = Encoding.ASCII.GetBytes(data);
             //PingReply Fournit les données résultant d'une opération Send ou SendAsync.
             // Set options for transmission:
                    // The data can go through 64 gateways or routers
                   // before it is destroyed, and the data packet
                   // cannot be fragmentedtrue, DontFragment true.
                   //PingOptions options = new PingOptions(64, true);
                  
            PingOptions options = new PingOptions(2, true);
            Console.WriteLine("Time to live: {0}", options.Ttl);
            Console.WriteLine("Don't fragment: {0}", options.DontFragment);
            Console.WriteLine("Data length: {0}", data.Length );
            
            PingReply TheReplyObject = pp.Send(machinename, timeout, buffer, options);

            if (TheReplyObject.Status == IPStatus.Success)
            {
                //https://docs.microsoft.com/fr-fr/dotnet/api/system.net.networkinformation.ping?view=netframework-4.7.2
                //White on DarkGray.
                Console.BackgroundColor = ConsoleColor.DarkGray;
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("Operation status : {0}", TheReplyObject.Status);
                Console.WriteLine("Reply address: {0}", TheReplyObject.Address.ToString());//qui réponds ? Obtient l'adresse de l'hôte qui envoie la réponse à écho ICMP.
                Console.WriteLine("RoundTrip time: {0}", TheReplyObject.RoundtripTime); //le temps que ça prend d'envoyer et recevoir réponse
                Console.WriteLine("Time to live: {0}", TheReplyObject.Options.Ttl);
                Console.WriteLine("Don't fragment: {0}", TheReplyObject.Options.DontFragment);
                Console.WriteLine("Buffer size: {0}", TheReplyObject.Buffer.Length);

               
                Console.ResetColor();
            }
            else
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(gateway + "  " + TheReplyObject.Status);
                Console.WriteLine();
            }



            //Instead of using the synchronous Ping.Send(), it harnessed the asynchronous 
            //Ping.SendAsync() along with the CountdownEvent class in System.Threading. 
            //Finally, we have a working solution! Using async/await with Tasks in System.Threading.Tasks yields promising results. 


            countdown = new CountdownEvent(1);  //using System.Threading;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            string ipBase = "192.168.1.";
           
            for (int i = 1; i < 254; i++)
            {
                //concatene
                string ipScan = ipBase + i.ToString();
              
                Ping p = new Ping();  //initialise une nouvelle instance de la classe ping
                p.PingCompleted += new PingCompletedEventHandler(p_PingCompleted);  //using System.Net.NetworkInformation;
                countdown.AddCount();
                p.SendAsync(ipScan, 100, ipScan);
            }
            countdown.Signal();
            countdown.Wait();
            sw.Stop();
            TimeSpan span = new TimeSpan(sw.ElapsedTicks);
            Console.BackgroundColor = ConsoleColor.Blue;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Took {0} milliseconds. {1} hosts active.", sw.ElapsedMilliseconds, upCount);
            Console.ReadLine();
        }

       
            static void p_PingCompleted(object sender, PingCompletedEventArgs e)  //using System.Net.NetworkInformation;
        {
            //loop 255
            string ip = (string)e.UserState;
            
            if (e.Reply != null && e.Reply.Status == IPStatus.Success)  //using System.Net.NetworkInformation;
            {
                //Black on White.
                Console.BackgroundColor = ConsoleColor.White;
                Console.ForegroundColor = ConsoleColor.Black;
                Console.WriteLine("{0} is up: ({1} ms)", ip, e.Reply.RoundtripTime);
              
                lock (lockObj)
                {
                    upCount++;
                }
            }
            else if (e.Reply == null)
            {
               Console.WriteLine("Pinging {0} failed. (Null Reply object?)", ip);
            }
            countdown.Signal();
        }
    }

  
}
 