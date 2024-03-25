using BookPaymentByCamera.Repo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookPaymentByCamera.Repo.Interfaces
{
    public interface IUnitOfWork
    {
        IGenericRepository<Author> AuthorRepository { get; }
        IGenericRepository<Book> BookRepository { get; }
        IGenericRepository<Publisher> PublisherRepository { get; }


        void Save();

    }
}
