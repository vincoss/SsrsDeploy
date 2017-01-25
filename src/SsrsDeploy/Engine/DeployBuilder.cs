using System;


namespace SsrsDeploy.Engine
{
    public class DeployBuilder
    {
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
    }
}
