using Nclex.Api.IntegrationTests.Infrastructure;

// One API host and one Postgres container for all tests in this assembly.
[assembly: AssemblyFixture(typeof(ApiFactory))]
