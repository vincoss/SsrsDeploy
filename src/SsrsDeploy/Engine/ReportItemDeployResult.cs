using System;
using System.Collections.Generic;
using System.Diagnostics;


namespace SsrsDeploy.Engine
{
    [DebuggerDisplay("Successful: {Successful}")]
    public class ReportItemDeployResult
    {
        public ReportItemDeployResult(IEnumerable<ReportItem> items, bool successful, Exception exception)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            Items = items;
            Successful = successful;
            if (exception != null)
            {
                Successful = false;
            }
            Exception = exception;
        }

        public Exception Exception { get; private set; }

        public IEnumerable<ReportItem> Items { get; private set; }

        public bool Successful { get; private set; }

    }
}
