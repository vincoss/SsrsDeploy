using System;
using System.Runtime.InteropServices;
using System.Security;

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
    }
}
