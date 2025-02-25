using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkillBridgeAPI.Models
{
    public partial class RefreshToken
    {
        public long TokenId { get; set; }

        [Required]
        public DateTimeOffset ExpiredAt { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public string Token { get; set; }
    }
}
