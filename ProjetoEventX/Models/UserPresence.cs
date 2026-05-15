using System.ComponentModel.DataAnnotations;

namespace ProjetoEventX.Models
{
    public class UserPresence
    {
        [Key]
        public int UserId { get; set; }

        public ApplicationUser? User { get; set; }

        public bool IsOnline { get; set; }

        public int ActiveConnections { get; set; }

        public DateTime LastHeartbeatAt { get; set; } = DateTime.UtcNow;

        public DateTime LastSeenAt { get; set; } = DateTime.UtcNow;

        [StringLength(200)]
        public string? LastConnectionId { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
