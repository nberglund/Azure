using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Hadoop.Avro;

namespace EventHubReceiver
{
  public class EventProcessor : IEventProcessor
  {

    PartitionContext partitionContext;
    Stopwatch checkpointStopWatch;
    public async Task CloseAsync(PartitionContext context, CloseReason reason)
    {
      Console.WriteLine(string.Format("Processor Shutting Down.  Partition '{0}', Reason: '{1}'. Offset: {2}", this.partitionContext.Lease.PartitionId, reason.ToString(), context.Lease.Offset));
      if (reason == CloseReason.Shutdown)
      {
        await context.CheckpointAsync();
      }
      
    }

    public Task OpenAsync(PartitionContext context)
    {
      Console.WriteLine(string.Format("EventProcessor initialize.  Partition: '{0}', Offset: '{1}'", context.Lease.PartitionId, context.Lease.Offset));
      
      this.partitionContext = context;
      //Console.WriteLine("Press any key to continue");
      //Console.ReadLine();
      this.checkpointStopWatch = new Stopwatch();
      this.checkpointStopWatch.Start();
      return Task.FromResult<object>(null);

    }

    public async Task ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> messages)
    {

      try
      {

        var avroSerializer = AvroSerializer.Create<AvroWager>();

        foreach (EventData eventData in messages)
        {
          
          //string msg = Encoding.UTF8.GetString(eventData.GetBytes());

          using (var buff = new MemoryStream(eventData.GetBytes()))
          {
            buff.Position = 0;
            var aw = avroSerializer.Deserialize(buff);


          }


          string key = eventData.PartitionKey;
                             
          Console.WriteLine(string.Format("Message received.  Partition: '{0}', Device: '{1}', Offset: {2}",
              this.partitionContext.Lease.PartitionId, key, context.Lease.Offset));

          context.Lease.Offset = eventData.Offset;

          //await context.CheckpointAsync();
        }

        //Call checkpoint every 5 minutes, so that worker can resume processing from the 5 minutes back if it restarts.
        if (this.checkpointStopWatch.Elapsed > TimeSpan.FromMinutes(5))
        {
          await context.CheckpointAsync();
          lock (this)
          {
            this.checkpointStopWatch.Reset();
          }
        }
      }
      catch (Exception exp)
      {
        Console.WriteLine("Error in processing: " + exp.Message);
      }

    }
  }
}
