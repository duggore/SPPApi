namespace BusinessTypes
{
    using System.Collections.Generic;
    
    public sealed class PagoType
    {
        public PagoType()
        {
            DetallePago  = new HashSet<DetallePagoType>();
        }
    
        public int PagoId { get; set; }
        public int ClienteId{ get; set; }
        public decimal Monto { get; set; }
        public System.DateTime Fecha { get; set; }
    
        public ClienteType Cliente { get; set; }

        public ICollection<DetallePagoType> DetallePago { get; set; }
    }
}
