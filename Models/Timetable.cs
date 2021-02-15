namespace SchoolDB.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Timetable")]
    public partial class Timetable
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(10)]
        public string Day_Of_Week { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Lesson_id { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(5)]
        public string Form_id { get; set; }

        public int Classroom { get; set; }

        public int Teacher_Subject_id { get; set; }

        public virtual Bells Bells { get; set; }

        public virtual Forms Forms { get; set; }

        public virtual Teacher_Subject Teacher_Subject { get; set; }

        public string Day_UA
        {
            get
            {
                string res = "";
                switch (Day_Of_Week)
                {
                    case "Monday":
                        res = "Понеділок";
                        break;
                    case "Tuesday":
                        res = "Вівторок";
                        break;
                    case "Wednesday":
                        res = "Середа";
                        break;
                    case "Thursday":
                        res = "Четвер";
                        break;
                    case "Friday":
                        res = $"Пятниця";
                        break;
                    default:
                        res = "";
                        break;
                }
                return res;
            }
        }

        public void Day_EN()
        {
            switch (Day_UA)
            {
                case "Понеділок":
                    Day_Of_Week = "Monday";
                    break;
                case "Вівторок":
                    Day_Of_Week = "Tuesday";
                    break;
                case "Середа":
                    Day_Of_Week = "Wednesday";
                    break;
                case "Четвер":
                    Day_Of_Week = "Thursday";
                    break;
                case "Пятниця":
                    Day_Of_Week = "Friday";
                    break;
                default:
                    Day_Of_Week = "";
                    break;
            }
            
        }
    }
}
