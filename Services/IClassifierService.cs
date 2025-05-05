using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpChat.Services
{
    internal interface IClassifierService
    {
        Task<string> GetClassifiedLabel(string input, string[] labels);
    }
}
