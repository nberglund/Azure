using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventHubSender
{
  class Program
  {
    static void Main(string[] args)
    {

      Console.WriteLine("Press Ctrl-C to stop the sender process");
      Console.WriteLine("Press Enter to start now");
      Console.ReadLine();


      SendingRandomMessages().Wait();

      Console.WriteLine("If we hit this, then something is wrong");
      Console.ReadLine();

    }

    static async Task SendingRandomMessages()
    {
      int nCount = 0;
      var eventHubName = "eh1";
      var eventHubConnectionString = GetEventHubConnectionString();
      var eventHubClient = EventHubClient.CreateFromConnectionString(eventHubConnectionString, eventHubName);
      while (nCount < 8)
      {
        try
        {
          nCount += 1;
          var message = string.Format("{0}", nCount);
          Console.WriteLine("{0} > Sending message: {1}", DateTime.Now.ToString(), message);
          var msg = new EventData(Encoding.UTF8.GetBytes(message));
          //msg.PartitionKey = "Hello";
          await eventHubClient.SendAsync(msg);
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

        // we send it slowly...
        await Task.Delay(200);
      }
    }


    static string GetEventHubConnectionString()
    {
      var connectionString = ConfigurationManager.AppSettings["Microsoft.ServiceBus.ConnectionString"];
      if (string.IsNullOrEmpty(connectionString))
      {
        Console.WriteLine("Did not find Service Bus connections string in appsettings (app.config)");
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

      return null;
    }

  }
}
