using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Server.Inst.ReadyToStart();
            Server.Inst.Start();

            while (Server.Inst.isDown == false)
            {
                Thread.Sleep(1000);
            }

            Console.WriteLine("Server is Fin, press any key");
            Console.ReadLine();
        }
    }
}
