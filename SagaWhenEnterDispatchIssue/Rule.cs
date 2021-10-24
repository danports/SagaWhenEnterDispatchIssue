using Automatonymous;
using System;
using System.ComponentModel.DataAnnotations;

namespace SagaWhenEnterDispatchIssue
{
    public class Rule : SagaStateMachineInstance
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public Guid CorrelationId { get; set; }
        [MaxLength(50)] public string CurrentState { get; set; } = "Initial";
        public Guid? ExecutionRequestId { get; set; }
        public DateTime? ExecutionRequestedAt { get; set; }
        [Timestamp] public byte[] Version { get; set; }

        public string Format() => $"Rule (#{Id} {CorrelationId})";
    }
}
