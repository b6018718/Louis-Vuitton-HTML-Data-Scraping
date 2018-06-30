using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HtmlAgilityPack;

namespace Louis_Vuitton
{
    public partial class LouisForm : Form
    {
        public LouisForm()
        {
            InitializeComponent();
        }

        private void LouisForm_Load(object sender, EventArgs e)
        {
            //Using the HTML Agility NuGet
            HtmlAgilityPack.HtmlWeb htmlWeb = new HtmlWeb();
            HtmlAgilityPack.HtmlDocument webPage = htmlWeb.Load("https://uk.louisvuitton.com/eng-gb/women/handbags/_/N-2keomb.html");

            HtmlAgilityPack.HtmlNodeCollection productNames = webPage.DocumentNode.SelectNodes("//div[@class='productName toMinimize']");
            HtmlAgilityPack.HtmlNodeCollection productPrices = webPage.DocumentNode.SelectNodes("//div[@class='from-price']//span");
            HtmlAgilityPack.HtmlNodeCollection productPageLinks = webPage.DocumentNode.SelectNodes("//a[@class='product-item tagClick tagClick']");
            HtmlAgilityPack.HtmlNodeCollection productImageLinks = webPage.DocumentNode.SelectNodes("//a[@class='product-item tagClick tagClick']//div[@class='imageWrapper']//img");

            List<Handbag> handbags = new List<Handbag>();
            for (int i = 0; i < productNames.Count; i++)
            {
                handbags.Add(new Handbag());

                //Product Name
                handbags.ElementAt(i).ProductName = productNames.ElementAt(i).InnerText;

                //Product Price
                handbags.ElementAt(i).ProductPrice = productPrices.ElementAt(i).Attributes["data-htmlContent"].Value;

                //Product Code
                handbags.ElementAt(i).ProductCode = productPageLinks.ElementAt(i).Attributes["data-sku"].Value;

                //Image Link
                handbags.ElementAt(i).ProductImage = productImageLinks.ElementAt(i).Attributes["data-src"].Value;
                handbags.ElementAt(i).ProductImage = handbags.ElementAt(i).ProductImage.Replace("IMG_WIDTH", "360");
                handbags.ElementAt(i).ProductImage = handbags.ElementAt(i).ProductImage.Replace("IMG_HEIGHT", "360");

                //Description
                handbags.ElementAt(i).PageLink = "https://uk.louisvuitton.com/" + productPageLinks.ElementAt(i).Attributes["href"].Value;
                webPage = htmlWeb.Load(handbags.ElementAt(i).PageLink);
                HtmlAgilityPack.HtmlNode description = webPage.DocumentNode.SelectSingleNode("//*[@id='productDescriptionSeeMore']");
                handbags.ElementAt(i).ProductDescription = description.InnerText;

                //Availability
                webPage = htmlWeb.Load("https://uk.louisvuitton.com/eng-gb/products/pochette-metis-monogram-empreinte-nvprod630173v#M44072");
                HtmlAgilityPack.HtmlNode availability = webPage.DocumentNode.SelectSingleNode("//div[@class='notInStockMessage']//span");
                if (availability.Attributes["data-htmlContent"] != null)
                    handbags.ElementAt(i).Availability = availability.Attributes["data-htmlContent"].Value;
                else
                    handbags.ElementAt(i).Availability = "In Stock";
            }
        }
    }
}
