# Domain Model Notes

The MVP domain model intentionally favors a small number of entities.

Key choices:

- `DemoPerson` centralizes Admin, Technician, and User demo characters.
- `TicketCategory`, `TicketPriority`, and `TicketStatus` are enums instead of lookup tables.
- `CompanyAccessCode` stores the recruiter code state.
- `CodeUsageLog` keeps a lightweight audit trail for owner statistics.

This keeps the SQL model recruiter-friendly and fast to implement while still demonstrating clean structure.
