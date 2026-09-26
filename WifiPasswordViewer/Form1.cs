using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WifiPasswordViewer
{
    public partial class Form1 : Form
    {
        private int count;
        private int count_names;

        public Form1()
        {
            InitializeComponent();
        }

        //Get all WiFi list 
        private string getWifiList()
        {
            Process processWiFi = new Process();
            processWiFi.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            processWiFi.StartInfo.FileName = "netsh";
            processWiFi.StartInfo.Arguments = "wlan show profile";
            processWiFi.StartInfo.UseShellExecute = false;
            processWiFi.StartInfo.RedirectStandardError = true;
            processWiFi.StartInfo.RedirectStandardInput = true;
            processWiFi.StartInfo.RedirectStandardOutput = true;
            processWiFi.StartInfo.CreateNoWindow = true;
            processWiFi.Start();

            string data = processWiFi.StandardOutput.ReadToEnd();
            string error = processWiFi.StandardError.ReadToEnd();
            processWiFi.WaitForExit();
            return data;
        }
        
        //read WiFi Password
        private string readPassword(string WiFiName) 
        {
            string argument = "wlan show profile name=\"" + WiFiName + "\" key=clear";
            Process processWifi = new Process();
            processWifi.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            processWifi.StartInfo.FileName = "netsh";
            processWifi.StartInfo.Arguments = argument;

            processWifi.StartInfo.UseShellExecute = false;
            processWifi.StartInfo.RedirectStandardError = true;
            processWifi.StartInfo.RedirectStandardInput = true;
            processWifi.StartInfo.RedirectStandardOutput = true;
            processWifi.StartInfo.CreateNoWindow = true;
            processWifi.Start();

            string data = processWifi.StandardOutput.ReadToEnd();
            string error = processWifi.StandardError.ReadToEnd();

            processWifi.WaitForExit();

            return data;
        }

        //Get Password
        private string getPassword(string WiFiName)
        {
            string get_pass = readPassword(WiFiName);
            using (StringReader reader = new StringReader(get_pass))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    Regex regex = new Regex(@"Key Content * : (?<after>.*)");
                    Match match = regex.Match(line);

                    if (match.Success)
                    {
                        string current_password = match.Groups["after"].Value;
                        return current_password;
                    }
                }
            }
            return "Open WiFi";
        }

        //Get WiFi Password
        private void getWiFiPassword()
        {
            string WifiPass = getWifiList();
            using (StringReader reader = new StringReader(WifiPass))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    count++;
                    Regex regex = new Regex(@"All User Profile * : (?<after>.*)");
                    Match match = regex.Match((line));

                    if (match.Success)
                    {
                        count_names++;
                        string WiFi_Name = match.Groups["after"].Value;
                        string Password = getPassword(WiFi_Name);
                        listView1.Items.Add(WiFi_Name).SubItems.Add(Password);

                    }
                }
            }
        }

        private void btnViewPass_Click(object sender, EventArgs e)
        {
            listView1.Items.Clear();
            btnViewPass.Enabled = false;
            getWiFiPassword();
            btnViewPass.Enabled = true;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // Specify that the link was visited.
            this.linkLabel1.LinkVisited = true;

            // Navigate to a URL.
            System.Diagnostics.Process.Start("https://github.com/ukmihiran");
        }

        //Copy Password from ListView
        private void listView1_KeyUp(object sender, KeyEventArgs e)
        {
            if (sender != listView1) { return; }

            if (e.Control && e.KeyCode == Keys.C)
                CopySelectedValues();
        }
        private void CopySelectedValues()
        {
            var builder = new StringBuilder();
            foreach (ListViewItem item in listView1.SelectedItems)
                builder.AppendLine(item.SubItems[1].Text);

            Clipboard.SetText(builder.ToString());
        }

        //Export Data
        private async void btnExport_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog() { Filter = "Text Documents |*.txt", ValidateNames = true })
            {
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    using (TextWriter writer = new StreamWriter(new FileStream(saveFileDialog.FileName, FileMode.Create), Encoding.UTF8))
                    {
                        foreach(ListViewItem item in listView1.Items)
                        {
                            await writer.WriteLineAsync(item.SubItems[0].Text + "\t" + item.SubItems[1].Text);
                        }
                        MessageBox.Show("Your data has been successfully exported.","Message",MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
            }
        }
    }
}
