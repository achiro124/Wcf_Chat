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
        [DataMember]
        public string ID { get; private set; }
        [DataMember]
        public int FromId { get; set; }
        [DataMember]
        public int ToId { get; set; }
        [DataMember]
        public string Msg { get; set; }
        [DataMember]
        public DateTime dateTime { get; set; }

        public PrivateMessage()
        {
            ID = Guid.NewGuid().ToString();
        }

        public PrivateMessage(string Id)
        {
            ID = Id;
        }
    }
}
