

// ============================================================================
// ============================================================================
// ============================================================================
using Newtonsoft.Json.Schema;
using System.Collections.Generic;

namespace Com.AiricLenz.OpenApi.JsonModel
{

    // ============================================================================
    // ============================================================================
    // ============================================================================{
    internal class RequestBody
    {
        public string Description { get; set; }
        public string ContentType { get; set; }
        public bool IsRequired { get; set; }
        public Schema Schema {  get; set; } 

    }
}
