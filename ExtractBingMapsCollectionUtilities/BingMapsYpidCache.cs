using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

#if NET8_0_OR_GREATER // Always true for this file
#nullable disable
#endif

namespace ExtractBingMapsCollectionUtilities
{
    internal class BingMapsYpidCache
    {
        private const string CacheFilePath = "c:\\temp\\2025\\TestingBingMapsCollection\\BingMapsYpidCache.json";
        private List<BingMapsYpidWorkItem> CachedItems { get; } = new List<BingMapsYpidWorkItem>();

        public BingMapsYpidWorkItem Get(string id)
        {
            foreach (var item in CachedItems)
            {
                if (item.Id == id)
                {
                    return item;
                }
            }
            return null;
        }

        public int AddOrUpdate(BingMapsYpidWorkItem item)
        {
            var existing = Get(item.Id);
            if (existing == null)
            {
                CachedItems.Add(item);
            }
            else
            {
                // Update 
                CachedItems.Remove(existing);
                CachedItems.Add(item);
            }
            return CachedItems.Count;
        }

        private static JsonSerializerOptions Options = new JsonSerializerOptions()
        {
            WriteIndented = true,
            IncludeFields = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            // The .NET JSON serializer by default incorrectly escapes characters such as & to be e.g. \u0026.
            // For my usage, where the JSON is not going to be embedded in HTML, this is unnecessary and makes the JSON harder to read.
            // See also https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/character-encoding#serialize-all-characters
            // https://www.rfc-editor.org/rfc/rfc8259
        };

        private static string ConvertToJson(List<BingMapsYpidWorkItem> list)
        {
            var retval = System.Text.Json.JsonSerializer.Serialize(list, Options);
            return retval;
        }

        /// <summary>
        /// Call this to re-save the cache to disk. See also the Restore() method.
        /// </summary>
        public void Save()
        {
            var list = new List<BingMapsYpidWorkItem>();
            foreach (var item in CachedItems)
            {
                if (item.Status == BingMapsYpidWorkItem.WorkItemStatus.Ok)
                {
                    list.Add(item);
                }
            }
            var json = System.Text.Json.JsonSerializer.Serialize(list, Options);
            System.IO.File.WriteAllText(CacheFilePath, json);
        }

        public void Restore()
        {
            if (System.IO.File.Exists(CacheFilePath))
            {
                var json = System.IO.File.ReadAllText(CacheFilePath);
                var list = System.Text.Json.JsonSerializer.Deserialize<List<BingMapsYpidWorkItem>>(json, Options);
                CachedItems.Clear();
                foreach (var item in list)
                {
                    CachedItems.Add(item);
                }
            }
        }
    }
}
