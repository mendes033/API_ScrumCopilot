using API_Projeto_Credcitrus.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace API_Projeto_Credcitrus.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EtapasController : ControllerBase
    {
        private readonly Context _dbContext;

        public EtapasController(Context dbContext)
        {
            _dbContext = dbContext;
        }

        DateTime now = DateTime.Now;

        [HttpGet]
        [Route("~/api/sprint/getEtapas/")]
        public async Task<ActionResult<ReturnResultModel>> GetEtapas([Required]string identificador_meta, [Required]string identificador_usuario, [Required]bool supervisor)
        {
            Guid guid_meta = new Guid(identificador_meta);
            Guid guid_usuario = new Guid(identificador_usuario);

            var etapas_meta = supervisor ? await (from x in _dbContext.EtapasSet
                                                  join u in _dbContext.UsuariosSet on x.identificador_responsavel equals u.identificador_usuario into uGroup
                                                  from u in uGroup.DefaultIfEmpty()
                                                  where x.identificador_meta == guid_meta
                                                  select new Etapa()
                                                  {
                                                      identificador_etapa = x.identificador_etapa,
                                                      identificador_responsavel = x.identificador_responsavel,
                                                      identificador_meta = x.identificador_meta,
                                                      titulo_etapa = x.titulo_etapa,
                                                      descricao = x.descricao,
                                                      status = x.status,
                                                      data_conclusao = x.data_conclusao,
                                                      data_conclusao_prevista = x.data_conclusao_prevista,
                                                      nome_completo = u.nome_completo,
                                                      progresso = x.progresso,
                                                      impedimentos = x.impedimentos
                                                  }).ToListAsync()
                                         : await (from x in _dbContext.EtapasSet
                                                  join u in _dbContext.UsuariosSet on x.identificador_responsavel equals u.identificador_usuario into uGroup
                                                  from u in uGroup.DefaultIfEmpty()
                                                  where x.identificador_meta == guid_meta && x.identificador_responsavel == guid_usuario
                                                  select new Etapa()
                                                  {
                                                      identificador_etapa = x.identificador_etapa,
                                                      identificador_responsavel = x.identificador_responsavel,
                                                      identificador_meta = x.identificador_meta,
                                                      titulo_etapa = x.titulo_etapa,
                                                      descricao = x.descricao,
                                                      status = x.status,
                                                      data_conclusao = x.data_conclusao,
                                                      data_conclusao_prevista = x.data_conclusao_prevista,
                                                      nome_completo = u.nome_completo,
                                                      progresso = x.progresso,
                                                      impedimentos = x.impedimentos
                                                  }).ToListAsync();
            try
            {
                return new ReturnResultModel { response = HttpStatusCode.OK, mensagem = "Etapas", dados_extras = etapas_meta };
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return new ReturnResultModel { response = HttpStatusCode.BadRequest, mensagem = ex.Message };
            }
        }

        [HttpPut]
        [Route("~/api/sprint/novaEtapa/{identificador_meta}")]
        public async Task<ActionResult<ReturnResultModel>> PutEtapa([Required] DadosEtapa etapa, string identificador_meta)
        {
            Guid guid = new Guid(identificador_meta);

            if(guid != Guid.Empty)
            {
                EtapaModel etapaModel = new EtapaModel();
                etapaModel = new()
                {
                    identificador_meta = guid,
                    titulo_etapa = etapa.titulo_etapa,
                    descricao = etapa.descricao,
                    identificador_responsavel = etapa.identificador_responsavel,
                    data_conclusao_prevista = etapa.data_conclusao_prevista,
                    status = etapa.progresso >= 100 ? "FINALIZADO" : etapa.progresso != 0 && etapa.progresso != 100 ? "EM ANDAMENTO" : "PENDENTE",
                    impedimentos = etapa.impedimentos,
                    progresso = etapa.progresso
                };

                var result = await _dbContext.EtapasSet.AddAsync(etapaModel);

                try
                {
                    await _dbContext.SaveChangesAsync();
                    if (Convert.ToBoolean(await AttMeta(guid))) 
                    {
                        return new ReturnResultModel { response = HttpStatusCode.OK, mensagem = "Etapa criada com sucesso" };
                    }
                    else
                    {
                        return new ReturnResultModel { response = HttpStatusCode.BadRequest, mensagem = "" };
                    }
                }
                catch (Exception ex)
                {
                    string msg = ex.Message;
                    return new ReturnResultModel { response = HttpStatusCode.BadRequest, mensagem = ex.Message };
                }
            }
            else
            {
                return new ReturnResultModel { response = HttpStatusCode.NoContent, mensagem = "Identificador não está em um formato válido de Guid" };
            }
        }

        [HttpPost]
        [Route("~/api/sprint/editEtapa/{identificador_etapa}")]
        public async Task<ActionResult<ReturnResultModel>> EditEtapa([Required] DadosEtapa etapa, string identificador_etapa)
        {
            Guid guid_etapa = new Guid(identificador_etapa);

            var etapa_existente = await _dbContext.EtapasSet.FirstOrDefaultAsync(x => x.identificador_etapa == guid_etapa);

            if (etapa_existente != null)
            {

                etapa_existente.titulo_etapa = etapa.titulo_etapa;
                etapa_existente.data_conclusao_prevista = etapa.data_conclusao_prevista;
                etapa_existente.descricao = etapa.descricao;
                etapa_existente.identificador_responsavel = etapa.identificador_responsavel;
                etapa_existente.status = etapa.progresso >= 100 ? "FINALIZADO" : etapa.progresso != 0 && etapa.progresso != 100 ? "EM ANDAMENTO" : "PENDENTE" ;
                etapa_existente.data_conclusao = etapa.progresso >= 100 ? now : null;
                etapa_existente.impedimentos = etapa.impedimentos;
                etapa_existente.progresso = etapa.progresso;

                try
                {
                    await _dbContext.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return new ReturnResultModel { response = HttpStatusCode.BadRequest, mensagem = ex.Message };
                }
                if (Convert.ToBoolean(await AttMeta(etapa_existente.identificador_meta)))
                {
                    try
                    {
                        await _dbContext.SaveChangesAsync();

                        return new ReturnResultModel { response = HttpStatusCode.OK, mensagem = "Etapa atualizada com sucesso" };
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        return new ReturnResultModel { response = HttpStatusCode.BadRequest, mensagem = ex.Message };
                    }
                }
                else
                {
                    return new ReturnResultModel { response = HttpStatusCode.BadRequest, mensagem = "" };
                }
            }
            else
            {
                return new ReturnResultModel { response = HttpStatusCode.NoContent, mensagem = "Etapa não encontrada" };
            }
        }

        [HttpDelete]
        [Route("~/api/sprint/deleteEtapa/{identificador_etapa}")]
        public async Task<ActionResult<ReturnResultModel>> DeleteEtapa(string identificador_etapa)
        {
            Guid guid = new Guid(identificador_etapa);

            var identificador_meta = _dbContext.EtapasSet.Where(x => x.identificador_etapa == guid).Select(x => x.identificador_meta).ToList()[0].ToString();

            Guid guid_meta = new Guid(identificador_meta);
            try
            {
                await _dbContext.EtapasSet.Where(x => x.identificador_etapa == guid).ExecuteDeleteAsync();
                await _dbContext.SaveChangesAsync();

                if (Convert.ToBoolean(await AttMeta(guid_meta)))
                {
                    return new ReturnResultModel { response = HttpStatusCode.OK, mensagem = "Etapa excluída com sucesso" };
                }
                else
                {
                    return new ReturnResultModel { response = HttpStatusCode.BadRequest, mensagem = "" };
                }
            }
            catch (Exception ex)
            {
                return new ReturnResultModel { response = HttpStatusCode.BadRequest, mensagem = ex.Message };
            }
        }

        private async Task<bool> AttMeta(Guid identificador_meta)
        {
            int i = 0;
            do
            {
                try
                {
                    var meta = await _dbContext.MetasSet.FirstOrDefaultAsync(x => x.identificador_meta == identificador_meta);
                    if (meta != null)
                    {
                        var etapas_meta = await _dbContext.EtapasSet.Where(x => x.identificador_meta == meta.identificador_meta).ToListAsync();

                        int quantidade_etapas = etapas_meta.Count();
                        int quantidade_etapas_finalizadas = etapas_meta.Where(x => x.progresso >= 100).Count();

                        string status = quantidade_etapas != 0 && quantidade_etapas == quantidade_etapas_finalizadas ? "FINALIZADO" :
                                        etapas_meta.Any(x => x.progresso != 0 || x.progresso < 100) ? "EM ANDAMENTO" : "PENDENTE";

                        decimal progresso_parcial = 0;
                        foreach (var item in etapas_meta)
                        {
                            if(item.progresso != 0)
                            {
                                progresso_parcial += (decimal)item.progresso / 100;
                            }
                        }


                        decimal porcentagem_progresso = progresso_parcial != 0 ? (progresso_parcial / quantidade_etapas) * 100 : 0;

                        if (meta.status != status) meta.status = status;
                        if (meta.progresso != porcentagem_progresso) meta.progresso = porcentagem_progresso;
                        if (quantidade_etapas == quantidade_etapas_finalizadas) meta.data_conclusao = now;

                        await _dbContext.SaveChangesAsync();
                        i = 10; //AQUI SETA 10 PARA DIZER QUE DEU CERTO
                    }
                }
                catch
                {
                    i++;
                }
            }
            while (i < 5); //AQUI ESTÁ A QUANTIDADE DE TENTATIVAS DE ATUALIZAÇÃO DA META QUE O SISTEMA IRÁ FAZER
            if(i == 10)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public class DadosEtapa
    {
        public string titulo_etapa { get; set; }
        public string? descricao { get; set; }
        public Guid? identificador_responsavel { get; set; }
        public DateTime? data_conclusao_prevista { get; set; }
        public string? impedimentos { get; set; } = "";
        public int? progresso { get; set; } = 0;
    }

    public class Etapa
    {
        public Guid? identificador_etapa { get; set; }
        public Guid identificador_meta { get; set; }
        public string titulo_etapa { get; set; }
        public string? descricao { get; set; }
        public Guid? identificador_responsavel { get; set; }
        public DateTime? data_conclusao_prevista { get; set; }
        public DateTime? data_conclusao { get; set; }
        public string? status { get; set; }
        public string? nome_completo { get; set; }
        public string? impedimentos { get; set; } = "";
        public decimal? progresso { get; set; } = 0;
    }
}
