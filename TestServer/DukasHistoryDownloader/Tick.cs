using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DukasHistoryDownloader
{
    public class Tick
    {
        public DateTime time;
        public double Ask, Bid, AskVol, BidVol;
        public static DateTime startTime = new DateTime(1970, 1, 1, 00, 00, 00);
        public static readonly long tickInOneMinute = new TimeSpan(0, 1, 0).Ticks;

        public static Tick GetTickFromBff(byte[] bytes)
        {
            Tick tick = new Tick();
            tick.time = new DateTime(10000 * GetInt64FromBytes(bytes, 0) + startTime.Ticks);
            tick.Ask = GetDoubleFromBytes(bytes, 8 * 1);
            tick.Bid = GetDoubleFromBytes(bytes, 8 * 2);
            tick.AskVol = GetDoubleFromBytes(bytes, 8 * 3);
            tick.BidVol = GetDoubleFromBytes(bytes, 8 * 4);
            return tick;
        }
        public override string ToString()
        {
            return string.Format("{0},{1},{2},{3},{4}", this.time.ToString("yyyy.MM.dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture), Ask, Bid, AskVol, BidVol);
        }
        public static Tick GetTickFromString(string str)
        {
            Tick tick = new Tick();
            string[] tokens = str.Split(new char[]{','});
            if (tokens.Length != 5)
                return null;
            tick.time = DateTime.ParseExact(tokens[0], "yyyy.MM.dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
            tick.time = tick.time.AddHours(3);
            tick.Ask = Double.Parse(tokens[1]);
            tick.Bid = Double.Parse(tokens[2]);
            tick.AskVol = Double.Parse(tokens[3]);
            tick.BidVol = Double.Parse(tokens[4]);
            return tick;
        }
        public static Int64 GetInt64FromBytes(byte[] bff, int shift)
        {
            return ((bff[7 + shift] & 0xFFL) << 0) +
                ((bff[6 + shift] & 0xFFL) << 8) +
                ((bff[5 + shift] & 0xFFL) << 16) +
                ((bff[4 + shift] & 0xFFL) << 24) +
                ((bff[3 + shift] & 0xFFL) << 32) +
                ((bff[2 + shift] & 0xFFL) << 40) +
                ((bff[1 + shift] & 0xFFL) << 48) +
                (((long)bff[0 + shift]) << 56);
        }
        public static Double GetDoubleFromBytes(byte[] bff, int shift)
        {
            return BitConverter.Int64BitsToDouble(GetInt64FromBytes(bff, shift));
        }

    }
}
