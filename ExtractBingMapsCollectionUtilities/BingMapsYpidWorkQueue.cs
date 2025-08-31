using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Provider;

#if NET8_0_OR_GREATER // Always true for this file
#nullable disable
#endif

namespace ExtractBingMapsCollectionUtilities
{
    internal class BingMapYpidWorkQueue
    {
        public BingMapsYpidCache Cache { get; set; } = null;
        public int Add(BingMapsYpidWorkItem item)
        {
            WorkItems.Add(item);
            return WorkItems.Count;
        }
        private List<BingMapsYpidWorkItem> WorkItems { get; } = new List<BingMapsYpidWorkItem>();

        BingMapsYpidWorkItem CurrWorkItem = null;
        TextBlock UIStatus = null;
        public async Task Run(WebView2 webView, TextBlock uiStatus)
        {
            UIStatus = uiStatus;
            await webView.EnsureCoreWebView2Async();
            webView.CoreWebView2.NavigationCompleted += CoreWebView2_NavigationCompleted;
            webView.CoreWebView2.SourceChanged += CoreWebView2_SourceChanged;

            try
            {

                uiStatus.Text = $"Running {WorkItems.Count} work items.";
                foreach (var item in WorkItems)
                {
                    CurrWorkItem = item;
                    CurrWorkItem.Status = BingMapsYpidWorkItem.WorkItemStatus.Evaluating;
                    uiStatus.Text = $"YpidWorkQueue: Starting item {CurrWorkItem.Id}\n" + UIStatus.Text;
                    webView.CoreWebView2.Navigate(item.StartingUrl);

                    while (CurrWorkItem.Status == BingMapsYpidWorkItem.WorkItemStatus.Evaluating)
                    {
                        await Task.Delay(100);
                    }
                    await Task.Delay(5000); // TODO: how much time???
                    if (CurrWorkItem.ResponseUrl.Contains("cp=") && CurrWorkItem.ResponseUrl.Contains("~"))
                    {
                        uiStatus.Text = $"    Item {CurrWorkItem.Id} OK\n" + UIStatus.Text;
                        CurrWorkItem.Status = BingMapsYpidWorkItem.WorkItemStatus.Ok;
                        Cache.AddOrUpdate(CurrWorkItem);
                    }
                    else
                    {
                        uiStatus.Text = $"    Item {CurrWorkItem.Id} did not find CP\n" + uiStatus.Text;
                        CurrWorkItem.Status = BingMapsYpidWorkItem.WorkItemStatus.Other;
                    }
                }
            }
            catch (Exception ex)
            {
                uiStatus.Text = $"    ERROR: {ex.Message}\n" + uiStatus.Text;
            }

            webView.CoreWebView2.NavigationCompleted -= CoreWebView2_NavigationCompleted;

        }

        private void CoreWebView2_SourceChanged(Microsoft.Web.WebView2.Core.CoreWebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2SourceChangedEventArgs args)
        {
            if (CurrWorkItem == null) return; // can't happen
            switch (CurrWorkItem.Status)
            {
                case BingMapsYpidWorkItem.WorkItemStatus.Evaluating:
                    return; // None of the source changes matter until after the navigation completes.
            }

            // Just one source change is enough?

            UIStatus.Text = $"    *Status changed: {sender.Source}\n" + UIStatus.Text;

            CurrWorkItem.Status = BingMapsYpidWorkItem.WorkItemStatus.EvaluationCompleted;
            var decoded = System.Net.WebUtility.UrlDecode(sender.Source);
            CurrWorkItem.ResponseUrl = decoded;
            CurrWorkItem.LatitudeLongitude = BingMapsCollectionParser.ParseQueryLatitudeLongitude(decoded, BingMapsCollectionParser.LatitudeLongitudeParseOptions.UrlStyle);
        }

        private void CoreWebView2_NavigationCompleted(Microsoft.Web.WebView2.Core.CoreWebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs args)
        {
            if (CurrWorkItem == null) return; // can't happen
            UIStatus.Text = $"    Navigation complete: {sender.Source}\n" + UIStatus.Text;

            CurrWorkItem.Status = BingMapsYpidWorkItem.WorkItemStatus.NavigationComplete;
            var decoded = System.Net.WebUtility.UrlDecode(sender.Source);
            CurrWorkItem.ResponseUrl = decoded;
            CurrWorkItem.LatitudeLongitude = BingMapsCollectionParser.ParseQueryLatitudeLongitude(decoded, BingMapsCollectionParser.LatitudeLongitudeParseOptions.UrlStyle);
        }

        public override string ToString()
        {
            Dictionary<BingMapsYpidWorkItem.WorkItemStatus, int> statusCounts = new Dictionary<BingMapsYpidWorkItem.WorkItemStatus, int>();
            foreach (var item in WorkItems)
            {
                int value = 0;
                statusCounts.TryGetValue(item.Status, out value);
                value++;
                statusCounts[item.Status] = value;
            }
            return $"BingMapYpidWorkQueue: {WorkItems.Count} items";
        }
    }
}
