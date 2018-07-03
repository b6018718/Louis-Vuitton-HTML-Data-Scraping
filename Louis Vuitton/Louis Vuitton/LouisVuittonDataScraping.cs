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


        bool webSiteLoaded = false;
        WebBrowser webBrowser = new WebBrowser();

        private void LouisForm_Load(object sender, EventArgs e)
        {
            //Load Browser
            if (!WBEmulator.IsBrowserEmulationSet())
                WBEmulator.SetBrowserEmulationVersion();
            //Using the HTML Agility NuGet
            HtmlAgilityPack.HtmlWeb htmlWeb = new HtmlWeb();
            HtmlAgilityPack.HtmlDocument webPage = htmlWeb.Load("https://uk.louisvuitton.com/eng-gb/women/handbags/_/N-2keomb.html");
            //HtmlAgilityPack.HtmlDocument webPage = htmlWeb.LoadFromBrowser("https://uk.louisvuitton.com/eng-gb/women/handbags/_/N-2keomb.html");


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
                handbags.ElementAt(i).ProductImage = handbags.ElementAt(i).ProductImage.Replace("{IMG_WIDTH}", "360");
                handbags.ElementAt(i).ProductImage = handbags.ElementAt(i).ProductImage.Replace("{IMG_HEIGHT}", "360");

                //Page Link
                handbags.ElementAt(i).PageLink = "https://uk.louisvuitton.com/" + productPageLinks.ElementAt(i).Attributes["href"].Value;

                //Navigate Browser for JavaScript (The only way to get stock messages is using JavaScript, so the program opens an internet browser)
                webBrowser.Navigate(handbags.ElementAt(i).PageLink);

                //Out of Stock (For Testing Purposes)
                //webBrowser.Navigate("https://uk.louisvuitton.com/eng-gb/products/pochette-metis-monogram-empreinte-nvprod630173v#M44072");

                //Description
                webBrowser.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(JavaScriptLoaded);
                while (!webSiteLoaded)
                    Application.DoEvents();
                var dom = (mshtml.IHTMLDocument3)webBrowser.Document.DomDocument;
                string html = dom.documentElement.innerHTML;
                webPage.LoadHtml(html);

                HtmlAgilityPack.HtmlNode description = webPage.DocumentNode.SelectSingleNode("//*[@id='productDescriptionSeeMore']");
                handbags.ElementAt(i).ProductDescription = description.InnerText;

                //Availability
                HtmlAgilityPack.HtmlNode availability = webPage.DocumentNode.SelectSingleNode("//div[@id='notInStock']");
                if (availability.Attributes["class"].Value == "getIndexClass")
                    handbags.ElementAt(i).Availability = "Currently out of stock online";
                else
                    handbags.ElementAt(i).Availability = "In Stock";

            }
            //DATA COLLECTED!
            webBrowser.Dispose();

            int defaultWidth = 600;
            int defaultHeight = 500;
            
            //Create a box with 4 default spots + more depending on how many bags are present. 300px per 2 bags
            int scrollBoxMaxHeight = Convert.ToInt32(defaultHeight + Math.Ceiling((Convert.ToDouble(handbags.Count()) - 4) / 2) * defaultHeight / 2);
            Panel leftPanel = new Panel
            {
                Location = new Point(30, 30),
                Size = new Size(defaultWidth, defaultHeight),
                AutoScroll = true,
                BackColor = Color.Gray,
                
            };
            leftPanel.VerticalScroll.Visible = true;

            //CheckBox checkers = new CheckBox
            //{ Location = new Point(700, 700) };
            //leftPanel.Controls.Add(checkers);
            /*GroupBox leftBox = new GroupBox
            {
                BackColor = Color.Gray,
                Size = new Size (defaultWidth, defaultHeight),
            };
            leftBox.Controls.Add(leftPanel);*/

            this.Controls.Add(leftPanel);
        }

        private void JavaScriptLoaded(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            webSiteLoaded = true;
        }
    }
}
