using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Wcf_Chat;

namespace ChatHost
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using (var host = new ServiceHost(typeof(Wcf_Chat.ServiceChat)))
            {
                host.Open();
                Console.WriteLine("Хост стартовал");
                Console.ReadLine();
            }
        }
    }
}
