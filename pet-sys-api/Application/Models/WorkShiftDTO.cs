using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Application.Models
{
    public class WorkShiftDTO
    {
        [Required]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DayOfWeek Day { get; set; }
        [Required]
        public TimeOnly StartTime { get; set; }
        [Required]
        public TimeOnly EndTime { get; set; }
    }
}
