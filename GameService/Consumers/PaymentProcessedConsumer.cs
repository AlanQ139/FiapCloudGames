using GameService.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts;

namespace GameService.Consumers;

/// <summary>
/// Consumer MassTransit para processar eventos de pagamento
/// </summary>
public class PaymentProcessedConsumer : IConsumer<IPaymentProcessed>
{
    private readonly AppDbContext _context;
    private readonly ILogger<PaymentProcessedConsumer> _logger;

    public PaymentProcessedConsumer(
        AppDbContext context,
        ILogger<PaymentProcessedConsumer> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Método chamado automaticamente quando uma mensagem chega
    /// MassTransit gerencia:
    /// - Deserialização automática
    /// - Retry (se configurado)
    /// - Dead letter queue (se falhar)
    /// - ACK/NACK automático
    /// </summary>
    public async Task Consume(ConsumeContext<IPaymentProcessed> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Pagamento recebido via MassTransit: Payment={PaymentId}, Purchase={PurchaseId}, Status={Status}",
            message.PaymentId, message.PurchaseId, message.Status);

        // Busca a compra relacionada
        var purchase = await _context.Purchases
            .FirstOrDefaultAsync(p => p.Id == message.PurchaseId);

        if (purchase == null)
        {
            _logger.LogWarning("Compra {PurchaseId} não encontrada", message.PurchaseId);

            // MassTransit vai mover para _error queue automaticamente
            throw new InvalidOperationException($"Purchase {message.PurchaseId} not found");
        }

        // Atualiza status baseado no resultado do pagamento
        if (message.Status == PaymentStatus.Paid)
        {
            _logger.LogInformation("Compra {PurchaseId} confirmada!", message.PurchaseId);
                        
            // - Enviar email de confirmação
            // - Liberar download do jogo
            // - Atualizar estatísticas
            // - Publicar evento IGamePurchaseCompleted

            // Exemplo: publicar outro evento
            // await context.Publish<IGamePurchaseCompleted>(new { ... });
        }
        else if (message.Status == PaymentStatus.Failed)
        {
            _logger.LogWarning(
                "Falhou no pagamento da compra {PurchaseId}: {ErrorMessage}", message.PurchaseId, message.ErrorMessage);

            // Remove a compra se o pagamento falhou
            _context.Purchases.Remove(purchase);
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Processamento concluído para Purchase={PurchaseId}", message.PurchaseId);

        // MassTransit faz ACK automático se não lançar exceção
    }
}


/// <summary>
/// OPCIONAL: Consumer para Command (ao invés de Event)
/// Use quando quiser garantir que apenas UM serviço processe
/// </summary>
public class ProcessPaymentConsumer : IConsumer<IProcessPayment>
{
    private readonly ILogger<ProcessPaymentConsumer> _logger;

    public ProcessPaymentConsumer(ILogger<ProcessPaymentConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<IProcessPayment> context)
    {
        var command = context.Message;

        _logger.LogInformation("Comando recebido: ProcessPayment para Purchase={PurchaseId}",
            command.PurchaseId);

        // Processar comando...
        await Task.Delay(100); // Simula processamento

        // Responder ao sender (se necessário)
        await context.RespondAsync<IPaymentProcessed>(new
        {
            PaymentId = Guid.NewGuid(),
            command.PurchaseId,
            command.UserId,
            command.GameId,
            command.Amount,
            Status = PaymentStatus.Paid,
            ProcessedAt = DateTime.UtcNow,
            ErrorMessage = (string?)null
        });
    }
}