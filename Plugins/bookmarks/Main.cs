using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using Wox.Plugin;
using BookmarksManager.Chrome;
using Newtonsoft.Json;

namespace bookmarks
{
    public class Main : IPlugin
    {
        private PluginInitContext context;
        private BookmarksManager.BookmarkFolder bookmarks;
        private static string LocalAppData = Environment.GetEnvironmentVariable("LOCALAPPDATA");
        private string bookmarkFile = string.Format(@"{0}\Google\Chrome\User Data\Default\Bookmarks", LocalAppData);
        List<Result> bookmarkList = new List<Result> { };
        public List<Result> Query(Query query)
        {
            var resList1 = bookmarkList.Where(p => p.Title.ToLower().Contains(query.Search.ToLower())).ToList();
            return resList1;
        }

        public void Init(PluginInitContext context)
        {
            this.context = context;

            try
            {
                var fileTest = File.OpenRead(this.bookmarkFile);
            }
            catch (Exception e)
            {
                Console.WriteLine("{0} Exception caught.", e);
            }

            using (var file = File.OpenRead(this.bookmarkFile))
            {
                var reader = new ChromeBookmarksReader();

                this.bookmarks = reader.Read(file);

                foreach (var b in this.bookmarks.AllLinks)
                {
                    bookmarkList.Add(new Result
                    {
                        Title = b.Title,
                        SubTitle = b.Url,
                        Action = e =>
                        {
                            if (!b.Url.ToLower().StartsWith("http"))
                            {
                                b.Url = "http://" + b.Url;
                            }
                            try
                            {
                                Process.Start(b.Url);
                                return true;
                            }
                            catch (Exception ex)
                            {
                                context.API.ShowMsg(string.Format(context.API.GetTranslation("wox_plugin_url_canot_open_url"), b.Url));
                                return false;
                            }
                        }
                    });
                }
            }
        }
    }    
}
