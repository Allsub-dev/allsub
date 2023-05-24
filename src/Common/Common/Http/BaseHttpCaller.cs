using Microsoft.Extensions.Logging;
using AllSub.Common.Helpers;
using AllSub.CommonCore.Interfaces.Http;
using AllSub.CommonCore.Services;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net.Http.Formatting;
using System.Net.Http.Json;

namespace AllSub.Common.Http
{
    public abstract class BaseHttpCaller
    {
        private readonly Type _stringType = typeof(string);
        private readonly HttpClient _client;
        protected abstract ILogger Log { get; }

        private const string ResponseIsNullMessage = "HttpClient returns null";

        public BaseHttpCaller(HttpClient httpClient)
        {
            _client = httpClient;
        }

        public async Task<bool> DeleteAsync(string restApiPath)
        {
            LogInformation($"DeleteAsync called for: '{restApiPath}'");
            using HttpResponseMessage response = await _client.DeleteAsync(restApiPath).ConfigureAwait(false) ?? throw new ServiceException(ResponseIsNullMessage);
            return response.IsSuccessStatusCode;
        }

        public async Task<ICallerResult<T>> GetAsync<T>(string restApiPath)
        {
            var returnType = typeof(T);
            if (returnType == _stringType)
            {
                LogInformation($"GetAsync (return value is string) is called for: '{restApiPath}'");
                return (ICallerResult<T>)(object)await GetStringResponseAsync(() => _client.GetAsync(restApiPath)).ConfigureAwait(false);
            }

            LogInformation($"GetAsync (return value is {returnType.Name}) called for: '{restApiPath}'");
            return await GetResponseAsync<T>(() => _client.GetAsync(restApiPath)).ConfigureAwait(false);
        }

        public async Task<ICallerResult<TReturn>> PostAsync<TReturn>(string restApiPath, IEnumerable<KeyValuePair<string, string>> formVariables)
        {
            LogInformation($"PostAsync called for: '{restApiPath}'");
            var formContent = new FormUrlEncodedContent(formVariables);
            return await GetResponseAsync<TReturn>(() => _client.PostAsync(restApiPath, formContent)).ConfigureAwait(false);
        }
        public async Task<ICallerResult<TReturn>> PostAsync<TReturn>(string restApiPath)
        {
            LogInformation($"PostAsync (no data) called for: '{restApiPath}'");
            return await PostAsync<EmptyClass, TReturn>(restApiPath, new EmptyClass()).ConfigureAwait(false);
        }
        public async Task<ICallerResult<TReturn>> PostAsync<T, TReturn>(string restApiPath, T entity) where T : class
        {
            var returnType = typeof(TReturn);
            if (returnType == _stringType)
            {
                LogInformation($"PostAsync (return value is string) is called for: '{restApiPath}'");
                return (ICallerResult<TReturn>)(object)await GetStringResponseAsync(() => _client.PostAsJsonAsync(restApiPath, entity)).ConfigureAwait(false);
            }

            LogInformation($"PostAsync (return value is {returnType.Name}) called for: '{restApiPath}'");
            return await GetResponseAsync<TReturn>(() => _client.PostAsJsonAsync(restApiPath, entity)).ConfigureAwait(false);
        }

        public Task<ICallerResult<T>> PutAsync<T>(string restApiPath)
        {
            return PutAsync<EmptyClass, T>(restApiPath, new EmptyClass());
        }
        public async Task<ICallerResult<TReturn>> PutAsync<T, TReturn>(string restApiPath, T entity) where T : class
        {
            var returnType = typeof(TReturn);
            if (returnType == _stringType)
            {
                LogInformation($"PutAsync (return value is string) is called for: '{restApiPath}'");
                return (ICallerResult<TReturn>)(object)await GetStringResponseAsync(() => _client.PutAsJsonAsync(restApiPath, entity)).ConfigureAwait(false);
            }

            LogInformation($"PutAsync (return value is {returnType.Name}) called for: '{restApiPath}'");
            return await GetResponseAsync<TReturn>(() => _client.PutAsJsonAsync(restApiPath, entity)).ConfigureAwait(false);
        }

        private async Task<ICallerResult<TReturn>> GetResponseAsync<TReturn>(
           Func<Task<HttpResponseMessage>> callFunc)
        {
            using HttpResponseMessage response = await callFunc().ConfigureAwait(false) ?? throw new ServiceException(ResponseIsNullMessage);

            var result = default(TReturn);
            if (!response.IsSuccessStatusCode)
            {
                LogError($"Http call was unsuccessful. Statuscode: {response.StatusCode}. Reasonphrase: {response.ReasonPhrase}.");
            }
            else if (response.Content != null)
            {
                try
                {
                    result = await response.Content.ReadAsAsync<TReturn>().ConfigureAwait(false);
                }
                catch (JsonReaderException exception)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    LogError($"The response has bad json: {content}", exception);
                    throw;
                }
            }

            return new HttpCallerResult<TReturn>(response, result);
        }

        private async Task<ICallerResult<string>> GetStringResponseAsync(
            Func<Task<HttpResponseMessage>> callFunc)
        {
            using HttpResponseMessage response = await callFunc().ConfigureAwait(false) ?? throw new ServiceException(ResponseIsNullMessage);

            string? result = null;
            if (!response.IsSuccessStatusCode)
            {
                LogError($"Http call was unsuccessful. Statuscode: {response.StatusCode}. Reasonphrase: {response.ReasonPhrase}.");
            }
            else if (response.Content != null)
            {
                result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }

            return new HttpCallerResult<string>(response, result); ;
        }

        private class EmptyClass
        {
        }

        private void LogInformation(string message)
        {
            Log.LogInformation("{message}", message);
        }

        private void LogError(string message, object? data = null)
        {
            if (data != null)
            {
                Log.LogError("{message}", message + JsonHelper.GetJson(data));
            }
            else
            {
                Log.LogError("{message}", message);
            }
        }

        private void LogError(string message, Exception exception)
        {
            Log.LogError(exception, "{message}", message);
        }
    }
}
