using System;
using System.Collections.Generic;

#nullable disable

namespace BookPaymentByCamera.Repo.Models
{
    public partial class Book
    {
        public int BookId { get; set; }
        public string BookName { get; set; }
        public decimal? BookPrice { get; set; }
        public int? AuthorId { get; set; }
        public int? PublisherId { get; set; }

        public virtual Author Author { get; set; }
        public virtual Publisher Publisher { get; set; }
    }
}
