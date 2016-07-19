using AxiomObjects;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Threading;

namespace DataAPI_Console
{
    public class Program
    {
        static StreamWriter writer = new StreamWriter("output.txt");

        private static void Main(string[] args)
        {
            // Controlled to be stopped. 
            var exitEvent = new ManualResetEvent(false);
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                exitEvent.Set();
            };

            writer.AutoFlush = true;
            string mqServer = ConfigurationManager.AppSettings["MqServer"];
            ushort mqPort = Convert.ToUInt16(ConfigurationManager.AppSettings["MqPort"]);
            string mqUser = ConfigurationManager.AppSettings["MqUser"];
            string mqPassword = ConfigurationManager.AppSettings["MqPassword"];
            string requestQueue = ConfigurationManager.AppSettings["RequestQueue"];
            string httpServer = ConfigurationManager.AppSettings["DataStoreManagerIp"];
            ushort httpPort = Convert.ToUInt16(ConfigurationManager.AppSettings["DataStoreManagerPort"]);

            DataAPI.DataAPI api = new DataAPI.DataAPI(mqServer, mqPort, mqUser, mqPassword, requestQueue, httpServer, httpPort, OnMessage);
            api.Start(); Console.WriteLine("API started. ");

            //api.Subscribe("600000-SSE", '2');
            api.Subscribe("600000-SSE", '1');
            //api.Subscribe("000001-SSE", '1');
            //api.Subscribe("EURUSD-MT4", '1');
            api.Subscribe("GBPUSD-MT4", '1');
            //api.Subscribe("Index1-AXIOM", '1');
            //api.Subscribe(MarketsEnum.MT4, '1');
            //api.Subscribe(MarketsEnum.CFFEX, '1');
            api.Subscribe("IF1607-CFFEX", '1');

            var c = api.GetSymbolList();
            if (c != null)
            {
                Console.WriteLine("Symbol count = {0}", c.Count);
                //Console.WriteLine(c.Contains("600000-SSE"));
                int i = 0;
                foreach (string symbol in c)
                {
                    Log.Info(String.Format("Symbol[{0}]: {1}", ++i, symbol));
                    //Console.WriteLine("Symbol[{0}]: {1}", ++i, symbol);
                }
            }

            var b = api.GetPreTradeInfo();
            if (b != null)
            {
                Console.WriteLine("Data count = {0}", b.Count);
            }

            List<Bar> a = api.GetHistoricalBarSync("600000-SSE", '1', DateTime.Today.AddDays(-1), DateTime.Now, 1); //IF1607-CFFEX
            api.GetHistoricalBarAsync("IF1608-CFFEX", '1', DateTime.Today, DateTime.Now, 5);    // Test of asynchronous method. 
            //List<MarketData> a = api.GetHistoricalTickSync("IF1607-CFFEX", '2', DateTime.Today, DateTime.Now);
            if (a != null)
            {
                Console.WriteLine("Data count = {0}", a.Count);
            }

            exitEvent.WaitOne();
            api.Stop(); Console.WriteLine("DataAPI Console stopped. ");
        }

        private static void OnMessage(object o)
        {
            if (o is MarketData)
            {
                MarketData data = o as MarketData;
                //Console.WriteLine(new DateTime(data.TickID) + " " + data.Symbol + " " + data.LastPrice);
                //Console.WriteLine(data.Time + "  " + (DateTime.Now - data.Time).TotalMilliseconds + "ms  " + data.Symbol + "  " + data.TickLevel + "  " + data.LastPrice);
                Console.WriteLine("{0}  {1:F3}ms  {2}  {3}  {4}", data.Time, ((DateTime.Now - data.Time).TotalMilliseconds), data.Symbol, data.TickLevel, data.LastPrice);
                if (data.LastVolume > 0)
                {
                    writer.WriteLine(data.Time.ToString("HH:mm:ss:fff") + "    " + data.LastPrice + "    " + data.LastVolume + "    " + "0");
                }
                //Log.debug(new DateTime(data.TickID) + " " + data.Symbol + " " + data.LastPrice);
            }
            else if (o is List<Bar>)
            {
                List<Bar> data = o as List<Bar>;
                Console.WriteLine("HistoricalBar Count = " + data.Count);
            }
            else if (o is List<MarketData>)
            {
                List<MarketData> data = o as List<MarketData>;
                Console.WriteLine("HistoricalTick Count = " + data.Count);
            }
        }
    }
}
