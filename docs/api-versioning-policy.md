# API Versioning Policy

This policy describes how the TMS API manages versions and how we treat breaking vs additive changes, sunset timing, communication, and version skipping.

## 1. Breaking changes
A change is considered breaking if an existing client can no longer use the API without code changes.

- Removing a field from a response or request schema.
- Renaming a field or changing a property name.
- Changing a response status code for an existing successful or error path.
- Tightening validation rules so an earlier request that used to succeed now fails.
- Changing the default sort order for a list endpoint when the sort order is not explicitly specified.

Breaking changes require a new major API version.

## 2. Additive (non-breaking) changes
A change is additive when existing clients continue to work unchanged.

- Adding a new optional field to a response or request.
- Adding a new endpoint under the current version.
- Adding a new optional query parameter that does not alter existing behavior when omitted.

Additive enhancements can be released in the same API version.

## 3. Sunset window
When a new major version ships, the previous major version remains available for at least six months.

This minimum 6-month window gives rural training centres and quarterly maintenance schedules enough time to migrate. During this window:

- V1 continues to serve traffic.
- V2 is the recommended current version.
- V1 may be retired only after the sunset date is clearly communicated.

## 4. Communication
We communicate version changes openly from day one of V2.

- **Deprecation headers**: V1 responses include Deprecation: true.
- **Sunset headers**: V1 responses include Sunset: <sunset-date> with the scheduled shutdown date.
- **Link headers**: V1 responses include a Link header pointing to the successor version when a direct mapping exists.
- **CHANGELOG**: Add a release note entry for the version launch, deprecation, and sunset schedule.
- **Email**: Notify every team that holds an API key when V2 is released and again before V1 is retired.
- **Calendar invite**: Send a calendar invite for the V1 shutdown date so teams can block migration time.

## 5. Skipping versions
Clients may migrate directly from an older supported major version to a later one without adopting every intermediate major version.

- Migrating from V1 to V3 is allowed.
- Clients are not forced to move through V2 first.
- Each supported version is treated as a valid migration target.

By following these rules, the TMS API keeps compatibility expectations clear and supports a smooth transition path for all clients.
