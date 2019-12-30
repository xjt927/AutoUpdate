using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Services;

/// <summary>
/// StationPrinterUpdateService 的摘要说明
/// </summary>
[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// 若要允许使用 ASP.NET AJAX 从脚本中调用此 Web 服务，请取消注释以下行。 
// [System.Web.Script.Services.ScriptService]
public class StationPrinterUpdateService : System.Web.Services.WebService {

    public StationPrinterUpdateService () {

        //如果使用设计的组件，请取消注释以下行 
        //InitializeComponent(); 
    }

    /// <summary>
    /// 获取版本号
    /// </summary>
    /// <returns>更新版本号</returns>
    [WebMethod]
    public string GetVersion()
    {
        return ConfigurationManager.ConnectionStrings["StationPrinterVersion"].ConnectionString;
    }
    /// <summary>
    /// 获取下载地址
    /// </summary>
    /// <returns>下载地址</returns>
    [WebMethod]
    public string GetUrl()
    {
        return ConfigurationManager.ConnectionStrings["StationPrinterUrl"].ConnectionString + ConfigurationManager.ConnectionStrings["StationPrinterDirectory"].ConnectionString + "/";
    }
    /// <summary>
    /// 获取下载zip压缩包
    /// </summary>
    /// <returns>下载zip压缩包</returns>
    [WebMethod]
    public string[] GetZips()
    {
        string folder = HttpRuntime.AppDomainAppPath + ConfigurationManager.ConnectionStrings["StationPrinterDirectory"].ConnectionString;
        string[] zips = Directory.GetFileSystemEntries(folder);
        for (int i = 0; i < zips.Length; i++)
        {
            zips[i] = Path.GetFileName(zips[i]);
        }
        return zips;
    }
    
}
