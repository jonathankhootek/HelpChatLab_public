using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpChat.Evaluators
{
    // TODO Insert 5.1 below

    internal interface IEvaluator
    {
        //Return true if we are OK to use the cloud
        bool Evaluate(string input);
    }

    //Insert above
}
