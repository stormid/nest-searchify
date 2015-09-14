using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nest.Queryify.Abstractions;
using Nest.Queryify.Abstractions.Queries;
using Nest.Searchify.Abstractions;

namespace Nest.Searchify
{
    public class RepositorySearchService : ISearchService
    {
        private readonly IElasticsearchRepository _repository;

        public RepositorySearchService(IElasticsearchRepository repository)
        {
            _repository = repository;
        }

        public TSearchResult Search<TSearchParameters, TDocument, TSearchResult>(IElasticClientQueryObject<TSearchResult> query) where TSearchParameters : class, ICommonParameters where TDocument : class where TSearchResult : class, ISearchResult<TSearchParameters, TDocument>
        {
            return _repository.Query(query);
        }

        public ISearchResult<TSearchParameters, TDocument> Search<TSearchParameters, TDocument>(IElasticClientQueryObject<ISearchResult<TSearchParameters, TDocument>> query) where TSearchParameters : class, ICommonParameters where TDocument : class
        {
            return _repository.Query(query);
        }
    }
}
