using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using log4net;
using log4net.Config;
using News;

//[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace DukasHistoryDownloader
{
    public partial class Form1 : Form
    {
        //private static readonly ILog logger = LogManager.GetLogger(typeof(Form1));
        static DateTime startTime = new DateTime(1970, 1, 1, 00, 00, 00);

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            TextBoxSymbol.Text = "AUDJPY, AUDNZD, AUDUSD, CADJPY, CHFJPY, EURAUD, EURCAD, EURCHF, EURGBP, EURJPY, EURNOK, EURSEK, EURUSD, GBPCHF, GBPJPY, GBPUSD, NZDUSD, USDCAD, USDCHF, USDJPY, USDNOK, USDSEK";
        }

        private void button4_Click(object sender, EventArgs e)
        {
            button4.Enabled = false;

            QuotesManager.DownloadNewQuotes( TextBoxSymbol.Text, dateTimePicker1.Value.Date, dateTimePicker2.Value.Date, true);

            button4.Enabled = true;
        }

        private void ButtonDownloadNews_Click(object sender, EventArgs e)
        {
            NewsManager.UpdateNews(System.Configuration.ConfigurationManager.AppSettings["NewsFilePath"]);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            NewsManager.ReadWrite(System.Configuration.ConfigurationManager.AppSettings["NewsFilePath"]);
        }

        private void ButtonDownloadTicks_Click(object sender, EventArgs e)
        {
            ButtonDownloadTicks.Enabled = false;

            QuotesManager.DownloadNewQuotes(TextBoxSymbol.Text, dateTimePicker1.Value.Date, dateTimePicker2.Value.Date, false);

            ButtonDownloadTicks.Enabled = true;
        }
    }
}
