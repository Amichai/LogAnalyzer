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

        private bool collision(Dictionary<int, Vector> pos, double threshold) {
            foreach (var id1 in pos.Keys) {
                foreach (var id2 in pos.Keys) {
                    if (id1 == id2) { continue; }
                    var p1 = pos[id1];
                    var p2 = pos[id2];
                    var d = p1.Dist(p2);
                    if (d < threshold) {
                        return true;
                    }
                }
            }
            return false;
        }

        private RegexFilter get(string name) {
            return this.Filters.Where(i => i.Name == name).Single();
        }

        public void custom2() {
            List<double> thresholds = new List<double>() { 14, 18, 22, 26, 30, 40, 50 };
            //List<double> thresholds = new List<double>() { 50 };
            foreach (var collisionThreshold in thresholds) {


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
                int lineIdx = 0;
                foreach (var l in this.Lines) {
                    lineIdx++;
                    if ((bool)swap.Val(l)) {
                        trialStart = timestamp.Val(l) as DateTime?;
                    }

                    var xVal = (double?)x.Val(l);
                    var yVal = (double?)y.Val(l);
                    var IDVal = (double?)ID.Val(l);
                    if (xVal.HasValue && yVal.HasValue && IDVal.HasValue) {
                        int thisID = (int)IDVal.Value;
                        robotPosition[thisID] = new Vector(xVal.Value, yVal.Value);
                        if (this.collision(robotPosition, collisionThreshold)) {
                            collisions++;
                            if (collisionStart == null) {
                                collisionStart = (DateTime?)timestamp.Val(l);
                                Debug.Print(string.Format("Collision start. Line idx: {0} threshold: {1}, time: {2}", lineIdx, collisionThreshold, collisionStart.Value.TimeOfDay));
                            }
                        } else {
                            if (collisionStart.HasValue) {

                                collisionEnd = (DateTime?)timestamp.Val(l);
                                var dur = collisionEnd.Value - collisionStart.Value;
                                Debug.Print(string.Format("Collision end. Line idx: {0} threshold: {1}, time: {2}, duration: {3}", lineIdx, collisionThreshold, collisionEnd.Value.TimeOfDay, dur));
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
                            trial.SetValue("CollisionTime" + collisionThreshold, collisionTime.ToString(), "TimeSpan", "time spent in a collision state");
                            trialStart = null;
                            trialEnd = null;
                            collisions = 0;
                            collisionTime = TimeSpan.FromSeconds(0);
                            collisionStart = null;
                            collisionEnd = null;
                        }
                    }
                }

            }
        }

        public void custom1() {
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
