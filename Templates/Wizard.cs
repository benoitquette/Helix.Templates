using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.TemplateWizard;
using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sitecore.Helix.Templates
{
    class Wizard : IWizard
    {
        private static DTE2 dte;
        private static Dictionary<string, string> parameters;

        public void BeforeOpeningFile(ProjectItem projectItem)
        {
        }

        public void ProjectFinishedGenerating(Project project)
        {
        }

        public void ProjectItemFinishedGenerating(ProjectItem projectItem)
        {
        }

        public void RunFinished()
        {
            Solution2 solution = (Solution2)dte.Solution;
            Project project = GetProject(
                solution.Projects.OfType<Project>(), 
                parameters["$projectname$"]);

            // remove the created project from the solution
            solution.Remove(project);

            string destinationDir = parameters["$destinationdirectory$"];

            // check that the project was created in the right path. If not, we move it
            string properPath = Path.Combine(parameters["$solutiondirectory$"], "src", parameters["$layer$"], parameters["$safeprojectname$"]);
            if (String.Compare(destinationDir, properPath) != 0)
            {
                Directory.Move(destinationDir, properPath);
                destinationDir = properPath;

                // notify the user that the destination dir was changed
                string message = String.Format("To comply with the Helix guidlines, the module was installed in the folder '{0}'.", destinationDir);
                MessageBox.Show(message, "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            // rename the project folder to 'code'
            string currentPath = Path.Combine(destinationDir, parameters["$modulefullname$"]);
            string newPath = Path.Combine(destinationDir, parameters["$modulewebsitefolder$"]);
            Directory.Move(currentPath, newPath);

            // add the new project to the solution and save it
            SolutionFolder layerFolder = GetLayerFolder(parameters["$layer$"], solution);
            if (layerFolder == null)
            {
                throw new Exception(String.Format("Could not found the folder '´{0}' in the solution.", parameters["$layer$"]));
            }
            else
            {
                Project folderProject = layerFolder.AddSolutionFolder(parameters["$projectname$"]);
                SolutionFolder solutionFolder = (SolutionFolder)folderProject.Object;
                string projectFilePath = Path.Combine(newPath, String.Concat(parameters["$modulefullname$"], ".csproj"));
                solutionFolder.AddFromFile(projectFilePath);

                // delete serialization files and folders from disk
                string serializationFolder = Path.Combine(destinationDir, "serialization");
                Directory.Delete(Path.Combine(serializationFolder, "bin"), true);
                Directory.Delete(Path.Combine(serializationFolder, "obj"), true);
                File.Delete(Path.Combine(serializationFolder, "serialization.csproj"));
                File.Delete(Path.Combine(serializationFolder, "serialization.csproj.user"));
            }
        }

        private static SolutionFolder GetLayerFolder(string layerName, Solution2 solution)
        {
            foreach (Project project in solution.Projects.OfType<Project>())
            {
                if (project.Name == layerName && project.Kind == ProjectKinds.vsProjectKindSolutionFolder)
                {
                    return (SolutionFolder)project.Object;
                }
            }
            return null;
        }

        public void RunStarted(
            object automationObject,
            Dictionary<string, string> replacementsDictionary,
            WizardRunKind runKind,
            object[] customParams)
        {
            dte = (DTE2)automationObject;

            // create module names and layer params
            string moduleName = replacementsDictionary["$safeprojectname$"];
            string moduleFullName = String.Format("Sitecore.{0}.{1}", replacementsDictionary["$layer$"], moduleName);
            replacementsDictionary.Add("$modulefullname$", moduleFullName);
            replacementsDictionary.Add("$modulename$", moduleName);
            replacementsDictionary.Add("$timestamp$", DateTime.Now.ToString("yyyyMMddThhmmssZ"));
            replacementsDictionary.Add("$userdisplayname$", UserPrincipal.Current.DisplayName);

            parameters = new Dictionary<string, string>(replacementsDictionary);
            //ShowDictionary(replacementsDictionary);

            //try
            //{
            //    WizardForm wizardForm = new WizardForm();
            //    wizardForm.ShowDialog();
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.ToString());
            //}
        }

        private static void ShowDictionary(Dictionary<string, string> dico)
        {
            StringBuilder dictionary = new StringBuilder();
            foreach (string key in dico.Keys)
            {
                dictionary.AppendFormat("{0}: {1}{2}", key, dico[key], Environment.NewLine);
            }
            MessageBox.Show(dictionary.ToString());
        }

        public bool ShouldAddProjectItem(string filePath)
        {
            return true;
        }

        public static Project GetProject(IEnumerable<Project> projects, string name)
        {
            foreach (Project project in projects)
            {
                var projectName = project.Name;
                if (projectName == name)
                {
                    return project;
                }
                else if (project.Kind == ProjectKinds.vsProjectKindSolutionFolder)
                {
                    var subProjects = project
                        .ProjectItems
                        .OfType<ProjectItem>()
                        .Where(item => item.SubProject != null)
                        .Select(item => item.SubProject);

                    var projectInFolder = GetProject(subProjects, name);

                    if (projectInFolder != null)
                    {
                        return projectInFolder;
                    }
                }
            }

            return null;
        }
    }
}