using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Web;

namespace ExtractBingMapsCollectionUtilities
{

    /* Example of the HTML for a Bing Map showing a collection of places
     * See https://github.com/stevendirven1/Bing_Maps_Collections_Export_Python/blob/master/BingMapsCollectionExport.py
     * ypid is https://bpprodpublicstorage.blob.core.windows.net/bingplacesapi/BingPlaces_API_v1.0.pdf
     * Copilot suggests taking the ypid https://www.bing.com/maps?ypid=YN1029x8122487137375247842
     *This will redirect to e.g., https://www.bing.com/maps?ypid=YN1029x8122487137375247842&cp=51.51327%7E-0.11025&lvl=16.0
     * and then I can extract the cp from there.

    <li data-draggable="draggable" role="listitem" class="bm_draggableItem">
        <div class="collectionEntityRow" ordinal="1" data-entityid="9e7eb1db-a0d9-4695-828f-9d9cce942b5d">
        <a class="collectionEntity" data-tag="collectionItem" 
          data-entityid="9e7eb1db-a0d9-4695-828f-9d9cce942b5d"
          data-task="{&quot;type&quot;:&quot;LocalDetailsTask&quot;,&quot;state&quot;:{&quot;entryPoint&quot;:&quot;Collections&quot;,&quot;descriptionList&quot;:[&quot;p31: Jasper seals the letter dropped by Broadribb near Oliver Goldsmith's grave&quot;],&quot;id&quot;:&quot;ypid:YN1029x8122487137375247842&quot;,&quot;query&quot;:&quot;The Temple Church&quot;,&quot;taskTitle&quot;:&quot;Pontifex: Temple Church&quot;,&quot;originalName&quot;:&quot;The Temple Church&quot;,&quot;selectedCategoryId&quot;:&quot;30944&quot;,&quot;flippingCard&quot;:true,&quot;collectionId&quot;:&quot;24b06cf3-7ddb-4ee6-8c79-dfcc29ba6eae&quot;,&quot;savedEntityId&quot;:&quot;9e7eb1db-a0d9-4695-828f-9d9cce942b5d&quot;},&quot;activateOptions&quot;:{&quot;parentId&quot;:&quot;CollectionsTask$$2&quot;}}"
          href="#" >

          <!-- Contents of the data-task
            {
              "type":"LocalDetailsTask",
              "state":{
                "entryPoint":"Collections",
                "descriptionList":["p31: Jasper seals the letter dropped by Broadribb near Oliver Goldsmith's grave"],
                "id":"ypid:YN1029x8122487137375247842",
                "query":"The Temple Church",
                "taskTitle":"Pontifex: Temple Church",
                "originalName":"The Temple Church",
                "selectedCategoryId":"30944",
                "flippingCard":true,
                "collectionId":"24b06cf3-7ddb-4ee6-8c79-dfcc29ba6eae",
                "savedEntityId":"9e7eb1db-a0d9-4695-828f-9d9cce942b5d"
                },
              "activateOptions":{
                "parentId":"CollectionsTask$$2"
                }
            }

            -->

          <div class="collectionEntityContents" data-entityid="9e7eb1db-a0d9-4695-828f-9d9cce942b5d">
            <div class="gradientBar"></div>
            <div class="collectionEntityImageContainer">
              <div class="collectionEntityImagePlaceholder" style="display: none;"></div>
              <img role="none" class="collectionEntityImage wide" src="https://www.bing.com/th?id=OLC.2+KJJV7jk77Zbg480x360&amp;pid=Local&amp;w=248&amp;h=155&amp;r=0" alt="" data-bm="441" style="">
            </div>
            <div class="collectionEntityDetails">
              <div class="collectionEntityTitle" data-tag="collectionItemTitle" ordinal="1">Pontifex: Temple Church</div>
              <div class="collectionEntityAddress" data-tag="collectionItemAddress" style="">London, Greater London</div>
              <div class="collectionEntityMetaData" data-tag="collectionEntityMetaData" style="display: none;"></div>
            </div>
            <a role="button" class="bm_contextMenuButton" data-entityid="9e7eb1db-a0d9-4695-828f-9d9cce942b5d" data-tag="contextMenuButton" title="Additional actions on this entity" href="#" aria-label="Additional actions on this entity"></a>
          </div>
          <div class="cardDeleteConfirmationRow" data-tag="deleteConfirmationRow" data-entityid="9e7eb1db-a0d9-4695-828f-9d9cce942b5d">
            <div class="deleteConfirmationText">Are you sure you want to delete this place?</div>
            <span class="cardDeleteConfirmationButtonContainer">
              <a role="button" class="commonButtonControl cancelDeleteButton" data-tag="cancelDelete" href="#" aria-label="cancel collection entity delete">Cancel</a>
              <a role="button" class="commonButtonControl highlighted confirmDeleteButton" data-tag="confirmDelete" href="#" aria-label="confirm collection entity delete">Delete</a>
            </span>
          </div>
          <div class="editEntryRow" data-tag="editEntryRow" data-entityid="9e7eb1db-a0d9-4695-828f-9d9cce942b5d">
            <div class="editNicknameText">Nickname</div>
            <input type="text" class="nameEditBox" data-tag="nameEditBox" maxlength="100">
              <div class="editDescriptionText">Description</div>
              <textarea class="descriptionEditBox" data-tag="descriptionEditBox" contenteditable="true" rows="4" maxlength="1000" value=""></textarea>
              <span class="editRowButtonContainer">
                <a role="button" class="cancelEditButton commonButtonControl" data-tag="cancelEdit" href="#" aria-label="cancel collection entity edit">Cancel</a>
                <a role="button" class="confirmEditButton commonButtonControl" data-tag="confirmEdit" href="#" aria-label="confirm collection entity edit">Done</a>
              </span>
          </div>
        </a>
      </div>
    </li>


    */

    public class ParserResult
    {
        public string CollectionName { get; set; } = "Collection";
        public List<MapCollectionItem> Items { get; set; } = new List<MapCollectionItem>();
    }
    /// <summary>
    /// Given the HTML for a Bing Map that's showing a collection, parse out the items in the collection
    /// See example above for what the HTML looks like.
    /// Find the class="collectionEntity" for the 'a' tag and then pull out hte data-task value
    /// </summary>
    public static class BingMapsCollectionParser
    {
        public static ParserResult Parse(string html)
        {
            var retval = new ParserResult();


            // Grab the collection name from the HTML. It will look like this:
            // <div class="collectionPanelTitle cardTitle" data-tag="collectionPanelTitle" id="collectionTitleText">Thorndyke</div>
            const string collectionTitleIdMarker = "id=\"collectionTitleText\">";
            int collectionTitleIdIndex = html.IndexOf(collectionTitleIdMarker);
            if (collectionTitleIdIndex != -1)
            {
                var closeAngleIndex = html.IndexOf(">", collectionTitleIdIndex);
                if (closeAngleIndex != -1)
                {
                    closeAngleIndex ++; // move past the >
                    var endDivIndex = html.IndexOf("</div>", closeAngleIndex);
                    if (endDivIndex != -1)
                    {
                        var len = endDivIndex - closeAngleIndex;
                        var collectionName = html.Substring(closeAngleIndex, len);
                        collectionName = WebUtility.HtmlDecode(collectionName);

                        retval.CollectionName = collectionName;
                    }
                }
            }

            const string entityMarker = "class=\"collectionEntity\"";
            const string dataTaskMarker = "data-task=\"";
            int index = -1;
            while (true) // exits with breaks
            {
                index = html.IndexOf(entityMarker, index + 1);
                if (index == -1)
                {
                    break; // no more items
                }

                // From here on in, we always return a new item in the return array.
                var item = new MapCollectionItem();
                item.SourceDetails.StartLocation = index;
                retval.Items.Add(item);


                var dataTaskIndex = html.IndexOf(dataTaskMarker, index);
                if (dataTaskIndex == -1)
                {
                    item.SourceDetails.ItemParseResult = MapCollectionItemSourceDetails.ParseResult.NoDataTask;
                    item.SourceDetails.DataTaskRaw = html.Substring(index, 100);
                    break;
                }

                dataTaskIndex += dataTaskMarker.Length; // gets me to the double-quote
                var endQuoteIndex = html.IndexOf("\"", dataTaskIndex+1);

                if (endQuoteIndex == -1)
                {
                    item.SourceDetails.ItemParseResult = MapCollectionItemSourceDetails.ParseResult.NoDataTaskEndQuote;
                    item.SourceDetails.DataTaskRaw = html.Substring(index, 100);
                    break;
                }

                var len = endQuoteIndex - dataTaskIndex;
                item.SourceDetails.DataTaskRaw = html.Substring(dataTaskIndex, len);
                item.SourceDetails.DataTaskUnescaped = WebUtility.HtmlDecode(item.SourceDetails.DataTaskRaw);


                // Parse it!
                var deserialized = System.Text.Json.JsonSerializer.Deserialize<BingMapsCollectionItemJson>(item.SourceDetails.DataTaskUnescaped);
                if (deserialized == null)
                {
                    item.SourceDetails.ItemParseResult = MapCollectionItemSourceDetails.ParseResult.UnableToParseJson;
                    break;
                }

                item.TaskTitle = deserialized.state.taskTitle;
                item.DescriptionList = deserialized.state.descriptionList; 
                item.Id = deserialized.state.id;
                item.Query = deserialized.state.query;
                item.OriginalName = deserialized.state.originalName;
                item.ItemStatus = MapCollectionItem.Status.Ok;

                // Validate: make sure we either
                // -- have a query with a latitude,longitude (and id should be blank)
                // -- have a id with a ypid or sid (and query might be blank)
                var queryLatitudeLongitude = ParseQueryLatitudeLongitude(deserialized.state.query, LatitudeLongitudeParseOptions.JsonStyle);
                if (queryLatitudeLongitude.LatLongStatus == LatitudeLongitudeResult.Status.Ok)
                {
                    // check for a bad case
                    if (!string.IsNullOrEmpty(deserialized.state.id) && !deserialized.state.id.StartsWith("//no")) // default id is //no id//
                    {
                        item.ItemStatus = MapCollectionItem.Status.Other;
                        item.StatusDetails = "Both query and id present";
                    }
                    else
                    {
                        item.LatitudeLongitude = queryLatitudeLongitude;
                    }
                }
                else
                {
                    // Not a lat,long query. We should have an id with ypid or sid. The item.LatitudeLongitude will be filled in later
                    // by doing a lookup with the BingMapsYpidWorkQueue.
                    if (string.IsNullOrEmpty(deserialized.state.id) ||
                        !(deserialized.state.id.StartsWith("ypid:") || deserialized.state.id.StartsWith("sid:")))
                    {
                        item.ItemStatus = MapCollectionItem.Status.Other;
                        item.StatusDetails = "No lat,long in query, and id missing or not ypid/sid";
                    }
                }

                // Validate my understanding. Count the number of ": entries
                var ncolon = StringCount(item.SourceDetails.DataTaskUnescaped, "\":");
                int expected = 14;
                string notpresent = "";
                bool hasIdYpid = item.SourceDetails.DataTaskUnescaped.Contains("\"id\":\"ypid:");
                bool hasIdSid = item.SourceDetails.DataTaskUnescaped.Contains("\"id\":\"sid:");
                bool hasId = hasIdSid || hasIdYpid;
                if (!hasId)
                {
                    notpresent += "id(ypid sid) ";
                    expected--;
                }
                if (!item.SourceDetails.DataTaskUnescaped.Contains("\"collectionId\":\""))
                {
                    notpresent += "collectionId ";
                    expected--;
                }
                if (!item.SourceDetails.DataTaskUnescaped.Contains("\"selectedCategoryId\":\""))
                {
                    notpresent += "selectedCategoryId ";
                    expected--;
                }
                if (!item.SourceDetails.DataTaskUnescaped.Contains("\"originalName\":\""))
                {
                    notpresent += "originalName ";
                    expected--;
                }
                if (ncolon != expected)
                {
                    if (notpresent != "") notpresent = ", removed=" + notpresent.Substring(0, notpresent.Length - 1);
                    item.StatusDetails = $"Expected {expected} colons, found {ncolon}{notpresent}";
                }

                // Look for missing item
                if (item.TaskTitle.Contains("((taskTitle))"))
                {
                    item.ItemStatus = MapCollectionItem.Status.Other;
                    item.StatusDetails = "TaskTitle missing";
                }

            }

            return retval;
        }

        private static int StringCount(string str, string lookFor)
        {
            int count = 0;
            int index = -1;
            while (true)
            {
                index = str.IndexOf(lookFor, index + 1);
                if (index == -1)
                {
                    break;
                }
                count++;
            }
            return count;
        }


        public enum LatitudeLongitudeParseOptions {  JsonStyle, UrlStyle };
        /// <summary>
        /// Given a query like "51.413674, 1.189225" split into latitude and longitude. (Json style)
        /// Given a full url like ""https://www.bing.com/maps/?ypid=YN1029x324883948043716913&cp=51.532146~-0.093867&lvl=17.3" find the cp-value and split that into latitude and longitude. (Url style)
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static LatitudeLongitudeResult ParseQueryLatitudeLongitude(string query, LatitudeLongitudeParseOptions options)
        {
            var retval = new LatitudeLongitudeResult() { RawQuery = query };

            try
            {
                if (options == LatitudeLongitudeParseOptions.UrlStyle)
                {
                    // Is it a URL?
                    var url = new Uri(query); // query is e.g., ""https://www.bing.com/maps/?ypid=YN1029x324883948043716913&cp=51.532146~-0.093867&lvl=17.3"
                    var parsed = HttpUtility.ParseQueryString(url.Query); // pull out the ypid=YN1029x324883948043716913&cp=51.532146~-0.093867&lvl=17.3
                    var cp = parsed.Get("cp"); // just the 51.532146~-0.093867
                    if (string.IsNullOrEmpty(cp))
                    {
                        retval.LatLongStatus = LatitudeLongitudeResult.Status.UrlDoesNotContainCp;
                        retval.RawQuery += " No cp= in URL";
                        return retval;
                    }
                    if (!cp.Contains("~"))
                    {
                        retval.LatLongStatus = LatitudeLongitudeResult.Status.UrlDoesNotContainTilde;
                        retval.RawQuery += " No ~ in cp= value";
                        return retval;
                    }
                    cp = cp.Replace('~', ',');
                    query = cp;
                }

                var split = query.Split(',');
                switch (split.Length)
                {
                    case 2:
                        // good case: there are exactly two numbers.
                        // Latitude first.
                        var n1status = double.TryParse(split[0], out var n1);
                        var n2status = double.TryParse(split[1], out var n2);

                        if (!n1status)
                        {
                            retval.LatLongStatus = LatitudeLongitudeResult.Status.LatitudeNotANumber;
                        }
                        else if (n1 < -90)
                        {
                            retval.LatLongStatus = LatitudeLongitudeResult.Status.LatitudeTooSmall;
                        }
                        else if (n1 > 90)
                        {
                            retval.LatLongStatus = LatitudeLongitudeResult.Status.LatitudeTooLarge;
                        }
                        else if (!n2status) // Longitude
                        {
                            retval.LatLongStatus = LatitudeLongitudeResult.Status.LongitudeNotANumber;
                        }
                        else if (n2 < -180)
                        {
                            retval.LatLongStatus = LatitudeLongitudeResult.Status.LongitudeTooSmall;
                        }
                        else if (n2 > 180)
                        {
                            retval.LatLongStatus = LatitudeLongitudeResult.Status.LongitudeTooLarge;
                        }
                        else
                        {
                            retval.Latitude = n1;
                            retval.Longitude = n2;
                            retval.LatLongStatus = LatitudeLongitudeResult.Status.Ok;
                        }
                        break;

                    case 0:
                    case 1:
                        retval.LatLongStatus = LatitudeLongitudeResult.Status.NoComma;
                        break;

                    default:
                        retval.LatLongStatus = LatitudeLongitudeResult.Status.TooManyNumbers;
                        break;
                }
            }
            catch (Exception ex)
            {
                retval.LatLongStatus = LatitudeLongitudeResult.Status.ExceptionWhileParsing;
                retval.RawQuery += " Exception: " + ex.Message;
            }

            if (retval.LatLongStatus != LatitudeLongitudeResult.Status.Ok)
            {
                ; // Good place to put a breakpoint.
            }

            return retval;
        }


    }
}
