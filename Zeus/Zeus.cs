


using System.Collections.Generic;

namespace Zeus
{
    using System;
    using System.Linq;
    using System.Windows.Input;

    using Ensage;
    using Ensage.Common;
    using Ensage.Common.Extensions;

    using SharpDX;
    using SharpDX.Direct3D9;
    class Zeus
    {
        #region Static Fields

        private static Item ScepterOfDivinity;
        
        private static Item blink;
        private static float blinkRange;
        private static bool enableBlink = true;

        // Abilities
        private static Ability arcLightning;
        private static bool enableQ = true;

        private static Ability lightningBolt;
        private static bool enableW = true;

        // Ultimate Ability
        private static Ability ThunderGodsWrath;

        // No clue what this is
        private static float hullsum;

        private static float lastActivity;

        private static float lastStack;

        private static bool loaded;

        private static Hero me;

        private static Vector3 mePosition;

        private static float nextAttack;

        private static Hero target;

        private static float targetDistance;

        private static Font text;

        private static double turnTime;


        #endregion

        #region Public Methods and Operators
        public static void Init()
        {
            Game.OnUpdate += Game_OnUpdate;
            loaded = false;

            text = new Font(
                Drawing.Direct3DDevice9,
                new FontDescription
                {
                    FaceName = "Tahoma",
                    Height = 13,
                    OutputPrecision = FontPrecision.Default,
                    Quality = FontQuality.Default
                }
                );

            Drawing.OnPreReset += Drawing_OnPreReset;
            Drawing.OnPostReset += Drawing_OnPostReset;
            Drawing.OnEndScene += Drawing_OnEndScene;
            AppDomain.CurrentDomain.DomainUnload += CurrentDomainDomainUnload;
            Game.OnWndProc += Game_OnWndProc;

        }
        #endregion

        private static bool CastCombo()
        {
            if (!Utils.SleepCheck("casting") || !me.CanCast() || !target.IsVisible)
            {
                return false;
            } // end if

            var casted = false;
            if (ScepterOfDivinity != null && ScepterOfDivinity.CanBeCasted() && targetDistance <= (350 + hullsum) &&
                Utils.SleepCheck("scepter"))
            {
                var canUse = !target.IsStunned() && !target.IsHexed() && !target.IsInvul() && target.IsMagicImmune();

                if (canUse)
                {
                    ScepterOfDivinity.UseAbility(target);
                    Utils.Sleep(turnTime * 1000 + 1000 + Game.Ping, "scepter");
                    Utils.Sleep(turnTime * 1000 + 100, "move");
                    Utils.Sleep(turnTime * 1000 + 100, "casting");
                    casted = true;

                } // end if can use if statement


            } // end if  below casted


            if (blink != null && blink.CanBeCasted() && targetDistance > 400 && targetDistance < (blinkRange + hullsum)
               && Utils.SleepCheck("blink"))
            {
                var position = target.Position + target.Vector3FromPolarAngle() * (hullsum + me.AttackRange);
                if (mePosition.Distance(position) < targetDistance)
                {
                    position = target.Position;
                }
                var dist = position.Distance2D(mePosition);
                if (dist > blinkRange)
                {
                    position = (position - mePosition) * (blinkRange - 1) / position.Distance2D(me) + mePosition;
                }
                blink.UseAbility(position);
                mePosition = position;
                Utils.Sleep(turnTime * 1000 + 1000 + Game.Ping, "blink");
                Utils.Sleep(turnTime * 1000 + 100, "move");
                Utils.Sleep(turnTime * 1000 + 100, "casting");
                casted = true;
            }
            const int Radius = 300;
            var canAttack = !target.IsInvul() && !target.IsAttackImmune() && me.CanAttack();
            if (!canAttack)
            {
                return casted;
            }

            if (me.Spellbook.Spell2.CanBeCasted() && me.Mana > me.Spellbook.Spell2.ManaCost && !target.IsMagicImmune() && !target.IsIllusion && Utils.SleepCheck("W") || enableW)
            {
                me.Spellbook.Spell2.UseAbility(target);
                Utils.Sleep(200 + Game.Ping, "W");
            }



            else if (arcLightning.CanBeCasted())
            {
                if (mePosition.Distance2D(target) <= (Radius + hullsum + 100))
                {
                    arcLightning.UseAbility(target);
                    Utils.Sleep(1000 + Game.Ping, "Q");
                    Utils.Sleep(100, "casting");
                    casted = true;
                }
                else
                {
                    var pos = target.Position
                          + target.Vector3FromPolarAngle() * ((Game.Ping / 1000 + 0.3f) * target.MovementSpeed);
                    me.Move(pos);
                    Utils.Sleep(200, "moveCloser");
                    casted = false;
                }
            }



            if (!ThunderGodsWrath.CanBeCasted() || !Utils.SleepCheck("R"))
            {
                return casted;
            }
            if (!(mePosition.Distance2D(target) <= (Radius + hullsum)))
            {
                return casted;
            }
            ThunderGodsWrath.UseAbility();
            Utils.Sleep(1000 + Game.Ping, "R");
            Utils.Sleep(100, "casting");
            return casted;
        }

        private static void CurrentDomainDomainUnload(object sender, EventArgs e)
        {
            text.Dispose();
        }

        private static void Drawing_OnEndScene(EventArgs args)
        {
            if (Drawing.Direct3DDevice9 == null || Drawing.Direct3DDevice9.IsDisposed || !Game.IsInGame)
            {
                return;
            }

            var player = ObjectMgr.LocalPlayer;
            if (player == null || player.Team == Team.Observer)
            {
                return;
            }

            text.DrawText(
                null,
                enableQ ? "Zeus: Combo - DISABLED! | [G] for toggle" : "Zeus: Combo - ENABLED!! | [G] for toggle",
                5,
                96,
                Color.Teal);
        }

        private static void Drawing_OnPostReset(EventArgs args)
        {
            text.OnResetDevice();
        }

        private static void Drawing_OnPreReset(EventArgs args)
        {
            text.OnLostDevice();
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (!loaded)
            {
                me = ObjectMgr.LocalHero;
                if (!Game.IsInGame || me == null)
                {
                    return;
                }
                arcLightning = me.Spellbook.Spell1;
                lightningBolt = me.Spellbook.SpellW;
                //ThunderGodsWrath = me.FindSpell("centaur_stampede");
                ThunderGodsWrath = me.Spellbook.SpellR;
                blink = me.FindItem("item_blink");
                ScepterOfDivinity = me.FindItem("item_scepter_of_divinity");
                //abyssalBlade = me.FindItem("item_abyssal_blade");
                lastStack = 0;
                loaded = true;
                lastActivity = 0;
            }

            if (!Game.IsInGame || me == null)
            {
                loaded = false;
                return;
            }

            if (Game.IsPaused)
            {
                return;
            }

            var tick = Environment.TickCount;
            if (me.NetworkActivity != (NetworkActivity)lastActivity && target != null)
            {
                lastActivity = (float)me.NetworkActivity;
                if (lastActivity == 1503)
                {
                    nextAttack = (tick + me.SecondsPerAttack * 1000 - Game.Ping);
                }
            }

            if (blink == null)
            {
                blink = me.FindItem("item_blink");
            }

           // if (abyssalBlade == null)
           // {
           //     abyssalBlade = me.FindItem("item_abyssal_blade");
          //  }

            if (!Game.IsKeyDown(Key.Space) || (Game.IsChatOpen))
            {
                target = null;
                lastStack = 0;
                return;
            }
            if (Utils.SleepCheck("blink"))
            {
                mePosition = me.Position;
            }
            var range = 1000f;
            var mousePosition = Game.MousePosition;
            if (blink != null)
            {
                blinkRange = blink.AbilityData.FirstOrDefault(x => x.Name == "blink_range").GetValue(0);
                range = blinkRange + me.HullRadius + 200;
            }
            var lastTarget = target;
            target = me.ClosestToMouseTarget(range);
            if (!Equals(target, lastTarget))
            {
                lastStack = 0;
            }
            if (target == null || !target.IsAlive || !target.IsVisible
                || target.Distance2D(mousePosition) > target.Distance2D(me) + 1000)
            {
                if (!Utils.SleepCheck("move"))
                {
                    return;
                }
                me.Move(mousePosition);
                Utils.Sleep(100, "move");
                return;
            }
            targetDistance = mePosition.Distance2D(target);
            hullsum = (me.HullRadius + target.HullRadius) * 2;
            turnTime = me.GetTurnTime(target);
            var casting = CastCombo();
            if (casting)
            {
                return;
            }
            //OrbWalk(tick);
        }

        private static void Game_OnWndProc(WndEventArgs args)
        {
            if (args.Msg != (ulong)Utils.WindowsMessages.WM_KEYUP || args.WParam != 'G' || Game.IsChatOpen)
            {
                return;
            }
            
            enableQ = !enableQ;
            enableW = !enableW;



        }
        





    }
}

