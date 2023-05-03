using IpHlpApidotnet;
using Microsoft.Win32;
using System;
using System.Windows.Forms;

namespace DbDMatchNotification
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        TCPUDPConnections ts;
        bool tool_tip = false;
        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                using (RegistryKey Key = Registry.CurrentUser.OpenSubKey
                            ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", false))
                {
                    object a = Key.GetValue("DbDMatchNotification");
                    if (a != null)
                    {
                        autostartcheckBox.Checked = true;
                        this.WindowState = FormWindowState.Minimized;
                        this.Hide();
                    }
                    else
                    {
                        this.Activate();
                        this.Show();
                        this.WindowState = FormWindowState.Normal;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error!");
            }
            detect();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.WindowState = FormWindowState.Minimized;
                this.Hide();
            }
        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {

            this.Activate();
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        private void menuStrip_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem.Text.Equals("Close"))
            {
                if (ts != null)
                {
                    ts.StopAutoRefresh();

                    ts.ItemInserted -= Ts_ItemInserted;
                    ts.ItemDeleted -= Ts_ItemDeleted;
                };
                Application.Exit();
            }
            else if (e.ClickedItem.Text.Equals("Open"))
            {
                this.Activate();
                this.Show();
                this.WindowState = FormWindowState.Normal;
            }
        }

        private void notifyIcon1_Click(object sender, EventArgs e)
        {
            menuStrip.Show(Cursor.Position);
        }

        private void autostartcheckBox_CheckStateChanged(object sender, EventArgs e)
        {
            if (autostartcheckBox.Checked)
            {
                using (RegistryKey Key = Registry.CurrentUser.OpenSubKey
                        ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
                {
                    try
                    {
                        Key.SetValue("DbDMatchNotification", Application.ExecutablePath);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error!");
                    }
                }
            }
            else
            {
                using (RegistryKey Key = Registry.CurrentUser.OpenSubKey
                        ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
                {
                    try
                    {
                        Key.DeleteValue("DbDMatchNotification");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error!");
                    }
                }
            }
        }

        /// <summary>
        /// Match detection
        /// </summary>
        private void detect()
        {
            if (ts == null)
            {
                ts = new TCPUDPConnections();
            };
            ts.AutoRefresh = true;
            ts.StartAutoRefresh();
            ts.ItemInserted += Ts_ItemInserted;
            ts.ItemDeleted += Ts_ItemDeleted;
        }

        private void Ts_ItemInserted(object sender, TCPUDPConnection item, int Position)
        {
            if (item.ProcessName.Contains("DeadByDaylight"))
            {
                if (item.Protocol.Equals(Protocol.UDP) && !tool_tip)
                {
                    tool_tip = true;
                    this.notifyIcon1.ShowBalloonTip(20, "Dead By Daylight", "Found a match", ToolTipIcon.Info);
                }
            }
        }

        private void Ts_ItemDeleted(object sender, TCPUDPConnection item, int Position)
        {
            if (item.ProcessName.Contains("DeadByDaylight"))
            {
                if (!item.Protocol.Equals(Protocol.UDP))
                {
                    tool_tip = false;
                }
            }
        }

    }
}
