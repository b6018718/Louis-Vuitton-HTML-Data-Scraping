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
        //Count Lines OF Code = ctrl + shift + f
        //^(?!(\s*\*))(?!(\s*\-\-\>))(?!(\s*\<\!\-\-))(?!(\s*\n))(?!(\s*\*\/))(?!(\s*\/\*))(?!(\s*\/\/\/))(?!(\s*\/\/))(?!(\s*\}))(?!(\s*\{))(?!(\s(using))).*$

        //Global Variables
        bool webSiteLoaded = false; //Website Fully Loaded in Memory
        bool loadedOnce = false;    //Button pressed once before during lifetime of application
        bool productsLoaded = false;//When the user is allowed to click on the product
        bool midwork = false;       //When program is in the middle of mining data

        //Variables for the product boxes
        int boxSize = 200;          
        int xMargin = 40;
        int yMargin = 50;
        int columnDistance = 290;

        //List of handbag objects generated from by Web Scraping
        List<Handbag> handbags = new List<Handbag>();

        public LouisForm()
        {
            InitializeComponent();
        }

        private void LouisForm_Load(object sender, EventArgs e)
        {
            progressBar.Minimum = 0;
            progressBar.Maximum = 100;
        }

        private void load_Click(object sender, EventArgs e)
        {
            if (!midwork)
            {
                midwork = true;
                //Create new thread for background work
                if (loadedOnce)
                {
                    EraseDisplay();
                    if (this.Controls.ContainsKey("leftPanel"))
                        this.Controls.Remove(this.Controls["leftPanel"]);
                }
                loadedOnce = true;
                Thread thread = new Thread(new ThreadStart(WorkerMine_DoWork));
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
            }
        }

        //MAIN CODE TO LOAD HANDBAGS
        private void WorkerMine_DoWork()
        {
            //Set Progress Text to "0/0"
            SetLabelTextDelegate setLabelTextDelegate = new SetLabelTextDelegate(SetLabelText);
            String progressTextString = "0/0";
            this.Invoke(setLabelTextDelegate, new object[] { "progressText", progressTextString });
            productsLoaded = false;

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

            //Click Event
            this.Click += clickEvent;
            leftPanel.Click += clickEvent;
            AddPanelDelegate addPanel = new AddPanelDelegate(AddPanel);
            this.Invoke(addPanel, new object[] { leftPanel });

            //Variables for item boxes
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
            HtmlAgilityPack.HtmlNodeCollection productPageLinks = webPage.DocumentNode.SelectNodes("//a[@class='product-item productItem tagClick tagClick']");
            //HtmlAgilityPack.HtmlNodeCollection productPageLinks = webPage.DocumentNode.SelectNodes("//a[@class='product-item tagClick tagClick']");
            HtmlAgilityPack.HtmlNodeCollection productImageLinks = webPage.DocumentNode.SelectNodes("//a[@class='product-item productItem tagClick tagClick']//div[@class='imageWrapper']//img");

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
                handbags.ElementAt(i).ProductPrice = productPrices.ElementAt(i).Attributes["data-htmlContent"].Value;

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
                {
                    handbags.ElementAt(i).ProductDescription = description.InnerText;
                    handbags.ElementAt(i).ProductDescription = handbags.ElementAt(i).ProductDescription.Remove(0, 15);
                }

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
                progressTextString = (i + 1) + "/" + productNames.Count;
                VoidDelegate progressStep = new VoidDelegate(ProgressStep);
                this.Invoke(setLabelTextDelegate, new object[] { "progressText", progressTextString });
                this.Invoke(progressStep);

            }
            //DATA COLLECTED!
            VoidDelegate endProgressBar = new VoidDelegate(EndProgressBar);
            this.Invoke(endProgressBar);
            webBrowser.Dispose();
            webSiteLoaded = false;
            midwork = false;
            productsLoaded = true;
        }

        private GroupBox LoadProduct(Handbag handbag, int xCoord, int yCoord, int boxSize, int handbagNo)
        {
            //Product Image
            GroupBox groupBox = new GroupBox();
            PictureBox pictureBox = new PictureBox();
            pictureBox.Load(handbag.ProductImage);
            pictureBox.Size = new Size(200, 200);
            pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            groupBox.Location = new Point(xCoord, yCoord);
            groupBox.Size = new Size(boxSize, boxSize);
            groupBox.Name = "handbagNo-" + handbagNo;
            groupBox.Controls.Add(pictureBox);
            groupBox.Click += clickEvent;

            //Product Name
            var image = pictureBox.Image;
            var font = new Font("Microsoft Sans Serif", 30, FontStyle.Regular, GraphicsUnit.Pixel);
            var graphics = Graphics.FromImage(image);
            graphics.DrawString(handbag.ProductName, font, Brushes.Black, new Point(10, 10));
            pictureBox.Image = image;
            pictureBox.Click += clickEvent;
            return groupBox;
        }

        private void JavaScriptLoaded(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            webSiteLoaded = true;
        }

        private void LoadWebPageWithBrowser(HtmlAgilityPack.HtmlDocument webPageDesc, String url, WebBrowser webBrowser)
        {
            //Navigate Internet Explorer 11 Browser
            webSiteLoaded = false;
            webBrowser.Navigate(url);

            //Load DOM Object
            while (!webSiteLoaded)
                Application.DoEvents();
            var dom = (mshtml.IHTMLDocument3)webBrowser.Document.DomDocument;
            string html = dom.documentElement.innerHTML;
            webPageDesc.LoadHtml(html);
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
            progressBar.Value = 0;
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

        private void clickEvent(object sender, EventArgs e)
        {
            if (productsLoaded)
            {
                //Get Left Panel
                Panel clickPanel = (Panel)this.Controls["leftPanel"];

                //Get Mouse Position
                Point point = clickPanel.PointToClient(Cursor.Position);

                //Calculate Mouse Position w/ scroll bar
                int abX = clickPanel.AutoScrollPosition.X * -1;
                int abY = clickPanel.AutoScrollPosition.Y * -1;
                point.Offset(new Point(abX, abY));
                int xTopCoord = 0;
                int yTopCoord = 0;
                int xBottonCoord = 0;
                int yBottomCoord = 0;

                //Loop through objects
                bool found = false;
                int i = 0;
                while (i < handbags.Count && !found)
                {
                    int rowNumber = Convert.ToInt32(Math.Floor(Convert.ToDouble(i) / 2));
                    yTopCoord = yMargin + rowNumber * boxSize + yMargin * rowNumber;
                    yBottomCoord = yMargin + rowNumber * boxSize + yMargin * rowNumber + boxSize;
                    if (i % 2 == 0)
                    {
                        xTopCoord = xMargin;
                        xBottonCoord = xMargin + boxSize;
                    }
                    else
                    {
                        xTopCoord = columnDistance;
                        xBottonCoord = columnDistance + boxSize;
                    }
                    //Check for collision for boxes & mouse
                    if (point.X >= xTopCoord && point.X <= xBottonCoord && point.Y >= yTopCoord && point.Y <= yBottomCoord)
                    {
                        //Get rid of current labels & picture boxes
                        EraseDisplay();

                        //Set new display on right hand side
                        SetDisplay(i);
                        found = true;
                    }
                    else
                        i++;
                }
            }
        }

        private void SetDisplay(int i)
        {
            //Image
            PictureBox pictureBox = new PictureBox();
            pictureBox.Load(handbags.ElementAt(i).ProductImage);
            pictureBox.Size = new Size(306, 306);
            pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox.Location = new Point(600, 50);
            pictureBox.Name = "bigPicture";
            this.Controls.Add(pictureBox);

            //Font
            Font sans = new Font("Microsoft Sans Serif", 10, FontStyle.Regular);

            //Name
            Label name = new Label
            {
                Name = "bigName",
                Text = handbags.ElementAt(i).ProductName,
                Location = new Point(600, 370),
                Font = sans,
                AutoSize = true,
            };
            this.Controls.Add(name);

            //Product Code
            Label code = new Label
            {
                Name = "bigCode",
                Text = handbags.ElementAt(i).ProductCode,
                Location = new Point(600, 390),
                Font = sans,
                AutoSize = true,
            };
            this.Controls.Add(code);

            //Price
            Label price = new Label()
            {
                Name = "bigPrice",
                Text = handbags.ElementAt(i).ProductPrice,
                Location = new Point(600, 410),
                Font = sans,
                AutoSize = true,
            };
            this.Controls.Add(price);

            //Availability
            if (handbags.ElementAt(i).Availability != null)
            {
                Label availability = new Label()
                {
                    Name = "bigAvailability",
                    Text = handbags.ElementAt(i).Availability,
                    Location = new Point(600, 430),
                    Font = sans,
                    AutoSize = true,
                };
                this.Controls.Add(availability);
            }

            //Product Description
            Label description = new Label()
            {
                Name = "bigDescription",
                Text = handbags.ElementAt(i).ProductDescription,
                Location = new Point(600, 450),
                AutoSize = true,
                MaximumSize = new Size(360, 0),
                Font = sans,
            };
            this.Controls.Add(description);
        }

        private void EraseDisplay()
        {
            if (this.Controls.ContainsKey("bigName"))
                this.Controls.Remove(this.Controls["bigName"]);
            if (this.Controls.ContainsKey("bigCode"))
                this.Controls.Remove(this.Controls["bigCode"]);
            if (this.Controls.ContainsKey("bigPicture"))
                this.Controls.Remove(this.Controls["bigPicture"]);
            if (this.Controls.ContainsKey("bigDescription"))
                this.Controls.Remove(this.Controls["bigDescription"]);
            if (this.Controls.ContainsKey("bigPrice"))
                this.Controls.Remove(this.Controls["bigPrice"]);
            if (this.Controls.ContainsKey("bigAvailability"))
                this.Controls.Remove(this.Controls["bigAvailability"]);
        }
    }
}
