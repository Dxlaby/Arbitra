using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Arbitra.DataStructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using OpenQA.Selenium.Firefox;
using ProtoBuf;
using Microsoft.ClearScript.V8;

namespace Arbitra.Background.MatchFinders
{
    internal class SynottipMatchFinder : IMatchFinder
    {
        string _bettingShopName; 
        string url;
        string mainUrl;
        string urlAllMatchesAPI;
        private string jsPath;
        private readonly V8ScriptEngine _engine;

        public SynottipMatchFinder()
        {
            _bettingShopName = "Synottip";
            url = "https://sport.synottip.cz";
            urlAllMatchesAPI =
                "https://sport.synottip.cz/WebServices/Api/SportsBettingService.svc/GetWebStandardEvents";
            _engine = new V8ScriptEngine();
            jsPath = "./Background/MatchFinders/SynottipAll.js";
            // Load the original JS file content
            string script = File.ReadAllText(jsPath);
        
            // Execute the script to initialize the 's' and 'r' namespaces 
            // that the decode function depends on.
            // _engine.Execute(@"
            //     var window = this; 
            //     var webpackJsonp = []; 
            //     var s = {}; // This will hold our Protobuf models
            //     var u = {   // This will hold our Utility/Base64 functions
            //         util: {
            //             base64: {
            //                 decode: function(string) {
            //                     // Manual Base64 decode for the engine
            //                     return new Uint8Array(Array.from(atob(string), c => c.charCodeAt(0)));
            //                 }
            //             }
            //         }
            //     };
            // ");
            // _engine.Execute(script);
        }

        public ListOfMatches FindAllMatchesApi()
        {
            using var httpClient = new HttpClient();
            
            // 1. Define the JSON body based on what you found in
            var requestBody = new
            {
                LanguageID = 12,
                Token = "4be2ffeaf2d471cac7316cb9ee9abb72", // Note: Tokens usually expire!
                CategoryID = "20",
                Top = 50,
                IncludeLiveCategories = true
            };
            var requestJson = JsonConvert.SerializeObject(requestBody);
            var payLoad = new StringContent(requestJson, Encoding.UTF8, "application/json");
            var response = httpClient.PostAsync(urlAllMatchesAPI, payLoad);
            var resultString = response.Result.Content.ReadAsStringAsync().Result;
            var resultJson = JsonNode.Parse(resultString);
            string encodedReturnValue = (string)resultJson["ReturnValue"];  
            
            // Source - https://stackoverflow.com/a
            // Posted by Matthew Abbott, modified by community. See post 'Timeline' for change history
            // Retrieved 2026-01-27, License - CC BY-SA 4.0
            
            // byte[] rawData = Convert.FromBase64String(encodedReturnValue);
            string messyJson = Decode(encodedReturnValue);
            Console.WriteLine(messyJson);
            return new ListOfMatches();
        }

        public string Decode(string base64string)
        {
            byte[] bytes = Convert.FromBase64String(base64string);
            using (var ms = new MemoryStream(bytes))
            {
                // This 'RootClass' is the top-level class QuickType made for you
                var decodedData = Serializer.Deserialize<WebStandardEventResponse>(ms);
                return  JsonConvert.SerializeObject(decodedData);
            }
        }
        
        public string DecodeJS(string base64Data)
        {
            // We pass the Base64 string as a variable to the JS engine
            string script = File.ReadAllText(jsPath);
            byte[] bytes = Convert.FromBase64String(base64Data);
            _engine.Script.bytes = bytes;

            _engine.AddHostObject("dotnetConsole", new JsConsole());
            _engine.Execute(@"
                var console = { log: (m) => dotnetConsole.log(m), error: (m) => dotnetConsole.error(m) };
                var window = this;
                var webpackJsonp = [];
                var s = undefined;
            ");
            
            // This code 'force-exports' the internal 'o' (the loader) to the window
            // so we can call it from C# anytime.
            string exportPatch = @"
                ; (function() {
                    // In your file, 'o' is the loader. We find it by looking at 
                    // the very first module which usually receives it.
                    window.getLoader = function() {
                        var loader = null;
                        window.webpackJsonp.push([
                            ['bridge'], 
                            { 'chk': function(m, e, o) { loader = o; } }, 
                            [['chk']]
                        ]);
                        return loader;
                    };
                })();";

            _engine.Execute(script + exportPatch);
            
            _engine.Execute(@"
                var loader = window.getLoader();
                if (loader) {
                    // We know from your file that module 304 initializes the roots
                    var protoModule = loader(303);
                    
                    // This 's' is the variable you saw in the JS code: 
                    // var s = a.roots.default || (a.roots.default = {});
                    window.s = protoModule.roots;
                    
                    console.log('Successfully captured Protobuf roots!');
                }
            ");
            
            var json = _engine.Evaluate(@"
                (function() {
                    try {
                        // Use the library's built-in base64 tool we found in your file
                        var decoded = s.GetWebStandardEventExtResponse.decode(bytes);
                        return JSON.stringify(decoded);
                    } catch(e) {
                        return 'Error during decode: ' + e.message;
                    }
                })()
            ");

            return "";
        }
    }

    public class JsConsole
    {
        public void log(object message)
        {
            Console.WriteLine($"[JS LOG]: {message}");
        }

        public void error(object message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[JS ERROR]: {message}");
            Console.ResetColor();
        }
    }

    [ProtoContract]
    public class WebStandardEventResponse
    {
        [ProtoMember(1)]
        public List<SportContainer> Sports { get; set; }

        [ProtoMember(2)]
        public CommonField CommonInfo { get; set; }

        [ProtoMember(3)]
        public string GlobalValue { get; set; } // "As uint: 148"
    }

    [ProtoContract]
    public class SportContainer
    {
        [ProtoMember(1)]
        public SportDetail Detail { get; set; }

        [ProtoMember(3)]
        public string SportStatus { get; set; } // "As uint: 865"

        [ProtoMember(4)]
        public LocationContainer Location { get; set; }
    }

    [ProtoContract]
    public class SportDetail
    {
        [ProtoMember(1)]
        public string Id { get; set; } // "20"

        [ProtoMember(2)]
        public string Name { get; set; } // "Stolní tenis"

        [ProtoMember(7)]
        public string Type { get; set; }
    }

    [ProtoContract]
    public class LocationContainer
    {
        [ProtoMember(1)]
        public Region Region { get; set; }

        [ProtoMember(2)]
        public LeagueWrapper LeagueWrap { get; set; }
    }

    [ProtoContract]
    public class Region
    {
        [ProtoMember(1)]
        public string RegionId { get; set; } // "x43"

        [ProtoMember(2)]
        public string Name { get; set; } // "Mezinárodní"
    }

    [ProtoContract]
    public class LeagueWrapper
    {
        [ProtoMember(1)]
        public LeagueDetail Detail { get; set; }
    }

    [ProtoContract]
    public class LeagueDetail
    {
        [ProtoMember(1)]
        public string LeagueId { get; set; } // "xx8379"

        [ProtoMember(2)]
        public string Name { get; set; } // "TT Elite Series"

        [ProtoMember(5)]
        public Match0 Match { get; set; }
    }

    [ProtoContract]
    public class Match0
    {
        [ProtoMember(1)]
        public string MatchId { get; set; }

        [ProtoMember(2)]
        public string Participants { get; set; } // "Fabis, Adrian - Szlubowski, Dariusz"

        [ProtoMember(4)]
        public Timestamp Time { get; set; }

        [ProtoMember(6)]
        public MarketGroup MarketGroup { get; set; }

        [ProtoMember(12)]
        public string Status { get; set; }
    }

    [ProtoContract]
    public class MarketGroup
    {
        [ProtoMember(1)]
        public string GroupId { get; set; }

        [ProtoMember(2)]
        public string GroupName { get; set; } // "Hlavní sázky"

        [ProtoMember(3)]
        public Market Market { get; set; }
    }

    [ProtoContract]
    public class Market
    {
        [ProtoMember(1)]
        public string MarketId { get; set; }

        [ProtoMember(2)]
        public string MarketName { get; set; } // "Vítěz zápasu"

        [ProtoMember(6)]
        public SelectionGroup Selections { get; set; }
    }

    [ProtoContract]
    public class SelectionGroup
    {
        [ProtoMember(4)]
        public Selection Odds { get; set; }
    }

    [ProtoContract]
    public class Selection
    {
        [ProtoMember(1)]
        public string OutcomeId { get; set; }

        [ProtoMember(2)]
        public string OutcomeValue { get; set; }

        [ProtoMember(3)]
        public string Price { get; set; } // Kurz (binární podoba)
    }

    [ProtoContract]
    public class Timestamp { [ProtoMember(1)] public long Value { get; set; } }

    [ProtoContract]
    public class CommonField
    {
        [ProtoMember(1)] public string Id { get; set; }
        [ProtoMember(2)] public string Label { get; set; } // "X. set - Lichá/Sudá"
    }
}
