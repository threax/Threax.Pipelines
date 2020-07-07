using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Text;

namespace Threax.Provision.AzPowershell
{
    public class InvalidPowershellOperation : Exception
    {
        public InvalidPowershellOperation(String message, IEnumerable<ErrorRecord> errors)
            :base(message)
        {
            Errors = errors;
        }

        public IEnumerable<ErrorRecord> Errors { get; }
    }
}
