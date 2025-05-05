using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpChat.Evaluators
{
    internal class Evaluators
    {
        //TODO Insert 5.2 below

        List<IEvaluator> _evaluators = new List<IEvaluator>();

        internal Evaluators()
        {
            //TODO Insert 8.1 below
            _evaluators.Add(new NetworkEvaluator());
            _evaluators.Add(new PrivacyEvaluator());
            //Insert above
        }

        internal bool UseCloud(string input)
        {
            //To return true ("use the cloud"), we must either have no evaluators
            //or all evaluators must return true
            return (_evaluators.Count == 0 || _evaluators.All(x => x.Evaluate(input)));
        }

        //Insert above
    }
}
