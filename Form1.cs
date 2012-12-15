using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;

namespace SVNDelete
{
    public partial class Form1 : Form
    {
        public void addListItem(string item)
        {
            if (lstList.InvokeRequired)
            {
                BeginInvoke(new MethodInvoker(delegate() { addListItem(item); }));
            }
            else
            {
                lstList.Items.Add(item);
            }
        }

        WindowsIdentity self = WindowsIdentity.GetCurrent();
        
        public Form1()
        {
            InitializeComponent();
        }

        public void parseDir(String path)
        {
            foreach (String f in Directory.GetDirectories(path))
            {
                if (f.EndsWith(".svn"))
                {
                    DirectoryInfo dinfo;
                    try
                    {
                        DirectorySecurity ds = new DirectorySecurity(f, AccessControlSections.Access);
                        FileSystemAccessRule fsRule = new FileSystemAccessRule(self.Name,
                            FileSystemRights.FullControl,
                            AccessControlType.Allow);
                        ds.SetAccessRule(fsRule);

                        dinfo = new DirectoryInfo(f);
                        dinfo.Attributes = FileAttributes.Normal;
                        dinfo.SetAccessControl(ds);
                    }
                    catch (Exception err)
                    {
                    }

                    dinfo = new DirectoryInfo(f);
                    DirectorySecurity sec = dinfo.GetAccessControl();
                    sec.SetOwner(self.Owner);

                    setFileSecurity(f);

                    addListItem(f);
                    Application.DoEvents();

                    try
                    {
                        Directory.Delete(f, true);
                    }
                    catch (Exception err)
                    {
                        MessageBox.Show(err.Message);
                    }
                }
                else
                {
                    //addListItem(f);
                    Application.DoEvents();
                    parseDir(f);
                }
            }
        }

        public void setFileSecurity(String path)
        {
            FileSecurity fs;
            FileSystemAccessRule fsRule = new FileSystemAccessRule(self.Name, FileSystemRights.FullControl, AccessControlType.Allow);
            foreach (String file in Directory.GetFiles(path, "*.*", SearchOption.AllDirectories))
            {
                try
                {
                    fs = new FileSecurity(file, AccessControlSections.Access);
                    fs.AddAccessRule(fsRule);
                    File.SetAccessControl(file, fs);
                    File.SetAttributes(file, FileAttributes.Normal);
                    fs.SetOwner(self.Owner);
                }
                catch (Exception err)
                {
                }

                FileInfo finfo = new FileInfo(file);
                finfo.Attributes = FileAttributes.Normal;
                FileSecurity sec = finfo.GetAccessControl();
                sec.SetAccessRule(fsRule);
                sec.SetOwner(self.Owner);

                Application.DoEvents();
                try
                {
                    File.Delete(file);
                    Application.DoEvents();
                }
                catch (Exception err)
                {
                    MessageBox.Show(err.Message);
                }
                Application.DoEvents();
            }            
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void cmdSelect_Click_1(object sender, EventArgs e)
        {
            try
            {
                FolderBrowserDialog dlg = new FolderBrowserDialog();
                dlg.RootFolder = Environment.SpecialFolder.MyComputer;
                dlg.Description = "Selecteer de directory";
                DialogResult res = dlg.ShowDialog();
                if (res == DialogResult.OK)
                {
                    String path = dlg.SelectedPath;
                    lstList.Items.Clear();
                    cmdSelect.Enabled = false;
                    Application.DoEvents();
                    parseDir(path);
                }
                cmdSelect.Enabled = true;
                MessageBox.Show("Ok done!");
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }
    }
}
