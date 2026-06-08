using System;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;

namespace EXPLORADORDEARCHIVOS_TERMINADO
{
    internal static class Program
    {
        public static IServiceProvider ServiceProvider { get; private set; }
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            var services = new ServiceCollection();

            // Registrar servicios y repositorios
            services.AddSingleton<Services.AudioService>();
            services.AddSingleton<Data.DataRepository>();
            services.AddSingleton<Services.DataService>();

            // Registrar DefaultMediaFactory pasando el ServiceProvider
            services.AddSingleton<Services.DefaultMediaFactory>(sp => new Services.DefaultMediaFactory(sp));

            // Componentes extraídos
            services.AddSingleton<Interfaces.IFileNavigator, Services.FileNavigator>();
            services.AddSingleton<Interfaces.IIconProvider>(sp => new Services.IconProvider(sp.GetRequiredService<Services.ImageListProvider>().ImageList));
            services.AddSingleton<Interfaces.IFormManager, Services.FormManager>();
            services.AddSingleton<Interfaces.IMediaManager, Services.MultiMediaPlayer>();

            // Registrar proveedor de ImageList (adaptador pequeño)
            services.AddSingleton<Services.ImageListProvider>();

            // Formularios
            services.AddTransient<FormDataBase>();
            services.AddTransient<FormMP3>();
            services.AddTransient<FormMP4>();
            services.AddTransient<FormCorrector>();
            services.AddTransient<FormEdit>();
            services.AddTransient<FormGrabadora>();
            services.AddTransient<FormEditarFotos>();
            services.AddTransient<Form1>();

            var provider = services.BuildServiceProvider();

            // Exponer el service provider globalmente para casos UI que lo necesiten
            ServiceProvider = provider;

            Application.Run(provider.GetRequiredService<Form1>());
        }
    }
}