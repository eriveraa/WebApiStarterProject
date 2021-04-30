using Microsoft.Extensions.DependencyInjection;
using EMT.BLL.Services;

namespace EMT.BLL
{
    /// <summary>
    /// En esta clase se registran en el container todas las Clases y sus respectivas interfaces para que
    /// puedan ser inyectadas mediante DI.
    /// </summary>
    public static class ServiceRegistrationExtensions
    {
        public static void RegisterBLLServices(this IServiceCollection services)
        {
            services.AddScoped<IMyNoteService, MyNoteService>();

            // Additional services here...
        }
    }
}
