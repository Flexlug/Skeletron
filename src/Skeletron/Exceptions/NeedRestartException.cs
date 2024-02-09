using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skeletron.Exceptions
{
    /// <summary>
    /// Occurs when it is necessary to restart the bot
    /// </summary>
    internal class NeedRestartException : Exception
    {
        public NeedRestartException() { }
        public NeedRestartException(Exception ex) : base("See InnerException", ex) { }
    }
}