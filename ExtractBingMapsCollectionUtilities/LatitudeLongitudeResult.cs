using System;
using System.Collections.Generic;
using System.Text;

namespace ExtractBingMapsCollectionUtilities
{
    /// <summary>
    /// Result of parsing a latitude,longitude pair from a string. Parser is ParseQueryLatitudeLongitude and get parse
    /// from either a Json query or a URL.
    /// </summary>
    public class LatitudeLongitudeResult
    {
        public enum Status { Ok, NotSet, OtherError, 
            NoComma, TooManyNumbers, ExceptionWhileParsing,
            UrlDoesNotContainCp, UrlDoesNotContainTilde,
            LatitudeNotANumber, LatitudeTooSmall, LatitudeTooLarge, LongitudeNotANumber, LongitudeTooSmall, LongitudeTooLarge };
        public Status LatLongStatus { get; set; } = Status.OtherError;
        public string RawQuery { get; set; } = "(not initialized)";


        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public override string ToString()
        {
            if (LatLongStatus == Status.Ok)
            {
                return $"Lat={Latitude} Long={Longitude}";
            }
            else
            {
                return $"LatLongStatus={LatLongStatus} RawQuery={RawQuery}";
            }
        }
    }
}
