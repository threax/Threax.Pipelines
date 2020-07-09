using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
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
    }
}
