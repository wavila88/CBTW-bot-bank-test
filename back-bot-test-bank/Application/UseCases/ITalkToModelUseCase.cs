namespace Application.UseCases
{
    public interface ITalkToModelUseCase
    {
        /// <exception cref="Domain.Exceptions.DomainException">Code InvalidRequest: the message is empty.</exception>
        Task<string> ExecuteAsync(string message, CancellationToken ct = default);
    }
}
