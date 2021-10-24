using System;

namespace SagaWhenEnterDispatchIssue
{
    public interface ExecuteRule
    {
        public Guid RuleId { get; set; }
    }
}
