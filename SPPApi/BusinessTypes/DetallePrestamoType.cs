namespace BusinessTypes
{    
    public class DetallePrestamoType
    {
        public int DetallePrestamoId { get; set; }
        public int PrestamoId { get; set; }
        public decimal Movimiento { get; set; }
        public int TipoMovimientoId { get; set; }
        public int? DetallePagoId { get; set; }
        public string Comentario { get; set; }
        public System.DateTime Fecha { get; set; }
    
        public virtual DetallePagoType DetallePago { get; set; }
        public virtual PrestamoType Prestamo { get; set; }
        public virtual TipoMovimientoType TipoMovimiento{ get; set; }
    }
}
