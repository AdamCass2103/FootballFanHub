# FootballFanHub

A sports-centre web application built with **Blazor Server** and **.NET 10** (C#).
It combines **football** and **NBA basketball** data from two different third-party
web service providers into a single, coherent application.

---

## Contents

1. [What the application does](#1-what-the-application-does)
2. [Third-party APIs used](#2-third-party-apis-used)
3. [Technologies](#3-technologies)
4. [Project structure](#4-project-structure)
5. [How to run](#5-how-to-run)
6. [API key setup (User Secrets)](#6-api-key-setup-user-secrets)
7. [Design overview](#7-design-overview)
8. [Error handling and the NBA rate limit](#8-error-handling-and-the-nba-rate-limit)
9. [Testing](#9-testing)
10. [Known limitations](#10-known-limitations)
11. [References](#11-references)

---

## 1. What the application does

FootballFanHub is a **sports centre**: one application where a user can browse
football and NBA data side by side.

**Football (football-data.org)**
- Search for clubs (filtered in memory with LINQ)
- View team details: crest, venue, founded year, club colours, manager, squad
- View recent form, upcoming fixtures and recent results
- View league standings (Premier League, La Liga, Serie A, Bundesliga, Ligue 1)
- Follow / unfollow teams; followed teams appear on the home page with their
  next fixture

**NBA (balldontlie)**
- Browse NBA teams (logos loaded from ESPN's image CDN)
- Search NBA teams by name, city or abbreviation
- View team details, games and player statistics
- Team record calculated from returned game scores

The application is **contextualised as a sports centre** — it is not a demo of
web services. The two providers are combined to serve one user-facing purpose.

---

## 2. Third-party APIs used

| Provider | Purpose | Documentation |
|---|---|---|
| **football-data.org** | Football teams, fixtures, results, standings | https://www.football-data.org/documentation/quickstart |
| **balldontlie** | NBA teams, games, player statistics | https://docs.balldontlie.io/ |

NBA team logos are images served from ESPN's public CDN
(`https://a.espncdn.com/i/teamlogos/nba/500/{abbreviation}.png`). This is an
image CDN, **not** a third data API — all NBA *data* comes from balldontlie.

---

## 3. Technologies

- C# / .NET 10
- Blazor Server (interactive server rendering, SignalR)
- Razor components, HTML, Bootstrap CSS
- `HttpClient` with typed clients
- `System.Text.Json` for deserialization
- Dependency Injection and interfaces
- async / await
- JSON file persistence
- xUnit for unit testing

No database is used — the assignment permits file-based storage, so followed
teams are stored in `Data/favourites.json`.


## 4. Project structure


FootballFanHub
├── Components
│ ├── Layout NavMenu, MainLayout
│ └── Pages Home, Search, Team, Standings, NBA pages
├── Models Strongly-typed classes mirroring the API JSON
├── Services Interfaces + implementations for APIs and storage
├── Data favourites.json (persisted followed teams)
├── CacheData Cached API responses
├── Program.cs Startup, DI registration, HttpClient config
└── appsettings.json

FootballFanHub.Tests xUnit test project


## 5. How to run

1. Clone the repository.
2. Open the solution in Visual Studio.
3. Configure the API keys (see section 6 — **required**).
4. Set `FootballFanHub` as the startup project and run (`F5`).
5. Navigate to the Football or NBA sections from the shared nav menu.

---

## 6. API key setup (User Secrets)

API keys are **not** stored in source code. They are stored in **User Secrets**,
which keeps development credentials outside the project files so they cannot be
committed to Git.

The project has a `UserSecretsId` in the `.csproj`. Set the keys with:

```bash
dotnet user-secrets set "FootballData:ApiKey" "YOUR_FOOTBALL_DATA_KEY"
dotnet user-secrets set "Balldontlie:ApiKey" "YOUR_BALLDONTLIE_KEY"
Alternatively, right-click the project in Visual Studio → Manage User Secrets
and add:

json
{
  "FootballData:ApiKey": "YOUR_FOOTBALL_DATA_KEY",
  "Balldontlie:ApiKey": "YOUR_BALLDONTLIE_KEY"
}
The football base URL is read from FootballData:BaseUrl in configuration.

7. Design overview
The application follows a layered structure:

Razor pages
    ↓
Service interfaces (ITeamService, IStandingsService, IBalldontlieService,
                    IFavouritesRepository)
    ↓
Service implementations
    ↓
External APIs or JSON storage
    ↓
Models returned to the pages
Pages never call APIs or touch the file system directly. They inject
interfaces, and Program.cs supplies the implementations via Dependency
Injection. This keeps the UI decoupled from external systems and makes the
logic mockable for testing.

Key points:

Interfaces + DI — pages depend on ITeamService etc., not the concrete
classes, so implementations can be replaced or mocked.

Typed HttpClients — the football services are registered with
AddHttpClient, sharing one configuration block for base URL, auth header
and timeout (DRY).

Generic helper — BalldontlieService uses a GetAsync<T> helper so the
HTTP request and JSON parsing logic is written once and reused (DRY).

Repository pattern — favourites are hidden behind
IFavouritesRepository; JsonFavouritesRepository performs the file
operations, including duplicate prevention, missing-file handling and
invalid-JSON recovery, using SemaphoreSlim to prevent concurrent writes
corrupting the file.

Pure logic — FormCalculator converts match data into W/D/L form. It is
independent of any API, which makes it straightforward to unit test.

8. Error handling and the NBA rate limit
Every service call checks the HTTP response. Unsuccessful responses throw an
exception, and each Razor page catches it and displays a user-friendly error
message instead of crashing or showing a blank page. Pages also display loading
states while data is in flight.

NBA rate limit. The balldontlie free tier allows a limited number of
requests per minute. When the limit is reached the API returns HTTP 429.
BalldontlieService detects 429 explicitly and throws a clear message; the UI
reports that the rate limit has been exceeded. This is a limitation of the
free external API plan, not a bug in the application — the application handles
it gracefully rather than crashing.

9. Testing
The project contains FootballFanHub.Tests, an xUnit test project with
10 tests, all passing.

The tests deliberately do not call any external API. External services are
outside the developer's control and can fail because of rate limits, network
issues, provider changes or authentication — testing them would make the suite
slow and unreliable. Instead the tests cover logic written in this project:

FormCalculator

Home win returns W

Away loss returns L

Draw returns D

Missing score returns ?

Form results sorted newest-first

Result count limited correctly

JsonFavouritesRepository

Missing file returns an empty list

Adding a team saves it

A saved team can be found

Duplicate teams are rejected

Existing teams can be removed

Removing a missing team returns false

Invalid JSON returns an empty list

Tests use in-memory objects and temporary files, so they never modify real
application data.

10. Known limitations
NBA rate limit — the balldontlie free tier is rate limited; repeated
reloads can trigger HTTP 429 (handled gracefully, see section 8).

MLB removed — an MLB section was prototyped but removed because the
legacy MLB API returned HTTP 530 and was unreliable. The application still
uses two distinct providers, satisfying the brief.

No database — permitted by the brief; favourites use JSON file storage.

Unit tests cover core logic, not every page; API-dependent UI paths are
covered by manual demonstration.

11. References
(Replace the placeholder links below with the exact pages you actually used.)

football-data.org API documentation — https://www.football-data.org/documentation/quickstart

balldontlie API documentation — https://docs.balldontlie.io/

Microsoft Learn: Dependency injection in ASP.NET Core — https://learn.microsoft.com/aspnet/core/fundamentals/dependency-injection

Microsoft Learn: Make HTTP requests using IHttpClientFactory — https://learn.microsoft.com/dotnet/core/extensions/httpclient-factory

Microsoft Learn: Safe storage of app secrets in development (User Secrets) — https://learn.microsoft.com/aspnet/core/security/app-secrets

Microsoft Learn: Blazor interactive server rendering — https://learn.microsoft.com/aspnet/core/blazor/components/render-modes

Microsoft Learn: Unit testing C# with xUnit — https://learn.microsoft.com/dotnet/core/testing/unit-testing-with-dotnet-test

