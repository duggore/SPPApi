using System;
using BusinessServices.ClienteServices.DTO.Output;

namespace BusinessServices.PrestamoServices.DTO.OutPut
{
    public class PrestamoOutputType
    {
        public int PrestamoId { get; set; }
        public decimal Cantidad { get; set; }
        public decimal Tasa { get; set; }
        public DateTime Fecha { get; set; }
        public string Estado { get; set; }
        public decimal Deuda { get; set; }
        public virtual ClienteOutputType Cliente { get; set; }
    }
}
