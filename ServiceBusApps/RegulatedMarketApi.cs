using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls.Expressions;
using MGS.Account.Properties;
using MGS.AccountExternal.RegulatedMarkets;
using MGS.Logging;
using MGS.Account.Diagnostics;
using MGS.Account.Database;
using Newtonsoft.Json;


namespace MGS.Account.External.RegulatedMarkets
{
  public class RegulatedMarketApi
  {

    public static RegulatedMarketApi Instance;
    
    
    private readonly HttpClient _httpClient;

    /// <summary>
    /// 
    /// </summary>
    public RegulatedMarketApi()
    {
      
      var regulatedMarketApiUrl = Settings.Default.RegulatedMarketApiUrl;
      _httpClient = new HttpClient { BaseAddress = new Uri(regulatedMarketApiUrl) };
    }

    static RegulatedMarketApi()
    {
      Instance = new RegulatedMarketApi();
    }

    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="someStuff"></param>
    /// <param name="userID"></param>
    /// <param name="marketTypeId"></param>
    /// <param name="accountStatus"></param>
    /// <returns></returns>
    public async Task<bool> HandlePreLoginCheckAsync(PreLoginCheckRequest someStuff, int userID,  int marketTypeId, int accountStatus)
    {
      bool ret;
      int code = 0;
      string message;
      // Pre login check may require Regulated Market API involvement
      if (marketTypeId == 7)
      {

        if (String.IsNullOrEmpty(_httpClient.BaseAddress.ToString()))
        {
          throw new RegulatedMarketException((int)RegulatedMarketErrorCodeEnum.NoAPIUrl, "No URL can be found for the Regulated Market API.");
        }
        
        //string reqUri = string.Format("{0}/account/product/{1}/userName/{2}/verify", _httpClient.BaseAddress, someStuff.ProductID, someStuff.LoginName);
        //var response = await _httpClient.PostAsync(reqUri, new StringContent(string.Empty));

        string reqUri = string.Format("{0}/account/verify", _httpClient.BaseAddress);
        var response = await _httpClient.PostAsJsonAsync(reqUri, someStuff);
        
        if (!response.IsSuccessStatusCode)
        {
          Logger.LogWarningEvent("RMAPI returned status: {0}, reason: {1}", EventType.Network, null, response.StatusCode, response.ReasonPhrase);
          throw new RegulatedMarketException((int)RegulatedMarketErrorCodeEnum.CannotReachRMAPI, "There was a problem communicating with the regulated market api when performing the PreLoginCheck");
        }
        
        var result = await response.Content.ReadAsAsync<PreLoginCheckResult>();
        if (!MapAPICodeToErrorCode(result, marketTypeId, out code, out message))
        {
          //if it is an active / provisional player who has been excluded
          if ((accountStatus != 7) && code == (int)RegulatedMarketErrorCodeEnum.UserIsExcluded)
          {
            var storedProc = new UpdateAccountStatusStoredProcedure(userID, someStuff.ProductID, 7, 48);
            await WorkloadConsumer.ExecuteAsync(someStuff.ProductID, storedProc);
          }

          throw new RegulatedMarketException(code, message);
        }
        if (accountStatus == 7 && code == 0) //this is a player who is marked as excluded in the db, but has been un-excluded in EPIS
        {
          var storedProc = new UpdateAccountStatusStoredProcedure(userID, someStuff.ProductID, 4, 49);
          await WorkloadConsumer.ExecuteAsync(someStuff.ProductID, storedProc);

        }
        else if ((accountStatus == 1 || accountStatus == 0) && code == 0) //this is a provisional player, going to active
        {
          var storedProc = new UpdateAccountStatusStoredProcedure(userID, someStuff.ProductID, 4, 49);
          await WorkloadConsumer.ExecuteAsync(someStuff.ProductID, storedProc);
        }
      }
      if (code == 0 || code == 109)
      {
        return true;
      }
      else
      {
        return false;
      }
    }

    private bool MapAPICodeToErrorCode(PreLoginCheckResult res, int marketTypeId, out int code, out string message)
    {
      message = "";
      code = 0;
      if (marketTypeId == 7)
      {
        switch (res.Code)
        {
          case 2:
            code = (int)RegulatedMarketErrorCodeEnum.RMAPIDBError;
            message = "Regulated Market API Database error.";
            Logger.LogWarningEvent(message, EventType.RegulatedMarketExternalAPIError, null, null);
            break;
          case 100:
            code = (int)RegulatedMarketErrorCodeEnum.UserInvalidParams;
            message = "Invalid user parameters.";
            Logger.LogWarningEvent(string.Format("RMAPI: {0}.", message), EventType.RegulatedMarketInvalidParameters, null, null);
            break;
          case 104:
            Logger.LogWarningEvent("Regulated Market External API error", EventType.RegulatedMarketExternalAPIError, null, null);
            break;
          case 105:
            Logger.LogWarningEvent("Regulated Market External API timeout", EventType.RegulatedMarketExternalTimeout, null, null);
            break;
          case 106:
            code = (int)RegulatedMarketErrorCodeEnum.RMAPIUnknownError;
            message = "Regulated Market API Unknown Error.";
            Logger.LogWarningEvent(string.Format("RMAPI: {0}.", message), EventType.RegulatedMarketUnknownError, null, null);
            break;
          case 109:
            code = 109;
            Logger.LogWarningEvent("Regulated Market Provisional User", EventType.RegulatedMarketProvisionalAccount, null, null);
            break;
          case 110:
            code = (int) RegulatedMarketErrorCodeEnum.UserIsExcluded;
            message = "User is excluded.";
            Logger.LogWarningEvent(string.Format("RMAPI: {0}.", message), EventType.RegulatedMarketUserExcluded, null, null);
            break;
          case 111:
            Logger.LogWarningEvent("Regulated Market EMP Procedure", EventType.RegulatedMarketEMPProcedure, null, null);
            break;
          case 112:
            code = (int)RegulatedMarketErrorCodeEnum.UserIsUnknown;
            message = "Unknown user.";
            Logger.LogWarningEvent(string.Format("RMAPI: {0}.", message), EventType.RegulatedMarketUserUnknown, null, null);
            break;
          case 113:
            code = (int)RegulatedMarketErrorCodeEnum.CasinoIsKilled;
            message = "Casino is killed.";
            Logger.LogWarningEvent(string.Format("RMAPI: {0}.", message), EventType.RegulatedMarketCasinoKilled, null, null);
            break;
          case 119:
            code = (int)RegulatedMarketErrorCodeEnum.RMAPIConfigError;
            message = "RMAPI Config Error..";
            Logger.LogWarningEvent(string.Format("RMAPI: {0}.", message), EventType.RegulatedMarketConfigError, null, null);
            break;

        }
      }

      if (code == 0 || code == 109)
      {
        return true;
      }
      else
      {
        return false;
      }
      
    }
  
  }
}
