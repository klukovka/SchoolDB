using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SchoolDB.Models;
using System.Data;
using SautinSoft.Document;
using SchoolDB.Controllers;
using Microsoft.Win32;
using SautinSoft.Document.Drawing;
using System.Net.Mail;
using System.Net;
using Xceed.Words.NET;
using Xceed.Document.NET;
using System.IO;
using Paragraph = Xceed.Document.NET.Paragraph;


namespace SchoolDB.Models
{
    public static class CreateDoc
    {
        static SchoolContext db = new SchoolContext();
        static SqlConnection connection = new SqlConnection(new Connection().ConnectionString);
        static SqlCommand command;
        static SqlDataReader reader;
     
        private static void GetLine(ref Paragraph par, string val1, string val2)
        {
            par.Append(val1)
                .Font(new Xceed.Document.NET.Font("Times New Roman"))
                .FontSize(16)
                .Italic()
                .Append(val2)
                .Font(new Xceed.Document.NET.Font("Times New Roman"))
                .FontSize(16)
                .Bold();

            par.AppendLine();
        }
        public static void CreateForm(string id, out MemoryStream stream, out string name)
        {
            

            Forms form = new Forms();
            using (connection)
            {
                connection = new SqlConnection(new Connection().ConnectionString);

                connection.Open();
                command = new SqlCommand($"SELECT F.Form_id, F.Direction, T.Teacher_name " +
                    $"FROM Forms F " +
                    $"INNER JOIN Teachers T ON F.Teacher_id = T.Teacher_id " +
                    $"WHERE F.Form_id = N'{id}'", connection);
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    form.Form_id = reader.GetString(0);
                    form.Direction = reader.GetString(1);
                    form.Teachers = new Teachers() { Teacher_name = reader.GetString(2) };
                }
                reader.Close();

                command = new SqlCommand("SELECT Student_name FROM " +
                    $"Students WHERE Student_form=N'{id}' " +
                    "ORDER BY Student_name", connection);

                reader = command.ExecuteReader();
                while (reader.Read())
                    form.Students.Add(new Models.Students() { Student_name = reader.GetString(0) });
                reader.Close();

            }

            stream = new MemoryStream();
            name = $"Список {form.Form_id}.docx";
            DocX document = DocX.Create(stream);
          
            Paragraph par = document.InsertParagraph();

            GetLine(ref par, "Клас: ", form.Form_id);
            GetLine(ref par, "Спрямування: ", form.Direction);
            GetLine(ref par, "Класний керівник: ", form.Teachers.Teacher_name);

            par.Append("Список класу:")
             .Font(new Xceed.Document.NET.Font("Times New Roman"))
             .FontSize(16)
             .Bold();
            par.AppendLine();

            int i = 1;

            foreach (var s in form.Students)
            {

                par.Append($"{i}. ")
                    .Font(new Xceed.Document.NET.Font("Times New Roman"))
                    .FontSize(16)
                    .Bold()
                    .Append(s.Student_name)
                    .Font(new Xceed.Document.NET.Font("Times New Roman"))
                    .FontSize(16);

                par.AppendLine();

                i++;
            }
            document.Save();
        }

        public static void CreateTab(string id, out MemoryStream stream, out string name)
        {
            stream = new MemoryStream();
            name = $"Табель {id}.docx";
            DocX document = DocX.Create(stream);

            Paragraph par = document.InsertParagraph();

            using (connection)
            {
                connection = new SqlConnection(new Connection().ConnectionString);

                connection.Open();
                command = new SqlCommand("SELECT DISTINCT Student_name " +
                  $"FROM AVG_Marks_Students_Subjects " +
                  $"WHERE Student_id={id}", connection);

                GetLine(ref par, "ПІБ учня: ", command.ExecuteScalar().ToString());

                command = new SqlCommand("SELECT DISTINCT Student_form " +
                  $"FROM AVG_Marks_Students_Subjects " +
                  $"WHERE Student_id={id}", connection);

                GetLine(ref par, "Клас: ", command.ExecuteScalar().ToString());

                par.Append("Оцінки:")
                    .Font(new Xceed.Document.NET.Font("Times New Roman"))
                    .FontSize(16)
                    .Bold();
                par.AppendLine();

                command = new SqlCommand("SELECT Subject, AVG_Mark " +
                  "FROM AVG_Marks_Students_Subjects " +
                  $"WHERE Student_id={id} ORDER BY 1", connection);
                reader = command.ExecuteReader();

                while (reader.Read())
                {
                    if ((object)reader.GetValue(1) != DBNull.Value)
                    {
                        GetLine(ref par, $"{reader.GetValue(0).ToString()}: ",
                            Math.Round(Convert.ToDouble(reader.GetValue(1)), 0).ToString());
                    }
                }
                reader.Close();

            }

            document.Save();


        }


        public static void CreateInvoice(string id, out MemoryStream stream, out string name)
        {
            stream = new MemoryStream();
            name = $"Чек {id}.docx";
            DocX document = DocX.Create(stream);
            Paragraph par = document.InsertParagraph();

            par.Append("Чек з їдальні")
             .Font(new Xceed.Document.NET.Font("Times New Roman"))
             .FontSize(20)
             .Bold()
             .Italic();
             
            par.AppendLine();

            using (connection)
            {
                connection = new SqlConnection(new Connection().ConnectionString);

                connection.Open();
                command = new SqlCommand($"SELECT * FROM Students_dinning WHERE Student_id={id}",
                    connection);
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    GetLine(ref par, "Номер учнівського: ", reader.GetValue(0).ToString());
                    GetLine(ref par, "ПІБ учня: ", reader.GetValue(1).ToString());
                    GetLine(ref par, "Клас: ", reader.GetValue(2).ToString());
                    GetLine(ref par, "Соціальна категорія: ", reader.GetValue(3).ToString());
                    GetLine(ref par, "Знижка: ", reader.GetValue(4).ToString()+"%");
                    GetLine(ref par, "Сплата за день: ", reader.GetValue(5).ToString());
                    GetLine(ref par, "Сплата за тиждень: ", reader.GetValue(6).ToString());
                }
                reader.Close();

            }

            GetLine(ref par, "Дата: ", DateTime.Now.ToString("yyyy-MM-dd"));
            document.Save();


        }

        private static void ParForParents(ref DocX document, string id)
        {
            Paragraph par = document.InsertParagraph();
            par.Append("Шановні батьки!")
            .Font(new Xceed.Document.NET.Font("Times New Roman"))
            .FontSize(20)
            .Bold()
            .Italic();

            par.AppendLine();

            using (connection)
            {
                connection = new SqlConnection(new Connection().ConnectionString);

                connection.Open();
                command = new SqlCommand($"SELECT Student_id, Student_name, Student_form FROM Students " +
                     $"WHERE Student_id={id}", connection);

                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    GetLine(ref par, "Номер учнівського: ", reader.GetValue(0).ToString());
                    GetLine(ref par, "ПІБ учня: ", reader.GetValue(1).ToString());
                    GetLine(ref par, "Клас: ", reader.GetValue(2).ToString());
                }
                reader.Close();

                command = new SqlCommand("SELECT COUNT(Subject) FROM AVG_Marks_Students_Subjects " +
                      $"WHERE AVG_Mark<7 AND Student_id={id}", connection);
                int count = (int)command.ExecuteScalar();

                if (count == 0)
                {
                    par.Append("Ваша дитина не відстає від навчального процесу!")
                        .Font(new Xceed.Document.NET.Font("Times New Roman"))
                        .FontSize(20)
                        .Bold()
                        .Italic();

                    par.AppendLine();
                }

                else
                {
                    par.Append("Зверніть увагу на цей список предметів: ")
                        .Font(new Xceed.Document.NET.Font("Times New Roman"))
                        .FontSize(16)
                        .Bold();

                    par.AppendLine();

                    command = new SqlCommand($"SELECT Subject, AVG_Mark FROM AVG_Marks_Students_Subjects " +
                         $"WHERE AVG_Mark<7 AND Student_id={id}", connection);
                    reader = command.ExecuteReader();
                    while (reader.Read())
                        GetLine(ref par, reader.GetValue(0).ToString() + ": ",
                            reader.GetValue(1).ToString());
                    reader.Close();

                }

            }
        }
        public static void CreateForParents(string id, out string pathDocument)
        {

            pathDocument = AppDomain.CurrentDomain.BaseDirectory + $"Нагадування батькам {id}.docx";
            DocX document = DocX.Create(pathDocument);
            ParForParents(ref document, id);          

            document.Save();  




        }

        public static void CreateForParents (string id, out MemoryStream stream, out string name)
        {
            stream = new MemoryStream();
            name = $"Нагадування батькам {id}.docx";
            DocX document = DocX.Create(stream);
            ParForParents(ref document, id);
            document.Save();

        }


    } 
}