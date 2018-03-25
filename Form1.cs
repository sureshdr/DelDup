using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Management;
using System.Collections;
using System.Data.SQLite;
using System.IO;
using sureshdr.libddupe;

namespace DelDup
{

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ArrayList hdArray = new ArrayList();
            txtDevice.Text = "Processing...";
            hdArray = GetDeviceID();
            if (hdArray.Count > 0)
            {
                if (hdArray.Count == 1)
                    txtDevice.Text = "[" + hdArray.Count + "] " + ((HardDrive)hdArray[0]).Model.Trim() + " [" + ((HardDrive)hdArray[0]).SerialNo.Trim()  + "]";
                else
                    txtDevice.Text = "[" + hdArray.Count + "] ";
            }
            else txtDevice.Text = "Error! No HDD found!";
            tsDateTime.Text = DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString(); 
        }

        private void SaveDevice(ArrayList hdArr)
        {


        }

        
        private void DirEnum(string sFullPath, List<FileInfo> fileInfoList)
        {
            try
            {
                DirectoryInfo di = new DirectoryInfo(sFullPath);
                FileInfo[] files = di.GetFiles();

                foreach (FileInfo file in files)
                    fileInfoList.Add(file);

                //Scan recursively
                DirectoryInfo[] dirs = di.GetDirectories();
                if (dirs == null || dirs.Length < 1)
                    return;
                MessageBox.Show(@"Contains #" + dirs.Length + "number of entries!");
                foreach (DirectoryInfo dir in dirs)
                {
                    if (dir.FullName.StartsWith(@"C:\Windows") ||
                        dir.FullName.StartsWith(@"C:\Program"))
                    {
                        MessageBox.Show("Contains " + dir.ToString());
                    }
                    DirEnum(dir.FullName, fileInfoList);
                }
            }
            catch (Exception ex)
            {
               // Logger.Write("Exception in Helper.EnumerateFiles", ex);
            }
        }

        private ArrayList GetDeviceID()
        {
            int counter = 0;
            ArrayList hdCollection = new ArrayList();

            using (ManagementObjectSearcher searcher = new
            ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive"))
            {

                foreach (ManagementObject wmi_HD in searcher.Get())
                {
                    HardDrive hd = new HardDrive();
                    hd.Model = wmi_HD["Model"].ToString();
                    hd.Type = wmi_HD["InterfaceType"].ToString();
                    hd.SerialNo = wmi_HD["SerialNumber"].ToString().Trim();
                    hd.Guid = Guid.NewGuid().ToString();
                    hdCollection.Add(hd);
                    counter++;
                }
            }
            return hdCollection;
        }

        private void btnCheck_Click(object sender, EventArgs e)
        {
            using (DataMgmt dm = new DataMgmt(Properties.Settings.Default.DBFile))
            {
                foreach (HardDrive hd in GetDeviceID())
                {
                    if (!dm.IsExistDrive(hd.SerialNo))
                    {
                        DialogResult rs = MessageBox.Show("Do you want to save " + hd.SerialNo + " ?", "Drive Save", MessageBoxButtons.YesNo);
                        switch (rs)
                        {
                            case DialogResult.Yes:
                                dm.AddDrive(hd);
                                break;
                            case DialogResult.No:

                                break;
                        }
                    }
                    else
                        MessageBox.Show(hd.SerialNo + " already exists in the Master DB");
                }
                SaveDevice(GetDeviceID());
            }
        }

        private void statusStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void btnScan_Click(object sender, EventArgs e)
        {
            List<FileInfo> FileList = new List<FileInfo>();
            DirEnum(@"C:\",  FileList);
            MessageBox.Show("Final Count: " + FileList.Count);           
        }
    }

}
