using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Configuration;

namespace FxAdvisorHost
{
    public partial class Main : Form
    {
        private EventLog log = new EventLog();
        private int port;
        private string channelType;

        public Main()
        {
            InitializeComponent();
        }

        private void Main_Load(object sender, EventArgs e)
        {
            try
            {
                channelType = ConfigurationSettings.AppSettings["ChannelType"];
                port = int.Parse(ConfigurationSettings.AppSettings["Port"]);
            }
            catch (Exception)
            {
                log.AddEntry("Failed to load the configuration file. Using default configuration.",
                    EventSeverity.Error);
                port = 8080;
                channelType = "ipc";
            }

            ThreadPool.QueueUserWorkItem(new WaitCallback(AsyncConfigure));
            updateTimer.Enabled = true;
        }

        private void AsyncConfigure(object state)
        {
            try
            {
                string repository = Path.Combine(Application.StartupPath, "Advisors");

                Host.Instance.AdvisorsAdded += new EventHandler<HostEventArgs>(Instance_AdvisorsAdded);
                Host.COnfigure(log, channelType, port, repository);
            }
            catch (Exception ex)
            {
                log.AddEntry("Failed to configure host." + ex.ToString(), EventSeverity.Error);
            }
        }

        void Instance_AdvisorsAdded(object sender, HostEventArgs e)
        {
            this.BeginInvoke(
                new EventHandler<HostEventArgs>(updateGrid),
                new object[] {sender, e});
        }

        private void updateGrid(object sender, HostEventArgs e)
        {
            ListViewItem newItem = 
                this.listView1.Items.Add(e.RegisteredAdvisor.Name);
            newItem.SubItems.Add(e.RegisteredAdvisor.Url);
            newItem.SubItems.Add(string.Empty);
            newItem.Tag = e.RegisteredAdvisor;
        }

        private void updateAdvisorDetails()
        {
            listView1.BeginUpdate();
            foreach (ListViewItem item in this.listView1.Items)
            {
                RegisteredAdvisor regAdv = (RegisteredAdvisor)item.Tag;
                item.SubItems[2].Text = regAdv.Instance.Operations.ToString();
            }
            listView1.EndUpdate();
        }

        private void updateTimer_Tick(object sender, EventArgs e)
        {
            string entry;
            while( (entry = log.GetNextEvent()) != null )
            {
                this.richTextBoxLog.AppendText(entry);
                this.richTextBoxLog.AppendText(Environment.NewLine);
            }
            updateAdvisorDetails();
        }
    }
}
