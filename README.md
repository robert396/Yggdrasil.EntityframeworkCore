# Yggdrasil.EntityframeworkCore
Set of extensions to the EFCore package by Microsoft for .NET Core

## Seeding ##

### Warnings ###

Using seeding of data in this way can be a risky due to the fact that you have to make any and all seeders
be idempotent, but also if you are not using __Raw SQL__ to inject/modify data, then you have to make sure
that any changes you make to the data models, such as removing a property that existed in a previous migration
and you are using a seeder for that migration, then that seeder will break and will not be backwards compatible.

For easier maintenance it is prudent that you use the __Migration Builder__ in the migrations for seeding instead
of this way.

__Use at your own risk!__

### Getting Started ###

Using seeding with EF Core is nice and easy with only a single line of code being needed to be enabled!

```c#
public void ConfigureServices(IServiceCollection services)
{
    /* Removed for brevity */
    services.AddDbContext<ApplicationDbContext>(opts => {
        opts.UseSqlServer(_configuration.GetConnectionString("DefaultConnection"))
        opts.UseSeeding(); // The magic line 
    });
    /* Removed for brevity */
}
```

Once seeding has been configured in the `DbContextOptions` for the `DbContext` then any calls to migrate the database
via either of the following ways:

Method 1:
```c#
dbContext.Database.Migrate(); // Synchronous
// -- or --
dbContext.Database.MigrateAsync(); // Asynchronous
```

Method 2:
```c#
var migrator = dbContext.Database.GetService<IMigrator>();
migrator.Migrate(); // Synchronous
// -- or --
migrator.MigrateAsync(); // Asynchronous
```

Will create instances of any __*valid*__ seeder classes that exist in the __*same*__ assembly as the migrations.

### How to create a Seeder ###

To create a seeder is very simple.

__Step 1:__

Create a CLR class that implements the interface `Microsoft.EntityFramework.Core.IDbContextSeeder`, this will setup
the class to have the correct methods required.

__Step 2:__

Add the `Microsoft.EntityFrameworkCore.SeederForMigrationAttribute` attribute to the seeder class.

The attribute takes the migration id in as a parameter - NOTE: this does not need to be the full migration id such as `20181029083508_CreateUserSchema` it can just be
`CreateUserSchema`.

If you are using the __*full*__ migration id, the set the `UseFullMigrationId` parameter on the attribute to `true`.

If you need to have the seeders run in a specific order then you can set the `Order` property accordingly.

__Step 3:__

Add the `Microsoft.EntityFrameworkCore.Infrastructure.DbContextAttribute` attribute to the seeder class.

This requires the __type__ of the DbContext that this current seeder will be applied to e.g. `DbContext(typeof(ApplicationDbContext))`

#### Full Example ####

```c#
using ExampleApp.Contexts;
using ExampleApp.Migrations;
using ExampleApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ExampleApp.Seeders
{
    [SeederForMigration(nameof(CreateUserSchema)), DbContext(typeof(ApplicationDbContext))]
    public class UserSeeder : IDbContextSeeder
    {
        private readonly ApplicationDbContext _dbContext;

        public ValidSeeder(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task PreUpAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.CompletedTask;
        }

        public async Task PostUpAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var user = new User
            {
                Id = Guid.NewGuid().ToString(),
                Username = "admin",
                FirstName = "System",
                LastName = "Admin"
            };

            await _dbContext.Users.AddAsync(user, cancellationToken);
            var res = await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public Task PreDownAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.CompletedTask;
        }

        public Task PostDownAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.CompletedTask;
        }
    }
}
```

### Dependency Injection ###

As the seeders are created from `Dependency Injection` then any and all seeders that are found in the `Migrations Assembly`
need to be registered at the application level.

Example:

```c#
public void ConfigureServices(IServiceCollection services)
{
    /* Removed for brevity */
    services.AddScoped<CreateUsersSchemaSeeder>();
    /* Removed for brevity */
}
```

This can be done manually or via any service discovery mechanism that your chosen `DI` system supports such as `Autofac`, `Ninject` etc.

This can also be done by adding an extra package called [scrutor](https://github.com/khellang/Scrutor).

Simply add the nuget package into your application and then add the following code:

```c#
public void ConfigureServices(IServiceCollection services)
{
    /* Removed for brevity */
    services.Scan(scanner => {
        scanner.FromAssemblyOf<ApplicationDbContext>().AddClasses(filter => {
            filter.WithAttribute<SeederForMigrationAttribute>();
        }).AsSelf().WithScopedLifetime(); // Change the Lifetime to suit your requirements
    });
    /* Removed for brevity */
}
```