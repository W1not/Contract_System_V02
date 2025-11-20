using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sistema_de_Contratos.Models
{
    public class TrabajadorCompleto
    {
        public string NombreCompleto { get; set; }
        public string Sexo { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public string EstadoCivil { get; set; }
        public string DomicilioParticular { get; set; }
        public string CURP { get; set; }
        public string RFC { get; set; }
        public string DomicilioFiscal { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public decimal Sueldo { get; set; }
        public string Puesto { get; set; }
    }
}