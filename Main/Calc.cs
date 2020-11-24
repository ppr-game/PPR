using System;
using System.Collections.Generic;
using System.Linq;

using PPR.Main.Levels;

namespace PPR.Main {
    public static class Calc {
        public static float StepsToMilliseconds(float steps) => StepsToMilliseconds(steps, Map.currentLevel.speeds);
        public static float StepsToMilliseconds(float steps, List<LevelSpeed> sortedSpeeds) {
            float useSteps = steps;

            int speedIndex = 0;
            for(int i = 0; i < sortedSpeeds.Count; i++)
                if(sortedSpeeds[i].step <= useSteps) speedIndex = i;
            float time = 0;
            for(int i = 0; i <= speedIndex; i++)
                if(i == speedIndex)
                    time += (useSteps - sortedSpeeds[i].step) *
                            (sortedSpeeds[i].speed == 0 ? 0 : 60000f / Math.Abs(sortedSpeeds[i].speed));
                else
                    time += (sortedSpeeds[i + 1].step - sortedSpeeds[i].step) *
                            (sortedSpeeds[i].speed == 0 ? 0 : 60000f / Math.Abs(sortedSpeeds[i].speed));
            return time;
        }
        
        public static float MillisecondsToSteps(float time) => MillisecondsToSteps(time, Map.currentLevel.speeds);
        public static float MillisecondsToSteps(float time, List<LevelSpeed> sortedSpeeds) {
            float useTime = time;

            float steps = 0;
            for(int i = 0; i < sortedSpeeds.Count; i++) {
                float stepsIncrement = i < sortedSpeeds.Count - 1 ? sortedSpeeds[i + 1].step - sortedSpeeds[i].step :
                    float.PositiveInfinity;
                float newUseTime = useTime - stepsIncrement *
                    (sortedSpeeds[i].speed == 0 ? 0 : 60000f / Math.Abs(sortedSpeeds[i].speed));
                if(newUseTime <= 0f) {
                    steps += sortedSpeeds[i].speed == 0 ? 0 : useTime / (60000f / Math.Abs(sortedSpeeds[i].speed));
                    break;
                }
                steps += stepsIncrement;
                useTime = newUseTime;
            }

            return steps;
        }
        
        public static float StepsToOffset(float steps) => StepsToOffset(steps, Map.currentLevel.speeds);
        public static float StepsToOffset(float steps, List<LevelSpeed> sortedSpeeds) {
            float useSteps = steps;
            float offset = 0;
            
            for(int i = 0; i < sortedSpeeds.Count; i++) {
                float stepsDecrement = i < sortedSpeeds.Count - 1 ? sortedSpeeds[i + 1].step - sortedSpeeds[i].step :
                    float.PositiveInfinity;
                float newUseSteps = useSteps - stepsDecrement;
                if(newUseSteps <= 0f) {
                    offset += useSteps * MathF.Sign(sortedSpeeds[i].speed);
                    break;
                }
                offset += stepsDecrement * MathF.Sign(sortedSpeeds[i].speed);
                useSteps = newUseSteps;
            }

            return offset;
        }
        
        public static float OffsetToSteps(float offset, int directionLayer) => OffsetToSteps(offset, directionLayer, Map.currentLevel.speeds);
        public static float OffsetToSteps(float offset, int directionLayer, List<LevelSpeed> sortedSpeeds) {
            for(int i = sortedSpeeds.Count - 1; i >= 0; i--) {
                float currentOffset = StepsToOffset(sortedSpeeds[i].step);
                if(currentOffset <= offset && StepsToDirectionLayer(sortedSpeeds[i].step) == directionLayer)
                    return sortedSpeeds[i].step + offset - currentOffset;
            }

            return float.NaN;
        }
        
        public static int StepsToDirectionLayer(float steps) => StepsToDirectionLayer(steps, Map.currentLevel.speeds);
        public static int StepsToDirectionLayer(float steps, List<LevelSpeed> sortedSpeeds) {
            int speedIndex = 0;
            for(int i = 0; i < sortedSpeeds.Count; i++)
                if(sortedSpeeds[i].step <= steps) speedIndex = i;
                else break;
            int directionLayer = 0;
            for(int i = 1; i <= speedIndex; i++)
                if(MathF.Sign(sortedSpeeds[i].speed) != MathF.Sign(sortedSpeeds[i - 1].speed)) directionLayer++;
            return directionLayer;
        }
        
        public static int GetBPMAtStep(int step, IEnumerable<LevelSpeed> sortedSpeeds) {
            int bpm = 0;
            foreach(LevelSpeed speed in sortedSpeeds)
                if(speed.step <= step) bpm = speed.speed;
                else break;
            return bpm;
        }
        
        public static IEnumerable<int> GetBPMBetweenSteps(int start, int end, IEnumerable<LevelSpeed> sortedSpeeds) =>
            from speed in sortedSpeeds where speed.step > start && speed.step < end select speed.speed;
        
        public static List<LevelSpeed> GetSpeedsBetweenSteps(int start, int end, List<LevelSpeed> sortedSpeeds) =>
            sortedSpeeds.FindAll(speed => speed.step >= start && speed.step <= end);
        
        public static float GetDifficulty(IEnumerable<LevelObject> objects, List<LevelSpeed> sortedSpeeds,
            int lengthMins) => GetDifficulty(ObjectsToLightObjects(objects).ToList(), sortedSpeeds, lengthMins);
        public static float GetDifficulty(IReadOnlyCollection<LightLevelObject> lightObjects, List<LevelSpeed> sortedSpeeds,
            int lengthMins) {
            if (lightObjects.Count == 0 || sortedSpeeds.Count == 0) return 0f;

            List<LightLevelObject> sortedObjects = new List<LightLevelObject>(lightObjects);
            sortedObjects.Sort((obj1, obj2) => obj1.step.CompareTo(obj2.step));
            for(int i = 1; i < sortedObjects.Count; i++)
                if(sortedObjects[i].character == LevelHoldNote.DisplayChar)
                    sortedObjects.RemoveAt(i - 1);
            sortedObjects = sortedObjects.FindAll(obj => obj.character != LevelHoldNote.DisplayChar);

            List<float> diffFactors = new List<float>();
            
            List<float> speeds = new List<float>();
            
            List<LightLevelObject>[] objects = {
                new List<LightLevelObject>(),
                new List<LightLevelObject>()
            };
            foreach(LightLevelObject obj in sortedObjects)
                objects[GetXPosForCharacter(obj.character) / 40].Add(obj);
            foreach(List<LightLevelObject> objs in objects)
                for(int i = 1; i < objs.Count; i++) {
                    LightLevelObject prevObj = objs[i - 1];
                    LightLevelObject currObj = objs[i];
                    int startBPM = Math.Abs(GetBPMAtStep(prevObj.step, sortedSpeeds));
                    int endBPM = Math.Abs(GetBPMAtStep(currObj.step, sortedSpeeds));
                    int currStep = prevObj.step - startBPM / 600;
                    int endStep = currObj.step + endBPM / 600;
                    float time = 0;
                    int currBPM = startBPM;
                    foreach(LevelSpeed speed in GetSpeedsBetweenSteps(prevObj.step, currObj.step,
                        sortedSpeeds)) {
                        time += 60f / currBPM * (speed.step - currStep);
                        currStep = speed.step;
                        currBPM = Math.Abs(speed.speed);
                    }
                    time += 60f / endBPM * (endStep - currStep);
                    float distance = GetPhysicalKeyDistance(currObj.character, prevObj.character);
                    float spd = distance / time;
                    if(spd > 0f && float.IsFinite(spd)) speeds.Add(spd);
                }

            float averageBPM = GetAverageBPM(sortedSpeeds, GetLastObject(lightObjects.ToList()).step);
            
            diffFactors.Add(speeds.Count > 0 ? speeds.Average() : 0f);
            diffFactors.Add(MathF.Max((66f - 360f / (averageBPM / 6f)) / 30f, 0f));
            diffFactors.Add(lengthMins);

            return diffFactors.Count > 0 ? diffFactors.Average() : 0f;
        }

        public static float GetAverageBPM(IReadOnlyList<LevelSpeed> speeds, int endStep) {
            float totalBPM = 0f;
            int bpmCount = 0;
            for(int i = 0; i < speeds.Count; i++) {
                int secondStep = i + 1 >= speeds.Count || speeds[i + 1].step > endStep ? endStep : speeds[i + 1].step;
                int length = secondStep - speeds[i].step;
                totalBPM += MathF.Abs(speeds[i].speed) * length;
                bpmCount += length;
                if(secondStep >= endStep) break;
            }
            return totalBPM / bpmCount;
        }

        public static string TimeSpanToLength(TimeSpan span) =>
            $"{(span < TimeSpan.Zero ? "-" : "")}{span.ToString($"{(span.Hours != 0 ? "h':'mm" : "m")}':'ss")}";

        public static LevelNote GetFirstObject(IEnumerable<LevelNote> notes) =>
            notes.OrderBy(note => note.step).FirstOrDefault();
        public static LightLevelObject GetFirstObject(List<LightLevelObject> objects) {
            List<LightLevelObject> sortedObjects = objects.FindAll(obj => obj.character != LevelSpeedObject.DisplayChar);
            sortedObjects.Sort((obj1, obj2) => obj1.step.CompareTo(obj2.step));
            return sortedObjects.Count > 0 ? sortedObjects[0] : new LightLevelObject('\n', -1);
        }
        public static LevelNote GetLastObject(IEnumerable<LevelNote> notes) =>
            notes.OrderBy(note => note.step).LastOrDefault();
        public static LightLevelObject GetLastObject(List<LightLevelObject> objects) {
            List<LightLevelObject> sortedObjects = objects.FindAll(obj => obj.character != LevelSpeedObject.DisplayChar);
            sortedObjects.Sort((obj1, obj2) => obj2.step.CompareTo(obj1.step));
            return sortedObjects.Count > 0 ? sortedObjects[0] : new LightLevelObject('\n', -1);
        }

        public static TimeSpan GetTotalLevelLength(List<LightLevelObject> objects, List<LevelSpeed> sortedSpeeds,
            int musicOffset) {
            if(objects.Count <= 0 || sortedSpeeds.Count <= 0) return TimeSpan.Zero;
            float ms = StepsToMilliseconds(GetLastObject(objects).step, sortedSpeeds) - musicOffset;
            return TimeSpan.FromMilliseconds(float.IsNaN(ms) ? 0d : ms);
        }
        public static TimeSpan GetLevelLength(List<LightLevelObject> objects, List<LevelSpeed> sortedSpeeds,
            int musicOffset) {
            if(objects.Count <= 0 || sortedSpeeds.Count <= 0) return TimeSpan.Zero;
            int firstStep = GetFirstObject(objects).step;
            int lastStep = GetLastObject(objects).step;
            if(firstStep < 0 || lastStep < 0) return TimeSpan.Zero;
            float ms = StepsToMilliseconds(lastStep, sortedSpeeds) - musicOffset -
                       StepsToMilliseconds(firstStep, sortedSpeeds);
            return TimeSpan.FromMilliseconds(float.IsNaN(ms) ? 0d : ms);
        }

        public static List<LightLevelObject> ObjectsToLightObjects(List<LevelObject> objects) {
            List<LightLevelObject> objs = new List<LightLevelObject>(objects.Count);
            // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
            foreach(LevelObject obj in objects) objs.Add(new LightLevelObject(obj.character, obj.step));
            return objs;
        }
        public static IEnumerable<LightLevelObject> ObjectsToLightObjects(IEnumerable<LevelObject> objects) =>
            objects.Select(obj => new LightLevelObject(obj.character, obj.step));

        public static int GetXPosForCharacter(char character) {
            character = char.ToLower(character);
            int x = 0;
            int xLineOffset = 0;
            int mul = 90 / LevelObject.lines.Select(line => line.Length).Max();
            foreach(string line in LevelObject.lines) {
                if(line.Contains(character)) {
                    x = (line.IndexOf(character) + 1) * (mul - 1) + xLineOffset * mul / 3;
                    break;
                }
                xLineOffset++;
            }
            return x;
        }
        public static float GetPhysicalKeyDistance(char leftChar, char rightChar) {
            int leftX = GetXPosForCharacter(leftChar);
            int rightX = GetXPosForCharacter(rightChar);
            int leftY = 0;
            int rightY = 0;
            int lineOffset = 0;
            foreach(string line in LevelObject.lines) {
                if(line.Contains(leftChar)) leftY = lineOffset;
                if(line.Contains(rightChar)) rightY = lineOffset;
                lineOffset++;
            }
            return MathF.Sqrt((leftX - rightX) / 6f * ((leftX - rightX) / 6f) + (leftY - rightY) * (leftY - rightY));
        }
    }
}
