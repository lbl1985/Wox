using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wox.Plugin;

namespace SwitchProcess
{
    public class Main : IPlugin
    {
        public List<Result> Query(Query query)
        {
            var result = new Result
            {
                Title = "Switch Process",
                SubTitle = $"Query: {query.Search}",
                IcoPath = "app.png"
            };
            return new List<Result> { result };
        }

        public void Init(PluginInitContext context)
        {

        }
    }
}
