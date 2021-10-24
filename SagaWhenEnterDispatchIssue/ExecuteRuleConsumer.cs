using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace SagaWhenEnterDispatchIssue
{
    public class ExecuteRuleConsumer : IConsumer<ExecuteRule>
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ExecuteRuleConsumer> _logger;

        public ExecuteRuleConsumer(ApplicationDbContext context, ILogger<ExecuteRuleConsumer> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<ExecuteRule> context)
        {
            var rule = await _context.Rules.SingleOrDefaultAsync(x => x.CorrelationId == context.Message.RuleId);

            _logger.LogInformation("Executing {Rule}: current state {CurrentState}, request ID {RequestId}",
                rule.Format(), rule.CurrentState, rule.ExecutionRequestId);

            await Task.Delay(1000);

            await context.RespondAsync<ExecuteRuleResponse>(new { RuleId = rule.CorrelationId });
            _logger.LogInformation("Completed executing {Rule}", rule.Format());
        }
    }
}
