namespace BusinessServices.PagoService.DTO.Input
{
    public class RealizarPagoInputType
    {
        public int ClienteId { get; set; }
        public decimal Monto { get; set; }
        public string Comentario { get; set; }
    }
}
