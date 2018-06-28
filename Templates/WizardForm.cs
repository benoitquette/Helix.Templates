using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sitecore.Helix.Templates
{
    class WizardForm: Form
    {
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // WizardForm
            // 
            this.ClientSize = new System.Drawing.Size(588, 314);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "WizardForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.ResumeLayout(false);

        }
    }
}
