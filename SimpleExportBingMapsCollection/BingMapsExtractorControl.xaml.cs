using ExtractBingMapsCollectionUtilities;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.Storage.Pickers;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SimpleExportBingMapsCollection
{
    public sealed partial class BingMapsExtractorControl : UserControl
    {
        BingMapsYpidCache YpidCache = new BingMapsYpidCache();
        ParserResult FoundCollection = new ParserResult();
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

            // Load the help page
            uiHelpText.UriPrefix = "ms-appx:///Assets/Help/";
            //uiHelpText.LinkClicked += UiHelpText_LinkClicked;

            const string StartPage = "HelpForSimpleExport.md";
            await GotoAsync(StartPage);
        }

        private async Task GotoAsync(string filename)
        {
            try
            {
                StorageFolder InstallationFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
                string fname = @"Assets\Help\" + filename;
                var f = await InstallationFolder.GetFileAsync(fname);
                var fcontents = File.ReadAllText(f.Path);
                uiHelpText.Text = fcontents;
            }
            catch (Exception)
            {
            }
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

        private async Task NotifyUser(string title, string message, string url="", string urlcontent="")
        {
            ContentDialog notifyUserDialog = new ContentDialog
            {
                XamlRoot = this.XamlRoot,
                Title = title,
                Content = message,
                CloseButtonText = "OK"
            };

            if (url != "")
            {
                if (urlcontent == "") urlcontent = url;
                var content = new StackPanel();
                content.Children.Add(new TextBlock() { Text=message, TextWrapping=TextWrapping.Wrap, IsTextSelectionEnabled=true });
                var link = new HyperlinkButton() { Content = urlcontent };

                // Per 4438, mailto: doesn't work
                // https://github.com/microsoft/microsoft-ui-xaml/issues/4438
                link.Click += async (sender, e) =>
                {
                    //var subject = System.Web.HttpUtility.UrlEncode("Feedback for Simple Export for Bing Maps Collection", Encoding.ASCII); ;
                    var subject = Uri.EscapeDataString("Feedback for Simple Export for Bing Maps Collection"); ;
                    var mailtoUrl = new Uri(url + "?subject=" + subject);
                    await Windows.System.Launcher.LaunchUriAsync(mailtoUrl);
                };
                content.Children.Add(link);
                notifyUserDialog.Content = content;
            }

            ContentDialogResult result = await notifyUserDialog.ShowAsync();
        }

        private void Link_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
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
        /// Pulls the collection data from the HTML "goo"
        /// </summary>
        /// <returns></returns>
        private async Task DoExtractAsync()
        { 
            await uiWebView.EnsureCoreWebView2Async(); // Idiotic but necessary
            string htmlContent = await uiWebView.ExecuteScriptAsync("document.documentElement.outerHTML");
            if (string.IsNullOrEmpty(htmlContent))
            {
                await NotifyUser("Something went wrong", "No HTML was found. Did you navigate to the collection you want to export?");
                uiStatus.Text = "No HTML content found";
                return;
            }

            var content = System.Text.Json.JsonSerializer.Deserialize<string>(htmlContent); // Deserialize the JSON-encoded result
            if (string.IsNullOrEmpty(content))
            {
                if (string.IsNullOrEmpty(htmlContent))
                {
                    await NotifyUser("Something went wrong", "No HTML was discovered. Did you navigate to the collection you want to export?");
                    uiStatus.Text = "No HTML content found";
                    return;
                }
            }
            else
            {
                // MAIN CODE: grab our list of MapCollectionItem from the HTML.
                // And then just down below we'll try to get ypid values for those that don't have lat/long
                FoundCollection = BingMapsCollectionParser.Parse(content);

                if (FoundCollection.Items.Count == 0)
                { 
                    await NotifyUser("No collection found", "No collection was found. Did you navigate to the collection you want to export?");
                    uiStatus.Text = "No HTML content found";
                    FoundCollection = new ParserResult(); // reset to blank.
                    return;
                }
                List<MapCollectionItem> listNeedingLatitudeLongitude = new List<MapCollectionItem>();

                var str = "";

                // Try to get a ypid: location
                var queue = new BingMapYpidWorkQueue();
                queue.Cache = YpidCache;
                foreach (var item in FoundCollection.Items)
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

                // If there are any items, tell the user. I tried an experiment where if there was just one or two
                // to go ahead and do it, but it was a jarring and surprising experience.
                bool tellUser = listNeedingLatitudeLongitude.Count > 0;
                if (tellUser)
                {
                    var expectedTimeInMinutes = Math.Ceiling((listNeedingLatitudeLongitude.Count * 5.0) / 60.0);
                    await NotifyUser($"Must determine exact locations", $"The extraction will have to determine the exact location of some of your collection items.\nNumber of items: {listNeedingLatitudeLongitude.Count}\nExpected time: {expectedTimeInMinutes} minutes.");
                }
                var ypidDictionary = new Dictionary<MapCollectionItem, BingMapsYpidWorkItem>();
                foreach (var item in listNeedingLatitudeLongitude)
                {
                    var workItem = new BingMapsYpidWorkItem() { Id = item.Id };
                    ypidDictionary.Add(item, workItem);
                    int n = queue.Add(workItem);
                }

                uiDeveloper.Text = str;

                await queue.Run(uiWebView, uiDeveloper); // This will update the status as it goes. Cache is updated, too.
                // Update the cache and save the location data into the work items.
                var cacheFile = YpidCache.Save();
                str += $"Cache file: {cacheFile}\n";
                int nOK = 0;
                int nFailed = 0;
                foreach (var (mapitem, workItem) in ypidDictionary)
                {
                    if (workItem.Status == BingMapsYpidWorkItem.WorkItemStatus.Ok)
                    {
                        mapitem.LatitudeLongitude = workItem.LatitudeLongitude;
                        nOK++;
                        str += $"    Ypid: {workItem}\n";
                    }
                    else
                    {
                        nFailed++;
                        str += $"    ERROR: {workItem}\n";
                    }
                }

                if (tellUser)
                {
                    await NotifyUser("Finished discoving exact locations", $"The exact location of your items have been found.\nNumber found OK: {nOK}\nNumber failed: {nFailed}");
                }

                // Sort!.
                FoundCollection.Items = FoundCollection.Items.OrderBy((item) => item.TaskTitle).ToList();


                // Finally, output the full list to the developer tab

                foreach (var item in FoundCollection.Items)
                {
                    str += item.ToString() + "\n\n";
                }
                uiDeveloper.Text = str + "\n\n" + uiDeveloper.Text;

                uiStatus.Text = $"Extract: Number of items found: {FoundCollection.Items.Count}\n" + uiStatus.Text;

            }
        }

        private void OnFile_Exit(object sender, RoutedEventArgs e)
        {
            Application.Current.Exit();
        }

        private async void OnHelp_About(object sender, RoutedEventArgs e)
        {
            var package = Package.Current;
            var version = package.Id.Version;
            string appVersion = $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";

            await NotifyUser($"About {package.DisplayName}", 
                $"{package.DisplayName} version {appVersion}\n\nfrom Shipwreck Software.\n\n Got feedback? Email to ShipwreckSoftware@live.com is welcome any time (or press the button to open your email client)", "mailto:ShipwreckSoftare@live.com", "ShipwreckSoftware@live.com");
        }

        // Needed by the picker
        public static IntPtr AppWindowHandleHwnd = 0;

#if CODE_NOT_TESTABLE
// I no longer have a MapMaker account and cannot test the MapMaker CSV
// The CSV is similar to the Google Maps CSV, but with different columns names.
        private void OnFile_Export_MapMakerCsv(object sender, RoutedEventArgs e)
        {
            if (FoundCollection.Items.Count == 0)
            {
                uiStatus.Text = "No collection items found to export";
                return;
            }

            var str = "lat,lng,name,note\n";
            foreach (var item in FoundCollection.Items)
            {
                str += item.ToMapMakerCsv();
            }
            uiDeveloper.Text = str;

        }
#endif

        private string Make_GoogleMapsCsv()
        {
            var str = "latitude,longitude,title,notes\n";
            foreach (var item in FoundCollection.Items)
            {
                str += item.ToMapMakerCsv();
            }
            return str;
        }

        private string Make_GeoJson()
        {
            var str = "";
            foreach (var item in FoundCollection.Items)
            {
                if (str != "") str += ",\n";
                str += item.ToGeoJson();
            }
            str = "{\n    \"type\": \"FeatureCollection\",\n    \"features\": [\n" + str + "\n]}\n";
            return str;
        }
        private async Task<string> Make_HtmlAsync()
        {
            var filename = "simple.css";
            StorageFolder InstallationFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            string fname = @"Assets\" + filename;
            var f = await InstallationFolder.GetFileAsync(fname);
            var css_fcontents = File.ReadAllText(f.Path);

            var head = $"<head><title>{FoundCollection.CollectionName}</title>\n<style type=\"text/css\">\n{css_fcontents}\n</style></head>";
            var str = $"<html>{head}<body>\n<h1>{FoundCollection.CollectionName}</h1>\n";
            foreach (var item in FoundCollection.Items)
            {
                str += item.ToHtml();
            }
            str = str + $"\n</body></html>\n\n";
            return str;
        }
        private string Make_Markdown()
        {
            var str = "";
            foreach (var item in FoundCollection.Items)
            {
                str += item.ToMarkdown();
            }
            str = $"# {FoundCollection.CollectionName}\n\n" + str;
            return str;
        }

        private enum ExportType { CsvGoogle, GeoJson, Html, Markdown };

        private async Task DoExport(ExportType exportType)
        {
            uiDeveloper.Text = "";// clear it

            // The first time you export, you must have done an export.
            if (FoundCollection.Items.Count == 0)
            {
                await DoExtractAsync();
                if (FoundCollection.Items.Count == 0)
                {
                    uiStatus.Text = "No collection items found to export";
                    return;
                }
            }


            var savePicker = new Windows.Storage.Pickers.FileSavePicker();
#if UTTER_FAILURE_1
            // From Bing
            // Get the parent window of the FrameworkElement
            var window = Window.GetWindow(element);

            // Use IWindowNative to retrieve the HWND
            var windowNative = window.As<IWindowNative>();
            IntPtr hwnd = windowNative.WindowHandle;
#endif
            // UTTER_FAILURE_2 (wrong type) var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            // UTTER_FAILURE_3: can't recursively use the Parent to get a Window.
            // UTTER_FAILURE_$: can't use this.XamlRoot to get anything useful.

            var hWnd = AppWindowHandleHwnd; // Set by App :-(
            WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hWnd);

            savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            switch (exportType)
            {
                case ExportType.CsvGoogle:
                    savePicker.FileTypeChoices.Add("Geographical CSV", new List<string>() { ".csv" }); // Change #1: file type
                    break;
                case ExportType.GeoJson:
                    savePicker.FileTypeChoices.Add("GeoJSON", new List<string>() { ".geojson" }); // Change #1: file type
                    break;
                case ExportType.Html:
                    savePicker.FileTypeChoices.Add("HTML", new List<string>() { ".html" }); // Change #1: file type
                    break;
                case ExportType.Markdown:
                    savePicker.FileTypeChoices.Add("Markdown", new List<string>() { ".md" }); // Change #1: file type
                    break;
            }
            savePicker.SuggestedFileName = FoundCollection.CollectionName;
            var file = await savePicker.PickSaveFileAsync();

            if (file != null)
            {
                string str = "";
                switch (exportType)
                {
                    case ExportType.CsvGoogle:
                        str = Make_GoogleMapsCsv(); // Change #2: generate string
                        break;
                    case ExportType.GeoJson:
                        str = Make_GeoJson(); // Change #2: generate string
                        break;
                    case ExportType.Html:
                        str = await Make_HtmlAsync(); // Change #2: generate string
                        break;
                    case ExportType.Markdown:
                        str = Make_Markdown(); // Change #2: generate string
                        break;
                }

                await FileIO.WriteTextAsync(file, str);
                uiStatus.Text = $"Export complete to {file.Path}";
                uiDeveloper.Text += str;
            }
            else
            {
                uiStatus.Text = $"Export cancelled";
            }
        }


        private async void OnFile_Export_GoogleMapsCsv(object sender, RoutedEventArgs e)
        {
            await DoExport(ExportType.CsvGoogle);
        }
        private async void OnFile_Export_GeoJson(object sender, RoutedEventArgs e)
        {
            await DoExport(ExportType.GeoJson);
        }

        private async void OnFile_Export_Html(object sender, RoutedEventArgs e)
        {
            await DoExport(ExportType.Html);
        }
        private async void OnFile_Export_Markdown(object sender, RoutedEventArgs e)
        {
            await DoExport(ExportType.Markdown);
        }


        private async void OnFile_Reextract(object sender, RoutedEventArgs e)
        {
            uiStatus.Text = "";
            await DoExtractAsync();
        }

        private void OnHelp_Help(object sender, RoutedEventArgs e)
        {
            var vis = uiHelpBorder.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
            uiHelpBorder.Visibility = vis;
        }

        private void OnCloseHelpTap(object sender, TappedRoutedEventArgs e)
        {
            uiHelpBorder.Visibility = Visibility.Collapsed;
            uiDeveloperBorder.Visibility = Visibility.Collapsed;
        }

        private void OnDeveloper_Log(object sender, RoutedEventArgs e)
        {
            var vis = uiDeveloperBorder.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
            uiDeveloperBorder.Visibility = vis;
        }
    }
}
