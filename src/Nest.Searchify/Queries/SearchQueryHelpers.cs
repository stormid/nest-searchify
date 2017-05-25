using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Nest.Searchify.Queries
{
    public static class SearchQueryHelpers
    {
        public static QueryContainer GeoDistanceFilter<T>(Expression<Func<T, GeoLocation>> field, GeoLocation location, Distance distance) where T : class
        {
            return Query<T>
                .GeoDistance(g => g
                    .Field(field)
                    .Location(location)
                    .Distance(distance)
                );
        }

        public static QueryContainer GeoDistanceFilter<T>(Expression<Func<T, GeoLocation>> field, GeoLocation location, double? radius, DistanceUnit distanceUnit = DistanceUnit.Miles) where T : class
        {
            if (!radius.HasValue)
            {
                return null;
            }

            return Query<T>
                .GeoDistance(g => g
                    .Field(field)
                    .Location(location)
                    .Distance(radius.Value, distanceUnit)
                );
        }

        public static QueryContainer MultiTermOrFilterFor<T, TValue>(Expression<Func<T, object>> field, IEnumerable<TValue> terms)
            where T : class
        {
            return Query<T>.Terms(t => t.Field(field).Terms(terms));
        }

        public static QueryContainer MultiTermAndFilterFor<T, TValue>(Expression<Func<T, object>> field, IList<TValue> terms)
            where T : class
        {
            if (terms == null || !terms.Any())
            {
                return null;
            }

            var queryContainer = new QueryContainer();
            return terms.Aggregate(queryContainer, (current, term) => current && Query<T>.Term(field, term));
        }


        public static QueryContainer TermFilterFor<T, TValue>(Expression<Func<T, object>> field, TValue term)
            where T : class
        {
            return Query<T>.Term(t => t.Field(field).Value(term));
        }

        public static QueryContainer NestedTermsAndFilterFor<T, TValue>(Expression<Func<T, object>> path, Expression<Func<T, object>> field, IList<TValue> terms)
            where T : class
        {
            if (terms == null || !terms.Any())
            {
                return null;
            }

            return Query<T>
                .Nested(n => n.Path(path)
                    .Query(q => MultiTermAndFilterFor(field, terms))
                );
        }

        public static QueryContainer NestedTermsOrFilterFor<T, TValue>(Expression<Func<T, object>> path, Expression<Func<T, object>> field, IEnumerable<TValue> terms)
            where T : class
        {
            return Query<T>
                .Nested(n => n.Path(path)
                    .Query(q => MultiTermOrFilterFor(field, terms))
                );
        }

        public static QueryContainer NestedTermFilterFor<T, TValue>(Expression<Func<T, object>> path, Expression<Func<T, object>> field, TValue term)
            where T : class
        {
            return Query<T>
                .Nested(n => n.Path(path)
                    .Query(q => TermFilterFor(field, term))
                );
        }
    }
}
