using Microsoft.AspNetCore.Mvc;
using MottuChallenge.API.Models.Common;
using MottuChallenge.API.Services;

namespace MottuChallenge.API.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public abstract class BaseController : ControllerBase
    {
        protected string RequestedApiVersion => HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        
        protected ActionResult HandleServiceResponse<T>(ServiceResponse<T> response)
        {
            if (!response.Success)
            {
                return BadRequest(CreateErrorResponse(response.Message));
            }

            if (response.Data == null)
                return NotFound(CreateErrorResponse("Recurso não encontrado"));

            if (response.Data is System.Collections.IEnumerable enumerable && !enumerable.GetEnumerator().MoveNext())
                return NoContent();

            return Ok(new
            {
                response.Data,
                response.Message,
                Timestamp = DateTime.Now,
                Version = RequestedApiVersion
            });
        }

        protected ActionResult HandleServiceResponse<T>(ServiceResponse<T> response, string resourceType, string resourceId)
        {
            if (!response.Success)
            {
                return BadRequest(CreateErrorResponse(response.Message));
            }

            if (response.Data == null)
                return NotFound(CreateErrorResponse("Recurso não encontrado"));

            if (response.Data is System.Collections.IEnumerable enumerable && !enumerable.GetEnumerator().MoveNext())
                return NoContent();

            var links = CreateResourceLinks(resourceType, resourceId);

            return Ok(new
            {
                response.Data,
                response.Message,
                Links = links,
                Timestamp = DateTime.Now,
                Version = RequestedApiVersion
            });
        }

        protected ActionResult HandlePagedResponse<T>(List<T> data, int page, int pageSize, int totalCount, string message = "Dados recuperados com sucesso")
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var links = CreatePaginationLinks(baseUrl, page, pageSize, totalCount);
            
            var pagedResponse = new PagedResponse<T>(data, page, pageSize, totalCount, links, message);

            return Ok(new
            {
                pagedResponse.Data,
                pagedResponse.Page,
                pagedResponse.PageSize,
                pagedResponse.TotalCount,
                pagedResponse.TotalPages,
                pagedResponse.Links,
                pagedResponse.Message,
                Timestamp = DateTime.Now,
                Version = RequestedApiVersion
            });
        }

        protected ActionResult HandlePagedResponse<T>(List<T> data, int page, int pageSize, int totalCount, string resourceType, string message = "Dados recuperados com sucesso")
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}/api/v{RequestedApiVersion}/{resourceType.ToLower()}s";
            var links = CreatePaginationLinks(baseUrl, page, pageSize, totalCount);
            
            var pagedResponse = new PagedResponse<T>(data, page, pageSize, totalCount, links, message);

            return Ok(new
            {
                pagedResponse.Data,
                pagedResponse.Page,
                pagedResponse.PageSize,
                pagedResponse.TotalCount,
                pagedResponse.TotalPages,
                pagedResponse.Links,
                pagedResponse.Message,
                Timestamp = DateTime.Now,
                Version = RequestedApiVersion
            });
        }

        protected ActionResult HandleCreatedResponse<T>(string actionName, object routeValues, T data, string message = "Recurso criado com sucesso")
        {
            var links = new List<Link>
            {
                new Link("self", Url.Action(actionName, routeValues) ?? "", "GET"),
                new Link("update", Url.Action("Update", routeValues) ?? "", "PUT"),
                new Link("delete", Url.Action("Delete", routeValues) ?? "", "DELETE")
            };

            return CreatedAtAction(actionName, routeValues, new
            {
                Data = data,
                Message = message,
                Links = links,
                Timestamp = DateTime.Now,
                Version = RequestedApiVersion
            });
        }

        protected ActionResult HandleCreatedResponse<T>(string resourceType, string resourceId, T data, string message = "Recurso criado com sucesso")
        {
            var links = CreateResourceLinks(resourceType, resourceId);
            var uri = $"{Request.Scheme}://{Request.Host}/api/v{RequestedApiVersion}/{resourceType.ToLower()}s/{resourceId}";

            return Created(uri, new
            {
                Data = data,
                Message = message,
                Links = links,
                Timestamp = DateTime.Now,
                Version = RequestedApiVersion
            });
        }

        private List<Link> CreateResourceLinks(string resourceType, string resourceId)
        {
            var basePath = $"/api/v{RequestedApiVersion}/{resourceType.ToLower()}s";
            var links = new List<Link>
            {
                new Link($"{basePath}/{resourceId}", "self", "GET"),
                new Link($"{basePath}/{resourceId}", "update", "PUT"),
                new Link($"{basePath}/{resourceId}", "delete", "DELETE")
            };

            switch (resourceType.ToLower())
            {
                case "moto":
                    links.AddRange(new[]
                    {
                        new Link($"{basePath}/{resourceId}/prever-manutencao", "prever-manutencao", "GET"),
                        new Link($"{basePath}/{resourceId}/alugar", "alugar", "POST"),
                        new Link($"{basePath}/{resourceId}/manutencao", "registrar-manutencao", "POST")
                    });
                    break;
                    
                case "patio":
                    links.AddRange(new[]
                    {
                        new Link($"{basePath}/{resourceId}/estatisticas", "estatisticas", "GET"),
                        new Link($"{basePath}/{resourceId}/vagas", "vagas", "GET"),
                        new Link($"{basePath}/{resourceId}/funcionarios", "funcionarios", "GET"),
                        new Link($"{basePath}/{resourceId}/motos", "motos", "GET")
                    });
                    break;
                    
                case "funcionario":
                    links.AddRange(new[]
                    {
                        new Link($"{basePath}/{resourceId}/motos", "motos", "GET"),
                        new Link($"{basePath}/{resourceId}/patio", "patio", "GET")
                    });
                    break;
                    
                case "cliente":
                    links.AddRange(new[]
                    {
                        new Link($"{basePath}/{resourceId}/registrar-manutencao", "registrar-manutencao", "POST"),
                        new Link($"/api/v{RequestedApiVersion}/clientes/por-moto/{resourceId}", "buscar-por-moto", "GET")
                    });
                    break;
            }

            links.Add(new Link(basePath, "collection", "GET"));

            return links;
        }

        private List<Link> CreatePaginationLinks(string baseUrl, int page, int pageSize, int totalCount)
        {
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            var links = new List<Link>
            {
                new Link($"{baseUrl}?pageNumber={page}&pageSize={pageSize}", "self", "GET")
            };
            
            if (page > 1)
                links.Add(new Link($"{baseUrl}?pageNumber={page - 1}&pageSize={pageSize}", "prev", "GET"));
            
            if (page < totalPages)
                links.Add(new Link($"{baseUrl}?pageNumber={page + 1}&pageSize={pageSize}", "next", "GET"));

            links.Add(new Link($"{baseUrl}?pageNumber=1&pageSize={pageSize}", "first", "GET"));
            links.Add(new Link($"{baseUrl}?pageNumber={totalPages}&pageSize={pageSize}", "last", "GET"));

            return links;
        }

        protected ErrorResponse CreateErrorResponse(string message, List<string>? errors = null)
        {
            if (errors == null && !ModelState.IsValid)
            {
                errors = ModelState.Values
                    .SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                    .ToList();
            }

            return new ErrorResponse 
            { 
                Message = message, 
                Errors = errors ?? new List<string>(),
                Path = $"{Request.Method} {Request.Path}",
                Timestamp = DateTime.Now
            };
        }

        protected bool IsCurrentUser(string username) => User.Identity?.Name == username;
    }
}