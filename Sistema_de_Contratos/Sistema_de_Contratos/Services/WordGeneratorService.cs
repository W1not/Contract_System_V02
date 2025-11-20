using Sistema_de_Contratos.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xceed.Words.NET;
using System.Globalization;
using System.Windows;
using TemplateEngine.Docx;


namespace Sistema_de_Contratos.Services
{


    public class WordGeneratorService
    {
        [Obsolete]
        public void GenerarContratoWord(string plantillaPath, ContratoWordModel datos, string destinoPath)
        {
            if (!File.Exists(plantillaPath))
            {
                MessageBox.Show("No se encontró la plantilla seleccionada.");
                return;
            }

            try
            {
                // Cargar el documento de Word
                using (var doc = DocX.Load(plantillaPath))
                {
                    // Reemplazar los marcadores {Campo} con los valores
                    doc.ReplaceText("{NombreCompleto}", datos.NombreCompleto ?? "");
                    doc.ReplaceText("{Pronombre}", datos.Pronombre ?? "");
                    doc.ReplaceText("{PronombreCorto}", datos.PronombreCorto ?? "");
                    doc.ReplaceText("{Edad}", datos.Edad.ToString() ?? "");
                    doc.ReplaceText("{EstadoCivil}", datos.EstadoCivil ?? "");
                    doc.ReplaceText("{DomicilioParticular}", datos.DomicilioParticular ?? "");
                    doc.ReplaceText("{Sexo}", datos.Sexo ?? "");
                    doc.ReplaceText("{CURP}", datos.CURP ?? "");
                    doc.ReplaceText("{RFC}", datos.RFC ?? "");
                    doc.ReplaceText("{DomicilioFiscal}", datos.DomicilioFiscal ?? "");
                    doc.ReplaceText("{FechaInicio}", datos.FechaInicio ?? "");
                    doc.ReplaceText("{FechaFin}", datos.FechaFin ?? "");
                    doc.ReplaceText("{Puesto}", datos.Puesto ?? "");
                    doc.ReplaceText("{DuracionContrato}", datos.DuracionContrato ?? "");
                    doc.ReplaceText("{Sueldo}", datos.Sueldo?.ToString("N2") ?? "");
                    doc.ReplaceText("{SueldoLetra}", datos.SueldoLetra ?? "");
                    doc.ReplaceText("{Estado}", datos.Estado ?? "");
                    doc.ReplaceText("{Ciudad}", datos.Ciudad ?? "");

                    // Guardar el documento modificado
                    doc.SaveAs(destinoPath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar el contrato: {ex.Message}");
            }
        }
    }
}

