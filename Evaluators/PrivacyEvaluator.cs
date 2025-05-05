using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HelpChat.Evaluators
{
    //TODO Insert 6.1 below

    internal class PrivacyEvaluator : IEvaluator
    {
        public bool Evaluate(string input)
        {
            var isOk = !Regex.IsMatch(input, @"\d{5,}");

            Log.Information($"🔎 Privacy evaluator says {(isOk ? "OK" : "fail")}");

            return isOk;
        }
    }

    //Insert above
}
