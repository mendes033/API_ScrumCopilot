using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace API_Projeto_Credcitrus.Models
{
    [Table("metas", Schema = "sprint")]
    public class MetaModel
    {
        [Key]
        public Guid? identificador_meta { get; set; }
        public string titulo_meta { get; set; }
        public string? descricao { get; set; }
        public DateTime? data_inicio { get; set; } = DateTime.Now;
        public DateTime? data_conclusao_prevista { get; set; }
        public DateTime? data_conclusao { get; set; }
        public string? status { get; set; } = "Pendente";
        public decimal? progresso { get; set; } = 0;
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? id_meta { get; set; }
    }
}