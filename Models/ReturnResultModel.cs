using Microsoft.EntityFrameworkCore;
using System.Net;

namespace API_Projeto_Credcitrus.Models
{
    [Keyless]
    public class ReturnResultModel
    {
        public HttpStatusCode response { get; set; }
        public string mensagem { get; set; }
        public object? dados_extras { get; set; }
        //public UsuariosModel model { get; set; }
    }
}
