﻿#region Information
// Behavior originally contributed by AknA.
// A simple Jump Forward QB.
#endregion

#region Using
using System;
using System.Collections.Generic;
using Styx.Common;
using Styx.CommonBot.Profiles;
using Styx.TreeSharp;

using CommonBehaviors.Actions;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;
using Action = Styx.TreeSharp.Action;
#endregion

namespace Styx.Bot.Quest_Behaviors {
    [CustomBehaviorFileName(@"Misc\RangeAttack")]
    public class RangeAttack : CustomForcedBehavior {
        public RangeAttack(Dictionary<string, string> args)
            : base(args) { }

        #region Variables
        // Attributes provided by caller

        // Private variables for internal state
        private static bool _isBehaviorDone;
        private bool _IsDisposed;
        private Composite _Root;
        public static LocalPlayer Me { get { return StyxWoW.Me; } }
        #endregion

        #region Dispose
        ~RangeAttack() { Dispose(false); }

        public void Dispose(bool isExplicitlyInitiatedDispose) {
            if (!_IsDisposed) {
                // NOTE: we should call any Dispose() method for any managed or unmanaged
                // resource, if that resource provides a Dispose() method.

                // Clean up managed resources, if explicit disposal...
                if (isExplicitlyInitiatedDispose) { }  // empty, for now

                // Clean up unmanaged resources (if any) here...
                _isBehaviorDone = false;

                // Call parent Dispose() (if it exists) here ...
                base.Dispose();
            }
            _IsDisposed = true;
        }

        public override void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        public void BotEvents_OnBotStop(EventArgs args) { Dispose(); }
        #endregion

        #region Methods
        private uint GetSpellIDByClass() {
            switch (Me.Class) {
                case WoWClass.DeathKnight:  return 45477;
                case WoWClass.Druid:        return 8921;
                case WoWClass.Hunter:       return 1978;
                case WoWClass.Mage:         return 30455;
                case WoWClass.Monk:         return 117952;
                case WoWClass.Paladin:      return 20271;
                case WoWClass.Priest:       return 589;
                case WoWClass.Rogue:        return 121733;
                case WoWClass.Shaman:       return 8042;
                case WoWClass.Warlock:      return 172;
                case WoWClass.Warrior:      return 122475;
            }
            return 0;
        }
        #endregion

        #region Overrides of CustomForcedBehavior
        protected override Composite CreateBehavior() {
            return _Root ?? (_Root =
                new PrioritySelector(
                    new Decorator(context => !StyxWoW.Me.IsMoving,
                        new DecoratorContinue(context => Me.GotTarget && Me.IsSafelyFacing(Me.CurrentTarget),
                            new Sequence(
                                new DecoratorContinue(context => GetSpellIDByClass() != 0,
                                    new Sequence(
                                        new Action(context => Lua.DoString(string.Format("CastSpellByID({0})",GetSpellIDByClass()))),
                                        new WaitContinue(TimeSpan.FromMilliseconds(500), context => false, new ActionAlwaysSucceed()),
                                        new Action(context => _isBehaviorDone = true)
                                    )
                                ),
                                new DecoratorContinue(context => GetSpellIDByClass() == 0,
                                    new Sequence(
                                        new Action(context => Logging.Write("Can't determine your class, stopping behavior")),
                                        new Action(context => _isBehaviorDone = true)
                                    )
                                )
                            )
                        )
                    )
                )
            );
        }

        public override bool IsDone { get { return _isBehaviorDone; } }

        public override void OnStart() {
            // This reports problems, and stops BT processing if there was a problem with attributes...
            // We had to defer this action, as the 'profile line number' is not available during the element's
            // constructor call.
            OnStart_HandleAttributeProblem();

            if (!IsDone) { }
        }
        #endregion
    }
}
