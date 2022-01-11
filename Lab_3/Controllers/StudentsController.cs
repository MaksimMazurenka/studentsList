using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using Lab_3.Models;
using System.Web;
using Newtonsoft.Json;
using System.Xml.Serialization;
using System.IO;
using System.Net.Http;
using System.Web.Http.Results;
using System.Reflection;

namespace Lab_3.Controllers {
    
    class Error : HateoasModel {
        public Error(string mdnRef) {
            _links.error = mdnRef;
        }
    }

    class BadRequestError : Error {
        public BadRequestError() : base("https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/400") {
        }
    }

    public class StudentsController : ApiController {

        private ApplicationContext db = new ApplicationContext();

        [Route("api/Students/search")]
        [HttpGet]
        public IHttpActionResult SearchStudents() {
            var qParams = HttpContext.Current.Request.QueryString;

            string name;
            string phone;
            string[] columns;
            int offset;
            int limit;
            int minId;
            int maxId;
            string globalLike;
            string orderBy;
            try {
                name = qParams["name"] ?? "";
                phone = qParams["phone"] ?? "";
                columns = qParams["columns"] == null ? new string[] {"Id", "Name", "Phone"} : qParams["columns"].Split(',');
                orderBy = qParams["orderby"] ?? "Id";
                offset = int.Parse(qParams["offset"]);
                limit = int.Parse(qParams["limit"]);
                minId = int.Parse(qParams["minId"]);
                maxId = int.Parse(qParams["maxId"]);
                globalLike = qParams["globallike"] ?? "";
            } catch {
                return BadRequest(JsonConvert.SerializeObject(new BadRequestError()));
            }

            var sqlQuery =
                $"SELECT * FROM Students " +
                $"WHERE Name LIKE '%' + @P0 + '%' AND Phone LIKE '%' + @P1 + '%' ";
            if (globalLike != null) {
                sqlQuery += $"AND CONCAT(Id, Name, Phone) LIKE '%' + @P2 + '%' ";
            }

            sqlQuery += $"AND Id BETWEEN @P3 AND @P4 ";
            sqlQuery += $"ORDER BY {orderBy} ";
            sqlQuery += $"OFFSET @P5 ROWS FETCH NEXT @P6 ROWS ONLY ";
            List<Student> students = db.Students
                .SqlQuery(sqlQuery,
                    name,
                    phone,
                    globalLike ?? "",
                    minId,
                    maxId,
                    offset,
                    limit)
                .ToList();

            List<PropertyInfo> props = typeof(Student)
                .GetProperties()
                .Where((prop) => columns.Contains(prop.Name))
                .ToList();
            students = students
                .Select((stud) => {
                    Student newStud = new Student();
                    newStud._links = stud._links;
                    props.ForEach(prop => {
                        prop.SetValue(newStud, prop.GetValue(stud));
                    });
                    return newStud;
                })
                .ToList();

            students.ForEach((stud) => {
                stud._links.self = new Link($"/api/Students/{stud.Id}");
            });


            CollectionModel<Student> colMod = new CollectionModel<Student>();
            colMod._embedded = students;
            colMod._links.self = new Link($"/api/Students");

            if (qParams.AllKeys.Contains("xml")) {
                // НИКАКОЙ ЭКСЭМЭЛЬ СЕРИАЛАЙЗЕР НЕ ХОЧЕТ + НЕ МОЖЕТ СЕРИАЛИЗОВАТЬ DYNAMIC ТИПЫ 👍👍👍👍👍👍👍👍👍👍👍👍👍👍👍👍👍👍 
                return Ok(JsonConvert.DeserializeXmlNode(JsonConvert.SerializeObject(colMod), "Root").InnerXml);
            }

            return Json(students);
        }

        // GET: api/Students/5
        [ResponseType(typeof(Student))]
        [Route("api/Students/{id}", Name = "GetStudent")]
        [HttpGet]
        public IHttpActionResult GetStudent(int id) {
            var qParams = HttpContext.Current.Request.QueryString;
            Student student;
            student = db.Students.Find(id);

            if (student == null) {
                return BadRequest(JsonConvert.SerializeObject(new BadRequestError()));
            }

            student._links.self = new Link($"/api/Students/{id}");
            student._links.all = new Link($"/api/Students");

            if (qParams.AllKeys.Contains("xml")) {
                return Ok(JsonConvert.DeserializeXmlNode(JsonConvert.SerializeObject(student), "Root").InnerXml);
            }

            return Json(student);
        }

        [ResponseType(typeof(Student))]
        [Route("api/Students/{id}")]
        [HttpPut]
        public IHttpActionResult PutStudent(int id, [FromBody] Student student) {
            var qParams = HttpContext.Current.Request.QueryString;
            if (!ModelState.IsValid) {
                return BadRequest(JsonConvert.SerializeObject(new BadRequestError()));
            }

            if (id != student.Id) {
                return BadRequest(JsonConvert.SerializeObject(new BadRequestError()));
            }

            db.Entry(student).State = EntityState.Modified;

            try {
                db.SaveChanges();
            } catch (DbUpdateConcurrencyException) {
                if (!StudentExists(id)) {
                    return BadRequest(JsonConvert.SerializeObject(new BadRequestError()));
                } else {
                    throw;
                }
            }

            student._links.self = new Link($"/api/Students/{id}");
            student._links.all = new Link($"/api/Students");

            if (qParams.AllKeys.Contains("xml")) {
                return Ok(JsonConvert.DeserializeXmlNode(JsonConvert.SerializeObject(student), "Root").InnerXml);
            }

            return Json(student);
        }

        // POST: api/Students
        [ResponseType(typeof(Student))]
        [HttpPost]
        public IHttpActionResult PostStudent(Student student) {
            var qParams = HttpContext.Current.Request.QueryString;
            if (!ModelState.IsValid) {
                    return BadRequest(JsonConvert.SerializeObject(new BadRequestError()));
            }

            db.Students.Add(student);
            db.SaveChanges();

            student._links.self = new Link($"/api/Students/{student.Id}");
            student._links.all = new Link($"/api/Students");

            if (qParams.AllKeys.Contains("xml")) {
                return Ok(JsonConvert.DeserializeXmlNode(JsonConvert.SerializeObject(student), "Root").InnerXml);
            }

            return CreatedAtRoute("GetStudent", new { id = student.Id }, student);
        }

        // DELETE: api/Students/5
        [ResponseType(typeof(Student))]
        [Route("api/Students/{id}")]
        [HttpDelete]
        public IHttpActionResult DeleteStudent(int id) {
            var qParams = HttpContext.Current.Request.QueryString;
            Student student = db.Students.Find(id);
            if (student == null) {
                return NotFound();
            }

            db.Students.Remove(student);
            db.SaveChanges();

            student._links.self = new Link($"/api/Students/{student.Id}");
            student._links.all = new Link($"/api/Students");

            if (qParams.AllKeys.Contains("xml")) {
                return Ok(JsonConvert.DeserializeXmlNode(JsonConvert.SerializeObject(student), "Root").InnerXml);
            }

            return Json(student);
        }

        // GET COUNT: api/Students/count
        [ResponseType(typeof(int))]
        [Route("api/Students/count")]
        [HttpGet]
        public IHttpActionResult Count() {
            var qParams = HttpContext.Current.Request.QueryString;
            int count = db.Students.Count();
            return Ok(count);
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool StudentExists(int id) {
            return db.Students.Count(e => e.Id == id) > 0;
        }
    }
}