using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Louis_Vuitton
{
    class Handbag
    {
        private String productName;
        private String productCode;
        private String productImage;
        private String productDescription;
        private String productPrice;
        private String availability;
        private String pageLink;

        public string ProductName { get => productName; set => productName = value; }
        public string ProductCode { get => productCode; set => productCode = value; }
        public string ProductImage { get => productImage; set => productImage = value; }
        public string ProductDescription { get => productDescription; set => productDescription = value; }
        public string ProductPrice { get => productPrice; set => productPrice = value; }
        public string Availability { get => availability; set => availability = value; }
        public string PageLink { get => pageLink; set => pageLink = value; }
    }
}
