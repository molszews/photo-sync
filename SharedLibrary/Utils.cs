using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SharedLibrary
{
    public class Utils
    {
        public static void Retry(Action a, Action<Exception> b, int retries, int sleepInSec = 0)
        {
            var retriesLeft = retries;
            while (retriesLeft > 0)
            {
                try
                {
                    a();
                }
                catch (Exception e)
                {
                    b(e);

                    retriesLeft--;
                    Thread.Sleep(TimeSpan.FromSeconds(sleepInSec));
                }
            }
        }
    }
}
