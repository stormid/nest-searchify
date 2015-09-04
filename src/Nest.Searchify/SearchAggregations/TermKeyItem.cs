using System;

namespace Nest.Searchify.SearchAggregations
{
    public class TermKeyItem : KeyItem
    {
        public string Value { get; private set; }
        public string Text { get; private set; }

        public TermKeyItem(KeyItem item)
        {
            DocCount = item.DocCount;
            Key = Text = Value = item.Key;
            KeyAsString = item.KeyAsString;
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