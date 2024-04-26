using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Linq;
using System.Xml.Linq;
using System.Runtime.Remoting.Messaging;
using System;
using System.Reflection;
using Com.AiricLenz.OpenApi.JsonModel;
using System.Runtime.InteropServices;


// ============================================================================
// ============================================================================
// ============================================================================
namespace Com.AiricLenz.OpenApi
{

    // ============================================================================
    // ============================================================================
    // ============================================================================
    internal class Helper
    {

        private string _jsonSchemaDocument =
            System.Text.Encoding.Default.GetString(
                Properties.Resources.SchemaDocument);

        private string _jsonSchemaPath =
            System.Text.Encoding.Default.GetString(
                Properties.Resources.SchemaPath);


        // ============================================================================
        public bool GenerateOpenApiDocument(
            out string errorLog)
        {

            var rootPath =FindSolutionRootPath();
            
            Console.WriteLine("Solution Root Path:         " + rootPath);

            var assemblyPath = 
                Path.GetDirectoryName(
                    Assembly.GetExecutingAssembly().Location);

            Console.WriteLine("Assembly Path:              " + assemblyPath);

            string mainJsonConfigPath;

            var mainJsonConfigContent =
                LoadOpenApiMainConfig(
                    rootPath,
                    out mainJsonConfigPath);
                        
            if (string.IsNullOrWhiteSpace(mainJsonConfigContent))
            {
                errorLog =
                    "No main config-file (.json) was found! \n" +
                    "Please check the projects example folder!";

                return false;
            }

            var jsonConfig =
                JsonConvert.DeserializeObject<JsonModel.Document>(mainJsonConfigContent);

            var document = ParseConfigFileContent(jsonConfig);

            var paths = FindAllApiPaths(rootPath);

            foreach (var path in paths)
            {
                document.Paths.Add(
                    path.PathKey,
                    path.OpenApiPathItem);
            }

            string outputFileName =
                string.IsNullOrWhiteSpace(jsonConfig.OutputFile) ?
                jsonConfig.Title + ".json" :
                jsonConfig.OutputFile;

            var outputFilePath = outputFileName;

            if (!Path.IsPathRooted(outputFileName))
            {
                outputFilePath =
                    Path.Combine(assemblyPath, outputFileName);
            }
            
            SaveOpenApiDocumentToFile(
                document,
                outputFilePath);

            Console.Write("Generated OpenApi-Spec at:  ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(outputFilePath);
            Console.ForegroundColor = ConsoleColor.Gray;

            errorLog = string.Empty;
            return true;
        }



        #region Private



        // ============================================================================
        public void SaveOpenApiDocumentToFile(
            OpenApiDocument document,
            string filePathAndOrName)
        {
            using (var streamWriter = new StringWriter())
            {
                var writer = new OpenApiJsonWriter(streamWriter);
                document.SerializeAsV3(writer);
                var content = streamWriter.ToString();

                using (StreamWriter outputFile = new StreamWriter(filePathAndOrName))
                {
                    outputFile.WriteLine(content);
                    outputFile.Close();
                }
            }
        }


        // ============================================================================
        private string FindSolutionRootPath()
        {
            var appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            appPath = Path.GetDirectoryName(appPath);
            var currentRootPath = appPath;


            bool found = false;

            while (!found)
            {
                var files = Directory.GetFiles(currentRootPath);

                foreach (var fielPath in files)
                {
                    var suffix = Path.GetExtension(fielPath);

                    if (suffix.ToLower() == ".sln")
                    {
                        found = true;
                        return currentRootPath;
                    }
                }

                // move one folder up
                var oneLevelUp = Path.GetFullPath(Path.Combine(currentRootPath, @"..\"));
                if (oneLevelUp != currentRootPath)
                {
                    currentRootPath = oneLevelUp;
                }
                else
                {
                    break;
                }
            }

            return currentRootPath;
        }

        
        // ============================================================================
        private List<string> GetAllFilesOfTypeDownwards(
            string extension,
            string currentPath)
        {
            var resultList = new List<string>();
            
            var files = Directory.GetFiles(currentPath);
            var folders = Directory.GetDirectories(currentPath);

            if (!extension.StartsWith("."))
            {
                extension = "." + extension;
            }

            foreach (var file in files)
            {
                var localExtension = Path.GetExtension(file);

                if (extension == localExtension)
                {
                    resultList.Add(file);
                }
            }

            foreach (var subFolder in folders)
            {
                var subList =
                    GetAllFilesOfTypeDownwards(
                        extension,
                        subFolder);

                resultList.AddRange(subList);
            }

            return resultList;
        }
        


        


        // ============================================================================
        private string LoadOpenApiMainConfig(
            string rootPath,
            out string resultPath)
        {
            var jsonFiles =
                GetAllFilesOfTypeDownwards(
                    "json",
                    rootPath);

            foreach (var jsonFile in jsonFiles)
            {
                var fileContent =
                    File.ReadAllText(jsonFile);

                if (CheckAgainstJsonSchema(_jsonSchemaDocument, fileContent))
                {
                    resultPath = Path.GetDirectoryName(jsonFile);
                    return fileContent;
                }
            }

            resultPath = string.Empty;
            return string.Empty;
        }



        // ============================================================================
        private List<OpenApiPath> FindAllApiPaths(
            string currentPath)
        {
            var resultList = new List<OpenApiPath>();

            var files = FindAllFilesOfType(currentPath, ".json");
            var folders = Directory.GetDirectories(currentPath);

            foreach (var filePath in files)
            {
                var pathItems = CheckForOpenApiPathDefinitions(filePath);

                if (pathItems != null)
                {
                    resultList.AddRange(pathItems);
                }
            }

            foreach (var subFolder in folders)
            {
                var subList = FindAllApiPaths(subFolder);
                resultList.AddRange(subList);
            }

            return resultList;
        }




        // ============================================================================
        private OpenApiDocument ParseConfigFileContent(
            JsonModel.Document jsonConfig)
        {
            var document = new OpenApiDocument();
            document.Paths = new OpenApiPaths();

            // Document Info
            var documentInfo = new OpenApiInfo
            {
                Title = jsonConfig.Title,
                Description = jsonConfig.Description,
                Version = jsonConfig.Version
            };

            if (jsonConfig.Contact != null)
            {
                var oaContact = new OpenApiContact
                {
                    Name = jsonConfig.Contact.Name,
                    Email = jsonConfig.Contact.Email
                };

                documentInfo.Contact = oaContact;

            }

            document.Info = documentInfo;


            // Servers
            foreach (var server in  jsonConfig.Servers)
            {
                var oaServer = new OpenApiServer
                {
                    Url = server.Url,
                    Description = server.Description,
                };

                document.Servers.Add(oaServer);
            }

            return document;
        }


        // ============================================================================
        private List<OpenApiPath> CheckForOpenApiPathDefinitions(
            string filePath)
        {
            if (!File.Exists(filePath))
            {
                return null;
            }

            string contents;
            contents = File.ReadAllText(filePath);

            if (!CheckAgainstJsonSchema(_jsonSchemaPath, contents))
            {
                return new List<OpenApiPath>();
            }

            var pathItems =
                JsonConvert.DeserializeObject<JsonModel.PathItemList>(contents);

            var resultList =
                ParsePathListContent(
                    pathItems);

            return resultList;
        }


        // ============================================================================
        private List<OpenApiPath> ParsePathListContent(
            JsonModel.PathItemList pathItems)
        {
            var resultList =
                new List<OpenApiPath>();

            foreach (var item in pathItems.Paths)
            {
                var newPath = new OpenApiPath();

                newPath.PathKey = item.PathKey;
                newPath.OpenApiPathItem = new OpenApiPathItem();

                var operation = new OpenApiOperation
                {
                    OperationId = item.PathKey,
                    Parameters = new List<OpenApiParameter>(),
                    Responses = new OpenApiResponses(),
                    Description = item.Description
                };

                AddResponses(
                    ref operation,
                    item);

                AddAttributes(
                    ref operation,
                    item);

                AddRequestBody(
                    ref operation,
                    item);

                newPath.OpenApiPathItem.Operations.Add(
                    GetOperationType(item.Method),
                    operation);

                resultList.Add(newPath);
            }


            return resultList;
        }



        // ============================================================================
        private void AddResponses(
            ref OpenApiOperation operation,
            PathItem item)
        {
            foreach (var response in item.Responses)
            {
                operation.Responses.Add(
                    response.Code.ToString(),
                    new OpenApiResponse
                    {
                        Description = response.Description,
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                                {
                                    response.ContentType,
                                    new OpenApiMediaType()
                                }
                        }
                    }
                );
            }
        }


        // ============================================================================
        private void AddAttributes(
            ref OpenApiOperation operation,
            PathItem item)
        {
            if (item.Attributes == null)
            {
                return;
            }

            foreach (var attribute in item.Attributes)
            {
                operation.Parameters.Add(
                    new OpenApiParameter
                    {
                        Name = attribute.Name,
                        Schema = new OpenApiSchema
                        {
                            Type = attribute.Type
                        },
                        In = GetParameterLocation(attribute.Location),
                        AllowEmptyValue = !attribute.IsRequired,
                    }
                    );
            }
        }


        // ============================================================================
        private void AddRequestBody(
            ref OpenApiOperation operation,
            PathItem item)
        {
            if (item.RequestBody == null)
            {
                return;
            }

            operation.RequestBody = new OpenApiRequestBody
            {
                Description = item.RequestBody.Description,
                Content = new Dictionary<string, OpenApiMediaType>(),
                Required = item.RequestBody.IsRequired
            };

            var mediaType = new OpenApiMediaType
            {
                Schema = new OpenApiSchema
                {
                    Type = item.RequestBody.Schema.Type.ToLower(),
                    Properties = new Dictionary<string, OpenApiSchema>()
                }
            };
            
            foreach (var property in item.RequestBody.Schema.Properties)
            {
                mediaType.Schema.Properties.Add(
                    new KeyValuePair<string, OpenApiSchema>(
                        property.Name,
                        new OpenApiSchema() { Type = property.Type }
                    )
                );
            }

            operation.RequestBody.Content.Add(
                item.RequestBody.ContentType, 
                mediaType);
        }



        // ============================================================================
        private string[] FindAllFilesOfType(
            string currentPath,
            string fileExtension)
        {
            var resultList = new List<string>();

            if (!fileExtension.StartsWith("."))
            {
                fileExtension = "." + fileExtension.ToLower();
            }

            var files = Directory.GetFiles(currentPath);

            foreach (var filePath in files)
            {
                var currentExtension = Path.GetExtension(filePath).ToLower();

                if (currentExtension == fileExtension)
                {
                    resultList.Add(filePath);
                }
            }

            return resultList.ToArray();
        }



        // ============================================================================
        private bool CheckAgainstJsonSchema(
            string jsonSchema,
            string jsonContent)
        {
            try
            {
                var jSchema = JSchema.Parse(jsonSchema);
                var jObject = JObject.Parse(jsonContent);

                return jObject.IsValid(jSchema);
            }
            catch 
            { 
                return false; 
            }
        }


        // ============================================================================
        private ParameterLocation GetParameterLocation(
            string locationName)
        {
            switch (locationName.ToUpper())
            {
                case "QUERY":
                    return ParameterLocation.Query;

                case "HEADER":
                    return ParameterLocation.Header;

                case "PATH":
                    return ParameterLocation.Path;

                case "COOKIE":
                    return ParameterLocation.Cookie;

            }

            return ParameterLocation.Query;
        }
        



        // ============================================================================
        private OperationType GetOperationType(
            string typeName)
        {
            switch (typeName.ToUpper())
            {
                case "GET":
                    return OperationType.Get;

                case "PUT":
                    return OperationType.Put;

                case "POST":
                    return OperationType.Post;

                case "DELETE":
                    return OperationType.Delete;

                case "OPTIONS":
                    return OperationType.Options;

                case "HEAD":
                    return OperationType.Head;

                case "PATCH":
                    return OperationType.Patch;

                case "TRACE":
                    return OperationType.Trace;
            }

            return OperationType.Get;
        }

        #endregion

    }
}
