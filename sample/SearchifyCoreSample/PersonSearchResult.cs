using Nest;
using Nest.Searchify.SearchResults;

namespace SearchifyCoreSample
{
    public class PersonSearchResult : SearchResult<PersonSearchParameters, PersonDocument>
    {
        public PersonSearchResult(PersonSearchParameters parameters, ISearchResponse<PersonDocument> response) : base(parameters, response)
        {
        }
    }
}