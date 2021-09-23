using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrganizationalChartPemex.Models
{
    public class Employee
    {
        public int id { get; set; }
        public string nombres { get; set; }
        public string puesto { get; set; }
        public int nivel { get; set; }
        public int antig_anios { get; set; }
        public int antig_dias { get; set; }
        public string region { get; set; }
        public int jefe { get; set; }
        public string bukrs { get; set; }
        public string direccion { get; set; }
        public string subdireccion { get; set; }
        public string desc_dpto { get; set; }
    }
}
