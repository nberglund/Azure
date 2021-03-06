﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Hadoop.Avro.Container;
using Microsoft.Hadoop.Avro;
using System.Runtime.Serialization;

namespace EventCollEmulator
{
  [DataContract(Name = "AvroWager")]
  public class AvroWager
  {
    [DataMember(IsRequired = false)]
    public int gs { get; set; } //gamingSystemId

    [DataMember(IsRequired = false)]
    public int usr { get; set; } //userId

    [DataMember(IsRequired = false)]
    public int prd { get; set; } //productId

    [DataMember(IsRequired = false)]
    public int mid { get; set; } //moduleId

    [DataMember(IsRequired = false)]
    public int cid { get; set; } //clientId

    [DataMember(IsRequired = false)]
    public int utx { get; set; } //userTransnumber

    [DataMember(IsRequired = false)]
    public int sid { get; set; } //sessionId

    [DataMember(IsRequired = false)]
    public string sguid { get; set; } //sessionGuid

    [DataMember(IsRequired = false)]
    public int scltp { get; set; } //sesssionClientTypeId

    [DataMember(IsRequired = false)]
    public int sprd { get; set; } //sessionProductId

    [DataMember(IsRequired = false)]
    public decimal balpc { get; set; } //balanceAfterLastPositiveChange

    [DataMember(IsRequired = false)]
    public string cy { get; set; } //currencyIsoCode

    [DataMember(IsRequired = false)]
    public string ocy { get; set; } //operatorCurrencyIsoCode

    [DataMember(IsRequired = false)]
    public decimal ptox { get; set; } //playerToOperatorExchangeRate

    [DataMember(IsRequired = false)]
    public decimal wa { get; set; } //wagerAmount

    [DataMember(IsRequired = false)]
    public decimal pa { get; set; } //payoutAmount

    [DataMember(IsRequired = false)]
    public decimal cb { get; set; } //cashBalance

    [DataMember(IsRequired = false)]
    public decimal bb { get; set; } //bonusBalance

    [DataMember(IsRequired = false)]
    public decimal tb { get; set; } //totalBalance

    [DataMember(IsRequired = false)]
    public decimal bot { get; set; } //betsOnTable

    [DataMember(IsRequired = false)]
    public decimal pot { get; set; } //payoutsOnTable

    [DataMember(IsRequired = false)]
    public string[] pgs { get; set; } //playerGroups

    [DataMember(IsRequired = false)]
    public decimal mnba { get; set; } //minBetAmount

    [DataMember(IsRequired = false)]
    public decimal dba { get; set; } //defaultBetAmount

    [DataMember(IsRequired = false)]
    public decimal mxba { get; set; } //maxBetAmount

    [DataMember(IsRequired = false)]
    public decimal tpop { get; set; } //theoreticalPayoutPercentage

    [DataMember(IsRequired = false)]
    public string utet { get; set; } //utcEventTime

    [DataMember(IsRequired = false)]
    public long tet { get; set; } //ticksEventTime

    [DataMember(IsRequired = false)]
    public DateTime udtet { get; set; } //proper ticks eventtime converted to datetime
    
    [DataMember(IsRequired = false)]
    public decimal twa { get; set; } //totalWagerAmount

    [DataMember(IsRequired = false)]
    public decimal tpa { get; set; } //totalPayoutAmount

    [DataMember(IsRequired = false)]
    public int cmpl { get; set; } //isComplete

    [DataMember(IsRequired = false)]
    public string clc { get; set; } //countryLongCode

    [DataMember(IsRequired = false)]
    public string lc { get; set; } //languageCode

    [DataMember(IsRequired = false)]
    public string sclc { get; set; } //sessionCountryLongCode

    [DataMember(IsRequired = false)]
    public string slc { get; set; } //sessionLanguageCode

    [DataMember(IsRequired = false)]
    public int op { get; set; } //operatorId

    public AvroWager() { }

    public AvroWager(Wager w)
    {
      gs = w.gamingSystemId;
      usr = w.userId;
      prd = w.productId;
      mid = w.moduleId;
      cid = w.clientId;
      utx = w.userTransnumber;
      sid = w.sessionId;
      sguid = w.sessionGuid;
      scltp = w.sessionClientTypeId;
      sprd = w.sessionProductId;
      balpc = w.balanceAfterLastPositiveChange;
      cy = w.currencyIsoCode;
      ocy = w.operatorCurrencyIsoCode;
      ptox = w.playerToOperatorExchangeRate;
      wa = w.wagerAmount;
      pa = w.payoutAmount;
      cb = w.cashBalance;
      bb = w.bonusBalance;
      tb = w.totalBalance;
      bot = w.betsOnTable;
      pot = w.payoutsOnTable;
      pgs = w.playerGroups;
      mnba = w.minBetAmount;
      dba = w.defaultBetAmount;
      mxba = w.maxBetAmount;
      tpop = w.theoreticalPayoutPercentage;
      utet = w.utcEventTime;
      tet = w.ticksEventTime;
      udtet = new DateTime(tet);
      twa = w.totalWagerAmount;
      tpa = w.totalPayoutAmount;
      cmpl = w.isComplete;
      clc = w.countryLongCode;
      lc = w.languageCode;
      sclc = w.sessionCountryLongCode;
      slc = w.sessionLanguageCode;
      op = w.operatorId;

    }

  }
}

