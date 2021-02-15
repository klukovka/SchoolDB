namespace SchoolDB.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Marks
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Student_id { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Teacher_Subject_id { get; set; }

        public int Mark { get; set; }

        [Key]
        [Column(Order = 2, TypeName = "date")]
        public DateTime Date_create { get; set; }

        [Key]
        [Column(Order = 3)]
        [StringLength(20)]
        public string Work_id { get; set; }

        public virtual Students Students { get; set; }

        public virtual Teacher_Subject Teacher_Subject { get; set; }

        public virtual Types_of_work Types_of_work { get; set; }
    }
}
