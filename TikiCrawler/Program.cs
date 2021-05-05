using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using FileHelpers;

namespace TikiCrawler
{
    [DelimitedRecord(",")]
    class Program
    {
        public ProductWooCommerce mapDataToWooCommerce(ProductTiki inputProduct)
        {
            ProductWooCommerce outputProduct = new ProductWooCommerce();
            outputProduct.ID = Guid.NewGuid().ToString();
            outputProduct.Type = "simple";
            outputProduct.SKU = inputProduct.SKU;
            outputProduct.Name = inputProduct.Title;
            outputProduct.Published = "1";
            outputProduct.Isfeatured = "0";
            outputProduct.VisibilityInCatalog = "visible";
            outputProduct.ShortDescription = "";
            outputProduct.Description= inputProduct.Description;
            outputProduct.DateSalePriceStarts = "";
            outputProduct.DateSalePriceEnds = "";
            outputProduct.TaxStatus = "taxable";
            outputProduct.TaxClass = "";
            outputProduct.InStock = "10";
            outputProduct.Stock= "";
            outputProduct.LowStockAmount = "";
            outputProduct.BackordersAllowed = "0";
            outputProduct.SoldIndividually = "0";
            outputProduct.Weight = "";
            outputProduct.Height = "";
            outputProduct.Length = "";
            outputProduct.Width = "";
            outputProduct.AllowCustomerReviews = "1";
            outputProduct.PurchaseNote = "";
            outputProduct.SalePrice = inputProduct.SalePrice;
            outputProduct.RegularPrice = inputProduct.RegularPrice;
            outputProduct.Categories = inputProduct.Categories;
            outputProduct.Tags = "";
            outputProduct.Shipping = "";
            outputProduct.Images = inputProduct.Images;
            outputProduct.DownloadLimit = "";
            outputProduct.DownloadExpiryDays = "";
            outputProduct.Parent = "";
            outputProduct.GroupedProducts = "";
            outputProduct.Upsells = "";
            outputProduct.CrossSells = "";
            outputProduct.ExternalURL = "";
            outputProduct.ButtonText = "";
            outputProduct.Position = "0";
            
            return outputProduct;
        }

        static void Main(string[] args)
        {
            //Create an instance of Chrome driver
            IWebDriver browser = new ChromeDriver();

            //Navigate to website Tiki.vn > Laptop category
            browser.Navigate().GoToUrl("https://tiki.vn/laptop/c8095");

            //Select all product items by CSS Selector
            List<string> listProductLink = new List<string>();
            var products = browser.FindElements(By.CssSelector(".product-item"));
            foreach (var product in products)
            {
                string outerHtml = product.GetAttribute("outerHTML");
                string productLink = Regex.Match(outerHtml, "href=\"(.*?)\"").Groups[1].Value;
                if (productLink.Contains("tka.tiki.vn"))
                {
                    productLink = "https:" + productLink;
                    listProductLink.Add(productLink);
                }
                else
                {
                    productLink = "https://tiki.vn" + productLink;
                    listProductLink.Add(productLink);                
                }
            }

            List<ProductWooCommerce> listProduct = new List<ProductWooCommerce>();
            //Go to each product link
            for (int i = 0; i <= listProductLink.Count; i++)
            {
                ProductTiki result = new ProductTiki();
                //Go to product link
                browser.Navigate().GoToUrl(listProductLink[i]);

                //Extract product information by CSS Selector
                string productTitle = browser.FindElements(By.CssSelector(".title"))[0].Text;
                //Extract product brand by CSS Selector then remove redundant data by Regular Expression
                string productBrand = browser.FindElements(By.CssSelector(".brand-and-author"))[0].GetAttribute("outerHTML");
                productBrand = Regex.Match(productBrand, "brand\">(.*?)</a>").Groups[1].Value;
                //Extract product price
                System.Threading.Thread.Sleep(1800);
                var priceMode = browser.FindElements(By.CssSelector(".flash-sale-price"));
                if (priceMode.Count > 0)
                {
                    string currentPrice = browser.FindElements(By.XPath("//div[@class='flash-sale-price']/child::span"))[0].Text;
                    string listPrice = browser.FindElements(By.CssSelector(".list-price"))[0].Text;
                    result.SalePrice = currentPrice;
                    result.RegularPrice = listPrice;
                }
                else
                {
                    //var x = browser.FindElements(By.CssSelector(".product-price__current-price"));
                    string currentPrice = browser.FindElements(By.CssSelector(".product-price__current-price"))[0].Text;
                    var listPriceElement = browser.FindElements(By.CssSelector(".product-price__list-price"));
                    string listPrice = listPriceElement.Count > 0 ? listPriceElement[0].Text : currentPrice;
                    result.SalePrice = currentPrice;
                    result.RegularPrice = listPrice;
                }
                //Extract product sku
                string productSKU = browser.FindElements(By.XPath("//td[text()='SKU']/parent::tr"))[0].GetAttribute("outerHTML");
                productSKU = Regex.Matches(productSKU, "<td>(.*?)</td>")[1].Groups[1].Value;
                //Extract product images
                string productImage = browser.FindElements(By.CssSelector(".PictureV2__StyledImage-tfuu67-1"))[0].GetAttribute("src");
                //Extract product category
                List<string> listCategory = new List<string>();
                var categories = browser.FindElements(By.CssSelector(".breadcrumb-item"));
                string productCategory = "";
                for(int j = 1; j < categories.Count -1; j++ )
                {
                    string categoryItem;
                    if(j != categories.Count - 2) {
                        categoryItem = categories[j].Text + ">";
                    }
                    else
                    {
                        categoryItem = categories[j].Text;
                    }
                    productCategory += categoryItem;
                }
                //Extract product description
                var viewMoreElement = browser.FindElements(By.CssSelector(".btn-more"));
                if(viewMoreElement.Count > 0)
                {
                    viewMoreElement[0].Click();
                    System.Threading.Thread.Sleep(800);
                }
                string productDescription = browser.FindElements(By.CssSelector(".ToggleContent__View-sc-1hm81e2-0"))[0].Text;
                productDescription = productDescription.Replace("\n", " ").Replace("\r", " ").Replace("\r\n", " ");
                result.SKU = productSKU;
                result.Title = productTitle;
                result.Description = productDescription;
                result.Categories = productCategory;
                result.Images = productImage;
                Program program = new Program();
                
                listProduct.Add(program.mapDataToWooCommerce(result));
                System.Threading.Thread.Sleep(1000);
            }
            var engine = new FileHelperEngine<ProductWooCommerce>();
            engine.WriteFile("F:/output.txt", listProduct);
            //Console.WriteLine(products.Count);
            //System.IO.StreamWriter writer = new System.IO.StreamWriter("D:\\tiki.csv", false, System.Text.Encoding.UTF8);
            //writer.WriteLine("ProductName\tImageLink");
            ////System.Threading.Thread.Sleep(10000);
            ////string productLink = product.GetAttribute("href");
            ////string productName = product.FindElement(By.CssSelector(".product-item .name")).Text;
            ////string innerHtml = product.GetAttribute("innerHTML");
            //string productName = Regex.Match(outerHtml, "alt=\"(.*?)\"").Groups[1].Value;
            //string productThumbnail = Regex.Match(outerHtml, "<img src=\"(.*?)\"").Groups[1].Value;
            //writer.WriteLine(productName + "\t" + productThumbnail);
            //writer.Close();

            //browser.FindElements(By.CssSelector(".title"))[0].Text;
            //browser.FindElements(By.CssSelector(".title"))[0].GetAttribute("");

        }
    }
}

//browser.FindElements(By.XPath(""));
//browser.FindElement(By.CssSelector(""));
//browser.FindElement(By.XPath(""));