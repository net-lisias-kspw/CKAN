/*
        This file is part of KSP CKAN and its ©2021 LisiasT

        THIE FILE is distributed in the hope that it will be useful,
        but WITHOUT ANY WARRANTY; without even the implied warranty of
        MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
*/
using System;
using System.Windows.Forms;
using CKAN;

namespace CKANPluginThisDoesNotExists
{
    public class CKANPluginThisDoesNotExists : IGUIPlugin
    {
        private static readonly CKAN.Versioning.ModuleVersion VERSION = new CKAN.Versioning.ModuleVersion("0");
        public override void Deinitialize ()
        {
            throw new NotImplementedException();
        }

        public override string GetName ()
        {
            return "I don't exist. Move on.";
        }

        public override CKAN.Versioning.ModuleVersion GetVersion ()
        {
            return VERSION;
        }

        public override void Initialize ()
        {
            MessageBox.Show("Hi, Mom!");
            try {
                MenuStrip menuStrip = new MenuStrip();
                menuStrip.Dock = DockStyle.Top;
                Main.Instance.Controls.Add(menuStrip);
                MenuStrip menu = new MenuStrip();
                menuStrip.Items.Add("This thingy does not exists!", null, Installed);
            } catch (Exception e) {
                MessageBox.Show(e.Message);
            }
        }

        private void Installed (object sender, EventArgs e)
        {
            DialogResult res = MessageBox.Show("This Plugin does not exists and should not be pursued.", "Confirmation", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
            if (res != DialogResult.OK) {
                MessageBox.Show("My man, you can't cancel something that does not exists!");
            }
        }
    }
}
