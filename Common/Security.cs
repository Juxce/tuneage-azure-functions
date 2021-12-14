using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace Juxce.Tuneage.Common {
    public static class Security {
        private static readonly IConfigurationManager<OpenIdConnectConfiguration> _configurationManager;
        private static readonly string ISSUER = Environment.GetEnvironmentVariable("Auth0Url", EnvironmentVariableTarget.Process);
        private static readonly string AUDIENCE = Environment.GetEnvironmentVariable("Auth0Audience", EnvironmentVariableTarget.Process);

        static Security()
        {
            var documentRetriever = new HttpDocumentRetriever {RequireHttps = ISSUER.StartsWith("https://")};

            _configurationManager = new ConfigurationManager<OpenIdConnectConfiguration> (
                $"{ISSUER}.well-known/openid-configuration",
                new OpenIdConnectConfigurationRetriever(),
                documentRetriever
            );
        }

        public static async Task<ClaimsPrincipal> ValidateTokenAsync(AuthenticationHeaderValue value) {
            if(value?.Scheme != "Bearer")
                return null;

            var config = await _configurationManager.GetConfigurationAsync(CancellationToken.None);

            var validationParameter = new TokenValidationParameters {
                RequireSignedTokens = true,
                ValidAudience = AUDIENCE,
                ValidateAudience = true,
                ValidIssuer = ISSUER,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                IssuerSigningKeys = config.SigningKeys
            };

            ClaimsPrincipal result = null;
            var tries = 0;

            while (result == null && tries <= 1) {
                try {
                    var handler = new JwtSecurityTokenHandler();
                    result = handler.ValidateToken(value.Parameter, validationParameter, out var token);
                } catch (SecurityTokenSignatureKeyNotFoundException) {
                    // This excepion is thrown if the signature key of the JWT could not be found.
                    // This could be the case when the issuer changed its signing keys, so we trigger
                    // a refresh and retry validation.
                    _configurationManager.RequestRefresh();
                    tries++;
                } catch (SecurityTokenException) {
                    return null;
                }
            }

            return result;
        }
    }
}