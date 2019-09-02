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

            var helper = new AzureRestApiHelper(personalAccessToken, "devops-sdx", "HRNext");
            Task.Run(async () =>
            {
                try
                {
                    var result = await helper.GetCurrentIterationPBIs();
                    var ids = result.Select(x => x.id).ToArray();
                    var workitems = await helper.GetWorkItemsDetail(ids);

                    HandlebarsHelper.GeneratePBIs(workitems);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.ReadLine();
                }
            });
            Console.WriteLine("Success!");
            Console.ReadLine();
        }
    }
}
