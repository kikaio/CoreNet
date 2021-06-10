using CoreNet.Networking;
using CoreNet.Utils.Loggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreNet.Protocols
{
    public class Translate
    {
        public static ConsoleLogger logger = new ConsoleLogger();
        public class Tranlator
        {

            public Func<NetStream, object> Reader;
            public Action<NetStream, object> Writer;
            public Tranlator(Action<NetStream, object> _w, Func<NetStream, object> _r)
            {
                Reader = _r;
                Writer = _w;
            }
        }

        public static Dictionary<Type, Tranlator> transDict = new Dictionary<Type, Tranlator>();

        public static void RegistValueTypes()
        {
            transDict[typeof(byte)] = new Tranlator((NetStream _s, object _obj)
                => {
                    _s.WriteByte((byte)_obj);
                },
                (NetStream _s)
                => {
                    return _s.ReadByte();
                });

            transDict[typeof(bool)] = new Tranlator((NetStream _s, object _obj)
                => {
                    _s.WriteBool((bool)_obj);
                },
                (NetStream _s)
                => {
                    return _s.ReadBool();
                });
            transDict[typeof(char)] = new Tranlator((NetStream _s, object _obj)
                => {
                    _s.WriteChar((char)_obj);
                },
                (NetStream _s)
                => {
                    return _s.ReadChar();
                });
            transDict[typeof(string)] = new Tranlator((NetStream _s, object _obj)
                => {
                    _s.WriteStr((string)_obj);
                },
                (NetStream _s)
                => {
                    return _s.ReadStr();
                });
            transDict[typeof(short)] = new Tranlator((NetStream _s, object _obj)
                => {
                    _s.WriteInt16((short)_obj);
                },
                (NetStream _s)
                => {
                    return _s.ReadInt16();
                });
            transDict[typeof(int)] = new Tranlator((NetStream _s, object _obj)
                => {
                    _s.WriteInt32((int)_obj);
                },
                (NetStream _s)
                => {
                    return _s.ReadInt32();
                });
            transDict[typeof(long)] = new Tranlator((NetStream _s, object _obj)
                => {
                    _s.WriteInt64((long)_obj);
                },
                (NetStream _s)
                => {
                    return _s.ReadInt64();
                });
            transDict[typeof(ushort)] = new Tranlator((NetStream _s, object _obj)
                => {
                    _s.WriteUInt16((ushort)_obj);
                },
                (NetStream _s)
                => {
                    return _s.ReadUInt16();
                });
            transDict[typeof(uint)] = new Tranlator((NetStream _s, object _obj)
                => {
                    _s.WriteUInt32((uint)_obj);
                },
                (NetStream _s)
                => {
                    return _s.ReadUInt32();
                });
            transDict[typeof(ulong)] = new Tranlator((NetStream _s, object _obj)
                => {
                    _s.WriteUInt64((ulong)_obj);
                },
                (NetStream _s)
                => {
                    return _s.ReadUInt64();
                });
            logger.WriteDebugTrace("complete");
        }

        public static void RegistCommonEnum()
        {
            Type t = typeof(Packet.PACKET_TYPE);
            transDict[t] = new Tranlator((NetStream _s, object _obj)
                => {
                    _s.WriteUInt16((ushort)_obj);
                },
                (NetStream _s)
                => {
                    var ret = (Packet.PACKET_TYPE)_s.ReadUInt16();
                    return ret;
                });
            logger.WriteDebugTrace("complete");
        }

        public static void RegistCustom()
        {
            logger.WriteDebugTrace("complete");
            return;
        }

        public static bool Init()
        {
            RegistValueTypes();

            RegistCommonEnum();
            RegistCustom();
            return true;
        }

        public static T Read<T>(NetStream _s)
        {
            Type t = typeof(T);
            if (transDict.ContainsKey(t))
                return (T)transDict[t].Reader(_s);
            else
            {
                logger.WriteDebugError($"{t.FullName} is not regist translate reader");
                return default(T);
            }
        }

        public static void Write<T>(NetStream _s, T _val)
        {
            Type t = typeof(T);
            if (transDict.ContainsKey(t))
            {
                transDict[t].Writer(_s, (object)_val);
            }
            else
            {
                logger.WriteDebugWarn($"{t.FullName} is not regist translate writer");
                return;
            }
        }
    }
}
