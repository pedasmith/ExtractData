using System;
using System.Collections.Generic;
using System.Text;

namespace ExtractBingMapsCollectionUtilities
{ 
 
    /// <summary>
    ///  See https://www.rfc-editor.org/rfc/pdfrfc/rfc7946.txt.pdf for protocol details.
    /// </summary>
    public class GeoJson
    {
        public GeoJson (MapCollectionItem value)
        {
            // GeoJSON says that Longitude comes first.
            geometry.coordinates[0] = value.LatitudeLongitude.Longitude;
            geometry.coordinates[1] = value.LatitudeLongitude.Latitude;
            properties.Add("name", value.TaskTitle);
            properties.Add("notes", value.DescriptionListAsString());
        }
        public string type { get; set; } = "Feature";
        public GeoJsonGeometry geometry { get; set; } = new GeoJsonGeometry();
        public Dictionary<string, string> properties { get; } = new Dictionary<string, string>();


        public class GeoJsonGeometry
        {
            public string type { get; set; } = "Point";
            public double[] coordinates { get; set; } = [0, 0];
        }
    }
}
