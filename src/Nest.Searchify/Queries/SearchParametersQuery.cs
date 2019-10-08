using System;
using Nest.Searchify.Abstractions;
using Nest.Searchify.SearchResults;

namespace Nest.Searchify.Queries
{
    public class SearchParametersQuery<TSearchParameters, TDocument, TSearchResult> : SearchParametersQuery<TSearchParameters, TDocument, TSearchResult, TDocument>
		where TSearchParameters : class, ISearchParameters, new()
	where TDocument : class
        where TSearchResult : SearchResult<TSearchParameters, TDocument>
	{
        public SearchParametersQuery(TSearchParameters parameters) : base(parameters)
        {
        }

        public SearchParametersQuery(TSearchParameters parameters, string docTypeName, Func<string> docTypeValueFunc) : base(parameters, docTypeName, docTypeValueFunc)
        {
        }
    }

    public class SearchParametersQuery<TSearchParameters, TDocument, TSearchResult, TOutputEntity> :
        ParametersQuery<TSearchParameters, TDocument, TSearchResult, TOutputEntity>
        where TSearchParameters : class, ISearchParameters, new()
        where TDocument : class
        where TOutputEntity : class
        where TSearchResult : SearchResult<TSearchParameters, TDocument, TOutputEntity>
    {
        protected static string DocTypeValueFuncDefault = $"{typeof(TDocument).FullName}, {typeof(TDocument).Assembly.GetName().Name}";

        private readonly string docTypeName;
        private readonly Func<string> docTypeValueFunc;

        public SearchParametersQuery(TSearchParameters parameters) : base(parameters)
        {
        }

        public SearchParametersQuery(TSearchParameters parameters, string docTypeName, Func<string> docTypeValueFunc) : base(parameters)
        {
            this.docTypeName = docTypeName;
            this.docTypeValueFunc = docTypeValueFunc;
        }

        protected virtual QueryContainer WithQuery(IQueryContainer query, string queryTerm)
        {
            return !string.IsNullOrWhiteSpace(queryTerm)
                ? Query<TDocument>.QueryString(q => q.Query(queryTerm))
                : Query<TDocument>.MatchAll();
        }

        protected virtual QueryContainer BuildQuery(TSearchParameters parameters, QueryContainer query, QueryContainer filters)
        {
            return Query<TDocument>
                .Bool(b => b
                    .Must(query)
                    .Filter(DocTypeFilterCore(docTypeName, docTypeValueFunc?.Invoke() ?? DocTypeValueFuncDefault, parameters) && filters)
                );
        }

        protected QueryContainer DocTypeFilterCore(string docTypeFieldName, string docTypeValue, TSearchParameters parameters)
        {
            if (!string.IsNullOrWhiteSpace(docTypeName))
            {
                return DocTypeFilter(docTypeFieldName, docTypeValue, parameters);
            }

            return null;
        }

        protected virtual QueryContainer DocTypeFilter(string docTypeFieldName, string docTypeValue, TSearchParameters parameters)
        {
            return Query<TDocument>.Term(docTypeFieldName, docTypeValue);
        }

        protected virtual QueryContainer WithFilters(IQueryContainer query, TSearchParameters parameters)
        {
            return null;
        }

        protected sealed override QueryContainer BuildQueryCore(QueryContainer query, TSearchParameters parameters)
        {
            return BuildQuery(
                parameters,
                WithQuery(query, parameters.Query),
                WithFilters(query, parameters)
            );
        }
    }
}
