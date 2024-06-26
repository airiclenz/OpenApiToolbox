using System.Collections.Generic;


// ============================================================================
// ============================================================================
// ============================================================================
namespace Com.AiricLenz.OpenApi.JsonModel
{

    // ============================================================================
    // ============================================================================
    // ============================================================================
    internal class PathItemList
    {
        public List<PathItem> Paths;
    }

    // ============================================================================
    // ============================================================================
    // ============================================================================
    internal class PathItem
    {

        public string PathKey { get; set; }
        public string Method { get; set; }
        public string Description { get; set; }


        public List<Response> Responses { get; set; }

        public List<Attribute> Attributes { get; set; }

        public RequestBody RequestBody { get; set; }

    }

}