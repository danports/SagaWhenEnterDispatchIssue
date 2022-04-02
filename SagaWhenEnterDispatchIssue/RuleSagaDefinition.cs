using MassTransit;
using System;

namespace SagaWhenEnterDispatchIssue
{
    public class RuleSagaDefinition : SagaDefinition<Rule>
    {
        protected override void ConfigureSaga(IReceiveEndpointConfigurator endpointConfigurator, ISagaConfigurator<Rule> sagaConfigurator)
        {
            endpointConfigurator.UseMessageRetry(retry => retry.Interval(5, TimeSpan.FromSeconds(5)));
            endpointConfigurator.UseInMemoryOutbox();
        }
    }
}
