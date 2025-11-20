InitializeDb (scaffold)

This folder contains a minimal scaffold for the database initializer described in `solution.plan.md`.

Tasks to complete:

- Add a .NET project file (targeting net8.0) and reference the ApplicationCore and Infrastructure projects.
- Add NHibernate packages and implement `NHibernateHelper` to build a SessionFactory from `NHibernate.cfg.xml`.
- Register repository implementations and IUnitOfWork in an IServiceCollection and use them to run SchemaExport.
- Optionally implement the seed using `ApplicationCore.Domain.CEN` classes and `ApplicationCore.Domain.CP.InitializeDbCP`.

PowerShell (example) to run once project is created:

```powershell
Push-Location .\InitializeDb
dotnet run --project .\InitializeDb.csproj
Pop-Location
```
