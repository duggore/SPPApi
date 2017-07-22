using System.Collections.Generic;
using BusinessServices.ClienteServices.DTO.Input;
using BusinessServices.ClienteServices.DTO.Output;

namespace BusinessServices.ClienteServices
{
    /// <summary>
    /// Contratos del servicio de clientes
    /// </summary>
    public interface IClienteService
    {
        ClienteOutputType BuscarClientePorId(int clienteId);
        IEnumerable<ClienteOutputType> BuscarClientes();
        int GuardarCliente(ClienteInputType clienteType);
        bool ActualizarCliente(int clienteId, ClienteInputType actualizarClienteInputType);
    }
}
