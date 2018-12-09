using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace MetricTestWebApp.Controllers
{
    [Route("[controller]/[action]")]
    public class TestController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        private bool IsPrime(int i)
        {
            for (var j = 2; j < i; j++)
            {
                if (i % j == 0)
                {
                    return false;
                }
            }
            return true;
        }

        [HttpPost]
        public IActionResult Cpu()
        {
            var primes = new List<int>();
            foreach (var i in Enumerable.Range(2, 1000 * 100))
            {
                if (IsPrime(i))
                {
                    primes.Add(i);
                }
            }
            Console.WriteLine(string.Join(", ", primes));
            return Redirect("/");
        }

        [HttpPost]
        public IActionResult Memory()
        {
            var strings = new List<string>();
            foreach (var i in Enumerable.Range(1, 1000))
            {
                strings.Add(new string('*', 1000));
            }
            Console.WriteLine(string.Join(", ", strings.Select(x => x.Length)));
            return Redirect("/");
        }
    }
}