using MassTransit;
using System;
using System.ComponentModel.DataAnnotations;

namespace SagaWhenEnterDispatchIssue
{
    public class Rule : SagaStateMachineInstance
    {
        [Key] public Guid CorrelationId { get; set; }
        public int CategoryId { get; set; }
        [MaxLength(50)] public string CurrentState { get; set; } = "Initial";
        public Guid? ExecutionRequestId { get; set; }
        public DateTime? ExecutionRequestedAt { get; set; }
        [Timestamp] public byte[] Version { get; set; }

        public string Format() => $"Rule ({CorrelationId})";
    }
}
