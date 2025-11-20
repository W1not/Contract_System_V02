using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sistema_de_Contratos.Models
{
    internal class Trabajador
    {
        public int IdTrabajador { get; set; }
        public string NombreCompleto { get; set; }
        public string Sexo { get; set; }
        public string CURP { get; set; }
        public string RFC { get; set; }
        public string EstadoCivil { get; set; }
        public string Cedula { get; set; }
        public string DomicilioParticular { get; set; }
        public string DomicilioFiscal { get; set; }
        public DateTime FechaRegistro { get; set; }
    }
}
