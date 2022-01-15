using EnlaceVA.Adapters;
using EnlaceVA.Authentication;
using EnlaceVA.Bots;
using EnlaceVA.Dialogs;
using EnlaceVA.Services;
using EnlaceVA.TokenExchange;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.ApplicationInsights;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Integration.ApplicationInsights.Core;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.Integration.AspNet.Core.Skills;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Solutions.Responses;
using Microsoft.Bot.Solutions.Skills;
using Microsoft.Bot.Solutions.Skills.Dialogs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using EnlaceVA.DialogsCards;

namespace EnlaceVA
{
    public class Startup
    {
        public Startup(IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddJsonFile("cognitivemodels.json", optional: true)
                .AddJsonFile($"cognitivemodels.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddNewtonsoftJson();

            //var str = Properties.Resources.



            var settings = new BotSettings();
            Configuration.Bind(settings);
            services.AddSingleton(settings);

            services.AddSingleton<IChannelProvider, ConfigurationChannelProvider>();

            services.AddSingleton<ICredentialProvider, ConfigurationCredentialProvider>();

            var skillsConfig = new SkillsConfiguration(Configuration);
            services.AddSingleton(skillsConfig);

            services.AddSingleton(sp => new AuthenticationConfiguration { ClaimsValidator = new AllowedCallersClaimsValidator(skillsConfig) });

            services.AddApplicationInsightsTelemetry();
            services.AddSingleton<IBotTelemetryClient, BotTelemetryClient>();
            services.AddSingleton<ITelemetryInitializer, OperationCorrelationTelemetryInitializer>();
            services.AddSingleton<ITelemetryInitializer, TelemetryBotIdInitializer>();
            services.AddSingleton<TelemetryInitializerMiddleware>();
            services.AddSingleton<TelemetryLoggerMiddleware>();

            services.AddSingleton<BotServices>();

            services.AddSingleton<IStorage>(new CosmosDbPartitionedStorage(settings.CosmosDb));
            services.AddSingleton<UserState>();
            services.AddSingleton<ConversationState>();            

            services.AddSingleton<TransactionalServices>();

            var localizedTemplates = new Dictionary<string, string>();
            var templateFile = "AllResponses";
            var supportedLocales = new List<string>() { "es-es"};

            foreach (var locale in supportedLocales)
            {
                var localeTemplateFile = locale.Equals("en-us")
                    ? Path.Combine(".", "Responses", $"{templateFile}.lg")
                    : Path.Combine(".", "Responses", $"{templateFile}.{locale}.lg");

                localizedTemplates.Add(locale, localeTemplateFile);
            }

            services.AddSingleton(new LocaleTemplateManager(localizedTemplates, settings.DefaultLocale ?? "es-es"));

            services.AddSingleton<BotFrameworkHttpAdapter, DefaultAdapter>();
            services.AddSingleton<BotAdapter>(sp => sp.GetService<BotFrameworkHttpAdapter>());

            services.AddSingleton<SkillConversationIdFactoryBase, SkillConversationIdFactory>();
            services.AddHttpClient<SkillHttpClient>();
            services.AddSingleton<ChannelServiceHandler, TokenExchangeSkillHandler>();

            services.AddTransient<MainDialog>();
            services.AddTransient<SwitchSkillDialog>();
            services.AddTransient<OnboardingDialog>();
            services.AddTransient<FeedBack>();           
            services.AddTransient<TratamientoDialog>();
            services.AddTransient<ConstaciaPagoDialog>();
            services.AddTransient<TrasladoDeudaDialog>();
            services.AddTransient<ExencionContribucionDialog>();
            services.AddTransient<CambioNombreDialog>();
            services.AddTransient<TerminacionContratoDialog>();
            services.AddTransient<EmergenciasDialog>();
            services.AddTransient<ConceptoFacturaDialog>();
            services.AddTransient<EstadoCuentaDialog>();
            services.AddTransient<TomaLecturaDialog>();
            services.AddTransient<ConsultaPQRdialog>();
            services.AddTransient<ConceptoFacturaDialogCard>();
            services.AddTransient<ConsumoFacturadoDialogCard>();
            services.AddTransient<ConsumoFacturadoDialog>();
            services.AddTransient<PagoParcialDialog>();
            services.AddTransient<ConsultaPQRDialogCard>();
            services.AddTransient<TomalecturaDialogCard>();
            services.AddTransient<EstadoCuentaDialogCard>();
            services.AddTransient<PagoParcialDialogCard>();
            services.AddTransient<VentaSegurosBrillaDialog>();
            services.AddTransient<VentaSegurosBrillaDialogCard>();
            services.AddTransient<VisitaFinanciacionDialog>();
            services.AddTransient<VisitaFinanciacionDialogCard>();
            services.AddTransient<ReconexionPorPagoDialog>();
            services.AddTransient<ReconexionPorPagoDialogCard>();
            services.AddTransient<ConsultaCupoBrillaDialog>();
            services.AddTransient<ConsultaCupoBrillaDialogCard>();


            var botId = Configuration.GetSection(MicrosoftAppCredentials.MicrosoftAppIdKey)?.Value;
            if (string.IsNullOrWhiteSpace(botId))
            {
                throw new ArgumentException($"{MicrosoftAppCredentials.MicrosoftAppIdKey} is not in configuration");
            }

            foreach (var skill in skillsConfig.Skills.Values)
            {
                services.AddSingleton(sp =>
                {
                    var skillDialogOptions = new SkillDialogOptions
                    {
                        BotId = botId,
                        ConversationIdFactory = sp.GetService<SkillConversationIdFactoryBase>(),
                        SkillClient = sp.GetService<SkillHttpClient>(),
                        SkillHostEndpoint = skillsConfig.SkillHostEndpoint,
                        Skill = skill,
                        ConversationState = sp.GetService<ConversationState>()
                    };

                    return new SkillDialog(skillDialogOptions, skill.Id);
                });
            }

            if (settings.TokenExchangeConfig != null)
            {
                services.AddSingleton<ITokenExchangeConfig>(settings.TokenExchangeConfig);
            }

            services.AddTransient<IBot, DefaultActivityHandler<MainDialog>>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseWebSockets()
                .UseRouting()
                .UseEndpoints(endpoints => endpoints.MapControllers());

        }
    }
}