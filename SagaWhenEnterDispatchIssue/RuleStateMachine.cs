using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace SagaWhenEnterDispatchIssue
{
    public class RuleStateMachine : MassTransitStateMachine<Rule>
    {
        public RuleStateMachine(ILogger<RuleStateMachine> logger)
        {
            InstanceState(x => x.CurrentState);

            Request(() => Execute, x => x.ExecutionRequestId, r =>
            {
                r.ServiceAddress = new Uri($"loopback://localhost/ExecuteRule");
                r.Timeout = TimeSpan.Zero;
            });

            Event(() => CategoryUpdated, evt => evt
                .CorrelateBy((instance, context) => context.Message.CategoryId == instance.CategoryId)
                .OnMissingInstance(x => x.Discard()));

            During(new[] { Waiting },
                When(CategoryUpdated)
                    .Then(context => logger.LogInformation("Category updated for {Rule}; starting execution...", context.Saga.Format()))
                    .TransitionTo(StartingExecution)
            );

            WhenEnter(StartingExecution, x => x
                .ThenAsync(async context =>
                {
                    logger.LogInformation("Starting execution for {Rule}...", context.Saga.Format());
                    context.Saga.ExecutionRequestedAt = DateTime.UtcNow;
                    await Task.Delay(new Random().Next(50, 1000));
                })
                .Request(Execute, context => context.Init<ExecuteRule>(new { RuleId = context.Saga.CorrelationId }))
                .TransitionTo(Executing)
            );

            During(Executing,
                When(Execute.Completed)
                    .Then(context =>
                    {
                        logger.LogInformation("Executed {Rule}.", context.Saga.Format());
                        context.Saga.ExecutionRequestedAt = null;
                    })
                    .TransitionTo(Waiting),
                When(Execute.Faulted)
                    .Then(context =>
                    {
                        logger.LogError("Failed to execute {Rule}; retrying later.", context.Saga.Format());
                        context.Saga.ExecutionRequestedAt = null;
                    })
                    .TransitionTo(Waiting)
            );
        }

        public State StartingExecution { get; set; }
        public State Executing { get; set; }
        public State Waiting { get; set; }

        public Event<CategoryUpdated> CategoryUpdated { get; set; }
        public Request<Rule, ExecuteRule, ExecuteRuleResponse> Execute { get; set; }
    }
}
