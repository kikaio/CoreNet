using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreNet.Utils
{
    public abstract class CoreLogger : IDisposable
    {
        public CoreLogger()
        { }

        public virtual void Dispose()
        {
            throw new NotImplementedException();
        }

        public abstract void Write(string _log);

        public abstract void WriteTrace(string _log, [CallerMemberName]string _mName = "", [CallerLineNumber]int _line = 0);

        public abstract void Error(string _log);

        [Conditional("DEBUG")]
        public abstract void WriteDebug(string _log = "");

        [Conditional("DEBUG")]
        public abstract void WriteDebugTrace(string _log = "", [CallerMemberName]string _mName = "", [CallerLineNumber]int _line = 0);

        [Conditional("DEBUG")]
        public abstract void WriteDebugWarn(string _log = "");

        [Conditional("DEBUG")]
        public abstract void WriteDebugError(string _log = "");
    }

}
