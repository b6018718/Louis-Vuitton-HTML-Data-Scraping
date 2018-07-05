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
            progressBar.Minimum = 0;
            progressBar.Maximum = 100;
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

        private void load_Click(object sender, EventArgs e)
        {
            //Create a box with 4 default spots + more depending on how many bags are present
            int defaultWidth = 550;
            int defaultHeight = 550;
            Panel leftPanel = new Panel
            {
                Location = new Point(30, 30),
                Size = new Size(defaultWidth, defaultHeight),
                AutoScroll = false,
                BackColor = Color.Gray,
            };
            leftPanel.VerticalScroll.Visible = false;

            this.Controls.Add(leftPanel);

            //Variables for item boxes
            int boxSize = 200;
            int xMargin = 40;
            int yMargin = 50;
            int columnDistance = 290;
            int xCoord = 0;
            int yCoord = 0;

            //Load an instance of Internet Explorer 11 for Web Browsing
            webBrowser.ScriptErrorsSuppressed = true;
            if (!WBEmulator.IsBrowserEmulationSet())
                WBEmulator.SetBrowserEmulationVersion();
            //Using the HTML Agility NuGet
            HtmlAgilityPack.HtmlWeb htmlWeb = new HtmlWeb();
            HtmlAgilityPack.HtmlDocument webPage = new HtmlAgilityPack.HtmlDocument();
            HtmlAgilityPack.HtmlDocument webPageDesc = new HtmlAgilityPack.HtmlDocument();
            webPage = htmlWeb.Load("https://uk.louisvuitton.com/eng-gb/women/handbags/_/N-2keomb.html");

            //Web Browser Event
            webBrowser.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(JavaScriptLoaded);

            //Collect HTML Data
            HtmlAgilityPack.HtmlNodeCollection productNames = webPage.DocumentNode.SelectNodes("//div[@class='productName toMinimize']");
            HtmlAgilityPack.HtmlNodeCollection productPrices = webPage.DocumentNode.SelectNodes("//div[@class='from-price']//span");
            HtmlAgilityPack.HtmlNodeCollection productPageLinks = webPage.DocumentNode.SelectNodes("//a[@class='product-item tagClick tagClick']");
            HtmlAgilityPack.HtmlNodeCollection productImageLinks = webPage.DocumentNode.SelectNodes("//a[@class='product-item tagClick tagClick']//div[@class='imageWrapper']//img");

            //Progress Bar
            progressBar.Step = 100 / (productNames.Count + 1);
            progressBar.PerformStep();

            //Collect JavaScript Data + Add Item to Form
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
                while (!webSiteLoaded)
                    Application.DoEvents();
                Uri url = webBrowser.Url;
                var dom = (mshtml.IHTMLDocument3)webBrowser.Document.DomDocument;
                string html = dom.documentElement.innerHTML;
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

                //Add Item to Form
               
                int rowNumber = Convert.ToInt32(Math.Floor(Convert.ToDouble(i) / 2));

                //Get X & Y Coordinates
                yCoord = yMargin + rowNumber * boxSize + yMargin * rowNumber;
                if (i % 2 == 0)
                    xCoord = xMargin;
                else
                    xCoord = columnDistance;

                //Add Box To Panel
                leftPanel.Controls.Add(LoadProduct(handbags.ElementAt(i), xCoord, yCoord, boxSize, i));
                progressBar.PerformStep();
            }
            //DATA COLLECTED!
            progressBar.PerformStep();
            webBrowser.Dispose();
            leftPanel.Height = yCoord + boxSize + yMargin;
            leftPanel.AutoScroll = true;
            leftPanel.VerticalScroll.Visible = true;
            webSiteLoaded = false;
        }
    }
}
