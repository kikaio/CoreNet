using CoreNet.Networking;
using CoreNet.Utils.Loggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreNet.Protocols
{
    using Writer = Action<NetStream, object>;
    using Reader = Func<NetStream, object>;
    using TransDict = Dictionary<Type, Translator>;
    public class Translator
    {

        public Func<NetStream, object> Reader;
        public Action<NetStream, object> Writer;
        public Translator(Action<NetStream, object> _w, Func<NetStream, object> _r)
        {
            Reader = _r;
            Writer = _w;
        }
    }
    public class Translate
    {
        public static ConsoleLogger logger = new ConsoleLogger();
        

        public static TransDict transDict = new TransDict();

        public static void RegistValueTypes()
        {
            transDict[typeof(byte)] = new Translator((NetStream _s, object _obj)
                => {
                    _s.WriteByte((byte)_obj);
                },
                (NetStream _s)
                => {
                    return _s.ReadByte();
                });

            transDict[typeof(bool)] = new Translator((NetStream _s, object _obj)
                => {
                    _s.WriteBool((bool)_obj);
                },
                (NetStream _s)
                => {
                    return _s.ReadBool();
                });
            transDict[typeof(char)] = new Translator((NetStream _s, object _obj)
                => {
                    _s.WriteChar((char)_obj);
                },
                (NetStream _s)
                => {
                    return _s.ReadChar();
                });
            transDict[typeof(string)] = new Translator((NetStream _s, object _obj)
                => {
                    _s.WriteStr((string)_obj);
                },
                (NetStream _s)
                => {
                    return _s.ReadStr();
                });
            transDict[typeof(short)] = new Translator((NetStream _s, object _obj)
                => {
                    _s.WriteInt16((short)_obj);
                },
                (NetStream _s)
                => {
                    return _s.ReadInt16();
                });
            transDict[typeof(int)] = new Translator((NetStream _s, object _obj)
                => {
                    _s.WriteInt32((int)_obj);
                },
                (NetStream _s)
                => {
                    return _s.ReadInt32();
                });
            transDict[typeof(long)] = new Translator((NetStream _s, object _obj)
                => {
                    _s.WriteInt64((long)_obj);
                },
                (NetStream _s)
                => {
                    return _s.ReadInt64();
                });
            transDict[typeof(ushort)] = new Translator((NetStream _s, object _obj)
                => {
                    _s.WriteUInt16((ushort)_obj);
                },
                (NetStream _s)
                => {
                    return _s.ReadUInt16();
                });
            transDict[typeof(uint)] = new Translator((NetStream _s, object _obj)
                => {
                    _s.WriteUInt32((uint)_obj);
                },
                (NetStream _s)
                => {
                    return _s.ReadUInt32();
                });
            transDict[typeof(ulong)] = new Translator((NetStream _s, object _obj)
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
            transDict[t] = new Translator((NetStream _s, object _obj)
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

        public static bool RegistCustom<T>(Writer _w, Reader _r)
        {
            Type t = typeof(T);
            if (transDict.ContainsKey(t))
                return false;
            transDict[t] = new Translator(_w, _r);
            return true;
        }


        public static bool Init()
        {
            RegistValueTypes();
            RegistCommonEnum();
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
