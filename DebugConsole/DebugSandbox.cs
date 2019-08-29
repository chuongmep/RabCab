// -----------------------------------------------------------------------------------
//     <copyright file="DebugSandbox.cs" company="CraterSpace">
//     Copyright (c) 2019 CraterSpace - All Rights Reserved 
//     </copyright>
//     <author>Zach Ayers</author>
//     <date>04/10/2019</date>
//     Description:    
//     Notes:  
//     References:          
// -----------------------------------------------------------------------------------

using System;
using System.Windows.Forms;
using RabCab.Agents;
using RabCab.Settings;

namespace DebugConsole
{
    internal static class DebugSandbox
    {
        private static string DecryptedUserName = "RabCabError@gmail.com";

        private static string EncryptedUserName =
            "BbC/rifCgTufy2ZHzLvJ64DqAbPfMvsJ9aBOeGMVlwLDHBXVtoJyKELc6bs0Vj3xq4UjPZa7HnNTgecGE8ZxQcZzx76tVcD1my8J3TP1w1M=";

        private static string DecryptedPass = "4815162342Ravioli100%";

        private static string EncryptedPass =
            "tBPv5NsSV/Y2hBoeLfCwqaAPjyu77TDAvIW7mcK5SK7hpp+zQQNbd8+n4S5P5p6jJotZV04d+eVHRyQrRilHnvmxGldCotQ3g+OAvO1JhSU=";

        /// <summary>
        ///     Main method
        /// </summary>
        /// <param name="args"></param>
        private static void Main()
        {
            var settingsGui = new SettingsGui();
            settingsGui.Dock = DockStyle.Fill;
            var form = new Form();
            form.Controls.Add(settingsGui);
            
            Application.EnableVisualStyles();
            Application.Run(form); 
        }

        private static void GenerateAuthKey()
        {
            var key = CryptoAgent.NewKey();

            for (var i = 0; i < key.Length; i++) Console.Write(key.GetValue(i) + ",");
        }

        private static void GenerateCryptKey()
        {
            var key = CryptoAgent.NewKey();

            for (var i = 0; i < key.Length; i++) Console.Write(key.GetValue(i) + ",");
        }

        private static void RcEncrypt(string str)
        {
            Console.WriteLine(CryptoAgent.SimpleEncrypt(str));
        }

        private static void RcDecrypt(string str)
        {
            Console.WriteLine(CryptoAgent.SimpleDecrypt(str));
        }
    }
}