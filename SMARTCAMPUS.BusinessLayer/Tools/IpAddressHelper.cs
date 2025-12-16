using System.Net;
using System.Net.Sockets;

namespace SMARTCAMPUS.BusinessLayer.Tools
{
    public static class IpAddressHelper
    {
        /// <summary>
        /// Checks if an IP address is within campus network ranges
        /// </summary>
        public static bool IsCampusIpAddress(string? ipAddress)
        {
            if (string.IsNullOrEmpty(ipAddress))
                return false;

            if (!IPAddress.TryParse(ipAddress, out var ip))
                return false;

            foreach (var range in Constants.CampusNetworkConstants.CampusIpRanges)
            {
                if (IsIpInRange(ip, range))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if IP is in CIDR range
        /// </summary>
        private static bool IsIpInRange(IPAddress ip, string cidr)
        {
            var parts = cidr.Split('/');
            if (parts.Length != 2)
                return false;

            if (!IPAddress.TryParse(parts[0], out var networkIp))
                return false;

            if (!int.TryParse(parts[1], out var prefixLength))
                return false;

            var ipBytes = ip.GetAddressBytes();
            var networkBytes = networkIp.GetAddressBytes();

            if (ipBytes.Length != networkBytes.Length)
                return false;

            var bytesToCheck = prefixLength / 8;
            var bitsToCheck = prefixLength % 8;

            for (int i = 0; i < bytesToCheck; i++)
            {
                if (ipBytes[i] != networkBytes[i])
                    return false;
            }

            if (bitsToCheck > 0)
            {
                var mask = (byte)(0xFF << (8 - bitsToCheck));
                if ((ipBytes[bytesToCheck] & mask) != (networkBytes[bytesToCheck] & mask))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Gets client IP address from HttpContext
        /// </summary>
        public static string? GetClientIpAddress(Microsoft.AspNetCore.Http.HttpContext? httpContext)
        {
            if (httpContext == null)
                return null;

            // Check for forwarded IP (behind proxy/load balancer)
            if (httpContext.Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].ToString();
                if (!string.IsNullOrEmpty(forwardedFor))
                {
                    // X-Forwarded-For can contain multiple IPs, take the first one
                    var firstIp = forwardedFor.Split(',')[0].Trim();
                    return firstIp;
                }
            }

            // Check for real IP header
            if (httpContext.Request.Headers.ContainsKey("X-Real-IP"))
            {
                var realIp = httpContext.Request.Headers["X-Real-IP"].ToString();
                if (!string.IsNullOrEmpty(realIp))
                    return realIp;
            }

            // Fallback to RemoteIpAddress
            return httpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();
        }
    }
}

