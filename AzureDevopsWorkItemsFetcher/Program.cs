using System;
using System.Linq;
using System.Threading.Tasks;

namespace AzureWorkItemsFetcher
{
    class Program
    {
        static string personalAccessToken;

        static void Main(string[] args)
        {

            if (args.Length == 0)
            {
                Console.WriteLine("Please Enter Personal Access Token: ");
                personalAccessToken = Console.ReadLine();
            }
            else
            {
                personalAccessToken = args[0];
            }

            if (string.IsNullOrWhiteSpace(personalAccessToken))
                throw new Exception("Missing Personal Access Token!");

            var op = "";
            do
            {
                Console.WriteLine("Select Operation: ");
                Console.WriteLine("1: Get Current Iteration PBIs");
                Console.WriteLine("2: Get Child PBIs From Parent Id");
                op = Console.ReadLine();
            }
            while (!"1,2".Split(',').Contains(op));

            var parentId = "";
            if (op == "2")
            {
                Console.WriteLine("Enter Parent WorkItem Id: ");
                parentId = Console.ReadLine();
            }

            var helper = new AzureRestApiHelper(personalAccessToken, "devops-sdx", "HRNext");
            Task.Run(async () =>
            {
                try
                {
                    string[] ids = new string[0];
                    switch (op)
                    {
                        case "1":
                            var getCurrentIterationPBIsResult = await helper.GetCurrentIterationPBIs();
                            ids = getCurrentIterationPBIsResult.Select(x => x.id).ToArray();
                            break;

                        case "2":
                            var resultGetChildPBIsFromParentId = await helper.GetChildWorkItemsFromParentWorkItemId(parentId);
                            ids = resultGetChildPBIsFromParentId.Where(x => x.target.id != parentId).Select(x => x.target.id).ToArray();
                            break;
                    }

                    var workitems = await helper.GetWorkItemsDetail(ids);
                    HandlebarsHelper.GeneratePBIs(workitems);
                    Console.WriteLine("Success!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.ReadLine();
                }
            });
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }
    }
}
