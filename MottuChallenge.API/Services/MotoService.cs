using MottuChallenge.API.Models;
using MottuChallenge.API.DTOs;
using MottuChallenge.API.Repositories;
using MottuChallenge.API.Models.ML;
using MottuChallenge.API.Enums;
using MottuChallenge.API.Models.Common;

namespace MottuChallenge.API.Services
{
    public class MotoService
    {
        private readonly MotoRepository _motoRepository;
        private readonly FuncionarioRepository _funcionarioRepository;
        private readonly PatioRepository _patioRepository;
        private readonly MotoPredictionService _predictionService;

        public MotoService(MotoRepository motoRepository, FuncionarioRepository funcionarioRepository, 
                         PatioRepository patioRepository, MotoPredictionService predictionService)
        {
            _motoRepository = motoRepository;
            _funcionarioRepository = funcionarioRepository;
            _patioRepository = patioRepository;
            _predictionService = predictionService;
        }

        public async Task<ServiceResponse<PagedResponse<Moto>>> GetMotosPagedAsync(PaginationParams paginationParams, StatusMoto? status = null, SetorMoto? setor = null)
        {
            try
            {
                List<Moto> motos;
                int totalCount;

                if (status.HasValue)
                {
                    var result = await _motoRepository.GetByStatusPagedAsync(status.Value, paginationParams.PageNumber, paginationParams.PageSize);
                    motos = result.Motos;
                    totalCount = result.TotalCount;
                }
                else
                {
                    var result = await _motoRepository.GetAllPagedAsync(paginationParams.PageNumber, paginationParams.PageSize);
                    motos = result.Motos;
                    totalCount = result.TotalCount;
                }

                if (setor.HasValue)
                {
                    motos = motos.Where(m => m.Setor == setor.Value).ToList();
                    totalCount = motos.Count;
                }

                var pagedResponse = new PagedResponse<Moto>(motos, paginationParams.PageNumber, paginationParams.PageSize, totalCount, new List<Link>());
                return ServiceResponse<PagedResponse<Moto>>.Ok(pagedResponse, "Motos recuperadas com sucesso");
            }
            catch (Exception ex)
            {
                return ServiceResponse<PagedResponse<Moto>>.Error($"Erro ao buscar motos: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<PagedResponse<Moto>>> GetMotosPorPatioPagedAsync(string nomePatio, PaginationParams paginationParams)
        {
            try
            {
                var result = await _motoRepository.GetByPatioPagedAsync(nomePatio, paginationParams.PageNumber, paginationParams.PageSize);
                var pagedResponse = new PagedResponse<Moto>(result.Motos, paginationParams.PageNumber, paginationParams.PageSize, result.TotalCount, new List<Link>());
                return ServiceResponse<PagedResponse<Moto>>.Ok(pagedResponse, $"Motos do pátio {nomePatio} recuperadas com sucesso");
            }
            catch (Exception ex)
            {
                return ServiceResponse<PagedResponse<Moto>>.Error($"Erro ao buscar motos do pátio: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<List<Moto>>> GetMotosAsync(StatusMoto? status = null, SetorMoto? setor = null)
        {
            try
            {
                List<Moto> motos;

                if (status.HasValue)
                    motos = await _motoRepository.GetByStatusAsync(status.Value);
                else
                    motos = await _motoRepository.GetAllAsync();

                if (setor.HasValue)
                    motos = motos.Where(m => m.Setor == setor.Value).ToList();

                return ServiceResponse<List<Moto>>.Ok(motos, "Motos recuperadas com sucesso");
            }
            catch (Exception ex)
            {
                return ServiceResponse<List<Moto>>.Error($"Erro ao buscar motos: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<Moto>> GetMotoAsync(string placa)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(placa))
                    return ServiceResponse<Moto>.Error("Placa é obrigatória");

                var moto = await _motoRepository.GetByIdAsync(placa);
                return moto is null 
                    ? ServiceResponse<Moto>.NotFound("Moto")
                    : ServiceResponse<Moto>.Ok(moto, "Moto encontrada com sucesso");
            }
            catch (Exception ex)
            {
                return ServiceResponse<Moto>.Error($"Erro ao buscar moto: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<Moto>> CreateMotoAsync(MotoDto motoDto)
        {
            try
            {
                if (await _motoRepository.ExistsAsync(motoDto.Placa))
                    return ServiceResponse<Moto>.Error("Placa já cadastrada");

                var funcionario = await _funcionarioRepository.GetByIdAsync(motoDto.UsuarioFuncionario);
                if (funcionario is null)
                    return ServiceResponse<Moto>.NotFound("Funcionário");

                if (!funcionario.PertenceAoPatio(motoDto.NomePatio))
                    return ServiceResponse<Moto>.Error("Funcionário não pertence a este pátio");

                var patio = await _patioRepository.GetByIdAsync(motoDto.NomePatio);
                if (patio is null)
                    return ServiceResponse<Moto>.NotFound("Pátio");

                bool ocupaVaga = motoDto.Status == StatusMoto.Disponivel || motoDto.Status == StatusMoto.Manutencao;
                if (ocupaVaga && !patio.TemVagaDisponivel())
                    return ServiceResponse<Moto>.Error("Não há vagas disponíveis no pátio");

                var moto = new Moto
                {
                    Placa = motoDto.Placa.Trim().ToUpper(),
                    Modelo = motoDto.Modelo,
                    Status = motoDto.Status,
                    Setor = motoDto.Setor,
                    NomePatio = motoDto.NomePatio.Trim(),
                    UsuarioFuncionario = motoDto.UsuarioFuncionario.Trim(),
                    Quilometragem = motoDto.Quilometragem,
                    DataUltimaRevisao = motoDto.DataUltimaRevisao,
                    QuantidadeRevisoes = motoDto.DataUltimaRevisao.HasValue ? 1 : 0,
                    DataCriacao = DateTime.Now,
                    DataAtualizacao = DateTime.Now
                };

                var prediction = _predictionService.PredictManutencao(moto);
                moto.PrecisaManutencao = prediction.PrecisaManutencao ? 1 : 0;
                moto.ProbabilidadeManutencao = prediction.Probability;

                if (ocupaVaga)
                {
                    patio.OcuparVaga();
                    await _patioRepository.UpdateAsync(patio);
                }

                await _motoRepository.AddAsync(moto);
                return ServiceResponse<Moto>.Ok(moto, "Moto criada com sucesso!");
            }
            catch (Exception ex)
            {
                return ServiceResponse<Moto>.Error($"Erro ao criar moto: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<Moto>> UpdateMotoAsync(string placa, MotoDto motoDto)
        {
            try
            {
                var motoExistente = await _motoRepository.GetByIdAsync(placa);
                if (motoExistente is null)
                    return ServiceResponse<Moto>.NotFound("Moto");

                var funcionario = await _funcionarioRepository.GetByIdAsync(motoDto.UsuarioFuncionario);
                if (funcionario is null)
                    return ServiceResponse<Moto>.NotFound("Funcionário");

                if (!funcionario.PertenceAoPatio(motoDto.NomePatio))
                    return ServiceResponse<Moto>.Error("Funcionário não pertence a este pátio");

                var novoPatio = await _patioRepository.GetByIdAsync(motoDto.NomePatio);
                if (novoPatio is null)
                    return ServiceResponse<Moto>.NotFound("Novo pátio");

                var resultadoGerenciamento = await GerenciarVagasDuranteAtualizacao(motoExistente, motoDto, novoPatio);
                if (!resultadoGerenciamento.success)
                    return ServiceResponse<Moto>.Error(resultadoGerenciamento.message);

                motoExistente.Modelo = motoDto.Modelo;
                motoExistente.Status = motoDto.Status;
                motoExistente.Setor = motoDto.Setor;
                motoExistente.NomePatio = motoDto.NomePatio.Trim();
                motoExistente.UsuarioFuncionario = motoDto.UsuarioFuncionario.Trim();
                motoExistente.Quilometragem = motoDto.Quilometragem;
                
                if (motoDto.DataUltimaRevisao.HasValue && motoDto.DataUltimaRevisao != motoExistente.DataUltimaRevisao)
                {
                    motoExistente.QuantidadeRevisoes++;
                }
                
                motoExistente.DataUltimaRevisao = motoDto.DataUltimaRevisao;
                motoExistente.DataAtualizacao = DateTime.Now;

                var prediction = _predictionService.PredictManutencao(motoExistente);
                motoExistente.PrecisaManutencao = prediction.PrecisaManutencao ? 1 : 0;
                motoExistente.ProbabilidadeManutencao = prediction.Probability;

                await _motoRepository.UpdateAsync(motoExistente);
                return ServiceResponse<Moto>.Ok(motoExistente, "Moto atualizada com sucesso!");
            }
            catch (Exception ex)
            {
                return ServiceResponse<Moto>.Error($"Erro ao atualizar moto: {ex.Message}");
            }
        }

        private async Task<(bool success, string message)> GerenciarVagasDuranteAtualizacao(Moto moto, MotoDto motoDto, Patio novoPatio)
        {
            var patioAntigo = await _patioRepository.GetByIdAsync(moto.NomePatio);
            if (patioAntigo is null)
                return (false, "Pátio antigo não encontrado");

            bool ocupavaVagaAntes = moto.OcupaVaga;
            bool ocuparaVagaAgora = motoDto.Status == StatusMoto.Disponivel || motoDto.Status == StatusMoto.Manutencao;
            bool mudouPatio = moto.NomePatio != motoDto.NomePatio;

            try
            {
                if (mudouPatio)
                {
                    if (ocupavaVagaAntes)
                    {
                        patioAntigo.LiberarVaga();
                        await _patioRepository.UpdateAsync(patioAntigo);
                    }

                    if (ocuparaVagaAgora)
                    {
                        if (!novoPatio.TemVagaDisponivel())
                            return (false, "Não há vagas disponíveis no novo pátio");
                        
                        novoPatio.OcuparVaga();
                        await _patioRepository.UpdateAsync(novoPatio);
                    }
                }
                else
                {
                    if (ocupavaVagaAntes && !ocuparaVagaAgora)
                    {
                        patioAntigo.LiberarVaga();
                        await _patioRepository.UpdateAsync(patioAntigo);
                    }
                    else if (!ocupavaVagaAntes && ocuparaVagaAgora)
                    {
                        if (!patioAntigo.TemVagaDisponivel())
                            return (false, "Não há vagas disponíveis no pátio");
                        
                        patioAntigo.OcuparVaga();
                        await _patioRepository.UpdateAsync(patioAntigo);
                    }
                }

                return (true, "Vagas gerenciadas com sucesso");
            }
            catch (Exception ex)
            {
                return (false, $"Erro ao gerenciar vagas: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<bool>> DeleteMotoAsync(string placa)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(placa))
                    return ServiceResponse<bool>.Error("Placa é obrigatória");

                var moto = await _motoRepository.GetByIdAsync(placa);
                if (moto is null)
                    return ServiceResponse<bool>.NotFound("Moto");

                var patio = await _patioRepository.GetByIdAsync(moto.NomePatio);
                if (patio is not null && moto.OcupaVaga)
                {
                    patio.LiberarVaga();
                    await _patioRepository.UpdateAsync(patio);
                }

                await _motoRepository.DeleteAsync(moto);
                return ServiceResponse<bool>.Ok(true, "Moto removida com sucesso!");
            }
            catch (Exception ex)
            {
                return ServiceResponse<bool>.Error($"Erro ao excluir moto: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<MotoManutencaoPrediction>> PreverManutencaoAsync(string placa)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(placa))
                    return ServiceResponse<MotoManutencaoPrediction>.Error("Placa é obrigatória");

                var moto = await _motoRepository.GetByIdAsync(placa);
                if (moto is null)
                    return ServiceResponse<MotoManutencaoPrediction>.NotFound("Moto");

                var prediction = _predictionService.PredictManutencao(moto);
                
                await _motoRepository.AtualizarStatusManutencaoAsync(placa, prediction.PrecisaManutencao, prediction.Probability);

                return ServiceResponse<MotoManutencaoPrediction>.Ok(prediction, "Predição realizada com sucesso");
            }
            catch (Exception ex)
            {
                return ServiceResponse<MotoManutencaoPrediction>.Error($"Erro ao prever manutenção: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<List<Moto>>> GetMotosPrecisandoManutencaoAsync()
        {
            try
            {
                var motos = await _motoRepository.GetMotosPrecisandoManutencaoAsync();
                return ServiceResponse<List<Moto>>.Ok(motos, "Motos precisando de manutenção recuperadas");
            }
            catch (Exception ex)
            {
                return ServiceResponse<List<Moto>>.Error($"Erro ao buscar motos para manutenção: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<List<Moto>>> GetMotosPorPatioAsync(string nomePatio)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nomePatio))
                    return ServiceResponse<List<Moto>>.Error("Nome do pátio é obrigatório");

                var motos = await _motoRepository.GetByPatioAsync(nomePatio);
                return ServiceResponse<List<Moto>>.Ok(motos, "Motos do pátio recuperadas com sucesso");
            }
            catch (Exception ex)
            {
                return ServiceResponse<List<Moto>>.Error($"Erro ao buscar motos do pátio: {ex.Message}");
            }
        }
    }
}