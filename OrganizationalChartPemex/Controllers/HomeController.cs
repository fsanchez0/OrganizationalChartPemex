using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OrganizationalChartPemex.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace OrganizationalChartPemex.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Privacy()
        {
            return View();
        }

        [HttpPost]
        public JsonResult GetEmpChartData()
        {
            
            List<Employee> empChartList = new List<Employee>();
            
            string query = "SELECT FICHA, NOMBRES, MC_STEXT, nivel_plaza, ANTIG_ANIOS, ANTIG_DIAS, REGIONAL, BUKRS, direccion_coduni, subdireccion_coduni";
            query += " FROM[00_tablero_dg]";
            query += " WHERE NIVEL_PLAZA >= 44";
            query += " ORDER BY BUKRS, NIVEL_PLAZA DESC";

            try
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();

                builder.DataSource = "vwtutsqlp065.un.pemex.com";
                builder.UserID = "sapp";
                builder.Password = "Pemex.2020*";
                builder.InitialCatalog = "PEMEX";

                using (SqlConnection con = new SqlConnection(builder.ConnectionString))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                // Agregando nuevo empleado a la lista
                                empChartList.Add(new Employee()
                                 {
                                     id = Decimal.ToInt32(dr.GetDecimal(0)),
                                     nombres = dr.GetString(1),
                                     puesto = dr.GetString(2),
                                     nivel = int.Parse(dr.GetString(3)),
                                     antig_anios = dr.IsDBNull(4)?0:int.Parse(dr.GetString(4)),
                                     antig_dias = dr.IsDBNull(5)?0:int.Parse(dr.GetString(5)),
                                     bukrs = dr.GetString(7),
                                     direccion = dr.GetString(8),
                                     subdireccion = dr.GetString(9)
                                 }) ;
                            }
                        }
                        con.Close();
                    }
                }
            }
            catch (SqlException e)
            {
                _logger.LogInformation(e.ToString());
            }

            if(empChartList != null && empChartList.Count() > 0)
            {
                // agregamos las vacantes TEMPORALES!
                empChartList.Add(new Employee()
                {
                    id = 000001, nombres = "VACANTE", puesto = "DIRECTOR DE PEMEX TRANSFORMACIÓN INDUSTRIAL", nivel = 46, antig_anios = 0, antig_dias = 0, bukrs = "PTRI", subdireccion = "DIRECCIÓN GENERAL DE PEMEX TRANSFORMACIÓN INDUSTRIAL",
                    direccion = "DIRECCIÓN GENERAL DE PEMEX TRANSFORMACIÓN INDUSTRIAL"
                });
                empChartList.Add(new Employee()
                {
                    id = 000002,
                    nombres = "VACANTE",
                    puesto = "SUBDIRECTOR DE SSTPA",
                    nivel = 45,
                    antig_anios = 0,
                    antig_dias = 0,
                    bukrs = "PTRI",
                    direccion = "DIRECCIÓN GENERAL DE PEMEX TRANSFORMACIÓN INDUSTRIAL",
                    subdireccion = "SUBDIRECCIÓN DE SEGURIDAD, SALUD EN EL TRABAJO Y PROTECCIÓN AMBIENTAL"
                });
                foreach (Employee emp in empChartList){
                    if(emp.nivel != 48)
                        emp.jefe = buscarJefe(emp, empChartList);
                }
            }
            return Json(empChartList);
        }

        public int buscarJefe(Employee emp, List<Employee> list)
        {
            int jefe = 0;
            foreach (Employee empleado in list)
            {
                if (emp.nivel == 46 && empleado.nivel == 48)
                    jefe = empleado.id;

                if (emp.nivel == 45 && (empleado.nivel == 46 || empleado.nivel == 48) && empleado.bukrs == emp.bukrs && empleado.direccion == emp.direccion)
                    jefe = empleado.id;

                if (emp.nivel == 44 && (empleado.nivel == 45 || empleado.nivel == 46) && empleado.bukrs == emp.bukrs && empleado.subdireccion == emp.subdireccion)
                    jefe = empleado.id;
            }
            return jefe;
        }
    }
}
