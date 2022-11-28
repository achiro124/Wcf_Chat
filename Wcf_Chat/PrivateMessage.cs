using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Wcf_Chat
{
    [DataContract]
    public class PrivateMessage
    {
        private static int num = 0000;

        [DataMember]
        public int ID { get; private set; }
        [DataMember]
        public int From { get; set; }
        [DataMember]
        public int To { get; set; }
        [DataMember]
        public string Text { get; set; }
        [DataMember]
        public DateTime dateTime { get; set; }

        public PrivateMessage()
        {
            ID = num;
            num++;
        }

    }
}
