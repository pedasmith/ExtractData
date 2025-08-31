using System;
using System.Collections.Generic;
using System.Text;
using Windows.Globalization;

namespace ExtractBingMapsCollectionUtilities
{
    internal class BingMapsYpidWorkItem
    {
        public enum WorkItemStatus {  NotStarted, Ok, Evaluating, NavigationComplete, SourceUpdated, EvaluationCompleted, Other };
        public WorkItemStatus Status = WorkItemStatus.NotStarted;


        /// <summary>
        /// Uses the id straight from the id field, e.g., ypid:YN1029x8122487137375247842
        /// </summary>
        public string Id { get; set; } = "ypid:YN1029x8122487137375247842";


        /// <summary>
        /// Uses Converted id: https://www.bing.com/maps/?ypid=YN1029x8122487137375247842
        /// Note that the normal colon : is replaced with an equals =
        /// </summary>
        public string StartingUrl
        {
            get
            {
                var webId = Id.Replace(":", "=");
                var retval = $"https://www.bing.com/maps/?{webId}";
                return retval;
            }
        }

        /// <summary>
        /// Is, e.g., https://www.bing.com/maps/?ypid=YN1029x8122487137375247842&cp=51.51327%7E-0.11025&lvl=16.0
        /// </summary>
        public string ResponseUrl { get; set; } = "";

        public LatitudeLongitudeResult LatitudeLongitude { get; set; } = new LatitudeLongitudeResult();

        public override string ToString()
        {
            return $"YpidWorkItem: status={Status} id={Id} At={LatitudeLongitude}";
        }
    }
}
