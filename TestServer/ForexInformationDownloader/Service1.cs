using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using DukasHistoryDownloader;
using log4net;
using log4net.Config;
using System.IO;
using News;

namespace ForexInformationDownloader
{
    public partial class ForexDownlader : ServiceBase
    {
        StreamWriter logger;
        string startupPath;

        System.Timers.Timer timer1;
        System.Timers.Timer timerNews;

        public ForexDownlader()
        {
            InitializeComponent();
        }
        #region Logger
        public void Info(string text)
        {
            logger.WriteLine(DateTime.Now.ToString()+ " Info: " + text);
            logger.Flush();
        }
        public void Error(string text)
        {
            logger.WriteLine(DateTime.Now.ToString() + " Error: " + text);
            logger.Flush();
        }
        #endregion

        protected override void OnStart(string[] args)
        {
            startupPath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            logger = new StreamWriter(Path.Combine(startupPath, "log.txt"), true);
            Info("OnStart");
            Info("OnStart2");
 
            //timer1 = new System.Timers.Timer(1000);
            //timer1.Elapsed += new System.Timers.ElapsedEventHandler(timer1_Elapsed);
            //timer1.Start();

            timerNews = new System.Timers.Timer(Settings1.Default.TimeoutMinutes*60000);
            timerNews.Elapsed += new System.Timers.ElapsedEventHandler(timerNews_Elapsed);

            timerNews_Elapsed(null, null);
            Info("OnStart3");

        }
        void timerNews_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Info("timerNews_Elapsed");
            timerNews.Enabled = false;
            
            try
            {
                NewsManager.UpdateNews(Settings1.Default.NewsPath);
            }
            catch (Exception exc)
            {
                Error(exc.ToString());
            }
            finally
            {
                timerNews.Start();
            }
        }

        void timer1_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //Info("timer1_Elapsed");
            //timer1.Enabled = false;

            //try
            //{
            //    NewsManager.UpdateNews(Settings1.Default.NewsPath);
            //    Directory.SetCurrentDirectory(startupPath);
            //    QuotesManager.DownloadNewQuotes(Settings1.Default.Symbols, DateTime.Now.AddDays(-7), DateTime.Now.AddDays(-1));
            //}
            //catch (Exception exc)
            //{
            //    Error(exc.ToString());
            //}
            //finally
            //{
            //    DateTime now = DateTime.Now;
            //    DateTime tommorow = DateTime.Now.AddDays(1);
            //    DateTime nextStart = new DateTime(tommorow.Year, tommorow.Month, tommorow.Day, Settings1.Default.StartHour, 0, 0);
            //    timer1.Interval = ((nextStart - now).TotalMilliseconds);
            //    Info("timer1 Interval = " + timer1.Interval.ToString());
            //    timer1.Enabled = true;
            //}
        }

        protected override void OnStop()
        {
            timer1.Stop();
        }

    }
}
