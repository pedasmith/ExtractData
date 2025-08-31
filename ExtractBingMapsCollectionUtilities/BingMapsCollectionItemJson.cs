using System;
using System.Collections.Generic;
using System.Text;

namespace ExtractBingMapsCollectionUtilities
{
    internal class BingMapsCollectionItemJson
    {
        public string type { get; set; } = "";
        public BingMapsCollectionItemState state { get; set; } = new BingMapsCollectionItemState();
        public BingMapsCollectionItemActivateOptions activateOptions { get; set; } = new BingMapsCollectionItemActivateOptions();

        public override string ToString()
        {
            return state.ToString(); // the rest is pretty uninteresting!
        }
    }

    internal class BingMapsCollectionItemState
    {
        public string entryPoint { get; set; } = "((entryPoint))";
        public List<string> descriptionList { get; set; } = new List<string>();
        public string id { get; set; } = "//no id//";
        public string query { get; set; } = "((query))";
        public string taskTitle { get; set; } = "((taskTitle))";
        public string originalName { get; set; } = "//no originalName//";
        public string selectedCategoryId { get; set; } = "((categoryId))";
        public bool flippingCard { get; set; } = true;
        public string collectionId { get; set; } = "((collectionId))";
        public string savedEntityId { get; set; } = "((savedEntityId))";

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"taskTitle={taskTitle}");
            if (descriptionList != null && descriptionList.Count > 0)
            {
                sb.AppendLine("descriptionList:");
                foreach (var desc in descriptionList)
                {
                    sb.AppendLine($"  {desc}");
                }
            }
            sb.AppendLine($"id={id}");
            sb.AppendLine($"query={query}");
            sb.AppendLine($"originalName={originalName}");
            sb.AppendLine($"entryPoint={entryPoint}");
            sb.AppendLine($"selectedCategoryId={selectedCategoryId}");
            sb.AppendLine($"flippingCard={flippingCard}");
            sb.AppendLine($"collectionId={collectionId}");
            sb.AppendLine($"savedEntityId={savedEntityId}");
            return sb.ToString();
        }
    }

    internal class BingMapsCollectionItemActivateOptions
    {
        public string parentId { get; set; } = "";

        public override string ToString()
        {
            return $"parentId={parentId}";
        }
    }   
}
