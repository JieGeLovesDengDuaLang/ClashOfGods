using System.Net.Http;

namespace COG.Utils;

public static class WebUtils
{
    public static string GetWeb(string url)
    {
        // 创建 HttpClient 实例
        var client = new HttpClient();

        try
        {
            // 发送 GET 请求，并获取响应
            var response = client.GetAsync(url).Result;

            // 确保响应成功
            response.EnsureSuccessStatusCode();

            // 读取响应内容
            var responseBody = response.Content.ReadAsStringAsync().Result;

            // 返回网页源代码
            return responseBody;
        }
        catch (System.Exception e)
        {
            Main.Logger.LogError(e.Message);
        }

        // 请求失败时返回空字符串
        return string.Empty;
    }
}