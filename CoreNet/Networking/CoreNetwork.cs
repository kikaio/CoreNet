using CoreNet.Protocols;
using CoreNet.Utils;
using CoreNet.Utils.LockHelper;
using CoreNet.Utils.Loggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CoreNet.Networking
{
    public abstract class CoreNetwork
    {
        protected CoreLogger logger = new ConsoleLogger();

        public LockedSwapQ<Package> packageQ { get; private set; } = new LockedSwapQ<Package>();

        public int port { get; protected set; } = 0;
        public string name { get; protected set; } = "MuYaHo";
        protected int backlog = 10;
        protected EndPoint ep;

        public Action shutdownAct { get; protected set; } = null;

        public bool isDown { get; protected set; } = false;

        public CoreNetwork(string _name = "CoreNetwork"
            , int _port = 30000)
        {
            name = _name;
            port = _port;
            Translate.Init();
        }


        public void PushPackage(Package _pkg)
        {
            packageQ.Push(_pkg);
        }

        public void PackageDispatcher(Package _pkg)
        {
            //network 단계에서 packet type, content type을 미리 읽는다.
            _pkg.Packet.ReadPacketType();
            switch (_pkg.Packet.pType)
            {
                case Packet.PACKET_TYPE.REQ:
                    Analizer_Req(_pkg.session, _pkg.Packet);
                    break;
                case Packet.PACKET_TYPE.ANS:
                    Analizer_Ans(_pkg.session, _pkg.Packet);
                    break;
                case Packet.PACKET_TYPE.NOTI:
                    Analizer_Noti(_pkg.session, _pkg.Packet);
                    break;
                case Packet.PACKET_TYPE.TEST:
                    Analizer_Test(_pkg.session, _pkg.Packet);
                    break;
                default:
                    break;
            }
        }

        public async Task PackageDispatcherAsync(Package _pkg)
        {
            var s = _pkg.session;
            var p = _pkg.Packet;

            p.ReadPacketType();

            logger.WriteDebug($"[{p.pType.ToString()}] analized");
            switch (p.pType)
            {
                case Packet.PACKET_TYPE.NOTI:
                    await AnalizerAsync_Noti(s, p);
                    break;
                case Packet.PACKET_TYPE.REQ:
                    await AnalizerAsync_Req(s, p);
                    break;
                case Packet.PACKET_TYPE.ANS:
                    await AnalizerAsync_Ans(s, p);
                    break;
                case Packet.PACKET_TYPE.TEST:
                    await AnalizerAsync_Test(s, p);
                    break;
                default:
                    break;
            }
        }

        public abstract void ReadyToStart();
        public abstract void Start();

        protected abstract void Analizer_Req(CoreSession _s, Packet _p);
        protected abstract void Analizer_Ans(CoreSession _s, Packet _p);
        protected abstract void Analizer_Noti(CoreSession _s, Packet _p);
        protected abstract void Analizer_Test(CoreSession _s, Packet _p);

        protected async virtual Task AnalizerAsync_Req(CoreSession _s, Packet _p)
        {
        }
        protected async virtual Task AnalizerAsync_Ans(CoreSession _s, Packet _p)
        {
        }
        protected async virtual Task AnalizerAsync_Noti(CoreSession _s, Packet _p)
        {
        }
        protected async virtual Task AnalizerAsync_Test(CoreSession _s, Packet _p)
        {
        }

        public void NetworkShutDown()
        {
            shutdownAct?.Invoke();
        }
    }
}
