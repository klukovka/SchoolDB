namespace SchoolDB.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Teachers
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Teachers()
        {
            Forms = new HashSet<Forms>();
            Teacher_Subject = new HashSet<Teacher_Subject>();
            SubjectForms = new Dictionary<string, List<string>>();
        }

        [Key]
        public int Teacher_id { get; set; }

        [Required]
        [StringLength(50)]
        public string Teacher_name { get; set; }

        public int? Teacher_room { get; set; }

        [StringLength(50)]
        public string Teacher_sex { get; set; }

        [StringLength(20)]
        public string Category_id { get; set; }

        [StringLength(50)]
        public string Teacher_post { get; set; }

        [StringLength(50)]
        public string Teacher_password { get; set; }

        public virtual Categories Categories { get; set; }

        [Column(TypeName = "date")]
        public DateTime? Teacher_birthday { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Forms> Forms { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Teacher_Subject> Teacher_Subject { get; set; }

        public Dictionary<string, List<string>> SubjectForms;
    }
}
