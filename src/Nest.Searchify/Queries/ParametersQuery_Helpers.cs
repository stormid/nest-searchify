using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Nest.Searchify.Abstractions;
using Nest.Searchify.Queries.Models;
using Nest.Searchify.SearchResults;

namespace Nest.Searchify.Queries
{
    public partial class ParametersQuery<TParameters, TDocument, TSearchResult, TOutputEntity> where TParameters : class, IPagingParameters, ISortingParameters, new()
        where TDocument : class
        where TOutputEntity : class
        where TSearchResult : SearchResult<TParameters, TDocument, TOutputEntity>
    {
        /// <summary>
        /// Creates a terms filter against the given document (<see cref="TFilterOnDocument"/>)
        /// </summary>
        /// <typeparam name="TFilterOnDocument">document to filter</typeparam>
        /// <typeparam name="TValue">filter value</typeparam>
        /// <param name="field">expression describing the field to filter on <see cref="TFilterOnDocument"/></param>
        /// <param name="value">value of the filter derived from your parameters</param>
        /// <returns>a filter that can be added to a query</returns>
        protected FilterContainer TermFilterFor<TFilterOnDocument, TValue>(Expression<Func<TFilterOnDocument, TValue>> field, TValue value) where TFilterOnDocument : class
        {
            return Filter<TFilterOnDocument>.Term(field, value);
        }

        /// <summary>
        /// Creates a terms filter against the document (<see cref="TDocument"/>)
        /// </summary>
        /// <typeparam name="TValue">filter value</typeparam>
        /// <param name="field">expression describing the field to filter on <see cref="TDocument"/></param>
        /// <param name="value">value of the filter derived from your parameters</param>
        /// <returns>a filter that can be added to a query</returns>
        protected FilterContainer TermFilterFor<TValue>(Expression<Func<TDocument, TValue>> field, TValue value)
        {
            return TermFilterFor<TDocument, TValue>(field, value);
        }

        /// <summary>
        /// Creates a terms filter against multiple values that must all exist in a document (<see cref="TFilterOnDocument"/>)
        /// </summary>
        /// <typeparam name="TFilterOnDocument">document to filter</typeparam>
        /// <typeparam name="TValue">filter value</typeparam>
        /// <param name="field">expression describing the field to filter on <see cref="TFilterOnDocument"/></param>
        /// <param name="values">values of the filter derived from your parameters</param>
        /// <returns>a filter that can be added to a query</returns>
        protected FilterContainer MultiTermAndFilterFor<TFilterOnDocument, TValue>(Expression<Func<TFilterOnDocument, TValue>> field, IEnumerable<TValue> values) where TFilterOnDocument : class
        {
            if (values != null)
            {
                return MultiTermAndFilterFor(field, values.ToArray());
            }
            return null;
        }

        /// <summary>
        /// Creates a terms filter against multiple values that must all exist in a document (<see cref="TDocument"/>)
        /// </summary>
        /// <typeparam name="TDocument">document to filter</typeparam>
        /// <typeparam name="TValue">filter value</typeparam>
        /// <param name="field">expression describing the field to filter on <see cref="TDocument"/></param>
        /// <param name="values">values of the filter derived from your parameters</param>
        /// <returns>a filter that can be added to a query</returns>
        protected FilterContainer MultiTermAndFilterFor<TValue>(Expression<Func<TDocument, TValue>> field, IEnumerable<TValue> values)
        {
            return MultiTermAndFilterFor<TDocument, TValue>(field, values);
        }

        /// <summary>
        /// Creates a terms filter against multiple values where at least 1 must exist in a document (<see cref="TDocument"/>)
        /// </summary>
        /// <typeparam name="TValue">filter value</typeparam>
        /// <param name="field">expression describing the field to filter on <see cref="TDocument"/></param>
        /// <param name="values">values of the filter derived from your parameters</param>
        /// <returns>a filter that can be added to a query</returns>
        protected FilterContainer MultiTermOrFilterFor<TValue>(Expression<Func<TDocument, TValue>> field, IEnumerable<TValue> values)
        {
            return MultiTermOrFilterFor<TDocument, TValue>(field, values);
        }

        /// <summary>
        /// Creates a terms filter against multiple values where at least 1 must exist in a document (<see cref="TFilterOnDocument"/>)
        /// </summary>
        /// <typeparam name="TFilterOnDocument">document to filter</typeparam>
        /// <typeparam name="TValue">filter value</typeparam>
        /// <param name="field">expression describing the field to filter on <see cref="TFilterOnDocument"/></param>
        /// <param name="values">values of the filter derived from your parameters</param>
        /// <returns>a filter that can be added to a query</returns>
        protected FilterContainer MultiTermOrFilterFor<TFilterOnDocument, TValue>(Expression<Func<TFilterOnDocument, TValue>> field, IEnumerable<TValue> values) where TFilterOnDocument : class
        {
            if (values != null)
            {
                return MultiTermOrFilterFor(field, values.ToArray());
            }
            return null;
        }

        /// <summary>
        /// Creates a terms filter against multiple values that must all exist in a document (<see cref="TDocument"/>)
        /// </summary>
        /// <typeparam name="TDocument">document to filter</typeparam>
        /// <typeparam name="TValue">filter value</typeparam>
        /// <param name="field">expression describing the field to filter on <see cref="TDocument"/></param>
        /// <param name="values">values of the filter derived from your parameters</param>
        /// <returns>a filter that can be added to a query</returns>

        protected FilterContainer MultiTermAndFilterFor<TValue>(Expression<Func<TDocument, TValue>> field, params TValue[] values)
        {
            return MultiTermAndFilterFor<TDocument, TValue>(field, values);
        }

        /// <summary>
        /// Creates a terms filter against multiple values that must all exist in a document (<see cref="TFilterOnDocument"/>)
        /// </summary>
        /// <typeparam name="TFilterOnDocument">document to filter</typeparam>
        /// <typeparam name="TValue">filter value</typeparam>
        /// <param name="field">expression describing the field to filter on <see cref="TFilterOnDocument"/></param>
        /// <param name="values">values of the filter derived from your parameters</param>
        /// <returns>a filter that can be added to a query</returns>

        protected FilterContainer MultiTermAndFilterFor<TFilterOnDocument, TValue>(Expression<Func<TFilterOnDocument, TValue>> field, params TValue[] values) where TFilterOnDocument : class
        {
            if (values != null)
            {
                var valuesList = values.ToList();

                if (valuesList.Any())
                {

                    var filters = valuesList.Select(
                        termFilter => new Func<FilterDescriptor<TFilterOnDocument>, FilterContainer>(filter => Filter<TFilterOnDocument>.Term(field, termFilter)))
                        .ToArray();

                    return Filter<TFilterOnDocument>.And(filters);
                }
            }
            return null;
        }

        /// <summary>
        /// Creates a terms filter against multiple values where at least 1 must exist in a document (<see cref="TDocument"/>)
        /// </summary>
        /// <typeparam name="TValue">filter value</typeparam>
        /// <param name="field">expression describing the field to filter on <see cref="TDocument"/></param>
        /// <param name="values">values of the filter derived from your parameters</param>
        /// <returns>a filter that can be added to a query</returns>

        protected FilterContainer MultiTermOrFilterFor<TValue>(Expression<Func<TDocument, TValue>> field, params TValue[] values)
        {
            return MultiTermOrFilterFor<TDocument, TValue>(field, values);
        }

        /// <summary>
        /// Creates a terms filter against multiple values where at least 1 must exist in a document (<see cref="TFilterOnDocument"/>)
        /// </summary>
        /// <typeparam name="TFilterOnDocument">document to filter</typeparam>
        /// <typeparam name="TValue">filter value</typeparam>
        /// <param name="field">expression describing the field to filter on <see cref="TFilterOnDocument"/></param>
        /// <param name="values">values of the filter derived from your parameters</param>
        /// <returns>a filter that can be added to a query</returns>

        protected FilterContainer MultiTermOrFilterFor<TFilterOnDocument, TValue>(Expression<Func<TFilterOnDocument, TValue>> field, params TValue[] values) where TFilterOnDocument : class
        {
            if (values != null)
            {
                var valuesList = values.ToList();

                if (valuesList.Any())
                {

                    var filters = valuesList.Select(
                        termFilter => new Func<FilterDescriptor<TFilterOnDocument>, FilterContainer>(filter => Filter<TFilterOnDocument>.Term(field, termFilter)))
                        .ToArray();

                    return Filter<TFilterOnDocument>.Or(filters);
                }
            }
            return null;
        }

        protected FilterContainer GeoDistanceFilter<TFilterOnDocument>(
            Expression<Func<TFilterOnDocument, object>> field, double latitude, double longitude, double distance,
            GeoUnit unit = GeoUnit.Miles, GeoOptimizeBBox optimize = GeoOptimizeBBox.None)
            where TFilterOnDocument : class
        {
            return Filter<TFilterOnDocument>.GeoDistance(field, geo => geo
                    .Location(latitude, longitude)
                    .Optimize(optimize)
                    .Distance(distance, unit));
        }

        protected FilterContainer GeoDistanceFilter<TFilterOnDocument>(
            Expression<Func<TFilterOnDocument, object>> field, GeoPoint point, double distance,
            GeoUnit unit = GeoUnit.Miles, GeoOptimizeBBox optimize = GeoOptimizeBBox.None)
            where TFilterOnDocument : class
        {
            return GeoDistanceFilter(field, point.Latitude, point.Longitude, distance, unit, optimize);
        }

        protected FilterContainer GeoBBoxFilter<TFilterOnDocument>(
            Expression<Func<TFilterOnDocument, object>> field, GeoBoundingBox boundingBox, GeoExecution? geoExecution = null)
            where TFilterOnDocument : class
        {
            return Filter<TFilterOnDocument>
                .GeoBoundingBox(field, 
                boundingBox.TopLeft.Longitude, 
                boundingBox.TopLeft.Latitude, 
                boundingBox.BottomRight.Longitude, 
                boundingBox.BottomRight.Latitude, 
                geoExecution);
        }

        protected FilterContainer GeoBBoxFilter<TFilterOnDocument>(
            Expression<Func<TFilterOnDocument, object>> field, GeoPoint topLeft, GeoPoint bottomRight, GeoExecution? geoExecution = null)
            where TFilterOnDocument : class
        {
            return GeoBBoxFilter(field, new GeoBoundingBox(topLeft, bottomRight));
        }
    }
}
