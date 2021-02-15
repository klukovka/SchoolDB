using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SchoolDB.Models;

namespace SchoolDB.Controllers
{
    public class HomeController : Controller
    {

        SchoolContext db = new SchoolContext();
        SqlConnection connection = new SqlConnection(new Connection().ConnectionString);
        SqlCommand command;
        SqlDataReader reader;
        public ActionResult Index()
        {
            Session["name"] = null;
            return View();
        }       
        public ActionResult Timetable(string day="All", string form="Всі", string lesson="Всі", string subject="Всі",
            string teacher="Всі", string room="Всі")
        {

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
            if (day.Length!=0)
                where += " Timetable.Day_Of_Week IN ('" + day + "') AND ";
            if (form.Length!=0)
                where+= " Timetable.Form_id IN (N'" + form + "') AND ";
            if (lesson.Length!=0)
                where+= " Timetable.Lesson_id IN (" + lesson + ") AND ";
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

        public ActionResult Privileges()
        {
            List<Privileges> privileges = new List<Privileges>();
            using (connection)
            {
                connection.Open();
                command = new SqlCommand("SELECT * FROM Privileges ORDER BY Sale DESC", connection);
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    privileges.Add(new Models.Privileges()
                    {
                        Privilege_id = reader.GetValue(0).ToString(),
                        Sale = Convert.ToInt32(reader.GetValue(1))
                    });
                }
                reader.Close();
            }
            ViewBag.Prvl = privileges;
            return View();
        }

        public ActionResult Teacher_Subject(int cr=1, string val="")
        {
            List<Teacher_Subject> teacher_Subjects = new List<Teacher_Subject>();
            using (connection)
            {
                string query;
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

        public ActionResult LogIn(string post = "", string password = "")
        {
            string query = "SELECT Student_id FROM Students " +
                   "WHERE Student_post = '" + post + "' AND Student_password = '" + password + "'";

            object id;
            using (connection)
            {
                connection.Open();
                command = new SqlCommand(query, connection);
                id = command.ExecuteScalar();
                if (id != null)
                    Session["name"] = id.ToString();
            }

            if (Session["name"] != null)
                return RedirectToAction("Index", "Student");

            SqlConnection sql = new SqlConnection(new Connection().ConnectionString);

            using (sql)
            {
                sql.Open();
                query = "SELECT Teacher_id FROM Teachers " +
                     "WHERE Teacher_post = '" + post + "' AND Teacher_password = '" + password + "'";
                command = new SqlCommand(query, sql);
                id = command.ExecuteScalar();
                if (id != null)
                    Session["name"] = id.ToString();
            }

            if (Session["name"] != null)
            {
                if (id.ToString() == "1")
                    return RedirectToAction("Index", "HighTeacher");
                return RedirectToAction("Index", "Teacher");
            }

            if (post!="" && password!="")
                ViewBag.Messege = "Перевірте правильність вводу даних";
            return View();

        }

       

    }
}