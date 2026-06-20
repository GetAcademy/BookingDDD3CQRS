# CQRS without Event Sourcing

This project is a teaching example of CQRS in a small monolith. It keeps the
existing DDD model and SQL tables, but makes the command side and query side
visible in the code.

## Command side

Commands change the system. They live under
`BookingDDD.Core/Application/Commands`.

The commands in this example are:

- `BookResource`
- `CancelBooking`

Command handlers use the domain model because they must protect business
rules. `BookResourceHandler` loads the `Resource` aggregate, creates a
`BookingPeriod`, calls `resource.Book(...)`, saves the aggregate, commits the
unit of work, and publishes domain events after the commit.

`CancelBookingHandler` follows the same shape: it loads the `Resource`
aggregate that owns the booking, calls `resource.CancelBooking(...)`, saves,
commits, and then publishes domain events.

The important flow is:

```text
Command
Handler
Repository
Aggregate root
Domain method
Repository Save
Unit of Work Commit
Domain Events
```

The rule that a resource cannot be double-booked still belongs to the
`Resource` aggregate. The API layer does not own that rule.

## Query side

Queries only read data. They live under
`BookingDDD.Core/Application/Queries`.

The queries in this example are:

- `GetBookingsForResource`
- `GetCalendarForDay`
- `GetAvailableSlots`

Query handlers return DTOs/read models:

- `BookingForResourceDto`
- `CalendarItemDto`
- `AvailableSlotDto`

The SQL implementation is `DapperBookingQueries` in
`BookingDDD.Infrastructure`. It reads directly from tables and returns shapes
that are useful for screens or API responses. It does not rehydrate the
`Resource` aggregate, does not call domain methods, and does not publish
domain events.

`GetCalendarForDay` is intentionally flat: it joins bookings with resources so
the caller gets `ResourceName` next to the booking data. That read model is
different from the write model because a calendar view does not need the full
aggregate.

## No Event Sourcing

This is CQRS without Event Sourcing.

The current state is still stored in ordinary SQL tables such as `Resources`
and `Bookings`. There is no event store, no event stream, no replay, no
snapshots, and no rebuilding state from historical events.

The project still has domain events, but they are not the source of truth.
Here they are simple post-commit notifications for consequences such as audit,
calendar updates, notifications, and logging.

## Why this split helps

Commands are optimized for correctness. They go through the model that protects
rules.

Queries are optimized for reading. They can return exactly the shape the caller
needs.

CQRS does not require two databases, microservices, a message bus, or Event
Sourcing. In this project it is just a clear structure inside one codebase.

## A later Event Sourcing variant

A later version could introduce an event store and rebuild aggregate state from
events. That would be a different lesson. This version deliberately avoids it
so students can first see CQRS as a simple command/query split.
