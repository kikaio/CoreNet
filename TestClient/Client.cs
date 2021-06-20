using CoreNet.Jobs;
using CoreNet.Networking;
using CoreNet.Protocols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestClient
{
    public class Client : CoreNetwork
    {
        public static Client Inst => new Client();

        private CoreSession mSession;
        private Dictionary<string, Worker> wDict = new Dictionary<string, Worker>();


        public override void ReadyToStart()
        {
            wDict["hb"] = new Worker();
            wDict["pkg"] = new Worker();
        }

        public override void Start()
        {
        }

        protected override void Analizer_Ans(CoreSession _s, Packet _p)
        {
        }

        protected override void Analizer_Noti(CoreSession _s, Packet _p)
        {
        }

        protected override void Analizer_Req(CoreSession _s, Packet _p)
        {
        }

        protected override void Analizer_Test(CoreSession _s, Packet _p)
        {
        }
    }
}
