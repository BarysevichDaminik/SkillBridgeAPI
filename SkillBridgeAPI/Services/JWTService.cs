using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System;
using System.Security.Cryptography;

namespace SkillBridgeAPI.Services
{
    public static class JWTService
    {
        private const string PublicKey = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAqsxgmLbxRvkftBbUgfma QoUj48ZEgFfqguncV9qVdjo2H0to6MXUtH42mqdGxSKO0evETRdqLfzX1CZpr9mm I8jDWoP7DYm9DCR7cSe+YnPqr2a9LLQh/slA6WCHBq/kN5u88r3tc4q63g0B9Zaw xSO8r/YCG+V8rBzIHlv2sM5YFrWpO6v6nGuk5xRvCMKfdeDTG/Tzrs/qMr7veRUJ MrD7edNDot5k+9n+9Ucil+K0xQPPyGozuS1YtPQkTLTx7qRveV+qrrVAvKqso3jO +iJRR6bjzsFWN5UI9KT1Wiu7ifwC6RSNiZg50lg8003Vj6uQqLe3svWgq1CL2T+0 lQIDAQAB";
        private const string SecretKey = "MIIEvAIBADANBgkqhkiG9w0BAQEFAASCBKYwggSiAgEAAoIBAQCqzGCYtvFG+R+0FtSB+ZpChSPjxkSAV+qC6dxX2pV2OjYfS2joxdS0fjaap0bFIo7R68RNF2ot/NfUJmmv2aYjyMNag/sNib0MJHtxJ75ic+qvZr0stCH+yUDpYIcGr+Q3m7zyve1zirreDQH1lrDFI7yv9gIb5XysHMgeW/awzlgWtak7q/qca6TnFG8Iwp914NMb9POuz+oyvu95FQkysPt500Oi3mT72f71RyKX4rTFA8/IajO5LVi09CRMtPHupG95X6qutUC8qqyjeM76IlFHpuPOwVY3lQj0pPVaK7uJ/ALpFI2JmDnSWDzTTdWPq5Cot7ey9aCrUIvZP7SVAgMBAAECggEAC48DldeCR5Z8yJq87JjgPCfiJbwuuZgkNOeFCRBatLgRc+0OCfwNaN5GbWaW+aiyN/mifMHHZppUXt/vShPK1GQgowOzM0ItiuCogUq6bdOWFSMOTV6uvZ06voXQ+CkMzZ3wBmYdEjMtcfE5FI41+KQc/7QrMLPHS/yHUdVRF2ZF1xLH9KLIscNnJUxmr1OESYfOlO1kEaSOcfZSTQga7NOPMXBVoouxqTDQJstryvJ5T+oZpgV6btR+ByHWSmTy4PGsmlWdddmDx1aKGMnPcajAh93cuX9/V8DJiCtpLyh/sbT3KCocdH/PqVleX6oFDU06NhQJzTNvcJ2GpI/LAQKBgQDWiu5mqeAItmEZIYaIhuhjmFdPWn5L8+x6yTTbk0CPkIPXTvgsg3xx+uJYWIcI2F3onoAUPtXy/8bbYko6sIig98bMIPNGizrzlR3K84UaEO3gq37q82tvCeeKyhozyx7+3z0Wvd5UXbShb4exc4GXGLFNLZopqAk50cb8v49PgQKBgQDLzXxz3hvKsaxou702g5KMTkdydHTRZvwE4N7m7Dkfd+qMyTDviILY/PUSd2khR1RMGp6d98eKRWzKDhv0FSQ1w9GNcxjQaiX23vBWdz84dSzjA8e8teGiYWDN6NcUpLkNAITTOd3xSx6kxuthl6EazcXpDcJaki/3D70lbaWvFQKBgCCCnwuplExdrqsLPIK1xsNI03ov4VGLHfuhP8RCNRdMM95NlifQqOvws5nlmFjLyLc2RXxL5UnUXoLiCxOHqryRr2tBVvwKnx1ILGKTski35gQUmL/rsQz7eD280GmmzwSaOXyXuvgX1wZbizllom6ODFgAoArN7s/3LOsh8AqBAoGAIZwpwhIHE05p2HvdoHfiWzEtpzp9aWtKdKOBHyQl54KnM8CaSWaB7bcJ05nNxKc2x7Y8ImESj0MTxd69zWsPJa69iE5K8VQQHr51dE0GKBFq7aVZ800rgNP+WvbjQYxI2FQVk6AfcgOpchM8DkQrVXhKAz05qCYjsuLtlpaWlGECgYBMhj2H1Kc7KCLQGUtnNKfRKfrgq8RJz0obYeVW3D+mxEafFvdWwN0ctMDVcl10uqc8WwqKMdyI+nwjFABxfqLUDc8zV105msTrs2SqP4SO44pemt8326aobxV1Boy5FiSmaQT8/nZWrBR673ZVggcVHHeJrdcIg9TimbOQmJvIeA==";
        private const string Issuer = "SkillBridgeAPI";
        private const string Audience = "Frontend";

        public static string CreateToken(long ulid)
        {
            var claims = new List<Claim>
      {
        new(JwtRegisteredClaimNames.Sub,
          ulid.ToString()),
        new(JwtRegisteredClaimNames.Jti,
          Ulid.NewUlid().ToString()),
        new(JwtRegisteredClaimNames.Iat,
          DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
          ClaimValueTypes.Integer64),
        new(JwtRegisteredClaimNames.Exp,
          DateTimeOffset.UtcNow.AddMinutes(1).ToUnixTimeSeconds().ToString(),
          ClaimValueTypes.Integer64),
        new(JwtRegisteredClaimNames.Iss, Issuer),
        new(JwtRegisteredClaimNames.Aud, Audience),
        new("subscription", "0"),
        new("admin", "0")
      };
            var payload = new JwtPayload(claims);

            var rsaPrivateKey = RSA.Create();
            ReadOnlySpan<byte> secretKeyBytes = Encoding.UTF8.GetBytes(SecretKey).AsSpan();
            int bytesRead;
            rsaPrivateKey.ImportPkcs8PrivateKey(secretKeyBytes, out bytesRead);

            var signingCredentials = new SigningCredentials(new RsaSecurityKey(rsaPrivateKey), SecurityAlgorithms.RsaSha512);
            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(payload.Claims),
                SigningCredentials = signingCredentials,
                Issuer = Issuer,
                Audience = Audience,
                Expires = DateTime.UtcNow.AddHours(1)
            };

            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = jwtTokenHandler.CreateJwtSecurityToken(descriptor);

            return jwtTokenHandler.WriteToken(jwtToken);
        }
        public static bool ValidateToken(this string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var rsaPublicKey = RSA.Create();
                rsaPublicKey.ImportSubjectPublicKeyInfo(Convert.FromBase64String(PublicKey), out _);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = Issuer,
                    ValidateAudience = true,
                    ValidAudience = Audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new RsaSecurityKey(rsaPublicKey),
                    ClockSkew = TimeSpan.Zero
                };


                SecurityToken validatedToken;
                tokenHandler.ValidateToken(token, validationParameters, out validatedToken);
            }
            catch (SecurityTokenException)
            {
                return false;
            }
            catch (ArgumentException)
            {
                return false;
            }

            return true;
        }
    }
}