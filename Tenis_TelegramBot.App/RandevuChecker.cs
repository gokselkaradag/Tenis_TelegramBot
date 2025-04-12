using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Microsoft.Extensions.Configuration;

namespace Tenis_TelegramBot.App;

public class RandevuChecker
{
    private readonly IConfiguration _config;

    public RandevuChecker(IConfiguration config)
    {
        _config = config;
    }

    public List<string> GetAvailableSessions()
    {
        var options = new ChromeOptions();
        options.AddArgument("--headless");
        options.AddArgument("--disable-gpu");
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-dev-shm-usage");

        using var driver = new ChromeDriver(options);

        try
        {
            string tcId = _config["Credentials:TCId"];
            string password = _config["Credentials:Password"];

            driver.Navigate().GoToUrl("https://online.spor.istanbul/uyegiris");
            Thread.Sleep(2000);

            driver.FindElement(By.XPath("//input[@placeholder='TC Kimlik No']")).SendKeys(tcId);
            driver.FindElement(By.XPath("//input[@placeholder='Şifre']")).SendKeys(password + Keys.Enter);
            Thread.Sleep(5000);

            driver.Navigate().GoToUrl("https://online.spor.istanbul/satiskiralik");
            Thread.Sleep(2000);

            new SelectElement(driver.FindElement(By.Id("ddlBransFiltre"))).SelectByText("TENİS");
            Thread.Sleep(3000);
            new SelectElement(driver.FindElement(By.Id("ddlTesisFiltre"))).SelectByText("FLORYA SPOR TESİSİ");
            Thread.Sleep(3000);

            var salonNames = new List<string>
            {
                "AÇIK TENİS KORTU 2",
                "KAPALI TENİS KORTU 1",
                "KAPALI TENİS KORTU 3",
                "KAPALI TENİS KORTU 4",
                "KAPALI TENİS KORTU 5",
                "KAPALI TENİS KORTU 6"
            };

            var availableSessions = new List<string>();
            var now = DateTime.Now;

            foreach (var salonName in salonNames)
            {
                new SelectElement(driver.FindElement(By.Id("ddlSalonFiltre"))).SelectByText(salonName);
                Thread.Sleep(4000);

                var slots = driver.FindElements(By.CssSelector(".wellPlus"));
                foreach (var slot in slots)
                {
                    try
                    {
                        var timeSpan = slot.FindElement(By.ClassName("lblStyle")).Text.Trim();
                        var rezervasyonBtn = slot.FindElement(By.CssSelector("a[title='Rezervasyon']"));

                        if (!string.IsNullOrEmpty(timeSpan) && rezervasyonBtn != null)
                        {
                            if (DateTime.TryParse(timeSpan.Split('-')[0].Trim(), out DateTime startTime))
                            {
                                bool isSunday = now.DayOfWeek == DayOfWeek.Sunday;
                                if (isSunday || (!isSunday && startTime.Hour >= 17))
                                {
                                    availableSessions.Add($"[{salonName}] {now:dddd dd.MM.yyyy} - {timeSpan}");
                                }
                            }
                        }
                    }
                    catch (NoSuchElementException)
                    {
                        // slot içinde buton yoksa atla
                    }
                }
            }

            return availableSessions;
        }
        catch (Exception ex)
        {
            return new List<string> { $"❌ Bot hata ile karşılaştı: {ex.Message}" };
        }
        finally
        {
            driver.Quit();
        }
    }
}