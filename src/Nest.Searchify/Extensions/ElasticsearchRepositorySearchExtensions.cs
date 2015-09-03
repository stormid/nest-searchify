using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nest.Queryify.Abstractions;
using Nest.Queryify.Abstractions.Queries;
using Nest.Searchify.Abstractions;
using Nest.Searchify.Queries;

namespace Nest.Searchify.Extensions
{
    public static class ElasticsearchRepositorySearchExtensions
    {
        public static ISearchResponse<TDocument> Search<TParameters, TDocument>(
            this IElasticsearchRepository repository, CommonParametersQuery<TParameters, TDocument, TDocument> query, string index = null) where TParameters : ICommonParameters where TDocument : class
        {
            return repository.Search<TParameters, TDocument, TDocument>(query, index);
        }

        public static ISearchResponse<TReturnAs> Search<TParameters, TDocument, TReturnAs>(
            this IElasticsearchRepository repository, CommonParametersQuery<TParameters, TDocument, TReturnAs> query, string index = null) where TParameters : ICommonParameters where TDocument : class where TReturnAs : class
        {
            return repository.Query(query, index);
        }

        public static ISearchResponse<TDocument> Search<TDocument>(
            this IElasticsearchRepository repository, ICommonParameters parameters, string index = null) where TDocument : class
        {
            return repository.Search<ICommonParameters, TDocument>(new CommonParametersQuery<TDocument>(parameters), index);
        }
    }
}
