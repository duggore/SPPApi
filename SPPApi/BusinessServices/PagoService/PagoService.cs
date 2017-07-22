using System;
using System.Linq;
using System.Transactions;
using BusinessServices.PagoService.DTO.Input;
using DataModel;
using DataModel.UnitOfWork;

namespace BusinessServices.PagoService
{
    public class PagoService : IPagoService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly PrestamoServices.PrestamoServices _prestamoService;
        private readonly ClienteServices.ClienteServices _clienteService;
        /// <summary>
        /// Public constructor.
        /// </summary>
        public PagoService()
        {
            _unitOfWork = new UnitOfWork();
            _clienteService = new ClienteServices.ClienteServices();
            _prestamoService = new PrestamoServices.PrestamoServices();

        }
        
        public int RealizarPago(RealizarPagoInputType realizarPagoDto)
        {
            //----------------------------------------REGLAS DE INTEGRIDAD----------------------------------------
            if (realizarPagoDto.Monto <= 0) { throw new ApplicationException("El monto a pagar debe ser mayor a cero"); }
            if (realizarPagoDto.ClienteId <= 0) { throw new ApplicationException("El id del cliente debe ser mayor a cero"); }
            //cuando la deuda sea mayor
            var deudaTotal = _prestamoService.CalcularDeudaPorClienteId(realizarPagoDto.ClienteId);
            if (deudaTotal < realizarPagoDto.Monto) { throw new ApplicationException("El pago supera la deuda : "+deudaTotal+" pesos"); }
            //cliente no existe
            var cliente = _clienteService.BuscarClientePorId(realizarPagoDto.ClienteId);
            if (cliente == null) { throw new ApplicationException("Cliente no existe"); }

            //----------------------------------------REGLAS DE NEGOCIO----------------------------------------
            using (var scope = new TransactionScope())
            {

                //inserto la cabecera del pago
                var pago = new PAGO
                {
                    CLIENTE_ID = realizarPagoDto.ClienteId,
                    FECHA = DateTime.Now,
                    MONTO = realizarPagoDto.Monto
                };

                _unitOfWork.PagoRepository.Insert(pago);

                var prestamosActivosOrdenadOrderByFecha = _unitOfWork.PrestamoRepository.GetMany(n => n.ESTADO == "AC").OrderBy(n => n.FECHA_PRESTAMO).ThenBy(n => n.TASA_INTERES);

                //cuando la deuda sea igual a lo que se esta pagando
                if (deudaTotal == realizarPagoDto.Monto)
                {

                    foreach (var obj in prestamosActivosOrdenadOrderByFecha)
                    {

                        var buscarDeudaPorPrestamoId = _prestamoService.BuscarDeudaPorPrestamoId(obj.PRESTAMO_ID);

                        //inserto un detalle pago por cada detalle prestamo
                        var detallePago = new DETALLE_PAGO
                        {
                            MONTO = buscarDeudaPorPrestamoId,
                            PAGO_ID = pago.PAGO_ID,
                            PRESTAMO_ID = obj.PRESTAMO_ID,
                            TASA_EFECTIVA = _prestamoService.BuscarTasaPorPrestamoId(obj.PRESTAMO_ID),
                        };
                        _unitOfWork.DetallePagoRepository.Insert(detallePago);

                        //inserto un detalle prestamo por cada detalle pago
                        var detallePrestamo = new DETALLE_PRESTAMO
                        {
                            DETALLE_PAGO_ID = detallePago.DETALLE_PAGO_ID,
                            PRESTAMO_ID = obj.PRESTAMO_ID,
                            COMENTARIO = realizarPagoDto.Comentario + " PAGO " + realizarPagoDto.Monto +
                                         " dividido entre  " + prestamosActivosOrdenadOrderByFecha.Count() + "prestamos",
                            FECHA = DateTime.Now,
                            MOVIMIENTO = buscarDeudaPorPrestamoId * -1,
                            TIPO_MOVIMIENTO_ID = 3
                        };
                        _unitOfWork.DetallePrestamoRepository.Insert(detallePrestamo);

                        //aqui marco el prestamo como finalizado
                        _prestamoService.FinalizarPrestamoPrestamo(obj.PRESTAMO_ID);

                        _unitOfWork.Save();
                    }


                    scope.Complete();

                    return pago.PAGO_ID;

                }

                //cuando la deuda total es mayor al pago
                if (deudaTotal > realizarPagoDto.Monto)
                {
                    
                        var balancePago = realizarPagoDto.Monto;

                        //itero entre todos los prestamos a pagar
                        foreach (var obj in prestamosActivosOrdenadOrderByFecha)
                        {
                            var balancePrestamo = _prestamoService.BuscarDeudaPorPrestamoId(obj.PRESTAMO_ID);

                            if (balancePago < balancePrestamo)
                            {
                                //inserto un detalle pago por cada detalle prestamo
                                var detallePago = new DETALLE_PAGO
                                {
                                    MONTO = balancePago,
                                    PAGO_ID = pago.PAGO_ID,
                                    PRESTAMO_ID = obj.PRESTAMO_ID,
                                    TASA_EFECTIVA = _prestamoService.BuscarTasaPorPrestamoId(obj.PRESTAMO_ID),
                                };
                                _unitOfWork.DetallePagoRepository.Insert(detallePago);

                                //inserto un detalle prestamo por cada detalle pago
                                var detallePrestamo = new DETALLE_PRESTAMO
                                {
                                    DETALLE_PAGO_ID = detallePago.DETALLE_PAGO_ID,
                                    PRESTAMO_ID = obj.PRESTAMO_ID,
                                    COMENTARIO = realizarPagoDto.Comentario,
                                    FECHA = DateTime.Now,
                                    MOVIMIENTO = balancePago *-1,
                                    TIPO_MOVIMIENTO_ID = 3
                                };
                                _unitOfWork.DetallePrestamoRepository.Insert(detallePrestamo);

                                _unitOfWork.Save();
                                scope.Complete();
                                return pago.PAGO_ID;
                            }

                            if (balancePago > balancePrestamo)
                            {
                                var buscarDeudaPorPrestamoId =
                                    _prestamoService.BuscarDeudaPorPrestamoId(obj.PRESTAMO_ID);

                                //inserto un detalle pago por cada detalle prestamo
                                var detallePago = new DETALLE_PAGO
                                {
                                    MONTO = buscarDeudaPorPrestamoId,
                                    PAGO_ID = pago.PAGO_ID,
                                    PRESTAMO_ID = obj.PRESTAMO_ID,
                                    TASA_EFECTIVA = _prestamoService.BuscarTasaPorPrestamoId(obj.PRESTAMO_ID),
                                };
                                _unitOfWork.DetallePagoRepository.Insert(detallePago);

                                //inserto un detalle prestamo por cada detalle pago
                                var detallePrestamo = new DETALLE_PRESTAMO
                                {
                                    DETALLE_PAGO_ID = detallePago.DETALLE_PAGO_ID,
                                    PRESTAMO_ID = obj.PRESTAMO_ID,
                                    COMENTARIO = realizarPagoDto.Comentario,
                                    FECHA = DateTime.Now,
                                    MOVIMIENTO = buscarDeudaPorPrestamoId *-1,
                                    TIPO_MOVIMIENTO_ID = 3
                                };
                                _unitOfWork.DetallePrestamoRepository.Insert(detallePrestamo);

                                //aqui marco el prestamo como finalizado
                                _prestamoService.FinalizarPrestamoPrestamo(obj.PRESTAMO_ID);

                                balancePago = balancePago - balancePrestamo;
                                _unitOfWork.Save();
                            }

                            if (balancePago == balancePrestamo)
                            {
                                var buscarDeudaPorPrestamoId =
                                    _prestamoService.BuscarDeudaPorPrestamoId(obj.PRESTAMO_ID);

                                //inserto un detalle pago por cada detalle prestamo
                                var detallePago = new DETALLE_PAGO
                                {
                                    MONTO = buscarDeudaPorPrestamoId,
                                    PAGO_ID = pago.PAGO_ID,
                                    PRESTAMO_ID = obj.PRESTAMO_ID,
                                    TASA_EFECTIVA = _prestamoService.BuscarTasaPorPrestamoId(obj.PRESTAMO_ID),
                                };
                                _unitOfWork.DetallePagoRepository.Insert(detallePago);

                                //inserto un detalle prestamo por cada detalle pago
                                var detallePrestamo = new DETALLE_PRESTAMO
                                {
                                    DETALLE_PAGO_ID = detallePago.DETALLE_PAGO_ID,
                                    PRESTAMO_ID = obj.PRESTAMO_ID,
                                    COMENTARIO = realizarPagoDto.Comentario,
                                    FECHA = DateTime.Now,
                                    MOVIMIENTO = buscarDeudaPorPrestamoId *-1,
                                    TIPO_MOVIMIENTO_ID = 3
                                };
                                _unitOfWork.DetallePrestamoRepository.Insert(detallePrestamo);

                                //aqui marco el prestamo como finalizado
                                _prestamoService.FinalizarPrestamoPrestamo(obj.PRESTAMO_ID);

                                _unitOfWork.Save();
                                scope.Complete();
                                return pago.PAGO_ID;

                            }

                        }

                    _unitOfWork.Save();
                    scope.Complete();
                    return pago.PAGO_ID;
                }

            }
            
            throw new Exception();

        }

    }
}
