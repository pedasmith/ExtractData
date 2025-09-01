using System;
using System.Collections.Generic;
using System.Text;
using static ExtractBingMapsCollectionUtilities.BingMapsCollectionParser;

namespace ExtractBingMapsCollectionUtilities
{
    /*




     */
    public class MapCollectionItem
    {
        public enum Status {  Ok, Other };
        public Status ItemStatus = Status.Other;
        /// <summary>
        /// Internal information about why this was parsed the way it was.
        /// </summary>
        public string StatusDetails = ""; 

        public string TaskTitle = "(TaskTitle)";
        public List<string> DescriptionList = new List<string>();
        public string Address = "(Address)";
        public string ImageUrl = "";
        public string Id = "(Id)";
        public string Query = "(Query)";
        public string OriginalName = "(OriginalName)";
        public string SelectedCategoryId = "";
        public bool FlippingCard = false;
        public string CollectionId = "";

        // Derived from the query or from the id
        public LatitudeLongitudeResult LatitudeLongitude = new LatitudeLongitudeResult();

        // Details about where in the HTML this item was found
        public MapCollectionItemSourceDetails SourceDetails = new MapCollectionItemSourceDetails();
        public string DescriptionListAsString()
        {
            var retval = "";
            foreach (var item in DescriptionList)
            {
                retval += "    " + item + "\n";
            }
            return retval;
        }
        public string DescriptionListAsStringNoIndent()
        {
            var retval = "";
            foreach (var item in DescriptionList)
            {
                retval += item + "\n";
            }
            return retval;
        }
        private static System.Text.Json.JsonSerializerOptions GeoJsonOptions = new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true
        };

        public string ToGeoJson()
        {
            var geoJson = new GeoJson(this);
            var retval = System.Text.Json.JsonSerializer.Serialize(geoJson, GeoJsonOptions);
            return retval;
        }

        /// <summary>
        /// See https://maps.co/help/layers/importing-csv for the format. 
        /// Has 4 columns: lat, lng, name, note. Also works for Google CSV.
        /// </summary>
        /// <returns></returns>
        public string ToMapMakerCsv()
        {
            var retval = $"{LatitudeLongitude.Latitude},{LatitudeLongitude.Longitude},{CsvEscape.Escape(TaskTitle)},{CsvEscape.Escape(DescriptionListAsString())}\n";
            return retval;
        }

        public string ToMarkdown()
        {
            var retval = "";
            if (!string.IsNullOrEmpty(StatusDetails)) retval = $"NOTE: {StatusDetails}\n";
            switch (ItemStatus)
            {
                case Status.Ok:
                    // Reminder: DescriptionListAsString always inclues the trailing \n
                    var idstr = $"|ID|{Id}\n";
                    if (Id.StartsWith("//no")) idstr = ""; // suppress the id if it is not valid
                    var originalname = $"|Original name|{OriginalName}\n";
                    if (OriginalName.StartsWith("//no")) originalname = "";
                    var raw =SourceDetails.ToString().Replace("\r\n", "<br>").Replace("\n", "<br>").Replace("\r", "<br>");
                    retval += $"## {TaskTitle}\n{DescriptionListAsStringNoIndent()}### Details\n|Item|Value|\n|-----|-----|\nLocation|{LatitudeLongitude}\n{idstr}|Query|{Query}\n{originalname}|JSON|<small>{raw}</small>\n\n\n\n";
                    break;
                case Status.Other:
                default:
                    retval = $"ItemStatus: {ItemStatus} StatusDetails: {StatusDetails}\n{SourceDetails.ToString()}";
                    break;
            }
            return retval;
        }

        public override string ToString()
        {
            var retval = "";
            if (!string.IsNullOrEmpty(StatusDetails)) retval = $"NOTE: {StatusDetails}\n";
            switch (ItemStatus)
            {
                case Status.Ok:
                    // Reminder: DescriptionListAsString always inclues the trailing \n
                    var idstr = $"id={Id}\n";
                    if (Id.StartsWith ("//no")) idstr = ""; // suppress the id if it is not valid
                    var originalname = $"original name={OriginalName}\n";
                    if (OriginalName.StartsWith("//no")) originalname = "";
                    retval += $"title={TaskTitle}\n{DescriptionListAsString()}at={LatitudeLongitude}\n{idstr}query={Query}\n{originalname}\nRAW={SourceDetails.ToString()}\n------------\n\n\n";
                    break;
                case Status.Other:
                default:
                    retval = $"ItemStatus: {ItemStatus} StatusDetails: {StatusDetails}\n{SourceDetails.ToString()}";
                    break;
            }
            return retval;
        }
    }

    public class MapCollectionItemSourceDetails
    {
        public enum ParseResult { Ok, Other, NoDataTask, NoDataTaskEndQuote, UnableToParseJson }
        public ParseResult ItemParseResult = ParseResult.Other;
        public int StartLocation = -1;
        public string DataTaskRaw = "";
        public string DataTaskUnescaped = "";

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(DataTaskUnescaped))
            {
                return $"SourceDetails: result={ItemParseResult} start={StartLocation} unescaped={FormatUnescaped(DataTaskUnescaped)}";
            }
            return $"SourceDetails: result={ItemParseResult} start={StartLocation} raw={DataTaskRaw}";
        }

        private string FormatUnescaped(string unescaped)
        {
            return unescaped.Replace("\",", "\",\n    ").Replace("\"},", "\"},\n    ").Replace(":true,", ":true,\n    ").Replace("\"],", "\"],\n    ").Replace(":[],", ":[],\n    ");
        }
    }
}
