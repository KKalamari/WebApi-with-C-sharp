using System.ComponentModel.DataAnnotations;
namespace MyWebApi.Models
{
    public record Data
    {
        public string? Name{ get; init; }
        public double Latitude{ get; init; }
        public double Longitude{ get; init; }
    }
}