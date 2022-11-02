using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wcf_Chat
{
    public class PrivateMessage
    {
        int ID { get; set; }
        int From { get; set; }
        int To { get; set; }
        string Text { get; set; }
        DateTime dateTime { get; set; }

    }
}
