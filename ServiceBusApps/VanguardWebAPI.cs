using System;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using MGS.Account.Diagnostics;
using MGS.Account.Properties;
using MGS.Json;
using MGS.Logging;

namespace MGS.Account.External
{
  public class VanguardWebAPI : HttpClient
  {
    private readonly object _gate = new object();
    private string _serviceToken;

    private static readonly UTF8Encoding _encoding = new UTF8Encoding();
    private static readonly Random _randomWait = new Random((int)DateTime.UtcNow.Ticks);

    public VanguardWebAPI()
    {
      BaseAddress = new Uri(Settings.Default.VanguardURL);
      Timeout = TimeSpan.FromSeconds(Settings.Default.VanguardTimeoutSeconds);

      _serviceToken = AccountService.ServiceToken;
      if (_serviceToken != null)
      {
        DefaultRequestHeaders.Add(HttpRequestHeader.Authorization.ToString(), new AuthenticationHeaderValue("bearer", _serviceToken).ToString());
      }
    }

    private void EnsureServiceToken()
    {
      if (_serviceToken == null || _serviceToken == AccountService.ServiceToken)
      {
        return;
      }
      lock (_gate)
      {
        if (_serviceToken == AccountService.ServiceToken)
        {
          return;
        }
        _serviceToken = AccountService.ServiceToken;
        DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", _serviceToken);
      }
    }

    public async Task<LoginResponse> Login(LoginRequest loginRequest, int retryCount = 3)
    {
      EnsureServiceToken();

      using (HttpContent httpRequest = new StringContent(loginRequest.ToJson(), _encoding, "application/json"))
      {
        string postUri = String.Format(CultureInfo.InvariantCulture, "{0}/api/login", BaseAddress);
        using (HttpResponseMessage httpResponse = await PostAsync(postUri, httpRequest).ConfigureAwait(false))
        {
          string json = (await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false));

          var loginResponse = !json.StartsWith("<") ? json.FromJson<LoginResponse>() : null;
          if (HandleExternalResponse(postUri, httpResponse, loginResponse, retryCount))
          {
            return loginResponse;
          }

          Logger.LogVerboseEvent("Vanguard API HTTP error. Retrying...", EventType.VanguardAPI, httpResponse);
        }
      }

      return await Retry(Login, loginRequest, retryCount).ConfigureAwait(false);
    }

    public async Task<GetTokenResponse> GetToken(GetTokenRequest getTokenRequest, int retryCount = 3)
    {
      
      using (HttpContent httpRequest = new StringContent(getTokenRequest.ToJson(), _encoding, "application/json"))
      {
        string postUri = String.Format(CultureInfo.InvariantCulture, "{0}/api/gettoken", BaseAddress);
        using (HttpResponseMessage httpResponse = await PostAsync(postUri, httpRequest).ConfigureAwait(false))
        {
          string json = (await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false));

          var tokenResponse = !json.StartsWith("<") ? json.FromJson<GetTokenResponse>() : null;
          if (HandleExternalResponse(postUri, httpResponse, tokenResponse, retryCount))
          {
            return tokenResponse;
          }

          Logger.LogVerboseEvent("Vanguard API HTTP error. Retrying...", EventType.VanguardAPI, httpResponse);
        }
      }

      return await Retry(GetToken, getTokenRequest, retryCount).ConfigureAwait(false);
    }

    public async Task<GetBalanceResponse> GetBalance(GetBalanceRequest getBalanceRequest, int retryCount = 3)
    {
      EnsureServiceToken();

      using (HttpContent httpRequest = new StringContent(getBalanceRequest.ToJson(), _encoding, "application/json"))
      {
        string postUri = String.Format(CultureInfo.InvariantCulture, "{0}/api/getbalance", BaseAddress);
        using (HttpResponseMessage httpResponse = await PostAsync(postUri, httpRequest).ConfigureAwait(false))
        {
          string json = (await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false));

          var balanceResponse = !json.StartsWith("<") ? json.FromJson<GetBalanceResponse>() : null;
          if (HandleExternalResponse(postUri, httpResponse, balanceResponse, retryCount))
          {
            return balanceResponse;
          }

          Logger.LogVerboseEvent("Vanguard API HTTP error. Retrying...", EventType.VanguardAPI, httpResponse);
        }
      }

      return await Retry(GetBalance, getBalanceRequest, retryCount).ConfigureAwait(false);
    }

    public async Task<UpdateBalanceResponse> UpdateBalance(UpdateBalanceRequest updateRequest, int retryCount = 3)
    {
      EnsureServiceToken();

      using (HttpContent httpRequest = new StringContent(updateRequest.ToJson(), _encoding, "application/json"))
      {
        string postUri = String.Format(CultureInfo.InvariantCulture, "{0}/api/updatebalance", BaseAddress);
        using (HttpResponseMessage httpResponse = await PostAsync(postUri, httpRequest).ConfigureAwait(false))
        {
          string json = (await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false));

          var updateResponse = !json.StartsWith("<") ? json.FromJson<UpdateBalanceResponse>() : null;
          if (HandleExternalResponse(postUri, httpResponse, updateResponse, retryCount))
          {
            return updateResponse;
          }

          Logger.LogVerboseEvent("Vanguard API HTTP error. Retrying...", EventType.VanguardAPI, httpResponse);
        }
      }

      return await Retry(UpdateBalance, updateRequest, retryCount).ConfigureAwait(false);
    }

    private static bool HandleExternalResponse(string postUri, HttpResponseMessage httpResponse, IExternalResponse externalResponse, int retryCount)
    {
      bool canRetry = CanRetry(httpResponse, retryCount);
      if (externalResponse == null && !canRetry)
      {
        throw new VanguardException(httpResponse) {Url = postUri};
      }

      if (externalResponse != null)
      {
        Logger.LogVerboseEvent("Vanguard response received", EventType.VanguardAPI, externalResponse);
        if (externalResponse.Error == null)
        {
          return true;
        }
        if (!canRetry)
        {
          throw new VanguardException(httpResponse, externalResponse.Error) {Url = postUri};
        }
      }

      return false;
    }

    private static bool CanRetry(HttpResponseMessage response, int retryCount)
    {
      if (retryCount == 0)
      {
        return false;
      }
      return response.StatusCode >= (HttpStatusCode)500;
    }

    private async Task<TResult> Retry<T1, TResult>(Func<T1, int, Task<TResult>> retryMethod, T1 arg1, int retryCount)
    {
      return await Task.Delay(_randomWait.Next(25, 76))
                       .ContinueWith((task, state) => retryMethod((T1)state, --retryCount), arg1, TaskContinuationOptions.ExecuteSynchronously)
                       .Unwrap();
    }
  }
}
