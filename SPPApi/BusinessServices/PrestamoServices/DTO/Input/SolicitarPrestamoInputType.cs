namespace BusinessServices.PrestamoServices.DTO.Input
{
    public class SolicitarPrestamoInputType
    {
        public int ClienteId { get; set; }
        public decimal Cantidad { get; set; }
        public decimal Tasa { get; set; }
        public bool DeseaTotalDisponiblilidad { get; set; }
        public bool DeseaCantidadSolicitada { get; set; }
    }
}
