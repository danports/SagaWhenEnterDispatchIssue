using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace SagaWhenEnterDispatchIssue
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureHostConfiguration(configHost => configHost.AddCommandLine(args))
                .ConfigureAppConfiguration((hostContext, configApp) => configApp.AddJsonFile("appsettings.json", true, true))
                .ConfigureLogging((context, logging) =>
                {
                    logging.AddConfiguration(context.Configuration.GetSection("Logging"));
                    logging.AddConsole();
                    logging.AddDebug();
                    logging.AddConfiguration(context.Configuration);
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddDbContext<ApplicationDbContext>(options =>
                    {
                        options.UseMySql("server=localhost;port=3307;userid=root;pwd=example;database=testing", new MySqlServerVersion("8.0.23")).EnableSensitiveDataLogging();
                    });
                    services.AddMassTransit(x =>
                    {
                        x.AddConsumer<ExecuteRuleConsumer, ExecuteRuleConsumerDefinition>();

                        x.AddSagaStateMachine<RuleStateMachine, Rule, RuleSagaDefinition>()
                            .EntityFrameworkRepository(r =>
                            {
                                r.ExistingDbContext<ApplicationDbContext>();
                                r.ConcurrencyMode = ConcurrencyMode.Optimistic;
                            });

                        x.SetEndpointNameFormatter(new DefaultEndpointNameFormatter("Development_", false));
                        x.UsingAmazonSqs((registration, sqs) =>
                        {
                            sqs.Host("us-east-1", h =>
                            {
                                h.AccessKey("MYACCESSKEY");
                                h.SecretKey("MYSECRETKEY");
                                h.Scope("Development");
                                h.EnableScopedTopics();
                            });
                            sqs.ConfigureEndpoints(registration);
                            sqs.UseDelayedMessageScheduler();
                        });
                    });
                })
                .UseConsoleLifetime()
                .Build();

            Console.WriteLine("Starting host...");
            var run = host.RunAsync();

            var bus = host.Services.GetRequiredService<IBusControl>();
            await bus.WaitForHealthStatus(BusHealthStatus.Healthy, TimeSpan.FromMinutes(1));
            Console.WriteLine("Bus ready!");

            var context = host.Services.GetRequiredService<ApplicationDbContext>();
            await context.Database.EnsureCreatedAsync();

            for (int i = 0; i < 5; i++)
                context.Rules.Add(new Rule { CorrelationId = NewId.NextGuid(), CategoryId = 5, CurrentState = "Waiting" });
            await context.SaveChangesAsync();

            Console.WriteLine("Publishing events...");
            for (int i = 0; i < 20; i++)
            {
                await bus.Publish<CategoryUpdated>(new { CategoryId = 5 });
                await Task.Delay(2000);
            }

            await run;
        }
    }
}
