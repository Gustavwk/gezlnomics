using Backend.Application.Abstractions;
using Backend.Application.Models;
using Backend.Application.Services;
using Backend.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/transactions")]
public sealed class TransactionsController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] DateOnly? from, [FromQuery] DateOnly? to, [FromQuery] TransactionKind? kind, [FromQuery] string? category, [FromQuery(Name = "q")] string? query, [FromServices] ICurrentUserService currentUserService, [FromServices] ITransactionService service, CancellationToken cancellationToken)
    {
        var transactions = await service.GetAllAsync(currentUserService.GetRequiredUserId(), from, to, kind, category, query, cancellationToken);
        return Ok(transactions);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] UpsertTransactionRequest request, [FromServices] ICurrentUserService currentUserService, [FromServices] ITransactionService service, CancellationToken cancellationToken)
    {
        var transaction = await service.CreateAsync(currentUserService.GetRequiredUserId(), request, cancellationToken);
        return Ok(transaction);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpsertTransactionRequest request, [FromServices] ICurrentUserService currentUserService, [FromServices] ITransactionService service, CancellationToken cancellationToken)
    {
        var transaction = await service.UpdateAsync(currentUserService.GetRequiredUserId(), id, request, cancellationToken);
        return transaction is null ? NotFound() : Ok(transaction);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, [FromServices] ICurrentUserService currentUserService, [FromServices] ITransactionService service, CancellationToken cancellationToken)
    {
        var deleted = await service.DeleteAsync(currentUserService.GetRequiredUserId(), id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
