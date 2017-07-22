using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using BusinessServices.PagoService;
using BusinessServices.PagoService.DTO.Input;

namespace SPPApi.Controllers
{
    public class PagoController : ApiController
    {

        private readonly IPagoService _pagoService;

        #region Public Constructor

        /// <summary>
        /// Public constructor to initialize product service instance
        /// </summary>
        public PagoController()
        {
            _pagoService = new PagoService();
        }

        #endregion
        // POST api/prestamo
        [HttpPost]
        public HttpResponseMessage Post([FromBody] RealizarPagoInputType realizarPagoInputType)
        {

            try
            {
                var obj = _pagoService.RealizarPago(realizarPagoInputType);

                return Request.CreateResponse(HttpStatusCode.OK, obj);
            }
            catch (ApplicationException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
            }

        }

    }
}
