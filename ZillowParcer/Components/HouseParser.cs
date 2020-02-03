using Homebrew;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ZillowParser.Rucaptcha;

namespace ZillowParser.Components
{
    public class HouseParser:Parser
    {
        public List<Zillow> results = new List<Zillow>();
        private int threadCount = 0;
        private int progress = 0;
        public HouseParser(List<string> phraseForSearch)
        {
            DebugBox.WriteLine("Начинаем парсинг данных.");
            phraseForSearch.ForEach(phrase =>
            {
                Zillow zillow = new Zillow();
                string link = "https://www.zillow.com/homes/" + phrase.Replace(" ", "-")+ "_rb/";
                zillow.URL = link;
                results.Add(zillow);
            });
            CookieSet();
            results.ForEach(result =>
            {
                while (threadCount >= 200)
                {
                    Thread.Sleep(500);
                }
                threadCount++;
                ParsInfo(result);
            });
            while (threadCount != 0)
            {
                Thread.Sleep(1000);
            }
            DebugBox.WriteLine("Парсинг завершён!");

        }
        private void CookieSet()
        {
            LinkParser linkParser;
            do
            {
                ReqParametres reqParametres = new ReqParametres("https://www.zillow.com/");
                reqParametres.SetUserAgent(Useragents.GetNewUseragent());
                //reqParametres.SetProxy();
                if (SavedCookies != null)
                {
                    reqParametres.SetCookie(SavedCookies);
                }
                linkParser = new LinkParser(reqParametres.Request);
                SavedCookies = linkParser.Cookies;
            } while (isCaptcha(linkParser.Data));
        }
        private void ParsInfo(Zillow zillow)
        {
            Thread thread = new Thread(() =>
            {
                LinkParser linkParser;
                ReqParametres reqParametres;
                do
                {
                    //Парсим предварительную ссылку
                    reqParametres = new ReqParametres(zillow.URL);
                    reqParametres.SetUserAgent(Useragents.GetNewUseragent());
                    reqParametres.SetProxy();
                    linkParser = new LinkParser(reqParametres.Request);
                    SavedCookies = linkParser.Cookies;

                } while (isCaptcha(linkParser.Data));
                string newLink = linkParser.Data.ParsFromTo("<link rel=\"canonical\" href=\"", "\"");
                //Проверяем на неверную ссылку
                if (newLink.Contains("https://www.zillow.com/homes/for_sale/"))
                {
                    zillow.Status = "No such adress";
                }
                else
                {
                    zillow.URL = newLink;

                    do
                    {
                        reqParametres = new ReqParametres(zillow.URL);
                        reqParametres.SetUserAgent(Useragents.GetNewUseragent());
                        reqParametres.SetProxy();
                        linkParser = new LinkParser(reqParametres.Request);
                    } while (isCaptcha(linkParser.Data));
                    zillow.Status = CheckOnStatus(linkParser.Data.ToLower()).Replace("<span tabindex=\"0\" role=\"button\"><span class=\"zsg-tooltip-launch_keyword\">","")
                                                                            .Replace("<Span Tabindex=\"0\" Role=\"Button\"><Span Class=\"Zsg-Tooltip-Launch_Keyword\">","");
                    if (zillow.Status.Equals("Undefined"))
                    {
                        DebugBox.WriteLine(linkParser.Data);
                        while (true)
                        {
                            Thread.Sleep(1000);
                        }
                    }
                    //Zestimate set
                    List<string> rawZestimate = linkParser.Data.ParsRegex("Zestimate<sup>®</sup></span></span>(.*?)\\$([0-9,./a-zA-Z]+)<", 2);
                    if (rawZestimate.Count != 0)
                    {
                        zillow.Zestimate = "$" + rawZestimate[0];
                    }
                    zillow.SoldPrice = CheckPrice(linkParser.Data);
                    SavedCookies = linkParser.Cookies;
                }
                threadCount--;
                progress++;
                DebugBox.WriteLine($"Обработано ссылок: {progress} из {results.Count}.");
                double val = 100.0f / results.Count * progress;
                WorkProgress.SetValue(val);
                
            });
            thread.IsBackground = true;
            thread.Start();
            
        }

        private string CheckPrice(string data)
        {
            if (!data.ParsFromTo("Sold<span>: <!-- -->", "</span>").Equals(""))
            {
                return data.ParsFromTo("Sold<span>: <!-- -->", "</span>");
            }
            else if (!data.ParsFromTo("ds-price\"><span><span class=\"ds-value\">", "</span>").Equals(""))
            {
                return data.ParsFromTo("ds-price\"><span><span class=\"ds-value\">", "</span>");
            } else
            {
                return "";
            }
        }
        private string CheckOnStatus(string dataForCheck)
        {
            if (dataForCheck.Contains("zsg-icon-recently-sold\"></span>sold</span>")||dataForCheck.Contains("zsg-icon-recently-sold\"></span> <!-- -->sold<span>: <!-- -->"))
            {
                return "Sold";
            } 
            else if (dataForCheck.Contains("sc-kgobcf sc-hghygh brijv"))
            {
                return "Rent Units";
            }
            else if (dataForCheck.Contains("ds-status-icon zsg-icon-for-rent"))
            {
                return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(dataForCheck.ParsRegex("<span class=\"ds-status-icon zsg-icon-for-rent\"></span>(.*?)</span>",1)[0]);
            }
            else if (dataForCheck.Contains("ds-status-icon zsg-icon-for-sale"))
            {
                return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(dataForCheck.ParsRegex("ds-status-icon zsg-icon-for-sale\"></span>(.*?)</span>",1)[0]);
            } 
            else if (dataForCheck.Contains("zsg-tooltip-launch_keyword\">off market</span></span>"))
            {
                return "Off Market";
            }
            else
            {
                return "Undefined";
            }
        }
        private bool isCaptcha(string data)
        {
            if (data.Contains("https://captcha.px-cdn.net/PXHYx10rg3/captcha.js") || data.Equals(""))
            {
                SavedCookies = new CookieContainer();
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
