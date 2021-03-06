﻿#region	License
//------------------------------------------------------------------------------------------------
// <License>
//     <Copyright> 2017 © Top Nguyen → AspNetCore → Puppy </Copyright>
//     <Url> http://topnguyen.net/ </Url>
//     <Author> Top </Author>
//     <Project> Puppy </Project>
//     <File>
//         <Name> HttpDetectionHelper.cs </Name>
//         <Created> 09/09/17 3:52:39 PM </Created>
//         <Key> b0bd4094-c38a-4d87-abe3-8f6147916633 </Key>
//     </File>
//     <Summary>
//         HttpDetectionHelper.cs
//     </Summary>
// <License>
//------------------------------------------------------------------------------------------------
#endregion License

using MaxMind.GeoIP2;
using Microsoft.AspNetCore.Http;
using Puppy.Core.LinqUtils;
using Puppy.Core.StringUtils;
using Puppy.Web.Constants;
using Puppy.Web.HttpUtils.HttpDetection.Device;
using System;
using System.IO;
using System.Linq;

namespace Puppy.Web.HttpUtils.HttpDetection
{
    public static class HttpDetectionHelper
    {
        // Marker
        public static string GetMarkerFullInfo(HttpRequest request)
        {
            var agent = GetUserAgent(request);
            if (string.IsNullOrWhiteSpace(agent)) return null;

            int iEnd = agent.IndexOf('(');

            if (iEnd < 0)
            {
                return null;
            }

            string markerFullInfo = agent.Substring(0, iEnd)?.Trim();
            return markerFullInfo;
        }

        public static string GetMarkerName(HttpRequest request)
        {
            string markerName = GetMarkerFullInfo(request)?.Split('/').FirstOrDefault()?.Trim();
            return markerName;
        }

        public static string GetMarkerVersion(HttpRequest request)
        {
            string markerVersion = GetMarkerFullInfo(request)?.Split('/').LastOrDefault()?.Trim();
            return markerVersion;
        }

        // OS
        public static string GetOsFullInfo(HttpRequest request)
        {
            var agent = GetUserAgent(request);
            if (string.IsNullOrWhiteSpace(agent)) return null;

            int iStart = agent.IndexOf('(') + 1;
            int iEnd = agent.IndexOf(')') - iStart;

            if (iEnd < 0)
            {
                return null;
            }

            string osFullInfo = agent.Substring(iStart, iEnd)?.Trim();
            return osFullInfo;
        }

        public static string GetOsName(HttpRequest request)
        {
            string osName = GetOsFullInfo(request)?.Split(';').FirstOrDefault()?.Trim();
            return osName;
        }

        public static string GetOsVersion(HttpRequest request)
        {
            var infos = GetOsFullInfo(request)?.Split(';');

            string osVersion = null;

            if (infos != null && infos.Length > 0)
            {
                int i = 1;
                while (i <= infos.Length && (osVersion == null || osVersion.ToLowerInvariant() == "u"))
                {
                    osVersion = infos[i];
                    i++;
                }
            }

            return osVersion;
        }

        // Engine
        public static string GetEngineFullInfo(HttpRequest request)
        {
            var agent = GetUserAgent(request);
            if (string.IsNullOrWhiteSpace(agent)) return null;

            int iStart = agent.IndexOf(')') + 1;
            string engineFullInfo = agent.Substring(iStart)?.Trim();

            if (string.IsNullOrWhiteSpace(engineFullInfo)) return null;

            int iEnd = engineFullInfo.IndexOf(' ');

            if (iEnd < 0)
            {
                return null;
            }

            engineFullInfo = engineFullInfo.Substring(0, iEnd);
            return engineFullInfo;
        }

        public static string GetEngineName(HttpRequest request)
        {
            string engineName = GetEngineFullInfo(request)?.Split('/').FirstOrDefault()?.Trim();

            // Standardize engine name
            const string webKitStandardName = "WebKit";
            engineName = engineName?.EndsWith(webKitStandardName) == true ? webKitStandardName : engineName;

            return engineName;
        }

        public static string GetEngineVersion(HttpRequest request)
        {
            string engineName = GetEngineFullInfo(request)?.Split('/').LastOrDefault()?.Trim();
            return engineName;
        }

        // Browser

        /// <summary>
        ///     Get browser info in user agent 
        /// </summary>
        /// <param name="request">      </param>
        /// <param name="isStandardize"> will exclude "version", "mobile" from browser info </param>
        /// <returns></returns>
        public static string GetBrowserFullInfo(HttpRequest request, bool isStandardize = true)
        {
            var agent = GetUserAgent(request);
            if (string.IsNullOrWhiteSpace(agent)) return null;

            int iStart = agent.LastIndexOf(')') + 1;
            string browserFullInfo = agent.Substring(iStart)?.Trim();

            // Filters
            if (!string.IsNullOrWhiteSpace(browserFullInfo))
            {
                var listExclude = new[] { "version", "mobile" };

                var browserInfos = browserFullInfo.Split(' ').Where(x => !string.IsNullOrWhiteSpace(x)).ToList();

                if (isStandardize)
                {
                    browserInfos = browserInfos.RemoveWhere(x => listExclude.Any(y => x.ToLowerInvariant().Contains(y))).ToList();
                }

                browserFullInfo = string.Join(", ", browserInfos);
            }

            return browserFullInfo;
        }

        public static string GetBrowserName(HttpRequest request)
        {
            string browserName = GetBrowserFullInfo(request)?.Split(',').FirstOrDefault()?.Split('/').FirstOrDefault()?.Trim();
            return browserName;
        }

        public static string GetBrowserVersion(HttpRequest request)
        {
            string browserFullInfo = GetBrowserFullInfo(request, false);

            // Filters
            if (string.IsNullOrWhiteSpace(browserFullInfo)) return null;

            var browserInfos = browserFullInfo.Split(' ').ToList();
            var browserVersion = browserInfos.FirstOrDefault(x => x.ToLowerInvariant().Contains("version"))?.Split('/').LastOrDefault()?.Trim().Trim(',');

            if (string.IsNullOrWhiteSpace(browserVersion))
            {
                browserVersion = GetBrowserFullInfo(request)?.Split(',').FirstOrDefault()?.Split('/').LastOrDefault()?.Trim();
            }

            return browserVersion;
        }

        // Other
        public static string GetIpAddress(HttpRequest request)
        {
            var ipAddress = string.Empty;

            // Priority to Proxy Server

            if (request.Headers.TryGetValue(HeaderKey.CFConnectingIP, out var cloudFareConnectingIp))
            {
                ipAddress = cloudFareConnectingIp;

                return ipAddress;
            }

            if (request.Headers.TryGetValue(HeaderKey.CFTrueClientIP, out var cloudFareTrueClientIp))
            {
                ipAddress = cloudFareTrueClientIp;

                return ipAddress;
            }

            // Look for the X-Forwarded-For (XFF) HTTP header field it's used for identifying the
            // originating IP address of a client connecting to a web server through an HTTP proxy or
            // load balancer.
            string xff = request.Headers?
                .Where(x => HeaderKey.XForwardedFor.Equals(x.Key, StringComparison.OrdinalIgnoreCase))
                .Select(k => request.Headers[k.Key]).FirstOrDefault();

            // If you want to exclude private IP addresses, then see http://stackoverflow.com/questions/2577496/how-can-i-get-the-clients-ip-address-in-asp-net-mvc
            if (!string.IsNullOrWhiteSpace(xff))
            {
                var lastIp = xff.Split(',').FirstOrDefault();
                ipAddress = lastIp;
            }

            if (string.IsNullOrWhiteSpace(ipAddress) || ipAddress == "::1" || ipAddress == "127.0.0.1")
            {
                ipAddress = request.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            }

            if (string.IsNullOrWhiteSpace(ipAddress))
            {
                return null;
            }

            // Standardize
            if (ipAddress == "::1")
            {
                ipAddress = "127.0.0.1";
            }

            // Remove port
            int index = ipAddress.IndexOf(":", StringComparison.OrdinalIgnoreCase);

            if (index > 0)
            {
                ipAddress = ipAddress.Substring(0, index);
            }

            return ipAddress;
        }

        public static DeviceModel GetLocation(HttpRequest request, DeviceModel device)
        {
            string geoDbRelativePath = Path.Combine(nameof(HttpUtils), nameof(HttpDetection), "GeoCity.mmdb");

            string geoDbAbsolutePath = geoDbRelativePath.GetFullPath();

            if (!File.Exists(geoDbAbsolutePath))
            {
                // Try to get folder in executed assembly
                geoDbAbsolutePath = geoDbRelativePath.GetFullPath(null);
            }

            if (!File.Exists(geoDbAbsolutePath)) return null;

            using (var reader = new DatabaseReader(geoDbAbsolutePath))
            {
                var ipAddress = GetIpAddress(request);

                if (!reader.TryCity(ipAddress, out var city))
                {
                    return device;
                }

                if (city == null) return device;

                device.IpAddress = city.Traits.IPAddress;

                // City
                device.CityName = city.City.Names.TryGetValue("en", out var cityName)
                    ? cityName
                    : city.City.Name;
                device.CityGeoNameId = city.City.GeoNameId;

                // Country
                device.CountryName = city.Country.Names.TryGetValue("en", out var countryName)
                    ? countryName
                    : city.Country.Name;
                device.CountryGeoNameId = city.Country.GeoNameId;
                device.CountryIsoCode = city.Country.IsoCode;

                // Continent
                device.ContinentName = city.Continent.Names.TryGetValue("en", out var continentName)
                    ? continentName
                    : city.Continent.Name;
                device.ContinentGeoNameId = city.Continent.GeoNameId;
                device.ContinentCode = city.Continent.Code;

                // Location
                device.Latitude = city.Location.Latitude;
                device.Longitude = city.Location.Longitude;
                device.AccuracyRadius = city.Location.AccuracyRadius;

                device.PostalCode = city.Postal.Code;

                // Time Zone
                device.TimeZone = city.Location.TimeZone;

                return device;
            }
        }

        public static string GetUserAgent(HttpRequest request, bool isLowerCase = false)
        {
            var agent = request?.Headers[HeaderKey.UserAgent].ToString();

            if (!string.IsNullOrWhiteSpace(agent) && isLowerCase)
            {
                agent = agent.ToLowerInvariant();
            }

            return agent;
        }
    }
}