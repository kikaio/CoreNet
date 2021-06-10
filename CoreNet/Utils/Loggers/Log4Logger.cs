using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CoreNet.Utils.Loggers
{
    public class Log4Logger : CoreLogger
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(Log4Logger));

        public override void Error(string _log)
        {
            logger.Error(_log);
        }

        public override void Write(string _log)
        {
            logger.Info(_log);
        }

        public override void WriteDebug(string _log = "")
        {
            logger.Debug(_log);
        }

        public override void WriteDebugError(string _log = "")
        {
            logger.Error(_log);
        }

        public override void WriteDebugTrace(string _log = "", [CallerMemberName] string _mName = "", [CallerLineNumber] int _line = 0)
        {
            logger.Debug($"[{_mName}::{_line}]{_log}");
        }

        public override void WriteDebugWarn(string _log = "")
        {
            logger.Warn(_log);
        }

        public override void WriteTrace(string _log, [CallerMemberName] string _mName = "", [CallerLineNumber] int _line = 0)
        {
            logger.Warn($"[{_mName}::{_line}]{_log}");
        }
    }

}
