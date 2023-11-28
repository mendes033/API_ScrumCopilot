using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_Projeto_Credcitrus.Models
{
    [Table("usuarios", Schema = "seguranca")]
    public class UsuariosModel
    {
        [Key]
        public string email { get; set; }
        public Guid identificador_usuario { get; set; } = Guid.NewGuid();
        public string nome_completo { get; set; }
        public string senha { get; set; }
        public DateTime? data_inclusao { get; set; } = DateTime.Now;
        public string? recuperar_senha { get; set; }
        public bool? supervisor { get; set; }
    }
}
