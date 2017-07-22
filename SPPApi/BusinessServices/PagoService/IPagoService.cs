using BusinessServices.PagoService.DTO.Input;

namespace BusinessServices.PagoService
{
    public interface IPagoService
    {
        int RealizarPago(RealizarPagoInputType realizarPagoDto);
    }
}
