using Homebrew;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZillowParser.Components
{
    public class HouseParser:Parser
    {
        private List<Zillow> results = new List<Zillow>();
        private int threadCount = 0;
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
                while (threadCount > 1)
                {
                    Thread.Sleep(500);
                }
                threadCount++;
                ParsInfo(result);
            });

        }
        private void CookieSet()
        {
            ReqParametres reqParametres = new ReqParametres("https://www.zillow.com/");
            reqParametres.SetUserAgent("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.53 Safari/537.36");
            LinkParser linkParser = new LinkParser(reqParametres.Request);
            SavedCookies = linkParser.Cookies;
        }
        private void ParsInfo(Zillow zillow)
        {
            new Thread(()=>
            {
                //Парсим предварительную ссылку
                ReqParametres reqParametres = new ReqParametres(zillow.URL);
                reqParametres.SetUserAgent("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.53 Safari/537.36");
                reqParametres.SetCookie(SavedCookies);
                LinkParser linkParser = new LinkParser(reqParametres.Request);
                string newLink = linkParser.Data.ParsFromTo("<link rel=\"canonical\" href=\"", "\"");
                SavedCookies = linkParser.Cookies;
                if (newLink.Contains("https://www.zillow.com/homes/for_sale/"))
                {
                    zillow.Status = "No such adress";
                }
                else
                {
                    zillow.URL = newLink;
                

                    reqParametres = new ReqParametres(zillow.URL);
                    reqParametres.SetUserAgent("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.53 Safari/537.36");
                    reqParametres.SetCookie(SavedCookies);
                    linkParser = new LinkParser(reqParametres.Request);
                    zillow.Status = CheckOnStatus(linkParser.Data);

                    //Zestimate set
                    List<string> rawZestimate = linkParser.Data.ParsRegex("Zestimate<sup>®</sup></span></span>(.*?)\\$([0-9,./a-zA-Z]+)<",2);
                    if (rawZestimate.Count != 0)
                    {
                        zillow.Zestimate = "$"+rawZestimate[0];
                    }

                    zillow.SoldPrice = linkParser.Data.ParsFromTo("Sold<span>: <!-- -->", "</span>");
                    SavedCookies = linkParser.Cookies;
                }
                threadCount--;
            }).Start();
        }        
        private string CheckOnStatus(string dataForCheck)
        {
            if (dataForCheck.Contains("<span class=\"zsg-icon-recently-sold\"></span> <!-- -->Sold<span>:"))
            {
                return "Sold";
            } 
            else if (dataForCheck.Contains("<h2 class=\"sc-kgoBCf sc-hgHYgh brIJv\">Units</h2"))
            {
                return "Rent Units";
            }
            else if (dataForCheck.Contains("<span class=\"ds-status-icon zsg-icon-for-rent\"></span>"))
            {
                return dataForCheck.ParsRegex("<span class=\"ds-status-icon zsg-icon-for-rent\"></span>(.*?)</span>")[0];
            }
            else if (dataForCheck.Contains("ds-status-icon zsg-icon-for-sale\"></span>"))
            {
                return dataForCheck.ParsRegex("ds-status-icon zsg-icon-for-sale\"></span>(.*?)</span>")[0];
            } else if (dataForCheck.Contains("zsg-tooltip-launch_keyword\">Off Market</span>"))
            {
                return "Off Market";
            }
            else
            {
                return "Undefined";
            }
        }
    }
}
