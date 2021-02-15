using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SchoolDB.Models;
using System.Net.Mail;
using System.Net;
using Xceed.Words.NET;
using Xceed.Document.NET;
using System.IO;
using Paragraph = Xceed.Document.NET.Paragraph;

namespace SchoolDB.Controllers
{
    public class HighTeacherController : Controller
    {
        // GET: HighTeacher
        SchoolContext db = new SchoolContext();
        SqlConnection connection = new SqlConnection(new Connection().ConnectionString);
        SqlCommand command;
        SqlDataReader reader;
        public ActionResult Index()
        {
            return RedirectToAction("Teachers", "HighTeacher", new
            {
                Teacher_birthday_start = new DateTime(1940, 1, 1),
                Teacher_birthday_end = DateTime.Now,
                cr = 1
            });
        }
        public ActionResult Forms(string Form_id = "", string Direction = "Всі", string Teacher_name = "")
        {
            ViewBag.Form_id = Form_id;
            ViewBag.Direction = Direction;
            ViewBag.Teacher_name = Teacher_name;
            List<string> diretions = new List<string>();
            diretions.Add("Всі");
            SqlConnection sql = new SqlConnection(new Connection().ConnectionString);
            using (sql)
            {
                sql.Open();
                command = new SqlCommand("SELECT DISTINCT Direction FROM Forms", sql);
                reader = command.ExecuteReader();
                while (reader.Read())
                    diretions.Add(reader.GetString(0));
                reader.Close();

            }

            ViewBag.Directions = diretions;

            string where = "";
            if (Form_id != "")
                where += $" AND  F.Form_id LIKE N'%{Form_id}%' ";
            if (Direction != "Всі")
                where += $" AND  F.Direction=N'{Direction}' ";
            if (Teacher_name != "")
                where += $" AND  T.Teacher_name LIKE N'%{Teacher_name}%' ";
            List<Forms> myforms = new List<Forms>();
            List<Forms> forms = new List<Forms>();
            using (connection)
            {
                connection.Open();
                command = new SqlCommand("SELECT F.Form_id, F.Direction, T.Teacher_name " +
                    "FROM Forms F " +
                    "INNER JOIN Teachers T ON F.Teacher_id=T.Teacher_id " +
                    "WHERE F.Form_id IN ( " +
                    "SELECT DISTINCT T.Form_id FROM Timetable T  " +
                    "INNER JOIN Teacher_Subject TS ON T.Teacher_Subject_id=TS.Teacher_Subject_id  " +
                    "INNER JOIN Teachers TR ON TS.Teacher_id=TR.Teacher_id " +
                    "WHERE TR.Teacher_id=" + Session["name"] + ")" + where, connection);
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    myforms.Add(new Models.Forms()
                    {
                        Form_id = reader.GetString(0),
                        Direction = reader.GetString(1),
                        Teachers = new Teachers() { Teacher_name = reader.GetString(2) }
                    });
                }
                reader.Close();

                foreach (Forms f in myforms)
                {
                    command = new SqlCommand("SELECT Student_name " +
                        "FROM Students WHERE Student_form = N'" + f.Form_id + "' " +
                        "ORDER BY 1", connection);
                    reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        f.Students.Add(new Models.Students() { Student_name = reader.GetString(0) });
                    }
                    reader.Close();

                }


                command = new SqlCommand("SELECT F.Form_id, F.Direction, T.Teacher_name " +
                   "FROM Forms F " +
                   "INNER JOIN Teachers T ON F.Teacher_id=T.Teacher_id " +
                   "WHERE F.Form_id NOT IN ( " +
                   "SELECT DISTINCT T.Form_id FROM Timetable T  " +
                   "INNER JOIN Teacher_Subject TS ON T.Teacher_Subject_id=TS.Teacher_Subject_id  " +
                   "INNER JOIN Teachers TR ON TS.Teacher_id=TR.Teacher_id " +
                   "WHERE TR.Teacher_id=" + Session["name"] + ")" + where, connection);

                reader = command.ExecuteReader();

                while (reader.Read())
                {
                    forms.Add(new Models.Forms()
                    {
                        Form_id = reader.GetString(0),
                        Direction = reader.GetString(1),
                        Teachers = new Teachers() { Teacher_name = reader.GetString(2) }
                    });
                }
                reader.Close();
                foreach (Forms f in forms)
                {
                    command = new SqlCommand("SELECT Student_name " +
                        "FROM Students WHERE Student_form = N'" + f.Form_id + "' " +
                        "ORDER BY 1", connection);
                    reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        f.Students.Add(new Models.Students() { Student_name = reader.GetString(0) });
                    }
                    reader.Close();

                }

                reader.Close();
            }

            ViewBag.MyForms = myforms;
            ViewBag.Forms = forms;
            return View();
        }
        [HttpGet]
        public ActionResult Marks(DateTime Date_of_create_start, DateTime Date_of_create_end,
            DateTime MyDate_of_create_start, DateTime MyDate_of_create_end,
            string MySubject = "Всі", string MyStudent_name = "", string MyMark = "Всі",
            string MyWork_id = "Всі", string MyStudent_form = "Всі",
            string Subject = "", string Student_name = "", string Mark = "",
            string Work_id = "Всі", string Student_form = "", string Teacher_name = "")
        {
            List<string> Mysub = new List<string>();
            Mysub.Add("Всі");
            List<string> MyMarksList = new List<string>();
            MyMarksList.Add("Всі");
            List<string> works = new List<string>();
            works.Add("Всі");


            if (MyDate_of_create_start > MyDate_of_create_end)
            {
                DateTime temp = MyDate_of_create_end;
                MyDate_of_create_end = MyDate_of_create_start;
                MyDate_of_create_start = temp;
            }

            ViewBag.MyStudent_form = MyStudent_form;
            ViewBag.MySubject = MySubject;
            ViewBag.MyStudent_name = MyStudent_name;
            ViewBag.MyMark = MyMark;
            ViewBag.MyDate_of_create_end = MyDate_of_create_end.ToString("yyyy-MM-dd");
            ViewBag.MyDate_of_create_start = MyDate_of_create_start.ToString("yyyy-MM-dd");
            ViewBag.MyWork_id = MyWork_id;

            ViewBag.Student_form = Student_form;
            ViewBag.Subject = Subject;
            ViewBag.Student_name = Student_name;
            ViewBag.Mark = Mark;
            ViewBag.Date_of_create_end = Date_of_create_end.ToString("yyyy-MM-dd");
            ViewBag.Date_of_create_start = Date_of_create_start.ToString("yyyy-MM-dd");
            ViewBag.Work_id = Work_id;
            ViewBag.Teacher_name = Teacher_name;

            SqlConnection sql = new SqlConnection(new Connection().ConnectionString);
            using (sql)
            {
                sql.Open();
                command = new SqlCommand("SELECT Subject FROM Teacher_Subject " +
                    $"WHERE Teacher_id={Session["name"]}", sql);
                reader = command.ExecuteReader();
                while (reader.Read())
                    Mysub.Add(reader.GetString(0));
                reader.Close();

                command = new SqlCommand("SELECT DISTINCT Mark FROM Marks M " +
                    "INNER JOIN Teacher_Subject TS ON M.Teacher_Subject_id=TS.Teacher_Subject_id " +
                    $"WHERE TS.Teacher_id={Session["name"]} ORDER BY 1", sql);
                reader = command.ExecuteReader();
                while (reader.Read())
                    MyMarksList.Add(reader.GetValue(0).ToString());
                reader.Close();

                command = new SqlCommand("SELECT Work_id FROM Types_of_work", sql);
                reader = command.ExecuteReader();
                while (reader.Read())
                    works.Add(reader.GetString(0));
                reader.Close();

            }
            ViewBag.MySubjects = Mysub;
            ViewBag.MyMarksList = MyMarksList;
            ViewBag.Works = works;
            string query;
            List<string> Myforms = new List<string>();
            Myforms.Add("Всі");
            List<string> forms = new List<string>();

            string Mywhere = $"";
            if (MyStudent_name != "")
                Mywhere += $" AND S.Student_name LIKE N'%{MyStudent_name}%' ";
            if (MyDate_of_create_start != null)
                Mywhere += $" AND M.Date_create>='{MyDate_of_create_start.ToString("yyyy-MM-dd")}' ";
            if (MyDate_of_create_end != null)
                Mywhere += $" AND M.Date_create<='{MyDate_of_create_end.ToString("yyyy-MM-dd")}' ";
            if (MySubject != "Всі")
                Mywhere = $" AND TS.Subject=N'{MySubject}' ";
            if (MyStudent_form != "Всі")
                Mywhere = $" AND S.Student_form=N'{MyStudent_form}' ";
            if (MyMark != "Всі")
                Mywhere = $" AND M.Mark={MyMark} ";
            if (MyWork_id != "Всі")
                Mywhere = $" AND M.Work_id=N'{MyWork_id}' ";


            string where = $"";
            if (Student_name != "")
                where += $" AND S.Student_name LIKE N'%{Student_name}%' ";
            if (Teacher_name != "")
                where += $" AND TR.Teacher_name LIKE N'%{Teacher_name}%' ";
            if (Date_of_create_start != null)
                where += $" AND M.Date_create>='{Date_of_create_start.ToString("yyyy-MM-dd")}' ";
            if (Date_of_create_end != null)
                where += $" AND M.Date_create<='{Date_of_create_end.ToString("yyyy-MM-dd")}' ";
            if (Subject != "")
                where = $" AND TS.Subject LIKE N'%{Subject}%' ";
            if (Student_form != "Всі")
                where = $" AND S.Student_form LIKE N'%{Student_form}%' ";
            if (Mark != "")
                where = $" AND CONVERT(VARCHAR(2), M.Mark) LIKE N'%{Mark}%' ";
            if (Work_id != "Всі")
                where = $" AND M.Work_id=N'{Work_id}' ";

            query = "SELECT TS.Subject, S.Student_form, S.Student_name, " +
               "M.Mark, M.Date_create, M.Work_id, S.Student_id, TS.Teacher_Subject_id " +
               "FROM Marks M INNER JOIN Students S ON M.Student_id=S.Student_id " +
               "INNER JOIN Teacher_Subject TS ON M.Teacher_Subject_id=TS.Teacher_Subject_id " +
               $"  WHERE TS.Teacher_id=1 {Mywhere} ORDER BY 1,2,3,5,4";



            string q;
            q = "SELECT TS.Subject, S.Student_form, S.Student_name, M.Mark, " +
                "M.Date_create, M.Work_id, TR.Teacher_name FROM Marks M  " +
                "INNER JOIN Students S ON M.Student_id=S.Student_id " +
                "INNER JOIN Teacher_Subject TS ON M.Teacher_Subject_id=TS.Teacher_Subject_id  " +
                "INNER JOIN Teachers TR ON TS.Teacher_id=TR.Teacher_id " +
                $" WHERE TS.Teacher_id!=1 {where} ORDER BY 7,1,2,3,5,4";
            List<Marks> myMarks = new List<Marks>();
            List<Marks> marks = new List<Marks>();

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

                command = new SqlCommand(q, connection);
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    marks.Add(new Models.Marks()
                    {
                        Teacher_Subject = new Teacher_Subject()
                        {
                            Subject = reader.GetString(0),
                            Teachers = new Models.Teachers() { Teacher_name = reader.GetString(6) }
                        },
                        Students = new Students()
                        {
                            Student_form = reader.GetString(1),
                            Student_name = reader.GetString(2)
                        },
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
                    Myforms.Add(reader.GetString(0));
                reader.Close();

                command = new SqlCommand($"SELECT DISTINCT T.Form_id FROM Timetable T " +
                    $"INNER JOIN Teacher_Subject TS ON T.Teacher_Subject_id=TS.Teacher_Subject_id " +
                    $"WHERE TS.Teacher_id!={Session["name"]}", connection);
                reader = command.ExecuteReader();
                while (reader.Read())
                    forms.Add(reader.GetString(0));
                reader.Close();

            }
            ViewBag.MyMarks = myMarks;
            ViewBag.Marks = marks;
            ViewBag.MyForms = Myforms;
            ViewBag.Forms = forms;

            return View();
        }
        [HttpPost]
        public ActionResult Marks(string action, string Form_id, DateTime Date_of_create_start, DateTime Date_of_create_end,
            DateTime MyDate_of_create_start, DateTime MyDate_of_create_end,
            string MySubject, string MyStudent_name, string MyMark,
            string MyWork_id, string MyStudent_form,
            string Subject, string Student_name, string Mark,
            string Work_id, string Student_form, string Teacher_name)
        {
            if (action == "Додати")
            {
                return RedirectToAction("CreateMark", "HighTeacher", new { id = Form_id });
            }
            else
            {
                return RedirectToAction("Marks", "HighTeacher", new
                {
                    Date_of_create_start = Date_of_create_start,
                    Date_of_create_end = Date_of_create_end,
                    MyDate_of_create_start = MyDate_of_create_start,
                    MyDate_of_create_end = MyDate_of_create_end,
                    MySubject = MySubject,
                    MyStudent_name = MyStudent_name,
                    MyMark = MyMark,
                    MyWork_id = MyWork_id,
                    MyStudent_form = MyStudent_form,
                    Subject = Subject,
                    Student_name = Student_name,
                    Mark = Mark,
                    Work_id = Work_id,
                    Student_form = Student_form,
                    Teacher_name = Teacher_name,
                });
            }
        }
        public ActionResult Rating() 
        {
            List<string> teachers = new List<string>();
            List<string> forms = new List<string>();
            List<string> subjects = new List<string>();
            string[,] Meets;  //кількість годин уроків кожного учителя в кожному класі
            string[,] CountSubject; //Кількість годин кожного предмету в тиждень у класі
            string query = "SELECT Form_id, ";

            List<string[]> MSS = new List<string[]>();
            List<string[]> MFS = new List<string[]>();
            List<string[]> MS = new List<string[]>();
            List<string[]> SD = new List<string[]>();
            List<string[]> FD = new List<string[]>();
            List<string[]> Salary = new List<string[]>();



            string d = "";
            using (connection)
            {
                connection.Open();
                command = new SqlCommand("SELECT DISTINCT Teacher_name " +
                    "FROM Form_Teacher_Meets", connection);
                reader = command.ExecuteReader();
                while (reader.Read())
                    teachers.Add(reader.GetString(0));
                reader.Close();

                command = new SqlCommand("SELECT DISTINCT Form_id " +
                    "FROM Form_Teacher_Meets", connection);
                reader = command.ExecuteReader();
                while (reader.Read())
                    forms.Add(reader.GetString(0));
                reader.Close();

                command = new SqlCommand("SELECT DISTINCT Subject " +
                    "FROM Form_Subject_Count", connection);
                reader = command.ExecuteReader();
                while (reader.Read())
                    subjects.Add(reader.GetString(0));
                reader.Close();

                foreach (string item in teachers)
                    query += $" SUM(CASE Teacher_name WHEN N'{item}' " +
                        $"THEN Count_Meets ELSE 0 END) AS \"{item}\",";
                query = query.Substring(0, query.Length - 1);
                query += " FROM Form_Teacher_Meets GROUP BY Form_id";

                Meets = new string[forms.Count + 1, teachers.Count + 1];
                Meets[0, 0] = "Клас";
                for (int i = 1; i < teachers.Count + 1; i++)
                    Meets[0, i] = teachers[i - 1];


                command = new SqlCommand(query, connection);
                reader = command.ExecuteReader();

                int k = 0;
                while (reader.Read())
                {
                    k++;
                    for (int j = 0; j < teachers.Count + 1; j++)
                        Meets[k, j] = reader.GetValue(j).ToString();
                }
                reader.Close();

                for (int i = 0; i < Meets.GetUpperBound(0); i++)
                {
                    for (int j = 0; j < Meets.GetUpperBound(1); j++)
                        d += $"{Meets[i, j]:10}";
                    d += "\n";
                }

                query = "SELECT Form_id, ";

                foreach (string item in subjects)
                    query += $" SUM(CASE Subject WHEN N'{item}' " +
                        $"THEN Count_Subject ELSE 0 END) AS \"{item}\",";
                query = query.Substring(0, query.Length - 1);
                query += " FROM Form_Subject_Count GROUP BY Form_id";

                CountSubject = new string[forms.Count + 1, subjects.Count + 1];
                CountSubject[0, 0] = "Клас";
                for (int i = 1; i < subjects.Count + 1; i++)
                    CountSubject[0, i] = subjects[i - 1];



                command = new SqlCommand(query, connection);
                reader = command.ExecuteReader();

                k = 0;
                while (reader.Read())
                {
                    k++;
                    for (int j = 0; j < subjects.Count + 1; j++)
                        CountSubject[k, j] = reader.GetValue(j).ToString();
                }
                reader.Close();


                for (int i = 0; i < CountSubject.GetUpperBound(0); i++)
                {
                    for (int j = 0; j < CountSubject.GetUpperBound(1); j++)
                        d += $"{CountSubject[i, j]:10}";
                    d += "\n";
                }

                command = new SqlCommand("SELECT Student_form, Subject, " +
                    "Teacher,Student_name,  AVG_PR, AVG_SR, AVG_KR, AVG_Mark " +
                    "FROM AVG_Marks_Students_Subjects ORDER BY 1,2,3,4", connection);
                reader = command.ExecuteReader();

                while (reader.Read())
                {
                    string[] item = new string[8];
                    for (int i = 0; i < item.Length; i++)
                        item[i] = reader.GetValue(i).ToString();
                    MSS.Add(item);
                }
                reader.Close();

                command = new SqlCommand("SELECT Student_form, Subject, Teacher, " +
                    "AVG_PR, AVG_SR, AVG_KR, AVG_KR " +
                    "FROM AVG_Marks_Forms_Subject ORDER BY 1,2,3", connection);
                reader = command.ExecuteReader();

                while (reader.Read())
                {
                    string[] item = new string[7];
                    for (int i = 0; i < item.Length; i++)
                        item[i] = reader.GetValue(i).ToString();
                    MFS.Add(item);
                }
                reader.Close();

                command = new SqlCommand("SELECT * FROM AVG_Mark_Student " +
                    "ORDER BY 1 DESC, 3", connection);
                reader = command.ExecuteReader();

                while (reader.Read())
                {
                    string[] item = new string[4];
                    for (int i = 0; i < item.Length; i++)
                        item[i] = reader.GetValue(i).ToString();
                    MS.Add(item);
                }
                reader.Close();

                command = new SqlCommand("SELECT * FROM Students_dinning " +
                    "ORDER BY 3,2", connection);
                reader = command.ExecuteReader();

                while (reader.Read())
                {
                    string[] item = new string[7];
                    for (int i = 0; i < item.Length; i++)
                        item[i] = reader.GetValue(i).ToString();
                    SD.Add(item);
                }
                reader.Close();

                command = new SqlCommand("SELECT * FROM Forms_dinning ORDER BY 3 DESC", connection);
                reader = command.ExecuteReader();

                while (reader.Read())
                {
                    string[] item = new string[3];
                    for (int i = 0; i < item.Length; i++)
                        item[i] = reader.GetValue(i).ToString();
                    FD.Add(item);
                }

                reader.Close();

                command = new SqlCommand("SELECT * FROM Salaries ORDER BY 1 DESC", connection);
                reader = command.ExecuteReader();

                while (reader.Read())
                {
                    string[] item = new string[4];
                    for (int i = 0; i < item.Length; i++)
                        item[i] = reader.GetValue(i).ToString();
                    Salary.Add(item);
                }
                reader.Close();


            }

            ViewBag.Meets = Meets;
            ViewBag.CountSubject = CountSubject;
            ViewBag.MSS = MSS;
            ViewBag.MS = MS;
            ViewBag.MFS = MFS;
            ViewBag.SD = SD;
            ViewBag.FD = FD;
            ViewBag.Salary = Salary;
            return View();
        }
        public ActionResult Students(DateTime Student_birthday_start, DateTime Student_birthday_end,
            int cr = 1, string Student_id = "", string Student_name = "", string Student_form = "",
            string Student_sex = "Всі", string Student_adress = "", string Privilege_id = "Всі",
            string Student_dinning = "Всі", string Student_post = "", string Student_post_parent = "")
        {
            if (Student_birthday_start > Student_birthday_end)
            {
                DateTime temp = Student_birthday_end;
                Student_birthday_end = Student_birthday_start;
                Student_birthday_start = temp;
            }
            SqlConnection conn = new SqlConnection(new Connection().ConnectionString);
            List<string> privilages = new List<string>();
            privilages.Add("Всі");
            using (conn)
            {
                conn.Open();
                command = new SqlCommand("SELECT Privilege_id FROM Privileges ORDER BY 1", conn);
                reader = command.ExecuteReader();
                while (reader.Read())
                    privilages.Add(reader.GetString(0));

                reader.Close();
            }

            List<string> sex = new List<string>();
            sex.Add("Всі");
            sex.Add("Чоловік");
            sex.Add("Жінка");
            List<string> dinning = new List<string>();
            dinning.Add("Всі");
            dinning.Add("Так");
            dinning.Add("Ні");

            ViewBag.Sex = sex;
            ViewBag.Privilages = privilages;
            ViewBag.Dinning = dinning;

            ViewBag.Student_birthday_start = Student_birthday_start.ToString("yyyy-MM-dd");
            ViewBag.Student_birthday_end = Student_birthday_end.ToString("yyyy-MM-dd");
            ViewBag.Student_id = Student_id;
            ViewBag.Student_name = Student_name;
            ViewBag.Student_form = Student_form;
            ViewBag.Student_sex = Student_sex;
            ViewBag.Student_adress = Student_adress;
            ViewBag.Privilege_id = Privilege_id;
            ViewBag.Student_dinning = Student_dinning;
            ViewBag.Student_post = Student_post;
            ViewBag.Student_post_parent = Student_post_parent;


            string myWhere = $"WHERE ";
            if (Student_name != "")
                myWhere += $" Student_name LIKE N'%{Student_name}%' AND";
            if (Student_form != "")
                myWhere += $" Student_form LIKE N'%{Student_form}%' AND";
            if (Student_id != "")
                myWhere += $" CONVERT(VARCHAR(10), Student_id) LIKE N'%{Student_id}%' AND";
            if (Student_birthday_start != null)
                myWhere += $" Student_birthday>='{Student_birthday_start.ToString("yyyy-MM-dd")}' AND";
            if (Student_birthday_end != null)
                myWhere += $"  Student_birthday<='{Student_birthday_end.ToString("yyyy-MM-dd")}' AND";
            if (Student_sex != "Всі")
                myWhere += $" Student_sex=N'{Student_sex} ' AND";
            if (Student_adress != "")
                myWhere += $"  Student_adress LIKE N'%{Student_adress}%' AND";
            if (Student_dinning == "Так")
                myWhere += $"  Student_dinning=1 AND";
            if (Student_dinning == "Ні")
                myWhere += $" Student_dinning=0 AND";
            if (Student_post != "")
                myWhere += $"  Student_post LIKE N'%{Student_post}%' AND";
            if (Student_post_parent != "")
                myWhere += $"  Student_post_parents LIKE N'%{Student_post_parent}%' AND";
            if (Privilege_id != "Всі")
                myWhere += $"  Privilege_id=N'{Privilege_id} ' AND";

            if (myWhere == " WHERE")
                myWhere = "";
            else
                myWhere = myWhere.Substring(0, myWhere.Length - 4);

            List<Students> students = new List<Students>();
            using (connection)
            {
                connection.Open();
                command = new SqlCommand("SELECT * FROM Students " +
                    $"  {myWhere}  ORDER BY Student_form, {cr}", connection);
                reader = command.ExecuteReader();

                while (reader.Read())
                {
                    students.Add(new Models.Students()
                    {
                        Student_id = reader.GetInt32(0),
                        Student_name = reader.GetString(1),
                        Student_form = reader.GetString(2),
                        Student_birthday = Convert.ToDateTime(reader.GetValue(3)),
                        Student_sex = reader.GetString(4),
                        Student_adress = reader.GetValue(5).ToString(),
                        Privilege_id = reader.GetString(6),
                        Student_dinning = Convert.ToBoolean(reader.GetValue(7)),
                        Student_post = reader.GetValue(8).ToString(),
                        Student_post_parents = reader.GetValue(9).ToString(),
                        Student_password = reader.GetValue(10).ToString()
                    });
                }
                reader.Close();
            }

            ViewBag.Students = students;
            return View();
        }
        public ActionResult Teachers(DateTime Teacher_birthday_start, DateTime Teacher_birthday_end,
            int cr = 1, string Teacher_id = "", string Teacher_name = "", string Teacher_form = "",
            string Teacher_sex = "Всі", string Teacher_room = "", string Category_id = "Всі",
            string Teacher_post = "")
        {
            Teachers teacher = new Teachers();

            ViewBag.Teacher_birthday_start = Teacher_birthday_start.ToString("yyyy-MM-dd");
            ViewBag.Teacher_birthday_end = Teacher_birthday_end.ToString("yyyy-MM-dd");
            ViewBag.Teacher_id = Teacher_id;
            ViewBag.Teacher_name = Teacher_name;
            ViewBag.Teacher_form = Teacher_form;
            ViewBag.Teacher_sex = Teacher_sex;
            ViewBag.Teacher_room = Teacher_room;
            ViewBag.Category_id = Category_id;
            ViewBag.Teacher_post = Teacher_post;

            List<string> sex = new List<string>();
            sex.Add("Всі");
            sex.Add("Чоловік");
            sex.Add("Жінка");
            List<string> categories = new List<string>();
            categories.Add("Всі");

            SqlConnection conn = new SqlConnection(new Connection().ConnectionString);
            using (conn)
            {
                conn.Open();
                command = new SqlCommand("SELECT Category_id FROM Categories", conn);
                reader = command.ExecuteReader();
                while (reader.Read())
                    categories.Add(reader.GetString(0));
                reader.Close();

            }

            ViewBag.Sex = sex;
            ViewBag.Categories = categories;



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



            SqlConnection high = new SqlConnection(new Connection().ConnectionString);
            using (high)
            {
                high.Open();
                command = new SqlCommand("SELECT DISTINCT TS.Subject " +
                    "FROM Teacher_Subject TS WHERE Teacher_id=" + Session["name"] +
                    " ORDER BY 1", high);
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    teacher.SubjectForms[reader.GetString(0)] = new List<string>();
                }
                reader.Close();
                command = new SqlCommand("SELECT TS.Subject, T.Form_id " +
                    "FROM Teacher_Subject TS " +
                    "INNER JOIN Timetable T ON TS.Teacher_Subject_id=T.Teacher_Subject_id " +
                    "WHERE Teacher_id=" + Session["name"] + " ORDER BY 1,2", high);
                reader = command.ExecuteReader();

                while (reader.Read())
                {
                    teacher.SubjectForms[reader.GetString(0)].Add(reader.GetString(1));
                }
                reader.Close();
            }
            ViewBag.MySubForm = teacher.SubjectForms;

            if (Teacher_birthday_start > Teacher_birthday_end)
            {
                DateTime temp = Teacher_birthday_end;
                Teacher_birthday_end = Teacher_birthday_start;
                Teacher_birthday_start = temp;
            }

            string where = " WHERE T.Teacher_id!=1 ";
            if (Teacher_name != "")
                where += $" AND T.Teacher_name LIKE N'%{Teacher_name}%' ";
            if (Teacher_post != "")
                where += $" AND T.Teacher_post LIKE N'%{Teacher_post}%' ";
            if (Teacher_form != "")
                where += $" AND F.Form_id LIKE N'%{Teacher_form}%' ";
            if (Teacher_id != "")
                where += $" AND CONVERT(VARCHAR(10), T.Teacher_id) LIKE N'%{Teacher_id}%' ";
            if (Teacher_room != "")
                where += $" AND CONVERT(VARCHAR(10), T.Teacher_room) LIKE N'%{Teacher_room}%' ";
            if (Teacher_birthday_start != null)
                where += $" AND T.Teacher_birthday>='{Teacher_birthday_start.ToString("yyyy-MM-dd")}' ";
            if (Teacher_birthday_end != null)
                where += $" AND T.Teacher_birthday<='{Teacher_birthday_end.ToString("yyyy-MM-dd")}' ";
            if (Teacher_sex != "Всі")
                where += $" AND T.Teacher_sex=N'{Teacher_sex} '";
            if (Category_id != "Всі")
                where += $" AND T.Category_id=N'{Category_id} '";


            List<Teachers> teachers = new List<Teachers>();
            SqlConnection sql = new SqlConnection(new Connection().ConnectionString);
            using (sql)
            {
                sql.Open();
                command = new SqlCommand("SELECT T.*, F.Form_id AS Form_id " +
                   "FROM Teachers T LEFT JOIN Forms F " +
                   "ON T.Teacher_id=F.Teacher_id " +
                   $" {where} ORDER BY {cr}", sql);
                reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Teachers t = new Teachers();
                    t.Teacher_id = reader.GetInt32(0);
                    t.Teacher_name = reader.GetString(1);
                    if ((object)reader.GetValue(2) == DBNull.Value)
                        t.Teacher_room = null;
                    else
                        t.Teacher_room = Convert.ToInt32(reader.GetValue(2));
                    t.Teacher_sex = reader.GetString(3);
                    t.Category_id = reader.GetString(4);
                    t.Teacher_post = reader.GetValue(5).ToString();
                    t.Teacher_password = reader.GetValue(6).ToString();
                    t.Teacher_birthday = Convert.ToDateTime(reader.GetValue(7));
                    t.Forms.Add(new Models.Forms() { Form_id = reader.GetValue(8).ToString() });
                    teachers.Add(t);
                }
                reader.Close();

                foreach (Teachers t in teachers)
                {

                    command = new SqlCommand("SELECT DISTINCT TS.Subject " +
                    "FROM Teacher_Subject TS WHERE Teacher_id=" + t.Teacher_id +
                    " ORDER BY 1", sql);

                    reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        t.SubjectForms[reader.GetString(0)] = new List<string>();
                    }
                    reader.Close();

                    command = new SqlCommand("SELECT TS.Subject, T.Form_id " +
                   "FROM Teacher_Subject TS " +
                   "INNER JOIN Timetable T ON TS.Teacher_Subject_id=T.Teacher_Subject_id " +
                   "WHERE Teacher_id=" + t.Teacher_id + " ORDER BY 1,2", sql);
                    reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        t.SubjectForms[reader.GetString(0)].Add(reader.GetString(1));
                    }
                    reader.Close();


                }

                reader.Close();
            }

            ViewBag.Teachers = teachers;


            return View();
        }
        public ActionResult Teacher_Subject(int cr = 1, string val = "")
        {
            List<Teacher_Subject> teacher_Subjects = new List<Teacher_Subject>();
            using (connection)
            {
                string query;
                if (val == "")
                    query = "SELECT Teacher_Subject.Subject, Teachers.Teacher_name, Teacher_Subject_id " +
                        "FROM Teacher_Subject " +
                    "INNER JOIN Teachers ON Teacher_Subject.Teacher_id = Teachers.Teacher_id " +
                    "ORDER BY " + cr;
                else
                    query = "SELECT Teacher_Subject.Subject, Teachers.Teacher_name, Teacher_Subject_id " +
                        "FROM Teacher_Subject " +
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
                    ts.Teacher_Subject_id = reader.GetInt32(2);
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
            string teacher = "Яковенко Галина Михайлівна", string room = "Всі")
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
        public ActionResult EditTeacher(int id)
        {
            Teachers teacher = new Teachers();
            List<string> forms = new List<string>();
            List<string> sex = new List<string>();
            List<string> categories = new List<string>();
            using (connection)
            {
                connection.Open();
                command = new SqlCommand("SELECT T.Teacher_id, T.Teacher_name, F.Form_id, " +
                    "T.Teacher_birthday, T.Teacher_sex, T.Teacher_room, T.Category_id, " +
                    "T.Teacher_post, T.Teacher_password FROM Teachers T " +
                    "LEFT JOIN Forms F ON T.Teacher_id=F.Teacher_id " +
                    "WHERE T.Teacher_id=" + id, connection);
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    teacher.Teacher_id = reader.GetInt32(0);
                    teacher.Teacher_name = reader.GetString(1);
                    if ((object)reader.GetValue(2) != DBNull.Value)
                        teacher.Forms.Add(new Models.Forms() { Form_id = reader.GetString(2) });
                    else
                        teacher.Forms.Add(new Models.Forms() { Form_id = "Без класного керівництва" });
                    teacher.Teacher_birthday = Convert.ToDateTime(reader.GetValue(3));
                    teacher.Teacher_sex = reader.GetString(4);
                    if ((object)reader.GetValue(5) == DBNull.Value)
                        teacher.Teacher_room = null;
                    else
                        teacher.Teacher_room = Convert.ToInt32(reader.GetValue(5));
                    teacher.Category_id = reader.GetString(6);
                    teacher.Teacher_post = reader.GetValue(7).ToString();
                    teacher.Teacher_password = reader.GetValue(8).ToString();

                }
                reader.Close();

                command = new SqlCommand("SELECT Form_id FROM Forms " +
                    "WHERE Teacher_id!=" + id, connection);
                reader = command.ExecuteReader();
                while (reader.Read())
                    forms.Add(reader.GetString(0));
                reader.Close();

                command = new SqlCommand("SELECT DISTINCT Teacher_sex FROM Teachers " +
                    "WHERE Teacher_sex != " +
                    "(SELECT DISTINCT Teacher_sex FROM Teachers " +
                    "WHERE Teacher_id=" + id + ")", connection);
                reader = command.ExecuteReader();
                while (reader.Read())
                    sex.Add(reader.GetString(0));
                reader.Close();

                command = new SqlCommand("SELECT DISTINCT Category_id FROM Teachers " +
                    "WHERE Category_id != " +
                    "(SELECT DISTINCT Category_id FROM Teachers " +
                    "WHERE Teacher_id=" + id + ")", connection);
                reader = command.ExecuteReader();
                while (reader.Read())
                    categories.Add(reader.GetString(0));

                reader.Close();
            }

            if (teacher.Forms.First().Form_id != "Без класного керівництва")
                forms.Add("Без класного керівництва");
            ViewBag.Form = teacher.Forms.First().Form_id;
            ViewBag.Info = teacher;
            ViewBag.Forms = forms;
            ViewBag.Sex = sex;
            ViewBag.Categories = categories;
            return View();
        }


        [HttpPost]
        public ActionResult EditTeacher(int Teacher_id, string Teacher_name, string Teacher_room,
            string Teacher_sex, string Category_id, DateTime Teacher_Birthday, string Form_start,
            string Form_id = "", string Teacher_post = "", string Teacher_password = ""
            )
        {

            if (Teacher_room == "")
                Teacher_room = "NULL";
            using (connection)
            {
                connection.Open();
                if (Form_id != "")
                {
                    command = new SqlCommand($"UPDATE Teachers SET " +
                        $"Teacher_name=N'{Teacher_name}', " +
                        $"Teacher_room={Teacher_room}," +
                        $"Teacher_sex=N'{Teacher_sex}', " +
                        $"Category_id=N'{Category_id}', " +
                        $"Teacher_Birthday='{Teacher_Birthday.ToString("yyyy-MM-dd")}' " +
                        $"WHERE Teacher_id={Teacher_id}", connection);
                    command.ExecuteNonQuery();

                    if (Form_start == "Без класного керівництва" &&
                        Form_id != "Без класного керівництва")
                    {
                        command = new SqlCommand($"UPDATE Forms SET" +
                            $" Teacher_id={Teacher_id }" +
                            $"WHERE Form_id=N'{Form_id}'", connection);
                        command.ExecuteNonQuery();
                    }
                    else if (Form_start != "Без класного керівництва" &&
                        Form_id == "Без класного керівництва")
                    {
                        command = new SqlCommand("SELECT MAX(Teacher_id) " +
                            "FROM Teachers " +
                            "WHERE Teacher_id NOT IN(" +
                            "SELECT Teacher_id FROM Forms)", connection);
                        string newTeacher = command.ExecuteScalar().ToString();
                        command = new SqlCommand($"UPDATE Forms SET " +
                            $"Teacher_id={newTeacher}" +
                            $"WHERE Form_id=N'{Form_start}'", connection);
                        command.ExecuteNonQuery();
                    }
                    else if (Form_start != "Без класного керівництва" &&
                        Form_id != "Без класного керівництва")
                    {
                        command = new SqlCommand($"SELECT Teacher_id " +
                            $"FROM Forms " +
                            $"WHERE Form_id=N'{Form_id}'", connection);
                        string anotherTeacher = command.ExecuteScalar().ToString();
                        command = new SqlCommand($"UPDATE Forms SET " +
                            $"Teacher_id={anotherTeacher}" +
                            $"WHERE Form_id=N'{Form_start}'", connection);
                        command.ExecuteNonQuery();
                        command = new SqlCommand($"UPDATE Forms SET " +
                            $"Teacher_id={Teacher_id}" +
                            $"WHERE Form_id=N'{Form_id}'", connection);
                        command.ExecuteNonQuery();
                    }
                }
                else
                {
                    command = new SqlCommand($"UPDATE Teachers SET " +
                      $"Teacher_name=N'{Teacher_name}', " +
                      $"Teacher_room={Teacher_room}," +
                      $"Teacher_sex=N'{Teacher_sex}', " +
                      $"Category_id=N'{Category_id}', " +
                      $"Teacher_post='{Teacher_post}', " +
                      $"Teacher_password=N'{Teacher_password}', " +
                      $"Teacher_Birthday=N'{Teacher_Birthday.ToString("yyyy-MM-dd")}' " +
                      $"WHERE Teacher_id={Teacher_id}", connection);
                    command.ExecuteNonQuery();
                }

                connection.Close();
            }

            return RedirectToAction("EditTeacher", "HighTeacher", new { id = Teacher_id });
        }


        [HttpGet]
        public ActionResult EditStudent(int id)
        {
            Students student = new Students();
            List<string> forms = new List<string>();
            List<string> sex = new List<string>();
            List<string> privilages = new List<string>();
            using (connection)
            {
                connection.Open();
                command = new SqlCommand("SELECT Student_id, Student_name, Student_form, " +
                    "Student_birthday, Student_sex, Student_adress,Privilege_id, " +
                    "Student_dinning FROM Students WHERE Student_id = " + id, connection);
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    student.Student_id = reader.GetInt32(0);
                    student.Student_name = reader.GetString(1);
                    student.Student_form = reader.GetString(2);
                    student.Student_birthday = Convert.ToDateTime(reader.GetValue(3));
                    student.Student_sex = reader.GetString(4);
                    student.Student_adress = reader.GetValue(5).ToString();
                    student.Privilege_id = reader.GetString(6);
                    student.Student_dinning = reader.GetBoolean(7);
                }
                reader.Close();


                command = new SqlCommand($"SELECT Form_id FROM Forms " +
                    $"WHERE Form_id!=N'{student.Student_form}'", connection);
                reader = command.ExecuteReader();
                while (reader.Read())
                    forms.Add(reader.GetString(0));
                reader.Close();

                command = new SqlCommand($"SELECT DISTINCT Student_sex " +
                    $"FROM Students " +
                    $"WHERE Student_sex!=N'{student.Student_sex}'", connection);
                reader = command.ExecuteReader();
                while (reader.Read())
                    sex.Add(reader.GetString(0));
                reader.Close();

                command = new SqlCommand($"SELECT Privilege_id " +
                    $"FROM Privileges " +
                    $"WHERE Privilege_id!=N'{student.Privilege_id}'", connection);
                reader = command.ExecuteReader();
                while (reader.Read())
                    privilages.Add(reader.GetString(0));

                reader.Close();
            }

            ViewBag.Info = student;
            ViewBag.Forms = forms;
            ViewBag.Sex = sex;
            ViewBag.Privilages = privilages;
            return View();
        }
        [HttpPost]
        public ActionResult EditStudent(int Student_id, string Student_name, string Student_form,
            DateTime Student_birthday, string Student_sex, string Student_adress,
            string Privilege_id, bool Student_dinning)
        {
            int dinning = Student_dinning ? 1 : 0;
            using (connection)
            {
                connection.Open();
                command = new SqlCommand($"UPDATE Students SET " +
                    $"Student_name=N'{Student_name}'," +
                    $"Student_form=N'{Student_form}'," +
                    $"Student_birthday='{Student_birthday.ToString("yyyy-MM-dd")}'," +
                    $"Student_sex=N'{Student_sex}'," +
                    $"Student_adress=N'{Student_adress}'," +
                    $"Privilege_id=N'{Privilege_id}'," +
                    $"Student_dinning={dinning}" +
                    $" WHERE Student_id=" + Student_id, connection);
                command.ExecuteNonQuery();
                connection.Close();
            }
            return RedirectToAction("EditStudent", "HighTeacher", new { id = Student_id });

        }

        [HttpGet]
        public ActionResult EditForm(string id)
        {
            Forms form = new Forms();
            List<string> teachers = new List<string>();
            List<string> directions = new List<string>();
            using (connection)
            {
                connection.Open();
                command = new SqlCommand($"SELECT F.Form_id, " +
                    $"T.Teacher_name, F.Direction, F.Teacher_id " +
                    $"FROM Forms F " +
                    $"LEFT JOIN Teachers T ON F.Teacher_id=T.Teacher_id " +
                    $"WHERE Form_id=N'{id}'", connection);
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    form.Form_id = reader.GetString(0);
                    form.Teachers = new Teachers() { Teacher_name = reader.GetString(1) };
                    form.Direction = reader.GetString(2);
                    form.Teacher_id = reader.GetInt32(3);
                }
                reader.Close();

                command = new SqlCommand($"SELECT DISTINCT Direction FROM Forms " +
                    $"WHERE Direction!=N'{form.Direction}'", connection);
                reader = command.ExecuteReader();
                while (reader.Read())
                    directions.Add(reader.GetString(0));
                reader.Close();

                command = new SqlCommand($"SELECT Teacher_name FROM Teachers " +
                    $"WHERE Teacher_name NOT IN (N'{form.Teachers.Teacher_name}'," +
                    $"N'Яковенко Галина Михайлівна')", connection);
                reader = command.ExecuteReader();
                while (reader.Read())
                    teachers.Add(reader.GetString(0));

                reader.Close();
            }
            ViewBag.Info = form;
            ViewBag.Teachers = teachers;
            ViewBag.Directions = directions;

            return View();
        }

        [HttpPost]
        public ActionResult EditForm(string Form_id, string Direction,
            string Teacher_name, int Teacher_id)
        {

            using (connection)
            {
                connection.Open();
                command = new SqlCommand($"UPDATE Forms SET " +
                    $"Direction=N'{Direction}' WHERE " +
                    $"Form_id=N'{Form_id}'", connection);
                command.ExecuteNonQuery();

                command = new SqlCommand($"SELECT COUNT(Form_id) " +
                    $"FROM Forms WHERE Teacher_id=(SELECT Teacher_id " +
                    $"FROM Teachers WHERE Teacher_name=N'{Teacher_name}')", connection);

                int count = Convert.ToInt32(command.ExecuteScalar());

                command = new SqlCommand($"SELECT Teacher_id FROM Teachers " +
                    $"WHERE Teacher_name=N'{Teacher_name}'", connection);

                int NewTeacher_id = Convert.ToInt32(command.ExecuteScalar());

                if (count == 0)
                {
                    command = new SqlCommand($"UPDATE Forms_id SET " +
                        $"Teacher_id={NewTeacher_id}  WHERE " +
                    $"Form_id=N'{Form_id}'", connection);
                    command.ExecuteNonQuery();
                }
                else
                {
                    command = new SqlCommand("SELECT Form_id FROM Forms " +
                        $"WHERE Teacher_id={NewTeacher_id}", connection);
                    string NewForm_id = command.ExecuteScalar().ToString();
                    command = new SqlCommand("UPDATE Forms SET " +
                        $"Teacher_id={Teacher_id} WHERE Form_id=N'{NewForm_id}'",
                        connection);
                    command.ExecuteNonQuery();
                    command = new SqlCommand("UPDATE Forms SET " +
                        $"Teacher_id={NewTeacher_id} WHERE Form_id=N'{Form_id}'",
                        connection);
                    command.ExecuteNonQuery();
                }

            }
            return RedirectToAction("EditForm", "HighTeacher", new { id = Form_id });
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
            return RedirectToAction("EditMark", "HighTeacher", new
            {
                Student_id = Student_id,
                Date_create = Date_create,
                Teacher_Subject_id = Teacher_Subject_id
            });
        }

        [HttpGet]
        public ActionResult CreateTeacher(DateTime Teacher_Birthday, string Teacher_name = "Введіть ПІБ",
            string Teacher_room = "Введіть номер кабінету", string Teacher_password = "11111111",
            string mes = "")
        {
            List<string> forms = new List<string>();
            forms.Add("Без класного керівництва");
            List<string> sex = new List<string>();
            sex.Add("Чоловік");
            sex.Add("Жінка");
            List<string> categories = new List<string>();



            using (connection)
            {
                connection.Open();
                command = new SqlCommand("SELECT Form_id FROM Forms", connection);
                reader = command.ExecuteReader();
                while (reader.Read())
                    forms.Add(reader.GetString(0));
                reader.Close();

                command = new SqlCommand("SELECT Category_id FROM Categories", connection);
                reader = command.ExecuteReader();
                while (reader.Read())
                    categories.Add(reader.GetString(0));


                reader.Close();
            }
            ViewBag.Forms = forms;
            ViewBag.Sex = sex;
            ViewBag.Categories = categories;
            ViewBag.Mes = mes;
            return View();
        }

        [HttpPost]
        public ActionResult CreateTeacher(DateTime Teacher_Birthday, string Teacher_name = "", string Teacher_room = "NULL",
            string Form_id = "Без класного керівництва", string Teacher_sex = "",
            string Category_id = "", string Teacher_post = "", string Teacher_password = "11111111")
        {


            Teacher_post = Teacher_post.Replace(" ", "");

            if (Teacher_post == "")
                return RedirectToAction("CreateTeacher", "HighTeacher",
                        new
                        {
                            Teacher_Birthday = Teacher_Birthday,
                            Teacher_name = Teacher_name,
                            Teacher_room = Teacher_room,
                            Teacher_password = Teacher_password,
                            mes = "Введіть пошту"
                        });



            if (Teacher_post.IndexOf("@") == -1)
                Teacher_post += "@gmail.com";

            SqlConnection sql = new SqlConnection(new Connection().ConnectionString);

            using (sql)
            {
                sql.Open();
                command = new SqlCommand("SELECT COUNT(Teacher_id) FROM Teachers WHERE " +
                    $"Teacher_post='{Teacher_post}'", sql);
                if (Convert.ToInt32(command.ExecuteScalar()) != 0)
                    return RedirectToAction("CreateTeacher", "HighTeacher",
                        new
                        {
                            Teacher_Birthday = Teacher_Birthday,
                            Teacher_name = Teacher_name,
                            Teacher_room = Teacher_room,
                            Teacher_password = Teacher_password,
                            mes = "Введіть іншу пошту"
                        });

            }
            try
            {
                Convert.ToInt32(Teacher_room);
            }
            catch
            {
                Teacher_room = "NULL";
            }

            using (connection)
            {
                connection.Open();
                command = new SqlCommand("INSERT INTO Teachers (Teacher_Birthday," +
                    "Teacher_name, Teacher_room, Teacher_sex, Category_id," +
                    "Teacher_post, Teacher_password) VALUES" +
                    $"('{Teacher_Birthday.ToString("yyyy-MM-dd")}', " +
                    $"N'{Teacher_name}', {Teacher_room}," +
                    $"N'{Teacher_sex}', N'{Category_id}'," +
                    $"'{Teacher_post}', N'{Teacher_password}')", connection);
                command.ExecuteNonQuery();


                if (Form_id != "Без класного керівництва")
                {
                    command = new SqlCommand("SELECT Teacher_id FROM Teachers " +
                   $"WHERE Teacher_post='{Teacher_post}' AND " +
                    $"Teacher_password='{Teacher_password}'", connection);
                    int id = Convert.ToInt32(command.ExecuteScalar());
                    command = new SqlCommand("UPDATE Forms SET " +
                    $"Teacher_id={id} WHERE Form_id=N'{Form_id}'",
                    connection);
                    command.ExecuteNonQuery();
                }


            }

            return RedirectToAction("Teachers", "HighTeacher", new
            {
                Teacher_birthday_start = new DateTime(1940, 1, 1),
                Teacher_birthday_end = DateTime.Now,
                cr = 1
            });
        }

        [HttpGet]
        public ActionResult CreateTeacherSubject()
        {
            List<Teachers> teachers = new List<Teachers>();
            List<string> subject = new List<string>();
            subject.Add("Алгебра");
            subject.Add("Англійська мова");
            subject.Add("Астрономія");
            subject.Add("Біологія");
            subject.Add("Всесвітня історія");
            subject.Add("Географія");
            subject.Add("Геометрія");
            subject.Add("Екологія");
            subject.Add("Економіка");
            subject.Add("Зарубіжна література");
            subject.Add("Захист Вітчизни");
            subject.Add("Інформатика");
            subject.Add("Історія України");
            subject.Add("Людина і світ");
            subject.Add("Музичне мистецтво");
            subject.Add("Німецька мова");
            subject.Add("Правознавство");
            subject.Add("Технології");
            subject.Add("Трудове навчання");
            subject.Add("Українська література");
            subject.Add("Українська мова");
            subject.Add("Фізика");
            subject.Add("Фізична культура");
            subject.Add("Хімія");
            subject.Add("Художня культура");

            using (connection)
            {
                connection.Open();
                command = new SqlCommand("SELECT Teacher_id, Teacher_name FROM Teachers",
                    connection);
                reader = command.ExecuteReader();
                while (reader.Read())
                    teachers.Add(new Models.Teachers()
                    {
                        Teacher_id = reader.GetInt32(0),
                        Teacher_name = reader.GetString(1)
                    });
                reader.Close();

            }
            ViewBag.Teachers = teachers;
            ViewBag.Subjects = subject;
            return View();
        }

        [HttpPost]
        public ActionResult CreateTeacherSubject(string Teacher_id, string Subjects)
        {
            using (connection)
            {
                connection.Open();
                command = new SqlCommand($"SELECT COUNT(*) FROM Teacher_Subject " +
                    $"WHERE Teacher_id={Teacher_id} " +
                    $"AND Subject=N'{Subjects}'", connection);
                int count = Convert.ToInt32(command.ExecuteScalar());
                if (count > 0)
                    return RedirectToAction("CreateTeacherSubject", "HighTeacher");

                command = new SqlCommand($"INSERT INTO Teacher_Subject " +
                    $"(Teacher_id, Subject) VALUES ({Teacher_id}," +
                    $"N'{Subjects}')", connection);
                command.ExecuteNonQuery();
            }
            return RedirectToAction("CreateTeacherSubject", "HighTeacher");
        }

        [HttpGet]
        public ActionResult CreateForm()
        {
            List<Teachers> teachers = new List<Teachers>();
            List<string> directions = new List<string>();
            directions.Add("Філологічне");
            directions.Add("Фізико-математичне");
            directions.Add("Хіміко-біологічне");
            using (connection)
            {
                connection.Open();
                command = new SqlCommand("SELECT Teacher_id, Teacher_name " +
                    "FROM Teachers " +
                    "WHERE Teacher_id NOT IN " +
                    "(SELECT Teacher_id FROM Forms) AND " +
                    "Teacher_id!=1", connection);
                reader = command.ExecuteReader();

                while (reader.Read())
                    teachers.Add(new Models.Teachers()
                    {
                        Teacher_id = reader.GetInt32(0),
                        Teacher_name = reader.GetString(1)
                    });

                reader.Close();
            }
            ViewBag.Teachers = teachers;
            ViewBag.Directions = directions;
            return View();
        }

        [HttpPost]
        public ActionResult CreateForm(string Form_id, int Teacher_id, string Direction)
        {
            using (connection)
            {
                connection.Open();
                command = new SqlCommand("INSERT INTO Forms " +
                    "(Form_id, Teacher_id, Direction) VALUES " +
                    $"(N'{Form_id}', {Teacher_id}, N'{Direction}')", connection);
                command.ExecuteNonQuery();
            }
            return RedirectToAction("Forms", "HighTeacher");
        }

        [HttpGet]
        public ActionResult CreateStudent(string Student_name = "", string Student_adress = "",
            string Student_post = "", string Student_post_parents = "",
            string Student_password = "", string mes = "")
        {
            List<string> forms = new List<string>();
            List<string> sex = new List<string>();
            sex.Add("Чоловік");
            sex.Add("Жінка");
            List<string> privileges = new List<string>();

            using (connection)
            {
                connection.Open();
                command = new SqlCommand("SELECT Form_id FROM Forms", connection);
                reader = command.ExecuteReader();

                while (reader.Read())
                    forms.Add(reader.GetString(0));
                reader.Close();

                command = new SqlCommand("SELECT Privilege_id FROM Privileges", connection);
                reader = command.ExecuteReader();

                while (reader.Read())
                    privileges.Add(reader.GetString(0));
                reader.Close();

            }
            ViewBag.Forms = forms;
            ViewBag.Sex = sex;
            ViewBag.Privileges = privileges;
            ViewBag.Mes = mes;

            return View();
        }
        [HttpPost]
        public ActionResult CreateStudent(string Student_name, string Form_id,
            DateTime Student_Birthday, string Student_sex, string Student_adress,
            string Privilege_id, bool Student_dinning, string Student_post_parents,
            string Student_post, string Student_password)
        {
            Student_post = Student_post.Replace(" ", "");

            if (Student_post == "")
                return RedirectToAction("CreateStudent", "HighTeacher",
                        new
                        {
                            Student_name = Student_name,
                            Student_adress = Student_adress,
                            Student_post = Student_post,
                            Student_post_parents = Student_post_parents,
                            Student_password = Student_password,
                            mes = "Введіть вашу пошту"
                        });



            if (Student_post.IndexOf("@") == -1)
                Student_post += "@gmail.com";

            SqlConnection sql = new SqlConnection(new Connection().ConnectionString);

            using (sql)
            {
                sql.Open();
                command = new SqlCommand("SELECT COUNT(Student_id) FROM Students WHERE " +
                    $"Student_post='{Student_post}'", sql);
                if (Convert.ToInt32(command.ExecuteScalar()) != 0)
                    return RedirectToAction("CreateStudent", "HighTeacher",
                       new
                       {
                           Student_name = Student_name,
                           Student_adress = Student_adress,
                           Student_post = Student_post,
                           Student_post_parents = Student_post_parents,
                           Student_password = Student_password,
                           mes = "Введіть іншу пошту"
                       });

            }

            using (connection)
            {
                connection.Open();
                int d = Student_dinning ? 1 : 0;
                command = new SqlCommand($"INSERT INTO Students (Student_name, " +
                    $"Student_form, Student_Birthday,Student_sex, Student_adress, " +
                    $"Privilege_id, Student_dinning, Student_post_parents,Student_post, " +
                    $"Student_password) VALUES (N'{Student_name}', N'{Form_id}', " +
                    $"'{Student_Birthday.ToString("yyyy-MM-dd")}', " +
                    $"N'{Student_sex}', N'{Student_adress}', N'{Privilege_id}', " +
                    $"{d}, '{Student_post_parents}', " +
                    $"'{Student_post}', N'{Student_password}')", connection);
                command.ExecuteNonQuery();
            }
            return RedirectToAction("Students", "HighTeacher", new
            {
                Student_birthday_start = new DateTime(2000, 1, 1),
                Student_birthday_end = DateTime.Now
            });
        }

        [HttpGet]
        public ActionResult CreateTimetable()
        {
            List<string> forms = new List<string>();
            List<Teacher_Subject> ts = new List<Teacher_Subject>();

            using (connection)
            {
                connection.Open();
                command = new SqlCommand("SELECT Form_id FROM Forms", connection);
                reader = command.ExecuteReader();

                while (reader.Read())
                    forms.Add(reader.GetString(0));
                reader.Close();

                command = new SqlCommand("SELECT TS.Teacher_Subject_id, TS.Subject, " +
                    "T.Teacher_name FROM Teacher_Subject TS LEFT JOIN Teachers T " +
                    "ON TS.Teacher_id=T.Teacher_id ORDER BY 2", connection);
                reader = command.ExecuteReader();

                while (reader.Read())
                    ts.Add(new Models.Teacher_Subject()
                    {
                        Teacher_Subject_id = reader.GetInt32(0),
                        Subject = reader.GetString(1),
                        Teachers = new Teachers() { Teacher_name = reader.GetString(2) }
                    });

                reader.Close();
            }
            ViewBag.Forms = forms;
            ViewBag.TS = ts;
            return View();
        }

        [HttpPost]
        public ActionResult CreateTimetable(string Day_Of_Week, int Lesson_id,
            string Form_id, int Teacher_Subject_id, string Classroom)
        {
            int room;
            try
            {
                room = Convert.ToInt32(Classroom);
            }
            catch
            {
                room = new Random().Next(20, 40);
            }

            using (connection)
            {
                connection.Open();
                command = new SqlCommand("SELECT COUNT(*) FROM Timetable " +
                    $"WHERE Day_Of_Week='{Day_Of_Week}' " +
                    $"AND Form_id=N'{Form_id}' " +
                    $"AND Lesson_id={Lesson_id}", connection);
                int count = Convert.ToInt32(command.ExecuteScalar());

                if (count == 0)
                    command = new SqlCommand("INSERT INTO Timetable " +
                       $"(Day_Of_Week, Lesson_id, Form_id, Teacher_Subject_id, Classroom) VALUES " +
                       $"('{Day_Of_Week}', {Lesson_id}, N'{Form_id}', " +
                       $"{Teacher_Subject_id}, {room})", connection);
                else
                    command = new SqlCommand("UPDATE Timetable SET " +
                      $"Day_Of_Week='{Day_Of_Week}'," +
                      $"Lesson_id={Lesson_id}," +
                      $"Form_id=N'{Form_id}'," +
                      $"Teacher_Subject_id={Teacher_Subject_id}," +
                      $"Classroom={room} " +
                        $"WHERE Day_Of_Week='{Day_Of_Week}' " +
                  $"AND Form_id=N'{Form_id}' " +
                  $"AND Lesson_id={Lesson_id}", connection);
                command.ExecuteNonQuery();

            }
            return RedirectToAction("Timetable", "HighTeacher");
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
            return RedirectToAction("Marks", "HighTeacher", new
            {
                Date_of_create_start = new DateTime(2020, 9, 1),
                Date_of_create_end = DateTime.Now,
                MyDate_of_create_start = new DateTime(2020, 9, 1),
                MyDate_of_create_end = DateTime.Now,
                Subject = "Всі",
                Student_name = "",
                Mark = "Всі",
                Work_id = "Всі",
                Student_form = "Всі"
            });
        }


        [HttpGet]
        public ActionResult DeleteTimetable(string Form_id, string Day_Of_Week, string Lesson_id)
        {
            string mes = "";
            using (connection)
            {
                connection.Open();
                command = new SqlCommand($"SELECT T.Day_Of_Week, T.Lesson_id, T.Form_id, T.Classroom, " +
                    $"TS.Subject FROM TIMETABLE T INNER JOIN Teacher_Subject TS " +
                    $" ON  T.Teacher_Subject_id=TS.Teacher_Subject_id WHERE " +
                    $" T.Form_id=N'{Form_id}' AND " +
                    $" T.Day_Of_Week='{Day_Of_Week}' AND " +
                    $" T.Lesson_id={Lesson_id}", connection);
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    string day = "";
                    if (reader.GetString(0) == "Monday")
                        day = "Понеділок";
                    else if (reader.GetString(0) == "Tuesday")
                        day = "Вівторок";
                    else if (reader.GetString(0) == "Wednesday")
                        day = "Середа";
                    else if (reader.GetString(0) == "Thursday")
                        day = "Четвер";
                    else if (reader.GetString(0) == "Friday")
                        day = "П'ятниця";
                    mes += $"Ви дійсно хочете видалити {reader.GetInt32(1)} урок {reader.GetString(2)} класу " +
                        $"з предмету {reader.GetString(4)} в {day} в кабінеті {reader.GetInt32(3)}";
                }
                reader.Close();

            }
            ViewBag.Mes = mes;
            return View();
        }
        [HttpPost]
        public ActionResult DeleteTimetable(string action, string Form_id, string Day_Of_Week, string Lesson_id)
        {
            if (action == "Так")
            {
                using (connection)
                {
                    connection.Open();
                    command = new SqlCommand($"DELETE FROM Timetable WHERE " +
                        $"Form_id=N'{Form_id}' AND " +
                        $"Day_Of_Week='{Day_Of_Week}' AND " +
                        $"Lesson_id={Lesson_id}", connection);
                    command.ExecuteNonQuery();
                }
            }
            return RedirectToAction("Timetable", "HighTeacher");

        }
        [HttpGet]
        public ActionResult DeleteStudent(int id)
        {
            string mes = "";
            
                using (connection)
                {
                    connection.Open();
                    command = new SqlCommand("SELECT Student_name, Student_form FROM Students " +
                        "WHERE Student_id=" + id, connection);
                reader = command.ExecuteReader();
                while (reader.Read())
                    mes += $"Ви дійсно хочете видалити {reader.GetString(0)} з {reader.GetString(1)} класу?";
                reader.Close();

            }

            ViewBag.Mes = mes;

            return View();
        }
        [HttpPost]
        public ActionResult DeleteStudent (string action, int id)
        {
            if (action == "Так")
            {
                using (connection)
                {
                    connection.Open();
                    command = new SqlCommand("DELETE FROM Students " +
                        "WHERE Student_id=" + id, connection);
                    command.ExecuteNonQuery();
                }
            }
            return RedirectToAction("Students", "HighTeacher", new
            {
                Student_birthday_start = new DateTime(2000, 1, 1),
                Student_birthday_end = DateTime.Now
            });
        }
        [HttpGet]
        public ActionResult DeleteTeacher(int id)
        {
            string mes = "";
                using (connection)
                {
                    connection.Open();
                    
                    command = new SqlCommand("Select Teacher_name FROM Teachers WHERE Teacher_id=" + id,
                        connection);
                mes += $"Ви дійсно хочете видалити вчителя {command.ExecuteScalar().ToString()}?";
                }
            ViewBag.Mes = mes;


            return View();
        }
        [HttpPost]
        public ActionResult DeleteTeacher (string action, int id)
        {
            if (action == "Так")
            {
                using (connection)
                {
                    connection.Open();
                    command = new SqlCommand("UPDATE Forms SET " +
                        "Teacher_id=(SELECT MAX(Teacher_id) FROM Teachers " +
                        "WHERE Teacher_id NOT IN (SELECT Teacher_id FROM Forms)) " +
                        "WHERE Teacher_id=" + id, connection);
                    command.ExecuteNonQuery();

                    command = new SqlCommand("DELETE FROM Teachers WHERE Teacher_id=" + id,
                        connection);
                    command.ExecuteNonQuery();
                }
            }
            return RedirectToAction("Teachers", "HighTeacher", new
            {
                Teacher_birthday_start = new DateTime(1940, 1, 1),
                Teacher_birthday_end = DateTime.Now,
                cr = 1
            });
        }
        [HttpGet]
        public ActionResult DeleteTeacherSubject(int id)
        {
            string mes = "";
            using (connection)
            {
                connection.Open();
                command = new SqlCommand("Select Teacher_name, Subject " +
                    "FROM Teacher_Subject TS " +
                    "INNER JOIN TEACHERS T ON TS.Teacher_id=T.Teacher_id " +
                    "WHERE Teacher_Subject_id=" + id, connection);
                reader = command.ExecuteReader();
                while (reader.Read())
                    mes += $"Ви дійсно хочете видалити запис {reader.GetString(0)} - {reader.GetString(1)}";
                reader.Close();

            }
            ViewBag.Mes = mes;
            return View();
        }
        [HttpPost]
        public ActionResult DeleteTeacherSubject(string action, int id)
        {
            if (action == "Так")
            {
                using (connection)
                {
                    connection.Open();
                    command = new SqlCommand("DELETE FROM Teacher_Subject " +
                        "WHERE Teacher_Subject_id=" + id, connection);
                    command.ExecuteNonQuery();
                }
            }
            return RedirectToAction("Teacher_Subject", "HighTeacher");
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
                while (reader.Read())
                    mes += $"Ви дійсно хочете видалити оцінку {reader.GetInt32(2).ToString()} " +
                        $"учня {reader.GetString(0)} з предмету {reader.GetString(1)}" +
            $" за {Convert.ToDateTime(reader.GetValue(3)).ToString("yyyy-MM-dd")} число за такий тип роботи : {reader.GetString(4)}?";
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
            return RedirectToAction("Marks", "HighTeacher", new
            {
                Date_of_create_start = new DateTime(2020, 9, 1),
                Date_of_create_end = DateTime.Now,
                MyDate_of_create_start = new DateTime(2020, 9, 1),
                MyDate_of_create_end = DateTime.Now,
                Subject = "Всі",
                Student_name = "",
                Mark = "Всі",
                Work_id = "Всі",
                Student_form = "Всі"
            });
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

            return RedirectToAction("Forms", "HighTeacher");
        }


        public ActionResult DownloadForParent(string id)
        {
            MemoryStream stream;
            string name;
            try
            {
                CreateDoc.CreateForParents(id, out stream, out name);
                return File(stream.ToArray(), "application/octet-stream", name);
            }
            catch { }

            return RedirectToAction("Rating", "HighTeacher");
        }

        public ActionResult DownloadTab (string id)
        {
            MemoryStream stream;
            string name;
            try
            {
                CreateDoc.CreateTab(id, out stream, out name);
                return File(stream.ToArray(), "application/octet-stream", name);

            }
            catch { }

            return RedirectToAction("Rating", "HighTeacher");

        }

        public ActionResult DownloadInvoice (string id)
        {
            MemoryStream stream;
            string name;
            try
            {
                CreateDoc.CreateInvoice(id, out stream, out name);
                return File(stream.ToArray(), "application/octet-stream", name);

            }
            catch { }

            return RedirectToAction("Rating", "HighTeacher");

        }

        public ActionResult SendForParent(string id = "")
        {
            using (connection)
            {
                connection.Open();
                string q = "SELECT Student_id, " +
                    "Student_post_parents FROM Students ";
                if (id != "")
                    q += $" WHERE Student_id={id}";
                command = new SqlCommand(q, connection);
                reader = command.ExecuteReader();
                while (reader.Read())
                    Send(reader.GetValue(0).ToString(), reader.GetValue(1).ToString());
                reader.Close();


            }


            return RedirectToAction("Rating", "HighTeacher");
        }

     private static void Send(string id, string post)
        {
            string path = "";
            try
            {
                
                MailAddress from = new MailAddress("courseprojectbd@gmail.com", "Яковенко Галина Михайлівна");
                MailAddress to = new MailAddress(post);
                MailMessage m = new MailMessage(from, to);
                m.Subject = "Успішність дитини";
                m.Body = "<h2>Шановні батьки! Перегляньте зауваження щодо успішності вашої дитини в документі</h2>";
                m.IsBodyHtml = true;
                try
                {
                    CreateDoc.CreateForParents(id, out path);
                    m.Attachments.Add(new Attachment(path));
                }
                catch { }
                SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
                smtp.Credentials = new NetworkCredential("courseprojectbd@gmail.com", "courseprojectpassword");
                smtp.EnableSsl = true;
                smtp.Send(m);

                

            }
            catch
            {

            }

        }
    }
}