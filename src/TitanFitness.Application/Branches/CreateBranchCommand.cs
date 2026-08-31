using FluentValidation;
using MediatR;
using TitanFitness.Domain.Branches;
using TitanFitness.Domain.Common;

namespace TitanFitness.Application.Branches;

public record CreateBranchCommand(string Name, string? Address, TimeOnly OpeningTime, TimeOnly ClosingTime) : IRequest<BranchDto>;

public class CreateBranchCommandValidator : AbstractValidator<CreateBranchCommand>
{
    public CreateBranchCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Address).MaximumLength(200);
        RuleFor(x => x.ClosingTime).GreaterThan(x => x.OpeningTime);
    }
}

public class CreateBranchCommandHandler : IRequestHandler<CreateBranchCommand, BranchDto>
{
    private readonly IBranchRepository _branches;
    private readonly IUnitOfWork _uow;

    public CreateBranchCommandHandler(IBranchRepository branches, IUnitOfWork uow)
    {
        _branches = branches;
        _uow = uow;
    }

    public async Task<BranchDto> Handle(CreateBranchCommand request, CancellationToken ct)
    {
        var hours = TimeRange.Create(request.OpeningTime, request.ClosingTime);
        var branch = Branch.Create(request.Name, request.Address, hours);

        await _branches.AddAsync(branch, ct);
        await _uow.SaveChangesAsync(ct);

        return new BranchDto(branch.Id, branch.Name, branch.Address, hours.Open, hours.Close);
    }
}
