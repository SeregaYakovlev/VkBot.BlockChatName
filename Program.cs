using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using PuppeteerSharp;

namespace VkBot
{
    class Program
    {
        private static string normalName = "10Б";
        private static int DefaultTimeout = 60 * 1000;
        private static WaitForSelectorOptions WaitForSelectorTimeout = new WaitForSelectorOptions { Timeout = DefaultTimeout };
        static async Task Main()
        {
            await InstallBrowserAsync();

            using (var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = false,
                DefaultViewport = null,

            }))
            {
                var Vk = await LoginInVk(browser);
                while (true)
                {
                    const string chatNameSelector = "#content > div > div.im-page.js-im-page.im-page_classic.im-page_history-show > div.im-page--history.page_block._im_page_history > div.im-page-history-w > div.im-page--chat-header._im_dialog_actions.im-page--chat-header_chat > div > div.im-page--toolsw > div.im-page--title-wrapper > div > span.im-page--title-main > span > a";
                    await Vk.WaitForSelectorAsync(chatNameSelector, WaitForSelectorTimeout);
                    string chatName = await Vk.QuerySelectorAsync(chatNameSelector).EvaluateFunctionAsync<string>("e => e.innerText");

                    if (chatName != normalName)
                    {
                        try
                        {
                            await ChangeChatName(Vk, chatNameSelector);
                        }
                        catch (PuppeteerException)
                        {
                            await Vk.ReloadAsync();
                        }
                    }
                    await Task.Delay(1000);
                }
            }
        }
        private static async Task<Page> LoginInVk(Browser browser)
        {
            var Vk = await browser.NewPageAsync();
            string url = "https://vk.com";
            await Vk.GoToAsync(url);
            Vk = await GetPage(browser, url);

            await Vk.WaitForSelectorAsync("#index_email", WaitForSelectorTimeout);
            await Vk.FocusAsync("#index_email");
            await Vk.Keyboard.TypeAsync("+79819720151");

            await Vk.WaitForSelectorAsync("#index_pass", WaitForSelectorTimeout);
            await Vk.FocusAsync("#index_pass");
            await Vk.Keyboard.TypeAsync("javascriptSergey/");

            await Vk.WaitForSelectorAsync("#index_login_button", WaitForSelectorTimeout);
            await Vk.ClickAsync("#index_login_button");

            await Vk.WaitForSelectorAsync("#l_msg > a > span > span.left_label.inl_bl", WaitForSelectorTimeout);
            await Vk.ClickAsync("#l_msg > a > span > span.left_label.inl_bl");
            return Vk;
        }

        private static async Task ChangeChatName(Page Vk, string chatNameSelector)
        {
            await Vk.ClickAsync(chatNameSelector);

            const string editalLabelSelector = "#ChatSettings > section > div > div.ChatSettingsInfo.ChatSettingsInfo--editable > header > h3 > div > div";
            await Vk.WaitForSelectorAsync(editalLabelSelector, WaitForSelectorTimeout);
            await Vk.ClickAsync(editalLabelSelector);

            await Vk.Keyboard.DownAsync("Control");
            await Vk.Keyboard.PressAsync("A");
            await Vk.Keyboard.UpAsync("Control");
            await Vk.Keyboard.PressAsync("Backspace");
            await Vk.Keyboard.TypeAsync(normalName);

            const string saveButtonSelector = "#ChatSettings > section > div > div.ChatSettingsInfo.ChatSettingsInfo--editable > header > h3 > div > button";
            await Vk.WaitForSelectorAsync(saveButtonSelector, WaitForSelectorTimeout);
            await Vk.ClickAsync(saveButtonSelector);

            const string closeWindowButton = "#ChatSettings > section > header > div > button";
            await Vk.WaitForSelectorAsync(closeWindowButton, WaitForSelectorTimeout);
            await Vk.ClickAsync(closeWindowButton);
        }
        private static async Task<Page> GetPage(Browser browser, string url)
        {
            Page[] pages;
            do
            {
                pages = await browser.PagesAsync();
                await Task.Delay(1000);
            } while (!pages.Any(p2 => p2.Url.StartsWith(url)));
            var page = pages.Single(p2 => p2.Url.StartsWith(url));
            page.DefaultTimeout = DefaultTimeout;
            page.DefaultNavigationTimeout = DefaultTimeout;
            return page;
        }

        private static async Task InstallBrowserAsync()
        {
            // Установка и обновление браузера chromium
            var browserFetcher = new BrowserFetcher();
            var localVersions = browserFetcher.LocalRevisions();

            if (!localVersions.Any() || BrowserFetcher.DefaultRevision != localVersions.Max())
            {
                Console.WriteLine("Downloading chromium...");
                browserFetcher.DownloadProgressChanged += (_, e) => { Console.Write("\r" + e.ProgressPercentage + "%"); };
                await browserFetcher.DownloadAsync(BrowserFetcher.DefaultRevision);
                Console.WriteLine();
            }
        }
    }
}
