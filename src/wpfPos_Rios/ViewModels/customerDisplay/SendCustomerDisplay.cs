using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace wpfPos_Rios.ViewModels.customerDisplay
{
    class SendCustomerDisplay
    {

        public void sendData(string Data)
        {
            string myIpHost = Dns.GetHostName();

            // Create a WebPermission that gives permissions to all the hosts containing the same host fragment.
            WebPermission myWebPermission = new WebPermission(NetworkAccess.Connect, myIpHost);
            //Add connect privileges for a www.adventure-works.com.
            myWebPermission.AddPermission(NetworkAccess.Connect, myIpHost);

            //Add accept privileges for www.alpineskihouse.com.
            myWebPermission.AddPermission(NetworkAccess.Accept, myIpHost);

            // Check whether all callers higher in the call stack have been granted the permission.
            myWebPermission.Demand();

            IPHostEntry myIpHostEntry;
            myIpHostEntry = Dns.GetHostEntry(myIpHost);
            IPEndPoint myLocalEndPoint = new IPEndPoint(myIpHostEntry.AddressList[1], 666);

            // Creates a SocketPermission restricting access to and from all URIs.
            SocketPermission mySocketPermission1 = new SocketPermission(PermissionState.Unrestricted);

            // The socket to which this permission will apply will allow connections from 127.0.0.1
            mySocketPermission1.AddPermission(NetworkAccess.Accept, TransportType.Tcp, myIpHost, 666);

            mySocketPermission1.Demand();
            //  Create a Regex that accepts all URLs containing the host fragment www.contoso.com.

            FileIOPermission f = new FileIOPermission(PermissionState.None);
            f.AllLocalFiles = FileIOPermissionAccess.Read;
            try
            {
                f.Demand();
            }
            catch (SecurityException)
            {
                //Console.WriteLine(s.Message);
            }

            // Socket
            Socket socket = new Socket(AddressFamily.InterNetwork,
                                         SocketType.Stream,
                                         ProtocolType.Tcp);

            Byte[] SendBytes = Encoding.ASCII.GetBytes(Data);
            try
            {
                socket.Connect(myLocalEndPoint);

                socket.Send(SendBytes,
                             SendBytes.Length,
                             SocketFlags.None);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception Thrown: " + e.ToString());
            }

            // Perform all socket operations in here.

            socket.Close();
        }

    }
}
