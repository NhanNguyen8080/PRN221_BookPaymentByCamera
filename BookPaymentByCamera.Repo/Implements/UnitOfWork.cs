using BookPaymentByCamera.Repo.Interfaces;
using BookPaymentByCamera.Repo.Models;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BookPaymentByCamera.Repo.Implements
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private  IGenericRepository<Author> AuthorRepo;
        private  IGenericRepository<Book> BookRepo;
        private IGenericRepository<Publisher> PublisherRepo;
        private readonly PRN2212024DBContext _dbContext;
        public UnitOfWork()
        {
            
        }
        public IGenericRepository<Author> AuthorRepository
        {
            get
            {
                if(this.AuthorRepo == null)
                {
                    this.AuthorRepo = new GenericRepository<Author>(_dbContext);
                }
                return this.AuthorRepo;
            }
        }

        public IGenericRepository<Book> BookRepository
        {
            get
            {
                if (this.BookRepo == null)
                {
                    this.BookRepo = new GenericRepository<Book>(_dbContext);
                }
                return this.BookRepo;
            }
        }

        public IGenericRepository<Publisher> PublisherRepository
        {
            get
            {
                if (this.PublisherRepo == null)
                {
                    this.PublisherRepo = new GenericRepository<Publisher>(_dbContext);
                }
                return this.PublisherRepo;
            }
        }
        public void Save()
        {
            _dbContext.SaveChanges();
        }
        private bool _disposedValue;
        private SafeHandle? _safeHandle = new SafeFileHandle(IntPtr.Zero, true);
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _safeHandle?.Dispose();
                    _safeHandle = null;
                }

                _disposedValue = true;
            }
        }
    }
}
