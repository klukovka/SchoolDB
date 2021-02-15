namespace SchoolDB.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Teacher_Subject
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Teacher_Subject()
        {
            Marks = new HashSet<Marks>();
            Timetable = new HashSet<Timetable>();
        }

        [Key]
        public int Teacher_Subject_id { get; set; }

        public int Teacher_id { get; set; }

        public bool Teach { get; set; }

        [Required]
        [StringLength(50)]
        public string Subject { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Marks> Marks { get; set; }

        public virtual Teachers Teachers { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Timetable> Timetable { get; set; }
    }
}
