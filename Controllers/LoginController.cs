using API_Projeto_Credcitrus.Models;
using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using RestSharp;
using System.Net.Mail;
using Newtonsoft.Json;

namespace API_Projeto_Credcitrus.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly Context _dbContext;

        public LoginController(Context dbContext)
        {
            _dbContext = dbContext;
        }

        #region Login e Autenticação
        private ReturnResultModel AuthenticateUser(LoginModel login)
        {
            var hashsenha = GerarHashMd5(login.senha);
            var usuario = _dbContext.UsuariosSet.FirstOrDefault(x => x.email == login.email && x.senha == hashsenha);

            HttpStatusCode statusCode = HttpStatusCode.BadRequest;
            string mensagem = "BadRequest";

            if (usuario == null)
            {
                statusCode = HttpStatusCode.Unauthorized;
                mensagem = "Login e/ou senha errados!";
                return new ReturnResultModel { response = statusCode, mensagem = mensagem };
            }
            else
            {
                var usuario_obj = new
                {
                    usuario.identificador_usuario,
                    usuario.email,
                    usuario.nome_completo,
                    usuario.supervisor
                };

                statusCode = HttpStatusCode.OK;
                mensagem = "Logado com sucesso!";
                return new ReturnResultModel { response = statusCode, mensagem = mensagem, dados_extras = usuario_obj };
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

        [AllowAnonymous]
        [Route("~/api/usuarios/authentication/")]
        [HttpPost]
        //public IActionResult Login([FromForm] string login,[FromForm] string senha)
        public ReturnResultModel Login([FromBody] LoginModel login)
        {
            ReturnResultModel authenticate = AuthenticateUser(login);
            return authenticate;
        }
        #endregion

        #region Recuperação de Senha

        [HttpPost]
        [Route("~/api/usuarios/recuperarsenha_email/{login}")]
        public async Task<ActionResult<ReturnResultModel>> PostRecuperarSenha(string login)
        {
            UsuariosModel usuarioExistente = await _dbContext.UsuariosSet.FirstOrDefaultAsync(x => x.email == login);
            if (usuarioExistente != null)
            {
                string token = codigoRecuperacao();

                MailMessage mail = new MailMessage("joaovitorsousamendes@gmail.com", login);

                mail.From = new MailAddress("joaovitorsousamendes@gmail.com");
                mail.Subject = "Recuperação de Senha";
                string Body = "Digite este código para iniciar o processo de recuperação de senha: " + token;
                mail.Body = Body;

                mail.IsBodyHtml = true;
                SmtpClient smtp = new SmtpClient();
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                smtp.Credentials = new System.Net.NetworkCredential("joaovitorsousamendes@gmail.com", "lnbklrqcertwcicz");

                smtp.EnableSsl = true;

                try
                {
                    smtp.Send(mail);
                    usuarioExistente.recuperar_senha = token;
                    await _dbContext.SaveChangesAsync();

                    return new ReturnResultModel { response = HttpStatusCode.OK, mensagem = "Email de recuperação enviado!", dados_extras = new { token, usuarioExistente.identificador_usuario } };
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return new ReturnResultModel { response = HttpStatusCode.BadRequest, mensagem = ex.Message };
                }
            }
            else
            {
                return new ReturnResultModel { response = HttpStatusCode.BadRequest, mensagem = "Não existe um usuário com este endereço de email!" };
            }
        }

        public static string codigoRecuperacao()
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            var result = new string(
                Enumerable.Repeat(chars, 6)
                          .Select(s => s[random.Next(s.Length)])
                          .ToArray());
            return result;
        }

        [HttpPost]
        [Route("~/api/usuarios/nova_senha/")]
        public async Task<ActionResult<ReturnResultModel>> PostNovaSenha(DadosRecuperacao dados)
        {
            UsuariosModel usuarioExistente = await _dbContext.UsuariosSet.FirstOrDefaultAsync(x => x.identificador_usuario == dados.identificador_usuario);
            if (usuarioExistente != null)
            {
                var hashsenha = GerarHashMd5(dados.nova_senha);
                usuarioExistente.senha = hashsenha;
                usuarioExistente.recuperar_senha = null;

                try
                {
                    await _dbContext.SaveChangesAsync();

                    return new ReturnResultModel { response = HttpStatusCode.OK, mensagem = "Senha alterada com sucesso!" };
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return new ReturnResultModel { response = HttpStatusCode.BadRequest, mensagem = ex.Message };
                }
            }
            else
            {
                return new ReturnResultModel { response = HttpStatusCode.BadRequest, mensagem = "Usuário não encontrado!" };
            }
        }

        [HttpGet]
        [Route("~/api/usuarios/get_token/{identificador_usuario}")]
        public async Task<ActionResult<ReturnResultModel>> GetToken(Guid identificador_usuario)
        {
            string token = _dbContext.UsuariosSet.FirstOrDefault(x => x.identificador_usuario == identificador_usuario).recuperar_senha ?? "";

            if (!String.IsNullOrEmpty(token))
            {
                return new ReturnResultModel { response = HttpStatusCode.OK, mensagem = "Requisição Ok", dados_extras = new { token } };
            }
            else
            {
                return new ReturnResultModel { response = HttpStatusCode.BadRequest, mensagem = "Usuario não encontrado!" };
            }
        }

        #endregion
    }
    public class DadosRecuperacao
    {
        public Guid identificador_usuario { get; set; }
        public string nova_senha { get; set; }
    }
}
