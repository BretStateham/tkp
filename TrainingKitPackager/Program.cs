using Strike.IE;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace TrainingKitPackager
{
    class Program
    {
        static void Main(string[] args)
        {
            string defaultPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Readiness-CloudCamp");
            //Get root directory
            DirectoryInfo dir;
            do
            {
                Console.WriteLine("Input source root (default is {0}): ",defaultPath);
                string rootPath = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(rootPath))
                {
                    rootPath = defaultPath;
                }
                dir = new DirectoryInfo(rootPath);
            } while (!dir.Exists);

            //Default to an empty template
            string template = "{{content}}";
            string templatePath = Path.Combine(dir.FullName, @"_layouts\default.html");
            if (File.Exists(templatePath))
            {
                template = File.ReadAllText(templatePath);
            }

            //Iterate .md files
            string extension = "*.md";
            var files = dir.GetFiles(extension, SearchOption.AllDirectories);

            foreach (var file in files)
            {
                Console.WriteLine("Processing file: {0}", file.FullName);

                //Hooks for markdown link fixup, image inlining
                //DefineDocumentProcessingHooks(md, file);

                //Write HTML to file
                string output = File.ReadAllText(file.FullName);

                output = TransformDoc(output);

                //Don't template if md is alreay an HTML doc
                if (!output.StartsWith("<html"))
                {
                    output = template.Replace("{{ content }}", output);
                    int depth = file.FullName.Replace(dir.FullName, "").Count(a => a == '\\');
                    string relative = string.Concat(Enumerable.Repeat("../", depth - 1));
                    output = output.Replace("{{ relative }}", relative);
                }

                using (var outHtml = File.CreateText(file.FullName.Replace(".md", ".htm")))
                {
                    outHtml.Write(output);
                }
            }

            //Clean up directories
            ".git;obj;bin;_layouts;".Split(';')
                .SelectMany(g => dir.EnumerateDirectories(g, SearchOption.AllDirectories))
                .ToList()
                .ForEach(d => ForceDelete(d));

            //Clean up files
            //"*.suo;*.user;*.gitignore;*.gitattributes;*.md".Split(';')
            //Removing the *.gitignore files from the list of files to delete
            "*.suo;*.user;*.gitattributes;*.md".Split(';')
                .SelectMany(g => dir.EnumerateFiles(g, SearchOption.AllDirectories))
                .ToList()
                .ForEach(d => d.Delete());

            //Compress top level directories
        }

        private static string TransformDoc(string input)
        {
            var renderMethods = new Strike.RenderMethods
            {
                Link =
                   @"function(href, title, text){
                       //Determine if relative url
                       if(!/^(https?:)\w+/.test(href)) {
                         //Filter out links to in-page anchors
                         if(href.charAt(0) !== '#') {
                           //Ensure relative urls to folders end with /readme.htm
                   	       if(href.slice(href.lastIndexOf('/')).lastIndexOf('.') < 0) {
                             //Ensure path ends with /
                             if(!/\/$/.test(href)) {
                               href += '/';
                             }
                             href += 'readme.htm';
                           // Otherwise, for relative URLs ending with .md, replace .md with .htm
                           } else if (href.toLowerCase().lastIndexOf('.md') == href.length - 3) {
                             href = href.replace('.md','.htm');
                           }
                         }
                       }

                     var out = marked.Renderer.prototype.link.apply(this, arguments);
                     return out; 
                   }",
                Image =
                   @"function(href, title, text){
                     var out = marked.Renderer.prototype.image.apply(this, arguments);
                     out = out.replace('<img ', '<img class=""img-responsive""');
                     return out; 
                   }",
                Heading =
                   @"function(text, level){
                     var out = marked.Renderer.prototype.heading.apply(this, arguments);
                     if(level === 1) {
                       out = out.replace('<h1', '<div class=""jumbotron""><h1');
                       out = out.replace('</h1>', '</h1></div>');
                     }
                     return out; 
                   }"
            };

            using (var markdownify = new Markdownify(new Strike.Options(), renderMethods))
            {

                return markdownify.Transform(input);
            }
        }

        public static void ForceDelete(DirectoryInfo directory)
        {
            directory.Attributes = FileAttributes.Normal;

            directory.GetFileSystemInfos("*", SearchOption.AllDirectories)
                .ToList()
                .ForEach(i => i.Attributes = FileAttributes.Normal);

            directory.Delete(true);
        }

        //private static void DefineDocumentProcessingHooks(MarkdownDeep.Markdown md, FileInfo file)
        //{
        //    md.PrepareImage = delegate(MarkdownDeep.HtmlTag tag, bool TitledImage)
        //    {
        //        //Compress PNG
        //        string relPath = tag.attributes["src"].Replace('/', '\\').Replace("?raw=true", "");
        //        string path = Path.Combine(file.DirectoryName, relPath);
        //        Console.WriteLine("\tProcessing image: {0}", path);

        //        System.Drawing.Bitmap img = (Bitmap)Bitmap.FromFile(path);
        //        var quant = new nQuant.WuQuantizer();
        //        var ms = new MemoryStream();
        //        try
        //        {
        //            using (var quantized = quant.QuantizeImage(img))
        //            {
        //                quantized.Save(ms, ImageFormat.Png);
        //            }
        //        }
        //        catch
        //        {
        //            img.Save(ms, ImageFormat.Png);
        //        }
        //        finally
        //        {
        //            var b64String = Convert.ToBase64String(ms.ToArray());
        //            tag.attributes["src"] = "data:image/png;base64," + b64String;
        //        }
        //        return true;
        //    };

        //    md.PrepareLink = delegate(MarkdownDeep.HtmlTag tag)
        //    {
        //        // GitHub automatically shows Readme.md content for a folder.
        //        // So if there's a relative link to a folder, we need to link to the readme
        //        string href = tag.attributes["href"];
        //        Uri uri = new Uri(href, UriKind.RelativeOrAbsolute);
        //        if (!uri.IsAbsoluteUri && !Path.HasExtension(href))
        //        {
        //            tag.attributes["href"] = string.Format("{0}/readme.htm", tag.attributes["href"]);
        //        }
        //        return true;
        //    };
        //}
    }
}