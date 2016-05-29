using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using NaturalLanguageTranslator;
using System.Globalization;

namespace NLT_UI
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadAvailableLanguages();
            UpdateUI();
        }

        private void LoadAvailableLanguages()
        {
            foreach (CultureInfo culture in NLT.Default.AvailableCultures)
            {
                ToolStripMenuItem menuItem = new ToolStripMenuItem(culture.DisplayName, null, (s, e) =>
                {
                    foreach (ToolStripMenuItem item in languageToolStripMenuItem.DropDownItems)
                    {
                        item.Checked = item == s;
                    }
                    NLT.Default.CurrentCulture = culture;
                    UpdateUI();
                });
                menuItem.Checked = culture.Equals(NLT.Default.CurrentCulture);
                languageToolStripMenuItem.DropDownItems.Add(menuItem);
            }
        }

        private void UpdateUI()
        {
            Text = "This is an example".Translated();
            languageToolStripMenuItem.Text = "Language".Translated();
            nameLabel.Text = "Name:".Translated();
            passwordLabel.Text = "Password:".Translated();
            rememberCheckBox.Text = "Remember password".Translated();
            acceptButton.Text = "Accept".Translated();
            cancelButton.Text = "Cancel".Translated();
            toolStripLabel.Text = string.Format("The current date is {0}".Translated(), DateTime.Now);
        }
    }
}
