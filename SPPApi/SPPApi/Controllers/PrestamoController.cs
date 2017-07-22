using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using BusinessServices.PrestamoServices;
using BusinessServices.PrestamoServices.DTO.Input;
using BusinessServices.PrestamoServices.DTO.OutPut;

namespace SPPApi.Controllers
{
    public class PrestamoController : ApiController
    {

        private readonly IPrestamoService _prestamoService;

        #region Public Constructor

        /// <summary>
        /// Public constructor to initialize product service instance
        /// </summary>
        public PrestamoController()
        {
            _prestamoService = new PrestamoServices();
        }

        #endregion

        // GET api/prestamo
        [HttpGet]
        public HttpResponseMessage Get()
        {
            var prestamos = _prestamoService.BuscarPrestamosPorEstado("AC");

            if (prestamos != null)
            {
                var prestamoOutputTypes = prestamos as PrestamoOutputType[] ?? prestamos.ToArray();
                if (prestamoOutputTypes.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, prestamoOutputTypes);
                }
            }
            return Request.CreateErrorResponse(HttpStatusCode.NotFound, "No se encontraron prestamos");
        }

        // GET api/prestamo/5
        [HttpGet]
        public HttpResponseMessage Get(int id)
        {
            var prestamo = _prestamoService.BuscarPrestamosPorId(id);

            if (prestamo != null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, prestamo);
            }

            return Request.CreateErrorResponse(HttpStatusCode.NotFound, "No se encontro prestamo con este id");
        }

        // POST api/prestamo
        [HttpPost]
        public HttpResponseMessage Post([FromBody] SolicitarPrestamoInputType solicitarPrestamoInput)
        {

            try
            {
                var obj = _prestamoService.SolicitarPrestamo(solicitarPrestamoInput);

                return Request.CreateResponse(HttpStatusCode.OK, obj);
            }
            catch (ApplicationException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
            }
             
        }

        // PUT api/inactivarPrestamo
        [HttpPut]
        public bool InactivarPrestamo(int id)
        {
            if (id > 0)
            {
                return _prestamoService.InactivarPrestamo(id);
            }
            return false;
        }

        [HttpPut]
        public bool ActualizarTasaPrestamo(int id, decimal nuevaTasa)
        {
            if (id > 0 && nuevaTasa >= 0)
            {
                return _prestamoService.ActualizarTasaPrestamo(id, nuevaTasa);
            }
            return false;
        }

        // POST api/BuscarDisponibilidadPrestamo/1/1
        [HttpGet]
        public HttpResponseMessage CalcularDisponibilidadPrestamo(int clienteId, decimal tasa)
        {
            try
            {
                return Request.CreateResponse(HttpStatusCode.OK, _prestamoService.CalcularDisponibilidadPrestamo(clienteId, tasa));
            }
            catch (ApplicationException e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, e.Message);
            }
            
        }
     
        [HttpPost]
        public HttpResponseMessage GenerarInteresPrestamo()
        {
            try
            {
               var resultado = _prestamoService.GenerarInteresPrestamo();

                return Request.CreateResponse(HttpStatusCode.OK, resultado);
            }
            catch (ApplicationException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
            }

        }

    }
}
