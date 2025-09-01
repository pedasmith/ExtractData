using ExtractBingMapsCollectionUtilities;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SimpleExportBingMapsCollection
{
    public sealed partial class BingMapsExtractorControl : UserControl
    {
        BingMapsYpidCache YpidCache = new BingMapsYpidCache();
        List<MapCollectionItem> FoundCollectionItems = new List<MapCollectionItem>();
        String FoundCollectionName = "";
        public BingMapsExtractorControl()
        {
            YpidCache.Restore();
            InitializeComponent();
            this.Loaded += BingMapsExtractorControl_Loaded;
        }

        private async void BingMapsExtractorControl_Loaded(object sender, RoutedEventArgs e)
        {
            await uiWebView.EnsureCoreWebView2Async(); // Idiotic but necessary
            //uiWebView.CoreWebView2.NavigationStarting += CoreWebView2_NavigationStarting;
            //uiWebView.CoreWebView2.NavigationCompleted += CoreWebView2_NavigationCompleted;
            uiWebView.CoreWebView2.SourceChanged += CoreWebView2_SourceChanged;
            // HAS_NO_VALUE: uiWebView.CoreWebView2.FrameNavigationStarting += CoreWebView2_FrameNavigationStarting;

            await DoGo();
        }

        private void CoreWebView2_SourceChanged(Microsoft.Web.WebView2.Core.CoreWebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2SourceChangedEventArgs args)
        {
            var srctext = (sender.Source == uiUrlTextBox.Text) ? "[nc]" : sender.Source;
            //uiStatus.Text = $"SourceChanged={srctext}   " + uiStatus.Text;
            uiUrlTextBox.Text = sender.Source;
        }

        private void CoreWebView2_NavigationCompleted(Microsoft.Web.WebView2.Core.CoreWebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs args)
        {
            var srctext = (sender.Source == uiUrlTextBox.Text) ? "[nc]" : sender.Source;
            uiStatus.Text = $"NavComplete={args.HttpStatusCode} Source={srctext}   " + uiStatus.Text;
        }

#if HAS_NO_VALUE
        private void CoreWebView2_FrameNavigationStarting(Microsoft.Web.WebView2.Core.CoreWebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs args)
        {
            uiStatus.Text = $"FrameURL={args.Uri}    " + uiStatus.Text;
        }
#endif

        private void CoreWebView2_NavigationStarting(Microsoft.Web.WebView2.Core.CoreWebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs args)
        {
            uiStatus.Text = $"NavUrl={args.Uri}    " + uiStatus.Text;
            if (args.Uri != uiUrlTextBox.Text)
            {
                // Update the URL box if needed
                uiUrlTextBox.Text = args.Uri;
            }
        }

        private async void GoButton_Click(object sender, RoutedEventArgs e)
        {
            await DoGo();
        }
        private async void OnDebug_Click(object sender, RoutedEventArgs e)
        {
            await uiWebView.EnsureCoreWebView2Async(); // Idiotic but necessary
            uiStatus.Text = ""; // "Source=" + uiWebView.Source;
        }

        private async Task DoGo()
        {
            try
            {
                var url = uiUrlTextBox.Text;
                await uiWebView.EnsureCoreWebView2Async(); // Idiotic but necessary
                uiWebView.CoreWebView2.Navigate(url);
            }
            catch (Exception ex)
            {
                uiStatus.Text = ex.Message;
            }
        }

        /// <summary>
        /// Read in the HTML from the web site
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnExtract_Click(object sender, RoutedEventArgs e)
        {
            await uiWebView.EnsureCoreWebView2Async(); // Idiotic but necessary
            string htmlContent = await uiWebView.ExecuteScriptAsync("document.documentElement.outerHTML");
            if (string.IsNullOrEmpty(htmlContent))
            {
                uiStatus.Text = "No HTML content found";
                return;
            }

            var content = System.Text.Json.JsonSerializer.Deserialize<string>(htmlContent); // Deserialize the JSON-encoded result
            if (!string.IsNullOrEmpty(content))
            {
                // MAIN CODE: grab our list of MapCollectionItem from the HTML.
                // And then just down below we'll try to get ypid values for those that don't have lat/long
                List<MapCollectionItem> listFromHtml = BingMapsCollectionParser.Parse(content);
                FoundCollectionName = BingMapsCollectionParser.ParsedCollectionName; // Ick, global variable. There a reasonable default.

                // Restric the size. Handy for debugging formats. listFromHtml = listFromHtml.GetRange(0, 4); 

                List<MapCollectionItem> listNeedingLatitudeLongitude = new List<MapCollectionItem>();

                var str = "";

                // Try to get a ypid: location
                var queue = new BingMapYpidWorkQueue();
                queue.Cache = YpidCache;
                foreach (var item in listFromHtml)
                {
                    if (item.LatitudeLongitude.LatLongStatus != LatitudeLongitudeResult.Status.Ok)
                    {
                        if (item.Id.StartsWith("//no"))
                        {
                            continue;
                        }
                        var cachedValue = YpidCache.Get(item.Id);
                        if (cachedValue != null)
                        {
                            // Use the cached value
                            str += $"Cached: {cachedValue}\n";
                            item.LatitudeLongitude = cachedValue.LatitudeLongitude;
                            continue;
                        }

                        listNeedingLatitudeLongitude.Add(item);
                    }
                }

                var ypidDictionary = new Dictionary<MapCollectionItem, BingMapsYpidWorkItem>();
                foreach (var item in listNeedingLatitudeLongitude)
                {
                    var workItem = new BingMapsYpidWorkItem() { Id = item.Id };
                    ypidDictionary.Add(item, workItem);
                    int n = queue.Add(workItem);
                    // TODO: should there be a limit? Try all for now! if (n >= 1) break; // for now, limit how many
                }

                uiDeveloper.Text = str;

                await queue.Run(uiWebView, uiDeveloper); // This will update the status as it goes. Cache is updated, too.

                // Update the cache and save the location data into the work items.
                YpidCache.Save();
                foreach (var (mapitem, workItem) in ypidDictionary)
                {
                    if (workItem.Status == BingMapsYpidWorkItem.WorkItemStatus.Ok)
                    {
                        mapitem.LatitudeLongitude = workItem.LatitudeLongitude;
                        str += $"    Ypid: {workItem}\n";
                    }
                    else
                    {
                        str += $"    ERROR: {workItem}\n";
                    }
                }


                // Finally, output the full list. TODO: for now, just send it to the uiDeveloper text box.
                FoundCollectionItems = listFromHtml;

                foreach (var item in listFromHtml)
                {
                    str += item.ToString() + "\n\n";
                }
                uiDeveloper.Text = str + "\n\n" + uiDeveloper.Text;
                uiStatus.Text = $"N Items: {listFromHtml.Count}\n" + uiStatus.Text;

            }
        }

        private void OnFile_Exit(object sender, RoutedEventArgs e)
        {

        }

        private void OnHelp_About(object sender, RoutedEventArgs e)
        {

        }

        // Needed by the picker
        public static IntPtr AppWindowHandleHwnd = 0;

        private void OnFile_Export_MapMakerCsv(object sender, RoutedEventArgs e)
        {
            if (FoundCollectionItems == null || FoundCollectionItems.Count == 0)
            {
                uiStatus.Text = "No collection items found to export";
                return;
            }

            var picker = new Windows.Storage.Pickers.FileSavePicker();

            var str = "lat,lng,name,note\n";
            foreach (var item in FoundCollectionItems)
            {
                str += item.ToMapMakerCsv();
            }
            uiDeveloper.Text = str;

        }

        private string Make_GoogleMapsCsv()
        {
            var str = "latitude,longitude,title,notes\n";
            foreach (var item in FoundCollectionItems)
            {
                str += item.ToMapMakerCsv();
            }
            return str;
        }

        private string Make_GeoJson()
        {
            var str = "";
            foreach (var item in FoundCollectionItems)
            {
                if (str != "") str += ",\n";
                str += item.ToGeoJson();
            }
            str = "{\n    \"type\": \"FeatureCollection\",\n    \"features\": [\n" + str + "\n]}\n";
            return str;
        }

        private async void OnFile_Export_GoogleMapsCsv(object sender, RoutedEventArgs e)
        {
            if (FoundCollectionItems == null || FoundCollectionItems.Count == 0)
            {
                uiStatus.Text = "No collection items found to export";
                return;
            }

            var savePicker = new Windows.Storage.Pickers.FileSavePicker();
            WinRT.Interop.InitializeWithWindow.Initialize(savePicker, AppWindowHandleHwnd);

            savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            savePicker.FileTypeChoices.Add("Geographical CSV", new List<string>() { ".csv" }); // Change #1: file type
            savePicker.SuggestedFileName = FoundCollectionName;
            var file = await savePicker.PickSaveFileAsync();

            if (file != null)
            {
                string str = Make_GoogleMapsCsv(); // Change #2: generate string
                await FileIO.WriteTextAsync(file, str);
                uiStatus.Text = $"Export complete to {file.Path}";
                uiDeveloper.Text = str;
            }
            else
            {
                uiStatus.Text = $"Export cancelled";
            }
        }
        private async void OnFile_Export_GeoJson(object sender, RoutedEventArgs e)
        {
            if (FoundCollectionItems == null || FoundCollectionItems.Count == 0)
            {
                uiStatus.Text = "No collection items found to export";
                return;
            }

            var savePicker = new Windows.Storage.Pickers.FileSavePicker();
            WinRT.Interop.InitializeWithWindow.Initialize(savePicker, AppWindowHandleHwnd);

            savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            savePicker.FileTypeChoices.Add("GeoJSON", new List<string>() { ".geojson" }); // Change #1: file type
            savePicker.SuggestedFileName = FoundCollectionName;
            var file = await savePicker.PickSaveFileAsync();

            if (file != null)
            {
                string str = Make_GeoJson(); // Change #2: generate string
                await FileIO.WriteTextAsync(file, str);
                uiStatus.Text = $"Export complete to {file.Path}";
                uiDeveloper.Text = str;
            }
            else
            {
                uiStatus.Text = $"Export cancelled";
            }
        }
    }
}
