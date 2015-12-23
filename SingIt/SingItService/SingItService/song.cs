namespace SingItService
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("singitdb.song")]
    public partial class song
    {
        public song()
        {
            ratings = Utilities.JSON_EMPTY_OBJECT;
        }

        [Key]
        [StringLength(255)]
        public string songfilename { get; set; }

        [Required]
        [StringLength(16)]
        public string username { get; set; }

        [Required]
        [StringLength(255)]
        public string songtitle { get; set; }

        [Required]
        [StringLength(255)]
        public string songgenre { get; set; }

        [StringLength(1073741823)]
        public string ratings { get; set; }

        public virtual user user { get; set; }
    }
}
