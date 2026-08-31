using TitanFitness.Domain.Classes;
using TitanFitness.Domain.Common;

namespace TitanFitness.Domain.Services;

/// <summary>
/// "A trainer cannot run two overlapping sessions, a studio cannot host two at
/// once." Both rules compare a candidate session against every OTHER session
/// for the same trainer/studio, which is a cross-aggregate concern (there is
/// no single ClassSession that owns "all sessions for trainer X"), so it lives
/// here rather than inside ClassSession.Schedule. The application handler is
/// responsible for fetching the "existing sessions on that date" set (a cheap,
/// narrow query) before calling this.
/// </summary>
public static class SessionSchedulingService
{
    public static void EnsureNoTrainerOrStudioConflict(
        ClassSession candidate,
        IEnumerable<ClassSession> otherSessionsOnSameDate)
    {
        foreach (var other in otherSessionsOnSameDate)
        {
            if (other.Id.Equals(candidate.Id)) continue;
            if (other.Status == SessionStatus.Cancelled) continue;
            if (!candidate.OverlapsTimeWith(other)) continue;

            if (other.TrainerId == candidate.TrainerId)
                throw new DomainException("This trainer is already running another session at that time.");

            if (other.StudioId == candidate.StudioId)
                throw new DomainException("This studio already hosts another session at that time.");
        }
    }

    /// <summary>"A member cannot... be booked onto two sessions that overlap."</summary>
    public static void EnsureNoMemberDoubleBooking(
        ClassSession candidate,
        Guid memberId,
        IEnumerable<ClassSession> memberOtherActiveSessions)
    {
        foreach (var other in memberOtherActiveSessions)
        {
            if (other.Id.Equals(candidate.Id)) continue;
            if (candidate.OverlapsTimeWith(other))
                throw new DomainException("This member is already booked on an overlapping session.");
        }
    }
}
