using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sistema_de_Contratos.Models
{
    public class ContratoWordModel
    {
        public string? NombreCompleto { get; set; }
        public string? Pronombre { get; set; }
        public string? PronombreCorto { get; set; }
        public int  Edad { get; set; }
        public string? EstadoCivil { get; set; }
        public string? DomicilioParticular { get; set; }
        public string? Sexo { get; set; }
        public string? CURP { get; set; }
        public string? RFC { get; set; }
        public string? DomicilioFiscal { get; set; }
        public string? FechaInicio { get; set; }
        public string? FechaFin { get; set; }
        public string? Puesto { get; set; }
        public string? DuracionContrato { get; set; }
        public decimal? Sueldo { get; set; }
        public string? SueldoLetra { get; set; }
        public string? SueldoCentavos { get; set; }

        public string? Estado {  get; set; }
        public string? Ciudad { get; set; }
    }
}