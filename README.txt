Tool to bulk deploy SSRS reports


### Example:

var variables = new Dictionary<string, string>();

var builder = new DeployBuilder()
                .WithReportServer("http://localhost:80/ReportServer/")
                .WithParentPath("/")
                .WithItemsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
                .WithVariables(variables)
                .WithCredentials(null, null, null)
                .WithLogToConsole();

var result = builder.PerformDeploy();

if(result.Successful)
{
    Console.WriteLine("Successful...");
}
else
{
    Console.WriteLine("):");
}

            	