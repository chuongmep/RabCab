// -----------------------------------------------------------------------------------
//     <copyright file="SurfaceExtensions.cs" company="CraterSpace">
//     Copyright (c) 2019 CraterSpace - All Rights Reserved 
//     </copyright>
//     <author>Zach Ayers</author>
//     <date>03/08/2019</date>
//     Description:    
//     Notes:  
//     References:          
// -----------------------------------------------------------------------------------

using Autodesk.AutoCAD.DatabaseServices;

namespace RabCab.Extensions
{
    public static class SurfaceExtensions
    {
        /// <summary>
        ///     Method to Create A Surface from a selected Face
        /// </summary>
        /// <param name="ent">The Subentity to create the Surface From</param>
        /// <param name="acCurDb">The Current Working Database</param>
        /// <param name="acTrans">The Current Working Transaction</param>
        /// <param name="saveSurface">Append the created surface to the Database?</param>
        public static Surface CreateSurfaceFromFace(this Entity ent, Database acCurDb, Transaction acTrans,
            bool saveSurface)
        {
            //Create a surface variable
            Surface surf1;
            Surface retSurface;


            //Check if the Entity is a surface, if not - create a surface from the face entity
            if (ent is Surface)
                surf1 = (Surface) ent;
            else
                surf1 = Surface.CreateFrom(ent);

            if (saveSurface)
            {
                acCurDb.AppendEntity(surf1, acTrans);
                retSurface = surf1;
            }
            else
            {
                retSurface = surf1.Clone() as Surface;
                surf1.Dispose();
            }

            return retSurface;
        }

        /// <summary>
        ///     Method to Create A Surface from a selected Face
        /// </summary>
        /// <param name="ent">The Subentity to create the Surface From</param>
        /// <param name="saveSurface">Append the created surface to the Database?</param>
        public static Surface CreateSurfaceFromFace(Entity ent, Database acCurDb, bool saveSurface)
        {
            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                //Create a surface variable
                Surface surf1;
                Surface retSurface;

                //Check if the Entity is a surface, if not - create a surface from the face entity
                if (ent is Surface)
                    surf1 = (Surface) ent;
                else
                    surf1 = Surface.CreateFrom(ent);

                if (saveSurface)
                {
                    acCurDb.AppendEntity(surf1, acTrans);
                    retSurface = surf1;
                }
                else
                {
                    retSurface = surf1.Clone() as Surface;
                    surf1.Dispose();
                }

                return retSurface;
            }
        }
    }
}