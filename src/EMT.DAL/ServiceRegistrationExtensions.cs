using Microsoft.Extensions.DependencyInjection;
using EMT.DAL.EF;
using EMT.DAL.Interfaces;

namespace EMT.DAL
{
    public static class ServiceRegistrationExtensions
    {
        public static void RegisterDALServices(this IServiceCollection services)
        {
            // ------------------------------------------------------------------------------------
            // Dependency Injection

            // Conexión a BD mediante Factory
            //services.AddScoped<IConnectionFactory, SQLConnectionFactory>();

            // Unity of Work
            services.AddScoped<IUnityOfWork_EF, UnityOfWork_EF>();

            // Repositories
            services.AddScoped(typeof(IGenericAsyncRepository<>), typeof(GenericAsyncEFRepository<>));
        }
    }
}
