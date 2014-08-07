using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using LogAnalyzer.Util;
using LogAnalyzer.Filters;
using System.Diagnostics;
using ResultStore;

namespace LogAnalyzer {
    class CustomAnalysis {
        public CustomAnalysis(List<LogLine> lines, List<RegexFilter> filters) {
            this.Lines = lines;
            this.Filters = filters;
        }

        private List<LogLine> Lines;
        private List<RegexFilter> Filters;

        private bool collision(Dictionary<int, Vector> pos, int idToCheck, double threshold) {
            var p1 = pos[idToCheck];
            foreach (var id in pos.Keys) {
                if (id == idToCheck) {
                    continue;
                }

                var d = pos[id].Dist(p1);
                if (d < threshold) {
                    return true;
                }
            }
            return false;
        }

        private RegexFilter get(string name) {
            return this.Filters.Where(i => i.Name == name).Single();
        }

        public void custom2() {
            double collisionThreshold = 14;
            int collisions = 0;

            var swap = this.get("Swap");
            var converged = this.get("Converged");
            var timestamp = this.get("Timestamp");
            TimeSpan collisionTime = TimeSpan.FromSeconds(0);
            var x = this.get("x");
            var y = this.get("y");
            var ID = this.get("ID");
            DateTime? trialStart = null, trialEnd = null;
            DateTime? collisionStart = null, collisionEnd = null;
            Dictionary<int, Vector> robotPosition = new Dictionary<int, Vector>();
            foreach (var l in this.Lines) {
                if ((bool)swap.Val(l)) {
                    trialStart = timestamp.Val(l) as DateTime?;
                }

                var xVal = (double?)x.Val(l);
                var yVal = (double?)y.Val(l);
                var IDVal = (double?)ID.Val(l);
                if (xVal.HasValue && yVal.HasValue && IDVal.HasValue) {
                    int thisID = (int)IDVal.Value;
                    robotPosition[thisID] = new Vector(xVal.Value, yVal.Value);
                    if (this.collision(robotPosition, thisID, collisionThreshold)) {
                        collisions++;
                        if (collisionStart == null) {
                            collisionStart = DateTime.Now;
                        }
                    } else {
                        if (collisionStart.HasValue) {
                            collisionEnd = DateTime.Now;
                            var dur = collisionEnd.Value - collisionStart.Value;
                            collisionTime = collisionTime.Add(dur);
                            collisionStart = null;
                            collisionEnd = null;
                        }
                    }
                }

                if ((bool)converged.Val(l)) {
                    trialEnd = timestamp.Val(l) as DateTime?;
                    if (trialStart != null && trialEnd != null) {
                        Debug.Print(string.Format("Start: {0}, end: {1}", trialStart.Value.TimeOfDay.ToString(), trialEnd.Value.TimeOfDay.ToString()));
                        var ex = Experiment.Get("ValidatorSwap");
                        var trial = ex.Trial(trialStart.Value);
                        var dur = trialEnd - trialStart;
                        trial.SetValue("Collisions" + collisionThreshold, collisions, "int", "Number of robot collisions when the collisionThreshold is taken to be " + collisionThreshold);
                        trial.SetValue("CollisionTime" + collisionThreshold, collisionTime, "TimeSpan", "time spent in a collision state");
                        trialStart = null;
                        trialEnd = null;
                        collisions = 0;
                        collisionTime = TimeSpan.FromSeconds(0);
                    }
                }

            }
        }

        public void custom1() {
            ///Segment the log file into test units
            ///

            var swap = this.get("Swap");
            var converged = this.get("Converged");
            var timestamp = this.get("Timestamp");
            DateTime? start = null, end = null;
            foreach (var l in this.Lines) {
                if ((bool)swap.Val(l)) {
                    start = timestamp.Val(l) as DateTime?;
                }

                if ((bool)converged.Val(l)) {
                    end = timestamp.Val(l) as DateTime?;
                    if (start != null && end != null) {
                        Debug.Print(string.Format("Start: {0}, end: {1}", start.Value.TimeOfDay.ToString(), end.Value.TimeOfDay.ToString()));
                        var ex = Experiment.Get("ValidatorSwap");
                        var trial = ex.Trial(start.Value);
                        var dur = end - start;
                        trial.SetValue("Duration", dur, "TimeSpan", "Duration of the trial");
                        start = null;
                        end = null;
                    }
                }

            }
        }

    }
}
