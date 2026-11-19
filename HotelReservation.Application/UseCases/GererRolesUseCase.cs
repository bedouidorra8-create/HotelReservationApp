using HotelReservation.Application.DTOs;
using HotelReservation.Domain.Common;
using HotelReservation.Domain.Entities;
using HotelReservation.Domain.Repositories;

namespace HotelReservation.Application.UseCases;

public class GererRolesUseCase
{
    private readonly IRoleRepository _repository;

    public GererRolesUseCase(IRoleRepository repository)
    {
        _repository = repository;
    }

    public async Task<RoleDto> ExecuteAsync(GererRolesRequest request, CancellationToken cancellationToken = default)
    {
        Role role;
        if (request.RoleId.HasValue)
        {
            role = await _repository.GetByIdAsync(request.RoleId.Value, cancellationToken)
                ?? throw new DomainException("Rôle introuvable.");
        }
        else
        {
            if (await _repository.GetByNomAsync(request.Nom, cancellationToken) != null)
                throw new DomainException("Un rôle avec ce nom existe déjà.");
            role = new Role(Guid.NewGuid(), request.Nom, request.Description);
        }

        foreach (var permission in request.Permissions)
            role.AjouterPermission(permission);

        if (request.RoleId.HasValue) await _repository.UpdateAsync(role, cancellationToken);
        else await _repository.AddAsync(role, cancellationToken);
        return new RoleDto(role.Id, role.Nom, role.Description, role.Permissions);
    }

    public async Task<RoleDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var role = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new DomainException($"Rôle avec l'ID '{id}' introuvable.");

        return new RoleDto(role.Id, role.Nom, role.Description, role.Permissions);
    }

    public async Task<IEnumerable<RoleDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var roles = await _repository.GetAllAsync(cancellationToken);
        return roles.Select(role => new RoleDto(role.Id, role.Nom, role.Description, role.Permissions));
    }
}
