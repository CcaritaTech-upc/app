using IoBuild.Api.Persistence;
using IoBuild.Api.Publishing.Domain.Repositories;
using IoBuild.Api.Publishing.Domain.Services;
using IoBuild.Api.Publishing.Domain.Services.Commands;

namespace IoBuild.Api.Publishing.Application.Internal.CommandServices;

public class ClientCommandService : IClientCommandService
{
    private readonly IClientRepository _clientRepository;
    private readonly IoBuildDbContext _dbContext;

    public ClientCommandService(IClientRepository clientRepository, IoBuildDbContext dbContext)
    {
        _clientRepository = clientRepository;
        _dbContext = dbContext;
    }

    public async Task<int> Handle(CreateClientCommand command, CancellationToken ct = default)
    {
        var client = new Client(command.FullName, command.ProjectName, command.AccountStatement, command.BuilderId, command.ProjectId);
        await _clientRepository.AddAsync(client, ct);
        await _dbContext.SaveChangesAsync(ct);
        return client.Id;
    }

    public async Task Handle(UpdateClientCommand command, CancellationToken ct = default)
    {
        var client = await _clientRepository.FindByIdAsync(command.Id, ct);
        if (client is null)
        {
            throw new KeyNotFoundException($"Client with ID {command.Id} not found.");
        }

        client.Update(command.FullName, command.ProjectName, command.AccountStatement, command.BuilderId, command.ProjectId);
        await _clientRepository.UpdateAsync(client, ct);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task Handle(DeleteClientCommand command, CancellationToken ct = default)
    {
        var client = await _clientRepository.FindByIdAsync(command.Id, ct);
        if (client is null)
        {
            throw new KeyNotFoundException($"Client with ID {command.Id} not found.");
        }

        await _clientRepository.DeleteAsync(client, ct);
        await _dbContext.SaveChangesAsync(ct);
    }
}
