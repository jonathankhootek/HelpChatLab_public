using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace HelpChat.Evaluators
{
    //TODO Insert 7.1 below

    internal class NetworkEvaluator : IEvaluator
    {
        public bool Evaluate(string input)
        {
            string host = App.Current.Resources["MLEndpoint"].ToString();
            Ping ping = new Ping();

            try
            {
                PingReply reply = ping.Send(host, 200); // 200 ms timeout

                if (reply.Status == IPStatus.Success) //We are below 200 ms, doesn't matter what the number is
                {
                    Log.Information("🔎 Network evaluator OK");
                    return true;
                }
                else
                {
                    throw new Exception();
                }
            }
            catch (Exception ex)
            {
                Log.Information("🔎 Network evaluator fail");
                return false;
            }
        }
    }

    //Insert above
}
