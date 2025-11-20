namespace Sistema_de_Contratos.Models
{
    internal class TrabajadorView
    {
        public int IdTrabajador { get; set; }
        public string NombreCompleto { get; set; }
        public string CURP { get; set; }
        public string Puesto { get; set; }
        public string EstadoContrato { get; set; }
        public int? DiasRestantes { get; set; }
        public DateTime FechaInicio { get; internal set; }
        public DateTime FechaFin { get; internal set; }
        public decimal Sueldo { get; internal set; }
    }
}
