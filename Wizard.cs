using EnvDTE;
using Microsoft.VisualStudio.TemplateWizard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Helix.Templates
{
    class Wizard : IWizard
    {
        private DTE _dte;

        public void BeforeOpeningFile(ProjectItem projectItem) { }
        public void ProjectFinishedGenerating(Project project) { }
        public void ProjectItemFinishedGenerating(ProjectItem projectItem) { }
        public void RunFinished() { }

        public void RunStarted(
            object automationObject, 
            Dictionary<string, string> replacementsDictionary, 
            WizardRunKind runKind, 
            object[] customParams)
        {
            _dte = (DTE)automationObject;

            replacementsDictionary.Add("$modulefullname$", String.Format("Sitecore.{0}.{1}", 
                replacementsDictionary["$layer$"], replacementsDictionary["$safeprojectname$"]));
            replacementsDictionary.Add("$modulename$", replacementsDictionary["$safeprojectname$"]);

            StringBuilder dictionary = new StringBuilder();
            foreach (string key in replacementsDictionary.Keys)
            {
                dictionary.AppendFormat("{0}: {1}{2}", key, replacementsDictionary[key], System.Environment.NewLine);
            }
            MessageBox.Show(dictionary.ToString());
            //MessageBox.Show("Module name: " + replacementsDictionary["$modulename$"]);
            //MessageBox.Show("Module name: " + replacementsDictionary["$modulefullname$"]);
        }

        public bool ShouldAddProjectItem(string filePath) { return true; }
    }
}