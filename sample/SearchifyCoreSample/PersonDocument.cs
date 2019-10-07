using System;
using System.Collections.Generic;
using Nest;
using Nest.Searchify;

namespace SearchifyCoreSample
{
    [ElasticsearchType(IdProperty = "Id", Name = "_doc")]
    public abstract class Document 
    {
        [Keyword]
        public string Id { get; set; }

        [Keyword(Name = "_docType")]
        public string DocType { get; set; }

        protected Document()
        {
            DocType = $"{GetType().FullName}, {GetType().Assembly.GetName().Name}";
        }
    }

    public class PersonDocument : Document
    {
        public string Name { get; set; }

        public int Age { get; set; }

        public FilterField Country { get; set; }

        public IEnumerable<FilterField> Tags { get; set; }
    }
}