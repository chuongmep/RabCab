using System;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using RabCab.Utilities.Calculators;
// ReSharper disable CompareOfFloatsByEqualityOperator

namespace RabCab.Utilities.Extensions
{
    internal static class EditorExtensions
    {

        #region Prompt Angle Options

        /// <summary>
        /// Method to get an angle from the user in radians
        /// </summary>
        /// <param name="acCurEd">The current working editor</param>
        /// <param name="prompt">The string to be prompted to the user</param>
        /// <param name="defaultValue">The default value to use in prompt -> pressing enter will automatically use the default distance. Default value is input in Degrees</param>
        /// <returns>Returns the value input by the user as a radian</returns>
        public static double GetRadian(this Editor acCurEd, string prompt, double defaultValue = 0)
        {//If default value, convert it from degrees to radians

            if (defaultValue != 0)
                defaultValue = UnitConverter.ConvertToRadians(defaultValue);

            //Prompt user to enter an angle in autoCAD
            var prAngOpts = new PromptAngleOptions(string.Empty)
            {
                Message = prompt,
                AllowNone = false,
                AllowZero = false,
                DefaultValue = defaultValue,
                UseDefaultValue = defaultValue != 0,
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
        /// Method to get an angle from the user and return it in degrees
        /// </summary>
        /// <param name="acCurEd">The current working editor</param>
        /// <param name="prompt">The prompt to present to the user</param>
        /// <param name="defaultValue">The default value to use in prompt -> pressing enter will automatically use the default distance. Default value is input in degrees</param>
        /// <returns>Returns the value input by the user as a degree</returns>
        public static double GetDegree(this Editor acCurEd, string prompt, double defaultValue = 0)
        {
            //If default value, convert it from degrees to radians
            if (defaultValue != 0)
                defaultValue = UnitConverter.ConvertToRadians(defaultValue);

            //Prompt user to enter an angle in autoCAD
            var prAngOpts = new PromptAngleOptions(string.Empty)
            {
                Message = prompt,
                AllowNone = false,
                AllowZero = false,
                DefaultValue = defaultValue,
                UseDefaultValue = defaultValue != 0,
            };

            //Prompt the editor to receive the angle from the user
            var prAngRes = acCurEd.GetAngle(prAngOpts);

            //If bad input -> return 0
            if (prAngRes.Status != PromptStatus.OK) return 0;

            //Return the angle entered into the editor
            var doubleResult = prAngRes.Value;
            return UnitConverter.ConvertToDegrees(doubleResult);
        }

        #endregion

        //TODO Prompt Corner Options

        #region Prompt Distance Options

        /// <summary>
        /// Gets any 3D distance in CAD, allows positive and negative values.
        /// </summary>
        /// <param name="acCurEd">The current working Editor.</param>
        /// <param name="prompt">The prompt to present to the user.</param>
        /// <param name="defaultValue">The default value to use in prompt -> pressing enter will automatically use the default distance.</param>
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
                UseDefaultValue = defaultValue != 0,
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
        /// Gets any 3D distance in CAD, uses a base point and allows positive and negative values.
        /// </summary>
        /// <param name="acCurEd">The current working Editor.</param>
        /// <param name="basePt">The point to start the distance from.</param>
        /// <param name="prompt">The prompt to present to the user.</param>
        /// <param name="defaultValue">The default value to use in prompt -> pressing enter will automatically use the default distance.</param>
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
                UseDefaultValue = defaultValue != 0,
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
        /// Gets any 3D distance in CAD, allows positive values.
        /// </summary>
        /// <param name="acCurEd">The current working Editor.</param>
        /// <param name="prompt">The prompt to present to the user.</param>
        /// <param name="defaultValue">The default value to use in prompt -> pressing enter will automatically use the default distance.</param>
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
                UseDefaultValue = defaultValue != 0,
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
        /// Gets any 3D distance in CAD, uses a base point and allows positive values.
        /// </summary>
        /// <param name="acCurEd">The current working Editor.</param>
        /// <param name="basePt">The point to start the distance from.</param>
        /// <param name="prompt">The prompt to present to the user.</param>
        /// <param name="defaultValue">The default value to use in prompt -> pressing enter will automatically use the default distance.</param>
        /// <returns>Returns a distance from the editor in decimal format.</returns>
        public static double GetPositiveDistance(this Editor acCurEd, Point3d basePt, string prompt, double defaultValue = 0)
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
                UseDefaultValue = defaultValue != 0,
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
        /// Gets any 2D distance in CAD, allows positive and negative values.
        /// </summary>
        /// <param name="acCurEd">The current working Editor.</param>
        /// <param name="prompt">The prompt to present to the user.</param>
        /// /// <param name="defaultValue">The default value to use in prompt -> pressing enter will automatically use the default distance.</param>
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
                UseDefaultValue = defaultValue != 0,
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
        /// Gets any 2D distance in CAD, uses a base point and allows positive and negative values.
        /// </summary>
        /// <param name="acCurEd">The current working Editor.</param>
        /// <param name="basePt">The point to start the distance from.</param>
        /// <param name="prompt">The prompt to present to the user.</param>
        /// <param name="defaultValue">The default value to use in prompt -> pressing enter will automatically use the default distance.</param>
        /// <returns>Returns a distance from the editor in decimal format.</returns>
        public static double GetAny2DDistance(this Editor acCurEd, Point3d basePt, string prompt, double defaultValue = 0)
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
                UseDefaultValue = defaultValue != 0,
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
        /// Gets any 2D distance in CAD, allows positive values.
        /// </summary>
        /// <param name="acCurEd">The current working Editor.</param>
        /// <param name="prompt">The prompt to present to the user.</param>
        /// <param name="defaultValue">The default value to use in prompt -> pressing enter will automatically use the default distance.</param>
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
                UseDefaultValue = defaultValue != 0,
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
        /// Gets any 2D distance in CAD, allows positive values.
        /// </summary>
        /// <param name="acCurEd">The current working Editor.</param>
        /// <param name="basePt">The point to start the distance from.</param>
        /// <param name="prompt">The prompt to present to the user.</param>
        /// <param name="defaultValue">The default value to use in prompt -> pressing enter will automatically use the default distance.</param>
        /// <returns>Returns a distance from the editor in decimal format.</returns>
        public static double GetPositive2DDistance(this Editor acCurEd, Point3d basePt, string prompt, double defaultValue = 0)
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
        /// Gets any double input,  Allows positive and negative values.
        /// </summary>
        /// <param name="acCurEd">The current working Editor.</param>
        /// <param name="prompt">The prompt to present to the user.</param>
        /// <param name="defaultValue">The default value to use in prompt -> pressing enter will automatically use the default distance.</param>
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
        /// Gets any double input, Allows positive values.
        /// </summary>
        /// <param name="acCurEd">The current working Editor.</param>
        /// <param name="prompt">The prompt to present to the user.</param>
        /// <param name="defaultValue">The default value to use in prompt -> pressing enter will automatically use the default distance.</param>
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
        /// Method to prompt user to select an entity and return its ObjectId for use in other methods.
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
                AllowObjectOnLockedLayer = false,
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
        /// Method to prompt user to select an entity and return its ObjectId for use in other methods.
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
        /// Method to prompt user to select an entity and return its ObjectId for use in other methods.
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
        /// Gets any integer input, Allows positive and negative values.
        /// </summary>
        /// <param name="acCurEd">The current working Editor.</param>
        /// <param name="prompt">The prompt to present to the user.</param>
        /// <param name="defaultValue">The default value to use in prompt -> pressing enter will automatically use the default distance.</param>
        /// <returns>Returns an integer from the editor.</returns>
        public static int GetAnyInteger(this Editor acCurEd, string prompt, int defaultValue)
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
        /// Gets any integer input, Allows positive values.
        /// </summary>
        /// <param name="acCurEd">The current working Editor.</param>
        /// <param name="prompt">The prompt to present to the user.</param>
        /// <param name="defaultValue">The default value to use in prompt -> pressing enter will automatically use the default distance.</param>
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
        /// Gets any integer input, Allows any value between provided limits.
        /// </summary>
        /// <param name="acCurEd">The current working Editor.</param>
        /// <param name="prompt">The prompt to present to the user.</param>
        /// <param name="min">The minimum limit allowed to be entered by the user.</param>
        /// <param name="max">The maximum limit allowed to be entered by the user.</param>
        /// <param name="defaultValue">The default value to use in prompt -> pressing enter will automatically use the default distance.</param>
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

    }
}
