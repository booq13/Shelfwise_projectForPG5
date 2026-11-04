using Microsoft.AspNetCore.Mvc;
using Shelfwise.Catalog.Application.DTOs;
using Shelfwise.Catalog.Application.Services;
using Shelfwise.Catalog.Domain.Exceptions;
using Shelfwise.Catalog.Presentation.Contracts;

namespace Shelfwise.Catalog.Presentation.Controllers;

[ApiController]
[Route("api/batches")]
public sealed class BatchesController : ControllerBase
{
    private readonly IBatchService _batchService;

    public BatchesController(IBatchService batchService)
    {
        _batchService = batchService;
    }

    // Command: Register batch -> 201 Created
    [HttpPost]
    public async Task<ActionResult<BatchDto>> RegisterBatch(
        [FromBody] RegisterBatchRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var batch = await _batchService.RegisterBatchAsync(
                request.ProductId, request.Quantity, request.ExpiryDate, cancellationToken);

            return CreatedAtAction(nameof(GetBatch), new { batchId = batch.BatchId }, batch);
        }
        catch (DomainException ex)
        {
            return MapDomainException(ex);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { code = "validation.invalid_argument", message = ex.Message });
        }
    }

    // Query: Available stock per batch -> 200 / 404
    [HttpGet("{batchId:guid}")]
    public async Task<ActionResult<BatchDto>> GetBatch(Guid batchId, CancellationToken cancellationToken)
    {
        var batch = await _batchService.GetBatchAsync(batchId, cancellationToken);
        if (batch is null)
        {
            return NotFound(new { code = "batch.not_found", message = $"Batch '{batchId}' was not found." });
        }

        return Ok(batch);
    }

    // Command: Reserve stock -> 200 / 400 / 404 / 422
    [HttpPost("{batchId:guid}/reservations")]
    public async Task<ActionResult<BatchDto>> ReserveStock(
        Guid batchId,
        [FromBody] ReserveStockRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var batch = await _batchService.ReserveStockAsync(
                batchId, request.OrderId, request.Quantity, cancellationToken);

            return Ok(batch);
        }
        catch (DomainException ex)
        {
            return MapDomainException(ex);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { code = "validation.invalid_argument", message = ex.Message });
        }
    }

    // Domain exception -> HTTP status code (this mapping belongs only to Presentation)
    private ActionResult MapDomainException(DomainException ex) => ex switch
    {
        BatchNotFoundException => NotFound(new { code = ex.Code, message = ex.Message }),

        InvalidQuantityException or InvalidExpiryDateException =>
            BadRequest(new { code = ex.Code, message = ex.Message }),

        InsufficientStockException stockEx => UnprocessableEntity(new
        {
            code = stockEx.Code,
            message = stockEx.Message,
            requestedQuantity = stockEx.RequestedQuantity,
            availableQuantity = stockEx.AvailableQuantity
        }),

        BatchExpiredException expiredEx => UnprocessableEntity(new
        {
            code = expiredEx.Code,
            message = expiredEx.Message,
            expiryDate = expiredEx.ExpiryDate
        }),

        _ => UnprocessableEntity(new { code = ex.Code, message = ex.Message })
    };
}