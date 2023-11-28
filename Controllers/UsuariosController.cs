using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore;
using API_Projeto_Credcitrus.Models;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace API_Projeto_Credcitrus.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {

        private readonly Context _dbContext;

        public UsuariosController(Context dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost]
        [Route("~/api/usuarios/gerarusuario/")]
        public async Task<ActionResult<ReturnResultModel>> PostUsuario(DadosUsuarioNovo usuario)
        {
            UsuariosModel usuarioModel = await _dbContext.UsuariosSet.FirstOrDefaultAsync(x => x.email == usuario.email);
            if (usuarioModel != null)
            {
                return new ReturnResultModel { response = HttpStatusCode.BadRequest, mensagem = "Já existe um usuário cadastrado com este endereço de email" };
            }
            else
            {
                var encryptedPass = GerarHashMd5(usuario.senha);

                usuarioModel = new UsuariosModel
                {
                    email = usuario.email,
                    senha = encryptedPass,
                    nome_completo = usuario.nome_completo,
                    supervisor = usuario.supervisor
                };

                _dbContext.UsuariosSet.Add(usuarioModel);
                try
                {
                    await _dbContext.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    string msg = ex.Message;
                }

                return new ReturnResultModel { response = HttpStatusCode.OK, mensagem = "Usuário criado com sucesso!" };
            }
        }

        [HttpGet]
        [Route("~/api/usuarios/getColaboradores/")]
        public async Task<ActionResult<ReturnResultModel>> GetColaboradores()
        {
            var usuarioModel = await _dbContext.UsuariosSet.Select(x => new { x.identificador_usuario, x.nome_completo }).ToListAsync();

            if (usuarioModel != null)
            {
                return new ReturnResultModel { response = HttpStatusCode.OK, mensagem = "", dados_extras = usuarioModel };
            }
            else
            {
                return new ReturnResultModel { response = HttpStatusCode.BadRequest, mensagem = "Nenhum usuário encontrado!" };
            }
        }

        public static string GerarHashMd5(string input)
        {
            MD5 md5Hash = MD5.Create();
            // Converter a String para array de bytes, que é como a biblioteca trabalha.
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Cria-se um StringBuilder para recompôr a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop para formatar cada byte como uma String em hexadecimal
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            return sBuilder.ToString();
        }
    }

    public class DadosUsuarioNovo
    {
        public string nome_completo { get; set; }
        public string senha { get; set; }
        public string email { get; set; }
        public bool supervisor { get; set; } = false;
    }

    public class Colaborador
    {
        public Guid? identificador_usuario { get; set; }
        public string? nome_completo { get; set; }
    }
}
