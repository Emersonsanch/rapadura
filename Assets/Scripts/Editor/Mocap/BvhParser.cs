using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace Rapadura.Editor.Mocap
{
    /// <summary>One joint of a BVH hierarchy. Offsets are in the file's units and right-handed space.</summary>
    public sealed class BvhJoint
    {
        public string Name;
        public int Parent = -1;
        public Vector3 Offset;
        public Vector3? EndSite;
        public readonly List<string> Channels = new List<string>();
        public readonly List<int> Children = new List<int>();
        /// <summary>Column of this joint's first channel inside a frame row.</summary>
        public int ChannelStart;
    }

    /// <summary>A parsed BVH file: hierarchy + raw channel values per frame.</summary>
    public sealed class BvhClip
    {
        public readonly List<BvhJoint> Joints = new List<BvhJoint>();
        public float FrameTime = 1f / 30f;
        public float[][] Frames = Array.Empty<float[]>();

        public int FrameCount => Frames.Length;

        public int IndexOf(string name)
        {
            for (int i = 0; i < Joints.Count; i++)
                if (string.Equals(Joints[i].Name, name, StringComparison.OrdinalIgnoreCase)) return i;
            return -1;
        }

        /// <summary>
        /// Forward kinematics for one frame, already converted to Unity's left-handed space
        /// (X mirrored — see <see cref="MocapMath.RightToLeft(Vector3)"/>). Positions are multiplied by <paramref name="scale"/>.
        /// </summary>
        public void Evaluate(int frame, float scale, Vector3[] worldPos, Quaternion[] worldRot)
        {
            var row = Frames[frame];
            var rhPos = new Vector3[Joints.Count];
            var rhRot = new Quaternion[Joints.Count];
            for (int j = 0; j < Joints.Count; j++)
            {
                var jt = Joints[j];
                MocapMath.ReadChannels(jt.Channels, row, jt.ChannelStart, out var localPos, out var localRot, out bool hasPos);
                Vector3 offset = jt.Offset + (hasPos ? localPos : Vector3.zero);
                if (jt.Parent < 0)
                {
                    rhPos[j] = offset;
                    rhRot[j] = localRot;
                }
                else
                {
                    rhPos[j] = rhPos[jt.Parent] + rhRot[jt.Parent] * offset;
                    rhRot[j] = rhRot[jt.Parent] * localRot;
                }
                worldPos[j] = MocapMath.RightToLeft(rhPos[j]) * scale;
                worldRot[j] = MocapMath.RightToLeft(rhRot[j]);
            }
        }
    }

    /// <summary>Minimal BVH reader (ROOT/JOINT/End Site, any channel order).</summary>
    public static class BvhParser
    {
        public static BvhClip Parse(string text)
        {
            var clip = new BvhClip();
            var tokens = Tokenize(text);
            int p = 0;
            var stack = new Stack<int>();
            bool inEndSite = false;
            int channelCursor = 0;

            string Next() => p < tokens.Count ? tokens[p++] : throw new FormatException("BVH: unexpected end of file");
            float NextFloat() => float.Parse(Next(), NumberStyles.Float, CultureInfo.InvariantCulture);

            while (p < tokens.Count)
            {
                string t = Next();
                string u = t.ToUpperInvariant();
                if (u == "HIERARCHY") continue;
                if (u == "ROOT" || u == "JOINT")
                {
                    var jt = new BvhJoint { Name = Next(), Parent = stack.Count > 0 ? stack.Peek() : -1 };
                    clip.Joints.Add(jt);
                    if (jt.Parent >= 0) clip.Joints[jt.Parent].Children.Add(clip.Joints.Count - 1);
                }
                else if (u == "END")
                {
                    Next(); // "Site"
                    inEndSite = true;
                }
                else if (t == "{")
                {
                    if (inEndSite) stack.Push(-2);
                    else stack.Push(clip.Joints.Count - 1);
                }
                else if (t == "}")
                {
                    int popped = stack.Pop();
                    if (popped == -2) inEndSite = false;
                }
                else if (u == "OFFSET")
                {
                    var v = new Vector3(NextFloat(), NextFloat(), NextFloat());
                    if (inEndSite)
                    {
                        int owner = -1;
                        foreach (int s in stack) { if (s >= 0) { owner = s; break; } }
                        if (owner >= 0) clip.Joints[owner].EndSite = v;
                    }
                    else clip.Joints[clip.Joints.Count - 1].Offset = v;
                }
                else if (u == "CHANNELS")
                {
                    var jt = clip.Joints[clip.Joints.Count - 1];
                    int n = int.Parse(Next(), CultureInfo.InvariantCulture);
                    jt.ChannelStart = channelCursor;
                    for (int i = 0; i < n; i++) jt.Channels.Add(Next());
                    channelCursor += n;
                }
                else if (u == "MOTION")
                {
                    break;
                }
            }

            // MOTION section: "Frames: N" "Frame Time: x"
            int frames = 0;
            while (p < tokens.Count)
            {
                string t = Next();
                if (t.StartsWith("Frames", StringComparison.OrdinalIgnoreCase))
                {
                    string v = t.Contains(":") && !t.EndsWith(":") ? t.Substring(t.IndexOf(':') + 1) : Next();
                    frames = int.Parse(v, CultureInfo.InvariantCulture);
                }
                else if (t.Equals("Frame", StringComparison.OrdinalIgnoreCase))
                {
                    Next(); // "Time:"
                    clip.FrameTime = NextFloat();
                    break;
                }
            }

            var rows = new float[frames][];
            for (int f = 0; f < frames; f++)
            {
                var row = new float[channelCursor];
                for (int c = 0; c < channelCursor; c++) row[c] = NextFloat();
                rows[f] = row;
            }
            clip.Frames = rows;
            return clip;
        }

        private static List<string> Tokenize(string text)
        {
            var list = new List<string>(text.Length / 6);
            int i = 0, n = text.Length;
            while (i < n)
            {
                while (i < n && char.IsWhiteSpace(text[i])) i++;
                int s = i;
                while (i < n && !char.IsWhiteSpace(text[i])) i++;
                if (i > s) list.Add(text.Substring(s, i - s));
            }
            return list;
        }
    }
}
