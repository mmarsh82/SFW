using System.ComponentModel.DataAnnotations;

namespace SFW.DataAccess.Model
{
    public class Machine
    {
        #region Properties

        [Required]
        public int ID { get; set; }

        [Required]
        [MaxLength(20)]
        public string Name { get; set; }

        [Required]
        [MaxLength(100)]
        public string Description { get; set; }

        [Required]
        [MaxLength(20)]
        public string Group { get; set; }

        #endregion
    }
}
