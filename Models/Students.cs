namespace SchoolDB.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Students
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Students()
        {
            Marks = new HashSet<Marks>();
        }

        [Key]
        public int Student_id { get; set; }

        [Required]
        [StringLength(50)]
        public string Student_name { get; set; }

        [StringLength(5)]
        public string Student_form { get; set; }

        [Column(TypeName = "date")]
        public DateTime? Student_birthday { get; set; }

        [StringLength(50)]
        public string Student_sex { get; set; }

        [StringLength(50)]
        public string Student_adress { get; set; }

        [StringLength(50)]
        public string Privilege_id { get; set; }

        public bool? Student_dinning { get; set; }

        [StringLength(50)]
        public string Student_post { get; set; }

        [StringLength(50)]
        public string Student_post_parents { get; set; }

        [StringLength(50)]
        public string Student_password { get; set; }

        public virtual Forms Forms { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Marks> Marks { get; set; }

        public virtual Privileges Privileges { get; set; }
    }
}
