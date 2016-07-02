using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Hadoop.Avro.Container;
using Microsoft.Hadoop.Avro;

namespace EventCollEmulator
{
  class Program
  {

    private static EventHubSender _ehSender;

    static void Main(string[] args)
    {

      Console.WriteLine("Creating instance of EventHubSender");
      _ehSender = new EventHubSender();

      Console.WriteLine("Sender created. Press any key to start sending");
      Console.ReadLine();

      Run(10).Wait();

      Console.WriteLine("We are done. Press any key to shutdown");
      Console.ReadLine();

      _ehSender.ShutDown();
      Console.WriteLine("We have shut down. Press any key to exit");
      Console.ReadLine();




    }


    static async Task Run(int nmbrMsgs)
    {
      Random rnd = new Random();
      for (int i = 0; i < nmbrMsgs; i++)
      {
        GenerateWagerAndSend();
        var delay = rnd.Next(10, 501);
        Console.WriteLine("Generated and sent msg: {0}. Waiting {1} ms to send next.", i + 1, delay);
        await Task.Delay(delay);
      }

      Console.WriteLine("Sent {0} messages", nmbrMsgs);
    }

    static async void GenerateWagerAndSend()
    {
      try
      {
        byte[] avroBuffer;
        var w = new Wager();
        var aw = new AvroWager(w);
        var avroSerializer = AvroSerializer.Create<AvroWager>();

        var jsonString = JsonConvert.SerializeObject(aw);
        avroBuffer = Encoding.UTF8.GetBytes(jsonString);
        
        //using (var buffer = new MemoryStream())
        //{
        //  avroSerializer.Serialize(buffer, aw);
        //  buffer.Position = 0;
        //  avroBuffer = buffer.GetBuffer();
        //}

        //var sysProps = new Dictionary<string, object>();
        //sysProps.Add("content-type", "avro/bin");
        ////sysProps.Add("Content-Type", "avro/bin");

        var props = new Dictionary<string, object>();
        props.Add("EventType", "wager");
        
        await _ehSender.Send(avroBuffer, properties:props).ConfigureAwait(false);
      }
      catch (AggregateException ae)
      {
        Console.WriteLine(ae.Flatten().Message);
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
      }

    }

  }
}
