

namespace DinoHub.MVVM.Models
{
    public class Dinosaurio
    {
        public string Nombre { get; set; } = string.Empty;
        public string Imagen { get; set; } = string.Empty;
        public bool Carnivoro { get; set; }
        public int Tamano { get; set; }
        public long Id { get; set; }
    }
}
