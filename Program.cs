using Newtonsoft.Json;
using System.IO;

namespace GlobalData.Agent.Acquisition
{
    class Program
    {
        static void Main(string[] args)
        {
            var agent = new AcquisitionAgent("COS2","GlobalData","Agent.Acquisition");
            agent.Configure();
            agent.Start();
        }
    }
}
