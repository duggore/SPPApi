using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using BusinessServices.ClienteServices;
using BusinessServices.ClienteServices.DTO.Input;
using BusinessServices.ClienteServices.DTO.Output;

namespace SPPApi.Controllers
{
    public class ClienteController : ApiController
    {

        private readonly IClienteService _clienteServices;

        #region Public Constructor

        /// <summary>
        /// Public constructor to initialize product service instance
        /// </summary>
        public ClienteController()
        {
            _clienteServices = new ClienteServices();
        }

        #endregion

        // GET api/cliente
        public HttpResponseMessage Get()
        {
            var clientes = _clienteServices.BuscarClientes();

            if (clientes != null)
            {
                var clienteOutputTypes = clientes as ClienteOutputType[] ?? clientes.ToArray();

                if (clienteOutputTypes.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, clienteOutputTypes);
                }
            }
            return Request.CreateErrorResponse(HttpStatusCode.NotFound, "No se encontro clientes");
        }

        // GET api/cliente/5
        public HttpResponseMessage Get(int id)
        {
            var product = _clienteServices.BuscarClientePorId(id);
            if (product != null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, product);
            }

            return Request.CreateErrorResponse(HttpStatusCode.NotFound, "No se encontro cliente con este id");
        }

        // POST api/cliente
        public HttpResponseMessage Post([FromBody] ClienteInputType clienteInputType)
        {
            try
            {
                var guardarCliente = _clienteServices.GuardarCliente(clienteInputType);

                return Request.CreateResponse(HttpStatusCode.OK, guardarCliente);
            }
            catch (ApplicationException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }

        }

        // PUT api/cliente/5
        public bool Put(int id, [FromBody]ClienteInputType clienteInputType)
        {
            if (id > 0)
            {
                return _clienteServices.ActualizarCliente(id, clienteInputType);
            }
            return false;
        }

    }
}
