// -----------------------------------------------------------------------------------
//     <copyright file="ProgressAgent.cs" company="CraterSpace">
//     Copyright (c) 2019 CraterSpace - All Rights Reserved 
//     </copyright>
//     <author>Zach Ayers</author>
//     <date>03/08/2019</date>
//     Description:    
//     Notes:  
//     References:          
// -----------------------------------------------------------------------------------

using System;
using System.Windows.Forms;
using Autodesk.AutoCAD.Runtime;
using RabCab.Exceptions;

namespace RabCab.Agents
{
    /// <summary>
    ///     Class to handle a Progress bar for CAD
    /// </summary>
    public class ProgressAgent : IDisposable, IMessageFilter
    {
        private static ProgressAgent _pmActive;
        private readonly bool _pmCycle;
        private ProgressMeter _pm;
        private int _pmLimit;
        private string _pmMsgBar;
        public bool Cancelled;

        public ProgressAgent(string barMessage, int limit, bool cycle = false)
        {
            Active = null;
            _pmMsgBar = barMessage;
            Application.AddMessageFilter(this);
            _pm = new ProgressMeter();
            _pm.SetLimit(limit);
            _pm.Start(barMessage);
            Position = 0;
            _pmCycle = cycle;
            _pmLimit = limit;
            Active = this;
        }

        public int Position { get; private set; }

        public static ProgressAgent Active
        {
            get { return _pmActive; }
            set
            {
                _pmActive?.Dispose();
                _pmActive = value;
            }
        }

        public void Dispose()
        {
            if (_pm != null && !_pm.IsDisposed)
            {
                _pm.Stop();
                _pm.Dispose();
            }

            Application.RemoveMessageFilter(this);
            if (_pmActive != this)
                return;
            _pmActive = null;
        }

        public bool PreFilterMessage(ref Message m)
        {
            if (m.Msg != 256)
                return false;
            var keys = (Keys) ((int) m.WParam & ushort.MaxValue);
            if (m.Msg == 256 && keys == Keys.Escape)
                Cancelled = true;
            return true;
        }

        public static ProgressAgent CreateIfNotExist(string message, int limit, bool cycle = false)
        {
            if (Active != null)
                return null;
            return new ProgressAgent(message, limit, cycle);
        }

        public void SetTotalOperations(int limit)
        {
            Position = 0;
            _pmLimit = limit;
            _pm.SetLimit(limit);
        }

        public void AddOperations(int add)
        {
            _pmLimit = _pmLimit + add;
            _pm.SetLimit(_pmLimit);
        }

        public void Reset(string barMessage = "")
        {
            if (barMessage != "")
                _pmMsgBar = barMessage;
            Position = 0;
            if (_pm != null && !_pm.IsDisposed)
            {
                _pm.Stop();
                _pm.Dispose();
            }

            _pm = new ProgressMeter();
            _pm.SetLimit(_pmLimit);
            _pm.Start(_pmMsgBar);
        }

        public bool Tick()
        {
            if (_pm == null || _pm.IsDisposed)
                return false;
            if (Cancelled)
            {
                _pm.Stop();
                return false;
            }

            Position = Position + 1;
            if (Position > _pmLimit)
            {
                if (_pmCycle)
                {
                    Reset();
                }
                else
                {
                    _pmLimit = _pmLimit*2;
                    _pm.SetLimit(_pmLimit);
                }
            }

            _pm.MeterProgress();
            Application.DoEvents();
            return true;
        }

        public void TickOrEsc()
        {
            if (!Tick())
                throw new CancelException();
        }

        public static void ActiveTickOrEsc()
        {
            if (Active == null || Active._pm.IsDisposed)
                throw new NullReferenceException("ProgressAgent must exist");
            Active.TickOrEsc();
        }
    }
}