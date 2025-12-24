# Wicked MMO Server

Server-authoritative MMO server (Tibia-inspired) built with C#/.NET 9.  
WebSocket networking via Fleck, PostgreSQL persistence via Npgsql + Dapper, and secure password hashing with BCrypt.

## Status
This project is under active development.

## Tech Stack
- **Language:** C#
- **Runtime:** .NET 9 (`net9.0`)
- **Networking:** Fleck (WebSocket)
- **Database:** PostgreSQL (Npgsql) + Dapper
- **Auth:** BCrypt.Net

## Getting Started

### Requirements
- .NET SDK 9
- PostgreSQL

### Run
```bash
dotnet restore
dotnet build -c Release
dotnet run
```

## Project Structure

### Example:
- core/ - game core
- Network/ - packets, protocols and managers
- docs/ - protocol + architecture docs
- Database/ - migrations / schema scripts
- .github/workflows/ - CI

### Documentation

- docs/protocol.md — message formats, versions, examples
- docs/architecture.md — server loop, world ownership, components

## Contributing

- PRs are welcome. Please:
- Create a feature branch
- Open a PR into main
- Ensure CI passes on Windows + Ubuntu

## 4 MIT License (LICENSE)