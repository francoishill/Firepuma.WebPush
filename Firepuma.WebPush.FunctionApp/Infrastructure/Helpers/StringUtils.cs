using System;

namespace Firepuma.WebPush.FunctionApp.Infrastructure.Helpers;

public static class StringUtils
{
    public static string CreateMd5(string input)
    {
        using var md5 = System.Security.Cryptography.MD5.Create();

        var inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
        var hashBytes = md5.ComputeHash(inputBytes);

        return Convert.ToHexString(hashBytes).ToLower();
    }
}