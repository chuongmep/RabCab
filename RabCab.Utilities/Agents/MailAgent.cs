using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Windows;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Windows;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Image = System.Drawing.Image;
using Window = Autodesk.AutoCAD.Windows.Window;

namespace RabCab.Agents
{
    public static class MailAgent
    {
        public static void Report(string eStr)
        {
            var acCurDoc = Application.DocumentManager.MdiActiveDocument;

            if (acCurDoc == null) return;

            _ = acCurDoc.Database;
            var acCurEd = acCurDoc.Editor;

            try
            {
                var uN = CryptoAgent.SimpleDecrypt(
                    "BbC/rifCgTufy2ZHzLvJ64DqAbPfMvsJ9aBOeGMVlwLDHBXVtoJyKELc6bs0Vj3xq4UjPZa7HnNTgecGE8ZxQcZzx76tVcD1my8J3TP1w1M=");
                var ps = CryptoAgent.SimpleDecrypt(
                    "tBPv5NsSV/Y2hBoeLfCwqaAPjyu77TDAvIW7mcK5SK7hpp+zQQNbd8+n4S5P5p6jJotZV04d+eVHRyQrRilHnvmxGldCotQ3g+OAvO1JhSU=");

                var fromAddress = new MailAddress(uN, "RabCab Error Report");
                var toAddress = new MailAddress("RabCabService@gmail.com", "RabCab Support");
                var fromPassword = ps;
                var subject = "Error Report - " + Environment.UserName;

                acCurEd.Command("COPYHIST");

                Thread.Sleep(100);
                var body = Clipboard.GetText();

                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
                };
                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = "Machine Name: " + Environment.MachineName +
                           "\n User Name: " + Environment.UserName +
                           "\n Domain Name: " + Environment.UserDomainName +
                           "\n OS Version: " + Environment.OSVersion +
                           "\n Is x64: " + Environment.Is64BitOperatingSystem +
                           "\n IP Address: " + GetLocalIPAddress() +
                           "\n RabCab Directory: " + Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                           "\n\n\n ERROR" + eStr +
                           "\n\n\n COMMAND LINE ENTRIES: \n" +
                           body
                })
                {
                    CaptureMainToClipboard();
                    AddImageToEmail(message, System.Windows.Forms.Clipboard.GetImage());

                    try
                    {
                        var hs = HostApplicationServices.Current;
                        var file = hs.FindFile(acCurDoc.Name, acCurDoc.Database, FindFileHint.Default);
                        var tempPath = Path.GetTempPath() + Path.GetFileName(acCurDoc.Name);
                        File.Copy(file, tempPath, true);
                        var attachment = new Attachment(tempPath);
                        message.Attachments.Add(attachment);
                        File.Delete(tempPath);
                    }
                    catch (Exception)
                    {
                        message.Body +=
                            "\n FILE WAS NOT SAVED BY USER BEFORE ERROR OCCURED - NO ATTACHMENT FILE CREATED!";
                    }

                    smtp.Send(message);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    return ip.ToString();
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }

        private static void AddImageToEmail(MailMessage mail, Image image)
        {
            var imageStream = GetImageStream(image);

            var imageResource = new LinkedResource(imageStream, "image/png") {ContentId = "added-image-id"};
            var alternateView =
                AlternateView.CreateAlternateViewFromString(mail.Body, mail.BodyEncoding, MediaTypeNames.Text.RichText);

            alternateView.LinkedResources.Add(imageResource);
            mail.AlternateViews.Add(alternateView);
        }

        private static Stream GetImageStream(Image image)
        {
            // Conver the image to a memory stream and return.
            var imageConverter = new ImageConverter();
            var imgaBytes = (byte[]) imageConverter.ConvertTo(image, typeof(byte[]));
            var memoryStream = new MemoryStream(imgaBytes);

            return memoryStream;
        }

        public static void CaptureMainWindow()
        {
            ScreenShotToFile(
                Application.DocumentManager.MdiActiveDocument.Window,
                "c:\\main-window.png",
                0, 0, 0, 0
            );
        }

        public static void CaptureDocWindow()
        {
            ScreenShotToFile(
                Application.DocumentManager.MdiActiveDocument.Window,
                "c:\\doc-window.png",
                30, 26, 10, 10
            );
        }

        public static void CaptureMainToClipboard()
        {
            ScreenShotToClipboard(
                Application.DocumentManager.MdiActiveDocument.Window,
                0, 0, 0, 0
            );
        }

        public static void CaptureDocToClipboard()
        {
            ScreenShotToClipboard(
                Application.DocumentManager.MdiActiveDocument.Window,
                30, 26, 10, 10
            );
        }

        private static void ScreenShotToFile(
            Window wd,
            string filename,
            int top, int bottom, int left, int right
        )
        {
            var pt = wd.GetLocation();
            var sz = wd.GetSize();
            pt.X += left;
            pt.Y += top;
            sz.Height -= top + bottom;
            sz.Width -= left + right;


            // Set the bitmap object to the size of the screen
            var bmp =
                new Bitmap(
                    sz.Width,
                    sz.Height,
                    PixelFormat.Format32bppArgb
                );

            using (bmp)
            {
                // Create a graphics object from the bitmap
                using (var gfx = Graphics.FromImage(bmp))
                {
                    // Take a screenshot of our window
                    gfx.CopyFromScreen(pt.X, pt.Y, 0, 0, sz, CopyPixelOperation.SourceCopy);

                    // Save the screenshot to the specified location
                    bmp.Save(filename, ImageFormat.Png);
                }
            }
        }

        private static void ScreenShotToClipboard(
            Window wd,
            int top, int bottom, int left, int right
        )
        {
            var pt = wd.GetLocation();
            var sz = wd.GetSize();
            pt.X += left;
            pt.Y += top;
            sz.Height -= top + bottom;
            sz.Width -= left + right;


            // Set the bitmap object to the size of the screen
            var bmp = new Bitmap(sz.Width, sz.Height, PixelFormat.Format32bppArgb);

            using (bmp)
            {
                // Create a graphics object from the bitmap
                using (var gfx = Graphics.FromImage(bmp))
                {
                    // Take a screenshot of our window
                    gfx.CopyFromScreen(pt.X, pt.Y, 0, 0, sz, CopyPixelOperation.SourceCopy);
                    System.Windows.Forms.Clipboard.SetImage(bmp);
                }
            }
        }
    }
}