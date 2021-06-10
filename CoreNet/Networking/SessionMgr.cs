using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CoreNet.Networking
{
    public class SessionMgr
    {
        public static SessionMgr Inst { get; private set; } = new SessionMgr();
        ConcurrentDictionary<long, CoreSession> sessionDict = new ConcurrentDictionary<long, CoreSession>();

        long curSessionId = 0;

        public bool AddSession(CoreSession _s)
        {
            var oldSession = default(CoreSession);
            if (CloseSession(_s.SessionId, out oldSession))
            {
                // 이경우는 뭘까.?
            }
            if (sessionDict.TryAdd(_s.SessionId, _s))
            {
                return true;
            }
            return false;
        }

        public void RemoveAllSession()
        {
            sessionDict.Clear();
        }
        public void CloseAllSession()
        {
            foreach (var ele in ToSessonList())
            {
                var sock = ele.Sock.Sock;
                ele.DoDispose(true);
                sock.Disconnect(true);
                sock.Close();
            }
            sessionDict.Clear();
        }
        public bool CloseSession(long _id, out CoreSession _s)
        {
            _s = default(CoreSession);
            if (sessionDict.TryRemove(_id, out _s))
            {
                _s.DoDispose(true);
                _s.Sock.Sock.Disconnect(true);
                _s.Sock.Sock.Close();
                return true;
            }
            return false;
        }

        public bool ForceCloseSession(long _id, out CoreSession _s)
        {
            _s = default(CoreSession);
            if (sessionDict.TryRemove(_id, out _s))
            {
                _s.DoDispose(true);
                _s.Sock.Sock.Shutdown(SocketShutdown.Both);
                _s.Sock.Sock.Close();

                return true;
            }
            return false;
        }

        public bool GetSession(long _id, out CoreSession _s)
        {
            _s = default(CoreSession);
            if (sessionDict.TryGetValue(_id, out _s))
                return true;
            return false;
        }

        public long GetNextSessionId()
        {
            return ++curSessionId;
        }

        public List<CoreSession> ToSessonList()
        {
            return sessionDict.Values.ToList();
        }
    }
}
