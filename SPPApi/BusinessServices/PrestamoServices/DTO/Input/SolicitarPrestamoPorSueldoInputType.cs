
namespace BusinessServices.PrestamoServices.DTO.Input
{
    public class SolicitarPrestamoPorSueldoInputType
    {
        private SolicitarPrestamoInputType SolicitudPrestamo { get; set; }
        public decimal Sueldo { get; set; }
    }
}
