using System.Collections.Generic;


// ============================================================================
// ============================================================================
// ============================================================================
namespace Com.AiricLenz.OpenApi.JsonModel
{


    // ============================================================================
    // ============================================================================
    // ============================================================================
    internal class Document

    {

        public string OutputFile { get; set; }

        public string Title { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }
        public string License { get; set; }


        public Contact Contact { get; set; }
		public List<Server> Servers { get; set; }

    }

}