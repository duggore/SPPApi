using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using BusinessServices.ClienteServices.DTO.Input;
using BusinessServices.ClienteServices.DTO.Output;
using BusinessTypes;
using DataModel;
using DataModel.UnitOfWork;

namespace BusinessServices.ClienteServices
{
    /// <summary>
    /// Offers services for product specific CRUD operations
    /// </summary>
    public class ClienteServices : IClienteService
    {
        private readonly UnitOfWork _unitOfWork;

        /// <summary>
        /// Public constructor.
        /// </summary>
        public ClienteServices()
        {
            _unitOfWork = new UnitOfWork();
        }

        /// <summary>
        /// Buscar cliente por Id
        /// </summary>
        /// <param name="clienteId"></param>
        /// <returns></returns>
        public ClienteOutputType BuscarClientePorId(int clienteId)
        {
            var cliente = _unitOfWork.ClienteRepository.GetById(clienteId);

            if (cliente != null)
            {
                var clienteModel = new ClienteOutputType
                {
                    ClienteId = cliente.CLIENTE_ID,
                    CorreoElectronico = cliente.CORREO_ELECTRONICO,
                    Nombre = cliente.NOMBRE,
                    PrimerApellido = cliente.PRIMER_APELLIDO
                };

                return clienteModel;
            }
            return null;
        }

        /// <summary>
        /// Busca todos los clientes
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ClienteOutputType> BuscarClientes()
        {
            var cliente = _unitOfWork.ClienteRepository.GetAll().ToList();

            if (cliente.Any())
            {
                var clientesModel = (from x in cliente
                                     select new ClienteOutputType
                                     {
                        ClienteId = x.CLIENTE_ID,
                        Nombre = x.NOMBRE,
                        CorreoElectronico = x.CORREO_ELECTRONICO,
                        PrimerApellido = x.PRIMER_APELLIDO
                        
                    });

                return clientesModel;
            }
            return null;
        }

        /// <summary>
        /// Guardar un cliente
        /// </summary>
        /// <param name="clienteInputType"></param>
        /// <returns></returns>
        public int GuardarCliente(ClienteInputType clienteInputType)
        {
            using (var scope = new TransactionScope())
            {
                var cliente = new CLIENTE
                {
                    CORREO_ELECTRONICO = clienteInputType.CorreoElectronico,
                    NOMBRE = clienteInputType.Nombre,
                    PRIMER_APELLIDO = clienteInputType.PrimerApellido
                    
                };
                _unitOfWork.ClienteRepository.Insert(cliente);
                _unitOfWork.Save();
                scope.Complete();
                return cliente.CLIENTE_ID;
            }
        }

        /// <summary>
        /// Actualiza producto
        /// </summary>
        /// <param name="clienteId"></param>
        /// <param name="clienteInputType"></param>
        /// <returns></returns>
        public bool ActualizarCliente(int clienteId, ClienteInputType clienteInputType)
        {
            var success = false;
            if (clienteInputType != null)
            {
                using (var scope = new TransactionScope())
                {
                    var cliente = _unitOfWork.ClienteRepository.GetById(clienteId);
                    if (cliente != null)
                    {
                        cliente.CORREO_ELECTRONICO = clienteInputType.CorreoElectronico;
                        cliente.NOMBRE = clienteInputType.Nombre;
                        cliente.PRIMER_APELLIDO = clienteInputType.PrimerApellido;

                        _unitOfWork.ClienteRepository.Update(cliente);
                        _unitOfWork.Save();
                        scope.Complete();
                        success = true;
                    }
                }
            }
            return success;
        }

        /// <summary>
        /// Elimina un cliente particular
        /// </summary>
        /// <param name="clienteId"></param>
        /// <returns></returns>
        private bool EliminarCliente(int clienteId)
        {
            var success = false;
            if (clienteId > 0)
            {
                using (var scope = new TransactionScope())
                {
                    var product = _unitOfWork.ClienteRepository.GetById(clienteId);
                    if (product != null)
                    {

                        _unitOfWork.ClienteRepository.Delete(product);
                        _unitOfWork.Save();
                        scope.Complete();
                        success = true;
                    }
                }
            }
            return success;
        }
        
    }
}
