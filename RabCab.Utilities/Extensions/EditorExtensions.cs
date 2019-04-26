// -----------------------------------------------------------------------------------
//     <copyright file="EditorExtensions.cs" company="CraterSpace">
//     Copyright (c) 2019 CraterSpace - All Rights Reserved 
//     </copyright>
//     <author>Zach Ayers</author>
//     <date>03/08/2019</date>
//     Description:    
//     Notes:  
//     References:          
// -----------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using RabCab.Agents;
using RabCab.Calculators;
using RabCab.Engine.Enumerators;
using RabCab.Engine.System;
using AcRx = Autodesk.AutoCAD.Runtime;

// ReSharper disable CompareOfFloatsByEqualityOperator

namespace RabCab.Extensions
{
    internal static class EditorExtensions
    {
        #region  Prompt Nested Entity Options

        /// <summary>
        ///     Method to get a nested entity selection from the user.
        /// </summary>
        /// <param name="acCurEd">The current working editor.</param>
        /// <param name="prompt">The prompt to present to the user.</param>
        /// <returns>Returns the ObjectID value of the selected nested entity.</returns>
        public static ObjectId GetNestedEntity(this Editor acCurEd, string prompt)
        {
            var prNestOpts = new PromptNestedEntityOptions("")
            {
                AllowNone = false,
                Message = prompt
            };

            //Prompt the user to select a nested entity
            var prNestRes = acCurEd.GetNestedEntity(prNestOpts);

            //If bad input -> return null
            if (prNestRes.Status != PromptStatus.OK) return ObjectId.Null;

            //Return the entity objectId selected in the editor
            var objId = prNestRes.ObjectId;
            return objId;
        }

        #endregion

        #region Prompt String Options

        /// <summary>
        ///     Gets any string input.
        /// </summary>
        /// <param name="acCurEd">The current working Editor.</param>
        /// <param name="prompt">The prompt to present to the user.</param>
        /// <param name="defaultValue">
        ///     The default value to use in prompt -> pressing enter will automatically use the default
        ///     string.
        /// </param>
        /// <returns>Returns a string from the editor.</returns>
        public static string GetString(this Editor acCurEd, string prompt, string defaultValue = "")
        {
            var prStrOpts = new PromptStringOptions("")
            {
                Message = prompt,
                DefaultValue = defaultValue,
                UseDefaultValue = defaultValue != ""
            };


            //Prompt the editor to receive the string from the user
            var prStrRes = acCurEd.GetString(prStrOpts);

            //If bad input -> return ""
            if (prStrRes.Status != PromptStatus.OK) return "";

            //Return the string entered into the editor
            var strResult = prStrRes.StringResult;
            return strResult;
        }

        #endregion

        #region Point Parsing

        /// <summary>
        ///     Method to return a displacement vector - transformed by the current UCS
        /// </summary>
        /// <param name="point1">Point3d to transform from</param>
        /// <param name="point2">Point3d to transform to</param>
        /// <param name="acCurEd">The Current Working Editor</param>
        /// <returns></returns>
        public static Vector3d GetTransformedVector(this Editor acCurEd, Point3d point1, Point3d point2)
        {
            //Get the vector from point1 to point2
            var acVec3D = point1.GetVectorTo(point2);

            //Transform the vector by the current UCS and return it
            return acVec3D.TransformBy(acCurEd.CurrentUserCoordinateSystem);
        }

        #endregion

        #region Methods To Read/Add Data From A Selected DWG File

        /// <summary>
        ///     Method Returns The Database of a Selected DWG
        /// </summary>
        /// <param name="acCurEd">The Current Working Editor</param>
        /// <returns>Returns an External aCAD Database</returns>
        public static Database GetExternalDatabase(Editor acCurEd)
        {
            // Create Database Object
            var importDb = new Database(false, true);

            //Prompt User To Select A File
            var fileOpts = new PromptOpenFileOptions("Select file to import: ")
            {
                Filter = "Drawing (*.dwg)|*.dwg|" +
                         "Design Interchange Format (*.dxf)|*.dxf|" +
                         "Drawing Template (*.dwt)|*.dwt|" +
                         "Drawing Standards (*.dws)|*.dws"
            };


            var fileRes = acCurEd.GetFileNameForOpen(fileOpts);

            if (fileRes.Status == PromptStatus.OK)
            {
                acCurEd.WriteMessage("\nParsing File: \"{0}\".", fileRes.StringResult);

                //Read the import DWG file
                importDb.ReadDwgFile(fileRes.StringResult, FileShare.Read, true, "");
            }

            return importDb;
        }

        #endregion

        #region Pick First Selection

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="acCurEd"></param>
        /// <param name="selSet"></param>
        /// <returns></returns>
        public static bool CheckForPickFirst(this Editor acCurEd, out SelectionSet selSet)
        {
            // Get the PickFirst selection set
            var acSsPrompt = acCurEd.SelectImplied();

            // If the prompt status is OK, objects were selected before
            // the command was started
            if (acSsPrompt.Status == PromptStatus.OK)
            {
                selSet = acSsPrompt.Value;
                return true;
            }

            // Clear the PickFirst selection set
            var idarrayEmpty = new ObjectId[0];
            acCurEd.SetImpliedSelection(idarrayEmpty);
            selSet = null;
            return false;
        }

        #endregion


        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="keyList"></param>
        /// <param name="prOpts"></param>
        private static void HandleKeywords(List<KeywordAgent> keyList, PromptSelectionOptions prOpts)
        {
            if (keyList == null) return;

            foreach (var key in keyList) prOpts.Keywords.Add(key.Key);

            var keyRes = prOpts.Keywords.GetDisplayString(true);


            prOpts.MessageForAdding = prOpts.MessageForAdding + " or " + keyRes;

            prOpts.MessageForRemoval = prOpts.MessageForRemoval + " or " + keyRes;

            // Implement a callback for when keywords are entered
            prOpts.KeywordInput += delegate(object sender, SelectionTextInputEventArgs e)
            {
                var userInput = e.Input;

                foreach (var key in keyList)
                    if (userInput == key.Key)
                        key.GetOutput();
            };
        }

        #region Prompt Angle Options

        /// <summary>
        ///     Method to get an angle from the user in radians
        /// </summary>
        /// <param name="acCurEd">The current working editor</param>
        /// <param name="prompt">The string to be prompted to the user</param>
        /// <param name="defaultValue">
        ///     The default value to use in prompt -> pressing enter will automatically use the default
        ///     distance. Default value is input in Degrees
        /// </param>
        /// <returns>Returns the value input by the user as a radian</returns>
        public static double GetRadian(this Editor acCurEd, string prompt, double defaultValue = 0)
        {
            //If default value, convert it from degrees to radians

            if (defaultValue != 0)
                defaultValue = CalcUnit.ConvertToRadians(defaultValue);

            //Prompt user to enter an angle in autoCAD
            var prAngOpts = new PromptAngleOptions(string.Empty)
            {
                Message = prompt,
                AllowNone = false,
                AllowZero = false,
                DefaultValue = defaultValue,
                UseDefaultValue = defaultValue != 0
            };

            //Prompt the editor to receive the angle from the user
            var prAngRes = acCurEd.GetAngle(prAngOpts);

            //If bad input -> return 0
            if (prAngRes.Status != PromptStatus.OK) return 0;

            //Return the angle entered into the editor
            var doubleResult = prAngRes.Value;
            return doubleResult;
        }

        /// <summary>
        ///     Method to get an angle from the user and return it in degrees
        /// </summary>
        /// <param name="acCurEd">The current working editor</param>
        /// <param name="prompt">The prompt to present to the user</param>
        /// <param name="defaultValue">
        ///     The default value to use in prompt -> pressing enter will automatically use the default
        ///     distance. Default value is input in degrees
        /// </param>
        /// <returns>Returns the value input by the user as a degree</returns>
        public static double GetDegree(this Editor acCurEd, string prompt, double defaultValue = 0)
        {
            //If default value, convert it from degrees to radians
            if (defaultValue != 0)
                defaultValue = CalcUnit.ConvertToRadians(defaultValue);

            //Prompt user to enter an angle in autoCAD
            var prAngOpts = new PromptAngleOptions(string.Empty)
            {
                Message = prompt,
                AllowNone = false,
                AllowZero = false,
                DefaultValue = defaultValue,
                UseDefaultValue = defaultValue != 0
            };

            //Prompt the editor to receive the angle from the user
            var prAngRes = acCurEd.GetAngle(prAngOpts);

            //If bad input -> return 0
            if (prAngRes.Status != PromptStatus.OK) return 0;

            //Return the angle entered into the editor
            var doubleResult = prAngRes.Value;
            return CalcUnit.ConvertToDegrees(doubleResult);
        }

        #endregion

        //TODO Prompt Corner 

        #region Prompt Bool Options

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="acCurEd"></param>
        /// <param name="prompt"></param>
        /// <returns></returns>
        public static bool? GetBool(this Editor acCurEd, string prompt, string t = null, string f = null)
        {           
            var bTrue = t ?? "Yes";
            var bFalse = f ?? "No";

            if (f != null)
                bFalse = f;

            var keys = new string[] { bTrue, bFalse };
            var key = acCurEd.GetSimpleKeyword(prompt, keys);

            if (string.IsNullOrEmpty(key))
            {
                return null;
            }

            return key == bTrue;
        }

        #endregion

        #region Prompt Distance Options

        /// <summary>
        ///     Gets any 3D distance in CAD, allows positive and negative values.
        /// </summary>
        /// <param name="acCurEd">The current working Editor.</param>
        /// <param name="prompt">The prompt to present to the user.</param>
        /// <param name="defaultValue">
        ///     The default value to use in prompt -> pressing enter will automatically use the default
        ///     distance.
        /// </param>
        /// <returns>Returns a distance from the editor in decimal format.</returns>
        public static double GetAnyDistance(this Editor acCurEd, string prompt, double defaultValue = 0)
        {
            //Prompt user to enter a distance in autoCAD
            var prDistOpts = new PromptDistanceOptions(string.Empty)
            {
                Message = prompt,
                AllowNone = false,
                AllowZero = true,
                DefaultValue = defaultValue,
                UseDefaultValue = defaultValue != 0
            };

            //Prompt the editor to receive the distance from the user
            var prDistRes = acCurEd.GetDistance(prDistOpts);

            //If bad input -> return 0
            if (prDistRes.Status != PromptStatus.OK) return 0;

            //Return the distance entered into the editor
            var distResult = prDistRes.Value;
            return distResult;
        }

        /// <summary>
        ///     Gets any 3D distance in CAD, uses a base point and allows positive and negative values.
        /// </summary>
        /// <param name="acCurEd">The current working Editor.</param>
        /// <param name="basePt">The point to start the distance from.</param>
        /// <param name="prompt">The prompt to present to the user.</param>
        /// <param name="defaultValue">
        ///     The default value to use in prompt -> pressing enter will automatically use the default
        ///     distance.
        /// </param>
        /// <returns>Returns a distance from the editor in decimal format.</returns>
        public static double GetAnyDistance(this Editor acCurEd, Point3d basePt, string prompt, double defaultValue = 0)
        {
            //Prompt user to enter a distance in autoCAD
            var prDistOpts = new PromptDistanceOptions(string.Empty)
            {
                Message = prompt,
                AllowNone = false,
                AllowZero = true,
                BasePoint = basePt,
                UseBasePoint = true,
                DefaultValue = defaultValue,
                UseDefaultValue = defaultValue != 0
            };

            //Prompt the editor to receive the distance from the user
            var prDistRes = acCurEd.GetDistance(prDistOpts);

            //If bad input -> return 0
            if (prDistRes.Status != PromptStatus.OK) return 0;

            //Return the distance entered into the editor
            var distResult = prDistRes.Value;
            return distResult;
        }

        /// <summary>
        ///     Gets any 3D distance in CAD, allows positive values.
        /// </summary>
        /// <param name="acCurEd">The current working Editor.</param>
        /// <param name="prompt">The prompt to present to the user.</param>
        /// <param name="defaultValue">
        ///     The default value to use in prompt -> pressing enter will automatically use the default
        ///     distance.
        /// </param>
        /// <returns>Returns a distance from the editor in decimal format.</returns>
        public static double GetPositiveDistance(this Editor acCurEd, string prompt, double defaultValue = 0)
        {
            //Prompt user to enter a distance in autoCAD
            var prDistOpts = new PromptDistanceOptions(string.Empty)
            {
                Message = prompt,
                AllowNone = false,
                AllowZero = true,
                AllowNegative = false,
                DefaultValue = defaultValue,
                UseDefaultValue = defaultValue != 0
            };

            //Prompt the editor to receive the distance from the user
            var prDistRes = acCurEd.GetDistance(prDistOpts);

            //If bad input -> return 0
            if (prDistRes.Status != PromptStatus.OK) return 0;

            //Return the distance entered into the editor
            var distResult = prDistRes.Value;
            return distResult;
        }

        /// <summary>
        ///     Gets any 3D distance in CAD, uses a base point and allows positive values.
        /// </summary>
        /// <param name="acCurEd">The current working Editor.</param>
        /// <param name="basePt">The point to start the distance from.</param>
        /// <param name="prompt">The prompt to present to the user.</param>
        /// <param name="defaultValue">
        ///     The default value to use in prompt -> pressing enter will automatically use the default
        ///     distance.
        /// </param>
        /// <returns>Returns a distance from the editor in decimal format.</returns>
        public static double GetPositiveDistance(this Editor acCurEd, Point3d basePt, string prompt,
            double defaultValue = 0)
        {
            //Prompt user to enter a distance in autoCAD
            var prDistOpts = new PromptDistanceOptions(string.Empty)
            {
                Message = prompt,
                AllowNone = false,
                AllowZero = true,
                AllowNegative = false,
                BasePoint = basePt,
                UseBasePoint = true,
                DefaultValue = defaultValue,
                UseDefaultValue = defaultValue != 0
            };

            //Prompt the editor to receive the distance from the user
            var prDistRes = acCurEd.GetDistance(prDistOpts);

            //If bad input -> return 0
            if (prDistRes.Status != PromptStatus.OK) return 0;

            //Return the distance entered into the editor
            var distResult = prDistRes.Value;
            return distResult;
        }

        /// <summary>
        ///     Gets any 2D distance in CAD, allows positive and negative values.
        /// </summary>
        /// <param name="acCurEd">The current working Editor.</param>
        /// <param name="prompt">The prompt to present to the user.</param>
        /// ///
        /// <param name="defaultValue">
        ///     The default value to use in prompt -> pressing enter will automatically use the default
        ///     distance.
        /// </param>
        /// <returns>SReturns a distance from the editor in decimal format.</returns>
        public static double GetAny2DDistance(this Editor acCurEd, string prompt, double defaultValue = 0)
        {
            //Prompt user to enter a distance in autoCAD
            var prDistOpts = new PromptDistanceOptions(string.Empty)
            {
                Message = prompt,
                AllowNone = false,
                AllowZero = true,
                Only2d = true,
                DefaultValue = defaultValue,
                UseDefaultValue = defaultValue != 0
            };

            //Prompt the editor to receive the distance from the user
            var prDistRes = acCurEd.GetDistance(prDistOpts);

            //If bad input -> return 0
            if (prDistRes.Status != PromptStatus.OK) return 0;

            //Return the distance entered into the editor
            var distResult = prDistRes.Value;
            return distResult;
        }

        /// <summary>
        ///     Gets any 2D distance in CAD, uses a base point and allows positive and negative values.
        /// </summary>
        /// <param name="acCurEd">The current working Editor.</param>
        /// <param name="basePt">The point to start the distance from.</param>
        /// <param name="prompt">The prompt to present to the user.</param>
        /// <param name="defaultValue">
        ///     The default value to use in prompt -> pressing enter will automatically use the default
        ///     distance.
        /// </param>
        /// <returns>Returns a distance from the editor in decimal format.</returns>
        public static double GetAny2DDistance(this Editor acCurEd, Point3d basePt, string prompt,
            double defaultValue = 0)
        {
            //Prompt user to enter a distance in autoCAD
            var prDistOpts = new PromptDistanceOptions(string.Empty)
            {
                Message = prompt,
                AllowNone = false,
                AllowZero = true,
                Only2d = true,
                BasePoint = basePt,
                UseBasePoint = true,
                DefaultValue = defaultValue,
                UseDefaultValue = defaultValue != 0
            };

            //Prompt the editor to receive the distance from the user
            var prDistRes = acCurEd.GetDistance(prDistOpts);

            //If bad input -> return 0
            if (prDistRes.Status != PromptStatus.OK) return 0;

            //Return the distance entered into the editor
            var distResult = prDistRes.Value;
            return distResult;
        }

        /// <summary>
        ///     Gets any 2D distance in CAD, allows positive values.
        /// </summary>
        /// <param name="acCurEd">The current working Editor.</param>
        /// <param name="prompt">The prompt to present to the user.</param>
        /// <param name="defaultValue">
        ///     The default value to use in prompt -> pressing enter will automatically use the default
        ///     distance.
        /// </param>
        /// <returns>Returns a distance from the editor in decimal format.</returns>
        public static double GetPositive2DDistance(this Editor acCurEd, string prompt, double defaultValue = 0)
        {
            //Prompt user to enter a distance in autoCAD
            var prDistOpts = new PromptDistanceOptions(string.Empty)
            {
                Message = prompt,
                AllowNone = false,
                AllowZero = true,
                Only2d = true,
                AllowNegative = false,
                DefaultValue = defaultValue,
                UseDefaultValue = defaultValue != 0
            };

            //Prompt the editor to receive the distance from the user
            var prDistRes = acCurEd.GetDistance(prDistOpts);

            //If bad input -> return 0
            if (prDistRes.Status != PromptStatus.OK) return 0;

            //Return the distance entered into the editor
            var distResult = prDistRes.Value;
            return distResult;
        }

        /// <summary>
        ///     Gets any 2D distance in CAD, allows positive values.
        /// </summary>
        /// <param name="acCurEd">The current working Editor.</param>
        /// <param name="basePt">The point to start the distance from.</param>
        /// <param name="prompt">The prompt to present to the user.</param>
        /// <param name="defaultValue">
        ///     The default value to use in prompt -> pressing enter will automatically use the default
        ///     distance.
        /// </param>
        /// <returns>Returns a distance from the editor in decimal format.</returns>
        public static double GetPositive2DDistance(this Editor acCurEd, Point3d basePt, string prompt,
            double defaultValue = 0)
        {
            //Prompt user to enter a distance in autoCAD
            var prDistOpts = new PromptDistanceOptions(string.Empty)
            {
                Message = prompt,
                AllowNone = false,
                AllowZero = true,
                Only2d = true,
                AllowNegative = false,
                BasePoint = basePt,
                UseBasePoint = true,
                DefaultValue = defaultValue,
                UseDefaultValue = defaultValue != 0
            };

            //Prompt the editor to receive the distance from the user
            var prDistRes = acCurEd.GetDistance(prDistOpts);

            //If bad input -> return 0
            if (prDistRes.Status != PromptStatus.OK) return 0;

            //Return the distance entered into the editor
            var distResult = prDistRes.Value;
            return distResult;
        }

        #endregion

        #region Prompt Double Options

        /// <summary>
        ///     Gets any double input,  Allows positive and negative values.
        /// </summary>
        /// <param name="acCurEd">The current working Editor.</param>
        /// <param name="prompt">The prompt to present to the user.</param>
        /// <param name="defaultValue">
        ///     The default value to use in prompt -> pressing enter will automatically use the default
        ///     distance.
        /// </param>
        /// <returns>Returns a double from the editor in decimal format.</returns>
        public static double GetDouble(this Editor acCurEd, string prompt, double defaultValue = 0)
        {
            var prDobOpts = new PromptDoubleOptions(prompt)
            {
                Message = prompt,
                AllowNone = false,
                AllowNegative = true,
                DefaultValue = defaultValue,
                UseDefaultValue = defaultValue != 0
            };

            //Prompt the editor to receive the double from the user
            var prDobRes = acCurEd.GetDouble(prDobOpts);

            //If bad input -> return 0
            if (prDobRes.Status != PromptStatus.OK) return 0;

            //Return the double entered into the editor
            var dobResult = prDobRes.Value;
            return dobResult;
        }

        /// <summary>
        ///     Gets any double input, Allows positive values.
        /// </summary>
        /// <param name="acCurEd">The current working Editor.</param>
        /// <param name="prompt">The prompt to present to the user.</param>
        /// <param name="defaultValue">
        ///     The default value to use in prompt -> pressing enter will automatically use the default
        ///     distance.
        /// </param>
        /// <returns>Returns a double from the editor in decimal format.</returns>
        public static double GetPositiveDouble(this Editor acCurEd, string prompt, double defaultValue = 0)
        {
            var prDobOpts = new PromptDoubleOptions(prompt)
            {
                Message = prompt,
                AllowNone = false,
                AllowNegative = false,
                DefaultValue = defaultValue,
                UseDefaultValue = defaultValue != 0
            };

            //Prompt the editor to receive the double from the user
            var prDobRes = acCurEd.GetDouble(prDobOpts);

            //If bad input -> return 0
            if (prDobRes.Status != PromptStatus.OK) return 0;

            //Return the double entered into the editor
            var dobResult = prDobRes.Value;
            return dobResult;
        }

        #endregion

        //TODO Prompt Drag Options

        #region Prompt Entity Options

        /// <summary>
        ///     Method to prompt user to select an entity and return its ObjectId for use in other methods.
        /// </summary>
        /// <param name="acCurEd">The current working editor.</param>
        /// <param name="prompt">The prompt to be presented to the user.</param>
        /// <param name="rejectMessage">The message presented to the user if the selected item is rejected.</param>
        /// <returns>Object ID of the selected entity</returns>
        public static ObjectId GetEntityId(this Editor acCurEd, string prompt, string rejectMessage = "")
        {
            //Create a variable to hold the object ID
            var entObjId = ObjectId.Null;

            //Prompt user to select entities in autoCAD
            var prEntOpts = new PromptEntityOptions(prompt)
            {
                AllowNone = false,
                AllowObjectOnLockedLayer = false
            };

            //Set the reject message
            prEntOpts.SetRejectMessage(rejectMessage);

            //Prompt the editor to receive the entity selected by the user
            var prEntRes = acCurEd.GetEntity(prEntOpts);

            //If bad input -> return Null
            if (prEntRes.Status != PromptStatus.OK) return entObjId;

            //Return the selected entities object ID
            return prEntRes.ObjectId;
        }

        /// <summary>
        ///     Method to prompt user to select an entity and return its ObjectId for use in other methods.
        /// </summary>
        /// <param name="acCurEd">The current working editor.</param>
        /// <param name="type">The only type of entity the editor is allowed to select.</param>
        /// <param name="prompt">The prompt to be presented to the user.</param>
        /// <param name="rejectMessage">The message presented to the user if the selected item is rejected.</param>
        /// <returns>Object ID of the selected entity</returns>
        public static ObjectId GetEntityId(this Editor acCurEd, Type type, string prompt,
            string rejectMessage = "")
        {
            //Create a variable to hold the object ID
            var entObjId = ObjectId.Null;

            //Prompt user to select entities in autoCAD
            var prEntOpts = new PromptEntityOptions(prompt)
            {
                AllowNone = false,
                AllowObjectOnLockedLayer = false
            };

            //Set the reject message
            prEntOpts.SetRejectMessage(rejectMessage);

            //Add the allowed class as the only selectable type
            prEntOpts.AddAllowedClass(type, true);

            //Prompt the editor to receive the entity selected by the user
            var prEntRes = acCurEd.GetEntity(prEntOpts);

            //If bad input -> return Null
            if (prEntRes.Status != PromptStatus.OK) return entObjId;

            //Return the selected entities object ID
            return prEntRes.ObjectId;
        }

        /// <summary>
        ///     Method to prompt user to select an entity and return its ObjectId for use in other methods.
        /// </summary>
        /// <param name="acCurEd">The current working editor.</param>
        /// <param name="types">Array of allowable types that can be selected by the editor</param>
        /// <param name="prompt">The prompt to be presented to the user.</param>
        /// <param name="rejectMessage">The message presented to the user if the selected item is rejected.</param>
        /// <returns>Object ID of the selected entity</returns>
        public static ObjectId GetEntityId(this Editor acCurEd, Type[] types, string prompt,
            string rejectMessage = "")
        {
            //Create a variable to hold the object ID
            var entObjId = ObjectId.Null;

            //Prompt user to select entities in autoCAD
            var prEntOpts = new PromptEntityOptions(prompt)
            {
                AllowNone = false,
                AllowObjectOnLockedLayer = false
            };

            //Set the reject message
            prEntOpts.SetRejectMessage(rejectMessage);

            //Add the allowed classes as the only selectable types
            foreach (var type in types)
                prEntOpts.AddAllowedClass(type, true);

            //Prompt the editor to receive the entity selected by the user
            var prEntRes = acCurEd.GetEntity(prEntOpts);

            //If bad input -> return Null
            if (prEntRes.Status != PromptStatus.OK) return entObjId;

            //Return the selected entities object ID
            return prEntRes.ObjectId;
        }

        #endregion

        #region Prompt Integer Options

        /// <summary>
        ///     Gets any integer input, Allows positive and negative values.
        /// </summary>
        /// <param name="acCurEd">The current working Editor.</param>
        /// <param name="prompt">The prompt to present to the user.</param>
        /// <param name="defaultValue">
        ///     The default value to use in prompt -> pressing enter will automatically use the default
        ///     distance.
        /// </param>
        /// <returns>Returns an integer from the editor.</returns>
        public static int GetAnyInteger(this Editor acCurEd, string prompt, int defaultValue = 0)
        {
            var prIntOpts = new PromptIntegerOptions(prompt)
            {
                Message = prompt,
                AllowNone = false,
                AllowNegative = true,
                DefaultValue = defaultValue,
                UseDefaultValue = defaultValue != 0
            };


            //Prompt the editor to receive the double from the user
            var prIntRes = acCurEd.GetInteger(prIntOpts);

            //If bad input -> return 0
            if (prIntRes.Status != PromptStatus.OK) return 0;

            //Return the distance entered into the editor
            var intResult = prIntRes.Value;
            return intResult;
        }

        /// <summary>
        ///     Gets any integer input, Allows positive values.
        /// </summary>
        /// <param name="acCurEd">The current working Editor.</param>
        /// <param name="prompt">The prompt to present to the user.</param>
        /// <param name="defaultValue">
        ///     The default value to use in prompt -> pressing enter will automatically use the default
        ///     distance.
        /// </param>
        /// <returns>Returns an integer from the editor.</returns>
        public static int GetPositiveInteger(this Editor acCurEd, string prompt, int defaultValue = 0)
        {
            var prIntOpts = new PromptIntegerOptions(prompt)
            {
                Message = prompt,
                AllowNone = false,
                AllowNegative = false,
                DefaultValue = defaultValue,
                UseDefaultValue = defaultValue != 0
            };


            //Prompt the editor to receive the double from the user
            var prIntRes = acCurEd.GetInteger(prIntOpts);

            //If bad input -> return 0
            if (prIntRes.Status != PromptStatus.OK) return 0;

            //Return the distance entered into the editor
            var intResult = prIntRes.Value;
            return intResult;
        }

        /// <summary>
        ///     Gets any integer input, Allows any value between provided limits.
        /// </summary>
        /// <param name="acCurEd">The current working Editor.</param>
        /// <param name="prompt">The prompt to present to the user.</param>
        /// <param name="min">The minimum limit allowed to be entered by the user.</param>
        /// <param name="max">The maximum limit allowed to be entered by the user.</param>
        /// <param name="defaultValue">
        ///     The default value to use in prompt -> pressing enter will automatically use the default
        ///     distance.
        /// </param>
        /// <returns>Returns an integer from the editor.</returns>
        public static int GetLimitedInteger(this Editor acCurEd, string prompt, int min, int max, int defaultValue = 0)
        {
            var prIntOpts = new PromptIntegerOptions(prompt)
            {
                Message = prompt,
                AllowNone = false,
                AllowNegative = true,
                LowerLimit = min,
                UpperLimit = max,
                DefaultValue = defaultValue,
                UseDefaultValue = defaultValue != 0
            };


            //Prompt the editor to receive the double from the user
            var prIntRes = acCurEd.GetInteger(prIntOpts);

            //If bad input -> return 0
            if (prIntRes.Status != PromptStatus.OK) return 0;

            //Return the distance entered into the editor
            var intResult = prIntRes.Value;
            return intResult;
        }

        #endregion

        #region Prompt Keyword Options

        /// <summary>
        ///     Method to get a keyword selection from the user - supports single word input (non-arbritrary)
        /// </summary>
        /// <param name="acCurEd">The current working editor.</param>
        /// <param name="prompt">The prompt to present to the user</param>
        /// <param name="keywords">Array of keywords to present to the user to select from.</param>
        /// <returns>Returns a string result of the keyword the user has selected -> if error occurs, returns null</returns>
        public static string GetSimpleKeyword(this Editor acCurEd, string prompt, string[] keywords)
        {
            var prKeyOpts = new PromptKeywordOptions("")
            {
                Message = prompt,
                AllowNone = false
            };

            //Append keywords to the message
            foreach (var key in keywords)
                prKeyOpts.Keywords.Add(key);

            var prKeyRes = acCurEd.GetKeywords(prKeyOpts);

            // If bad input -> return null
            if (prKeyRes.Status != PromptStatus.OK) return null;

            //Return the keyword selected in the editor
            return prKeyRes.StringResult;
        }

        /// <summary>
        ///     Method to get a keyword selection from the user - supports any keyword input.
        /// </summary>
        /// <param name="acCurEd">The current working editor.</param>
        /// <param name="prompt">The prompt to present to the user</param>
        /// <param name="keywords">Array of keywords to present to the user to select from.</param>
        /// <returns>Returns a string result of the keyword the user has selected -> if error occurs, returns null</returns>
        public static string GetComplexKeyword(this Editor acCurEd, string prompt, string[] keywords)
        {
            var prKeyOpts = new PromptKeywordOptions("")
            {
                Message = prompt,
                AllowNone = false,
                AllowArbitraryInput = true
            };

            //Create an iterator to append to the beginning of each complex keyword
            var iterator = 'A';

            //Create a dictionary to hold each iterator and the partner keyword
            var keyDict = new Dictionary<string, string>();

            //Append keywords to the message
            foreach (var key in keywords)
            {
                keyDict.Add(key, iterator.ToString());
                prKeyOpts.Keywords.Add(iterator.ToString(), iterator.ToString(),
                    iterator + ": " + key.ToLower());
                iterator++;
            }

            var prKeyRes = acCurEd.GetKeywords(prKeyOpts);

            // If bad input -> return null
            if (prKeyRes.Status != PromptStatus.OK) return null;

            var returnIterator = prKeyRes.StringResult;
            var selectedKeyword = "";

            //Loop back through the keyword dictionary and find the matching iterator - once found, exit the for loop.
            foreach (var entry in keyDict)
            {
                if (entry.Value != returnIterator) continue;

                selectedKeyword = entry.Key;
                break;
            }

            //Return the keyword selected in the editor
            return selectedKeyword;
        }

        #endregion

        #region Prompt Open File Options

        /// <summary>
        ///     Get any file of the file type DWG, DXF, DWT, or DWS from the user.
        /// </summary>
        /// <param name="acCurEd">The current working editor.</param>
        /// <param name="prompt">The prompt to present to the user.</param>
        /// <param name="initialDir">
        ///     The initial directory to open the file dialog at (no value = file will open at default
        ///     location)
        /// </param>
        /// <param name="initialFile">
        ///     The initial filename to open the file dialog at (no value = file will open at default
        ///     location)
        /// </param>
        /// <returns>Returns the file path of the selected file.</returns>
        public static string GetCadFile(this Editor acCurEd, string prompt, string initialDir = "",
            string initialFile = "")
        {
            //Prompt User To Select A File
            var fileOpts = new PromptOpenFileOptions("")
            {
                Message = prompt,
                AllowUrls = false,
                DialogCaption = "Select an AutoCAD file type",
                DialogName = "CAD File Selection",
                Filter = "Drawing (*.dwg)|*.dwg|" +
                         "Design Interchange Format (*.dxf)|*.dxf|" +
                         "Drawing Template (*.dwt)|*.dwt|" +
                         "Drawing Standards (*.dws)|*.dws"
            };

            //If an initial directory is specified -> set it
            if (initialDir != "") fileOpts.InitialDirectory = initialDir;

            //If an initial filename is specified -> set it
            if (initialFile != "") fileOpts.InitialFileName = initialFile;

            //Get the selected file for open
            var fileRes = acCurEd.GetFileNameForOpen(fileOpts);

            //If file is not available for open (or bad input) -> return empty string
            if (fileRes.Status != PromptStatus.OK) return "";

            //Return the selected filename & path
            return fileRes.StringResult;
        }

        /// <summary>
        ///     Get any file of the file type DWG from the user.
        /// </summary>
        /// <param name="acCurEd">The current working editor.</param>
        /// <param name="prompt">The prompt to present to the user.</param>
        /// <param name="initialDir">
        ///     The initial directory to open the file dialog at (no value = file will open at default
        ///     location)
        /// </param>
        /// <param name="initialFile">
        ///     The initial filename to open the file dialog at (no value = file will open at default
        ///     location)
        /// </param>
        /// <returns>Returns the file path of the selected file.</returns>
        public static string GetDwgFile(this Editor acCurEd, string prompt, string initialDir = "",
            string initialFile = "")
        {
            //Prompt User To Select A File
            var fileOpts = new PromptOpenFileOptions("")
            {
                Message = prompt,
                AllowUrls = false,
                DialogCaption = "Select an AutoCAD DWG file type",
                DialogName = "DWG File Selection",
                Filter = "Drawing (*.dwg)|*.dwg|"
            };

            //If an initial directory is specified -> set it
            if (initialDir != "") fileOpts.InitialDirectory = initialDir;

            //If an initial filename is specified -> set it
            if (initialFile != "") fileOpts.InitialFileName = initialFile;

            //Get the selected file for open
            var fileRes = acCurEd.GetFileNameForOpen(fileOpts);

            //If file is not available for open (or bad input) -> return empty string
            if (fileRes.Status != PromptStatus.OK) return "";

            //Return the selected filename & path
            return fileRes.StringResult;
        }

        /// <summary>
        ///     Get any file of the file type DXF from the user.
        /// </summary>
        /// <param name="acCurEd">The current working editor.</param>
        /// <param name="prompt">The prompt to present to the user.</param>
        /// <param name="initialDir">
        ///     The initial directory to open the file dialog at (no value = file will open at default
        ///     location)
        /// </param>
        /// <param name="initialFile">
        ///     The initial filename to open the file dialog at (no value = file will open at default
        ///     location)
        /// </param>
        /// <returns>Returns the file path of the selected file.</returns>
        public static string GetDxfFile(this Editor acCurEd, string prompt, string initialDir = "",
            string initialFile = "")
        {
            //Prompt User To Select A File
            var fileOpts = new PromptOpenFileOptions("")
            {
                Message = prompt,
                AllowUrls = false,
                DialogCaption = "Select an AutoCAD DXF file type",
                DialogName = "DXF File Selection",
                Filter = "Design Interchange Format (*.dxf)|*.dxf|"
            };

            //If an initial directory is specified -> set it
            if (initialDir != "") fileOpts.InitialDirectory = initialDir;

            //If an initial filename is specified -> set it
            if (initialFile != "") fileOpts.InitialFileName = initialFile;

            //Get the selected file for open
            var fileRes = acCurEd.GetFileNameForOpen(fileOpts);

            //If file is not available for open (or bad input) -> return empty string
            if (fileRes.Status != PromptStatus.OK) return "";

            //Return the selected filename & path
            return fileRes.StringResult;
        }

        /// <summary>
        ///     Get any file of the file type DWT from the user.
        /// </summary>
        /// <param name="acCurEd">The current working editor.</param>
        /// <param name="prompt">The prompt to present to the user.</param>
        /// <param name="initialDir">
        ///     The initial directory to open the file dialog at (no value = file will open at default
        ///     location)
        /// </param>
        /// <param name="initialFile">
        ///     The initial filename to open the file dialog at (no value = file will open at default
        ///     location)
        /// </param>
        /// <returns>Returns the file path of the selected file.</returns>
        public static string GetDwtFile(this Editor acCurEd, string prompt, string initialDir = "",
            string initialFile = "")
        {
            //Prompt User To Select A File
            var fileOpts = new PromptOpenFileOptions("")
            {
                Message = prompt,
                AllowUrls = false,
                DialogCaption = "Select an AutoCAD DWT file type",
                DialogName = "DWT File Selection",
                Filter = "Drawing Template (*.dwt)|*.dwt|"
            };

            //If an initial directory is specified -> set it
            if (initialDir != "") fileOpts.InitialDirectory = initialDir;

            //If an initial filename is specified -> set it
            if (initialFile != "") fileOpts.InitialFileName = initialFile;

            //Get the selected file for open
            var fileRes = acCurEd.GetFileNameForOpen(fileOpts);

            //If file is not available for open (or bad input) -> return empty string
            if (fileRes.Status != PromptStatus.OK) return "";

            //Return the selected filename & path
            return fileRes.StringResult;
        }

        /// <summary>
        ///     Get any file of the file type DWS from the user.
        /// </summary>
        /// <param name="acCurEd">The current working editor.</param>
        /// <param name="prompt">The prompt to present to the user.</param>
        /// <param name="initialDir">
        ///     The initial directory to open the file dialog at (no value = file will open at default
        ///     location)
        /// </param>
        /// <param name="initialFile">
        ///     The initial filename to open the file dialog at (no value = file will open at default
        ///     location)
        /// </param>
        /// <returns>Returns the file path of the selected file.</returns>
        public static string GetDwsFile(this Editor acCurEd, string prompt, string initialDir = "",
            string initialFile = "")
        {
            //Prompt User To Select A File
            var fileOpts = new PromptOpenFileOptions("")
            {
                Message = prompt,
                AllowUrls = false,
                DialogCaption = "Select an AutoCAD DWS file type",
                DialogName = "DWS File Selection",
                Filter = "Drawing Standards (*.dws)|*.dws"
            };

            //If an initial directory is specified -> set it
            if (initialDir != "") fileOpts.InitialDirectory = initialDir;

            //If an initial filename is specified -> set it
            if (initialFile != "") fileOpts.InitialFileName = initialFile;

            //Get the selected file for open
            var fileRes = acCurEd.GetFileNameForOpen(fileOpts);

            //If file is not available for open (or bad input) -> return empty string
            if (fileRes.Status != PromptStatus.OK) return "";

            //Return the selected filename & path
            return fileRes.StringResult;
        }

        #endregion

        #region Prompt Point Options

        /// <summary>
        ///     Gets any point input, Allows any 3d point selection
        /// </summary>
        /// <param name="acCurEd">The current working editor.</param>
        /// <param name="prompt">The prompt to present to the user.</param>
        /// <param name="basePt">
        ///     The base point to use in the editor, if a value is passed, the editor will use this as the base
        ///     point.
        /// </param>
        /// <returns></returns>
        public static Point3d Get3DPoint(this Editor acCurEd, string prompt, Point3d basePt = default)
        {
            var prPtOpts = new PromptPointOptions("")
            {
                Message = prompt,
                AllowNone = false
            };

            //Check if a base point has been passed to the method
            if (basePt != default)
            {
                prPtOpts.BasePoint = basePt;
                prPtOpts.UseBasePoint = true;
            }

            //Prompt the editor to receive the point from the user
            var prPtRes = acCurEd.GetPoint(prPtOpts);

            //If bad input -> return 0
            if (prPtRes.Status != PromptStatus.OK) return default;

            //Return the distance entered into the editor
            var ptResult = prPtRes.Value;
            return ptResult;
        }

        /// <summary>
        ///     Gets any point input, Allows any 3d point selection -> then converts the point to 2d
        /// </summary>
        /// <param name="acCurEd">The current working editor.</param>
        /// <param name="prompt">The prompt to present to the user.</param>
        /// <param name="basePt">
        ///     The base point to use in the editor, if a value is passed, the editor will use this as the base
        ///     point.
        /// </param>
        /// <returns></returns>
        public static Point2d Get2DPoint(this Editor acCurEd, string prompt, Point2d basePt = default)
        {
            var prPtOpts = new PromptPointOptions("")
            {
                Message = prompt,
                AllowNone = false
            };

            //Check if a base point has been passed to the method
            if (basePt != default)
            {
                prPtOpts.BasePoint = new Point3d(basePt.X, basePt.Y, 0);
                prPtOpts.UseBasePoint = true;
            }

            //Prompt the editor to receive the point from the user
            var prPtRes = acCurEd.GetPoint(prPtOpts);

            //If bad input -> return 0
            if (prPtRes.Status != PromptStatus.OK) return default;

            //Return the distance entered into the editor
            var ptResult = new Point2d(prPtRes.Value.X, prPtRes.Value.Y);
            return ptResult;
        }

        #endregion

        #region Prompt Save File Options

        /// <summary>
        ///     Get any file location to save the file of the file type DWG, DXF, DWT, or DWS from the user.
        /// </summary>
        /// <param name="acCurEd">The current working editor.</param>
        /// <param name="prompt">The prompt to present to the user.</param>
        /// <param name="initialDir">
        ///     The initial directory to open the file dialog at (no value = file will open at default
        ///     location)
        /// </param>
        /// <param name="initialFile">
        ///     The initial filename to open the file dialog at (no value = file will open at default
        ///     location)
        /// </param>
        /// <returns>Returns the file path & name to create a save file.</returns>
        public static string SaveCadFile(this Editor acCurEd, string prompt, string initialDir = "",
            string initialFile = "")
        {
            //Prompt User To Select A File
            var fileOpts = new PromptSaveFileOptions("")
            {
                Message = prompt,
                AllowUrls = false,
                DialogCaption = "",
                DialogName = "CAD File Save",
                Filter = "Drawing (*.dwg)|*.dwg|" +
                         "Design Interchange Format (*.dxf)|*.dxf|" +
                         "Drawing Template (*.dwt)|*.dwt|" +
                         "Drawing Standards (*.dws)|*.dws"
            };

            //If an initial directory is specified -> set it
            if (initialDir != "") fileOpts.InitialDirectory = initialDir;

            //If an initial filename is specified -> set it
            if (initialFile != "") fileOpts.InitialFileName = initialFile;

            //Get the selected file for open
            var fileRes = acCurEd.GetFileNameForSave(fileOpts);

            //If file is not available for open (or bad input) -> return empty string
            if (fileRes.Status != PromptStatus.OK) return "";

            //Return the selected filename & path
            return fileRes.StringResult;
        }

        /// <summary>
        ///     Get any file location to save the file of the file type DWG from the user.
        /// </summary>
        /// <param name="acCurEd">The current working editor.</param>
        /// <param name="prompt">The prompt to present to the user.</param>
        /// <param name="initialDir">
        ///     The initial directory to open the file dialog at (no value = file will open at default
        ///     location)
        /// </param>
        /// <param name="initialFile">
        ///     The initial filename to open the file dialog at (no value = file will open at default
        ///     location)
        /// </param>
        /// <returns>Returns the file path & name to create a save file.</returns>
        public static string SaveDwgFile(this Editor acCurEd, string prompt, string initialDir = "",
            string initialFile = "")
        {
            //Prompt User To Select A File
            var fileOpts = new PromptSaveFileOptions("")
            {
                Message = prompt,
                AllowUrls = false,
                DialogCaption = "",
                DialogName = "DWG File Save",
                Filter = "Drawing (*.dwg)|*.dwg|"
            };

            //If an initial directory is specified -> set it
            if (initialDir != "") fileOpts.InitialDirectory = initialDir;

            //If an initial filename is specified -> set it
            if (initialFile != "") fileOpts.InitialFileName = initialFile;

            //Get the selected file for open
            var fileRes = acCurEd.GetFileNameForSave(fileOpts);

            //If file is not available for open (or bad input) -> return empty string
            if (fileRes.Status != PromptStatus.OK) return "";

            //Return the selected filename & path
            return fileRes.StringResult;
        }

        /// <summary>
        ///     Get any file location to save the file of the file type DXF from the user.
        /// </summary>
        /// <param name="acCurEd">The current working editor.</param>
        /// <param name="prompt">The prompt to present to the user.</param>
        /// <param name="initialDir">
        ///     The initial directory to open the file dialog at (no value = file will open at default
        ///     location)
        /// </param>
        /// <param name="initialFile">
        ///     The initial filename to open the file dialog at (no value = file will open at default
        ///     location)
        /// </param>
        /// <returns>Returns the file path & name to create a save file.</returns>
        public static string SaveDxfFile(this Editor acCurEd, string prompt, string initialDir = "",
            string initialFile = "")
        {
            //Prompt User To Select A File
            var fileOpts = new PromptSaveFileOptions("")
            {
                Message = prompt,
                AllowUrls = false,
                DialogCaption = "",
                DialogName = "DXF File Save",
                Filter = "Design Interchange Format (*.dxf)|*.dxf|"
            };

            //If an initial directory is specified -> set it
            if (initialDir != "") fileOpts.InitialDirectory = initialDir;

            //If an initial filename is specified -> set it
            if (initialFile != "") fileOpts.InitialFileName = initialFile;

            //Get the selected file for open
            var fileRes = acCurEd.GetFileNameForSave(fileOpts);

            //If file is not available for open (or bad input) -> return empty string
            if (fileRes.Status != PromptStatus.OK) return "";

            //Return the selected filename & path
            return fileRes.StringResult;
        }

        /// <summary>
        ///     Get any file location to save the file of the file type DWT from the user.
        /// </summary>
        /// <param name="acCurEd">The current working editor.</param>
        /// <param name="prompt">The prompt to present to the user.</param>
        /// <param name="initialDir">
        ///     The initial directory to open the file dialog at (no value = file will open at default
        ///     location)
        /// </param>
        /// <param name="initialFile">
        ///     The initial filename to open the file dialog at (no value = file will open at default
        ///     location)
        /// </param>
        /// <returns>Returns the file path & name to create a save file.</returns>
        public static string SaveDwtFile(this Editor acCurEd, string prompt, string initialDir = "",
            string initialFile = "")
        {
            //Prompt User To Select A File
            var fileOpts = new PromptSaveFileOptions("")
            {
                Message = prompt,
                AllowUrls = false,
                DialogCaption = "",
                DialogName = "DWT File Selection",
                Filter = "Drawing Template (*.dwt)|*.dwt|"
            };

            //If an initial directory is specified -> set it
            if (initialDir != "") fileOpts.InitialDirectory = initialDir;

            //If an initial filename is specified -> set it
            if (initialFile != "") fileOpts.InitialFileName = initialFile;

            //Get the selected file for open
            var fileRes = acCurEd.GetFileNameForSave(fileOpts);

            //If file is not available for open (or bad input) -> return empty string
            if (fileRes.Status != PromptStatus.OK) return "";

            //Return the selected filename & path
            return fileRes.StringResult;
        }

        /// <summary>
        ///     Get any file location to save the file of the file type DWS from the user.
        /// </summary>
        /// <param name="acCurEd">The current working editor.</param>
        /// <param name="prompt">The prompt to present to the user.</param>
        /// <param name="initialDir">
        ///     The initial directory to open the file dialog at (no value = file will open at default
        ///     location)
        /// </param>
        /// <param name="initialFile">
        ///     The initial filename to open the file dialog at (no value = file will open at default
        ///     location)
        /// </param>
        /// <returns>Returns the file path & name to create a save file.</returns>
        public static string SaveDwsFile(this Editor acCurEd, string prompt, string initialDir = "",
            string initialFile = "")
        {
            //Prompt User To Select A File
            var fileOpts = new PromptSaveFileOptions("")
            {
                Message = prompt,
                AllowUrls = false,
                DialogCaption = "Select an AutoCAD DWS file type",
                DialogName = "DWS File Selection",
                Filter = "Drawing Standards (*.dws)|*.dws"
            };

            //If an initial directory is specified -> set it
            if (initialDir != "") fileOpts.InitialDirectory = initialDir;

            //If an initial filename is specified -> set it
            if (initialFile != "") fileOpts.InitialFileName = initialFile;

            //Get the selected file for open
            var fileRes = acCurEd.GetFileNameForSave(fileOpts);

            //If file is not available for open (or bad input) -> return empty string
            if (fileRes.Status != PromptStatus.OK) return "";

            //Return the selected filename & path
            return fileRes.StringResult;
        }

        #endregion

        #region Prompt Selection Options

        /// <summary>
        ///     Method to prompt the user to select any set of objects.
        /// </summary>
        /// <param name="acCurEd">The current working editor.</param>
        /// <returns>Returns an objectID collection of the selected objects.</returns>
        public static ObjectId[] GetAllSelection(this Editor acCurEd, bool singleSelection)
        {
            var prSelOpts = new PromptSelectionOptions
            {
                AllowDuplicates = false,
                AllowSubSelections = false,
                RejectObjectsFromNonCurrentSpace = true,
                RejectObjectsOnLockedLayers = true,
                MessageForAdding = singleSelection ? "Select object to add: " : "Select objects to add: ",
                MessageForRemoval = singleSelection ? "Select object to remove: " : "Select objects to remove: ",
                SingleOnly = singleSelection,
                SinglePickInSpace = singleSelection
            };

            //Get the selection from the user
            var prSelRes = acCurEd.GetSelection();

            //If bad input -> return empty array
            if (prSelRes.Status != PromptStatus.OK) return new ObjectId[0];

            //Get the array of object Id's and return the value;
            var objIds = prSelRes.Value.GetObjectIds();
            return objIds;
        }

        /// <summary>
        ///     Method to prompt the user to select a specific type of object (by DXF name)
        /// </summary>
        /// <param name="acCurEd">The current working editor.</param>
        /// <param name="filterArg">The DXF name to filter by.</param>
        /// <param name="singleSelection"></param>
        /// <param name="keyList"></param>
        /// <returns>Returns an objectID collection of the selected objects.</returns>
        public static ObjectId[] GetFilteredSelection(this Editor acCurEd, Enums.DxfNameEnum filterArg,
            bool singleSelection, List<KeywordAgent> keyList = null, string msgForAdding = null, string msgForRemoval = null)
        {
            //Convert the DXFName enum value to its string value
            var dxfName = EnumAgent.GetNameOf(filterArg);

            //Remove underscores from the enum name
            dxfName = dxfName.Replace("_", "");

            //Convert to Upper Case
            dxfName = dxfName.ToUpper();

            var prSelOpts = new PromptSelectionOptions
            {
                AllowDuplicates = false,
                AllowSubSelections = true,
                RejectObjectsFromNonCurrentSpace = true,
                RejectObjectsOnLockedLayers = true,
                SingleOnly = singleSelection,
                SinglePickInSpace = singleSelection,
                MessageForAdding = singleSelection
                    ? "Select " + dxfName.ToUpper() + " object to add: "
                    : "Select " + dxfName.ToUpper() + " objects to add: ",
                MessageForRemoval = singleSelection
                    ? "Select " + dxfName.ToUpper() + " object to remove: "
                    : "Select " + dxfName.ToUpper() + " objects to remove: "
            };

            if (msgForAdding != null)
                prSelOpts.MessageForAdding = msgForAdding;

            if (msgForRemoval != null)
                prSelOpts.MessageForRemoval = msgForRemoval;

            #region KeywordAgent

            HandleKeywords(keyList, prSelOpts);

            #endregion


            //Create a selection filter to only allow the specified object
            var selFilter = new SelectionFilter(new[] {new TypedValue((int) DxfCode.Start, dxfName)});

            //Get the selection from the user
            var prSelRes = acCurEd.GetSelection(prSelOpts, selFilter);

            //If bad input -> return empty array
            if (prSelRes.Status != PromptStatus.OK) return new ObjectId[0];

            //Get the array of object Id's and return the value;
            var objIds = prSelRes.Value.GetObjectIds();
            return objIds;
        }

        /// <summary>
        ///     Method to prompt the user to select a specific type of subentity = only allows single selection.
        /// </summary>
        /// <param name="acCurEd">The current working editor.</param>
        /// <param name="subEntType">The type of subentity to be selected.</param>
        /// <returns>Returns a tuple value of the subentity id and its parent objectId.</returns>
        public static Tuple<ObjectId, SubentityId> SelectSubentity(this Editor acCurEd, SubentityType subEntType)
        {
            //Set the ObjectId and SubentId to Null
            var objId = ObjectId.Null;
            var subId = SubentityId.Null;

            //Convert the DXFName enum value to its string value
            var subEntName = EnumAgent.GetNameOf(subEntType);

            var prSelOpts = new PromptSelectionOptions
            {
                AllowDuplicates = false,
                AllowSubSelections = true,
                ForceSubSelections = true,
                RejectObjectsFromNonCurrentSpace = true,
                RejectObjectsOnLockedLayers = true,
                SingleOnly = true,
                SinglePickInSpace = true,
                MessageForAdding = "Select " + subEntName.ToTitleCase() + " objects to add: ",
                MessageForRemoval = "Select " + subEntName.ToTitleCase() + " objects to remove: "
            };


            PromptSelectionResult prRes;

            //Set the SubObject Selection Mode
            var userSubSelectMode = AcVars.SubObjSelMode;

            try
            {
                switch (subEntType)
                {
                    case SubentityType.Vertex:
                        AcVars.SubObjSelMode = Enums.SubObjEnum.Vertex;
                        break;

                    case SubentityType.Edge:
                        AcVars.SubObjSelMode = Enums.SubObjEnum.Edge;
                        break;

                    case SubentityType.Face:
                        AcVars.SubObjSelMode = Enums.SubObjEnum.Face;
                        break;

                    default:
                        AcVars.SubObjSelMode = Enums.SubObjEnum.NoFilter;
                        break;
                }

                //Get the selection from the User         
                prRes = acCurEd.GetSelection(prSelOpts);
            }
            finally
            {
                //Set the Sub Select Mode back to the Users current setting
                AcVars.SubObjSelMode = userSubSelectMode;
            }

            //If the Prompt Result is OK
            if (prRes.Status == PromptStatus.OK)
            {
                //Get the selected Object and set the object Id
                var selObj = prRes.Value[0];
                objId = selObj.ObjectId;

                if (!objId.IsNull || !objId.IsErased)
                {
                    //Get the entity path to the sub entity
                    var subEnts = selObj.GetSubentities();
                    var fsPath = subEnts[0].FullSubentityPath;
                    var subEntId = fsPath.SubentId;
                    var subType = subEntId.Type;

                    if (subType == subEntType) subId = subEntId;
                }
            }

            //Return the selected subentity ID and its parent object ID
            return Tuple.Create(objId, subId);
        }

        public static ObjectId[] SelectAllOfType(this Editor acCurEd, string dxfVals, Transaction acTrans)
        {
            SelectionSet acSSet = null;

            var curSpace = 0;

            if (AcVars.TileMode == Enums.TileModeEnum.Paperspace)
            {
                curSpace = 1;
            }

            // Create a TypedValue array to define the filter criteria
            var acTypValAr = new TypedValue[2];
            acTypValAr.SetValue(new TypedValue((int)DxfCode.Start, dxfVals), 0);
            acTypValAr.SetValue(new TypedValue(67, curSpace), 1);

            // Assign the filter criteria to a SelectionFilter object
            var acSelFtr = new SelectionFilter(acTypValAr);

            // Request for objects to be selected in the drawing area
            var acSsPrompt = acCurEd.SelectAll(acSelFtr);

            // If the prompt status is OK, objects were selected
            if (acSsPrompt.Status == PromptStatus.OK)
            {
                acSSet = acSsPrompt.Value;
            }

            return acSSet != null ? acSSet.GetObjectIds() : new ObjectId[0];
        }

        /// <summary>
        ///     Method to prompt the user to select a specific type of subentities = only allows single selection.
        /// </summary>
        /// <param name="acCurEd">The current working editor.</param>
        /// <param name="subEntType">The type of subentities to be selected.</param>
        /// <returns>Returns a tuple value of the subentity ids and their parent objectIds.</returns>
        public static List<Tuple<ObjectId, SubentityId>> SelectSubentities(this Editor acCurEd,
            SubentityType subEntType)
        {
            //Set the ObjectId and SubentId to Null

            var entList = new List<Tuple<ObjectId, SubentityId>>();

            //Convert the DXFName enum value to its string value
            var subEntName = EnumAgent.GetNameOf(subEntType);

            var prSelOpts = new PromptSelectionOptions
            {
                AllowDuplicates = false,
                AllowSubSelections = true,
                ForceSubSelections = true,
                RejectObjectsFromNonCurrentSpace = true,
                RejectObjectsOnLockedLayers = true,
                SingleOnly = false,
                SinglePickInSpace = false,
                MessageForAdding = "Select " + subEntName.ToTitleCase() + " objects to add: ",
                MessageForRemoval = "Select " + subEntName.ToTitleCase() + " objects to remove: "
            };

            PromptSelectionResult prRes;

            //Set the SubObject Selection Mode
            var userSubSelectMode = AcVars.SubObjSelMode;

            try
            {
                switch (subEntType)
                {
                    case SubentityType.Vertex:
                        AcVars.SubObjSelMode = Enums.SubObjEnum.Vertex;
                        break;

                    case SubentityType.Edge:
                        AcVars.SubObjSelMode = Enums.SubObjEnum.Edge;
                        break;

                    case SubentityType.Face:
                        AcVars.SubObjSelMode = Enums.SubObjEnum.Face;
                        break;

                    default:
                        AcVars.SubObjSelMode = Enums.SubObjEnum.NoFilter;
                        break;
                }

                //Get the selection from the User         
                prRes = acCurEd.GetSelection(prSelOpts);
            }
            finally
            {
                //Set the Sub Select Mode back to the Users current setting
                AcVars.SubObjSelMode = userSubSelectMode;
            }

            //If the Prompt Result is OK
            if (prRes.Status == PromptStatus.OK)
                foreach (SelectedObject selObj in prRes.Value)
                {
                    var objId = selObj.ObjectId;

                    if (!objId.IsNull || !objId.IsErased)
                    {
                        //Get the entity path to the sub entity
                        var subEnts = selObj.GetSubentities();
                        var fsPath = subEnts[0].FullSubentityPath;
                        var subEntId = fsPath.SubentId;
                        var subType = subEntId.Type;

                        if (subType == subEntType)
                        {
                            var subId = subEntId;

                            //Add the new tuple value to the list
                            entList.Add(Tuple.Create(objId, subId));
                        }
                    }
                }

            //Return the list of selected subentityIds and their parent object IDs
            return entList;
        }

        /// <summary>
        ///     Method to Programmatically select all objects of a type in the current space.
        /// </summary>
        /// <param name="acCurEd">The current working Editor</param>
        /// <param name="filterArgs">The array of dxf Names to be filtered</param>
        /// <returns></returns>
        public static ObjectId[] GetObjectsByType(this Editor acCurEd, Enums.DxfNameEnum[] filterArgs)
        {
            SelectionSet acSSet = null;
            var curSpace = (int) AcVars.TileMode;

            var dxfNames = new List<string>();

            foreach (var filterArg in filterArgs)
            {
                //Convert the DXFName enum value to its string value
                var dxfName = EnumAgent.GetNameOf(filterArg);

                //Remove underscores from the enum name
                dxfName = dxfName.Replace("_", "");

                //Convert to Upper Case
                dxfName = dxfName.ToUpper();
                dxfNames.Add(dxfName);
            }

            var filterValue = string.Join(",", dxfNames);

            // Create a TypedValue array to define the filter criteria
            var acTypValAr = new TypedValue[2];
            acTypValAr.SetValue(new TypedValue((int) DxfCode.Start, filterValue), 0);
            acTypValAr.SetValue(new TypedValue(67, curSpace), 1);

            // Assign the filter criteria to a SelectionFilter object
            var acSelFtr = new SelectionFilter(acTypValAr);

            // Request for objects to be selected in the drawing area
            var acSsPrompt = acCurEd.SelectAll(acSelFtr);

            // If the prompt status is OK, objects were selected
            if (acSsPrompt.Status == PromptStatus.OK) acSSet = acSsPrompt.Value;

            return acSSet != null ? acSSet.GetObjectIds() : new ObjectId[0];
        }

        #endregion

        #region Coordinate Conversion

        /// <summary>
        ///     Gets the transformation matrix from the current User Coordinate System (UCS)
        ///     to the World Coordinate System (WCS).
        /// </summary>
        /// <param name="ed">The instance to which this method applies.</param>
        /// <returns>The UCS to WCS transformation matrix.</returns>
        public static Matrix3d Ucs2Wcs(this Editor ed)
        {
            return ed.CurrentUserCoordinateSystem;
        }

        /// <summary>
        ///     Gets the transformation matrix from the World Coordinate System (WCS)
        ///     to the current User Coordinate System (UCS).
        /// </summary>
        /// <param name="ed">The instance to which this method applies.</param>
        /// <returns>The WCS to UCS transformation matrix.</returns>
        public static Matrix3d Wcs2Ucs(this Editor ed)
        {
            return ed.CurrentUserCoordinateSystem.Inverse();
        }

        /// <summary>
        ///     Gets the transformation matrix from the current viewport Display Coordinate System (DCS)
        ///     to the World Coordinate System (WCS).
        /// </summary>
        /// <param name="ed">The instance to which this method applies.</param>
        /// <returns>The DCS to WCS transformation matrix.</returns>
        public static Matrix3d Dcs2Wcs(this Editor ed)
        {
            Matrix3d retVal;
            var tilemode = ed.Document.Database.TileMode;
            if (!tilemode)
                ed.SwitchToModelSpace();
            using (var vtr = ed.GetCurrentView())
            {
                retVal =
                    Matrix3d.Rotation(-vtr.ViewTwist, vtr.ViewDirection, vtr.Target) *
                    Matrix3d.Displacement(vtr.Target - Point3d.Origin) *
                    Matrix3d.PlaneToWorld(vtr.ViewDirection);
            }

            if (!tilemode)
                ed.SwitchToPaperSpace();
            return retVal;
        }

        /// <summary>
        ///     Gets the transformation matrix from the World Coordinate System (WCS)
        ///     to the current viewport Display Coordinate System (DCS).
        /// </summary>
        /// <param name="ed">The instance to which this method applies.</param>
        /// <returns>The WCS to DCS transformation matrix.</returns>
        public static Matrix3d Wcs2Dcs(this Editor ed)
        {
            return ed.Dcs2Wcs().Inverse();
        }

        /// <summary>
        ///     Gets the transformation matrix from the paper space active viewport Display Coordinate System (DCS)
        ///     to the Paper space Display Coordinate System (PSDCS).
        /// </summary>
        /// <param name="ed">The instance to which this method applies.</param>
        /// <returns>The DCS to PSDCS transformation matrix.</returns>
        /// <exception cref=" Autodesk.AutoCAD.Runtime.Exception">
        ///     eNotInPaperSpace is thrown if this method is called form Model Space.
        /// </exception>
        /// <exception cref=" Autodesk.AutoCAD.Runtime.Exception">
        ///     eCannotChangeActiveViewport is thrown if there is none floating viewport in the current layout.
        /// </exception>
        public static Matrix3d Dcs2Psdcs(this Editor ed)
        {
            var db = ed.Document.Database;
            if (db.TileMode)
                throw new AcRx.Exception(AcRx.ErrorStatus.NotInPaperspace);
            using (var tr = db.TransactionManager.StartTransaction())
            {
                var vp =
                    (Viewport) tr.GetObject(ed.CurrentViewportObjectId, OpenMode.ForRead);
                if (vp.Number == 1)
                    try
                    {
                        ed.SwitchToModelSpace();
                        vp = (Viewport) tr.GetObject(ed.CurrentViewportObjectId, OpenMode.ForRead);
                        ed.SwitchToPaperSpace();
                    }
                    catch
                    {
                        throw new AcRx.Exception(AcRx.ErrorStatus.CannotChangeActiveViewport);
                    }

                return vp.Dcs2Psdcs();
            }
        }

        /// <summary>
        ///     Gets the transformation matrix from the Paper space Display Coordinate System (PSDCS)
        ///     to the paper space active viewport Display Coordinate System (DCS).
        /// </summary>
        /// <param name="ed">The instance to which this method applies.</param>
        /// <returns>The PSDCS to DCS transformation matrix.</returns>
        /// <exception cref=" Autodesk.AutoCAD.Runtime.Exception">
        ///     eNotInPaperSpace is thrown if this method is called from Model Space.
        /// </exception>
        /// <exception cref=" Autodesk.AutoCAD.Runtime.Exception">
        ///     eCannotChangeActiveViewport is thrown if there is none floating viewport in the current layout.
        /// </exception>
        public static Matrix3d Psdcs2Dcs(this Editor ed)
        {
            return ed.Dcs2Psdcs().Inverse();
        }

        #endregion

        #region Wait For Exit

        public static void WaitForExit(this Editor acCurEd)
        {
            var prStrOpts = new PromptStringOptions("Press ENTER or ESC to continue ") {AllowSpaces = false};
            acCurEd.GetString(prStrOpts);
        }

        #endregion

        public static ObjectId[] SelectAtPoint(this Editor acCurEd, Point3d pt)
        {
            var p = pt;
            var tol = 0.01;
            var p1 = new Point3d(p.X - tol, p.Y - tol, p.Z - tol);
            var p2 = new Point3d(p.X + tol, p.Y + tol, p.Z + tol);

            var res = acCurEd.SelectCrossingWindow(p1, p2);

            if (res.Status != PromptStatus.OK)
            {
                return new ObjectId[0];
            }

            var ss = res.Value;
            return ss.GetObjectIds();
        }

        
    }
}