using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using BusinessServices.ClienteServices.DTO.Output;
using BusinessServices.PrestamoServices.DTO;
using BusinessServices.PrestamoServices.DTO.Input;
using BusinessServices.PrestamoServices.DTO.OutPut;
using BusinessTypes;
using DataModel;
using DataModel.UnitOfWork;

namespace BusinessServices.PrestamoServices
{
    /// <summary>
    /// Offers services for product specific CRUD operations
    /// </summary>
    public class PrestamoServices : IPrestamoService
    {
        private readonly UnitOfWork _unitOfWork;

        /// <summary>
        /// Public constructor.
        /// </summary>
        public PrestamoServices()
        {
            _unitOfWork = new UnitOfWork();
        }

        /// <summary>
        /// Con esta capacidad puedes obtener todos los prestamos filtrados por estados
        /// </summary>
        /// <returns></returns>
        public IEnumerable<PrestamoOutputType> BuscarPrestamosPorEstado(string estado)
        {
            var prestamo = _unitOfWork.PrestamoRepository.GetMany(n => n.ESTADO.ToUpper() == estado.ToUpper()).ToList();

            if (prestamo.Any())
            {
                var clientesModel = from x in prestamo
                    select new PrestamoOutputType
                    {
                            Cantidad = x.CANTIDAD_PRESTADA,
                            Estado = x.ESTADO,
                            Fecha = x.FECHA_PRESTAMO,
                            PrestamoId = x.PRESTAMO_ID,
                            Tasa = x.TASA_INTERES,
                            Deuda = CalcularDeudaPorPrestamoId(x.PRESTAMO_ID),
                            Cliente = new ClienteOutputType
                            {
                                ClienteId = x.CLIENTE.CLIENTE_ID,
                                Nombre = x.CLIENTE.NOMBRE,
                                CorreoElectronico = x.CLIENTE.CORREO_ELECTRONICO,
                                PrimerApellido = x.CLIENTE.PRIMER_APELLIDO
                            }
                    };

                return clientesModel;
            }
            return null;
        }

        /// <summary>
        /// Con esta capacidad puedes obtener un prestamo por id
        /// </summary>
        /// <returns></returns>
        public PrestamoOutputType BuscarPrestamosPorId(int prestamoId)
        {
            var prestamo = _unitOfWork.PrestamoRepository.GetById(prestamoId);

            if (prestamo != null)
            {
                var prestamoModel = new PrestamoOutputType
                {
                        Cantidad = prestamo.CANTIDAD_PRESTADA,
                        Estado = prestamo.ESTADO,
                        Fecha = prestamo.FECHA_PRESTAMO,
                        PrestamoId = prestamo.PRESTAMO_ID,
                        Tasa = prestamo.TASA_INTERES,
                        Deuda = CalcularDeudaPorPrestamoId(prestamo.PRESTAMO_ID),
                        Cliente = new ClienteOutputType
                        {
                            ClienteId = prestamo.CLIENTE.CLIENTE_ID,
                            Nombre = prestamo.CLIENTE.NOMBRE,
                            CorreoElectronico = prestamo.CLIENTE.CORREO_ELECTRONICO,
                            PrimerApellido = prestamo.CLIENTE.PRIMER_APELLIDO
                        }
                };

                return prestamoModel;
            }
            return null;
        }

        /// <summary>
        /// Con esta capacidad puedes solicitar un prestamo
        /// </summary>
        /// <returns></returns>
        public int SolicitarPrestamo(SolicitarPrestamoInputType solicitarPrestamoInputType)
        {

            var calcularTopePrestamos = CalcularDisponibilidadPrestamo(solicitarPrestamoInputType.ClienteId, solicitarPrestamoInputType.Tasa);

            if (calcularTopePrestamos < solicitarPrestamoInputType.Cantidad)
            {
                throw new  ApplicationException("Cantidad excede la disponibilidad de prestamos que tiene con nosotros, su disponibilidad de prestamos es de : " + calcularTopePrestamos + " pesos");
            }

            if (calcularTopePrestamos == solicitarPrestamoInputType.Cantidad)
            {
                return RegistrarPrestamo(new PrestamoType
                {
                    CantidadPrestada = solicitarPrestamoInputType.Cantidad,
                    Estado = "AC",
                    FechaPrestamo = DateTime.Now,
                    TasaInteres = solicitarPrestamoInputType.Tasa,
                    ClienteId = solicitarPrestamoInputType.ClienteId
                });
            }

            if (calcularTopePrestamos > solicitarPrestamoInputType.Cantidad)
            {
                if (solicitarPrestamoInputType.DeseaTotalDisponiblilidad)
                {
                    return RegistrarPrestamo(new PrestamoType
                    {
                        CantidadPrestada = calcularTopePrestamos,
                        Estado = "AC",
                        FechaPrestamo = DateTime.Now,
                        TasaInteres = solicitarPrestamoInputType.Tasa,
                        ClienteId = solicitarPrestamoInputType.ClienteId
                    });
                }

                if (solicitarPrestamoInputType.DeseaCantidadSolicitada)
                {
                    return RegistrarPrestamo(new PrestamoType
                    {
                        CantidadPrestada = solicitarPrestamoInputType.Cantidad,
                        Estado = "AC",
                        FechaPrestamo = DateTime.Now,
                        TasaInteres = solicitarPrestamoInputType.Tasa,
                        ClienteId = solicitarPrestamoInputType.ClienteId
                    });
                }
                
                 throw new ApplicationException("Esta seguro que desea tomar esta cantidad o prefiere tomar mas, su cantidad limite de prestamos es :" + calcularTopePrestamos + " pesos");
            }

            throw new Exception("Error interno del servicio de persistir favor llamar al administrados");

        }

        /// <summary>
        /// Con esta capacidad puedes buscar la dispobnibilidad de prestamos para un cliente en espesifico
        /// </summary>
        /// <returns></returns>
        public decimal CalcularDisponibilidadPrestamo(int clienteId, decimal tasa)
        {
            //busco todos los prestamos del cliente
            var cantidadPagos = _unitOfWork.PagoRepository.GetMany(n => n.CLIENTE_ID == clienteId).Count();
                
                if (cantidadPagos == 0)
                {
                    return 5000m;
                }

                if (cantidadPagos == 1)
                {
                    return _unitOfWork.PagoRepository.GetFirst(n => n.CLIENTE_ID == clienteId).MONTO;
                }

                if (cantidadPagos == 2)
                {
                    var lastOrDefault = _unitOfWork.PagoRepository.GetMany(n => n.CLIENTE_ID == clienteId).LastOrDefault();
                    if (lastOrDefault != null)
                    {
                        return lastOrDefault.MONTO;
                    }
                    throw new Exception();
                }

            if (cantidadPagos > 2)
            {
                var topePrestamos = new List<decimal>();

                //aqui busco los prestamos del cliente
                var prest = _unitOfWork.PrestamoRepository.GetMany(n => n.CLIENTE_ID == clienteId);

                //aqui itero entre los prestamos del cliente
                foreach (var pres in prest)
                {
                    if (pres.TASA_INTERES > tasa && pres.ESTADO == "AC")
                    {
                        throw new ApplicationException("Existe un prestamo activo con una tasa mayor a esta");
                    }
                
                    var minDate = pres.FECHA_PRESTAMO;
                    var maxDate = pres.DETALLE_PRESTAMO.Max(n => n.FECHA);
                    var detallePago = _unitOfWork.DetallePagoRepository.GetMany(n => n.PRESTAMO_ID == pres.PRESTAMO_ID);

                    // aqui busco los pagos que tiene el prestamo al que itero
                    foreach (var detPago in detallePago)
                    {
                        if (detPago.PAGO.FECHA >= minDate && detPago.PAGO.FECHA <= minDate.AddMonths(1) && maxDate >= detPago.PAGO.FECHA)
                        {
                            topePrestamos.Add(detPago.MONTO);
                        }
                        else
                        {
                            topePrestamos.Add(detPago.MONTO);

                            minDate = detPago.PAGO.FECHA;
                            maxDate = detPago.PAGO.FECHA.AddMonths(1);

                        }
                    }

                }

                var prestamo = topePrestamos.Sum() == 0 ? 0 : (topePrestamos.Average() / tasa);
                var deuda = CalcularDeudaPorClienteId(clienteId);

                return topePrestamos.Sum() <= 0 ? 0 : prestamo - deuda;

            }

            throw new Exception();

        }

        /// <summary>
        /// Con esta capacidad puedes solicitar un prestamo en base a tu sueldo
        /// </summary>
        /// <param name="solicitarPrestamoPorSueldoInput"></param>
        /// <returns></returns>
        public int SolicitarPrestamoPorSueldo(SolicitarPrestamoPorSueldoInputType solicitarPrestamoPorSueldoInput)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Con esta capacidad puedes Actualizar la tasa de un prestamo
        /// </summary>
        /// <param name="prestamoId"></param>
        /// <param name="nuevaTasa"></param>
        /// <returns></returns>
        public bool ActualizarTasaPrestamo(int prestamoId, decimal nuevaTasa)
        {
            var success = false;
            if (nuevaTasa >= 0)
            {
                using (var scope = new TransactionScope())
                {
                    var prestamo = _unitOfWork.PrestamoRepository.GetById(prestamoId);
                    if (prestamo != null)
                    {
                        prestamo.TASA_INTERES = nuevaTasa;

                        _unitOfWork.PrestamoRepository.Update(prestamo);
                        _unitOfWork.Save();
                        scope.Complete();
                        success = true;
                    }
                }
            }

            return success;
        }

        /// <summary>
        /// Con esta capacidad puedes inactivar un prestamo
        /// </summary>
        /// <param name="prestamoId"></param>
        /// <returns></returns>
        public bool InactivarPrestamo(int prestamoId)
        {
            var success = false;

            using (var scope = new TransactionScope())
            {
                var prestamo = _unitOfWork.PrestamoRepository.GetById(prestamoId);
                if (prestamo != null)
                {
                    prestamo.ESTADO = "IN";

                    _unitOfWork.PrestamoRepository.Update(prestamo);
                    _unitOfWork.Save();
                    scope.Complete();
                    success = true;
                }
            }
            return success;
        }

        /// <summary>
        /// Con esta capacidad puedes calcular la deuda de los prestamos de un cliente
        /// </summary>
        /// <param name="clienteId"></param>
        /// <returns></returns>
        public decimal CalcularDeudaPorClienteId(int clienteId)
        {
            var prestamos = _unitOfWork.PrestamoRepository.GetMany(n => n.CLIENTE_ID == clienteId && n.ESTADO == "AC");

            var prest = prestamos as PRESTAMO[] ?? prestamos.ToArray();
            var totalDeuda = 0m;

                foreach (var obj in prest)
                {
                    totalDeuda = totalDeuda + obj.DETALLE_PRESTAMO.Sum(n => n.MOVIMIENTO);
                }

            return totalDeuda;

        }

        /// <summary>
        /// Con esta capacidad puedes calcular la deuda de un prestamo
        /// </summary>
        /// <param name="prestamoId"></param>
        /// <returns></returns>
        public decimal CalcularDeudaPorPrestamoId(int prestamoId)
        {
            var prestamos = _unitOfWork.PrestamoRepository.GetById(prestamoId);

            var deuda = prestamos.DETALLE_PRESTAMO.Sum(n => n.MOVIMIENTO);
            
            return deuda;

        }
        
        /// <summary>
        /// Esta capacidad registra un prestamo
        /// </summary>
        /// <param name="prestamoType"></param>
        /// <returns></returns>
        private int RegistrarPrestamo(PrestamoType prestamoType)
        {
            using (var scope = new TransactionScope())
            {
                var prestamo = new PRESTAMO
                {
                    CANTIDAD_PRESTADA = prestamoType.CantidadPrestada,
                    ESTADO = "AC",
                    FECHA_PRESTAMO = prestamoType.FechaPrestamo,
                    CLIENTE_ID = prestamoType.ClienteId,
                    TASA_INTERES = prestamoType.TasaInteres,
                };

                _unitOfWork.PrestamoRepository.Insert(prestamo);

                _unitOfWork.DetallePrestamoRepository.Insert(
                    new DETALLE_PRESTAMO
                    {
                        FECHA = prestamoType.FechaPrestamo,
                        MOVIMIENTO = prestamoType.CantidadPrestada,
                        COMENTARIO = "",
                        TIPO_MOVIMIENTO_ID = 1,
                        PRESTAMO_ID = prestamo.PRESTAMO_ID
                    });

                _unitOfWork.DetallePrestamoRepository.Insert(
                    new DETALLE_PRESTAMO
                    {
                        FECHA = prestamoType.FechaPrestamo,
                        MOVIMIENTO = (prestamoType.CantidadPrestada) * prestamoType.TasaInteres,
                        COMENTARIO = "",
                        TIPO_MOVIMIENTO_ID = 2,
                        PRESTAMO_ID = prestamo.PRESTAMO_ID
                    });

                _unitOfWork.Save();

                scope.Complete();
                return prestamo.PRESTAMO_ID;

            }
        }

        /// <summary>
        /// Con esta capacidad puedes buscar lo que se le debe a un prestamo
        /// </summary>
        /// <param name="prestamoId"></param>
        /// <returns></returns>
        public decimal BuscarDeudaPorPrestamoId(int prestamoId)
        {
            var prestamos = _unitOfWork.PrestamoRepository.GetById(prestamoId);

            return prestamos.DETALLE_PRESTAMO.Sum(n => n.MOVIMIENTO);
            
        }

        /// <summary>
        /// Con esta capacidad puedes buscar la tasa de un prestamo
        /// </summary>
        /// <param name="prestamoId"></param>
        /// <returns></returns>
        public decimal BuscarTasaPorPrestamoId(int prestamoId)
        {
            var prestamos = _unitOfWork.PrestamoRepository.GetById(prestamoId);

            return prestamos.TASA_INTERES;

        }

        /// <summary>
        /// Con esta capacidad puedes Finalizar un prestamo
        /// </summary>
        /// <param name="prestamoId"></param>
        /// <returns></returns>
        public bool FinalizarPrestamoPrestamo(int prestamoId)
        {
            var success = false;
            if (prestamoId >= 0)
            {
                using (var scope = new TransactionScope())
                {
                    var prestamo = _unitOfWork.PrestamoRepository.GetById(prestamoId);
                    if (prestamo != null)
                    {
                        prestamo.ESTADO = "FN";

                        _unitOfWork.PrestamoRepository.Update(prestamo);
                        _unitOfWork.Save();
                        scope.Complete();
                        success = true;
                    }
                }
            }

            return success;
        }

        /// <summary>
        /// Con esta capacidad puedes generar el interes de los prestamos que cumplen 1 mes 
        /// </summary>
        public int GenerarInteresPrestamo()
        {
            using (var scope = new TransactionScope())
            {
                //levanta los prestamos activos
                var prestamosActivos = _unitOfWork.PrestamoRepository.GetMany(n => n.ESTADO == "AC");

                if (prestamosActivos == null ) { throw new ApplicationException("No se encontraron prestamos activos");}
                var cantidadInteresesGenerados = 0;
                //itera entre los prestamos
                foreach (var prestamo in prestamosActivos)
                {
                        //buscar ultimo interes calculado de cada prestamo
                        var detallePrestamo = prestamo.DETALLE_PRESTAMO.Where(n => n.TIPO_MOVIMIENTO_ID == 2).Max(n => n.FECHA);
    
                        var fechaUltimoCalculoInteres = detallePrestamo;

                        var aplicaCalculoInteres = (DateTime.Now.Subtract(fechaUltimoCalculoInteres).Days / (365.25 / 12)) > 0;

                        if (aplicaCalculoInteres)
                        {
                            //busco la deuda del prestamo a calcular interes
                            var deudaPrestamo = BuscarDeudaPorPrestamoId(prestamo.PRESTAMO_ID);

                            //Busca tasa del prestamo
                            var tasaPrestamo = BuscarTasaPorPrestamoId(prestamo.PRESTAMO_ID);

                            //Calcula el interes del prestamo 
                            var interes = deudaPrestamo * tasaPrestamo;

                            //registra el interes 
                            var nuevoDetallePrestamo = new DETALLE_PRESTAMO
                            {
                                PRESTAMO_ID = prestamo.PRESTAMO_ID,
                                FECHA = DateTime.Now,
                                COMENTARIO = "Interes",
                                MOVIMIENTO = interes,
                                TIPO_MOVIMIENTO_ID = 2
                            };

                            _unitOfWork.DetallePrestamoRepository.Insert(nuevoDetallePrestamo);
                            cantidadInteresesGenerados = cantidadInteresesGenerados + 1;

                        }

                }
                _unitOfWork.Save();
                scope.Complete();
                return cantidadInteresesGenerados;
            }
        }

    }

}

