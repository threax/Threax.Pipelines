using System;
using System.Collections.Generic;
using System.Text;

namespace Threax.Provision.AzPowershell
{
    public class AzPowershellOptions
    {
        /// <summary>
        /// Set this to true to use the dummy key vault access manager instead of the real one. The dummy one won't make changes.
        /// </summary>
        public bool UseDummyKeyVaultAccessManager { get; set; }
    }
}
