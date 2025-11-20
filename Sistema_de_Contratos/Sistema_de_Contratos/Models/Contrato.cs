using System;

namespace Sistema_de_Contratos.Models
{
    internal class Contrato
    {
        public int IdContrato { get; set; }
        public int IdTrabajador { get; set; }
        public int IdPuesto { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public decimal Sueldo { get; set; }
        public bool DatoDeBaja { get; set; } = false;
        public DateTime? FechaBaja { get; set; }
        public string MotivoBaja { get; set; }
        public bool EsContratoActual { get; set; } = true;
    }
}
