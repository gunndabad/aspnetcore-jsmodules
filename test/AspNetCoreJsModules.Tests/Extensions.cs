using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Html;

namespace AspNetCoreJsModules.Tests
{
    public static class Extensions
    {
        public static HtmlNode[] RenderToElements(this IHtmlContent content)
        {
            var html = content.RenderToString();
            var wrapped = $"<div>{html}</div>";
            var node = HtmlNode.CreateNode(wrapped);
            return node.ChildNodes.Where(n => n.NodeType == HtmlNodeType.Element).ToArray();
        }

        public static string RenderToString(this IHtmlContent content)
        {
            using var writer = new StringWriter();
            content.WriteTo(writer, HtmlEncoder.Default);
            return writer.ToString();
        }
    }
}
