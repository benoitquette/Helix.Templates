using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.TemplateWizard;
using System;
using System.Collections.Generic;
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

        public void ProjectFinishedGenerating(EnvDTE.Project project)
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

            // rename the project folder to code
            string currentPath = Path.Combine(
                parameters["$destinationdirectory$"],
                parameters["$modulefullname$"]);
            string newPath = Path.Combine(
                parameters["$destinationdirectory$"],
                parameters["$modulewebsitefolder$"]);
            Directory.Move(currentPath, newPath);

            // add the new project to the solution and save it
            Project folderProject = solution.AddSolutionFolder(parameters["$projectname$"]);
            SolutionFolder solutionFolder = (SolutionFolder)folderProject.Object;
            string projectFilePath = Path.Combine(newPath, String.Concat(parameters["$modulefullname$"], ".csproj"));
            solutionFolder.AddFromFile(projectFilePath);

            // delete serialization files and folders from disk
            string serializationFolder = Path.Combine(parameters["$destinationdirectory$"], "serialization");
            Directory.Delete(Path.Combine(serializationFolder, "bin"), true);
            Directory.Delete(Path.Combine(serializationFolder, "obj"), true);
            File.Delete(Path.Combine(serializationFolder, "serialization.csproj"));
            File.Delete(Path.Combine(serializationFolder, "serialization.csproj.user"));
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

            parameters = new Dictionary<string, string>(replacementsDictionary);
            //ShowDictionary(replacementsDictionary);
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