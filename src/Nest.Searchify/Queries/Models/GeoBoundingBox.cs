namespace Nest.Searchify.Queries.Models
{
    public class GeoBoundingBox
    {
        public GeoPoint TopLeft { get; set; }
        public GeoPoint BottomRight { get; set; }

        public GeoBoundingBox()
        {
            
        }

        public GeoBoundingBox(GeoPoint topLeft, GeoPoint bottomRight) : this()
        {
            BottomRight = bottomRight;
            TopLeft = topLeft;
        }
    }
}