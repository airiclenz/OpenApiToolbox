using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


// ============================================================================
// ============================================================================
// ============================================================================
namespace Com.AiricLenz.OpenApi
{

    // ============================================================================
    // ============================================================================
    // ============================================================================
    internal class OpenApiPath
    {

        private string _pathKey;

        public OpenApiPathItem OpenApiPathItem { get; set; }


        // ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        public string PathKey 
        { 
            get
            {
                return _pathKey;
            }
            set
            {
                if (value.StartsWith("/"))
                {
                    _pathKey = value;
                }
                else
                {
                    _pathKey = "/" + value;
                }
            }
        }


        // ============================================================================
        public OpenApiPath()
        {
            OpenApiPathItem = new OpenApiPathItem();
            PathKey = string.Empty;
        }


    }
}
