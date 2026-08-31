namespace TitanFitness.Domain.Common;

/// <summary>Whether a membership's terms open only the home branch, or every branch in the chain.</summary>
public enum AccessScope
{
    HomeBranchOnly = 0,
    AllBranches = 1
}
