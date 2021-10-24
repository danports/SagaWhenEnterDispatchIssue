using GreenPipes;
using MassTransit;
using MassTransit.ConsumeConfigurators;
using MassTransit.Definition;
using System;

namespace SagaWhenEnterDispatchIssue
{
    public class ExecuteRuleConsumerDefinition : ConsumerDefinition<ExecuteRuleConsumer>
    {
        protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
            IConsumerConfigurator<ExecuteRuleConsumer> consumerConfigurator) =>
            consumerConfigurator.UseMessageRetry(retry => retry.Interval(3, TimeSpan.FromSeconds(10)));
    }
}
