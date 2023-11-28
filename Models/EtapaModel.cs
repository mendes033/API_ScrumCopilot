using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace API_Projeto_Credcitrus.Models
{
    [Table("etapas_meta", Schema = "sprint")]
    public class EtapaModel
    {
        [Key]
        public Guid? identificador_etapa { get; set; }
        public Guid identificador_meta { get; set; }
        public string titulo_etapa { get; set; }
        public string? descricao { get; set; }
        public Guid? identificador_responsavel { get; set; }
        public DateTime? data_conclusao_prevista { get; set; }
        public DateTime? data_conclusao { get; set; }
        public string? status { get; set; }
        public string? impedimentos { get; set; } = "";
        public int? progresso { get; set; } = 0;
    }
}
