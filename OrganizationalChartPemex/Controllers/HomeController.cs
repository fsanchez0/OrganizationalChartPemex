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
//using System.Web;
//using System.Web.Mvc;

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
            
            string query = "SELECT FICHA, NOMBRES, MC_STEXT, TRFGR, ANTIG_ANIOS, ANTIG_DIAS, REGIONAL, BUKRS";
            query += " FROM[00_tablero_dg]";
            query += " WHERE NIVEL_PLAZA >= 45";
            query += " ORDER BY BUKRS, NIVEL_PLAZA DESC";

            // Get it from Web.config file
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
                        /*cmd.CommandType = CommandType.Text;
                        cmd.Connection = con;
                        con.Open();*/
                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                               _logger.LogInformation("{0} {1} {2} {3}", dr.GetDecimal(0), dr.GetString(1), dr.GetString(2), dr.GetString(3));
                                _logger.LogInformation("\n DB: {0}  CONV: {1}", dr.GetDecimal(0), Decimal.ToInt32(dr.GetDecimal(0)));
                                // Adding new Employee object to List
                                empChartList.Add(new Employee()
                                 {
                                     id = Decimal.ToInt32(dr.GetDecimal(0)),
                                     nombres = dr.GetString(1),
                                     puesto = dr.GetString(2),
                                     nivel = int.Parse(dr.GetString(3)),
                                     antig_anios = dr.IsDBNull(4)?0:int.Parse(dr.GetString(4)),
                                     antig_dias = dr.IsDBNull(5)?0:int.Parse(dr.GetString(5)),
                                     bukrs = dr.GetString(7)
                                 }) ;
                            }
                        }
                        con.Close();
                    }
                }
            }
            catch (SqlException e)
            {
                //Console.WriteLine(e.ToString());
                _logger.LogInformation(e.ToString());
            }
            _logger.LogInformation(Json(empChartList).ToString());

            if(empChartList != null && empChartList.Count() > 0)
            {
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
            if(emp.nivel == 46)
            {
                foreach(Employee empleado in list)
                {
                    if (empleado.nivel == 48)
                        jefe = empleado.id;
                }
            }
            if(emp.nivel == 45)
            {
                foreach(Employee empleado in list)
                {
                    if (empleado.nivel == 46 && empleado.bukrs == emp.bukrs)
                        jefe = empleado.id;
                }
            }/*
            if(emp.nivel == 44)
            {
                foreach (Employee empleado in list)
                {
                    if (empleado.nivel == 45)
                        jefe = empleado.id;
                }
            }*/
            return jefe;
        }
    }
}
