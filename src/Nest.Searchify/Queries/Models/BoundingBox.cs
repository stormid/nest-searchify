namespace Nest.Searchify.Queries.Models
{
    public struct BoundingBox
    {
        public GeoPoint TopLeft { get; }
        public GeoPoint BottomRight { get; }

        public BoundingBox(GeoPoint topLeft, GeoPoint bottomRight)
        {
            BottomRight = bottomRight;
            TopLeft = topLeft;
        }
    }
}