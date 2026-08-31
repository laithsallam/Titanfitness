using TitanFitness.Domain.Common;

namespace TitanFitness.Domain.Trainers;

public sealed class Trainer : AggregateRoot<Guid>
{
    public string Name { get; private set; } = null!;
    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    public bool IsActive { get; private set; }

    private Trainer() { }

    private Trainer(Guid id, string name, string? email, string? phone, bool isActive) : base(id)
    {
        Name = name;
        Email = email;
        Phone = phone;
        IsActive = isActive;
    }

    public static Trainer Create(string name, string? email, string? phone)
    {
        Guard.AgainstNullOrWhiteSpace(name, nameof(Name), 100);
        Guard.AgainstTooLong(email, nameof(Email), 100);
        Guard.AgainstTooLong(phone, nameof(Phone), 20);
        return new Trainer(Guid.NewGuid(), name, email, phone, isActive: true);
    }

    public void UpdateDetails(string name, string? email, string? phone)
    {
        Guard.AgainstNullOrWhiteSpace(name, nameof(Name), 100);
        Guard.AgainstTooLong(email, nameof(Email), 100);
        Guard.AgainstTooLong(phone, nameof(Phone), 20);
        Name = name;
        Email = email;
        Phone = phone;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
