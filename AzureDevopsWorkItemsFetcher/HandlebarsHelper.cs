using HandlebarsDotNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AzureWorkItemsFetcher
{
    public class HandlebarsHelper
    {
        public static void GeneratePBIs(WorkItemsDetailResult workItemsData)
        {
            var data = new
            {
                workitems = workItemsData.value
            };

            var templateName = "PBIsTemplate.html";
            var template = "";
            using (var reader = File.OpenText($@"Templates\{templateName}"))
            {
                template = reader.ReadToEnd();
            }

            var hbTemplate = Handlebars.Compile(template);
            var result = hbTemplate(data);

            // Clear Directory
            var outputPath = "Output";
            DirectoryInfo di = new DirectoryInfo(outputPath);
            if (!di.Exists)
            {
                di.Create();
            }

            // Write To Files
            File.WriteAllText($@"{outputPath}\PBIs.html", result);
        }
    }
}
