using EnvDTE;
using Microsoft.VisualStudio.TemplateWizard;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Helix.Templates
{
    class Wizard : IWizard
    {
        private static DTE _dte;
        private static Dictionary<string, string> parameters;

        public void BeforeOpeningFile(ProjectItem projectItem) { }

        public void ProjectFinishedGenerating(Project project)
        {
            
        }

        public void ProjectItemFinishedGenerating(ProjectItem projectItem) { }

        public void RunFinished()
        {
            
        }

        public void RunStarted(
            object automationObject,
            Dictionary<string, string> replacementsDictionary,
            WizardRunKind runKind,
            object[] customParams)
        {
            _dte = (DTE)automationObject;
            ShowDictionary(replacementsDictionary);

            //replacementsDictionary.Add("$modulefullname$", String.Format("Sitecore.{0}.{1}",
            //    replacementsDictionary["$layer$"], replacementsDictionary["$safeprojectname$"]));
            //replacementsDictionary.Add("$modulename$", replacementsDictionary["$safeprojectname$"]);

            parameters = new Dictionary<string, string>(replacementsDictionary);
        }

        private static void ShowDictionary(Dictionary<string, string> dico)
        {
            StringBuilder dictionary = new StringBuilder();
            foreach (string key in dico.Keys)
            {
                dictionary.AppendFormat("{0}: {1}{2}", key, dico[key], System.Environment.NewLine);
            }
            MessageBox.Show(dictionary.ToString());
        }

        public bool ShouldAddProjectItem(string filePath)
        {
            return true;
        }
    }
}