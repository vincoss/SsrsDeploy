using NDesk.Options;
using SsrsDeploy.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SsrsDeploy.ConsoleSample
{
    /// <summary>
    /// "-u=http:/localhost:80/ReportServer/" "-p=/"
    /// </summary>
    class Program
    {
        static int Main(string[] args)
        {
            string reportServerUrl = null;
            string parentPath = null;
            string userName = null;
            string password = null;
            string domainName = null;
            var reports = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            bool showHelp = args.Length == 0;

            var options = new OptionSet();
            options.Add("u|reportServerUrl=", "Report server URL is required.", a => reportServerUrl = a);
            options.Add("p|parentPath=", "Report parent path is required.", a => parentPath = a);

            options.Add("n|userName=", "User name is required.", a => userName = a);
            options.Add("s|password=", "Password is required.", a => password = a);
            options.Add("d|domainName=", "Domain name is required.", a => domainName = a);

            options.Add("?|help", v => showHelp = v == "?");

            try
            {
                options.Parse(args);
            }
            catch (OptionException e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("Try '?' for more information.");
                return -1;
            }

            if (showHelp)
            {
                ShowHelp(options);
                return -1;
            }

            var variables = new Dictionary<string, string>();

            var builder = new DeployBuilder()
                            .WithReportServer(reportServerUrl)
                            .WithParentPath(parentPath)
                            .WithItemsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
                            .WithVariables(variables)
                            .WithCredentials(userName, password, domainName)
                            .WithLogToConsole();

            var result = builder.PerformDeploy();

            if (result.Successful == false)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(result.Exception);
                Console.ResetColor();
                return -1;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Success!");
            Console.ResetColor();
            return 0;
        }

        private static void ShowHelp(OptionSet p)
        {
            p.WriteOptionDescriptions(Console.Out);
        }

        private static void AddVariable(string value, IDictionary<string, string> variables)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }
            if (value.IndexOf('=') == -1)
            {
                Console.WriteLine("Please enter correct form of variable name and variable value. -v=name=value");
            }
            string[] array = value.Split(new char[] { '=' });
            variables[array[0]] = ((array.Length > 1) ? array[1] : "");
        }
    }
}