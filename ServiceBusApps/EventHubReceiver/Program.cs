using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventHubReceiver
{
  class Program
  {

    static string eventHubName = "nielsbevthub";
    static string hostName = "singleprocessor";
    static string consumerGroupName;
    private static string acctName = "nielsbstorage";
    private static string acctKey = "PIheAUbXDPo/00yCvnfHaUG3TnPkVVOzGjlGT3mERFvtGTqI0sQD05myzjXJZGAZQgxHigZv4UentABGokEMcQ==";
    static EventProcessorHost host;

    static void Main(string[] args)
    {
      consumerGroupName = EventHubConsumerGroup.DefaultGroupName;
      
      StartReceiver().Wait();

      while (true)
      {
        if (Console.ReadKey().Key == ConsoleKey.Q)
        {
          Console.WriteLine("About to shut down the processor...");
          host.UnregisterEventProcessorAsync().Wait();
          return;
        }
      }

      Console.WriteLine("We are done!");
      Console.ReadLine();


    }

    static async Task StartReceiver()
    {
      var eventHubConnectionString = GetEventHubConnectionString();
      var storageConnectionString = GetStorageConnectionString();

      
            
      host = new EventProcessorHost(
               hostName,
               eventHubName,
               consumerGroupName,
               eventHubConnectionString,
               storageConnectionString, 
               eventHubName.ToLowerInvariant());
      try
      {
        
        host.RegisterEventProcessorAsync<EventProcessor>().Wait();
      }

      catch (AggregateException agexp)
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(agexp.Flatten());
        Console.ResetColor();
      }

      catch (Exception exception)
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("{0} > Exception: {1}", DateTime.Now.ToString(), exception.Message);
        Console.ResetColor();
      }


    }

    static string GetEventHubConnectionString()
    {
      var connectionString = ConfigurationManager.AppSettings["Microsoft.ServiceBus.ConnectionString"];
      if (string.IsNullOrEmpty(connectionString))
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Did not find Service Bus connections string in appsettings (app.config)");
        Console.ResetColor();
        return string.Empty;
      }

      try
      {
        var builder = new ServiceBusConnectionStringBuilder(connectionString);
        builder.TransportType = TransportType.Amqp;
        return builder.ToString();
      }
      catch (Exception exception)
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(exception.Message);
        Console.ResetColor();
      }

      return string.Empty;
    }


    private static string GetStorageConnectionString()
    {
      var connectionString = string.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}", acctName, acctKey);
      if (string.IsNullOrEmpty(connectionString))
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Did not find storage connections string in appsettings (app.config) as host needs blob storage.");
        Console.ResetColor();
        return string.Empty;
      }

      return connectionString;
    }

  }
}
