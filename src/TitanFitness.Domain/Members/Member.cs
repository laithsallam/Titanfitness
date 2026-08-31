using TitanFitness.Domain.Common;

namespace TitanFitness.Domain.Members;

/// <summary>
/// A person who has joined the gym. Being a Member grants nothing by itself -
/// access comes from holding a Membership (a separate aggregate) - so this
/// aggregate stays a plain identity record: who they are and which branch they
/// call home. It does not hold a collection of Memberships; Memberships are
/// looked up by MemberId from their own repository, because a member can
/// accumulate years of memberships and check-ins and none of that should have
/// to load through this root.
/// </summary>
public sealed class Member : AggregateRoot<Guid>
{
    public MembershipNumber MembershipNumber { get; private set; } = null!;
    public string FullName { get; private set; } = null!;
    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    public string? Address { get; private set; }
    public DateOnly JoinedDate { get; private set; }
    public string? PhotoUrl { get; private set; }
    public Guid HomeBranchId { get; private set; }

    private Member() { }

    private Member(Guid id, MembershipNumber membershipNumber, string fullName, string? email,
        string? phone, string? address, DateOnly joinedDate, string? photoUrl, Guid homeBranchId) : base(id)
    {
        MembershipNumber = membershipNumber;
        FullName = fullName;
        Email = email;
        Phone = phone;
        Address = address;
        JoinedDate = joinedDate;
        PhotoUrl = photoUrl;
        HomeBranchId = homeBranchId;
    }

    public static Member Register(MembershipNumber membershipNumber, string fullName, string? email,
        string? phone, string? address, DateOnly joinedDate, string? photoUrl, Guid homeBranchId)
    {
        Guard.AgainstNullOrWhiteSpace(fullName, nameof(FullName), 100);
        Guard.AgainstTooLong(email, nameof(Email), 100);
        Guard.AgainstTooLong(phone, nameof(Phone), 20);
        Guard.AgainstTooLong(address, nameof(Address), 200);

        return new Member(Guid.NewGuid(), membershipNumber, fullName, email, phone, address,
            joinedDate, photoUrl, homeBranchId);
    }

    public void UpdateProfile(string fullName, string? email, string? phone, string? address, string? photoUrl)
    {
        Guard.AgainstNullOrWhiteSpace(fullName, nameof(FullName), 100);
        Guard.AgainstTooLong(email, nameof(Email), 100);
        Guard.AgainstTooLong(phone, nameof(Phone), 20);
        Guard.AgainstTooLong(address, nameof(Address), 200);

        FullName = fullName;
        Email = email;
        Phone = phone;
        Address = address;
        PhotoUrl = photoUrl;
    }

    public void ChangeHomeBranch(Guid branchId) => HomeBranchId = branchId;
}
