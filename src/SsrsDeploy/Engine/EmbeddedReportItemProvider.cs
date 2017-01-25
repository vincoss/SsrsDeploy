using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;


namespace SsrsDeploy.Engine
{
    public class EmbeddedReportItemProvider : IReportItemProvider
    {
        private readonly Assembly[] _assemblies;
        private readonly Encoding _encoding;
        private readonly Func<string, bool> _filter;

        public EmbeddedReportItemProvider(Assembly[] assemblies, Func<string, bool> filter, Encoding encoding)
        {
            if (assemblies == null)
            {
                throw new ArgumentNullException(nameof(assemblies));
            }
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }
            if (encoding == null)
            {
                throw new ArgumentNullException(nameof(encoding));
            }
            _assemblies = assemblies;
            _filter = filter;
            _encoding = encoding;
        }

        /// <summary>
        /// Gets all items that should be executed.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ReportItem> GetItems()
        {
            var items = _assemblies
                .Select(assembly => new
                {
                    Assembly = assembly,
                    ResourceNames = assembly.GetManifestResourceNames().Where(_filter).ToArray()
                })
                .SelectMany(x => x.ResourceNames.Select(resourceName => ReportItem.FromStream(resourceName, x.Assembly.GetManifestResourceStream(resourceName), _encoding)))
                .OrderBy(o => o.FullName)
                .ToList();

            return items;
        }
    }
}
