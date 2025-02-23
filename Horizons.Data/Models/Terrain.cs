using System.ComponentModel.DataAnnotations;
using Horizons.Data.Common;

namespace Horizons.Data.Models
{
    public class Terrain
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(ValidationConstants.TerrainNameMaxLength)]
        public string Name { get; set; } = null!;

        public virtual ICollection<Destination> Destinations { get; set; } = new HashSet<Destination>();
    }
}
