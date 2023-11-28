using API_Projeto_Credcitrus.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Net;
using Microsoft.EntityFrameworkCore;

namespace API_Projeto_Credcitrus.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MetasController : ControllerBase
    {
        private readonly Context _dbContext;

        public MetasController(Context dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        [Route("~/api/sprint/getMetas")]
        public async Task<ActionResult<ReturnResultModel>> GetMeta([Required]string identificador_usuario, [Required]bool supervisor)
        {
            try
            {
                //FUNÇÃO QUE FAZ APARECER SOMENTE AS METAS QUE O USUÁRIO FAZ PARTE
                //var result = !supervisor ? _dbContext.MetasSet.FromSqlRaw(@"SELECT DISTINCT m.*
                //                                            FROM seguranca.usuarios u
                //                                            LEFT JOIN sprint.etapas_meta em ON em.identificador_responsavel = u.identificador_usuario
                //                                            LEFT JOIN sprint.metas m ON m.identificador_meta = em.identificador_meta
                //                                            WHERE identificador_usuario = {0}
                //                                            AND m.identificador_meta IS NOT NULL", identificador_usuario)
                //                         : _dbContext.MetasSet.FromSqlRaw(@"SELECT * FROM sprint.metas");

                var result = _dbContext.MetasSet.FromSqlRaw(@"SELECT * FROM sprint.metas");

                return new ReturnResultModel { response = HttpStatusCode.OK, mensagem = "Metas", dados_extras = result };
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return new ReturnResultModel { response = HttpStatusCode.BadRequest, mensagem = ex.Message };
            }
        }

        [HttpPut]
        [Route("~/api/sprint/novaMeta/")]
        public async Task<ActionResult<ReturnResultModel>> PutMeta([Required]DadosMeta meta)
        {
            MetaModel metaModel = new MetaModel();
            metaModel = new()
            {
                titulo_meta = meta.titulo_meta,
                descricao = meta.descricao,
                data_inicio = meta.data_inicio,
                data_conclusao_prevista = meta.data_conclusao_prevista,
                data_conclusao = meta.data_conclusao,
                status = meta.status,
                progresso = meta.progresso
            };

            try
            {
                var result = await _dbContext.MetasSet.AddAsync(metaModel);
                await _dbContext.SaveChangesAsync();
                return new ReturnResultModel { response = HttpStatusCode.OK, mensagem = "Meta criada com sucesso" };
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return new ReturnResultModel { response = HttpStatusCode.BadRequest, mensagem = ex.Message };
            }
        }

        [HttpPost]
        [Route("~/api/sprint/editMeta/{identificador_meta}")]
        public async Task<ActionResult<ReturnResultModel>> EditMeta([Required]DadosEditMeta meta, string identificador_meta)
        {
            Guid guid = new Guid(identificador_meta);

            MetaModel meta_existente = await _dbContext.MetasSet.FirstOrDefaultAsync(x => x.identificador_meta == guid);

            if(meta_existente != null)
            {
                meta_existente.titulo_meta = meta_existente.titulo_meta == meta.titulo_meta ? meta_existente.titulo_meta : meta.titulo_meta;
                meta_existente.descricao = meta_existente.descricao == meta.descricao ? meta_existente.descricao : meta.descricao;
                meta_existente.data_inicio = meta_existente.data_inicio == meta.data_inicio ? meta_existente.data_inicio : meta.data_inicio;
                meta_existente.data_conclusao_prevista = meta_existente.data_conclusao_prevista == meta.data_conclusao_prevista ? meta_existente.data_conclusao_prevista : meta.data_conclusao_prevista;
            }

            try
            {
                await _dbContext.SaveChangesAsync();
                return new ReturnResultModel { response = HttpStatusCode.OK, mensagem = "Meta editada com sucesso" };
            }
            catch (Exception ex)
            {
                return new ReturnResultModel { response = HttpStatusCode.BadRequest, mensagem = ex.Message };
            }
        }

        [HttpDelete]
        [Route("~/api/sprint/deleteMeta/{identificador_meta}")]
        public async Task<ActionResult<ReturnResultModel>> DeleteMeta(string identificador_meta)
        {
            Guid guid = new Guid(identificador_meta);
            try
            {
                await _dbContext.MetasSet.Where(x => x.identificador_meta == guid).ExecuteDeleteAsync();
                await _dbContext.EtapasSet.Where(x => x.identificador_meta == guid).ExecuteDeleteAsync();

                await _dbContext.SaveChangesAsync();
                return new ReturnResultModel { response = HttpStatusCode.OK, mensagem = "Meta excluída com sucesso" };
            }
            catch (Exception ex)
            {
                return new ReturnResultModel { response = HttpStatusCode.BadRequest, mensagem = ex.Message };
            }
        }
    }

    public class DadosMeta
    {
        public string titulo_meta { get; set; }
        public string? descricao { get; set; }
        public DateTime? data_inicio { get; set; } = DateTime.Now;
        public DateTime? data_conclusao_prevista { get; set; }
        public DateTime? data_conclusao { get; set; }
        public string? status { get; set; } = "Pendente";
        public int? progresso { get; set; } = 0;
    }

    public class DadosEditMeta
    {
        public string? titulo_meta { get; set; }
        public string? descricao { get; set; }
        public DateTime? data_inicio { get; set; } = DateTime.Now;
        public DateTime? data_conclusao_prevista { get; set; }
    }
}
