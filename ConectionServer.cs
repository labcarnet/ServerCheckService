using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerCheckService
{
    public class ConectionServer
    {

        public static bool ConnectToServer(string ipServer, int port)
        {
            try
            {
                using (var client = new TcpClient(ipServer, port))
                {
                    client.Close();
                }
                return true;

            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
