using HtmlAgilityPack;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;

namespace AzUpdate
{
    //public class UpdateInfo
    //{
    //    public string? URL { get; set; }
    //    public int? WatingDuration { get; set; }
    //}

    public class AzUpdateFunction
    {
        private readonly ILogger<AzUpdateFunction> _logger;

        public AzUpdateFunction(ILogger<AzUpdateFunction> logger)
        {
            _logger = logger;
        }

        [Function("GetUpdate")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {
            string description = "Nothing Happend";
            string url = "https://azure.microsoft.com/ko-kr/updates?id=498166";
            int waitingDuration = 3000;

            // 수정된 코드 (CS8600 방지)
            string? u = req.Query.TryGetValue("URL", out var urlValue) ? urlValue.ToString() : null;
            if (!string.IsNullOrEmpty(u))
            {
                url = u;
            }
            string? w = req.Query.TryGetValue("WaitingDuration", out var waitingValue) ? waitingValue.ToString() : null;
            if (!string.IsNullOrEmpty(w))
            {
                waitingDuration = int.Parse(w);
            }

            try
            {
                var chromeOptions = new ChromeOptions();
                chromeOptions.AddArguments(
                    "--headless",
                    "--no-sandbox",
                    "--disable-gpu",
                    "--whitelisted-ips"
                );

                using IWebDriver driver = new ChromeDriver(chromeOptions);                
                driver.Navigate().GoToUrl(url);

                // Wait for dynamic content to load
                System.Threading.Thread.Sleep(waitingDuration);

                string pageSource = driver.PageSource;
                driver.Quit();

                var doc = new HtmlDocument();
                doc.LoadHtml(pageSource);

                var descElement = doc.DocumentNode.SelectSingleNode("//div[@class='accordion-item col-xl-8']");
                description = descElement.InnerHtml.ToString();
            }
            catch (Exception ex)
            {
                description = ex.Message;
            }

            return new OkObjectResult(description);

            //_logger.LogInformation("C# HTTP trigger function processed a request.");
            //return new OkObjectResult("Welcome to Azure Functions!");
        }
    }
}
