using CodikSite.Algorithms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;


namespace CodikSite.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Данное веб-приложение предназначено для кодирования/декодирования текстов различными алгоритмами.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Напишите нам, если у вас возникли вопросы или предложения.";

            return View();
        }

        [HttpPost]
        public ActionResult Encode(string toEncode, string selection, int basis)
        {
            if (toEncode.Length == 0)
            {
                ViewData["ArgumentException"] = "Пустая строка, пожалуйста введите что-нибудь.";
                return View();
            }

            toEncode = toEncode.Replace("\r\n", "\n");
            try
            {
                double compressionRatio=0;
                switch (selection)
                {
                    case "LZ78":
                        ViewData["Encoded"] = new LZ78().Encode(toEncode, out compressionRatio);
                        ViewData["CompressionDegree"] = string.Format("{0:0.##}", compressionRatio);
                        break;
                    case "Хаффман":
                        ViewData["Encoded"] = new HuffmanCoding(basis).Encode(toEncode, out compressionRatio);
                        ViewData["CompressionDegree"] = string.Format("{0:0.##}", compressionRatio);
                        break;
                    case "Арифметическое":
                        ViewData["Encoded"] = new ArithmeticCoding().Encode(toEncode, out compressionRatio);
                        ViewData["CompressionDegree"] = string.Format("{0:0.##}", compressionRatio);
                        break;
                    case "BW":
                        ViewData["Encoded"] = new BWT().Encode(toEncode, out compressionRatio);
                        ViewData["CompressionDegree"] = string.Format("{0:0.##}", compressionRatio);
                        break;
                    case "RLE":
                        ViewData["Encoded"] = new RLE().Encode(toEncode, out compressionRatio);
                        ViewData["CompressionDegree"] = string.Format("{0:0.##}", compressionRatio);
                        break;
                    default:
                        ViewData["Encoded"] = "Упс...Что-то пошло не так.";
                        break;
                }
                //ViewData["CompressionDegree"] = string.Format("{0:0.##}", compressionRatio);
            }
            catch (Exception e)
            {
                ViewData["Error"] = e.Message;
            }

            return View();
        }

        public ActionResult Encode()
        {
            return View();
        }

        [HttpPost]
        public ActionResult UploadedText(IEnumerable<HttpPostedFileBase> fileUpload)
        {
            //var pathForView = (Request.UrlReferrer.AbsolutePath).Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries)[1];
            SetTextToView(fileUpload);

            return View("Encode");
        }

        [HttpPost]
        public ActionResult UploadedTextHuffman(IEnumerable<HttpPostedFileBase> fileUpload)
        {
            SetTextToView(fileUpload);

            return View("HuffmanDecode");
        }
        [HttpPost]
        public ActionResult UploadedTextRLE(IEnumerable<HttpPostedFileBase> fileUpload)
        {
            SetTextToView(fileUpload);

            return View("RLEDecode");
        }

        [HttpPost]
        public ActionResult UploadedTextBW(IEnumerable<HttpPostedFileBase> fileUpload)
        {
            SetTextToView(fileUpload);

            return View("BWDecode");
        }

        [HttpPost]
        public ActionResult UploadedTextLZ78(IEnumerable<HttpPostedFileBase> fileUpload)
        {
            SetTextToView(fileUpload);

            return View("LZ78Decode");
        }

        [HttpPost]
        public ActionResult UploadedTextArithmetic(IEnumerable<HttpPostedFileBase> fileUpload)
        {
            SetTextToView(fileUpload);

            return View("ArithmeticDecode");
        }

        [HttpPost]
        public ActionResult RLEDecode(string toDecode)
        {
            Decode(toDecode, new RLE());

            return View();
        }

        public ActionResult RLEDecode()
        {
            return View();
        }

        [HttpPost]
        public ActionResult HuffmanDecode(string toDecode)
        {
            Decode(toDecode, new HuffmanCoding());

            return View();
        }

        public ActionResult HuffmanDecode()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ArithmeticDecode(string toDecode)
        {
            Decode(toDecode, new ArithmeticCoding());

            return View();
        }

        public ActionResult ArithmeticDecode()
        {
            return View();
        }

        [HttpPost]
        public ActionResult LZ78Decode(string toDecode)
        {
            Decode(toDecode, new LZ78());

            return View();
        }

        public ActionResult LZ78Decode()
        {
            return View();
        }

        [HttpPost]
        public ActionResult BWDecode(string toDecode)
        {
            Decode(toDecode, new BWT());

            return View();
        }

        public ActionResult BWDecode()
        {
            return View();
        }

        private void Decode(string toDecode, ITextEncodingAlgorithm algorithm)
        {
            if (toDecode.Length == 0)
            {
                ViewData["ArgumentException"] = "Пустая строка, пожалуйста введите что-нибудь.";
                return;
            }
            toDecode = toDecode.Replace("\r\n", "\n");

            try
            {
                ViewData["Decoded"] = algorithm.Decode(toDecode);
            }
            catch (ArgumentException e)
            {
                ViewData["ArgumentException"] = e.Message;
            }
            catch (Exception e)
            {
                ViewData["Error"] = e.Message;
            }
        }

        [HttpPost]
        public FileResult GetFile(string result)
        {
            using (StreamWriter st = new StreamWriter(Server.MapPath("~/App_Data/" + "file.txt"), false, Encoding.Unicode))
            {
                st.Write(result);
            }
            string file_path = Server.MapPath("~/App_Data/file.txt");
            string file_type = "application/txt";
            string file_name = "file.txt";
            return File(file_path, file_type, file_name);
        }


        private static string GetText(IEnumerable<HttpPostedFileBase> fileUpload, string uploadedText)
        {
            if (fileUpload != null)
            {
                foreach (var el in fileUpload)
                    if (el != null)
                    {
                        using (StreamReader st = new StreamReader(el.InputStream, Encoding.Unicode))
                        {
                            uploadedText += st.ReadToEnd();
                        }
                    }
            }

            return uploadedText;
        }


        private void SetTextToView(IEnumerable<HttpPostedFileBase> fileUpload)
        {
            string uploadedText = string.Empty;
            uploadedText = GetText(fileUpload, uploadedText);
            ViewData["UploadedText"] = uploadedText;
        }
    }
}