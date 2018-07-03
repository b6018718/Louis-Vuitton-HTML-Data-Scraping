using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;  
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Net;
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
            webBrowser.ScriptErrorsSuppressed = true;
            //Load Browser
            if (!WBEmulator.IsBrowserEmulationSet())
                WBEmulator.SetBrowserEmulationVersion();
            //Using the HTML Agility NuGet
            HtmlAgilityPack.HtmlWeb htmlWeb = new HtmlWeb();
            HtmlAgilityPack.HtmlDocument webPage = new HtmlAgilityPack.HtmlDocument();
            HtmlAgilityPack.HtmlDocument webPageDesc = new HtmlAgilityPack.HtmlDocument();
            webBrowser.Navigate("https://uk.louisvuitton.com/eng-gb/women/handbags/_/N-2keomb.html");

            //Description
            webBrowser.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(JavaScriptLoaded);
            while (!webSiteLoaded)
                Application.DoEvents();
            var dom = (mshtml.IHTMLDocument3)webBrowser.Document.DomDocument;
            string html = dom.documentElement.innerHTML;
            webPage.LoadHtml(html);

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
                handbags.ElementAt(i).ProductPrice = productPrices.ElementAt(i).InnerText;

                //Product Code
                handbags.ElementAt(i).ProductCode = productPageLinks.ElementAt(i).Attributes["data-sku"].Value;

                //Image Link
                handbags.ElementAt(i).ProductImage = productImageLinks.ElementAt(i).Attributes["data-src"].Value;
                handbags.ElementAt(i).ProductImage = handbags.ElementAt(i).ProductImage.Replace("{IMG_WIDTH}", "360");
                handbags.ElementAt(i).ProductImage = handbags.ElementAt(i).ProductImage.Replace("{IMG_HEIGHT}", "360");
                handbags.ElementAt(i).ProductImage = handbags.ElementAt(i).ProductImage.Replace(" ", "%20");

                //Page Link
                handbags.ElementAt(i).PageLink = "https://uk.louisvuitton.com/" + productPageLinks.ElementAt(i).Attributes["href"].Value;

                //Navigate Browser for JavaScript (The only way to get stock messages is using JavaScript, so the program opens an internet browser)
                webSiteLoaded = false;
                webBrowser.Navigate(handbags.ElementAt(i).PageLink);

                //Out of Stock (For Testing Purposes)
                //webBrowser.Navigate("https://uk.louisvuitton.com/eng-gb/products/pochette-metis-monogram-empreinte-nvprod630173v#M44072");

                
                //Description
                webBrowser.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(JavaScriptLoaded);
                while (!webSiteLoaded)
                    Application.DoEvents();
                Uri url = webBrowser.Url;
                dom = (mshtml.IHTMLDocument3)webBrowser.Document.DomDocument;
                html = dom.documentElement.innerHTML;
                webPageDesc.LoadHtml(html);

                HtmlAgilityPack.HtmlNode description = webPageDesc.DocumentNode.SelectSingleNode("//div[@class='productDescription description-push-text onlyML ppTextDescription ']");
                if (description != null)
                    handbags.ElementAt(i).ProductDescription = description.InnerText;

                //Availability
                HtmlAgilityPack.HtmlNode availability = webPageDesc.DocumentNode.SelectSingleNode("//div[@id='notInStock']");
                if (availability != null)
                {
                    if (availability.Attributes["class"].Value == "getIndexClass")
                        handbags.ElementAt(i).Availability = "Currently out of stock online";
                    else
                        handbags.ElementAt(i).Availability = "In Stock";
                }

            }

            //DATA COLLECTED!
            webBrowser.Dispose();

            int defaultWidth = 550;
            int defaultHeight = 550;
            
            //Create a box with 4 default spots + more depending on how many bags are present. 300px per 2 bags
            //int scrollBoxMaxHeight = Convert.ToInt32(defaultHeight + Math.Ceiling((Convert.ToDouble(handbags.Count()) - 4) / 2) * defaultHeight / 2);
            Panel leftPanel = new Panel
            {
                Location = new Point(30, 30),
                Size = new Size(defaultWidth, defaultHeight),
                AutoScroll = true,
                BackColor = Color.Gray,
                
            };
            leftPanel.VerticalScroll.Visible = true;


            int boxSize = 200;
            int xMargin = 40;
            int yMargin = 5;
            int columnDistance = 290;
            int handbagNo = 0;
            foreach (Handbag handbag in handbags)
            {
                int xCoord;
                int yCoord;

                switch (handbagNo)
                {
                    case 0:
                        xCoord = xMargin;
                        yCoord = yMargin;
                        break;
                    case 1:
                        xCoord = columnDistance;
                        yCoord = yMargin;
                        break;
                    default:
                        yCoord = Convert.ToInt32(Math.Floor(Convert.ToDouble(handbagNo) / 2)) * boxSize + yMargin;
                        if (handbagNo % 2 == 0)
                            xCoord = xMargin;
                        else
                            xCoord = columnDistance;
                        break;
                }   // End Switch
                leftPanel.Controls.Add(LoadProduct(handbag, xCoord, yCoord, boxSize, handbagNo));
                handbagNo++;
            }   //End For Each Loop


            this.Controls.Add(leftPanel);
        }

        private GroupBox LoadProduct(Handbag handbag, int xCoord, int yCoord, int boxSize, int handbagNo)
        {
            GroupBox groupBox = new GroupBox();

            //Product Image
            PictureBox pictureBox = new PictureBox();
            pictureBox.Load(handbag.ProductImage);
            pictureBox.Size = new Size(200, 200);
            pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            groupBox.Location = new Point(xCoord, yCoord);
            groupBox.Size = new Size(boxSize, boxSize);
            groupBox.Name = "handbagNo-" + handbagNo;
            //this.Controls.Add(pictureBox);
            groupBox.Controls.Add(pictureBox);

            //Product Name
            var image = pictureBox.Image;
            var font = new Font("Times New Roman", 30, FontStyle.Regular, GraphicsUnit.Pixel);
            var graphics = Graphics.FromImage(image);
            graphics.DrawString(handbag.ProductName, font, Brushes.Black, new Point(10, 10));
            pictureBox.Image = image;
            return groupBox;
        }

        private void JavaScriptLoaded(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            webSiteLoaded = true;
        }
    }
}
