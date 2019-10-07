using Nest;
using Nest.Searchify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchifyCoreSample
{
    public class SportingTeamDocument : Document
    {
        public string Name { get; set; }

        public FilterField SportType { get; set; }
    }
}
