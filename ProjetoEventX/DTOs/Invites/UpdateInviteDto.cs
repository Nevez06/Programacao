using System.ComponentModel.DataAnnotations;

namespace ProjetoEventX.DTOs.Invites
{
    public class UpdateInviteDto
    {
        [StringLength(50, ErrorMessage = "Status muito longo.")]
        public string? Status { get; set; }

        public bool? CheckInRealizado { get; set; }
        public int? TemplateId { get; set; }
    }
}
