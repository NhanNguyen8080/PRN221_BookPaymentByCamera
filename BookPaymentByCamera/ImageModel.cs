using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace BookPaymentByCamera
{
    public class ImageModel
    {
        public int Id { get; set; }
        public BitmapSource Image { get; set; }

        //public override bool Equals(object obj)
        //{
        //    if (obj == null || GetType() != obj.GetType())
        //        return false;

        //    ImageModel other = (ImageModel)obj;

        //    // Implement your equality comparison based on the properties of ImageModel
        //    return this.Id == other.Id && this.FileName == other.FileName;
        //}

        // You should also override GetHashCode when overriding Equals
        //public override int GetHashCode()
        //{
        //    // Implement a hash code based on the same properties used in Equals
        //    return HashCode.Combine(Id, FileName);
        //}

    }
}
