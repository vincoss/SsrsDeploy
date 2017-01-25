using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Security;
using System.Text;
using SsrsDeploy.Logging;

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

        public ReportItemDeployResult PerformDeploy()
        {
            _logger = InbuiltLog.For(typeof(DeployBuilder));

            var executed = new List<ReportItem>();

            try
            {
                _logger.Debug("Begin items deploy.");

                var itemsToDeploy = new EmbeddedReportItemProvider(new[] { _assembly }, _filter, Encoding.Default).GetItems();

                if (itemsToDeploy.Any() == false)
                {
                    return new ReportItemDeployResult(executed, true, null);
                }

                foreach (var item in itemsToDeploy)
                {
                    _logger.Debug("Processing item: {0}", item.FullName);

                    if (IsUpgradeRequired(item) == false)
                    {
                        _logger.Debug("Item does not require update");

                        continue;
                    }

                    _logger.Debug("Begin item deploy");

                    Deploy(item); // TODO: where is return value

                    _logger.Debug("End item deploy");

                    executed.Add(item);
                }

                _logger.Debug("Items processed: {0}, successful : {1}", executed.Count, true);

                return new ReportItemDeployResult(executed, true, null);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "PerformDeploy");

                return new ReportItemDeployResult(executed, false, ex);
            }
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
        private void Deploy(ReportItem item)
        {
        }

        // TODO: refactor
        private bool IsUpgradeRequired(ReportItem item)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
