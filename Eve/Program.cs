using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Eve.Database;
using Eve.Domain;
using Newtonsoft.Json.Linq;
using ZMQ;
using log4net;
using log4net.Repository.Hierarchy;

namespace Eve
{
    class Program
    {
        private static readonly ILog logger = log4net.LogManager.GetLogger(typeof (Program));

        static void Main2(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();

            try
            {
                logger.Info("Starting...");
                UpdateDataFromEmdr();
            }
            catch(System.Exception ex)
            {
                logger.Error("Fatal Exception...", ex);
                Console.WriteLine("An exception occured:");
                Console.WriteLine(ex.ToString());
                Console.ReadKey();
            }
        }

        private static void UpdateDataFromEmdr()
        {
            using (var context = new Context())
            {
                using (var subscriber = context.Socket(SocketType.SUB))
                {
                    //Connect to the first publicly available relay.
                    //subscriber.Connect("tcp://relay-us-central-1.eve-emdr.com:8050");
                    //subscriber.Connect("tcp://relay-us-west-1.eve-emdr.com:8050");
                    subscriber.Connect("tcp://relay-us-east-1.eve-emdr.com:8050");

                    // Disable filtering.
                    subscriber.SetSockOpt(SocketOpt.SUBSCRIBE, Encoding.UTF8.GetBytes(""));

                    // Alternatively 'Subscribe' can be used
                    //subscriber.Subscribe("", Encoding.UTF8);
                    while (true)
                    {
                        try
                        {
                            RecieveNextBlockOfData(subscriber);
                        }
                        catch(System.Exception ex)
                        {
                            logger.Error("Error in UpdateDataFromEmdr loop.", ex);
                        }
                    }
                }
            }
        }

        static int ordercount = 0;
        private static void RecieveNextBlockOfData(Socket subscriber)
        {
            try
            {
                // Receive compressed raw market data.
                var receivedData = subscriber.Recv();
                var start = DateTime.Now;
                // The following code lines remove the need of 'zlib' usage;
                // 'zlib' actually uses the same algorith as 'DeflateStream'.
                // To make the data compatible for 'DeflateStream', we only have to remove
                // the four last bytes which are the adler32 checksum and
                // the two first bytes which are the 'zlib' header.
                byte[] decompressed;

                // Decompress the raw market data.
                using (var inStream = new MemoryStream(receivedData, 2, receivedData.Length-6))
                using (var outStream = new MemoryStream())
                {
                    var outZStream = new DeflateStream(inStream, CompressionMode.Decompress);
                    outZStream.CopyTo(outStream);
                    decompressed = outStream.ToArray();
                }

                // Transform data into JSON strings.
                string marketJson = Encoding.UTF8.GetString(decompressed);

                // Un-serialize the JSON data to a dictionary.
                var dictionary = JObject.Parse(marketJson);

                // Dump the market data to console or, you know, do more fun things here.
                //foreach (KeyValuePair<string, JToken> pair in dictionary)
                //{
                //    Console.WriteLine("{0}: {1}", pair.Key, pair.Value);
                //}
                if (dictionary["resultType"].Value<string>() == "orders")
                {
                    Console.WriteLine("Got order {0}", ++ordercount);
                    OrdersUpdate marketUpdate;
                    try
                    {
                        marketUpdate = DomainDeserializer.DeserializeIntoMarketUpdate(dictionary);
                    }
                    catch(System.Exception ex)
                    {
                        logger.Error("Error deserializing...", ex);
                        logger.Error(marketJson);
                        throw;
                    }


                    //These two operations should be setup so either can error without stopping the other from working
                    EveDatabase.WriteOrdersUpdateToDatabase(marketUpdate, 0);

                    //using (var context = new EveHistoryContext())
                    //{
                    //    context.OrdersUpdates.Add(marketUpdate);
                    //    context.SaveChanges();
                    //}
                }
                //var duration = DateTime.Now.Subtract(start).TotalMilliseconds;
                //Console.WriteLine("Processing took: {0}ms", duration);
            }
            catch (ZMQ.Exception ex)
            {
                Console.WriteLine("ZMQ Exception occurred : {0}", ex);
            }
        }
    }
}
