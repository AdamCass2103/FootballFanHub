# FootballFanHub

A sports-centre web application built with **Blazor Server** and **.NET 10** (C#).
It combines **football** and **NBA basketball** data from two different third-party
web service providers into a single application.

---

## Contents

1. [What the application does](#1-what-the-application-does)
2. [Third-party APIs used](#2-third-party-apis-used)
3. [Technologies](#3-technologies)
4. [Project structure](#4-project-structure)
5. [How to run](#5-how-to-run)
6. [API key setup (User Secrets)](#6-api-key-setup-user-secrets)
7. [Design overview](#7-design-overview)
8. [Error handling and API limits](#8-error-handling-and-api-limits)
9. [Testing](#9-testing)
10. [Known limitations](#10-known-limitations)
11. [References](#11-references)

---

## 1. What the application does

FootballFanHub is a **sports centre**: one application where a user can browse
football and NBA data side by side, and follow their favourite football teams.

**Football (football-data.org)**

- Search for clubs (filtered in memory with LINQ)
- View team details: crest, venue, founded year, club colours, manager, squad
- View recent form, upcoming fixtures and recent results
- View league standings (Premier League, La Liga, Serie A, Bundesliga, Ligue 1)
- Follow and unfollow teams. Followed teams appear on the home page with their
  next fixture

**NBA (balldontlie)**

- Browse NBA teams (logos loaded from ESPN's image CDN)
- Search NBA teams by name, city or abbreviation
- View team details, games and player statistics
- Team record calculated from the returned game scores

The application is **contextualised as a sports centre**. It is not a demo of
web services: the two providers are combined to serve one user-facing purpose,
with a shared navigation menu and a Football section and an NBA section.

---

## 2. Third-party APIs used

| Provider | Purpose | Documentation |
|---|---|---|
| **football-data.org** | Football teams, fixtures, results, standings | https://www.football-data.org/documentation/quickstart |
| **balldontlie** | NBA teams, games, player statistics | https://docs.balldontlie.io/ |

NBA team logos are images served from ESPN's public CDN
(`https://a.espncdn.com/i/teamlogos/nba/500/{abbreviation}.png`). This is an
image CDN, **not** a third data API. All NBA *data* comes from balldontlie.

---

## 3. Technologies

- C# / .NET 10
- Blazor Server (interactive server rendering over SignalR)
- Razor components, HTML, Bootstrap CSS
- `HttpClient` (typed clients via `AddHttpClient` for the football services)
- `System.Text.Json` for deserialisation
- Dependency Injection and interfaces
- async / await
- JSON file persistence
- xUnit for unit testing

No database is used. The assignment permits file-based storage, so followed
teams are stored in `Data/favourites.json`.

---

## 4. Project structure

```text
FootballFanHub
├── Components
│   ├── Layout          NavMenu, MainLayout
│   ├── Pages           Home, Search, Team, Standings, NBA pages
│   └── App.razor
├── Models              Strongly-typed classes mirroring the API JSON
├── Services            Interfaces + implementations for APIs and storage
├── Data                favourites.json (persisted followed teams)
├── CacheData           Cached API responses
├── Program.cs          Startup, DI registration, HttpClient configuration
└── appsettings.json

FootballFanHub.Tests    xUnit test project
```

---

## 5. How to run

**Requirements:** the .NET 10 SDK and Visual Studio (or the `dotnet` CLI).

1. Clone the repository.
2. Configure the two API keys (see section 6). **This is required**, because the
   keys are not stored in the repository.
3. Open the solution in Visual Studio, set `FootballFanHub` as the startup
   project and run it (`F5`). With the CLI, run `dotnet run --project FootballFanHub`.
4. Open the address shown in the console (normally `https://localhost:7156`).
5. Use the shared navigation menu to move between the Football and NBA sections.

To run the tests: `dotnet test`

---

## 6. API key setup (User Secrets)

API keys are **not** stored in source code. They are kept in **User Secrets**,
which stores development credentials outside the project folder so they cannot
be committed to Git. Register for a free key with each provider (see the
documentation links in section 2).

The project has a `UserSecretsId` in its `.csproj`. From the `FootballFanHub`
project folder, set the keys with:

```bash
dotnet user-secrets set "FootballData:ApiKey" "YOUR_FOOTBALL_DATA_KEY"
dotnet user-secrets set "Balldontlie:ApiKey" "YOUR_BALLDONTLIE_KEY"
```

Alternatively, in Visual Studio right-click the project, choose
**Manage User Secrets**, and add:

```json
{
  "FootballData:ApiKey": "YOUR_FOOTBALL_DATA_KEY",
  "Balldontlie:ApiKey": "YOUR_BALLDONTLIE_KEY"
}
```

The football base URL (`FootballData:BaseUrl`) is not secret and is read from
`appsettings.json`.

---

## 7. Design overview

The application follows a layered structure:

```text
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
```

Pages never call APIs or touch the file system directly. They inject interfaces,
and `Program.cs` supplies the implementations through Dependency Injection. This
keeps the UI decoupled from external systems and makes the logic easy to replace
or mock for testing.

Key design points:

- **Interfaces and DI.** Pages depend on `ITeamService` and similar interfaces,
  not on the concrete classes.
- **Typed HttpClients.** The football services are registered with
  `AddHttpClient`, sharing one configuration block for the base URL,
  authentication header and timeout (DRY).
- **Generic helper.** `BalldontlieService` uses a `GetAsync<T>` helper, so the
  HTTP request and JSON parsing logic is written once and reused for every
  endpoint (DRY).
- **Repository pattern.** Favourites are hidden behind `IFavouritesRepository`.
  `JsonFavouritesRepository` performs the file operations, including duplicate
  prevention, missing-file handling and invalid-JSON recovery. It uses
  `SemaphoreSlim` so two simultaneous writes cannot corrupt the file.
- **Pure logic.** `FormCalculator` converts match data into W/D/L form. It
  does not depend on any API, which makes it straightforward to unit test.
- **Caching.** Responses are cached so the same data is not requested
  repeatedly, which reduces the number of API calls and helps stay inside the
  free-tier limits.
- **Model classes.** Strongly-typed models mirror the API JSON and are converted
  into the application's own models, so the pages do not depend on the exact
  shape of a provider's response.

---

## 8. Error handling and API limits

Every service call checks the HTTP response. Unsuccessful responses are turned
into exceptions with a clear message, and each Razor page catches them and shows
a user-friendly error instead of crashing or showing a blank page. Pages also
display loading states while data is in flight.

**Rate limits.** Both providers' free tiers limit how many requests can be made
(football-data.org allows 10 requests per minute and delays scores, so there are
no live scores). When a limit is reached the API returns HTTP 429. The services
detect this explicitly and the UI reports that the limit was reached. This is a
limitation of the free plans, not a bug: the application handles it gracefully
instead of crashing.

---

## 9. Testing

The solution contains `FootballFanHub.Tests`, an xUnit test project. Run it with
`dotnet test`.

The tests deliberately **do not call any external API**. External services are
outside the developer's control and can fail because of rate limits, network
problems, provider changes or authentication, so testing them would make the
suite slow and unreliable. The tests cover logic written in this project.

**FormCalculator**

- A home win returns `W`
- An away loss returns `L`
- A draw returns `D`
- A missing score returns `?`
- Form results are sorted newest first
- The number of results returned is limited correctly

**JsonFavouritesRepository**

- A missing file returns an empty list
- Adding a team saves it
- A saved team can be found
- Duplicate teams are rejected
- An existing team can be removed
- Removing a team that is not saved returns `false`
- Invalid JSON returns an empty list

The tests use in-memory objects and temporary files, so they never modify real
application data.

---

## 10. Known limitations

- **API rate limits.** Both free tiers are rate limited. Repeatedly reloading
  pages can trigger HTTP 429 (handled gracefully, see section 8).
- **No live scores.** The football-data.org free tier delays scores.
- **MLB removed.** An MLB section was prototyped but removed because the legacy
  MLB API returned HTTP 530 and was unreliable. The application still uses two
  distinct providers, as the brief requires.
- **No database.** The brief permits this. Favourites use JSON file storage, so
  they are shared by everyone using the same running instance.
- **Test coverage.** The unit tests cover the core logic, not every page.
  API-dependent pages are covered by manual demonstration.

---

## 11. References

Where code was adapted from an external source, it is credited in a comment in
the class that uses it.

- football-data.org API documentation: https://www.football-data.org/documentation/quickstart
- balldontlie API documentation: https://docs.balldontlie.io/
- Microsoft Learn, Dependency injection in ASP.NET Core: https://learn.microsoft.com/aspnet/core/fundamentals/dependency-injection
- Microsoft Learn, Make HTTP requests using IHttpClientFactory: https://learn.microsoft.com/dotnet/core/extensions/httpclient-factory
- Microsoft Learn, Safe storage of app secrets in development: https://learn.microsoft.com/aspnet/core/security/app-secrets
- Microsoft Learn, Blazor render modes: https://learn.microsoft.com/aspnet/core/blazor/components/render-modes
- Microsoft Learn, Unit testing C# with xUnit: https://learn.microsoft.com/dotnet/core/testing/unit-testing-with-dotnet-test