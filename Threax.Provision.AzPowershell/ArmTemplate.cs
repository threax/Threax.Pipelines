using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Threax.Provision.AzPowershell
{
    public abstract class ArmTemplate
    {
        public virtual String GetParametersPath()
        {
            var type = this.GetType();
            string templateFolder = GetTemplateFolder(type);

            var path = Path.Combine(Path.GetDirectoryName(type.Assembly.Location), "ArmTemplates", templateFolder, "parameters.json");
            return path;
        }

        public virtual String GetTemplatePath()
        {
            var type = this.GetType();
            string templateFolder = GetTemplateFolder(type);

            var path = Path.Combine(Path.GetDirectoryName(type.Assembly.Location), "ArmTemplates", templateFolder, "template.json");
            return path;
        }

        protected static string GetTemplateFolder(Type type)
        {
            var templateFolder = type.Namespace;
            var lastDot = templateFolder.LastIndexOf('.');
            var startIndex = lastDot + 1;
            if (lastDot != -1 && startIndex < templateFolder.Length)
            {
                templateFolder = templateFolder.Substring(startIndex);
            }

            return templateFolder;
        }
    }
}
