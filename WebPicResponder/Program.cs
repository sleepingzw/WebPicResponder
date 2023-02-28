using System;
using System.Web;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;


WebListener WebListener = new WebListener();
Console.ReadKey();

public class WebListener
{
    private HttpListener _listener;
    private string port = "1145";//port

    public WebListener()
    {
        _listener = new HttpListener();
        _listener.Prefixes.Add("http://+:" + port + "/");

        _listener.Start();
        _listener.BeginGetContext(new AsyncCallback(ListenerHandle), _listener);
        Console.WriteLine("start listening");
    }
    private void ListenerHandle(IAsyncResult result)
    {
        try
        {
            var listener=(HttpListener)result.AsyncState!;
            if (listener.IsListening)
            {
                listener.BeginGetContext(ListenerHandle, listener);
                HttpListenerContext context = listener.EndGetContext(result);
                //解析Request请求
                HttpListenerRequest request = context.Request;

                //构造Response响应
                if (request.RawUrl!.StartsWith("/pics?"))
                {
                    GetPics(request.RawUrl, context.Response, !string.IsNullOrEmpty(request.Headers.Get("If-Modified-Since")));
                    Console.WriteLine("start request");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

    }
    private void GetPics(string url, HttpListenerResponse response, bool hasIfModifiedSince)
    {
        if (hasIfModifiedSince)
        {
            response.StatusCode = 304;
            response.Close();
            return;
        }

        response.StatusCode = 200;
        response.ContentType = "image/jpeg";
        response.ContentEncoding = Encoding.UTF8;
        response.AddHeader("Last-Modified", "Wed, 21 Oct 2000 12:00:00 GMT");
        var cardIdStr = url.Split('?').Last();

        var fileName = "C:\\Data\\Setu\\" + cardIdStr + "\\pic.jpg";//

        if (!File.Exists(fileName))
        {
            NoFound(url, response);
            Console.WriteLine(fileName);
            return;
        }

        using (var picFile = File.OpenRead(fileName))
            picFile.CopyTo(response.OutputStream);

        response.Close();
    }
    private void NoFound(string url, HttpListenerResponse response)
    {
        response.StatusCode = 404;
        response.ContentType = "text/html;charset=UTF-8";
        response.ContentEncoding = Encoding.UTF8;
        using (StreamWriter writer = new StreamWriter(response.OutputStream, Encoding.UTF8))
            writer.Write(url + " not found");
        response.Close();
    }
}
