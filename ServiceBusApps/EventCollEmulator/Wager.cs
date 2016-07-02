using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventCollEmulator
{
  public class Wager
  {
    public int gamingSystemId = 5001;
    public int userId { get; set; }
    public int socketId = -1;
    public int routerId = 9000;
    public int productId = 5999;
    public int moduleId { get; set; }
    public int clientId = 5;
    public string mgsUniqueGameName { get; set; }
    public string mgsGameCategory { get; set; }
    public int userTransnumber = -1;
    public int sessionId = -1;
    public string sessionGuid { get; set; }
    public int sessionClientTypeId = 5;
    public int sessionProductId = 5001;
    public decimal balanceAfterLastPositiveChange = 23.56M;
    public string currencyIsoCode = "EUR";
    public string operatorCurrencyIsoCode = "EUR";
    public decimal playerToOperatorExchangeRate = 1.00M;
    public decimal wagerAmount { get; set; }
    public decimal payoutAmount { get; set; }
    public decimal cashBalance = 123.56M;
    public decimal bonusBalance = 100.00M;
    public decimal totalBalance = 223.56M;
    public decimal betsOnTable = 0.0M;
    public decimal payoutsOnTable = 0.0M;
    public string[] playerGroups = new string[] {"5748FEEF-83C5-4534-BB6D-79CCF66A76C4", "16D75241-DF86-47C3-8147-C8032DD60959", "777434D9-28CC-46B0-BFAB-CE373B71FEDE", "B4247349-C2DE-4132-84E6-38D3B81B3E9D", "6C76EEF7-F980-4979-A9C1-E9A881025CDE"};
    public decimal minBetAmount = 1.00M;
    public decimal defaultBetAmount = 5.25M;
    public decimal maxBetAmount = 15.00M;
    public decimal theoreticalPayoutPercentage = 93.56M;
    public string utcEventTime { get; set; }
    public long ticksEventTime { get; set; }
    public decimal totalWagerAmount { get; set; }
    public decimal totalPayoutAmount { get; set; }
    public int isComplete = 1;
    public string countryLongCode = "SA";
    public string languageCode = "en";
    public string sessionCountryLongCode = "SA";
    public string sessionLanguageCode = "en";
    public string userName { get; set; }
    public int operatorId = 15000;


    private static int[] _moduleIds = new int[] {7, 78, 109, 135, 10025, 10031};
    public static int[] _userIds = new int[] {3002, 3010, 3030, 3354, 3522, 3578, 3650, 3678, 3722, 3934};



    static double NextDouble(Random rnd, double min, double max, int decimalPoints)
    {
      var dbl = rnd.NextDouble() * (max - min) + min;
      return Math.Round(dbl, decimalPoints, MidpointRounding.AwayFromZero);
    }


    public Wager()
    {
      bool isWin = false;
      
      var rnd = new Random();
      var uid = rnd.Next(0, 10);
      userId = _userIds[uid];
      userName = $"User: {userId}";
      var mid = rnd.Next(0, 6);
      moduleId = _moduleIds[mid];
      mgsUniqueGameName = $"Unique Game Name ModuleID: {moduleId}";
      mgsGameCategory = $"Game Category ModuleID: {moduleId}";
      sessionGuid = Guid.NewGuid().ToString();

      wagerAmount = (decimal) NextDouble(rnd, 1.00, 15.00, 2);
      payoutAmount = 0.0M;
      totalWagerAmount = wagerAmount;
      isWin = rnd.Next(0, 10)%3 == 0;

      if (isWin)
      {
        var multip = rnd.Next(1, 5);
        payoutAmount = wagerAmount*multip;
        totalPayoutAmount = payoutAmount;

      }

      utcEventTime = DateTime.UtcNow.ToString();
      ticksEventTime = DateTime.UtcNow.Ticks;

    }

  }
}
