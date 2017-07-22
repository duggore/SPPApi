using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Data.Entity.Validation;
using DataModel.GenericRepository;

namespace DataModel.UnitOfWork
{
    /// <summary>
    /// Unit of Work class responsible for DB transactions
    /// </summary>
    public class UnitOfWork : IDisposable
    {
        #region Private member variables...

        private readonly SPPEntities _context;
        private GenericRepository<CLIENTE> _clienteRepository;
        private GenericRepository<PRESTAMO> _prestamoRepository;
        private GenericRepository<DETALLE_PRESTAMO> _detallePrestamoRepository;
        private GenericRepository<PAGO> _pagoRepository;
        private GenericRepository<DETALLE_PAGO> _detallePagoRepository;
        #endregion

        public UnitOfWork()
        {
            _context = new SPPEntities();
        }

        #region Public Repository Creation properties...

        /// <summary>
        /// Get/Set Property for cliente repository.
        /// </summary>
        public GenericRepository<CLIENTE> ClienteRepository
        {
            get
            {
                if (_clienteRepository == null)
                {
                    _clienteRepository = new GenericRepository<CLIENTE>(_context);
                }
                
                return _clienteRepository;
            }
        }

        /// <summary>
        /// Get/Set Property for prestamo repository.
        /// </summary>
        public GenericRepository<PRESTAMO> PrestamoRepository
        {
            get
            {
                if (_prestamoRepository == null)
                {
                    _prestamoRepository = new GenericRepository<PRESTAMO>(_context);
                }

                return _prestamoRepository;
            }
        }

        /// <summary>
        /// Get/Set Property for detalle prestamo repository.
        /// </summary>
        public GenericRepository<DETALLE_PRESTAMO> DetallePrestamoRepository
        {
            get
            {
                if (_detallePrestamoRepository == null)
                {
                    _detallePrestamoRepository = new GenericRepository<DETALLE_PRESTAMO>(_context);
                }

                return _detallePrestamoRepository;
            }
        }

        /// <summary>
        /// Get/Set Property for pago repository.
        /// </summary>
        public GenericRepository<PAGO> PagoRepository
        {
            get
            {
                if (_pagoRepository == null)
                {
                    _pagoRepository = new GenericRepository<PAGO>(_context);
                }

                return _pagoRepository;
            }
        }

        /// <summary>
        /// Get/Set Property for pago repository.
        /// </summary>
        public GenericRepository<DETALLE_PAGO> DetallePagoRepository
        {
            get
            {
                if (_detallePagoRepository == null)
                {
                    _detallePagoRepository = new GenericRepository<DETALLE_PAGO>(_context);
                }

                return _detallePagoRepository;
            }
        }

        #endregion

        #region Public member methods...
        /// <summary>
        /// Save method.
        /// </summary>
        public void Save()
        {
            try
            {
                _context.SaveChanges();
            }
            catch (DbEntityValidationException e)
            {

                var outputLines = new List<string>();
                foreach (var eve in e.EntityValidationErrors)
                {
                    outputLines.Add(string.Format("{0}: Entity of type \"{1}\" in state \"{2}\" has the following validation errors:", DateTime.Now, eve.Entry.Entity.GetType().Name, eve.Entry.State));
                    foreach (var ve in eve.ValidationErrors)
                    {
                        outputLines.Add(string.Format("- Property: \"{0}\", Error: \"{1}\"", ve.PropertyName, ve.ErrorMessage));
                    }
                }
                System.IO.File.AppendAllLines(@"C:\errors.txt", outputLines);

                throw;
            }

        }

        #endregion

        #region Implementing IDiosposable...

        #region private dispose variable declaration...
        private bool _disposed; 
        #endregion

        /// <summary>
        /// Protected Virtual Dispose method
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Debug.WriteLine("UnitOfWork is being disposed");
                    _context.Dispose();
                }
            }
            _disposed = true;
        }

        /// <summary>
        /// Dispose method
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        } 
        #endregion
    }
}