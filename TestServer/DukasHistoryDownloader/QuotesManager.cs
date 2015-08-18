using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Ionic.Zip;

namespace DukasHistoryDownloader
{
    public class QuotesManager
    {
        static public void DownloadNewQuotes(string symbols, DateTime from, DateTime to, 
            bool IsSaveOnlyMinutes)
        {
            foreach (string csymbol in symbols.Split(new char[] { ',' }))
            {
                string symbol = csymbol.Trim();
                using (StreamWriter log = new StreamWriter(symbol + ".log", false))
                {
                    log.WriteLine("Start to download data.");
                    if (!Directory.Exists(symbol))
                        Directory.CreateDirectory(symbol);

                    //day
                    DateTime startDate = from;
                    DateTime endDate = to;
                    DateTime currentDate = endDate;
                    int errorCount = 0;

                    while (currentDate >= startDate)
                    {
                        foreach (int period in new int[] { 0 })
                        {
                            using (MemoryStream memStream = new MemoryStream())
                            {
                                using (StreamWriter streamWriter = new StreamWriter(memStream))
                                {

                                    if (0 == DownloadTicksPerDay(symbol, currentDate, streamWriter, log, period, IsSaveOnlyMinutes))
                                    {

                                        if (++errorCount > 100)
                                        {
                                            currentDate = startDate.AddDays(-1);
                                            break;
                                        }
                                        continue;
                                    }

                                    errorCount = 0;
                                    streamWriter.Flush();

                                    string fileName = currentDate.ToString("yyyy.MM.dd");
                                    string fileZipPath = symbol + "//" + fileName + ".zip";
                                    if (File.Exists(fileZipPath))
                                        File.Delete(fileZipPath);
                                    ZipFile zip = new ZipFile(fileZipPath);
                                    zip.AddFileStream(fileName + ".csv", "", memStream);
                                    zip.Save();
                                }
                            }
                        }

                        currentDate = currentDate.AddDays(-1);
                    }

                }
            }
        }
        static protected int DownloadTicksPerDay(string symbol, DateTime date, StreamWriter outputStream, StreamWriter log, int period, bool IsSaveOnlyMinutes)
        {
            int tickCount = 0;
            Tick previousTick = null;
            for (int currHour = 0; currHour < 24; currHour++)
            {
                if (date.DayOfWeek == DayOfWeek.Friday && currHour > 22)
                    continue;
                if (date.DayOfWeek == DayOfWeek.Saturday)
                    continue;
                if (date.DayOfWeek == DayOfWeek.Sunday && currHour < 20)
                    continue;
                log.WriteLine(symbol + ": Downloading " + period.ToString() + " period for " + date.ToShortDateString() + " HH=" + currHour.ToString());

                try
                {
                    string urlRequest;
                    urlRequest = string.Format("http://www.dukascopy.com/datafeed/{0}/{1}/{2}/{3}/{4}h_ticks.bin",
                    symbol, date.Year, (date.Month - 1).ToString("00"), date.Day.ToString("00"), currHour.ToString("00"));

                    MemoryStream mem = DownloadHelper.DownloadAndExtract(urlRequest);
                    int size = period == 0 ? 40 : 48;
                    byte[] tickRecord = new byte[size];
                    do
                    {
                        int readedBytes = mem.Read(tickRecord, 0, size);
                        if (readedBytes == 0)
                            break;
                        if (period == 0)
                        {
                            Tick tick = Tick.GetTickFromBff(tickRecord);
                            if(  previousTick!= null && previousTick.time.Ticks / Tick.tickInOneMinute != tick.time.Ticks / Tick.tickInOneMinute 
                                || !IsSaveOnlyMinutes )
                                outputStream.WriteLine(tick.ToString());

                            previousTick = tick;
                        }
                        tickCount++;
                    } while (true);
                }
                catch (Exception exc)
                {
                    log.WriteLine(exc.ToString());
                }
            }
            return tickCount;
        }
    }
}
