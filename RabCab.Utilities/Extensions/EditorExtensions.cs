using Autodesk.AutoCAD.EditorInput;
using RabCab.Utilities.Calculators;

namespace RabCab.Utilities.Extensions
{
    internal static class EditorExtensions
    {

        #region Prompt Angle Options

        /// <summary>
        /// Method to get an angle from the user
        /// </summary>
        /// <param name="acCurEd">The current working editor</param>
        /// <param name="prompt">The string to be prompted to the user</param>
        /// <returns>Returns the value input by the user as a Radian</returns>
        public static double GetRadian(this Editor acCurEd, string prompt)
        {
            //Prompt user to enter an angle in autoCAD
            var prAngOpts = new PromptAngleOptions("")
            {
                Message = prompt,
                AllowNone = false,
                AllowZero = false
            };

            //Prompt the editor to receive the angle from the user
            var prAngRes = acCurEd.GetAngle(prAngOpts);

            //If bad input entered - return 0
            if (prAngRes.Status != PromptStatus.OK) return 0;

            //Get the angle entered from the editor
            var doubleResult = prAngRes.Value;
            return doubleResult;
        }

        /// <summary>
        /// Method to get an angle from the user
        /// </summary>
        /// <param name="acCurEd">The current working editor</param>
        /// <param name="prompt">The string to be prompted to the user</param>
        /// <returns>Returns the value input by the user as a Radian</returns>
        public static double GetDegree(this Editor acCurEd, string prompt)
        {
            //Prompt user to enter an angle in autoCAD
            var prAngOpts = new PromptAngleOptions("")
            {
                Message = prompt,
                AllowNone = false,
                AllowZero = false
            };

            //Prompt the editor to receive the angle from the user
            var prAngRes = acCurEd.GetAngle(prAngOpts);

            //If bad input entered - return 0
            if (prAngRes.Status != PromptStatus.OK) return 0;

            //Get the angle entered from the editor
            var doubleResult = prAngRes.Value;
            return UnitConverter.ConvertToDegrees(doubleResult);
        }

        #endregion

    }
}
