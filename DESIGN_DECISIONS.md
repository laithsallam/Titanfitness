# Titan Fitness Staff Portal — Design Decisions

The brief is explicit that the entity list is raw material, not a schema, and
that aggregate boundaries are what's being assessed. This document explains
the boundaries chosen and, more importantly, *why* — each one is a direct
answer to a sentence in the business logic.

## Aggregate map

| Aggregate root | Owns (child entities/VOs) | Referenced by ID from elsewhere |
|---|---|---|
| `Branch` | `OperatingHours` (VO) | Studio, Member, CheckIn, ClassSession |
| `Studio` | — | ClassSession |
| `Trainer` | — | ClassSession |
| `Plan` | — | Membership (via `AgreedTerms` snapshot, not live reference) |
| `Member` | `MembershipNumber` (VO) | Membership, CheckIn, Booking |
| `Membership` | `AgreedTerms` (VO), `Freeze` (entity), `GuestPass` (entity) | — |
| `ClassSession` | `Booking` (entity) | — |
| `CheckIn` | — | (leaf; append-only) |

Six standalone/thin roots (Branch, Studio, Trainer, Plan, Member, CheckIn) and
two rich roots (Membership, ClassSession) that each own a small transactional
family of child entities. That split is deliberate:

## Why AgreedTerms exists (the rule that breaks the obvious design)

> "When a plan changes, members who already hold a membership keep the terms
> they were sold... Plan changes reach new purchases only. This is the single
> most important rule in the system, and the obvious design does not survive
> it."

The "obvious" design is `Membership { PlanId }` and reading freeze caps, guest
pass quotas, price, etc. straight off the live `Plan`. That fails the moment
management edits a plan: every existing member's rights would silently shift.

The fix is `AgreedTerms`, an immutable value object copied field-by-field from
the `Plan` at the instant of purchase (`AgreedTerms.CopyFrom(plan)`), stored
inside the `Membership`. Every rule the `Membership` aggregate enforces —
freeze day budget, freeze count, guest pass quota, access scope — reads from
`Terms`, never from `Plan`. `PlanId` is kept only for traceability/reporting.
`Plan.UpdateDetails(...)` can now freely change prices and caps; it is
*structurally* incapable of touching a single existing membership.

## Why Membership owns Freeze and GuestPass

Both have real identity (staff reference "freeze #3", a specific guest pass),
but every rule about them only makes sense at the level of the whole
membership: "does this freeze fit in the *remaining* budget", "has the
*whole* guest-pass quota been used up", "do these two freezes overlap".
Giving them their own repositories would let two concurrent requests each
check the budget against a stale view and both succeed — a classic
lost-update bug. Pulling them inside the `Membership` aggregate means every
check happens against the one loaded, in-memory object, inside one
transaction, so the invariant can never be violated in the database.

## Why ClassSession owns Booking

Same argument, same shape: "a session never takes more bookings than its
capacity... the first waiting is promoted automatically when a place is
freed" is one invariant over the *entire* booking list, not a property of any
single booking. `ClassSession.Book(...)` and `.CancelBooking(...)` are the
only ways a `Booking` is created or changes state, and the waitlist-promotion
logic lives in one place, guaranteed consistent.

## Why Branch, Studio, Trainer, Plan, and Member stay thin standalone roots

Studio, Trainer, and Member are all referenced by several *other* aggregates
(ClassSession needs a Studio's capacity and a Trainer's identity; Membership
and CheckIn need a Member's identity). If any of these lived inside a bigger
aggregate (e.g. Studio inside Branch), every one of those cross-references
would either have to pull the whole parent aggregate through the back door,
or duplicate IDs awkwardly. Keeping them as small, independent roots means
every cross-reference is a plain ID — cheap to store, cheap to look up, and
it makes the true consistency boundaries (Membership, ClassSession) stand out
instead of getting lost in a sea of "everything belongs to Branch."

`CheckIn` is a standalone root for a different reason: it's a pure,
append-only fact ("every attempt is recorded... an established member
accumulates a great many of these") with no invariant that spans other
CheckIns. It only *reads* other aggregates (via a domain service) to decide
Admit vs. Refuse.

## Domain services: rules that cross aggregate boundaries

Three rules in the brief span more than one aggregate and therefore cannot
live inside any single aggregate's `Configure`/method set. These live in
`Domain/Services` as stateless services that take already-loaded aggregates
and contain no I/O of their own (the application layer's command handlers do
the fetching):

- **`MembershipSchedulingRules`** — "a member must never hold two memberships
  covering the same day" (needs every other `Membership` for that member) and
  the Renew/Change-Plan flow (always produces a *new* `Membership` — see
  below).
- **`SessionSchedulingService`** — "a trainer cannot run two overlapping
  sessions, a studio cannot host two at once" and "a member cannot... be
  booked onto two sessions that overlap" (each needs every *other* session
  for that trainer/studio/member).
- **`CheckInEligibilityService`** — entry eligibility depends on Member (home
  branch), Membership (status + access scope) and the branch being entered —
  three aggregates at once.

## Why Renew / Change Plan never edits the existing Membership

> "Cancellation is final: it cannot be resumed, renewed from, or reversed."

Combined with `AgreedTerms` being immutable, the only way to move a member
onto new terms is to create a *new* `Membership` aggregate scheduled so it
never overlaps the one it replaces (`MembershipSchedulingRules.CreateFollowOn`).
"At renewal" schedules the new membership to start the day after the current
one ends; "Immediately" cancels the current one today and starts the new one
today. Either way, the old aggregate is never resurrected or mutated beyond
that one, final `Cancel()` call.

## Why MembershipStatus is computed, not stored

`Pending`/`Active`/`Frozen`/`Expired` are pure functions of "what date is it"
plus the freeze list; only `Cancelled` is a genuine fact that needs to be
remembered forever. Storing the other four as a column would require a
background job to tick every membership over at midnight — easy to forget to
run, and a source of "why does this say Active when it's clearly expired"
bugs. `Membership.GetStatus(DateOnly asOf)` derives the answer on demand, so
it is always correct the instant anyone asks, with no scheduled job at all.

## CQRS shape

Commands mutate exactly one aggregate root per handler (load → call a domain
method → `Update` → `SaveChangesAsync`) and never reach into a second
aggregate's *internal* state — cross-aggregate coordination goes through a
domain service, as above. Queries bypass the rich domain model entirely and
project straight from EF Core (`IClassSessionRepository.GetScheduleAsync`,
`IMembershipRepository.CountActiveByPlanAsync`, the Dashboard queries) since
read-side use cases don't need to protect invariants, only to answer
questions quickly.

## What was deliberately left out

- **Domain events aren't dispatched.** `AggregateRoot` collects them
  (`Raise`/`DomainEvents`), but nothing currently reads that list. Wiring a
  dispatcher (SaveChanges interceptor → MediatR notifications) is the natural
  next step if you want side effects like "email the member when a freeze is
  approved," but it wasn't needed for the assignment's scope and adding it
  speculatively would be over-engineering.
- **No authentication/authorization.** The brief describes a staff portal but
  doesn't specify a login model; adding one would be guessing at
  requirements that weren't given.
- **No "renew at end of month" background job.** Follow-on memberships are
  created explicitly by the Renew/Change-Plan action, matching Figures 5–6
  exactly (staff-triggered, not automatic).
