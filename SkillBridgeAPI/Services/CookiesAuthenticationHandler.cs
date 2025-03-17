using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Encodings.Web;
using System.Text;
using SkillBridgeAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace SkillBridgeAPI.Services
{
    public sealed class CookiesAuthenticationHandler : AuthenticationHandler<JwtBearerOptions>
    {
        readonly SkillbridgeContext Context;
        readonly RefreshTokenService refreshTokenService;
        public CookiesAuthenticationHandler(RefreshTokenService refreshTokenService, SkillbridgeContext Context, IOptionsMonitor<JwtBearerOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
            this.refreshTokenService = refreshTokenService;
            this.Context = Context;
        }

        const string PublicKey = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA8fH2lzQhzI1IBZmjZnY6hdnkwmTPmGw0ntUN/8eq9vHe97aRQmRWYxzi95CtXCiNnKmKdDJUW+Sx5SH0jKvJnkiCLZiLbGUOQhTAnJ4sbyFhokzkYREeAT+ep5IwRAqmpprfK3THpYCITNgi89Bn7vtS4oluFPJhSZYY2kQ9/5wvLNZYdDbD2vrf1S3EnFhQ4Lu9a0jMhRpG+tEHL44dTJKWoiyPbyAUR1SC5peb4lWU12MldEULmQkCXQtwcvkjM5x7h4yMf6TKUzkL/ndgvefAO4IRVxaY0vZZAQFBszQ/rXiI6r8zqC2N+4bi4lvfWmFvnJ5wWrWsf0dMqtkP2QIDAQAB";

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Cookies.TryGetValue("jwtToken", out string? jwtToken))
            {
                return AuthenticateResult.Fail("There is no jwtToken in cookies");
            }

            if (!ValidateToken(jwtToken))
            {
                if (!Request.Cookies.TryGetValue("refreshToken", out string? refreshToken))
                {
                    return AuthenticateResult.Fail("There is no refreshToken in cookies");
                }

                RefreshToken? token = await Context.RefreshTokens.FirstOrDefaultAsync(r => r.Token == refreshToken && r.ExpiredAt > DateTimeOffset.UtcNow);
                if (token is not null)
                {
                    var tokens = await refreshTokenService.RefreshToken(Request);
                    Response.Cookies.Append("jwtToken", tokens[0], new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.None
                    });

                    Response.Cookies.Append("refreshToken", tokens[1], new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.None
                    });
                }
                else
                {
                    return AuthenticateResult.Fail("Invalid JWT Token");
                }
            }

            try
            {
                string[] parts = jwtToken.Split('.');
                string payload = Base64UrlEncoder.Decode(parts[1]);
                var anonymousClaims = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(payload);

                if (anonymousClaims == null)
                {
                    return AuthenticateResult.Fail("Invalid Payload");
                }

                List<Claim> claims = anonymousClaims.ConvertAll(c => new Claim(c["Type"].ToString()!, c["Value"].ToString()!, c["ValueType"].ToString(), c["Issuer"].ToString(), c["OriginalIssuer"].ToString()));

                var identity = new ClaimsIdentity(claims, Scheme.Name);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, Scheme.Name);

                return AuthenticateResult.Success(ticket);
            }
            catch (JsonException ex)
            {
                return AuthenticateResult.Fail($"JSON Deserialization failed after validation: {ex.Message}");
            }
            catch (Exception ex)
            {
                return AuthenticateResult.Fail($"Authentication failed after validation: {ex.Message}");
            }
        }
        public static bool ValidateToken(string token)
        {
            string[] parts = token.Split('.');
            if (parts.Length != 3)
            {
                return false;
            }

            string header = Base64UrlEncoder.Decode(parts[0]);
            string payload = Base64UrlEncoder.Decode(parts[1]);
            string signature = parts[2];

            string headerAndPayload = $"{parts[0]}.{parts[1]}";
            byte[] headerAndPayloadHashed = SHA3_512.HashData(Encoding.UTF8.GetBytes(headerAndPayload));

            using var rsa = RSA.Create();
            rsa.ImportSubjectPublicKeyInfo(Convert.FromBase64String(PublicKey), out _);

            if (!rsa.VerifyData(headerAndPayloadHashed, Base64UrlEncoder.DecodeBytes(signature), HashAlgorithmName.SHA3_512, RSASignaturePadding.Pkcs1))
            {
                return false;
            }

            try
            {
                var anonymousClaims = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(payload);
                if (anonymousClaims == null)
                {
                    return false;
                }

                List<Claim> claims = anonymousClaims.ConvertAll(c => new Claim(c["Type"].ToString()!, c["Value"].ToString()!, c["ValueType"].ToString(), c["Issuer"].ToString(), c["OriginalIssuer"].ToString()));

                var expClaim = claims.FirstOrDefault(c => c.Type == "exp");
                var issClaim = claims.FirstOrDefault(c => c.Type == "iss");

                if (expClaim == null || issClaim == null)
                {
                    return false;
                }

                if (issClaim.Value != "SkillBridgeAPI")
                {
                    return false;
                }

                if (DateTimeOffset.Parse(expClaim.Value) < DateTimeOffset.UtcNow)
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
    }
}
