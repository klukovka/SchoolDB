using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SchoolDB.Models;
using SautinSoft.Document;
using System.IO;
using System.Drawing;
using Xceed.Words.NET;
using Xceed.Document.NET;

namespace SchoolDB.Controllers
{
    public class StudentController : Controller
    {

        SchoolContext db = new SchoolContext();
        SqlConnection connection = new SqlConnection(new Connection().ConnectionString);
        SqlCommand command;
        SqlDataReader reader;
        // GET: Student
        public ActionResult Index()
        {
            return RedirectToAction("Student");
        }
      
        public ActionResult Student()
        {
            Students student = new Students();
            using (connection)
            {
                connection.Open();
                command = new SqlCommand("SELECT * FROM Students WHERE Student_id ="+Session["name"], connection);
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    student.Student_id = Convert.ToInt32(reader.GetValue(0));
                    student.Student_name = reader.GetString(1);
                    student.Student_form = reader.GetString(2);
                    student.Student_birthday = Convert.ToDateTime(reader.GetValue(3));
                    student.Student_sex = reader.GetString(4);
                    student.Student_adress = reader.GetValue(5).ToString();
                    student.Privilege_id = reader.GetValue(6).ToString();
                    student.Student_dinning = (bool?)reader.GetValue(7);
                    student.Student_post = reader.GetString(8);
                    student.Student_post_parents = reader.GetValue(9).ToString();
                    student.Student_password = reader.GetString(10);
                }
                reader.Close();
            }
            ViewBag.Student = student;

            return View();
        }

        public ActionResult Eating()
        {
            Dictionary<string, object> eating = new Dictionary<string, object>();
            using (connection)
            {
                connection.Open();
                command = new SqlCommand("SELECT S.Student_name, S.Privilege_id, 30 AS Full_Payment, " +
                    "(30 * (100 - P.Sale) / 100) AS Payment, P.Sale FROM Students S " +
                    "INNER JOIN Privileges P ON S.Privilege_id = P.Privilege_id " +
                    "WHERE S.Student_id = " + Session["name"], connection);
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    eating.Add("Student_id", reader.GetValue(0).ToString());
                    eating.Add("Privalage_id", reader.GetValue(1).ToString());
                    eating.Add("Full_Payment", reader.GetValue(2).ToString());
                    eating.Add("Payment", reader.GetValue(3).ToString());
                    eating.Add("Sale", reader.GetValue(4).ToString()+"%");
                }
                reader.Close();
            }
            ViewBag.Info = eating;
            return View();
        }
        
        public ActionResult Form(DateTime Student_birthday_start, DateTime Student_birthday_end, string Student_name = "",
            string Student_adress="", string Student_sex="Всі", string Student_post="")
        {
            Forms form = new Forms();
            using (connection)
            {
                connection.Open();
                command = new SqlCommand("SELECT F.Form_id, F.Direction, T.Teacher_name " +
                    "FROM Forms F INNER JOIN Teachers T ON F.Teacher_id=T.Teacher_id " +
                    "WHERE F.Form_id=(SELECT MIN(Students.Student_form) " +
                    "FROM Students WHERE Student_id=" + Session["name"] + ")", connection);
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    form.Form_id = reader.GetValue(0).ToString();
                    form.Direction = reader.GetValue(1).ToString();
                    form.Teachers = new Teachers()
                    {
                        Teacher_name = reader.GetValue(2).ToString()
                    };
                }
                reader.Close();

            }

            if (Student_birthday_start > Student_birthday_end)
            {
                DateTime temp = Student_birthday_end;
                Student_birthday_end = Student_birthday_start;
                Student_birthday_start = temp;
            }

            List<string> sex = new List<string>();
            sex.Add("Всі");
            sex.Add("Жінка");
            sex.Add("Чоловік");

            ViewBag.Student_sex = Student_sex;
            ViewBag.Student_name = Student_name;
            ViewBag.Student_birthday_start = Student_birthday_start.ToString("yyyy-MM-dd");
            ViewBag.Student_birthday_end = Student_birthday_end.ToString("yyyy-MM-dd");
            ViewBag.Student_adress = Student_adress;
            ViewBag.Student_post = Student_post;

            Student_sex = Student_sex == "Всі" ? "" : Student_sex;

            string where = " WHERE Student_form =(SELECT MIN(Student_form) FROM Students WHERE Student_id=" + Session["name"] + ") ";

            if (Student_sex != "")
                where += $" AND Student_sex=N'{Student_sex}' ";
            if (Student_name != "")
                where += $" AND Student_name LIKE N'%{Student_name}%'";
            if (Student_birthday_start != null)
                where += $" AND Student_birthday>='{Student_birthday_start.ToString("yyyy-MM-dd")}' ";
            if (Student_birthday_end != null)
                where += $" AND Student_birthday<='{Student_birthday_end.ToString("yyyy-MM-dd")}' ";
            if (Student_adress != "")
                where += $" AND Student_name LIKE N'%{Student_adress}%' ";
            if (Student_post != "")
                where += $" AND Student_name LIKE N'%{Student_post}%' ";

            SqlConnection conn = new SqlConnection(new Connection().ConnectionString);
            using (conn)
            {
                conn.Open();
                command = new SqlCommand("SELECT Student_name, Student_birthday, " +
                    "Student_adress, Student_sex, Student_post " +
                    $"FROM Students {where} ORDER BY Student_name", conn);
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    form.Students.Add(new Students()
                    {
                        Student_name = reader.GetString(0),
                        Student_birthday = Convert.ToDateTime(reader.GetValue(1)),
                        Student_adress = reader.GetValue(2).ToString(),
                        Student_sex = reader.GetString(3),
                        Student_post = reader.GetValue(4).ToString()
                    });
                }
                reader.Close();
            }

            ViewBag.Form = form;
            ViewBag.Students = form.Students;
            ViewBag.Sex = sex;


          
            return View();
        }
        public ActionResult Marks(DateTime Date_start, DateTime Date_end, string Subject="",
            string Type="Всі", string Teacher="", string Mark = "Всі")
        {
            List<Marks> marks = new List<Marks>();
            string where = $" WHERE M.Student_id = {Session["name"]} ";


            List<string> types = new List<string>();
            types.Add("Всі");
  
            List<string> markList = new List<string>();
            markList.Add("Всі");

            SqlConnection sql = new SqlConnection(new Connection().ConnectionString);

            using (sql)
            {
                sql.Open();              

                command = new SqlCommand($"SELECT DISTINCT M.Work_id FROM Marks M {where} ORDER BY 1",
                    sql);
                reader = command.ExecuteReader();
                while (reader.Read())
                    types.Add(reader.GetString(0));
                reader.Close();

                command = new SqlCommand($"SELECT DISTINCT M.Mark FROM Marks M {where} ORDER BY 1", sql);
                reader = command.ExecuteReader();
                while(reader.Read())
                    markList.Add(reader.GetValue(0).ToString());
                reader.Close();


            }

            if (Date_start > Date_end)
            {
                DateTime temp = Date_end;
                Date_end = Date_start;
                Date_start = temp;
            }

            ViewBag.Subject = Subject;
            ViewBag.Mark = Mark;
            ViewBag.Type = Type;
            ViewBag.Date_start = Date_start.ToString("yyyy-MM-dd");
            ViewBag.Date_end = Date_end.ToString("yyyy-MM-dd");
            ViewBag.Teacher = Teacher;

            ViewBag.MarksList = markList;
            ViewBag.Types = types;

            if (Date_start != null)
                where += $" AND M.Date_create>='{Date_start.ToString("yyyy-MM-dd")}' ";
            if (Date_end != null)
                where += $" AND M.Date_create<='{Date_end.ToString("yyyy-MM-dd")}' "; 
            if (Subject != "")
                where += $" AND TS.Subject LIKE N'%{Subject}%' ";
            if (Type != "Всі")
                where += $" AND M.Work_id=N'{Type}'";
            if (Teacher != "")
                where += $" AND T.Teacher_name LIKE N'%{Teacher}%' ";
            if (Mark != "Всі")
                where += $" AND M.Mark={Mark}";

            using (connection)
            {
                connection.Open();
                command = new SqlCommand("SELECT TS.Subject, M.Mark, M.Work_id, M.Date_create, " +
                    "T.Teacher_name FROM Marks M " +
                    "INNER JOIN Teacher_Subject TS ON M.Teacher_Subject_id=TS.Teacher_Subject_id " +
                    "INNER JOIN Teachers T ON TS.Teacher_id=T.Teacher_id " +
                    $" {where} ORDER BY TS.Subject,M.Date_create", connection);
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    marks.Add(new Models.Marks()
                    {
                        Teacher_Subject = new Teacher_Subject()
                        {
                            Subject = reader.GetString(0),
                            Teachers = new Teachers() { Teacher_name = reader.GetString(4) }
                        },
                        Mark = Convert.ToInt32(reader.GetValue(1)),
                        Work_id = reader.GetString(2),
                        Date_create = Convert.ToDateTime(reader.GetValue(3))
                    });
                }
                reader.Close();
            }
            ViewBag.Marks = marks;
            return View();
        }

        public ActionResult Teacher_Subject(int cr = 1, string val = "", byte all = 0) 
        {
            List<Teacher_Subject> teacher_Subjects = new List<Teacher_Subject>();
            using (connection)
            {
                string query;
                if (all == 1)
                {
                    if (val == "")
                        query = "SELECT Teacher_Subject.Subject, Teachers.Teacher_name FROM Teacher_Subject " +
                        "INNER JOIN Teachers ON Teacher_Subject.Teacher_id = Teachers.Teacher_id " +
                        "ORDER BY " + cr;
                    else
                        query = "SELECT Teacher_Subject.Subject, Teachers.Teacher_name FROM Teacher_Subject " +
                            "INNER JOIN Teachers ON Teacher_Subject.Teacher_id = Teachers.Teacher_id " +
                            "WHERE Teachers.Teacher_name LIKE N'%" + val + "%' " +
                            "OR Teacher_Subject.Subject LIKE N'%" + val + "%'  " +
                            "ORDER BY " + cr;
                }
                else
                {
                    if (val == "")
                        query = "SELECT DISTINCT TS.Subject, TR.Teacher_name FROM Timetable T " +
             "INNER JOIN Teacher_Subject TS ON T.Teacher_Subject_id=TS.Teacher_Subject_id " +
             "INNER JOIN Teachers TR ON TS.Teacher_id=TR.Teacher_id " +
             "WHERE T.Form_id=(SELECT MIN(S.Student_form) FROM Students S " +
             "WHERE S.Student_id=" + Session["name"] + ") ORDER BY " + cr;
                    else
                        query = "SELECT DISTINCT TS.Subject, TR.Teacher_name FROM Timetable T " +
                            "INNER JOIN Teacher_Subject TS ON T.Teacher_Subject_id=TS.Teacher_Subject_id  " +
                            "INNER JOIN Teachers TR ON TS.Teacher_id=TR.Teacher_id " +
                            "WHERE (T.Form_id=(SELECT MIN(S.Student_form) FROM Students S " +
                            "WHERE S.Student_id=" + Session["name"] + ")) " +
                            "AND (TR.Teacher_name LIKE N'%" + val + "%' OR TS.Subject LIKE N'%" + val + "%') " +
                            "ORDER BY " + cr;
                }
                    
                connection.Open();
                command = new SqlCommand(query, connection);
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Teacher_Subject ts = new Teacher_Subject()
                    {
                        Subject = reader.GetValue(0).ToString()
                    };
                    ts.Teachers = new Teachers()
                    {
                        Teacher_name = reader.GetValue(1).ToString()
                    };
                    teacher_Subjects.Add(ts);

                }
                reader.Close();
            }
            ViewBag.TS = teacher_Subjects;
            return View();
        }
        public ActionResult Timetable(string day = "All", string form = "", string lesson = "Всі", string subject = "Всі",
           string teacher = "Всі", string room = "Всі")
        {
            if (form == "")
            {
                SqlConnection sqlConnection = new SqlConnection(new Connection().ConnectionString);
                using (sqlConnection)
                {
                    sqlConnection.Open();
                    command = new SqlCommand("SELECT Student_form FROM Students " +
                        "WHERE Student_id=" + Session["name"], sqlConnection);
                    form = command.ExecuteScalar().ToString();
                }
            }

            Dictionary<string, string> days = new Dictionary<string, string>();
            days["All"] = "Всі";
            days["Monday"] = "Понеділок";
            days["Tuesday"] = "Вівторок";
            days["Wednesday"] = "Середа";
            days["Thursday"] = "Четвер";
            days["Friday"] = "П'ятниця";
            ViewBag.Days = days;

            ViewBag.DAY = (day, days[day]);
            ViewBag.FORM = form;
            ViewBag.LESSON = lesson;
            ViewBag.SUBJECT = subject;
            ViewBag.TEACHER = teacher;
            ViewBag.ROOM = room;

            day = day == "All" ? "" : day;
            form = form == "Всі" ? "" : form;
            lesson = lesson == "Всі" ? "" : lesson;
            subject = subject == "Всі" ? "" : subject;
            teacher = teacher == "Всі" ? "" : teacher;
            room = room == "Всі" ? "" : room;


            List<string> lessons = new List<string>();
            lessons.Add("Всі");
            lessons.Add("1");
            lessons.Add("2");
            lessons.Add("3");
            lessons.Add("4");
            lessons.Add("5");
            lessons.Add("6");
            lessons.Add("7");

            ViewBag.Lessons = lessons;

            List<Bells> bells = new List<Bells>();

            using (connection)
            {
                connection.Open();
                command = new SqlCommand("SELECT * FROM Bells", connection);
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    bells.Add(new Bells()
                    {
                        Lesson_id = Convert.ToInt32(reader.GetValue(0)),
                        Begin = (TimeSpan)reader.GetValue(1),
                        End = (TimeSpan)reader.GetValue(2)
                    });
                }
                reader.Close();
            }
            ViewBag.Bells = bells;


            SqlConnection formsConn = new SqlConnection(new Connection().ConnectionString);
            HashSet<string> forms = new HashSet<string>();
            forms.Add("Всі");
            using (formsConn)
            {
                formsConn.Open();
                command = new SqlCommand("SELECT DISTINCT Timetable.Form_id FROM Timetable ORDER BY 1", formsConn);
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    forms.Add(reader.GetString(0));
                }
                reader.Close();
            }
            ViewBag.Forms = forms;

            SqlConnection subConn = new SqlConnection(new Connection().ConnectionString);
            HashSet<string> subs = new HashSet<string>();
            subs.Add("Всі");
            using (subConn)
            {
                subConn.Open();
                command = new SqlCommand("SELECT DISTINCT Teacher_Subject.Subject FROM Timetable " +
                    "INNER JOIN Teacher_Subject ON Timetable.Teacher_Subject_id=Teacher_Subject.Teacher_Subject_id " +
                    "ORDER BY 1", subConn);
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    subs.Add(reader.GetString(0));
                }
                reader.Close();
            }
            ViewBag.Subjects = subs;



            SqlConnection teachConn = new SqlConnection(new Connection().ConnectionString);
            HashSet<string> teachers = new HashSet<string>();
            teachers.Add("Всі");
            using (teachConn)
            {
                teachConn.Open();
                command = new SqlCommand("SELECT DISTINCT Teachers.Teacher_name FROM Timetable" +
                    " INNER JOIN Teacher_Subject ON Timetable.Teacher_Subject_id=Teacher_Subject.Teacher_Subject_id" +
                    " INNER JOIN Teachers ON Teacher_Subject.Teacher_id=Teachers.Teacher_id" +
                    " ORDER BY 1", teachConn);
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    teachers.Add(reader.GetString(0));
                }
                reader.Close();
            }
            ViewBag.Teachers = teachers;


            SqlConnection roomConn = new SqlConnection(new Connection().ConnectionString);
            HashSet<string> rooms = new HashSet<string>();
            rooms.Add("Всі");
            using (roomConn)
            {
                roomConn.Open();
                command = new SqlCommand("SELECT DISTINCT Timetable.Classroom FROM Timetable ORDER BY 1", roomConn);
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    rooms.Add(reader.GetValue(0).ToString());
                }
                reader.Close();
            }
            ViewBag.Rooms = rooms;

            string where = " WHERE";
            if (day.Length != 0)
                where += " Timetable.Day_Of_Week IN ('" + day + "') AND ";
            if (form.Length != 0)
                where += " Timetable.Form_id IN (N'" + form + "') AND ";
            if (lesson.Length != 0)
                where += " Timetable.Lesson_id IN (" + lesson + ") AND ";
            if (subject.Length != 0)
                where += " Teacher_Subject.[Subject] IN (N'" + subject + "') AND ";
            if (teacher.Length != 0)
                where += " Teachers.Teacher_name IN (N'" + teacher + "') AND ";
            if (room.Length != 0)
                where += " Timetable.Classroom IN (" + room + ") AND ";

            if (where == " WHERE")
                where = "";
            else
                where = where.Substring(0, where.Length - 4);


            string query = "SELECT Timetable.*, Teacher_Subject.Subject, Teachers.Teacher_name FROM Timetable " +
                   "INNER JOIN Teacher_Subject ON Timetable.Teacher_subject_id = Teacher_Subject.Teacher_Subject_id " +
                   "INNER JOIN Teachers ON Teacher_Subject.Teacher_id = Teachers.Teacher_id " + where +
                   "ORDER BY " +
                   "CASE" +
                   " WHEN Timetable.Day_Of_Week = 'Monday' THEN 1" +
                   " WHEN Timetable.Day_Of_Week = 'Tuesday' THEN 2" +
                   " WHEN Timetable.Day_Of_Week = 'Wednesday' THEN 3" +
                   " WHEN Timetable.Day_Of_Week = 'Thursday' THEN 4" +
                   " WHEN Timetable.Day_Of_Week = 'Friday' THEN 5" +
                   " END ASC, 3, 2";

            HashSet<Timetable> timetable = new HashSet<Timetable>();
            SqlConnection conn = new SqlConnection(new Connection().ConnectionString);
            using (conn)
            {
                conn.Open();
                command = new SqlCommand(query, conn);
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Timetable t = new Models.Timetable()
                    {
                        Day_Of_Week = reader.GetValue(0).ToString(),
                        Lesson_id = Convert.ToInt32(reader.GetValue(1)),
                        Form_id = reader.GetValue(2).ToString(),
                        Classroom = Convert.ToInt32(reader.GetValue(3)),
                        Teacher_Subject_id = Convert.ToInt32(reader.GetValue(4)),
                    };
                    t.Teacher_Subject = new Models.Teacher_Subject() { Subject = reader.GetValue(5).ToString() };
                    t.Teacher_Subject.Teachers = new Teachers() { Teacher_name = reader.GetValue(6).ToString() };
                    timetable.Add(t);

                }
                reader.Close();
            }
            ViewBag.Timetable = timetable;
            return View();
        }

        [HttpGet]
        public ActionResult EditLog()
        {
            string log = "";
            string log_p = "";
            string pass = "";

            using (connection)
            {
                connection.Open();
                command = new SqlCommand("SELECT Student_post, " +
                    "Student_password, Student_post_parents FROM " +
                    $"Students WHERE Student_id={Session["name"]}", connection);
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    log = reader.GetValue(0).ToString();
                    pass = reader.GetValue(1).ToString();
                    log_p = reader.GetValue(2).ToString();
                }
                reader.Close();
            }
            ViewBag.Student_post = log;
            ViewBag.Student_password = pass;
            ViewBag.Student_post_parent = log_p;
            return View();
        }

        [HttpPost]
        public ActionResult EditLog(string Student_post, string Student_password,
            string Student_post_parent="")
        {
            using (connection)
            {
                connection.Open();
                command = new SqlCommand($"UPDATE Students SET " +
                    $"Student_post='{Student_post}'," +
                    $"Student_password='{Student_password}'," +
                    $"Student_post_parents='{Student_post_parent}' " +
                    $" WHERE Student_id={Session["name"]}", connection);
                command.ExecuteNonQuery();
            }
            return RedirectToAction("EditLog", "Student");
        }
        public ActionResult DownloadMarks(DateTime Date_start, DateTime Date_end)
        {
            MemoryStream stream;
            string name;
            try
            {
                CreateDoc.CreateTab(Session["name"].ToString(), out stream, out name);
                return File(stream.ToArray(), "application/octet-stream", name);

            }
            catch { } 


            return RedirectToAction("Marks", "Student", new { Date_start = Date_start, Date_end = Date_end });
        }

        public ActionResult DownloadInvoice()
        {
            MemoryStream stream;
            string name;
            try
            {
                CreateDoc.CreateInvoice(Session["name"].ToString(), out stream, out name);
                return File(stream.ToArray(), "application/octet-stream", name);
            }
            catch { }

            return RedirectToAction("Eating", "Student");
        }
    }
}