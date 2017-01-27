using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Xml.Linq;

namespace SsrsDeploy
{
    public static class Extensions
    {
        public static UInt64 GetKnuthHash(string content)
        {
            UInt64 hashedValue = 3074457345618258791ul;

            if (content == null)
            {
                return hashedValue;
            }

            for (int i = 0; i < content.Length; i++)
            {
                hashedValue += content[i];
                hashedValue *= 3074457345618258799ul;
            }
            return hashedValue;
        }

        public static SecureString StringToSecureString(string value)
        {
            var ss = new SecureString();

            if (string.IsNullOrEmpty(value))
            {
                return ss;
            }

            foreach (var ch in value)
            {
                ss.AppendChar(ch);
            }
            return ss;
        }

        public static string SecureStringToString(SecureString value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            IntPtr valuePtr = IntPtr.Zero;
            try
            {
                valuePtr = Marshal.SecureStringToGlobalAllocUnicode(value);
                return Marshal.PtrToStringUni(valuePtr);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(valuePtr);
            }
        }

        #region XDocument

        public static string GetXAttributeValue(this XAttribute attribute, string defaultValue = null)
        {
            if (attribute == null)
            {
                return null;
            }

            return attribute.Value ?? defaultValue;
        }

        public static string GetXElementValue(this XElement element, string defaultValue = null)
        {
            if (element == null)
            {
                return null;
            }

            return element.Value ?? defaultValue;
        }

        #endregion
    }
}
