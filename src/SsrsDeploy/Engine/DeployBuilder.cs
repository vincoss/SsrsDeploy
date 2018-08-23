using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Security;
using System.Text;
using SsrsDeploy.Logging;
using System.IO;
using System.Xml.Linq;
using System.Dynamic;

namespace SsrsDeploy.Engine
{
    public class DeployBuilder
    {
        private string _reportServerUrl;
        private string _parentPath;
        private Assembly _assembly;
        private Func<string, bool> _filter;
        private IDictionary<string, string> _variables;
        private string _userName;
        private SecureString _password = new SecureString();
        private string _domainName;
        private InbuiltLogger _logger;

        public DeployBuilder()
        {
            InbuiltLog.SetFactory(new InbuiltConsoleLoggerFactory());
            _logger = InbuiltLog.For(typeof(DeployBuilder));
        }

        #region Public methods

        public DeployBuilder WithReportServer(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException(nameof(url));
            }
            _reportServerUrl = url;
            return this;
        }

        public DeployBuilder WithParentPath(string parentPath)
        {
            if (parentPath == null)
            {
                throw new ArgumentNullException(nameof(parentPath));
            }
            _parentPath = parentPath;
            return this;
        }

        public DeployBuilder WithItemsEmbeddedInAssembly(Assembly assembly)
        {
            return WithItemsEmbeddedInAssembly(assembly, DefaultFilter());
        }

        public DeployBuilder WithItemsEmbeddedInAssembly(Assembly assembly, Func<string, bool> filter)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }
            if (filter == null)
            {
                filter = DefaultFilter();
            }
            _assembly = assembly;
            _filter = filter;
            return this;
        }

        public DeployBuilder WithVariables(IDictionary<string, string> variables) // TODO:
        {
            if (variables == null)
            {
                throw new ArgumentNullException(nameof(variables));
            }
            _variables = variables;
            return this;
        }

        public DeployBuilder WithLogToConsole()
        {
            InbuiltLog.SetFactory(new InbuiltConsoleLoggerFactory());
            return this;
        }

        public DeployBuilder WithCredentials(string userName, string password, string domainName)
        {
            _userName = userName;
            _password = Extensions.StringToSecureString(password);
            _domainName = domainName;
            return this;
        }

        /// <summary>
        /// Filter for .rdl, .rsd, .rds files.
        /// </summary>
        public static Func<string, bool> DefaultFilter()
        {
            Func<string, bool> filter = (s) =>
            {
                if (s.EndsWith(".rdl", StringComparison.InvariantCultureIgnoreCase) ||
                    s.EndsWith(".rsd", StringComparison.InvariantCultureIgnoreCase) ||
                    s.EndsWith(".rds", StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
                return false;
            };
            return filter;
        }

        public ReportItemDeployResult PerformDeploy()
        {
            var executed = new List<ReportItem>();

            try
            {
                _logger.Debug("Begin items deploy.");

                var itemsToDeploy = new EmbeddedReportItemProvider(new[] { _assembly }, _filter, Encoding.Default)
                                    .GetItems()
                                    .OrderBy(x => x.SortOrder).ToArray();

                if (itemsToDeploy.Any() == false)
                {
                    return new ReportItemDeployResult(executed, true, null);
                }

                foreach (var item in itemsToDeploy)
                {
                    _logger.Debug("Processing item: {0}", item.FullName);

                    if (IsUpgradeRequired(item) == false)
                    {
                        executed.Add(item); // Add as executed

                        _logger.Debug("Item does not require update");

                        continue;
                    }

                    _logger.Debug("Begin item deploy");

                    var itemDeployResult = DeployItem(item);

                    _logger.Debug("End item deploy with status: {0}", itemDeployResult);

                    if (itemDeployResult == 0)
                    {
                        executed.Add(item);
                    }
                }

                var successful = executed.Count > 0;

                _logger.Debug("Items processed: {0}, successful : {1}", executed.Count, successful);

                return new ReportItemDeployResult(executed, successful, null);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "PerformDeploy");

                return new ReportItemDeployResult(executed, false, ex);
            }
        }

        #endregion

        #region Private methods

        private string GetItemPath(string fileName, string parentPath)
        {
            var path = string.Format(@"{0}/{1}", parentPath.Trim(new[] { '/' }), fileName.Trim());
            if (path[0] != '/')
            {
                path = string.Format("/{0}", path);
            }
            return path;
        }

        // TODO: refactor
        private int DeployItem(ReportItem item)
        {
            if(item.FullName.EndsWith(".rds"))
            {
                var config = GetConfigFromRptDataSource(item.Content);
                PublishDataSource(_reportServerUrl, _parentPath, config, _userName, Extensions.SecureStringToString(_password), _domainName);
                return 0;
            }

           return  CreateCatalogItemReportService2010(_reportServerUrl, _parentPath, item, _userName, Extensions.SecureStringToString(_password), _domainName);
        }

        // TODO: refactor
        private bool IsUpgradeRequired(ReportItem item)
        {
            var itemPath = GetItemPath(item.Name, _parentPath);
            var serverContent = GetExistingItemReportService2010(itemPath, _reportServerUrl, _userName, Extensions.SecureStringToString(_password), _domainName);

            var newItemHash = Extensions.GetKnuthHash(item.Content);
            var existingItemHash = Extensions.GetKnuthHash(serverContent);

            return (newItemHash != existingItemHash);
        }

        #endregion

        #region ReportService2010 methods TODO: Refactor into class

        protected virtual string GetExistingItemReportService2010(string itemPath, string reportServiceUrl, string userName, string password, string domain)
        {
            if (string.IsNullOrEmpty(itemPath))
            {
                throw new ArgumentNullException(nameof(itemPath));
            }
            if (string.IsNullOrEmpty(reportServiceUrl))
            {
                throw new ArgumentNullException(nameof(reportServiceUrl));
            }

            var reportingService = new ReportingService2010();

            reportingService.Url = GetFullServiceUrl(reportServiceUrl);
            reportingService.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;

            if (string.IsNullOrEmpty(userName) == false)
            {
                reportingService.Credentials = new System.Net.NetworkCredential(userName, password, domain);
            }

            var type = reportingService.GetItemType(itemPath);
            if (string.IsNullOrEmpty(type) || type.Equals("Unknown", StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            var result = reportingService.GetItemDefinition(itemPath);
            return Encoding.Default.GetString(result);
        }

        protected virtual int CreateCatalogItemReportService2010(string reportServiceUrl, string parentPath, ReportItem item, string userName, string password, string domain)
        {
            var reportingService = new ReportingService2010();

            reportingService.Url = GetFullServiceUrl(reportServiceUrl);
            reportingService.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;

            if (string.IsNullOrEmpty(userName) == false)
            {
                reportingService.Credentials = new System.Net.NetworkCredential(userName, password, domain);
            }
            
            Warning[] warnings = null;

            using (var ms = new MemoryStream())
            using (var writer = new StreamWriter(ms, Encoding.Default))
            {
                writer.Write(item.Content);
                writer.Flush();

                var itemType = GetCatalogItemType(item.FullName);

                CatalogItem catalogItem = reportingService.CreateCatalogItem(
                    itemType,
                    item.Name,
                    parentPath,
                    true,
                    ms.ToArray(),
                    null,
                    out warnings);
            }

            if (warnings == null || warnings.Length == 0)
            {
                return 0;
            }

            foreach (var w in warnings)
            {
                _logger.Debug(w.Message);
            }

            return 1;
        }

        private CatalogItem PublishDataSource(string reportServiceUrl, string parentPath, dynamic config, string userName, string password, string domain)
        {
            var reportingService = new ReportingService2010();

            reportingService.Url = GetFullServiceUrl(reportServiceUrl);
            reportingService.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;

            if (string.IsNullOrEmpty(userName) == false)
            {
                reportingService.Credentials = new System.Net.NetworkCredential(userName, password, domain);
            }

            var definition = new DataSourceDefinition
            {
                CredentialRetrieval = Get(config),
                ConnectString = (string)config.ConnectString,
                Enabled = true,
                EnabledSpecified = true,
                Extension = config.Extension,
                WindowsCredentials = false

                // TODO:
                //UserName = (string)config.DataSource.Username,
                //Password = (string)config.DataSource.Password
            };

            var item = reportingService.CreateDataSource(config.Name, parentPath, true, definition, null);

            return item;
        }

        public static dynamic GetConfigFromRptDataSource(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                throw new ArgumentNullException(nameof(content));
            }

            var root = XElement.Parse(content);

            if (string.Equals(root.Name.ToString(), "RptDataSource", StringComparison.Ordinal) == false)
            {
                throw new InvalidOperationException(string.Format("Invalid element. RptDataSource element is required"));
            }

            dynamic dynamo = new ExpandoObject();
            var pairs = (IDictionary<string, object>)dynamo;

            pairs.Add("Name", root.Attribute("Name").GetXAttributeValue());

            var connectionProperties = root.Element("ConnectionProperties");

            pairs.Add("Extension", connectionProperties.Element("Extension").GetXElementValue());
            pairs.Add("ConnectString", connectionProperties.Element("ConnectString").GetXElementValue());
            pairs.Add("IntegratedSecurity", connectionProperties.Element("IntegratedSecurity").GetXElementValue());
            pairs.Add("Prompt", connectionProperties.Element("Prompt").GetXElementValue());

            return dynamo;
        }

        private static CredentialRetrievalEnum Get(dynamic config)
        {
            if(config.Prompt != null)
            {
                return CredentialRetrievalEnum.Prompt;
            }
            if(IsIntegratedSecurity(config.IntegratedSecurity))
            {
                return CredentialRetrievalEnum.Integrated;
            }
            return CredentialRetrievalEnum.Store;
        }

        private static bool IsIntegratedSecurity(string value)
        {
            if(string.IsNullOrEmpty(value))
            {
                return false;
            }
            return value.Equals("true", StringComparison.OrdinalIgnoreCase);
        }

        private string GetCatalogItemType(string item)
        {
            if (string.IsNullOrEmpty(item))
            {
                throw new ArgumentNullException(nameof(item));
            }
            if (item.EndsWith(".rdl", StringComparison.InvariantCultureIgnoreCase))
            {
                return "Report";
            }

            if (item.EndsWith(".rsd", StringComparison.InvariantCultureIgnoreCase))
            {
                return "DataSet";
            }

            if (item.EndsWith(".rds", StringComparison.InvariantCultureIgnoreCase))
            {
                return "DataSource";
            }
            throw new NotSupportedException(string.Format("Unknown item type. {0}", item));
        }

        private string GetFullServiceUrl(string value)
        {
            return string.Format("{0}/ReportService2010.asmx", value.Trim(new[] { '/' }));
        }

        #endregion
        
    }
}