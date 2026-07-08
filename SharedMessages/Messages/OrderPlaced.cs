namespace SharedMessages.Messages;

// uma classe sealed tem melhor performance, pois o compilador pode otimizar melhor o código,
// já que sabe que a classe não pode ser herdada.
public sealed record OrderPlaced(Guid OrderId, int Quantity);
