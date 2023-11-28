
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata;

namespace API_Projeto_Credcitrus.Models
{
    public class Context : DbContext
    {
        public Context(DbContextOptions<Context> options) : base(options)
        {

        }

        public virtual DbSet<UsuariosModel> UsuariosSet { get; set; }
        public virtual DbSet<MetaModel> MetasSet { get; set; }
        public virtual DbSet<EtapaModel> EtapasSet { get; set; }
    }
}