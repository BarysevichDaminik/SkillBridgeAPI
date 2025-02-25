using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;
using Newtonsoft.Json;
using System.Buffers.Text;

namespace SkillBridgeAPI.Services
{
    public static class TokenService
    {
        const string SecretKey = "MIIEvgIBADANBgkqhkiG9w0BAQEFAASCBKgwggSkAgEAAoIBAQDx8faXNCHMjUgFmaNmdjqF2eTCZM+YbDSe1Q3/x6r28d73tpFCZFZjHOL3kK1cKI2cqYp0MlRb5LHlIfSMq8meSIItmItsZQ5CFMCcnixvIWGiTORhER4BP56nkjBECqammt8rdMelgIhM2CLz0Gfu+1LiiW4U8mFJlhjaRD3/nC8s1lh0NsPa+t/VLcScWFDgu71rSMyFGkb60Qcvjh1MkpaiLI9vIBRHVILml5viVZTXYyV0RQuZCQJdC3By+SMznHuHjIx/pMpTOQv+d2C958A7ghFXFpjS9lkBAUGzND+teIjqvzOoLY37huLiW99aYW+cnnBatax/R0yq2Q/ZAgMBAAECggEBAMZVr+urlLl2Z8APfl+UM81eHaUttvAgY9KAnQU8zO26wSkXxGRElHyTRYvkUfjmVZBDe3hdecIK67oF588ZVCIpFm4CSukEvnd1Q6AgGhTPrJW7qsvXmF01pf2AXYipoouZEbEx/ieyAgncdGqiQVErPXrvZgpN12rXXHAw2RIbgo+JOy6TdhNk4HmQqKB/0S73NftvmermyKLzCbd/aeQZobXQrTM6ocHZ0gXDLal0iRQy0eFRTltKKlPzpGpkFq857tFtVkQHPlbbuNXFAcN3GfIA/FZxqnJqU9ZFrzHI142OlffwBGEUPm7vWZNutuMV5ADTgZBp952wHrpii/ECgYEA/AaEA6T6Vq8D0fakWRP54S2Y78eJVRsOWU4rtmF8Ts3BOtmpMS+fz7j6OBq/Dde6Ee5YJ3c1TEERAPEQ5N1FhEPoxm+p2r6mYau4furf3eSWGV9mDgJpaV4/dMzn+gwVSIkkqw/zr5Igj+iGKLIqVv0T+ouzHGB1+uMZ8XIL//MCgYEA9cLAS/dnGXupWHqizEv25G5r4FNEdp7vkZq/5tYRzM0jojHn2/OkOuPg73Ec3riUhe/7PQVC6DTMxNZ6jZOQgamwtIZIU2YC6jKdM0alVQaGq8qKdO7xEv+VrXCvOI889cb1rCb4SYvl68e5koxn/wacu9fpd8EWxWCYfiXKsAMCgYEA68wY1eQUiOfklhzCdcl34JOt5KH3PtY6nZnC0jfxezWNFceyQh/B0TLLgZScrpHpOH+coQgqqLaz9wKVANx5/x8eehLdg5keyIFG9BBC9jO5r/GO5YqiH4CbtGdGn6+QdjZCRX5+TAVXS+2NICRZ8tuERsVQBjvGBr9WdY1z5rsCgYAR/BqOdKB64O3Xp8HaKYT72ojSdcWA2Mi3YxfAENJkpm6BJB3PntjZ5mtDmod+VQupcZJ1OLlYvORvUzLMwYvFsWFZFKqeT8zOzr1qTzUyL7QTRlMzk3jY5xNRCfoIrZLMea7o1kE9QJum0YrnCpdhtl4p8PcI6Hx+HT+Lm8BleQKBgGwerCJNv1rmmPsOJCAHk03uXGCmHiVgow8tFRbT0or2Axd45qr3knLk9QAxfaTWzpm1p8nHAI4TmYUvWau/KCRmlenUqOoHmDmjiZEBP+kLo9pf0mQ1GJx2oqLMHVY01piCX4FDaVrFBiijwBT4p+elmB5tpvs74NRiIgrvyKhT";
        private const string Issuer = "SkillBridgeAPI";

        public static string CreateJWTToken(string ulid)
        {
            string header = "{\"alg\":\"RsaSha512\",\"typ\":\"JWT\"}";
            string payload = JsonConvert.SerializeObject(new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, ulid),
                new(JwtRegisteredClaimNames.Jti, Ulid.NewUlid().ToString()),
                new(JwtRegisteredClaimNames.Iat,
                    DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                    ClaimValueTypes.Integer64),
                new(JwtRegisteredClaimNames.Exp,
                    DateTimeOffset.UtcNow.AddMinutes(1)
                    .ToString(),
                    ClaimValueTypes.Integer64),
                new(JwtRegisteredClaimNames.Iss, Issuer),
                new("subscription", "0"),
                new("admin", "0")
            });

            string headerBase64 = Base64UrlEncoder.Encode(Encoding.UTF8.GetBytes(header));
            string payloadBase64 = Base64UrlEncoder.Encode(Encoding.UTF8.GetBytes(payload));
            string headerAndPayload = $"{headerBase64}.{payloadBase64}";
            byte[] headerAndPayloadHashed = SHA3_512.HashData(Encoding.UTF8.GetBytes(headerAndPayload));

            using var rsa = RSA.Create();
            rsa.ImportPkcs8PrivateKey(Convert.FromBase64String(SecretKey), out _);

            byte[] signature = rsa.SignData(headerAndPayloadHashed, HashAlgorithmName.SHA3_512, RSASignaturePadding.Pkcs1);

            string signatureBase64 = Base64UrlEncoder.Encode(signature);

            return $"{headerAndPayload}.{signatureBase64}";
        }
        public static string CreateRefreshToken() => Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(32));
    }
}