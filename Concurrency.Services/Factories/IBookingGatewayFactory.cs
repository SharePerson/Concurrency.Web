using System;

namespace Concurrency.Services.Factories
{
    public interface IBookingGatewayFactory<InstanceType> where InstanceType : IAsyncDisposable
    {
        InstanceType Create();
        InstanceType CreateSqlite();
        InstanceType CreateScoped();
    }
}
