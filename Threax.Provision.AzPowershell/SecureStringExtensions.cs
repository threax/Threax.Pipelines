using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace Threax.Provision.AzPowershell
{
    public static class SecureStringExtensions
    {
        public static SecureString ToSecureString(this String str)
        {
            var secure = new SecureString();
            foreach(var c in str)
            {
                secure.AppendChar(c);
            }
            secure.MakeReadOnly();
            return secure;
        }

        public static String ToInsecureString(this SecureString value)
        {
            if(value == null)
            {
                throw new NullReferenceException("SecureString provided to make insecure was null.");
            }

            IntPtr bstr = Marshal.SecureStringToBSTR(value);

            try
            {
                return Marshal.PtrToStringBSTR(bstr);
            }
            finally
            {
                Marshal.FreeBSTR(bstr);
            }
        }
    }
}
