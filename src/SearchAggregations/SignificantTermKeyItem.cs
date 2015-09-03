using System;

namespace Nest.Searchify.SearchAggregations
{
    public class SignificantTermKeyItem : SignificantTermItem
    {
        public string Value { get; }
        public string Text { get; }

        public SignificantTermKeyItem(SignificantTermItem item)
        {
            DocCount = item.DocCount;
            BgCount = item.BgCount;
            Score = item.Score;

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