namespace ProjetoEventX.DTOs.Feed
{
    public class ToggleLikeDto
    {
        // null => alterna estado atual; true/false => forca estado.
        public bool? Curtir { get; set; }
    }
}
