using System;
using Autodesk.AutoCAD.EditorInput;

namespace RabCab.Agents
{
    public class KeywordAgent
    {
        private readonly Editor _acCurEd;
        public string Key;
        public string Prompt;
        public TypeCode T;
        public string DefValue;
        public object Output;

        /// <summary>
        /// TODO
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
        /// TODO
        /// </summary>
        public void GetOutput()
        {
            switch (T)
            {
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:

                    var prIntOpts = new PromptIntegerOptions("")
                    {
                        Message = Prompt,
                        AllowNone = false,
                        AllowNegative = false
                    };

                    if (DefValue != null)
                    {
                        try
                        {
                            prIntOpts.DefaultValue = Int32.Parse(DefValue);
                            prIntOpts.UseDefaultValue = true;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            throw;
                        }
                    }


                    //Prompt the editor to receive the double from the user
                    var prIntRes = _acCurEd.GetInteger(prIntOpts);

                    //If bad input -> return 0
                    if (prIntRes.Status != PromptStatus.OK) Output = 0;

                    //Return the distance entered into the editor
                    Output = prIntRes.Value;
                    break;

                case TypeCode.Double:
                    var prDobOpts = new PromptDoubleOptions("")
                    {
                        Message = Prompt,
                        AllowNone = false,
                        AllowNegative = true
                    };

                    if (DefValue != null)
                    {
                        try
                        {
                            prDobOpts.DefaultValue = Double.Parse(DefValue);
                            prDobOpts.UseDefaultValue = true;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            throw;
                        }
                    }

                    //Prompt the editor to receive the double from the user
                    var prDobRes = _acCurEd.GetDouble(prDobOpts);

                    //If bad input -> return 0
                    if (prDobRes.Status != PromptStatus.OK) Output = 0;

                    //Return the double entered into the editor
                    Output = prDobRes.Value;
                    break;

                case TypeCode.String:

                    var prStrOpts = new PromptStringOptions("")
                    {
                        Message = Prompt,
                        DefaultValue = DefValue,
                        UseDefaultValue = DefValue != ""
                    };

                    //Prompt the editor to receive the string from the user
                    var prStrRes = _acCurEd.GetString(prStrOpts);

                    //If bad input -> return ""
                    if (prStrRes.Status != PromptStatus.OK) Output = "";

                    //Return the string entered into the editor
                    Output = prStrRes.StringResult;
                    break;

                case TypeCode.Boolean:
                    //TODO
                    break;

            }

            if (Output != null)
            {
                DefValue = Output.ToString();
            }

        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="setVar"></param>
        public void Set<T>(ref T setVar)
        {
            if (setVar == null) return;
            if (Output == null) return;

            try
            {
                setVar = (T)Output;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

    }
}