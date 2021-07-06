using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WeatherApp.Models;
using Microsoft.AspNetCore.Identity;
using System.Net.Http;
using Newtonsoft.Json;

namespace WeatherApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public HomeController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [Authorize]
        public IActionResult Index()
        {
            ResultViewModel rslt = WeatherDetail("London");
            ViewData["Title"] = "Weather Web Application";
            return View(rslt);
        }

        [HttpPost]
        public IActionResult Index(string cityName)
        {
            if (cityName != null)
            {
                ResultViewModel rslt = WeatherDetail(cityName);
                ViewData["Title"] = "Weather Web Application";
                return View(rslt);
            }
            else
            {
                ResultViewModel rslt = new ResultViewModel();
                ViewData["Title"] = "Not working you douchebag";
                return View(rslt);
            }
        }
        public IActionResult Login()
        {
            ViewData["Title"] = "Welcome. Please log in";
            return View();
        }
        public IActionResult Register()
        {
            return View(); ;
        }

        [HttpPost]
        public  async Task<IActionResult> Login(string username, string password)
        {
            // Login 
            var user = await _userManager.FindByNameAsync(username);


            if(user != null)
            {
                // sign in
                var signInResult = await _signInManager.PasswordSignInAsync(user, password, false, false);

                if(signInResult.Succeeded)
                {
                    return RedirectToAction("Index");
                }
            };
                
            return RedirectToAction("Index");
        }


        [HttpPost]
        public async Task<IActionResult> Register(string username, string password)
        {
            // Register

            var user = new IdentityUser
            {
                UserName = username,
                Email = "",
         
            };

            var result = await _userManager.CreateAsync(user, password);

            if(result.Succeeded)
            {
                var signInResult = await _signInManager.PasswordSignInAsync(user, password, false, false);

                if (signInResult.Succeeded)
                {
                    return RedirectToAction("Index");
                }
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> LogOut()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        public ResultViewModel WeatherDetail(string cityname)
        {
            string city = cityname;


            // API KEY from openweathermap.org --- my account 
            string appId = "616c64f09691fd50e2522c653d9529f0";
            //

            //API path with CITY parameter and other parameters.  
            string url = string.Format("http://api.openweathermap.org/data/2.5/weather?q={0}&units=metric&cnt=1&APPID={1}", city, appId);
            System.Console.WriteLine("This is the url " + url);
            using (var client = new System.Net.WebClient())
            {
                string json = client.DownloadString(url);
                RootObject weatherInfo = JsonConvert.DeserializeObject<RootObject>(json);


                ResultViewModel rslt = new ResultViewModel();

                rslt.Country = weatherInfo.sys.country;
                rslt.City = weatherInfo.name;
                rslt.Lat = Convert.ToString(weatherInfo.coord.lat);
                rslt.Lon = Convert.ToString(weatherInfo.coord.lon);
                rslt.Description = weatherInfo.weather[0].description;
                rslt.Humidity = Convert.ToString(weatherInfo.main.humidity);
                rslt.Temp = Convert.ToString(weatherInfo.main.temp);
                rslt.TempFeelsLike = Convert.ToString(weatherInfo.main.feels_like);
                rslt.TempMax = Convert.ToString(weatherInfo.main.temp_max);
                rslt.TempMin = Convert.ToString(weatherInfo.main.temp_min);
                rslt.WeatherIcon = weatherInfo.weather[0].icon;
   
                var jsonstring = JsonConvert.SerializeObject(rslt);

                return rslt;
            }
        }
    }
}
