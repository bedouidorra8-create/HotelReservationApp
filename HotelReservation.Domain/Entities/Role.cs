using HotelReservation.Domain.Common;

namespace HotelReservation.Domain.Entities;

public class Role : AggregateRoot<Guid>
{
    private readonly List<string> _permissions = new();

    public string Nom { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    public IReadOnlyCollection<string> Permissions => _permissions.AsReadOnly();

    private Role() { }

    public Role(Guid id, string nom, string description) : base(id)
    {
        if (string.IsNullOrWhiteSpace(nom))
            throw new DomainException("Le nom du rôle est obligatoire.");

        Nom = nom.Trim();
        Description = description;
    }

    public void AjouterPermission(string permission)
    {
        if (string.IsNullOrWhiteSpace(permission))
            return;

        var perm = permission.Trim();
        if (!_permissions.Contains(perm))
        {
            _permissions.Add(perm);
        }
    }

    public void RetirerPermission(string permission)
    {
        if (string.IsNullOrWhiteSpace(permission))
            return;

        _permissions.Remove(permission.Trim());
    }
}
