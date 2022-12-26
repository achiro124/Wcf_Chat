using System.ServiceModel;

namespace Wcf_Chat
{
    public class ServerUser
    {
        public int ID { get; set; }
        public string Login { get; set; }
        public bool Active { get; set; } = false;
        public OperationContext operationContext { get; set; }
    }
}
