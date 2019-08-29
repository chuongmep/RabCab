using System;
using Autodesk.AutoCAD.EditorInput;

namespace RabCab.Agents
{
    public class KeywordAgent
    {
        private readonly Editor _acCurEd;
        public string DefValue;
        public string Key;
        public object Output;
        public string Prompt;
        public TypeCode T;

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="acCurEd"></param>
        /// <param name="key"></param>
        /// <param name="prompt"></param>
        /// <param name="retType"></param>
        /// <param name="defaultValue"></param>
        public KeywordAgent(Editor acCurEd, string key, string prompt, TypeCode retType, string defaultValue = null)
        {
            _acCurEd = acCurEd;
            Key = key;
            Prompt = prompt;
            T = retType;
            DefValue = defaultValue;
            Output = null;
        }

        /// <summary>
        ///     TODO
        /// </summary>
        public void GetOutput()
        {
            switch (T)
            {
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:

                    var prIntOpts = new PromptIntegerOptions(string.Empty)
                    {
                        Message = Prompt,
                        AllowNone = false,
                        AllowNegative = false
                    };

                    if (DefValue != null)
                        try
                        {
                            prIntOpts.DefaultValue = int.Parse(DefValue);
                            prIntOpts.UseDefaultValue = true;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            MailAgent.Report(e.Message);
                            throw;
                        }


                    //Prompt the editor to receive the double from the user
                    var prIntRes = _acCurEd.GetInteger(prIntOpts);

                    //If bad input -> return 0
                    if (prIntRes.Status != PromptStatus.OK) Output = 0;

                    //Return the distance entered into the editor
                    Output = prIntRes.Value;
                    break;

                case TypeCode.Double:
                    var prDobOpts = new PromptDoubleOptions(string.Empty)
                    {
                        Message = Prompt,
                        AllowNone = false,
                        AllowNegative = true
                    };

                    if (DefValue != null)
                        try
                        {
                            prDobOpts.DefaultValue = double.Parse(DefValue);
                            prDobOpts.UseDefaultValue = true;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            MailAgent.Report(e.Message);
                            throw;
                        }

                    //Prompt the editor to receive the double from the user
                    var prDobRes = _acCurEd.GetDouble(prDobOpts);

                    //If bad input -> return 0
                    if (prDobRes.Status != PromptStatus.OK) Output = 0;

                    //Return the double entered into the editor
                    Output = prDobRes.Value;
                    break;

                case TypeCode.String:

                    var prStrOpts = new PromptStringOptions(string.Empty)
                    {
                        Message = Prompt,
                        DefaultValue = DefValue,
                        UseDefaultValue = DefValue != string.Empty
                    };

                    //Prompt the editor to receive the string from the user
                    var prStrRes = _acCurEd.GetString(prStrOpts);

                    //If bad input -> return ""
                    if (prStrRes.Status != PromptStatus.OK) Output = string.Empty;

                    //Return the string entered into the editor
                    Output = prStrRes.StringResult;
                    break;

                case TypeCode.Boolean:
                    var bTrue = "Yes";
                    var bFalse = "No";
                    var keys = new[] {bTrue, bFalse};

                    var prKeyOpts = new PromptKeywordOptions(string.Empty)
                    {
                        Message = Prompt + " <" + DefValue + "> ",
                        AllowNone = false
                    };


                    //Append keywords to the message
                    foreach (var key in keys)
                        prKeyOpts.Keywords.Add(key);

                    var prKeyRes = _acCurEd.GetKeywords(prKeyOpts);

                    // If bad input -> return null
                    if (prKeyRes.Status != PromptStatus.OK) Output = false;

                    //Return the keyword selected in the editor
                    Output = prKeyRes.StringResult == bTrue;
                    break;
            }

            if (Output != null) DefValue = Output.ToString();
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="setVar"></param>
        public void Set<T>(ref T setVar)
        {
            if (setVar == null) return;
            if (Output == null) return;

            try
            {
                setVar = (T) Output;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                MailAgent.Report(e.Message);
                throw;
            }
        }
    }
}