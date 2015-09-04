using System;

namespace Nest.Searchify.SearchAggregations
{
    public class RangeKeyItem : RangeItem
    {
        public string Value { get; private set; }
        public string Text { get; private set; }

        public RangeKeyItem(RangeItem item)
        {
            DocCount = item.DocCount;
            From = item.From;
            To = item.To;
            FromAsString = item.FromAsString;
            ToAsString = item.ToAsString;
            Key = Text = Value = item.Key;
            Aggregations = SearchAggregationParser.Parse(item.Aggregations);

            var splitKey = Key.Split(new[] { FilterField.DefaultDelimiter }, StringSplitOptions.RemoveEmptyEntries);
            if (splitKey.Length == 2)
            {
                Value = splitKey[0];
                Text = splitKey[1];
            }
        }
    }
}