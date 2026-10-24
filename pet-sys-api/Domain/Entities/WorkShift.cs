using System;
using System.Text.Json.Serialization;

namespace Domain.Entities
{
    public record WorkShift
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DayOfWeek Day { get; init; }
        public TimeOnly StartTime { get; init; }
        public TimeOnly EndTime { get; init; }
    }
}
