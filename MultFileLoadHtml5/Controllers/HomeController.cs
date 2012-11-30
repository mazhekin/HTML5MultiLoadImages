using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MultFileLoadHtml5.Controllers
{
    public class HomeController : Controller
    {
        private const string filesDir = "~/App_Data/";

        //
        // GET: /Home/

        public ActionResult Index()
        {
            var files = Directory.GetFiles(HttpContext.Server.MapPath(filesDir)).Select(x => Path.GetFileName(x)).ToArray();
            return View(files);
        }

        //
        // GET: /Home/Icon/{id}

        public ActionResult Icon(string id/*fileName*/)
        {
            var fileName = id;
            string path = HttpContext.Server.MapPath(filesDir + fileName);

            using (Stream fileStream = System.IO.File.OpenRead(path))
            {
                using (var output = new System.IO.MemoryStream())
                {
                    FileService.GenerateThumbnail(fileStream, output, width: 200, height: 150);
                    this.Response.ContentType = "image/jpeg";
                    this.Response.AddHeader("Content-Length", output.Length.ToString());
                    this.Response.Cache.SetExpires(DateTime.Now/*.AddDays(30)*/);
                    this.Response.Cache.SetCacheability(HttpCacheability.NoCache/*Private*/);
                    this.Response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
                    output.CopyTo(this.Response.OutputStream);
                }

            }
            return new EmptyResult();
        }

        //
        // POST: /Home/Upload/

        public ActionResult Upload(FormCollection form)
        {
            System.Threading.Thread.Sleep(1 * 1000);

            var file = this.Request.Files[0];

            var stream = file.InputStream;

            string path = HttpContext.Server.MapPath(filesDir + file.FileName);
            using (Stream fileStream = System.IO.File.OpenWrite(path))
            {
                FileService.CopyStream(stream, fileStream);
            }

            return Json(new { result = "success" });
        }
    }

    class FileService
    {
        public static void GenerateThumbnail(System.IO.Stream input, System.IO.Stream output, int width, int height)
        {
            input.Position = 0;
            output.Position = 0;
            using (var image = new Bitmap(input))
            {
                var rect = FileService.GetThumbnailRectangle(image.Width, image.Height, ref width, ref height);

                using (var newImage = new Bitmap(width, height))
                using (var graphics = Graphics.FromImage(newImage))
                {
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.DrawImage(image, new RectangleF(0, 0, width, height), rect, GraphicsUnit.Pixel);

                    newImage.Save(output, ImageFormat.Jpeg);
                    output.Position = 0;
                }
            }
        }

        public static RectangleF GetThumbnailRectangle(Size originalSize, ref Size newSize)
        {
            var newWidth = newSize.Width;
            var newHeight = newSize.Height;
            var rect = FileService.GetThumbnailRectangle(originalSize.Width, originalSize.Height, ref newWidth, ref newHeight);
            newSize.Width = newWidth;
            newSize.Height = newHeight;
            return rect;
        }

        /// <summary>
        /// Calculate rectangle which will be used for thumbnail generation. For example if the original image is 200x300px
        /// and requested thumbnail size is 100x100px it should return { x = 0, y = 50, width = 200, height = 200 }. See unit test.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1305:FieldNamesMustNotUseHungarianNotation", Justification = "Reviewed. Suppression is OK here.")]
        public static RectangleF GetThumbnailRectangle(int originalWidth, int originalHeight, ref int newWidth, ref int newHeight)
        {
            // Basic validation
            if (originalWidth <= 0 || originalHeight <= 0)
            {
                throw new ArgumentException(string.Format("You must provide valid dimensions for an original image. OroginalWidth: {0}, OriginalHeight: {1}", originalWidth, originalHeight));
            }

            if (newWidth <= 0 && newHeight <= 0)
            {
                throw new ArgumentException(string.Format("You must provide valid dimensions for a new image. NewWidth: {0}, NewHeight: {1}", newWidth, newHeight));
            }

            int rectWidth = 0, rectHeight = 0;

            // Handle auto values. 0 means auto
            //
            // Original: 200x300, Requested: 0x100 (Auto x 100) => 67x100
            if (newWidth == 0)
            {
                newWidth = (int)Math.Round((float)originalWidth * (float)newHeight / (float)originalHeight);
                rectHeight = originalHeight;
            }

            // Original: 200x300, Requested: 100x0 (100 x Auto) => 100x150
            if (newHeight == 0)
            {
                newHeight = (int)Math.Round((float)originalHeight * (float)newWidth / (float)originalWidth);
                rectWidth = originalWidth;
            }

            // Calculate rectangle
            var xRatio = (float)newWidth / (float)originalWidth;
            var yRatio = (float)newHeight / (float)originalHeight;

            if (xRatio > yRatio)
            {
                var tmp = rectHeight == 0 ? ((float)newHeight * (float)originalWidth / (float)newWidth) : rectHeight;
                return new RectangleF(0, ((float)originalHeight - tmp) / (float)2, originalWidth, tmp);
            }
            else if (xRatio < yRatio)
            {
                var tmp = rectWidth == 0 ? ((float)newWidth * (float)originalHeight / (float)newHeight) : rectWidth;
                return new RectangleF(((float)originalWidth - tmp) / (float)2, 0, tmp, originalHeight);
            }

            return new RectangleF(0, 0, originalWidth, originalHeight);
        }

        public static void CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[8 * 1024];
            int len;
            while ((len = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, len);
            }
        }

    }
}
