using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SchoolDB.Models;
using SautinSoft.Document;
using System.IO;

namespace SchoolDB.Controllers
{
    public class TeacherController : Controller
    {
        SchoolContext db = new SchoolContext();
        SqlConnection connection = new SqlConnection(new Connection().ConnectionString);
        SqlCommand command;
        SqlDataReader reader;
        // GET: Teacher
        public ActionResult Index()
        {
            return RedirectToAction("Teacher");
        }
        public ActionResult Teacher()
        {
            Teachers teacher = new Teachers();
            using (connection)
            {
                connection.Open();
                command = new SqlCommand("SELECT T.*, F.Form_id AS Form_id " +
                    "FROM Teachers T LEFT JOIN Forms F " +
                    "ON T.Teacher_id=F.Teacher_id " +
                    "WHERE T.Teacher_id=" + Session["name"], connection);
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    teacher.Teacher_id = reader.GetInt32(0);
                    teacher.Teacher_name = reader.GetString(1);
                    if ((object)reader.GetValue(2) == DBNull.Value)
                        teacher.Teacher_room = null;
                    else
                        teacher.Teacher_room = Convert.ToInt32(reader.GetValue(2));
                    teacher.Teacher_sex = reader.GetString(3);
                    teacher.Category_id = reader.GetString(4);
                    teacher.Teacher_post = reader.GetValue(5).ToString();
                    teacher.Teacher_password = reader.GetValue(6).ToString();
                    teacher.Teacher_birthday = Convert.ToDateTime(reader.GetValue(7));
                    teacher.Forms.Add(new Models.Forms() { Form_id = reader.GetValue(8).ToString() });
                }
                reader.Close();

                command = new SqlCommand($"SELECT SALARY, [Hours] FROM Salaries " +
                    $"WHERE Teacher_id={Session["name"]}", connection);
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    ViewBag.Salary = reader.GetValue(0).ToString();
                    ViewBag.Hours = reader.GetValue(1).ToString();
                }
                reader.Close();

            }
            ViewBag.Teacher = teacher;
            return View();
        }
        public ActionResult Forms(DateTime MyStudent_birthday_start, DateTime MyStudent_birthday_end,
            DateTime Student_birthday_start, DateTime Student_birthday_end,
            string MyStudent_name="", string MyStudent_sex="Всі", string MyStudent_adress="", 
            string MyStudent_dinning = "Всі", string MyStudent_post="",string MyStudent_post_parent="", 
            string MyPrivilege_id="Всі",
            string Student_form = "Всі", string Student_name = "", string Student_adress = "",
            string Student_sex = "Всі", string Student_post = "")
        {
        
            Forms myform = new Forms();
            SqlConnection myformconn = new SqlConnection(new Connection().ConnectionString);
            List<string> privilages = new List<string>();
            privilages.Add("Всі");
            using (myformconn)
            {
                myformconn.Open();
                command = new SqlCommand("SELECT Form_id, Direction " +
                    "FROM Forms WHERE Teacher_id=" + Session["name"], myformconn);
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    myform.Form_id = reader.GetString(0);
                    myform.Direction = reader.GetString(1);
                }
                reader.Close();

                command = new SqlCommand("SELECT Privilege_id FROM Privileges ORDER BY 1", myformconn);
                reader = command.ExecuteReader();
                while (reader.Read())
                    privilages.Add(reader.GetString(0));

                reader.Close();
            }
            ViewBag.MyForm = myform;

            List<Students> mystudents = new List<Students>();

            if (MyStudent_birthday_start > MyStudent_birthday_end)
            {
                DateTime temp = MyStudent_birthday_end;
                MyStudent_birthday_end = MyStudent_birthday_start;
                MyStudent_birthday_start = temp;
            }

            if (Student_birthday_start > Student_birthday_end)
            {
                DateTime temp = Student_birthday_end;
                Student_birthday_end = Student_birthday_start;
                Student_birthday_start = temp;
            }

            List<string> sex = new List<string>();
            sex.Add("Всі");
            sex.Add("Чоловік");
            sex.Add("Жінка");
            List<string> dinning = new List<string>();
            dinning.Add("Всі");
            dinning.Add("Так");
            dinning.Add("Ні");
            

            ViewBag.MyStudent_name = MyStudent_name;
            ViewBag.MyStudent_birthday_start = MyStudent_birthday_start.ToString("yyyy-MM-dd");
            ViewBag.MyStudent_birthday_end = MyStudent_birthday_end.ToString("yyyy-MM-dd");
            ViewBag.MyStudent_sex = MyStudent_sex;
            ViewBag.MyStudent_adress = MyStudent_adress;
            ViewBag.MyStudent_dinning = MyStudent_dinning;
            ViewBag.MyStudent_post = MyStudent_post;
            ViewBag.MyStudent_post_parent = MyStudent_post_parent;
            ViewBag.MyPrivilege_id = MyPrivilege_id;
            ViewBag.Sex = sex;
            ViewBag.Privilages = privilages;
            ViewBag.Dinning = dinning;

            string myWhere = $"WHERE S.Student_form=(SELECT F.Form_id FROM Forms F WHERE F.Teacher_id={Session["name"]}) ";
            if (MyStudent_name != "")
                myWhere += $" AND Student_name LIKE N'%{MyStudent_name}%' ";
            if (MyStudent_birthday_start!=null)
                myWhere += $" AND Student_birthday>='{MyStudent_birthday_start.ToString("yyyy-MM-dd")}' ";
            if (MyStudent_birthday_end != null)
                myWhere += $" AND Student_birthday<='{MyStudent_birthday_end.ToString("yyyy-MM-dd")}' ";
            if (MyStudent_sex != "Всі")
                myWhere += $" AND Student_sex=N'{MyStudent_sex} '";
            if (MyStudent_adress != "")
                myWhere += $" AND Student_adress LIKE N'%{MyStudent_adress}%' ";
            if (MyStudent_dinning == "Так")
                myWhere += $" AND Student_dinning=1 ";
            if (MyStudent_dinning == "Ні")
                myWhere += $" AND Student_dinning=0 ";
            if (MyStudent_post != "")
                myWhere += $" AND Student_post LIKE N'%{MyStudent_post}%' ";
             if (MyStudent_post_parent != "")
                myWhere += $" AND Student_post_parents LIKE N'%{MyStudent_post_parent}%' ";
            if (MyPrivilege_id != "Всі")
                myWhere += $" AND Privilege_id=N'{MyPrivilege_id} '";

            using (connection)
            {
                connection.Open();
                command = new SqlCommand("SELECT Student_name, Student_birthday, " +
                    "Student_sex, Student_adress, Privilege_id, Student_dinning, " +
                    "Student_post, Student_post_parents " +
                    "FROM Students S " +
                    $" {myWhere} " +
                    "ORDER BY S.Student_name", connection);
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    mystudents.Add(new Students()
                    {
                        Student_name = reader.GetString(0),
                        Student_birthday = Convert.ToDateTime(reader.GetValue(1)),
                        Student_sex = reader.GetString(2),
                        Student_adress = reader.GetValue(3).ToString(),
                        Privilege_id = reader.GetString(4),
                        Student_dinning = Convert.ToBoolean(reader.GetValue(5)),
                        Student_post = reader.GetValue(6).ToString(),
                        Student_post_parents = reader.GetValue(7).ToString()
                    });
                }
                reader.Close();
            }

            ViewBag.MyStudents = mystudents;

            SqlConnection sql = new SqlConnection(new Connection().ConnectionString);
            List<string> forms = new List<string>();
            forms.Add("Всі");
            using (sql)
            {
                sql.Open();
                command = new SqlCommand("SELECT DISTINCT T.Form_id " +
                    "FROM Timetable T " +
                    "INNER JOIN Teacher_Subject TS ON T.Teacher_Subject_id=TS.Teacher_Subject_id " +
                    $"WHERE TS.Teacher_id={Session["name"]}", sql);
                reader = command.ExecuteReader();
                while (reader.Read())
                    forms.Add(reader.GetString(0));
                reader.Close();

            }
            ViewBag.Forms = forms;
            ViewBag.Form = Student_form;
            ViewBag.Student_name = Student_name;
            ViewBag.Student_birthday_start = Student_birthday_start.ToString("yyyy-MM-dd");
            ViewBag.Student_birthday_end = Student_birthday_end.ToString("yyyy-MM-dd");
            ViewBag.Student_adress = Student_adress;
            ViewBag.Student_sex = Student_sex;
            ViewBag.Student_post = Student_post;

            string where = $" WHERE TR.Teacher_id={Session["name"]}) ";
            if (Student_form != "Всі")
                where += $" AND Student_form=N'{Student_form}' ";
            if (Student_name != "")
                where += $" AND Student_name LIKE N'%{Student_name}%' ";
            if (Student_birthday_start != null)
                where += $" AND Student_birthday>='{Student_birthday_start.ToString("yyyy-MM-dd")}' ";
            if (Student_birthday_end != null)
                where += $" AND Student_birthday<='{Student_birthday_end.ToString("yyyy-MM-dd")}' ";
            if (Student_sex != "Всі")
                where += $" AND Student_sex=N'{Student_sex} '";
            if (Student_adress != "")
                where += $" AND Student_adress LIKE N'%{Student_adress}%' ";
            if (Student_post != "")
                where += $" AND Student_post LIKE N'%{Student_post}%' ";

            List<Students> students = new List<Students>();
            SqlConnection sqlConnection = new SqlConnection(new Connection().ConnectionString);
            using (sqlConnection)
            {
                sqlConnection.Open();
                command = new SqlCommand("SELECT Student_form, Student_name, Student_birthday, " +
                    "Student_adress, Student_sex, Student_post FROM Students S " +
                    "WHERE Student_form IN ( " +
                    "SELECT DISTINCT T.Form_id FROM Timetable T " +
                    "INNER JOIN Teacher_Subject TS ON T.Teacher_Subject_id=TS.Teacher_Subject_id " +
                    "INNER JOIN Teachers TR ON TS.Teacher_id=TR.Teacher_id " +
                    $" {where} ORDER BY 1,2", sqlConnection);
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    students.Add(new Students()
                    {
                        Student_form = reader.GetString(0),
                        Student_name = reader.GetString(1),
                        Student_birthday = Convert.ToDateTime(reader.GetValue(2)),
                        Student_adress = reader.GetValue(3).ToString(),
                        Student_sex = reader.GetString(4),
                        Student_post = reader.GetValue(5).ToString()
                    });
                }
                reader.Close();
            }
            ViewBag.Students = students;

            return View();
        }
        [HttpGet]
        public ActionResult Marks(DateTime Date_of_create_start, DateTime Date_of_create_end,
            string Subject = "Всі", string Student_name = "", string Mark = "Всі",
            string Work_id = "Всі", string Student_form = "Всі")
        {

            List<string> sub = new List<string>();
            sub.Add("Всі");
            List<string> marks = new List<string>();
            marks.Add("Всі");
            List<string> works = new List<string>();
            works.Add("Всі");


            if (Date_of_create_start > Date_of_create_end)
            {
                DateTime temp = Date_of_create_end;
                Date_of_create_end = Date_of_create_start;
                Date_of_create_start = temp;
            }

            ViewBag.Student_form = Student_form;
            ViewBag.Subject = Subject;
            ViewBag.Student_name = Student_name;
            ViewBag.Mark = Mark;
            ViewBag.Date_of_create_end = Date_of_create_end.ToString("yyyy-MM-dd");
            ViewBag.Date_of_create_start = Date_of_create_start.ToString("yyyy-MM-dd");
            ViewBag.Work_id = Work_id;



            SqlConnection sql = new SqlConnection(new Connection().ConnectionString);
            using (sql)
            {
                sql.Open();
                command = new SqlCommand("SELECT Subject FROM Teacher_Subject " +
                    $"WHERE Teacher_id={Session["name"]}", sql);
                reader = command.ExecuteReader();
                while (reader.Read())
                    sub.Add(reader.GetString(0));
                reader.Close();

                command = new SqlCommand("SELECT DISTINCT Mark FROM Marks M " +
                    "INNER JOIN Teacher_Subject TS ON M.Teacher_Subject_id=TS.Teacher_Subject_id " +
                    $"WHERE TS.Teacher_id={Session["name"]} ORDER BY 1", sql);
                reader = command.ExecuteReader();
                while(reader.Read())
                    marks.Add(reader.GetValue(0).ToString());
                reader.Close();

                command = new SqlCommand("SELECT Work_id FROM Types_of_work", sql);
                reader = command.ExecuteReader();
                while (reader.Read())
                    works.Add(reader.GetString(0));
                reader.Close();


            }
            ViewBag.Subjects = sub;
            ViewBag.Marks = marks;
            ViewBag.Works = works;
            string query;
            List<string> forms = new List<string>();

            string where = $" WHERE TS.Teacher_id={Session["name"]} ";
            if (Student_name != "")
                where += $" AND S.Student_name LIKE N'%{Student_name}%' ";
            if (Date_of_create_start != null)
                where += $" AND M.Date_create>='{Date_of_create_start.ToString("yyyy-MM-dd")}' ";
            if (Date_of_create_end != null)
                where += $" AND M.Date_create<='{Date_of_create_end.ToString("yyyy-MM-dd")}' ";
            if (Subject != "Всі")
                where = $" AND TS.Subject=N'{Subject}' ";
            if (Student_form != "Всі")
                where = $" AND S.Student_form=N'{Student_form}' ";
            if (Mark != "Всі")
                where = $" AND M.Mark={Mark} ";
            if (Work_id != "Всі")
                where = $" AND M.Work_id=N'{Work_id}' ";

            query = "SELECT TS.Subject, S.Student_form, S.Student_name, " +
               "M.Mark, M.Date_create, M.Work_id, S.Student_id, TS.Teacher_Subject_id " +
               "FROM Marks M INNER JOIN Students S ON M.Student_id=S.Student_id " +
               "INNER JOIN Teacher_Subject TS ON M.Teacher_Subject_id=TS.Teacher_Subject_id " +
               $" {where} ORDER BY 1,2,3,5,4";
            List<Marks> myMarks = new List<Marks>();


            using (connection)
            {
                connection.Open();
                command = new SqlCommand(query, connection);
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    myMarks.Add(new Models.Marks()
                    {
                        Teacher_Subject_id = reader.GetInt32(7),
                        Teacher_Subject = new Teacher_Subject() { Subject = reader.GetString(0) },
                        Students = new Students()
                        {
                            Student_form = reader.GetString(1),
                            Student_name = reader.GetString(2),
                            Student_id = reader.GetInt32(6)
                        },
                        Student_id = reader.GetInt32(6),
                        Mark = Convert.ToInt32(reader.GetValue(3)),
                        Date_create = Convert.ToDateTime(reader.GetValue(4)),
                        Work_id = reader.GetValue(5).ToString()
                    });
                }
                reader.Close();

                command = new SqlCommand($"SELECT DISTINCT T.Form_id FROM Timetable T " +
                $"INNER JOIN Teacher_Subject TS ON T.Teacher_Subject_id=TS.Teacher_Subject_id " +
                $"WHERE TS.Teacher_id={Session["name"]}", connection);
                reader = command.ExecuteReader();
                while (reader.Read())
                    forms.Add(reader.GetString(0));
                reader.Close();
            }
            ViewBag.MyMarks = myMarks;
            ViewBag.Forms = forms;

            return View();
        }
        [HttpPost]
        public ActionResult Marks(string action, string Form_id, DateTime Date_of_create_start, DateTime Date_of_create_end,
            string Subject = "Всі", string Student_name = "", string Mark = "Всі",
            string Work_id = "Всі", string Student_form = "Всі")
        {
            if (action == "Додати")
            {
                return RedirectToAction("CreateMark", "Teacher", new { id = Form_id });
            }
            else
            {
                return RedirectToAction("Marks", "Teacher", new
                {
                    Date_of_create_start = Date_of_create_start,
                    Date_of_create_end = Date_of_create_end,
                    Subject = Subject,
                    Student_name = Student_name,
                    Mark = Mark,
                    Work_id = Work_id,
                    Student_form = Student_form
                });

            }
        }
        public ActionResult Privileges()
        {
            List<Dictionary<string, object>> priv = new List<Dictionary<string, object>>();

            SqlConnection sql = new SqlConnection(new Connection().ConnectionString);
            int count;
            using (sql)
            {
                sql.Open();
                command = new SqlCommand("SELECT COUNT(Form_id) FROM Forms " +
                    "WHERE Teacher_id=" + Session["name"], sql);
                count = Convert.ToInt32(command.ExecuteScalar());
            }

            string query;
            if (count != 0)
                query = "SELECT S.Privilege_id, P.Sale, " +
                    "COUNT(S.Student_id) AS Amount FROM Students S " +
                    "INNER JOIN Privileges P ON S.Privilege_id=P.Privilege_id " +
                    "WHERE S.Student_form IN " +
                    "(SELECT Form_id FROM Forms WHERE Teacher_id=" + Session["name"] + ") " +
                    "GROUP BY S.Privilege_id, P.Sale " +
                    "ORDER BY P.Sale DESC, COUNT(S.Student_id) DESC";
            else
                query = "SELECT S.Privilege_id, P.Sale, " +
                    "COUNT(S.Student_id) AS Amount FROM Students S " +
                    "INNER JOIN Privileges P ON S.Privilege_id=P.Privilege_id " +
                    "GROUP BY S.Privilege_id, P.Sale ORDER BY P.Sale DESC, " +
                    "COUNT(S.Student_id) DESC";

            ViewBag.Count = count;
            using (connection)
            {
                connection.Open();
                command = new SqlCommand(query, connection);
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Dictionary<string, object> temp = new Dictionary<string, object>();
                    temp["Privilege_id"] = reader.GetString(0);
                    temp["Sale"] = reader.GetValue(1).ToString() + "%";
                    temp["Amount"] = Convert.ToInt32(reader.GetValue(2));
                    priv.Add(temp);
                }
                reader.Close();
            }
            ViewBag.Priv = priv;
            return View();
        }

        public ActionResult Teacher_Subject(int cr = 1, string val = "")
        {
            List<string> sub = new List<string>();
            SqlConnection conn = new SqlConnection(new Connection().ConnectionString);
            using (conn)
            {
                conn.Open();
                command = new SqlCommand("SELECT TS.Subject FROM Teacher_Subject TS " +
                    "INNER JOIN Teachers T ON TS.Teacher_id=T.Teacher_id " +
                    "WHERE T.Teacher_id=" + Session["name"] + " ORDER BY 1", conn);
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    sub.Add(reader.GetString(0));
                }
                reader.Close();
            }
            ViewBag.Sub = sub;

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
        public ActionResult Timetable(string day = "All", string form = "Всі", string lesson = "Всі", string subject = "Всі",
          string teacher = "", string room = "Всі")
        {
            if (teacher == "")
            {
                SqlConnection sqlConnection = new SqlConnection(new Connection().ConnectionString);
                using (sqlConnection)
                {
                    sqlConnection.Open();
                    command = new SqlCommand("SELECT Teacher_name FROM Teachers " +
                        "WHERE Teacher_id=" + Session["name"], sqlConnection);
                    teacher = command.ExecuteScalar().ToString();
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
        public ActionResult EditMark(int Student_id, DateTime Date_create,
    int Teacher_Subject_id)
        {
            Marks mark = new Marks();
            List<string> subjects = new List<string>();
            List<string> types = new List<string>();
            using (connection)
            {
                connection.Open();
                command = new SqlCommand($"SELECT M.Student_id, S.Student_name, " +
                    $"S.Student_form, M.Date_create, TS.Subject, " +
                    $"M.Mark, M.Work_id, M.Teacher_Subject_id FROM Marks M " +
                    $"LEFT JOIN Teacher_Subject TS ON M.Teacher_Subject_id=TS.Teacher_Subject_id " +
                    $"LEFT JOIN Students S ON M.Student_id=S.Student_id " +
                    $"WHERE M.Student_id={Student_id} " +
                    $"AND M.Date_create='{Date_create.ToString("yyyy-MM-dd")}' " +
                    $"AND M.Teacher_Subject_id={Teacher_Subject_id}", connection);
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    mark.Student_id = reader.GetInt32(0);
                    mark.Students = new Students();
                    mark.Students.Student_name = reader.GetString(1);
                    mark.Students.Student_form = reader.GetString(2);
                    mark.Date_create = Convert.ToDateTime(reader.GetValue(3));
                    mark.Teacher_Subject = new Teacher_Subject()
                    {
                        Subject = reader.GetString(4)
                    };
                    mark.Mark = reader.GetInt32(5);
                    mark.Work_id = reader.GetString(6);
                    mark.Teacher_Subject_id = reader.GetInt32(7);
                }
                reader.Close();

                command = new SqlCommand($"SELECT DISTINCT TS.Subject FROM Timetable T " +
                    $"LEFT JOIN Teacher_Subject TS ON T.Teacher_Subject_id=TS.Teacher_Subject_id " +
                    $"WHERE TS.Teacher_id={Session["name"]} " +
                    $"AND T.Form_id=N'{mark.Students.Student_form}' " +
                    $"AND TS.Subject!=N'{mark.Teacher_Subject.Subject}'", connection);
                reader = command.ExecuteReader();

                while (reader.Read())
                    subjects.Add(reader.GetString(0));

                reader.Close();

                command = new SqlCommand($"SELECT Work_id " +
                    $"FROM Types_of_work " +
                    $"WHERE Work_id!=N'{mark.Work_id}'", connection);
                reader = command.ExecuteReader();
                while (reader.Read())
                    types.Add(reader.GetString(0));
                reader.Close();

            }

            ViewBag.Info = mark;
            ViewBag.Subjects = subjects;
            ViewBag.Types = types;
            return View();
        }

        [HttpPost]
        public ActionResult EditMark(int Student_id, DateTime Date_create, string Subject,
            int Mark, string Work_id, DateTime Old_date, int Teacher_Subject_id)
        {
            using (connection)
            {
                connection.Open();
                command = new SqlCommand("DELETE FROM Marks " +
                    $"WHERE Student_id={Student_id} AND " +
                    $"Date_create='{Old_date.ToString("yyyy-MM-dd")}' AND " +
                    $"Teacher_Subject_id={Teacher_Subject_id}", connection);
                command.ExecuteNonQuery();

                command = new SqlCommand($"SELECT MIN(Teacher_Subject_id) " +
                    $"FROM Teacher_Subject WHERE Subject=N'{Subject}' " +
                    $"AND Teacher_id={Session["name"]}", connection);
                Teacher_Subject_id = Convert.ToInt32(command.ExecuteScalar());

                command = new SqlCommand("INSERT INTO Marks (Student_id, Teacher_Subject_id," +
                    "Mark, Date_create, Work_id) VALUES" +
                    $"({Student_id}, {Teacher_Subject_id}," +
                    $"{Mark}, '{Date_create.ToString("yyyy-MM-dd")}', N'{Work_id}')", connection);
                command.ExecuteNonQuery();
            }
            return RedirectToAction("EditMark", "Teacher", new
            {
                Student_id = Student_id,
                Date_create = Date_create,
                Teacher_Subject_id = Teacher_Subject_id
            });
        }

        [HttpGet]
        public ActionResult EditLog()
        {
            string log = "";
            string pass = "";
            using (connection)
            {
                connection.Open();
                command = new SqlCommand("SELECT Teacher_post," +
                    "Teacher_password FROM Teachers " +
                    "WHERE Teacher_id=" + Session["name"], connection);
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    log = reader.GetValue(0).ToString();
                    pass = reader.GetValue(1).ToString();
                }
                reader.Close();

            }
            ViewBag.Teacher_post = log;
            ViewBag.Teacher_password = pass;
            return View();
        }

        [HttpPost]
        public ActionResult EditLog(string Teacher_post, string Teacher_password)
        {
            using (connection)
            {
                connection.Open();
                command = new SqlCommand($"UPDATE Teachers SET " +
                    $"Teacher_post='{Teacher_post}', " +
                    $"Teacher_password='{Teacher_password}' " +
                    $"WHERE Teacher_id={Session["name"]}", connection);
                command.ExecuteNonQuery();
            }
            return RedirectToAction("EditLog", "Teacher");
        }

        [HttpGet]
        public ActionResult CreateMark(string id)
        {
            List<Students> students = new List<Students>();
            List<Teacher_Subject> sub = new List<Teacher_Subject>();
            List<string> type = new List<string>();

            using (connection)
            {
                connection.Open();
                command = new SqlCommand("SELECT DISTINCT S.Student_id, S.Student_name " +
                    "FROM Timetable T " +
                    "INNER JOIN Teacher_Subject TS ON T.Teacher_Subject_id=TS.Teacher_Subject_id " +
                    "INNER JOIN Students S ON T.Form_id=S.Student_form " +
                    $"WHERE T.Form_id=N'{id}' AND Teacher_id={Session["name"]}", connection);
                reader = command.ExecuteReader();
                while (reader.Read())
                    students.Add(new Models.Students()
                    {
                        Student_id = reader.GetInt32(0),
                        Student_name = reader.GetString(1)
                    });
                reader.Close();

                command = new SqlCommand($"SELECT DISTINCT TS.Teacher_Subject_id, TS.Subject " +
                    $"FROM Timetable " +
                    $"T INNER JOIN Teacher_Subject TS ON T.Teacher_Subject_id=TS.Teacher_Subject_id " +
                    $"WHERE T.Form_id=N'{id}' AND Teacher_id={Session["name"]}", connection);
                reader = command.ExecuteReader();
                while (reader.Read())
                    sub.Add(new Models.Teacher_Subject()
                    {
                        Teacher_Subject_id = reader.GetInt32(0),
                        Subject = reader.GetString(1)
                    });
                reader.Close();

                command = new SqlCommand("SELECT Work_id FROM Types_of_work",
                    connection);
                reader = command.ExecuteReader();
                while (reader.Read())
                    type.Add(reader.GetString(0));
                reader.Close();
            }

            ViewBag.TS = sub;
            ViewBag.Students = students;
            ViewBag.Types = type;

            return View();
        }

        [HttpPost]
        public ActionResult CreateMark(int Student_id, int Teacher_Subject_id,
            string Mark, string Type_Of_Work, DateTime Date_create)
        {
            int mark;
            try
            {
                mark = Convert.ToInt32(Mark);
            }
            catch
            {
                mark = new Random().Next(1, 12);
            }
            if (mark > 12)
                mark = 12;
            else if (mark < 1)
                mark = 1;

            using (connection)
            {
                connection.Open();
                command = new SqlCommand($"SELECT COUNT(*) FROM Marks " +
                    $"WHERE Student_id={Student_id} AND " +
                    $"Teacher_Subject_id={Teacher_Subject_id} AND " +
                    $"Date_create='{Date_create.ToString("yyyy-MM-dd")}'", connection);
                int count = Convert.ToInt32(command.ExecuteScalar());

                if (count == 0)
                    command = new SqlCommand("INSERT INTO Marks (" +
                        "Student_id, Teacher_Subject_id, Mark, Work_id, Date_create)" +
                        $"VALUES ({Student_id}, {Teacher_Subject_id}, {mark}, " +
                        $"N'{Type_Of_Work}','{Date_create.ToString("yyyy-MM-dd")}')",
                        connection);
                else
                    command = new SqlCommand($"UPDATE Marks SET " +
                        $"Student_id={Student_id}," +
                        $"Teacher_Subject_id={Teacher_Subject_id}," +
                        $"Mark={mark}, Work_id=N'{Type_Of_Work}'," +
                        $"Date_create='{Date_create.ToString("yyyy-MM-dd")}' " +
                            $"WHERE Student_id={Student_id} AND " +
                    $"Teacher_Subject_id={Teacher_Subject_id} AND " +
                    $"Date_create='{Date_create.ToString("yyyy-MM-dd")}'", connection);
                command.ExecuteNonQuery();

            }
            return RedirectToAction("Marks", "Teacher", new
            {
                Date_of_create_start = new DateTime(2020, 9, 1),
                Date_of_create_end = DateTime.Now,
                Subject = "Всі",
                Student_name = "",
                Mark = "Всі",
                Work_id = "Всі",
                Student_form = "Всі"
            });
        }

        [HttpGet]
        public ActionResult DeleteMark(int Student_id, DateTime Date_create, int Teacher_Subject_id)
        {
            string mes = "";
            using (connection)
            {
                connection.Open();
                command = new SqlCommand("SELECT S.Student_name, TS.Subject, M.Mark, Date_create, " +
                    "Work_id FROM MARKS M INNER JOIN STUDENTS S ON M.Student_id=S.Student_id " +
                    "INNER JOIN Teacher_Subject TS ON M.Teacher_Subject_id=TS.Teacher_Subject_id " +
                    $"WHERE M.Student_id ={ Student_id } AND " +
                    $"M.Teacher_Subject_id={Teacher_Subject_id} AND " +
                    $"M.Date_create='{Date_create.ToString("yyyy-MM-dd")}'", connection);
                reader = command.ExecuteReader();
                while(reader.Read())
                    mes += $"Ви дійсно хочете видалити оцінку {reader.GetInt32(2).ToString()} " +
                        $"учня {reader.GetString(0)} з предмету {reader.GetString(1)}" +
            $" за {reader.GetValue(3).ToString()} число за такий тип роботи:{reader.GetString(4)}?";
                reader.Close();

            }

            ViewBag.Mes = mes;
            return View();
        }
        [HttpPost]
        public ActionResult DeleteMark(string action, int Student_id, DateTime Date_create, int Teacher_Subject_id)
        {
            if (action == "Так")
            {
                using (connection)
                {
                    connection.Open();
                    command = new SqlCommand("DELETE FROM Marks " +
                        $"WHERE Student_id ={ Student_id } AND " +
                        $"Teacher_Subject_id={Teacher_Subject_id} AND " +
                        $"Date_create='{Date_create.ToString("yyyy-MM-dd")}'", connection);
                    command.ExecuteNonQuery();
                }
            }
            return RedirectToAction("Marks", "Teacher", new
            {
                Date_of_create_start = new DateTime(2020, 9, 1),
                Date_of_create_end = DateTime.Now,
                Subject = "Всі",
                Student_name = "",
                Mark = "Всі",
                Work_id = "Всі",
                Student_form = "Всі"
            }) ;
        }
        public ActionResult DownloadForms(string id)
        {
            MemoryStream stream;
            string name;
            try
            {
                CreateDoc.CreateForm(id, out stream, out name);
                return File(stream.ToArray(), "application/octet-stream", name);
            }
            catch { }

            return RedirectToAction("Forms", "Teacher", new
            {
                MyStudent_birthday_start = new DateTime(2000, 11, 11),
                MyStudent_birthday_end = DateTime.Now,
                Student_birthday_start = new DateTime(2000, 11, 11),
                Student_birthday_end = DateTime.Now
            });
        }


    }
}