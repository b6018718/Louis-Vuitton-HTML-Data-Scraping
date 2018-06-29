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
            HtmlAgilityPack.HtmlNodeCollection productCodes = webPage.DocumentNode.SelectNodes("//div[@id='products - grid']");
            //*[@id="sku_M51419"]/div[2]/div[2]/span
            //*[@id="sku_M51419"]/div[2]/div[2]
            //*[@id="sku_M43715"]/div[2]/div[2]/span

        }
    }
}
