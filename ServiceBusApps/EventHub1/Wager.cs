using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// , ('wager', 'create schema wager epocheventtime long, operatorid long, routerid long, socketid long)')


namespace EventHub1
{
  public class Wager
  {
    public long moduleid { get; set; }
    public long clientid { get; set; }
    public long userid { get; set; }
    public long usertransnumber { get; set; }
    public long sessionid { get; set; }
    public long sessionserverid { get; set; }
    public long totalwager { get; set; }
    public long totalpayout { get; set; }
    public DateTime gametimeutc { get; set; }
    public long operatorid { get; set; }
    public long routerid { get; set; }
    public long socketid { get; set; }
   
  }
}
