using System.Collections.Generic;
using BusinessServices.PrestamoServices.DTO.Input;
using BusinessServices.PrestamoServices.DTO.OutPut;

namespace BusinessServices.PrestamoServices
{
    /// <summary>
    /// Contratos del servicio de Prestamos
    /// </summary>
    public interface IPrestamoService
    {
        PrestamoOutputType BuscarPrestamosPorId(int id);
        IEnumerable<PrestamoOutputType> BuscarPrestamosPorEstado(string estado);
        int SolicitarPrestamo(SolicitarPrestamoInputType solicitarPrestamoType);
        decimal CalcularDisponibilidadPrestamo(int prestamoId, decimal tasa);
        int SolicitarPrestamoPorSueldo(SolicitarPrestamoPorSueldoInputType solicitarPrestamoPorSueldoInput);
        bool ActualizarTasaPrestamo(int prestamoId, decimal nuevaTasa);
        bool InactivarPrestamo(int prestamoId);
        decimal CalcularDeudaPorClienteId(int clienteId);
        int GenerarInteresPrestamo();
    }
}
