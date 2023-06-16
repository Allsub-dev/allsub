using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System.Diagnostics;
using System.Globalization;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace AllSub.OAuth.Vk
{
    /// <summary>
    /// Authentication handler for Vk's OAuth based authentication.
    /// </summary>
    public class VkAuthenticationHandler : OAuthHandler<VkOptions>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="VkAuthenticationHandler"/>.
        /// </summary>
        /// <inheritdoc />
        public VkAuthenticationHandler(IOptionsMonitor<VkOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock)
        { }

        /// <inheritdoc />
        protected override async Task<AuthenticationTicket> CreateTicketAsync(
            ClaimsIdentity identity,
            AuthenticationProperties properties,
            OAuthTokenResponse tokens)
        {
            // Get the Vk user
            var queryStrings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            queryStrings.Add("access_token", tokens.AccessToken ?? string.Empty);
            queryStrings.Add("v", Options.ApiVersion);
            //queryStrings.Add("fields", "id,email,first_name,last_name");

            var userInfoEndpoint = QueryHelpers.AddQueryString(Options.UserInformationEndpoint, queryStrings!);

            var request = new HttpRequestMessage(HttpMethod.Get, userInfoEndpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);

            var response = await Backchannel.SendAsync(request, Context.RequestAborted);
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"An error occurred when retrieving Vk user information ({response.StatusCode}). Please check if the authentication information is correct.");
            }

            using (var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync(Context.RequestAborted)))
            {
                var context = new OAuthCreatingTicketContext(new ClaimsPrincipal(identity), properties, Context, Scheme, Options, Backchannel, tokens, payload.RootElement);
                context.RunClaimActions();
                await Events.CreatingTicket(context);
                return new AuthenticationTicket(context.Principal!, context.Properties, Scheme.Name);
            }
        }

        /// <inheritdoc />
        protected override async Task<HandleRequestResult> HandleRemoteAuthenticateAsync()
        {
            var query = Request.Query;

            var state = query["state"];
            var properties = Options.StateDataFormat.Unprotect(state);

            if (properties == null)
            {
                return HandleRequestResult.Fail("The oauth state was missing or invalid.");
            }

            // OAuth2 10.12 CSRF
            if (!ValidateCorrelationId(properties))
            {
                return HandleRequestResult.Fail("Correlation failed.", properties);
            }

            var error = query["error"];
            if (!StringValues.IsNullOrEmpty(error))
            {
                // Note: access_denied errors are special protocol errors indicating the user didn't
                // approve the authorization demand requested by the remote authorization server.
                // Since it's a frequent scenario (that is not caused by incorrect configuration),
                // denied errors are handled differently using HandleAccessDeniedErrorAsync().
                // Visit https://tools.ietf.org/html/rfc6749#section-4.1.2.1 for more information.
                var errorDescription = query["error_description"];
                var errorUri = query["error_uri"];
                if (StringValues.Equals(error, "access_denied"))
                {
                    var result = await HandleAccessDeniedErrorAsync(properties);
                    if (!result.None)
                    {
                        return result;
                    }
                    var deniedEx = new Exception("Access was denied by the resource owner or by the remote server.");
                    deniedEx.Data["error"] = error.ToString();
                    deniedEx.Data["error_description"] = errorDescription.ToString();
                    deniedEx.Data["error_uri"] = errorUri.ToString();

                    return HandleRequestResult.Fail(deniedEx, properties);
                }

                var failureMessage = new StringBuilder();
                failureMessage.Append(error);
                if (!StringValues.IsNullOrEmpty(errorDescription))
                {
                    failureMessage.Append(";Description=").Append(errorDescription);
                }
                if (!StringValues.IsNullOrEmpty(errorUri))
                {
                    failureMessage.Append(";Uri=").Append(errorUri);
                }

                var ex = new Exception(failureMessage.ToString());
                ex.Data["error"] = error.ToString();
                ex.Data["error_description"] = errorDescription.ToString();
                ex.Data["error_uri"] = errorUri.ToString();

                return HandleRequestResult.Fail(ex, properties);
            }

            var code = query["code"];

            if (StringValues.IsNullOrEmpty(code))
            {
                return HandleRequestResult.Fail("Code was not found.", properties);
            }

            var codeExchangeContext = new OAuthCodeExchangeContext(properties, code.ToString(), BuildRedirectUri(Options.CallbackPath));
            using var tokens = await ExchangeCodeAsync(codeExchangeContext);

            if (tokens.Error != null)
            {
                return HandleRequestResult.Fail(tokens.Error, properties);
            }

            if (string.IsNullOrEmpty(tokens.AccessToken))
            {
                return HandleRequestResult.Fail("Failed to retrieve access token.", properties);
            }

            var identity = new ClaimsIdentity(ClaimsIssuer);

            if (Options.SaveTokens)
            {
                var authTokens = new List<AuthenticationToken>();

                authTokens.Add(new AuthenticationToken { Name = "access_token", Value = tokens.AccessToken });
                if (!string.IsNullOrEmpty(tokens.RefreshToken))
                {
                    authTokens.Add(new AuthenticationToken { Name = "refresh_token", Value = tokens.RefreshToken });
                }

                if (!string.IsNullOrEmpty(tokens.TokenType))
                {
                    authTokens.Add(new AuthenticationToken { Name = "token_type", Value = tokens.TokenType });
                }

                if (!string.IsNullOrEmpty(tokens.ExpiresIn))
                {
                    int value;
                    if (int.TryParse(tokens.ExpiresIn, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
                    {
                        // https://www.w3.org/TR/xmlschema-2/#dateTime
                        // https://msdn.microsoft.com/en-us/library/az4se3k1(v=vs.110).aspx
                        var expiresAt = Clock.UtcNow + TimeSpan.FromSeconds(value);
                        authTokens.Add(new AuthenticationToken
                        {
                            Name = "expires_at",
                            Value = expiresAt.ToString("o", CultureInfo.InvariantCulture)
                        });
                    }
                }

                properties.StoreTokens(authTokens);
            }

            var ticket = await CreateTicketAsync(identity, properties, tokens);
            if (ticket != null)
            {
                return HandleRequestResult.Success(ticket);
            }
            else
            {
                return HandleRequestResult.Fail("Failed to retrieve user information from remote server.", properties);
            }
        }


        /// <summary>
        /// Handles the current authentication request.
        /// </summary>
        /// <returns><see langword="true"/> if authentication was handled, otherwise <see langword="false"/>.</returns>
        public override async Task<bool> HandleRequestAsync()
        {
            if (!await ShouldHandleRequestAsync())
            {
                return false;
            }

            AuthenticationTicket? ticket = null;
            Exception? exception = null;
            AuthenticationProperties? properties = null;
            try
            {
                var authResult = await HandleRemoteAuthenticateAsync();
                if (authResult == null)
                {
                    exception = new InvalidOperationException("Invalid return state, unable to redirect.");
                }
                else if (authResult.Handled)
                {
                    return true;
                }
                else if (authResult.Skipped || authResult.None)
                {
                    return false;
                }
                else if (!authResult.Succeeded)
                {
                    exception = authResult.Failure ?? new InvalidOperationException("Invalid return state, unable to redirect.");
                    properties = authResult.Properties;
                }

                ticket = authResult?.Ticket;
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            if (exception != null)
            {
                //Logger.RemoteAuthenticationError(exception.Message);
                var errorContext = new RemoteFailureContext(Context, Scheme, Options, exception)
                {
                    Properties = properties
                };
                await Events.RemoteFailure(errorContext);

                if (errorContext.Result != null)
                {
                    if (errorContext.Result.Handled)
                    {
                        return true;
                    }
                    else if (errorContext.Result.Skipped)
                    {
                        return false;
                    }
                    else if (errorContext.Result.Failure != null)
                    {
                        throw new Exception("An error was returned from the RemoteFailure event.", errorContext.Result.Failure);
                    }
                }

                if (errorContext.Failure != null)
                {
                    throw new Exception("An error was encountered while handling the remote login.", errorContext.Failure);
                }
            }

            // We have a ticket if we get here
            Debug.Assert(ticket != null);
            var ticketContext = new TicketReceivedContext(Context, Scheme, Options, ticket)
            {
                ReturnUri = ticket.Properties.RedirectUri
            };

            ticket.Properties.RedirectUri = null;

            // Mark which provider produced this identity so we can cross-check later in HandleAuthenticateAsync
            ticketContext.Properties!.Items[".AuthScheme"] = Scheme.Name;

            await Events.TicketReceived(ticketContext);

            if (ticketContext.Result != null)
            {
                if (ticketContext.Result.Handled)
                {
                    //Logger.SignInHandled();
                    return true;
                }
                else if (ticketContext.Result.Skipped)
                {
                    //Logger.SignInSkipped();
                    return false;
                }
            }

            await Context.SignInAsync(SignInScheme, ticketContext.Principal!, ticketContext.Properties);

            // Default redirect path is the base path
            if (string.IsNullOrEmpty(ticketContext.ReturnUri))
            {
                ticketContext.ReturnUri = "/";
            }

            Response.Redirect(ticketContext.ReturnUri);
            return true;
        }


        /// <inheritdoc />
        protected override string BuildChallengeUrl(AuthenticationProperties properties, string redirectUri)
        {

            var queryStrings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            queryStrings.Add("response_type", "code");
            queryStrings.Add("client_id", Options.ClientId);
            queryStrings.Add("redirect_uri", redirectUri);
            queryStrings.Add("v", Options.ApiVersion);

            AddQueryString(queryStrings, properties, OAuthChallengeProperties.ScopeKey, FormatScope, Options.Scope);

            var state = Options.StateDataFormat.Protect(properties);
            queryStrings.Add("state", state);

            var authorizationEndpoint = QueryHelpers.AddQueryString(Options.AuthorizationEndpoint, queryStrings!);
            return authorizationEndpoint;
        }

        private static void AddQueryString<T>(
            IDictionary<string, string> queryStrings,
            AuthenticationProperties properties,
            string name,
            Func<T, string?> formatter,
            T defaultValue)
        {
            string? value;
            var parameterValue = properties.GetParameter<T>(name);
            if (parameterValue != null)
            {
                value = formatter(parameterValue);
            }
            else if (!properties.Items.TryGetValue(name, out value))
            {
                value = formatter(defaultValue);
            }

            // Remove the parameter from AuthenticationProperties so it won't be serialized into the state
            properties.Items.Remove(name);

            if (value != null)
            {
                queryStrings[name] = value;
            }
        }

        private static void AddQueryString(
            IDictionary<string, string> queryStrings,
            AuthenticationProperties properties,
            string name,
            string? defaultValue = null)
            => AddQueryString(queryStrings, properties, name, x => x, defaultValue);
    }
}
