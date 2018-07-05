using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
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
        List<Handbag> handbags = new List<Handbag>();

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
            Thread thread = new Thread(new ThreadStart(WorkerMine_DoWork));
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        private void LoadWebPageWithBrowser(HtmlAgilityPack.HtmlDocument webPageDesc, String url, WebBrowser webBrowser)
        {
            
            webSiteLoaded = false;
            webBrowser.Navigate(url);

            //Out of Stock (For Testing Purposes)
            //webBrowser.Navigate("https://uk.louisvuitton.com/eng-gb/products/pochette-metis-monogram-empreinte-nvprod630173v#M44072");

            //Description
            while (!webSiteLoaded)
                Application.DoEvents();
            var dom = (mshtml.IHTMLDocument3)webBrowser.Document.DomDocument;
            string html = dom.documentElement.innerHTML;
            webPageDesc.LoadHtml(html);
        }

        //MAIN CODE TO LOAD HANDBAGS
        private void WorkerMine_DoWork()
        {
            //Create a box with 4 default spots + more depending on how many bags are present
            WebBrowser webBrowser = new WebBrowser();
            int defaultWidth = 550;
            int defaultHeight = 550;
            Panel leftPanel = new Panel
            {
                Name = "leftPanel",
                Location = new Point(30, 30),
                Size = new Size(defaultWidth, defaultHeight),
                AutoScroll = false,
                BackColor = Color.Gray,
            };
            leftPanel.VerticalScroll.Visible = false;

            AddPanelDelegate addPanel = new AddPanelDelegate(AddPanel);
            this.Invoke(addPanel, new object[] { leftPanel });

            //Variables for item boxes
            int boxSize = 200;
            int xMargin = 40;
            int yMargin = 50;
            int columnDistance = 290;
            int xCoord = 0;
            int yCoord = 0;

            //Check if the user wants to get stock data (slow as it uses a web browser)
            bool getStock = false;
            if (stockCheckBox.Checked)
                getStock = true;

            //Load an instance of Internet Explorer 11 for Web Browsing
            if (getStock)
            {
                webBrowser.ScriptErrorsSuppressed = true;
                if (!WBEmulator.IsBrowserEmulationSet())
                    WBEmulator.SetBrowserEmulationVersion();
                //Web Browser Event
                webBrowser.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(JavaScriptLoaded);
            }

            //Using the HTML Agility NuGet
            HtmlAgilityPack.HtmlWeb htmlWeb = new HtmlWeb();
            HtmlAgilityPack.HtmlDocument webPage = new HtmlAgilityPack.HtmlDocument();
            HtmlAgilityPack.HtmlDocument webPageDesc = new HtmlAgilityPack.HtmlDocument();
            webPage = htmlWeb.Load("https://uk.louisvuitton.com/eng-gb/women/handbags/_/N-2keomb.html");

            //Collect HTML Data
            HtmlAgilityPack.HtmlNodeCollection productNames = webPage.DocumentNode.SelectNodes("//div[@class='productName toMinimize']");
            HtmlAgilityPack.HtmlNodeCollection productPrices = webPage.DocumentNode.SelectNodes("//div[@class='from-price']//span");
            HtmlAgilityPack.HtmlNodeCollection productPageLinks = webPage.DocumentNode.SelectNodes("//a[@class='product-item tagClick tagClick']");
            HtmlAgilityPack.HtmlNodeCollection productImageLinks = webPage.DocumentNode.SelectNodes("//a[@class='product-item tagClick tagClick']//div[@class='imageWrapper']//img");

            //Progress Bar
            InitialseProgressBar(productNames.Count);

            //Collect JavaScript Data + Add Item to Form
            handbags.Clear();
            for (int i = 0; i < productNames.Count; i++)
            {
                //Create Handbag Object
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
                if (getStock)
                    LoadWebPageWithBrowser(webPageDesc, handbags.ElementAt(i).PageLink, webBrowser);
                else
                    webPageDesc = htmlWeb.Load(handbags.ElementAt(i).PageLink);

                HtmlAgilityPack.HtmlNode description = webPageDesc.DocumentNode.SelectSingleNode("//div[@class='productDescription description-push-text onlyML ppTextDescription ']");
                if (description != null)
                    handbags.ElementAt(i).ProductDescription = description.InnerText;

                //Handbag Availability
                if (getStock)
                {
                    HtmlAgilityPack.HtmlNode availability = webPageDesc.DocumentNode.SelectSingleNode("//div[@id='notInStock']");
                    if (availability != null)
                    {
                        if (availability.Attributes["class"].Value == "getIndexClass")
                            handbags.ElementAt(i).Availability = "Currently out of stock online";
                        else
                            handbags.ElementAt(i).Availability = "In Stock";
                    }
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
                UpdatePanelDelegate updatePanelDelegate = new UpdatePanelDelegate(UpdatePanel);
                this.Invoke(updatePanelDelegate, new object[] { "leftPanel", handbags.ElementAt(i), xCoord, yCoord, boxSize, i });

                //Progress Text
                String progressTextString = (i + 1) + "/" + productNames.Count;
                SetLabelTextDelegate setLabelTextDelegate = new SetLabelTextDelegate(SetLabelText);
                VoidDelegate progressStep = new VoidDelegate(ProgressStep);
                this.Invoke(setLabelTextDelegate, new object[] { "progressText", progressTextString });
                this.Invoke(progressStep);

            }
            //DATA COLLECTED!
            VoidDelegate endProgressBar = new VoidDelegate(EndProgressBar);
            this.Invoke(endProgressBar);
            webBrowser.Dispose();
            webSiteLoaded = false;
        }

        private void InitialseProgressBar(int numberOfSteps)
        {
            SetIntDelegate setProgressStep = new SetIntDelegate(SetProgressStep);
            this.Invoke(setProgressStep, new object[] { numberOfSteps });
            VoidDelegate progressStep = new VoidDelegate(ProgressStep);
            this.Invoke(progressStep);
        }

        private delegate void AddPanelDelegate(Panel panel);
        private delegate void SetIntDelegate(int integer);
        private delegate void VoidDelegate();
        private delegate void UpdatePanelDelegate(string panelName, Handbag handbag, int xCoord, int yCoord, int boxSize, int handbagNo);
        private delegate void SetLabelTextDelegate(string labelName, string text);
        private void AddPanel(Panel panel)
        {
            this.Controls.Add(panel);
        }
        private void SetProgressStep(int itemCount)
        {
            progressBar.Step = 100 / (itemCount + 1);
        }
        private void ProgressStep()
        {
            progressBar.PerformStep();
        }
        private void UpdatePanel(string panelName, Handbag handbag, int xCoord, int yCoord, int boxSize, int handbagNo)
        {
            this.Controls[panelName].Controls.Add(LoadProduct(handbag, xCoord, yCoord, boxSize, handbagNo));
        }
        private void SetLabelText(string labelName, string text)
        {
            this.Controls[labelName].Text = text;
        }

        private void EndProgressBar()
        {
            progressBar.PerformStep();
            Panel panelToEdit = (Panel)this.Controls["leftPanel"];
            panelToEdit.AutoScroll = true;
            panelToEdit.VerticalScroll.Visible = true;
        }

    }
}
