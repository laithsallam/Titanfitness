using FluentValidation;
using MediatR;
using TitanFitness.Domain.Branches;
using TitanFitness.Domain.Common;
using TitanFitness.Domain.Studios;

namespace TitanFitness.Application.Studios;

public record CreateStudioCommand(string Name, Guid BranchId, int Capacity) : IRequest<StudioDto>;

public class CreateStudioCommandValidator : AbstractValidator<CreateStudioCommand>
{
    public CreateStudioCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(50);
        RuleFor(x => x.BranchId).NotEmpty();
        RuleFor(x => x.Capacity).GreaterThan(0);
    }
}

public class CreateStudioCommandHandler : IRequestHandler<CreateStudioCommand, StudioDto>
{
    private readonly IStudioRepository _studios;
    private readonly IBranchRepository _branches;
    private readonly IUnitOfWork _uow;

    public CreateStudioCommandHandler(IStudioRepository studios, IBranchRepository branches, IUnitOfWork uow)
    {
        _studios = studios;
        _branches = branches;
        _uow = uow;
    }

    public async Task<StudioDto> Handle(CreateStudioCommand request, CancellationToken ct)
    {
        var branch = await _branches.GetByIdAsync(request.BranchId, ct)
            ?? throw new DomainException("Branch not found.");

        var studio = Studio.Create(request.Name, branch.Id, request.Capacity);
        await _studios.AddAsync(studio, ct);
        await _uow.SaveChangesAsync(ct);

        return new StudioDto(studio.Id, studio.Name, studio.BranchId, studio.Capacity);
    }
}
