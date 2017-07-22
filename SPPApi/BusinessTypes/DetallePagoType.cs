namespace BusinessTypes
{
    using System.Collections.Generic;
    
    public class DetallePagoType
    {
    
        public int DetallePagoId { get; set; }
        public int PagoId{ get; set; }
        public int PrestamoId { get; set; }
        public decimal TasaEfectiva{ get; set; }
        public decimal Monto { get; set; }
    
        public virtual PagoType Pago { get; set; }
        public virtual PrestamoType Prestamo{ get; set; }

        public virtual ICollection<DetallePrestamoType> DetallePrestamo{ get; set; }
    }
}
